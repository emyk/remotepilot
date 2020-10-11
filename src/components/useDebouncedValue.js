import { useRef, useState } from "react";

export const useDebouncedValue = (value) => {
    const [debouncedValue, setDebouncedValue] = useState(null);
    const timeoutRef = useRef();

    const setDebounced = (newDebounced) => {
        setDebouncedValue(newDebounced);
        clearTimeout(timeoutRef.current);
        timeoutRef.current = setTimeout(
            () => setDebouncedValue(null),
            2000
        );
    };

    return [debouncedValue != null ? debouncedValue : value, setDebounced];
};
