import axios from 'axios';
import { useEffect, useState, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { useAuth } from './AuthContext';
import PorodicnoStabloPrikaz from "../components/PorodicnoStabloPrikaz";
import type { Dinastija, LicnostTree } from "../types";

export default function Dinastija() {
    const [dinastija, setDinastija] = useState<Dinastija | null>(null);
    const [treeRoots, setTreeRoots] = useState<LicnostTree[]>([]);
    const { id } = useParams();
    const { token } = useAuth(); 
    const hasTracked = useRef(false); 

    useEffect(() => {
        async function loadDinastija() {
            if (!id) return;

            try {
                
                const response = await axios.get<Dinastija>(
                    `http://localhost:5210/api/GetDinastija/${id}`
                );

                setDinastija(response.data);

                console.log("Učitana dinastija:", response.data.naziv);

                if (token && !hasTracked.current) {
                    hasTracked.current = true;
                    const label = `Dinastija: ${response.data.naziv}`;

                    await axios.post(
                        "http://localhost:5210/api/Auth/track",
                        { path: `/dinastija/${id}`, label },
                        { headers: { Authorization: `Bearer ${token}` } }
                    );

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




    return (
        <div className="dinastije my-[120px]">
            <div className="pozadinaStabla flex flex-col min-h-max min-w-max items-center justify-center relative mx-[100px] p-[20px] border-2 border-[#3f2b0a] bg-[#e6cda5] rounded-lg text-center text-[#3f2b0a]">
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

            </div>
        </div>
    );
}
