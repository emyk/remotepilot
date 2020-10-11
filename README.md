# Setup

- Install node (https://nodejs.org/en/, preferably LTS)
- Install the dotnet core sdk (https://dotnet.microsoft.com/download)
- Install the msfs sdk (enable dev mode in sim, then in the top menu, click `Help` - `Sdk Installer`)

- after checkout, copy 
    - `C:\MSFS SDK\SimConnect SDK\lib\managed\Microsoft.FlightSimulator.SimConnect.dll` to `<project-dir>\fsim`
    - `C:\MSFS SDK\SimConnect SDK\lib\SimConnect.cll to <project-dir>\fsim`

# Running
- run `npm install` and `npm run start`

# Caveats

This app changes the raw variables in the sim. Not all airplanes use these directly 
(e.g. app can't toggle between managed and selected hdg/alt/speed in the A320).

No capability check is done if the airplane has autopilot.
