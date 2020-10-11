import React from 'react';
import IconArrowDownward from '@material-ui/icons/ArrowDownward';
import IconArrowUpward from '@material-ui/icons/ArrowUpward';
import CircularProgress from '@material-ui/core/CircularProgress';

import { Button, ButtonGroup } from 'components/Button';
import { useDebouncedValue } from 'components/useDebouncedValue';
import { SectionHeader, Section } from 'components/Section';

export const Gear = ({ flightData, dispatchEvent }) => {
    const {
        HandlePosition,
        CenterPosition,
        LeftPosition,
        RightPosition,
        IsRetractable,
        TotalPctExtended,
        ParkingBrakePosition
    } = flightData.Gear;

    const [debouncedHandlePosition, setDebouncedHandlePosition] = useDebouncedValue(HandlePosition);
    const [debouncedParkingBrake, setDebouncedParkingBrake] = useDebouncedValue(ParkingBrakePosition);

    const toggleGear = () => {
        if (debouncedHandlePosition === 0) {
            dispatchEvent("gear:down");
            setDebouncedHandlePosition(1);
        } else if (debouncedHandlePosition === 1) {
            dispatchEvent("gear:up");
            setDebouncedHandlePosition(0);
        }
    };

    const toggleParkingBrakes = () => {
        dispatchEvent("gear:parkingBrakeToggle");
        setDebouncedParkingBrake(!debouncedParkingBrake ? 1 : 0)
    };

    const gearLocked = [0,1].includes(TotalPctExtended);
    const downStyle = {
        color: '#0f0'
    };
    const notDownStyle = {
        color: gearLocked ? '#000' : '#f00'
    };

    return (
        <Section>
            <SectionHeader>
                <h2>Gear</h2>
                <div>
                    {
                        LeftPosition || !IsRetractable ?
                            <IconArrowDownward style={ downStyle } /> :
                            <IconArrowUpward style={ notDownStyle } />
                    }
                    {
                        CenterPosition || !IsRetractable ?
                            <IconArrowDownward style={ downStyle } /> :
                            <IconArrowUpward style={ notDownStyle } />
                    }
                    {
                        RightPosition || !IsRetractable ?
                            <IconArrowDownward style={ downStyle } /> :
                            <IconArrowUpward style={ notDownStyle } />
                    }
                </div>
            </SectionHeader>
            <ButtonGroup>
                <Button
                    onClick={ toggleGear }
                    disabled={ !IsRetractable }
                >
                    { debouncedHandlePosition ? 'Gear up' : 'Gear down' }
                    { gearLocked ?
                        null :
                        <CircularProgress
                            variant="static"
                            value={ TotalPctExtended * 100 }
                            style={ { width: '15px', height: '15px', color: '#fff', marginLeft: '8px' } }
                        />
                    }
                </Button>
                <Button
                    onClick={ toggleParkingBrakes }
                    pressed={ !!debouncedParkingBrake }
                >
                    Parking brakes
                </Button>
            </ButtonGroup>
        </Section>
    );
};
