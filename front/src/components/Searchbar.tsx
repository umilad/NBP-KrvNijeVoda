import { useLocation } from "react-router-dom";
import { useMemo, useEffect, useState } from "react";
import { useSearch } from "./SearchContext";

interface SearchBarProps {
  onSearch?: () => void;
}

export default function SearchBar({ onSearch }: SearchBarProps) {
    const location = useLocation();
    const { query, setQuery } = useSearch();    
    const [typing, setTyping] = useState(false);

    useEffect(() => {
        if (!onSearch) return;
        if (!query) return;

        setTyping(true);
        const timeout = setTimeout(() => {
        onSearch();
        setTyping(false);
        }, 500);

        return () => clearTimeout(timeout);
    }, [query, onSearch]);

    const placeholder = useMemo(() => {
        if (location.pathname.startsWith("/licnosti")) return "Pretraži ličnosti...";
        if (location.pathname.startsWith("/dogadjaji")) return "Pretraži događaje...";
        if (location.pathname.startsWith("/dogadjaj")) return "Pretraži događaje...";
        if (location.pathname.startsWith("/dinastije")) return "Pretraži dinastije...";        
        if (location.pathname.startsWith("/dinastija")) return "Pretraži dinastije...";
        return "Pretraži godine...";
    }, [location.pathname]);

    const isDisabled = useMemo(() => {
        return location.pathname.startsWith("/prijava") || location.pathname.startsWith("/registracija");
    }, [location.pathname]);

    return (
        <input
            type="text"
            className="px-[30px] h-full border-none focus:outline-none"
            placeholder={placeholder}
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            disabled={isDisabled}
        />
    );
}
