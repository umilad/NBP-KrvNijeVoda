import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSearch } from "../components/SearchContext";
import { useAuth } from "../pages/AuthContext"; // ✅ import AuthContext
import type { Dinastija } from "../types";

export default function Dinastije() {
    const [dinastije, setDinastije] = useState<Dinastija[]>([]);
    const navigate = useNavigate();
    const { query } = useSearch();
    const { role } = useAuth(); // ✅ uzimamo ulogu korisnika

    // --- API poziv ---
    useEffect(() => {
        async function GetAllDinastije() {
            try {
                const response = await axios.get<Dinastija[]>("http://localhost:5210/api/GetAllDinastije");
                return response.data;
            } catch (error) {
                console.error("Error fetching dinastije:", error);
                return [];
            }
        }

        async function loadAllDinastije() {
            const data = await GetAllDinastije();
            setDinastije(data);
        }

        loadAllDinastije();
    }, []);

    const handleNavigate = (id: string) => navigate(`/dinastija/${id}`);
    const handleDodaj = () => navigate("/dodaj-dinastiju"); // ruta za dodavanje

    const filteredDinastije = dinastije.filter(d =>
        d.naziv.toLowerCase().includes(query.toLowerCase())
    );

    return (
        <div className="dinastije my-[100px]">
            {/* --- Dugme Dodaj Dinastiju samo za admin --- */}
           {role === "admin" && (
            <div className="flex justify-center mb-6">
                <button
                    onClick={handleDodaj}
                    className="bg-[#3f2b0a] text-[#e6cda5] px-6 py-2 rounded-lg shadow-md hover:bg-[#2b1d07] transition"
                >
                    Dodaj Dinastiju
                </button>
            </div>
        )}

            <div className='dinastije-grid grid grid-cols-[repeat(auto-fit,minmax(400px,1fr))] gap-6 justify-items-center'>
                {filteredDinastije.map((dinastija) => (
                    <div 
                        key={dinastija.id} 
                        onClick={() => handleNavigate(dinastija.id)}
                        className="dinastija-div w-[400px] flex flex-col items-center justify-center relative border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] m-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md overflow-hidden transition-transform hover:scale-110 cursor-pointer"
                    >
                        <span className='dogadjaj-header text-xl font-bold mt-2'>{dinastija.naziv}</span>
                        <span className='dogadjaj-godina text-l font-bold mt-2'>
                            {dinastija.pocetakVladavineGod} - {dinastija.krajVladavineGod}. 
                            {dinastija.krajVladavinePNE ? " p. n. e." : ""}
                        </span>

                        <div className="relative w-[300px] h-[355px] m-auto">
                            <div className="absolute inset-0 top-0 flex items-center justify-center z-0">
                                <img
                                    src={`/src/images/${dinastija?.slika}`}
                                    alt={dinastija.naziv}
                                    className="w-[190px] h-[235px] object-cover"
                                />
                            </div>
                        </div>
                    </div>
                ))}
            </div>
        </div>                
    );
}
