import axios from 'axios';
import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';
import type { Licnost } from "../types";

export default function Licnost() {
    const [licnost, setLicnost] = useState<Licnost | null>(null);
    const [tip, setTip] = useState<"Bitka" | "Rat" | "Dogadjaj" | null>(null); // novi state za tip
    const { id } = useParams();
    const { token, role } = useAuth();
    const hasTracked = useRef(false);
    const navigate = useNavigate();
    const location = useLocation<{ isVladar?: boolean }>();

    useEffect(() => {
        async function load() {
            if (!id) return;
            try {
                const endpoint = location.state?.isVladar 
                    ? `http://localhost:5210/api/GetVladar/${id}` 
                    : `http://localhost:5210/api/GetLicnost/${id}`;

                const res = await axios.get<Licnost & { tip?: string }>(endpoint);
                const data = res.data;
                setLicnost(data);

                // Postavi tip ako postoji
                if (data.tip) {
                    setTip(
                        data.tip === "Bitka" ? "Bitka" :
                        data.tip === "Rat" ? "Rat" : "Dogadjaj"
                    );
                } else {
                    setTip("Dogadjaj");
                }

                // Praćenje
                if (token && !hasTracked.current) {
                    hasTracked.current = true;
                    const label = `${data.titula || ""} ${data.ime || ""} ${data.prezime || ""}`.trim();

                    await axios.post(
                        "http://localhost:5210/api/Auth/track",
                        { path: location.state?.isVladar ? `/vladar/${id}` : `/licnost/${id}`, label },
                        { headers: { Authorization: `Bearer ${token}` } }
                    );
                    await axios.post(
                        "http://localhost:5210/api/Auth/track-visit",
                        { path: location.state?.isVladar ? `/vladar/${id}` : `/licnost/${id}`, label },
                        { headers: { Authorization: `Bearer ${token}` } }
                    );
                }
            } catch (err) {
                console.error(err);
            }
        }

        load();
    }, [id, token, location.state]);

    const handleDelete = async () => {
        if (!id || !token || !tip) return;
        if (!confirm("Da li ste sigurni da želite da obrišete ovu ličnost/događaj?")) return;

        try {
            let endpoint = "";
            if (tip === "Bitka") endpoint = `http://localhost:5210/api/DeleteBitka/${id}`;
            else if (tip === "Rat") endpoint = `http://localhost:5210/api/DeleteRat/${id}`;
            else endpoint = `http://localhost:5210/api/DeleteDogadjaj/${id}`;

            await axios.delete(endpoint, {
                headers: { Authorization: `Bearer ${token}` }
            });

            alert("Uspešno obrisano!");
            navigate("/licnosti");
        } catch (err) {
            console.error(err);
            alert("Greška prilikom brisanja");
        }
    };

    const handleUpdate = () => {
        if (!id) return;
        navigate(`/licnost/edit/${id}`, { state: { isVladar: location.state?.isVladar } });
    };

    if (!licnost) return <div className="text-center mt-20">Učitavanje...</div>;

    return (
        <div className="licnosti-container flex flex-col items-center justify-center text-white my-[100px]">
            <div className="relative w-[300px] h-[355px] m-auto">
                <img src="/src/images/picture-frame.png" alt="Frame" className="absolute top-0 left-0 w-full h-full z-10 pointer-events-none" />
                <div className="absolute inset-0 flex items-center justify-center z-0">
                    <img src={`/src/images/${licnost.slika}`} alt="Historical Figure" className="w-[190px] h-[235px] object-cover" />
                </div>
            </div>

            <div className="absolute top-100 mx-[100px] p-[20px] block border-2 border-[#3f2b0a] bg-[#e6cda5] rounded-lg text-center text-[#3f2b0a] mt-4">
                <p className="text-2xl font-bold mt-2">{licnost.titula} {licnost.ime} {licnost.prezime}</p>
                <p className="text-xl font-bold mt-2">
                    {licnost.godinaRodjenja ?? ""} {licnost.godinaSmrti ? `- ${licnost.godinaSmrti}` : ""}
                    {licnost.godinaRodjenjaPNE || licnost.godinaSmrtiPNE ? " p.n.e." : ""}
                </p>
                <p className="text-lg p-[30px] mt-2 text-justify">{licnost.tekst}</p>

                {role === "admin" && (
                    <div className="flex gap-4 justify-center mt-4">
                        <button onClick={handleDelete} className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110 cursor-pointer">Obriši</button>
                        <button onClick={handleUpdate} className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110 cursor-pointer">Ažuriraj</button>
                    </div>
                )}
            </div>
        </div>
    );
}
