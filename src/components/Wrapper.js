import React, { Component } from 'react';

export class Wrapper extends Component {

    static getDerivedStateFromError(error) {
        return {
            error
        };
    }

    state = {};

    componentDidCatch(error, errorInfo) {
        console.log('didCatch', error, errorInfo);
    }

    render() {
        const { children } = this.props;
        const { error } = this.state;

        if (error) {
            return (
                <div>error lols</div>
            );
        }

        return children;
    }
}
