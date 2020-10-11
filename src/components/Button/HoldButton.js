import React, { useRef } from 'react';
import cl from "classnames";

import style from "./Button.module.scss";

export const HoldButton = ({ value, onTick, mutator, onChange, children, type }) => {
    const startTickRef = useRef(null);
    const tickRef = useRef(null);
    const newValue = useRef(null);

    const onKeyDown = (event) => {
        if (event.key === 'Enter') {
            onMouseDown(event);
        }
    };
    const onMouseDown = (event) => {
        event.preventDefault();
        if (tickRef.current) {
            return;
        }

        newValue.current = mutator(value);
        onTick(newValue.current);

        startTickRef.current = setTimeout(() => {
            tickRef.current = setInterval(() => {
                newValue.current = mutator(newValue.current);
                onTick(newValue.current);
            }, 50);
        }, 300);
    };

    const onKeyUp = (event) => {
        if (event.key === 'Enter') {
            onMouseUp(event);
        }
    };
    const onMouseUp = (event) => {
        event.preventDefault();

        clearTimeout(startTickRef.current);
        clearInterval(tickRef.current);
        startTickRef.current = null;
        tickRef.current = null;
        newValue.current = null;

        onChange(value);
    };

    return (
        <button
            onKeyDown={ onKeyDown }
            onKeyUp={ onKeyUp }
            onMouseDown={ onMouseDown }
            onMouseUp={ onMouseUp }
            onTouchStart={ onMouseDown }
            onTouchEnd={ onMouseUp }
            className={cl(style.button, style[type])}
        >
            <span className={style.buttonContent}>
                { children }
            </span>
        </button>
    );
};
