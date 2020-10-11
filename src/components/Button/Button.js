import React from 'react';
import cl from 'classnames';

import style from './Button.module.scss';

export const ButtonGroup = ({ children }) => (
    <div className={style.buttonGroup}>
        { children }
    </div>
);

export const Button = ({ children, type, pressed, disabled, onClick }) => {
    const classNames = cl({
        [style.button]: true,
        [style[type]]: type,
        [style.pressed]: pressed
    });

    return (
        <button
            className={ classNames }
            disabled={ disabled }
            onClick={ onClick }
        >
            <span className={ style.buttonContent }>
                { children }
            </span>
        </button>
    );
};
