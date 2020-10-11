require('dotenv').config();
const express = require('express');
const http = require('http');

const simconnectSync = require('./simconnectSync');

const app = express();
app.disable('x-powered-by');
app.use(express.json({
    strict: true
}));

const server = http.createServer(app);

simconnectSync(server);

app.use('/', (req, res) => res.sendStatus(404));

app.use((err, req, res, next) => { // next må være med for at handleren skal funke
    console.error(err.stack);
    res.status(500).send({ message: err.message });
});

const nodeJsPort = process.env.NODEJS_PORT || 8090;

server.listen(nodeJsPort, () => {
    console.log('Listening on port', nodeJsPort);
});
