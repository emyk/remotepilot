const WebSocket = require('ws');
const { ConnectionBuilder } = require('electron-cgi');

let simconnectConnection = null;

const getConnection = () => {
    if (simconnectConnection) {
        return simconnectConnection;
    }

    console.log('Creating new connection');

    simconnectConnection = new ConnectionBuilder()
        .connectTo('dotnet', 'run', '--project', 'fsim')
        .onExit((code) => {
            console.log('onExit', code.toString());
            simconnectConnection = null;
        })
        .onStderr((err) => {
            console.log('onStdErr', err.toString());
            simconnectConnection = null;
        })
        .build();

    simconnectConnection.send('connect', (err) => {
        if (err) {
            console.log('ERR', err);
            simconnectConnection = null;
        }

        setTimeout(() => {
            simconnectConnection
                .send('startPoll', (err) => {
                    if (err) {
                        console.log('ERR', err);
                    }
                });
        }, 1000);

    });

    simconnectConnection.on('status', function (status) {
        console.log('SimConnect status:', status);
        if (status === 'disconnected') {
            simconnectConnection = null;
        }
    });

    simconnectConnection.on('log', function (data) {
        console.log(data);
    });

    simconnectConnection.onDisconnect = () => {
        console.log('Lost connection to the .Net process');
        simconnectConnection = null;
    };

    return simconnectConnection;
};

const startSync = (server) => {
    const wss = new WebSocket.Server({ noServer: true, path: '/websocket' });

    wss.on('connection', function connection(ws) {
        console.log('New connection');
        getConnection();

        ws.on('message', function incoming(message) {
            const m = JSON.parse(message);

            if (m.event) {
                getConnection().send('doEvent', m.event,(err) => {
                    if (err) {
                        console.log('ERR', err);
                    }
                });
            }
        });
    });

    getConnection()
        .on('flightdata', function (data) {
            wss.clients.forEach(client => {
                client.send(`{ "flightData": ${data} }`);
            });
        });

    server.on('upgrade', function upgrade(request, socket, head) {
        wss.handleUpgrade(request, socket, head, function done(ws) {
            wss.emit('connection', ws, request);
        });
    });
};

module.exports = startSync;
