import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Licnost, Vladar } from "../types";
import { useSearch } from "../components/SearchContext";
import { useAuth } from "../pages/AuthContext"; // dohvat user role
import LicnostPrikaz from "../components/LicnostPrikaz";

export default function Licnosti() {
    const [licnosti, setLicnosti] = useState<Licnost[]>([]);
    const navigate = useNavigate();
    const { query } = useSearch();
    const { role } = useAuth(); // role korisnika

    // API poziv
    

    useEffect(() => {
        async function GetAllLicnosti() {
            try {
                const response = await axios.get<Licnost[]>("http://localhost:5210/api/GetAllLicnosti");
                return response.data;
            } catch (error) {
                console.error("Error fetching licnosti:", error);
                return [];
            }
        }
        async function GetAllVladare() {
            try {
                const response = await axios.get<Vladar[]>("http://localhost:5210/api/GetAllVladare");
                return response.data;
            } catch (error) {
                console.error("Error fetching vladare:", error);
                return [];
            }
        }

        async function loadAllLicnosti() {
            try {
                const start = performance.now();
                const [vladariData, licnostiData] = await Promise.all([
                    GetAllVladare(),
                    GetAllLicnosti()
                ]);
                const allData = [
                    ...(vladariData ?? []),
                    ...(licnostiData ?? [])
                ]
                const end = performance.now();
                console.log(`⏱️ Request took ${(end - start).toFixed(2)} ms`);
                setLicnosti(allData);
            }
            catch (error) {
                console.error("Error loading events:", error);
            }  
        }
        loadAllLicnosti();       
    }, []);

    // Navigate na stranicu ličnosti
    //const handleNavigate = (id: string) => navigate(`/licnost/${id}`);

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
                    <LicnostPrikaz key={licnost.id} licnost={licnost} />
                ))}
            </div>            
        </div>
    );
}
