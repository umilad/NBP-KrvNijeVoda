import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSearch } from "../components/SearchContext";
import { useAuth } from "../pages/AuthContext";
import type { Dinastija } from "../types";
import DinastijaPrikaz from "../components/DinastijaPrikaz"

export default function Dinastije() {
    const [dinastije, setDinastije] = useState<Dinastija[]>([]);
    const navigate = useNavigate();
    const { query } = useSearch();
    const { role } = useAuth(); 

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

    const handleDodaj = () => navigate("/dodaj-dinastiju"); 

    const filteredDinastije = dinastije.filter(d =>
        d.naziv.toLowerCase().includes(query.toLowerCase())
    );

    return (
        <div className="dinastije my-[100px]">
           {role === "admin" && (
            <div className="flex justify-center mb-6">
                <button
                    onClick={handleDodaj}
                    className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110 cursor-pointer"
                >
                    Dodaj Dinastiju
                </button>
            </div>
        )}

            <div className='dinastije-grid grid grid-cols-[repeat(auto-fit,minmax(400px,1fr))] gap-6 justify-items-center'>
                {filteredDinastije.map((dinastija) => (
                    <DinastijaPrikaz key={dinastija?.id}
                                     dinastija={dinastija} />
                ))}
            </div>
        </div>                
    );
}
