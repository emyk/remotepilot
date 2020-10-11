using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace fsim
{
    public class Structs
    {
        public enum DEFINITIONS
        {
            AircraftStatus
        }

        public enum DATA_REQUEST
        {
            AircraftStatus
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct AircraftStatusStruct
        {
            public double Latitude;
            public double Longitude;
            public double Altitude;
            public double CurrentFuel;
            public double TotalFuel;
            public double TrueHeading;
            public double AirspeedIndicated;
            public double AirspeedTrue;

            public bool NavHasSignal;
            public bool NavHasDME;
            public double DMEDistance;

            public bool AutopilotAvailable;
            public bool AutopilotMaster;
            public bool AutopilotWingLevel;
            public bool AutopilotAltitudeHold;
            public int AutopilotAltitude;
            public bool AutopilotApproach;
            public bool AutopilotBackcourse;
            public bool AutopilotFlightDirector;
            public bool AutopilotAirspeedHold;
            public int AutopilotAirspeed;
            public bool AutopilotMach;
            public bool AutopilotYawDamper;
            public bool AutopilotAutothrottle;
            public bool AutopilotVerticalSpeedHold;
            public int AutopilotVerticalSpeed;
            public bool AutopilotHeadingHold;
            public int AutopilotHeadingDir;
            public bool AutopilotNav1Hold;

            public int GearHandlePosition;
            public int GearCenterPosition;
            public int GearLeftPosition;
            public int GearRightPosition;
            public bool GearIsRetractable;
            public double GearTotalPctExtended;
            public int ParkingBrakePosition;
        }
    }
}
