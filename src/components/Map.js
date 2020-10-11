import React, { useEffect, useLayoutEffect, useRef } from 'react';

const { OpenLayers } = window;

export const Map = ({ flightData }) => {
    const {
        Latitude,
        Longitude
    } = flightData;

    const mapRef = useRef(null);
    const markerRef = useRef(null);
    const markersRef = useRef(null);

    useLayoutEffect(() => {
        mapRef.current = new OpenLayers.Map({
            div: "map-container",
            projection: "EPSG:3857",
            layers: [new OpenLayers.Layer.OSM()]
        });

        const lonLat = new OpenLayers.LonLat( Longitude, Latitude )
            .transform(
                new OpenLayers.Projection("EPSG:4326"),
                mapRef.current.getProjectionObject()
            );

        markersRef.current = new OpenLayers.Layer.Markers( "Markers" );
        mapRef.current.addLayer(markersRef.current);
        markerRef.current = new OpenLayers.Marker(lonLat);
        markersRef.current.addMarker(markerRef.current);
        mapRef.current.setCenter(lonLat, 10);
    }, []); // eslint-disable-line react-hooks/exhaustive-deps

    useEffect(() => {
        if (!mapRef.current || !markerRef.current || !markersRef.current) {
            return;
        }

        const lonLat = new OpenLayers.LonLat( Longitude, Latitude )
            .transform(
                new OpenLayers.Projection("EPSG:4326"),
                mapRef.current.getProjectionObject()
            );

        markersRef.current.removeMarker(markerRef.current);
        markerRef.current = new OpenLayers.Marker(lonLat);
        markersRef.current.addMarker(markerRef.current);
        mapRef.current.setCenter(lonLat);
    },[Latitude, Longitude]);

    return (
        <div>
            <div id="map-container" style={ { height: '400px', width: '100%', padding: '8px' } }/>
        </div>
    )
};
