import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Licnost } from "../types";
import { useSearch } from "../components/SearchContext";
import { useAuth } from "../pages/AuthContext"; // dohvat user role

export default function Licnosti() {
    const [licnosti, setLicnosti] = useState<Licnost[]>([]);
    const navigate = useNavigate();
    const { query } = useSearch();
    const { role } = useAuth(); // role korisnika

    // API poziv
    async function GetAllLicnosti() {
        try {
            const response = await axios.get<Licnost[]>("http://localhost:5210/api/GetAllLicnosti");
            return response.data;
        } catch (error) {
            console.error("Error fetching licnosti:", error);
            return [];
        }
    }

    useEffect(() => {
        async function loadAllLicnosti() {
            const data = await GetAllLicnosti();
            setLicnosti(data);
        }
        loadAllLicnosti();
    }, []);

    // Navigate na stranicu ličnosti
    const handleNavigate = (id: string) => navigate(`/licnost/${id}`);

    // Filtriranje po search query
    const filteredLicnosti = licnosti.filter(l =>
        `${l.ime} ${l.prezime} ${l.titula}`.toLowerCase().includes(query.toLowerCase())
    );

    return (
        <div className="licnosti my-[100px]">
            {/* Dugme za admina */}
            {role?.toLowerCase() === "admin" && (
    <div className="flex justify-center mb-8">
        <button
            onClick={() => navigate("/dodaj-licnost")}
            className="bg-[#3f2b0a] text-[#e6cda5] px-8 py-4 text-lg rounded-lg shadow-md hover:bg-[#2b1d07] transition font-bold"
        >
            Dodaj Ličnost
        </button>
    </div>
)}

            <div className='licnosti-grid grid grid-cols-[repeat(auto-fit,minmax(300px,1fr))] gap-6 justify-items-center'>
                {filteredLicnosti.map((licnost) => (
                    <div key={licnost.id} onClick={() => handleNavigate(licnost.id)}
                        className="licnost-div w-[300px] flex flex-col items-center justify-center relative p-[20px] m-[20px] text-center text-[#3f2b0a] overflow-hidden transition-transform hover:scale-110 cursor-pointer">
                        
                        {/* Slika */}
                        <div className="relative w-[259px] h-[300px] m-auto">
                            <img
                                src="/src/images/picture-frame.png"
                                alt="Frame"
                                className="absolute top-0 left-0 w-full h-full z-10 pointer-events-none"
                            />
                            <div className="absolute inset-0 flex items-center justify-center z-0">
                                <img
                                    src={`/src/images/${licnost?.slika}`}
                                    alt="Historical Figure"
                                    className="max-w-[80%] max-h-[80%] object-contain"
                                />
                            </div>
                        </div>

                        {/* Podaci */}
                        <p className="text-2xl font-bold mt-2">{licnost?.titula} {licnost?.ime} {licnost?.prezime}</p>
                        <p className="text-xl font-bold mt-2">
                            {licnost.godinaRodjenja ? `${licnost.godinaRodjenja}${licnost.godinaRodjenjaPNE ? " p.n.e." : ""}` : ""}
                            {licnost.godinaSmrti ? ` - ${licnost.godinaSmrti}${licnost.godinaSmrtiPNE ? " p.n.e." : ""}` : ""}
                        </p>
                    </div>
                ))}
            </div>            
        </div>
    );
}
