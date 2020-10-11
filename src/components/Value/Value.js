import React from 'react';

import style from './Value.module.scss';

export const Value = ({ children }) => (
    <div className={style.value}>
        { children }
    </div>
);

export const Content = ({ children }) => (
    <div className={style.content}>
        { children }
    </div>
);

export const Toggle = ({ children }) => (
    <div className={style.toggle}>
        { children }
    </div>
);

export const Actions = ({ children }) => (
    <div className={style.actions}>
        { children }
    </div>
);

