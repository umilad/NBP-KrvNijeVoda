import axios from 'axios';
import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from './AuthContext';
import PorodicnoStabloPrikaz from "../components/PorodicnoStabloPrikaz";
import type { Dinastija, LicnostTree } from "../types";

export default function Dinastija() {
    const [dinastija, setDinastija] = useState<Dinastija | null>(null);
    const [treeRoots, setTreeRoots] = useState<LicnostTree[]>([]);
    const { id } = useParams();
    const { token } = useAuth(); // token je opcionalan
    const hasTracked = useRef(false); // flag da ne pošaljemo više puta
    const navigate = useNavigate();

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

                // Ako postoji token i još nismo trackovali
                if (token && !hasTracked.current) {
                    hasTracked.current = true;
                    const label = `Dinastija: ${response.data.naziv}`;

                    // track za history list
                    await axios.post(
                        "http://localhost:5210/api/Auth/track",
                        { path: `/dinastija/${id}`, label },
                        { headers: { Authorization: `Bearer ${token}` } }
                    );

                    // track za broj poseta (hash)
                    await axios.post(
                        "http://localhost:5210/api/Auth/track-visit",
                        { path: `/dinastija/${id}`, label },
                        { headers: { Authorization: `Bearer ${token}` } }
                    );
                }

            } catch (error) {
                console.error("Error fetching dinastija:", error);
            }
        }

        loadDinastija();
    }, [id, token]);

    useEffect(() => {
        if (!id) return;

        async function loadTree() {
            try {
                const res = await axios.get<LicnostTree[]>(
                    `http://localhost:5210/api/GetDinastijaTree/${id}`
                );
                console.log(res);
                setTreeRoots(res.data);
            } catch (err) {
                console.error(err);
            }
        }

        loadTree();
    }, [id]);


    const handleDelete = async () => {
        if (!id || !token) return;
        if (!window.confirm("Da li ste sigurni da želite da obrišete ovu dinastiju?")) return;

        try {
            await axios.delete(`http://localhost:5210/api/DeleteDinastija/${id}`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            alert("Dinastija obrisana!");
            navigate("/dinastije");
        } catch (error) {
            console.error("Greška prilikom brisanja:", error);
        }
    };

    const handleUpdate = () => {
        if (!id) return;
        navigate(`/dinastija/edit/${id}`);
    };

    return (
        <div className="dinastije my-[120px]">
            <div className="pozadinaStabla flex flex-col items-center justify-center relative mx-[100px] p-[20px] border-2 border-[#3f2b0a] bg-[#e6cda5] rounded-lg text-center text-[#3f2b0a]">
                <p className="text-2xl font-bold">{dinastija?.naziv}</p>
                <span className="text-xl font-bold mb-[20px]">
                    {dinastija?.pocetakVladavineGod} - {dinastija?.krajVladavineGod}. 
                    {dinastija?.krajVladavinePNE ? " p. n. e." : ""}
                </span>

                <div className="flex justify-center">
                    {treeRoots.map(root => (
                        <PorodicnoStabloPrikaz key={root.id} licnost={root} />
                    ))}
                </div>

                
                {/* DUGMAD NA DNU */}
                {dinastija && token && (
                    <div className="mt-6 flex gap-4 justify-center">
                        <button
                            onClick={handleDelete}
                            className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 transition"
                        >
                            Obriši
                        </button>
                        <button
                            onClick={handleUpdate}
                            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
                        >
                            Ažuriraj
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}
