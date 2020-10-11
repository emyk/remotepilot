using System;
using System.Collections.Generic;
using System.Text;
using static fsim.Structs;

namespace fsim
{
    class AircraftStatusModel
    {
        public class AutoPilot
        {
            public bool Available { get; set; }
            public bool Master { get; set; }
            public bool Level { get; set; }
            public bool AltitudeHold { get; set; }
            public int Altitude { get; set; }
            public bool Approach { get; set; }
            public bool Backcourse { get; set; }
            public bool FlightDirector { get; set; }
            public bool AirspeedHold { get; set; }
            public int Airspeed { get; set; }
            public bool Mach { get; set; }
            public bool YawDamper { get; set; }
            public bool Autothrottle { get; set; }
            public bool VerticalSpeedHold { get; set; }
            public int VerticalSpeed { get; set; }
            public bool HeadingHold { get; set; }
            public int HeadingDir { get; set; }
            public bool Nav1 { get; set; }
        }

        public class Gears
        {
            public int HandlePosition { get; set; }
            public int CenterPosition { get; set; }
            public int LeftPosition { get; set; }
            public int RightPosition { get; set; }
            public bool IsRetractable { get; set; }
            public double TotalPctExtended { get; set; }
            public int ParkingBrakePosition { get; set; }
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double TotalFuel { get; set; }
        public double CurrentFuel { get; set; }
        public double TrueHeading { get; set; }
        public double AirspeedIndicated { get; set; }
        public double AirspeedTrue { get; set; }
        public bool NavHasSignal { get; set; }
        public bool NavHasDME { get; set; }
        public double DMEDistance { get; set; }

        public AutoPilot Autopilot { get; set; }

        public Gears Gear { get; set; }

        public AircraftStatusModel(AircraftStatusStruct status)
        {
            Latitude = status.Latitude;
            Longitude = status.Longitude;
            Altitude = status.Altitude;
            TotalFuel = status.TotalFuel;
            CurrentFuel = status.CurrentFuel;
            TrueHeading = status.TrueHeading;
            AirspeedIndicated = status.AirspeedIndicated;
            AirspeedTrue = status.AirspeedTrue;

            NavHasSignal = status.NavHasSignal;
            NavHasDME = status.NavHasDME;
            DMEDistance = status.DMEDistance;

            Autopilot = new AutoPilot()
            {
                Available = status.AutopilotAvailable,
                Master = status.AutopilotMaster,
                FlightDirector = status.AutopilotFlightDirector,
                AirspeedHold = status.AutopilotAirspeedHold,
                Airspeed = status.AutopilotAirspeed,
                AltitudeHold = status.AutopilotAltitudeHold,
                Altitude = status.AutopilotAltitude,
                Approach = status.AutopilotApproach,
                Autothrottle = status.AutopilotAutothrottle,
                Backcourse = status.AutopilotBackcourse,
                HeadingHold = status.AutopilotHeadingHold,
                HeadingDir = status.AutopilotHeadingDir,
                Level = status.AutopilotWingLevel,
                Mach = status.AutopilotMach,
                Nav1 = status.AutopilotNav1Hold,
                VerticalSpeedHold = status.AutopilotVerticalSpeedHold,
                VerticalSpeed = status.AutopilotVerticalSpeed,
                YawDamper = status.AutopilotYawDamper
            };

            Gear = new Gears()
            {
                HandlePosition = status.GearHandlePosition,
                CenterPosition = status.GearCenterPosition,
                LeftPosition = status.GearLeftPosition,
                RightPosition = status.GearRightPosition,
                IsRetractable = status.GearIsRetractable,
                TotalPctExtended = status.GearTotalPctExtended,
                ParkingBrakePosition = status.ParkingBrakePosition
            };
        }

        // For testing
        public static AircraftStatusModel GetDummyData()
        {
            Random rnd = new Random();
            var dummyData = new AircraftStatusStruct
            {
                Latitude = 47.463631,
                Longitude = -122.307794,
                Altitude = rnd.Next(0, 30000),
                TotalFuel = 300,
                CurrentFuel = rnd.Next(0, 300),
                TrueHeading = 180,
                AirspeedIndicated = rnd.Next(0, 300),
                AirspeedTrue = 0,
                NavHasSignal = false,
                NavHasDME = false,
                DMEDistance = 0,
                AutopilotAvailable = false,
                AutopilotMaster = true,
                AutopilotFlightDirector = true,
                AutopilotAirspeedHold = true,
                AutopilotAltitudeHold = true,
                AutopilotApproach = false,
                AutopilotAutothrottle = false,
                AutopilotBackcourse = false,
                AutopilotHeadingHold = true,
                AutopilotWingLevel = false,
                AutopilotMach = false,
                AutopilotNav1Hold = false,
                AutopilotVerticalSpeedHold = false,
                AutopilotYawDamper = false
            };

            return new AircraftStatusModel(dummyData);
        }
    }
}
