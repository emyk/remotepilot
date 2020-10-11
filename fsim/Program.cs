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

namespace fsim
{
    class Program
    {
        static Connection connection = null;

        public Program()
        {
        }

        static void Main(string[] args)
        {
            connection = new ConnectionBuilder()
                .WithLogging()
                .Build();

            SimConnector simConnector = new SimConnector(connection);

            connection.On<string, string>("greeting", name =>
            {
                return $"Hello {name}!";
            });

            connection.On("connect", () =>
            {
                simConnector.Connect();
                connection.Send<string>("status", "connected");
            });

            Thread listenThread = new Thread(new ThreadStart(StartListener));
            listenThread.Start();
        }

        static void StartListener()
        {
            connection.Listen();
        }
    }
}
