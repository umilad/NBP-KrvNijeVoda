import { useAuth } from "./AuthContext";
import axios from "axios";
import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";

// Tipovi za trackovane stranice i top posete
type PageDto = {
    path: string;
    label: string;
};

type TopVisit = {
    path: string;
    label: string;
    count: number;
};

export default function Profil() {
    const { username, token, role, logout } = useAuth();
    const [history, setHistory] = useState<PageDto[]>([]);
    const [topVisits, setTopVisits] = useState<TopVisit[]>([]);
    const navigate = useNavigate();

    // Dohvatanje istorije
    useEffect(() => {
        if (!token) return;

        async function fetchHistory() {
            try {
                const res = await axios.get<PageDto[]>(
                    "http://localhost:5210/api/Auth/history",
                    { headers: { Authorization: `Bearer ${token}` } }
                );

                setHistory(res.data);
            } catch (err) {
                console.error("Error fetching history:", err);
            }
        }

        fetchHistory();
    }, [token]);

    // Dohvatanje najposećenijih stranica
   useEffect(() => {
    if (!token) return;

    async function fetchTopVisits() {
        try {
            // res.data je niz objekata: [{ path, label, count }, ...]
            const res = await axios.get<TopVisit[]>(
                "http://localhost:5210/api/Auth/top-visits",
                { headers: { Authorization: `Bearer ${token}` } }
            );

            console.log("Raw top visits from backend:", res.data);

            // mapiramo direktno na TopVisit tip
            const visits: TopVisit[] = res.data.map(item => ({
                path: item.path,
                label: item.label,
                count: item.count
            }));

            setTopVisits(visits);
        } catch (err) {
            console.error("Error fetching top visits:", err);
        }
    }

    fetchTopVisits();
}, [token]);

    const handleLogout = () => {
        logout();
        navigate("/"); // preusmeri na početnu stranu
    };

    return (
        <div className="profil my-[100px] w-full flex flex-col items-center">

            {/* Gornji box sa korisničkim podacima */}
            <div className="w-4/5 md:w-2/3 lg:w-1/2 mb-10 p-8 rounded-lg bg-[#e6cda5] border-2 border-[#3f2b0a] text-center text-[#3f2b0a]">
                <p className="text-3xl font-bold mb-2">USERNAME: {username}</p>
                <p className="text-2xl">{role}</p>
                <button
                    onClick={handleLogout}
                    className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110 cursor-pointer"
                >
                    Odjavi se
                </button>
            </div>

            {/* Istorija poseta */}
            <div className="w-4/5 md:w-2/3 lg:w-1/2 mb-10">
                <h2 className="text-2xl font-bold mb-4">ISTORIJA POSETA:</h2>

                {history.length === 0 && (
                    <p className="text-gray-600 italic text-lg">Nema posećenih stranica</p>
                )}

                <div className="history-grid grid grid-cols-[repeat(auto-fit,minmax(250px,1fr))] gap-6">
                    {history.map((page, index) => (
                        <Link 
                            to={page.path} 
                            key={index} 
                            className="w-full p-6 rounded-lg bg-[#f5f0e6] border-2 border-[#3f2b0a] text-center text-[#3f2b0a] text-xl font-semibold hover:bg-[#e0d2b8] transition"
                        >
                            {page.label || page.path}
                        </Link>
                    ))}
                </div>
            </div>

            {/* Najposećenije stranice */}
            <div className="w-4/5 md:w-2/3 lg:w-1/2 mt-10">
                <h2 className="text-2xl font-bold mb-4">NAJPOSEĆENIJE STRANICE:</h2>

                {topVisits.length === 0 && (
                    <p className="text-gray-600 italic text-lg">Nema poseta</p>
                )}

                <div className="history-grid grid grid-cols-[repeat(auto-fit,minmax(250px,1fr))] gap-6">
                    {topVisits.map((page, index) => (
                        <Link 
                            to={page.path} 
                            key={index} 
                            className="w-full p-6 rounded-lg bg-[#f5f0e6] border-2 border-[#3f2b0a] text-center text-[#3f2b0a] text-xl font-semibold hover:bg-[#e0d2b8] transition"
                        >
                            {page.label} ({page.count})
                        </Link>
                    ))}
                </div>
            </div>

        </div>
    );
}
