import React from 'react';

import { useDebouncedValue } from 'components/useDebouncedValue';
import { Button } from './Button';

export const ToggleButton = ({ children, onClick, pressed }) => {
    const [debouncedPressed, setDebouncedPressed] = useDebouncedValue(!!pressed);

    const _onClick = () => {
        onClick();
        setDebouncedPressed(!debouncedPressed);
    };

    return (
        <Button onClick={ _onClick } pressed={ debouncedPressed }>
            { children }
        </Button>
    )
};
