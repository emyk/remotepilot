import React from 'react';

import style from './Section.module.scss';

export const SectionHeader = ({ children }) => (
    <div className={style.header}>
        { children }
    </div>
);

export const Section = ({ children }) => (
    <div className={style.section }>
        { children }
    </div>
);

export const Block = ({ children }) => (
    <div className={ style.block }>
        { children }
    </div>
);
