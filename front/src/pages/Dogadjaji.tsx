import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Dogadjaj, Rat, Bitka } from "../types";
import { useSearch } from "../components/SearchContext";
import { useAuth } from "../pages/AuthContext"; 
import DogadjajPrikaz from '../components/DogadjajPrikaz';

export default function Dogadjaji() {
    const [dogadjaji, setDogadjaji] = useState<Dogadjaj[]>([]);
    const navigate = useNavigate();
    const { query } = useSearch();
    const { role } = useAuth(); 

    
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

    const handleAddDogadjaj = () => navigate("/dodaj-dogadjaj");

    const filteredDogadjaji = dogadjaji.filter(d =>
        d.ime.toLowerCase().includes(query.toLowerCase())
    );

    return (
        <div className="dogadjaji my-[100px]">
            
            {role === "admin" && (
                <div className="flex justify-center mb-[12px]">
                    <button
                        onClick={handleAddDogadjaj}
                        className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110 cursor-pointer"
                    >
                        Dodaj događaj
                    </button>
                </div>
            )}

            <div className='dogadjaji-grid grid grid-cols-[repeat(auto-fit,minmax(400px,1fr))] gap-6 justify-items-center'>
                {filteredDogadjaji.map((dogadjaj) => (
                    <DogadjajPrikaz key={dogadjaj.id} dogadjaj={dogadjaj} variant="short" />
                    
                ))}
            </div>
        </div>
    );
}
