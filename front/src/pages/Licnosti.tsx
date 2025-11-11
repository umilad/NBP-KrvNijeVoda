import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Licnost, Vladar } from "../types";
import { useSearch } from "../components/SearchContext";
import { useAuth } from "../pages/AuthContext";
import LicnostPrikaz from "../components/LicnostPrikaz";

export default function Licnosti() {
    const [licnosti, setLicnosti] = useState<(Licnost | Vladar & { isVladar: boolean })[]>([]);
    const navigate = useNavigate();
    const { query } = useSearch();
    const { role } = useAuth();

    useEffect(() => {
        async function GetAllLicnosti() {
            try {
                const res = await axios.get<Licnost[]>("http://localhost:5210/api/GetAllLicnosti");
                return res.data;
            } catch (err) {
                console.error(err);
                return [];
            }
        }

        async function GetAllVladare() {
            try {
                const res = await axios.get<Vladar[]>("http://localhost:5210/api/GetAllVladare");
                return res.data;
            } catch (err) {
                console.error(err);
                return [];
            }
        }

        async function loadAll() {
            const [vladariData, licnostiData] = await Promise.all([GetAllVladare(), GetAllLicnosti()]);
            const vladariWithFlag = (vladariData ?? []).map(v => ({ ...v, isVladar: true }));
            const licnostiWithFlag = (licnostiData ?? []).map(l => ({ ...l, isVladar: false }));
            setLicnosti([...vladariWithFlag, ...licnostiWithFlag]);
        }

        loadAll();
    }, []);

    const filteredLicnosti = licnosti.filter(l =>
        `${l.ime} ${l.prezime} ${l.titula}`.toLowerCase().includes(query.toLowerCase())
    );

    return (
        <div className="licnosti my-[100px]">
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
                    <div
                        key={licnost.id}
                        onClick={() => navigate(`/licnost/${licnost.id}`, { state: { isVladar: licnost.isVladar } })}
                        className="cursor-pointer"
                    >
                        <LicnostPrikaz licnost={licnost} />
                    </div>
                ))}
            </div>
        </div>
    );
}
