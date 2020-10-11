const { createProxyMiddleware } = require('http-proxy-middleware');

const nodeJsPort = process.env.NODEJS_PORT || 8090;

module.exports = function (app) {
    app
        .use('/api', createProxyMiddleware({
            pathRewrite: {
                '^/api': '/'
            },
            target: `http://localhost:${ nodeJsPort }`,
            logLevel: 'debug'
        }));
};
