using System;
using ElectronCgi.DotNet;
using Microsoft.FlightSimulator.SimConnect;
using static fsim.Structs;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace fsim
{

    public enum hSimConnect : int
    {
        group1
    }

    enum Events
    {
        GEAR_UP, 
        GEAR_DOWN,
        PARKING_BRAKES_TOGGLE,

        AUTOPILOT_MASTER_TOGGLE,
        AUTOPILOT_HEADING_INC, 
        AUTOPILOT_HEADING_DEC,
        AUTOPILOT_HEADING_SET,
        AUTOPILOT_HEADING_HOLD_TOGGLE,
        AUTOPILOT_ALTITUDE_HOLD_TOGGLE, 
        AUTOPILOT_SET_ALT_REF,
        AUTOPILOT_LOC_HOLD_TOGGLE,
        AUTOPILOT_APPR_HOLD_TOGGLE,
        AUTOPILOT_NAV_HOLD_TOGGLE,
        AUTOPILOT_FD_TOGGLE,
        AUTOPILOT_AUTO_THROTTLE_TOGGLE,
        AUTOPILOT_PANEL_SPEED_HOLD,
        AUTOPILOT_PANEL_SPEED_SET,
        AUTOPILOT_VERTICAL_SPEED_TOGGLE,
        AUTOPILOT_VERTICAL_SPEED_SET,

        CAMERA_TOGGLE_CHASE_VIEW,
        CAMERA_VIEW_MODE
    }

    class SimConnector
    {
        private SimConnect simconnect = null;

        const uint WM_USER_SIMCONNECT = 0x0402;

        private readonly IntPtr ptr = new IntPtr(0);
        private Connection connection = null;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        public SimConnector(Connection connection)
        {
            this.connection = connection;
        }

        private void LogToConnection(String log)
        {
            Debug.WriteLine(log);
            connection.Send<string>("log", "{log: " + log + "}");
        }

        public void Connect()
        {
            LogToConnection("Connecting to simconnect?");
            if (simconnect != null)
                return;


            try
            {
                IntPtr consolePtr = GetConsoleWindow();

                simconnect = new SimConnect("remotepilot", consolePtr, WM_USER_SIMCONNECT, null, 0);

                simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(OnRecvOpen);
                simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(OnRecvQuit);
                simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(RecvExceptionHandler);
                simconnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(RecvSimobjectDataBytype);
                simconnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(OnRecvSimobjectData);

                connection.On("startPoll", () =>
                {
                    ReceiveSimConnectMessage();
                });

                AddSetVarListener();
            
                LogToConnection("Connected to simconnect, maybe");
            }
            catch (COMException ex)
            {
                LogToConnection("Unable to create new SimConnect instance: {0}" + ex.Message);
                simconnect = null;
            }
        }

        private void AddSetVarListener()
        {
            connection.On<string>("doEvent", (stringEv) =>
            {
                Events? ev = GetEvent(stringEv);
                uint value = GetValue(stringEv);
                
                if (ev != null)
                {
                    LogToConnection("New event " + ev.ToString() + " value: " + value);
                    simconnect.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, ev, value, hSimConnect.group1, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                } else
                {
                    LogToConnection("Invalid event " + stringEv);
                }
            });
        }

        private uint GetValue(String var)
        {
            string[] vs = var.Split(":");
            string value = vs.Length == 3 ? vs[2] : "0";

            return UInt32.Parse(value, NumberStyles.Integer);
        }

        private Events? GetEvent(string var)
        {
            string[] vs = var.Split(":");
            string type = vs[0];
            string action = vs[1];

            switch (type)
            {
                case "gear":
                    {
                        switch (action)
                        {
                            case "up": return Events.GEAR_UP;
                            case "down": return Events.GEAR_DOWN;
                            case "parkingBrakeToggle": return Events.PARKING_BRAKES_TOGGLE;
                            default: return null;
                        }
                    }
                case "autopilot":
                    {
                        switch (action)
                        {
                            case "masterToggle": return Events.AUTOPILOT_MASTER_TOGGLE;

                            case "headingInc": return Events.AUTOPILOT_HEADING_INC;
                            case "headingDec": return Events.AUTOPILOT_HEADING_DEC;
                            case "headingSet": return Events.AUTOPILOT_HEADING_SET;
                            case "headingHoldToggle": return Events.AUTOPILOT_HEADING_HOLD_TOGGLE;

                            case "altSet": return Events.AUTOPILOT_SET_ALT_REF;
                            case "altHoldToggle": return Events.AUTOPILOT_ALTITUDE_HOLD_TOGGLE;

                            case "locHoldToggle": return Events.AUTOPILOT_LOC_HOLD_TOGGLE;
                            case "apprHoldToggle": return Events.AUTOPILOT_APPR_HOLD_TOGGLE;
                            case "navHoldToggle": return Events.AUTOPILOT_NAV_HOLD_TOGGLE;
                            case "fdToggle": return Events.AUTOPILOT_FD_TOGGLE;
                            case "autoThrottleToggle": return Events.AUTOPILOT_AUTO_THROTTLE_TOGGLE;

                            case "airspeedHoldToggle": return Events.AUTOPILOT_PANEL_SPEED_HOLD;
                            case "airspeedSet": return Events.AUTOPILOT_PANEL_SPEED_SET;

                            case "verticalSpeedToggle": return Events.AUTOPILOT_VERTICAL_SPEED_TOGGLE;
                            case "verticalSpeedSet": return Events.AUTOPILOT_VERTICAL_SPEED_SET;

                            default: return null;
                        }
                    }
                case "camera":
                    {
                        switch (action)
                        {
                            case "chaseView": return Events.CAMERA_TOGGLE_CHASE_VIEW;
                            case "viewMode": return Events.CAMERA_VIEW_MODE;
                            default: return null;
                        }
                    }

                default: return null;
            }
        }
        private void ReceiveSimConnectMessage()
        {
            simconnect?.ReceiveMessage();
        }

        private void OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            LogToConnection("Simconnect started receiving data from sim.");
            SetFlightDataDefinitions();
            simconnect.RequestDataOnSimObject(DATA_REQUEST.AircraftStatus, DEFINITIONS.AircraftStatus, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SECOND, 0, 0, 0, 0);
            
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    ReceiveSimConnectMessage();
                }
            });
        }

        private void OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            LogToConnection("SimConnect quit");
            Disconnect();
        }

        private void RecvExceptionHandler(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            LogToConnection("SimConnect exception: " + eException.ToString());
            Disconnect();
        }

        private void OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            AircraftStatusModel aircraftStatusModel = new AircraftStatusModel((AircraftStatusStruct)data.dwData[0]);

            var json = JsonConvert.SerializeObject(aircraftStatusModel);
            connection.Send<string>("flightdata", json);
        }

        private void RecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            LogToConnection("SimConnect RecvSimobjectDataBytype.");
        }

        private void Disconnect()
        {
            if (simconnect == null)
                return;

            simconnect.Dispose();
            simconnect = null;

            LogToConnection("SimConnect was disconnected from the flight sim.");
            connection.Send<string>("status", "disconnected");
        }

        private void SetFlightDataDefinitions()
        {
            #region Aircraft Properties
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "PLANE LATITUDE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "PLANE LONGITUDE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "FUEL TOTAL QUANTITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "FUEL TOTAL CAPACITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AIRSPEED INDICATED", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AIRSPEED TRUE", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            #endregion

            #region Nav Properties
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "NAV HAS NAV", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "NAV HAS DME", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "NAV DME", "nautical miles", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            #endregion

            #region Autopilot Properties
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT AVAILABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT MASTER", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT WING LEVELER", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT ALTITUDE LOCK", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT ALTITUDE LOCK VAR", "feet", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT APPROACH HOLD", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT BACKCOURSE HOLD", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT FLIGHT DIRECTOR ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT AIRSPEED HOLD", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT AIRSPEED HOLD VAR", "knots", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT MACH HOLD", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT YAW DAMPER", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOTHROTTLE ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT VERTICAL HOLD", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT VERTICAL HOLD VAR", "Feet/minute", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT HEADING LOCK", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "AUTOPILOT NAV1 LOCK", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "GEAR HANDLE POSITION", "position", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "GEAR CENTER POSITION", "position", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "GEAR LEFT POSITION", "position", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "GEAR RIGHT POSITION", "position", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "IS GEAR RETRACTABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "GEAR TOTAL PCT EXTENDED", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.AircraftStatus, "BRAKE PARKING POSITION", "position", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            #endregion

            simconnect.RegisterDataDefineStruct<AircraftStatusStruct>(DEFINITIONS.AircraftStatus);

            simconnect.MapClientEventToSimEvent(Events.GEAR_UP, "GEAR_UP");
            simconnect.MapClientEventToSimEvent(Events.GEAR_DOWN, "GEAR_DOWN");
            simconnect.MapClientEventToSimEvent(Events.PARKING_BRAKES_TOGGLE, "PARKING_BRAKES");

            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_MASTER_TOGGLE, "AP_MASTER");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_HEADING_INC, "HEADING_BUG_INC");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_HEADING_DEC, "HEADING_BUG_DEC");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_HEADING_SET, "HEADING_BUG_SET");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_HEADING_HOLD_TOGGLE, "AP_HDG_HOLD");

            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_SET_ALT_REF, "AP_ALT_VAR_SET_ENGLISH");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_ALTITUDE_HOLD_TOGGLE, "AP_ALT_HOLD");

            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_LOC_HOLD_TOGGLE, "AP_LOC_HOLD");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_APPR_HOLD_TOGGLE, "AP_APR_HOLD");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_NAV_HOLD_TOGGLE, "AP_NAV1_HOLD");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_FD_TOGGLE, "TOGGLE_FLIGHT_DIRECTOR");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_AUTO_THROTTLE_TOGGLE, "AUTO_THROTTLE_ARM");

            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_PANEL_SPEED_HOLD, "AP_PANEL_SPEED_HOLD");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_PANEL_SPEED_SET, "AP_SPD_VAR_SET");

            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_VERTICAL_SPEED_SET, "AP_VS_VAR_SET_ENGLISH");
            simconnect.MapClientEventToSimEvent(Events.AUTOPILOT_VERTICAL_SPEED_TOGGLE, "AP_VS_HOLD");

            simconnect.MapClientEventToSimEvent(Events.CAMERA_TOGGLE_CHASE_VIEW, "CHASE_VIEW_TOGGLE");
            simconnect.MapClientEventToSimEvent(Events.CAMERA_VIEW_MODE, "VIEW_MODE");
        }
    }
}
