import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Dogadjaj, Rat, Bitka } from "../types";
import { useSearch } from "../components/SearchContext";
import { useAuth } from "../pages/AuthContext"; // ✅ import auth konteksta

export default function Dogadjaji() {
    const [dogadjaji, setDogadjaji] = useState<Dogadjaj[]>([]);
    const navigate = useNavigate();
    const { query } = useSearch();
    const { role } = useAuth(); // ✅ dohvat role korisnika

    // API poziv
    
    useEffect(() => {
        async function GetAllDogadjaji() {
            try {
                const response = await axios.get<Dogadjaj[]>("http://localhost:5210/api/GetAllDogadjaji");
                return response.data;
            } catch (error) {
                console.error("Error fetching dogadjaji:", error);
                return [];
            }
        }

        async function GetAllBitke() {
            try {
                const response = await axios.get<Bitka[]>("http://localhost:5210/api/GetAllBitke");
                return response.data;
            } catch (error) {
                console.error("Error fetching bitke:", error);
                return [];
            }
        }

        async function GetAllRatovi() {
            try {
                const response = await axios.get<Rat[]>("http://localhost:5210/api/GetAllRatovi");
                return response.data;
            } catch (error) {
                console.error("Error fetching ratovi:", error);
                return [];
            }
        }
        async function loadAllDogadjaji() {
            try {
                const [dogadjajiData, ratoviData, bitkeData] = await Promise.all([
                    GetAllDogadjaji(),
                    GetAllRatovi(),
                    GetAllBitke()
                ]);
                const allData = [
                    ...(dogadjajiData ?? []),
                    ...(ratoviData ?? []),
                    ...(bitkeData ?? [])
                ]
                setDogadjaji(allData);
            }
            catch (error) {
                console.error("Error loading events:", error);
            }            
        }
        loadAllDogadjaji();
    }, []);

    const handleNavigate = (id: string) => navigate(`/dogadjaj/${id}`);
    const handleAddDogadjaj = () => navigate("/dodaj-dogadjaj"); // ✅ navigacija na stranicu za dodavanje

    // Filtriranje po search query
    const filteredDogadjaji = dogadjaji.filter(d =>
        d.ime.toLowerCase().includes(query.toLowerCase())
    );

    return (
        <div className="dogadjaji my-[100px]">
            {/* Dugme Dodaj događaj, vidi ga samo admin */}
            {role === "admin" && (
    <div className="flex justify-center mb-12">
        <button
            onClick={handleAddDogadjaj}
            className="px-12 py-6 bg-[#3f2b0a] text-[#e6cda5] text-3xl font-extrabold rounded-3xl shadow-2xl hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110"
        >
            Dodaj događaj
        </button>
    </div>
)}

            <div className='dogadjaji-grid grid grid-cols-[repeat(auto-fit,minmax(400px,1fr))] gap-6 justify-items-center'>
                {filteredDogadjaji.map((dogadjaj) => (
                    <div key={dogadjaj.id} onClick={() => handleNavigate(dogadjaj.id)}
                        className="dogadjaj-div w-[400px] flex flex-col items-center justify-center relative border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] m-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md overflow-hidden transition-transform hover:scale-110 cursor-pointer">
                        
                        <span className='dogadjaj-header text-xl font-bold mt-2'>{dogadjaj.ime}</span>
                        <span className='dogadjaj-godina text-l font-bold mt-2'>
                            {dogadjaj.godina ? `${dogadjaj.godina.god}` : ""}
                            {dogadjaj
                                ? (("godinaDo" in dogadjaj && dogadjaj.godinaDo)
                                    ? ` - ${dogadjaj.godinaDo}. ${dogadjaj.godinaDo ? " p.n.e." : ""}`
                                    : dogadjaj.godina
                                        ? `${dogadjaj.godina ? " p. n. e." : ""}`
                                        : "")
                                : ""}
                        </span>
                        <span className='text-justify'>
                            {dogadjaj.tekst}
                        </span>
                    </div>
                ))}
            </div>
        </div>
    );
}
