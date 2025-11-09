import axios from 'axios';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useAuth } from './AuthContext';
import type { Dogadjaj } from "../types";

export default function Dogadjaj() { 
    const [dogadjaj, setDogadjaj] = useState<Dogadjaj | null>(null);
    const { id } = useParams();
    const { token } = useAuth(); // token je opcionalan

    useEffect(() => {
        async function loadDogadjaj() {
            if (!id) return;

            try {
                // GET request bez tokena (događaj je javno dostupan)
                const response = await axios.get<Dogadjaj>(
                    `http://localhost:5210/api/GetDogadjaj/${id}`
                );

                setDogadjaj(response.data);

                console.log("Učitani dogadjaj:", response.data.ime);

                // Ako postoji token, pratimo posetu
                if (token) {
                    try {
                        await axios.post(
                            "http://localhost:5210/api/Auth/track",
                            {
                                path: `/dogadjaj/${id}`,
                                label: response.data.ime || ""
                            },
                            { headers: { Authorization: `Bearer ${token}` } }
                        );
                    } catch (err) {
                        console.error("Failed to track page", err);
                    }
                }

            } catch (err) {
                console.error("Error fetching dogadjaj:", err);
            }
        }

        loadDogadjaj();
    }, [id, token]);

    return (
        <div className="dogadjaj-container flex flex-col items-center justify-center text-white"> 
            <div className="absolute top-30 w-5/6 mx-[100px] p-[20px] block border-2 border-[#3f2b0a] bg-[#e6cda5] rounded-lg text-center text-[#3f2b0a] mt-4">
                <p className="text-2xl font-bold mt-2">{dogadjaj?.ime}</p>
                <span className="text-xl font-bold mt-2">
                    {dogadjaj?.godina ? `${dogadjaj?.godina.god}` : ""}
                    {dogadjaj
                        ? (("godinaDo" in dogadjaj && dogadjaj.godinaDo)
                            ? ` - ${dogadjaj.godinaDo}. ${dogadjaj.godinaDo ? "p.n.e." : "" }`
                            : dogadjaj.godina
                                ? `${dogadjaj.godina ? "p. n. e." : ""}` 
                                : "")
                        : ""}
                </span>
                <div>
                    <p className="text-lg p-[30px] mt-2 text-justify">{dogadjaj?.tekst}</p>
                </div>
            </div>
        </div>
    );
}
