import { useLocation } from "react-router-dom";
import { useMemo } from 'react';

export default function SearchBar() {
    const location = useLocation();

    const placeholder = useMemo(() => {
        if (location.pathname.startsWith("/licnosti")) return "Pretraži ličnosti...";
        if (location.pathname.startsWith("/dogadjaji")) return "Pretraži događaje...";
        if (location.pathname.startsWith("/dogadjaj")) return "Pretraži događaje...";
        if (location.pathname.startsWith("/dinastije")) return "Pretraži dinastije...";        
        if (location.pathname.startsWith("/dinastija")) return "Pretraži dinastije...";
        if (location.pathname.startsWith("/prijava")) return "Pretraži...";
        if (location.pathname.startsWith("/registracija")) return "Pretraži...";
        if (location.pathname.startsWith("/")) return "Pretraži godine...";
        return "Pretraži...";
    }, [location.pathname]);

    const isDisabled = useMemo(() => {
        return location.pathname.startsWith("/prijava") || location.pathname.startsWith("/registracija");
    }, [location.pathname]);

    return (
        <input
        type="text"
        className="absolute start-8 w-50 h-full px-4 py-2 border-none focus:outline-none "
        placeholder={placeholder}
        disabled={isDisabled}
        />
    );
}