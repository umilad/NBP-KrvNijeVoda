import { useAuth } from "./AuthContext";
import axios from "axios";
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

// Tip za trackovane stranice
type PageDto = {
    path: string;
    label: string;
};

export default function Profil() {
    const { username, firstName, lastName, token } = useAuth();
    const [history, setHistory] = useState<PageDto[]>([]);

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

    return (
        <div className="profil my-[100px] w-full flex flex-col items-center">

            {/* Gornji box sa korisničkim podacima */}
            <div className="w-4/5 md:w-2/3 lg:w-1/2 mb-10 p-8 rounded-lg bg-[#e6cda5] border-2 border-[#3f2b0a] text-center text-[#3f2b0a]">
                <p className="text-3xl font-bold mb-2">USERNAME: {username}</p>
                <p className="text-2xl">{firstName} {lastName}</p>
            </div>

            {/* Istorija poseta */}
            <div className="w-4/5 md:w-2/3 lg:w-1/2">
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

        </div>
    );
}
