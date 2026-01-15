import { useAuth } from "./AuthContext";
import axios from "axios";
import { useEffect, useState } from "react";
import { Link} from "react-router-dom";

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
    const { token, logout } = useAuth();
    const [history, setHistory] = useState<PageDto[]>([]);
    const [topVisits, setTopVisits] = useState<TopVisit[]>([]);

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

   useEffect(() => {
    if (!token) return;

    async function fetchTopVisits() {
        try {
            
            const res = await axios.get<TopVisit[]>(
                "http://localhost:5210/api/Auth/top-visits",
                { headers: { Authorization: `Bearer ${token}` } }
            );

            console.log("Raw top visits from backend:", res.data);

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

    

    return (
        <div className="profil my-[100px] w-full flex flex-col items-center">
    
            <div className="w-4/5 md:w-2/3 lg:w-1/2 mb-[50px]">
                <h2 className="text-[30px] font-bold mb-4">Istorija poseta:</h2>

                {history.length === 0 && (
                    <p className="text-gray-600 italic text-lg">Nema posećenih stranica</p>
                )}

                <div className="history-grid grid grid-cols-[repeat(auto-fit,minmax(250px,1fr))] gap-6">
                    {history.map((page, index) => (
                        <Link 
                            to={page.path} 
                            key={index} 
                            className="dogadjaj-div flex flex-col text-lg font-semibold border-2 border-[#3f2b0a] bg-[#e6cda5]/70 p-[20px] text-[#3f2b0a] rounded-lg text-center items-center justify-center relative mt-[5px] shadow-md transition-transform hover:scale-110 cursor-pointer"
                        >
                            {page.label || page.path}
                        </Link>
                    ))}
                </div>
            </div>

            
            <div className="w-4/5 md:w-2/3 lg:w-1/2 mt-10">
                <h2 className="text-[30px] font-bold mb-4">Najposećenije stranice:</h2>

                {topVisits.length === 0 && (
                    <p className="text-gray-600 italic text-lg">Nema poseta</p>
                )}

                <div className="history-grid grid grid-cols-[repeat(auto-fit,minmax(270px,1fr))] gap-6">
                    {topVisits.map((page, index) => (
                        <Link 
                            to={page.path} 
                            key={index} 
                            className="dogadjaj-div flex flex-col text-lg font-semibold border-2 border-[#3f2b0a] bg-[#e6cda5]/70 p-[20px] text-[#3f2b0a] rounded-lg text-center items-center justify-center relative mt-[5px] shadow-md transition-transform hover:scale-110 cursor-pointer"
                        >
                            {page.label}
                            <span className="absolute top-2 right-2 text-[13px] uppercase tracking-wide text-[#3f2b0a] px-2 py-0.5 rounded">
                                {page.count}
                            </span>
                        </Link>
                    ))}
                </div>
            </div>

        </div>
    );
}
