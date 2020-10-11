import React from 'react';

import { Section, SectionHeader } from "./Section";
import { Button, ButtonGroup } from "./Button";

// events not working
export const Camera = ({ dispatchEvent }) => {
    return (
        <Section>
            <SectionHeader>
                <h2>Camera</h2>
            </SectionHeader>
            <ButtonGroup>
                <Button
                    onClick={ () => dispatchEvent('camera:chaseView') }
                >
                    Toggle chase view
                </Button>
                <Button
                    onClick={ () => dispatchEvent('camera:viewMode') }
                >
                    View mode
                </Button>
            </ButtonGroup>
        </Section>
    )
};
