import axios from 'axios';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useAuth } from './AuthContext';
import type { Dinastija } from "../types";

export default function Dinastija() {
    const [dinastija, setDinastija] = useState<Dinastija | null>(null);
    const { id } = useParams();
    const { token } = useAuth(); // token je opcionalan

    useEffect(() => {
        async function loadDinastija() {
            if (!id) return;

            try {
                // GET request bez tokena – dinastija je javno dostupna
                const response = await axios.get<Dinastija>(
                    `http://localhost:5210/api/GetDinastija/${id}`
                );

                setDinastija(response.data);

                console.log("Učitana dinastija:", response.data.naziv);

                // Ako postoji token, pratimo posetu
                if (token) {
                    try {
                        const label = `Dinastija: ${response.data.naziv}`;
                        await axios.post(
                            "http://localhost:5210/api/Auth/track",
                            {
                                path: `/dinastija/${id}`,
                                label: label
                            },
                            { headers: { Authorization: `Bearer ${token}` } }
                        );
                    } catch (err) {
                        console.error("Failed to track page", err);
                    }
                }

            } catch (error) {
                console.error("Error fetching dinastija:", error);
            }
        }

        loadDinastija();
    }, [id, token]);

    return (
        <div className="dinastije my-[120px]">
            <div className="pozadinaStabla flex flex-col items-center justify-center relative mx-[100px] p-[20px] border-2 border-[#3f2b0a] bg-[#e6cda5] rounded-lg text-center text-[#3f2b0a]">
                <p className="text-2xl font-bold">{dinastija?.naziv}</p>
                <span className="text-xl font-bold mb-[10px]">
                    {dinastija?.pocetakVladavineGod} - {dinastija?.krajVladavineGod} 
                    {dinastija?.krajVladavinePNE ? " p. n. e." : ""}
                </span>
                <div className="stablo">
                    {/* Tree row - person container */}
                    <div className="relative flex justify-center mt-[60px]">
                        {/* Prva osoba */}
                        <div className="flex-col relative group block text-center text-[#3f2b0a] ">
                            <div className="absolute top-[-36px] left-1/2 transform -translate-x-1/2 w-[2px] h-[40px] bg-[#3f2b0a] z-0" />
                            <div className="absolute top-[54px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            <div className="absolute top-[54px] translate-x-[121px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            <div className="absolute -right-[1px] top-[55px] w-[2px] h-[130px] bg-[#3f2b0a] z-0" />

                            <div className="px-[4px] hover:scale-110 transition-transform duration-300">
                                <img
                                    src="/src/images/download.jpeg"
                                    alt="Historical Figure"
                                    className="w-[80px] h-[100px] object-cover mx-auto my-[4px] border-2 border-[#3f2b0a] rounded-lg"
                                />
                                <p className="text-lg font-bold mt-2">Titula Ime Prezime</p>
                                <p className="text-md mt-1">(od-do)</p>
                            </div>

                            <div className="absolute -top-10 -left-50 w-56 p-3 bg-[#e6cda5] text-[#3f2b0a] border border-[#3f2b0a] rounded-lg shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-300 z-20">
                                <p className="font-semibold">Dodatne informacije:</p>
                                <p>- Rođen: 1234</p>
                                <p>- Umro: 1299</p>
                                <p>- Bio poznat po: nečemu važnom</p>
                            </div>
                        </div>
                        {/* Druga osoba */}
                        <div className="flex-col relative group block text-center text-[#3f2b0a] ">
                            <div className="absolute top-[-36px] left-1/2 transform -translate-x-1/2 w-[2px] h-[40px] bg-[#3f2b0a] z-0" />
                            <div className="absolute top-[54px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            <div className="absolute top-[54px] translate-x-[121px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            <div className="absolute -right-[1px] top-[55px] w-[2px] h-[130px] bg-[#3f2b0a] z-0" />

                            <div className="px-[4px] hover:scale-110 transition-transform duration-300">
                                <img
                                    src="/src/images/download.jpeg"
                                    alt="Historical Figure"
                                    className="w-[80px] h-[100px] object-cover mx-auto my-[4px] border-2 border-[#3f2b0a] rounded-lg"
                                />
                                <p className="text-lg font-bold mt-2">Titula Ime Prezime</p>
                                <p className="text-md mt-1">(od-do)</p>
                            </div>

                            <div className="absolute -top-10 -left-50 w-56 p-3 bg-[#e6cda5] text-[#3f2b0a] border border-[#3f2b0a] rounded-lg shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-300 z-20">
                                <p className="font-semibold">Dodatne informacije:</p>
                                <p>- Rođen: 1234</p>
                                <p>- Umro: 1299</p>
                                <p>- Bio poznat po: nečemu važnom</p>
                            </div>
                        </div>
                    </div>

                    {/* Drugi red stabla */}
                    <div className="relative flex justify-center mt-[60px]">
                        {/* Prva osoba */}
                        <div className="flex-col relative group block text-center text-[#3f2b0a] ">
                            <div className="absolute top-[-36px] left-1/2 transform -translate-x-1/2 w-[2px] h-[40px] bg-[#3f2b0a] z-0" />
                            <div className="absolute top-[54px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            <div className="absolute top-[54px] translate-x-[121px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            <div className="absolute -right-[1px] top-[55px] w-[2px] h-[130px] bg-[#3f2b0a] z-0" />

                            <div className="px-[4px] hover:scale-110 transition-transform duration-300">
                                <img
                                    src="/src/images/download.jpeg"
                                    alt="Historical Figure"
                                    className="w-[80px] h-[100px] object-cover mx-auto my-[4px] border-2 border-[#3f2b0a] rounded-lg"
                                />
                                <p className="text-lg font-bold mt-2">Titula Ime Prezime</p>
                                <p className="text-md mt-1">(od-do)</p>
                            </div>

                            <div className="absolute -top-10 -left-50 w-56 p-3 bg-[#e6cda5] text-[#3f2b0a] border border-[#3f2b0a] rounded-lg shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-300 z-20">
                                <p className="font-semibold">Dodatne informacije:</p>
                                <p>- Rođen: 1234</p>
                                <p>- Umro: 1299</p>
                                <p>- Bio poznat po: nečemu važnom</p>
                            </div>
                        </div>
                        {/* Druga osoba */}
                        <div className="flex-col relative group block text-center text-[#3f2b0a] ">
                            <div className="absolute top-[-36px] left-1/2 transform -translate-x-1/2 w-[2px] h-[40px] bg-[#3f2b0a] z-0" />
                            <div className="absolute top-[54px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            <div className="absolute top-[54px] translate-x-[121px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            <div className="absolute -right-[1px] top-[55px] w-[2px] h-[130px] bg-[#3f2b0a] z-0" />

                            <div className="px-[4px] hover:scale-110 transition-transform duration-300">
                                <img
                                    src="/src/images/download.jpeg"
                                    alt="Historical Figure"
                                    className="w-[80px] h-[100px] object-cover mx-auto my-[4px] border-2 border-[#3f2b0a] rounded-lg"
                                />
                                <p className="text-lg font-bold mt-2">Titula Ime Prezime</p>
                                <p className="text-md mt-1">(od-do)</p>
                            </div>

                            <div className="absolute -top-10 -left-50 w-56 p-3 bg-[#e6cda5] text-[#3f2b0a] border border-[#3f2b0a] rounded-lg shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-300 z-20">
                                <p className="font-semibold">Dodatne informacije:</p>
                                <p>- Rođen: 1234</p>
                                <p>- Umro: 1299</p>
                                <p>- Bio poznat po: nečemu važnom</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
