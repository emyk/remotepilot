import React, { useCallback, useEffect, useState } from 'react';

import style from './Container.module.scss';
import { Autopilot } from 'components/Autopilot';
import { Gear } from 'components/Gear';
import { Map } from 'components/Map';

const newWebSocket = () => {
    const port = process.env.NODE_ENV === 'development' ? 8090 : '';
    const url = `${window.location.hostname}:${port}`;

    return new WebSocket(`ws://${url}/websocket`)
};

const useConnection = () => {
    const [ws, setWs] = useState(newWebSocket);
    const [flightData, setFlightData] = useState(null);

    useEffect(() => {
        ws.addEventListener('open', () => console.log('Connected'));

        ws.addEventListener('message', message => {
            const parsed = JSON.parse(message.data);

            if (parsed.flightData) {
                setFlightData(parsed.flightData);
            }
        });

        ws.addEventListener('close', () => {
            console.log('The connection has been closed, reconnecting.');

            setWs(newWebSocket);
        });
    }, [ws]);

    const dispatchEvent = useCallback((event) => {
        ws.send(JSON.stringify({ event }));
    }, [ws]);

    return {
        flightData,
        dispatchEvent,
        isConnected: ws.readyState === WebSocket.OPEN,
        isConnecting: ws.readyState === WebSocket.CONNECTING
    };
};

export const Container = () => {
    const {
        flightData,
        dispatchEvent,
        isConnected,
        isConnecting
    } = useConnection();

    const [showRawData, setShowRawData] = useState(localStorage.getItem('rawdata'));

    return (
        <div className={style.wrapper}>
            <h1>Remotepilot { !isConnected && <span style={{color: '#f00'}}>not connected{ isConnecting && ', connecting' }</span> }</h1>
            { flightData && (
               <>
                   <div className={ style.container }>
                       <Autopilot
                           flightData={ flightData }
                           dispatchEvent={ dispatchEvent }
                       />
                       <Gear
                           flightData={ flightData }
                           dispatchEvent={ dispatchEvent }
                       />
                   </div>
                   <div>
                       <Map flightData={ flightData }/>
                   </div>
                   {
                       showRawData ?
                           (
                             <>
                                 <button onClick={() => setShowRawData(false)}>hide</button>
                                 <pre>{ JSON.stringify(flightData, null, 2) }</pre>
                             </>
                           ) :
                           <button onClick={ () => setShowRawData(true) }>show raw data</button>
                   }
               </>
            ) }
        </div>
    );
};
