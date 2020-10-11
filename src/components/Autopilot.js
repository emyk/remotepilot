import React from 'react';
import IconAdd from '@material-ui/icons/Add'
import IconRemove from '@material-ui/icons/Remove'
import IconSync from '@material-ui/icons/Sync'

import { Button, ButtonGroup, HoldButton, ToggleButton } from "components/Button";
import { Value, Toggle, Actions, Content } from 'components/Value';
import { useDebouncedValue } from 'components/useDebouncedValue';
import { SectionHeader, Section, Block } from 'components/Section';

export const Autopilot = ({ dispatchEvent, flightData }) => {
    const {
        Airspeed,
        AirspeedHold,
        Altitude,
        AltitudeHold,
        Approach,
        Autothrottle,
        FlightDirector,
        HeadingHold,
        HeadingDir,
        Master,
        Nav1,
        VerticalSpeed,
        VerticalSpeedHold
    } = flightData.Autopilot;

    const { TrueHeading } = flightData;

    console.log(TrueHeading);

    const [headingDir, setHeadingDir] = useDebouncedValue(HeadingDir);
    const [airspeed, setAirspeed] = useDebouncedValue(Airspeed);
    const [altitude, setAltitude] = useDebouncedValue(Altitude);
    const [verticalSpeed, setVerticalSpeed] = useDebouncedValue(VerticalSpeed);

    const headingTick = (value) => setHeadingDir(value);
    const headingSet = value => {
        dispatchEvent(`autopilot:headingSet:${value}`);
        setHeadingDir(value);
    };

    const airspeedTick = (value) => setAirspeed(value);
    const airspeedSet = value => {
        dispatchEvent(`autopilot:airspeedSet:${value}`);
        setAirspeed(value);
    };

    const altitudeTick = (value) => setAltitude(value);
    const altitudeSet = value => {
        dispatchEvent(`autopilot:altSet:${value}`);
        setAltitude(value);
    };

    const verticalSpeedTick = (value) => setVerticalSpeed(value);
    const verticalSpeedSet = value => {
        dispatchEvent(`autopilot:verticalSpeedSet:${value}`);
        setVerticalSpeed(value);
    };

    return (
        <Section>
            <SectionHeader>
                <h2>Autopilot</h2>

                <ToggleButton
                    onClick={ () => dispatchEvent('autopilot:masterToggle') }
                    pressed={ Master }
                >
                    MASTER AP
                </ToggleButton>
            </SectionHeader>

            <Block>
                <Value>
                    <Toggle>
                        <ToggleButton
                            onClick={ () => dispatchEvent('autopilot:airspeedHoldToggle') }
                            pressed={ AirspeedHold }
                        >
                            SPD
                        </ToggleButton>
                    </Toggle>
                    <Content>
                        Airspeed: { airspeed }
                    </Content>
                    <Actions>
                        <HoldButton
                            value={ airspeed }
                            onTick={ airspeedTick }
                            mutator={ value => value > 0 ? value - 1 : 0 }
                            onChange={ airspeedSet }
                            type="round"
                        >
                            <IconRemove />
                        </HoldButton>
                        <HoldButton
                            value={ airspeed }
                            onTick={ airspeedTick }
                            mutator={ value => value + 1 }
                            onChange={ airspeedSet }
                            type="round"
                        >
                            <IconAdd />
                        </HoldButton>
                    </Actions>
                </Value>

                <Value>
                    <Toggle>
                        <ToggleButton
                            onClick={ () => dispatchEvent('autopilot:headingHoldToggle') }
                            pressed={ HeadingHold }
                        >
                            HDG
                        </ToggleButton>
                    </Toggle>
                    <Content>Heading: { headingDir }</Content>
                    <Actions>
                        <Button
                            value={ headingDir }
                            onClick={ () => dispatchEvent(`autopilot:headingSet:${Math.ceil(TrueHeading)}`) }
                            type="round"
                        >
                            <IconSync />
                        </Button>
                        <HoldButton
                            value={ headingDir }
                            onTick={ headingTick }
                            mutator={ value => value > 1 ? value - 1 : 360 }
                            onChange={ headingSet }
                            type="round"
                        >
                            <IconRemove />
                        </HoldButton>
                        <HoldButton
                            value={ headingDir }
                            onTick={ headingTick }
                            mutator={ value => value < 360 ? value + 1 : 1 }
                            onChange={ headingSet }
                            type="round"
                        >
                            <IconAdd />
                        </HoldButton>
                    </Actions>
                </Value>

                <Value>
                    <Toggle>
                        <ToggleButton
                            onClick={ () => dispatchEvent('autopilot:altHoldToggle') }
                            pressed={ AltitudeHold }
                        >
                            ALT
                        </ToggleButton>
                    </Toggle>
                    <Content>
                        Altitude: { altitude }
                    </Content>
                    <Actions>
                        <HoldButton
                            value={ altitude }
                            onTick={ altitudeTick }
                            mutator={ value => value > 100 ? value - 100 : 0 }
                            onChange={ altitudeSet }
                            type="round"
                        >
                            <IconRemove />
                        </HoldButton>
                        <HoldButton
                            value={ altitude }
                            onTick={ altitudeTick }
                            mutator={ value => value + 100 }
                            onChange={ altitudeSet }
                            type="round"
                        >
                            <IconAdd />
                        </HoldButton>
                    </Actions>
                </Value>

                <Value>
                    <Toggle>
                        <ToggleButton
                            onClick={ () => dispatchEvent('autopilot:verticalSpeedToggle') }
                            pressed={ VerticalSpeedHold }
                        >
                            VS
                        </ToggleButton>
                    </Toggle>
                    <Content>
                        Vertical speed: { verticalSpeed }
                    </Content>
                    <Actions>
                        <HoldButton
                            value={ verticalSpeed }
                            onTick={ verticalSpeedTick }
                            mutator={ value => value - 100 }
                            onChange={ verticalSpeedSet }
                            type="round"
                        >
                            <IconRemove />
                        </HoldButton>
                        <HoldButton
                            value={ verticalSpeed }
                            onTick={ verticalSpeedTick }
                            mutator={ value => value + 100 }
                            onChange={ verticalSpeedSet }
                            type="round"
                        >
                            <IconAdd />
                        </HoldButton>
                    </Actions>
                </Value>

                <ButtonGroup>
                    <ToggleButton
                        onClick={ () => dispatchEvent('autopilot:locHoldToggle') }
                        pressed={ false }
                    >
                        LOC
                    </ToggleButton>
                    <ToggleButton
                        onClick={ () => dispatchEvent('autopilot:apprHoldToggle') }
                        pressed={ Approach }
                    >
                        APPR
                    </ToggleButton>
                    <ToggleButton
                        onClick={ () => dispatchEvent('autopilot:navHoldToggle') }
                        pressed={ Nav1 }
                    >
                        NAV
                    </ToggleButton>
                    <ToggleButton
                        onClick={ () => dispatchEvent('autopilot:fdToggle') }
                        pressed={ FlightDirector }
                    >
                        FD
                    </ToggleButton>
                    <ToggleButton
                        onClick={ () => dispatchEvent('autopilot:autoThrottleToggle') }
                        pressed={ Autothrottle }
                    >
                        ATHR
                    </ToggleButton>
                </ButtonGroup>
            </Block>
        </Section>
    );
};

/*<Button onClick={ onClick('autopilot:setAlt:10000') }>setAlt</Button>*/

