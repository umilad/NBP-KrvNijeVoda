import { useLocation } from "react-router-dom";
import { useMemo } from 'react';

export default function SearchBar() {
    const location = useLocation();

    const placeholder = useMemo(() => {
        if (location.pathname.startsWith("/licnosti")) return "Pretraži ličnosti...";
        if (location.pathname.startsWith("/dogadjaji")) return "Pretraži događaje...";
        if (location.pathname.startsWith("/dinastije")) return "Pretraži dinastije...";
        if (location.pathname.startsWith("/")) return "Pretraži godine...";
        return "Pretraži...";
    }, [location.pathname]);

    return (
        <input
        type="text"
        className="absolute start-8 w-50 h-full px-4 py-2 border-none focus:outline-none "
        placeholder={placeholder}
        />
    );
}