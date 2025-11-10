import axios from 'axios';
import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from './AuthContext';
import type { Dogadjaj } from "../types";

export default function Dogadjaj() { 
    const [dogadjaj, setDogadjaj] = useState<Dogadjaj | null>(null);
    const { id } = useParams();
    const { token, role } = useAuth(); // dodali smo role
    const hasTracked = useRef(false);
    const navigate = useNavigate();

    useEffect(() => {
        async function loadDogadjaj() {
            if (!id) return;
            try {
                const response = await axios.get<Dogadjaj>(`http://localhost:5210/api/GetDogadjaj/${id}`);
                setDogadjaj(response.data);

                if (token && !hasTracked.current) {
                    hasTracked.current = true;

                    // track za history list
                    await axios.post(
                        "http://localhost:5210/api/Auth/track",
                        { path: `/dogadjaj/${id}`, label: response.data.ime || "" },
                        { headers: { Authorization: `Bearer ${token}` } }
                    );

                    // track za broj poseta (hash)
                    await axios.post(
                        "http://localhost:5210/api/Auth/track-visit",
                        { path: `/dogadjaj/${id}`, label: response.data.ime || "" },
                        { headers: { Authorization: `Bearer ${token}` } }
                    );
                }
            } catch (err) {
                console.error(err);
            }
        }

        loadDogadjaj();
    }, [id, token]);

    // Brisanje događaja
    const handleDelete = async () => {
        if (!id || !token) return;
        if (!confirm("Da li ste sigurni da želite da obrišete ovaj događaj?")) return;

        try {
            await axios.delete(`http://localhost:5210/api/DeleteDogadjaj/${id}`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            alert("Događaj obrisan");
            navigate("/dogadjaji"); // ili na listu događaja
        } catch (err) {
            console.error(err);
            alert("Greška prilikom brisanja događaja");
        }
    };

    // Navigacija na stranicu za ažuriranje
    const handleUpdate = () => {
        if (!id) return;
        navigate(`/dogadjaj/edit/${id}`);
    };

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

                {/* Dugmad samo za admina */}
                {role === "admin" && (
                    <div className="flex gap-4 justify-center mt-4">
                        <button
                            onClick={handleDelete}
                            className="px-6 py-3 text-white bg-red-600 hover:bg-red-700 rounded-lg text-lg font-bold"
                        >
                            Obriši
                        </button>
                        <button
                            onClick={handleUpdate}
                            className="px-6 py-3 text-white bg-blue-600 hover:bg-blue-700 rounded-lg text-lg font-bold"
                        >
                            Ažuriraj
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}
