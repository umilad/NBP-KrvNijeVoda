import type { Dinastija } from "../types";
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../pages/AuthContext';
import axios from 'axios';

interface DinastijaPrikazProps {
  dinastija: Dinastija;
  showAdminButtons?: boolean; 
}

export default function DinastijaPrikaz({
  dinastija,
  showAdminButtons = true, 
}: DinastijaPrikazProps){
    const { token, role } = useAuth();
    const navigate = useNavigate();

    const handleNavigate = (id: string) => navigate(`/dinastija/${id}`);

    const handleDelete = async (e: React.MouseEvent) => {
        e.stopPropagation();

        if (!dinastija.id || !token) return;
        if (!confirm("Da li ste sigurni da želite da obrišete ovu dinastiju?")) return;

        try {
            const endpoint = `http://localhost:5210/api/DeleteDinastija/${dinastija.id}`;
            await axios.delete(endpoint, {
                headers: { Authorization: `Bearer ${token}` }
            });

            alert("Dinastija obrisana");
            navigate("/dinastije");
        } catch (err) {
            console.error(err);
            alert("Greška prilikom brisanja dinastije");
        }
    };

    const handleUpdate = (e: React.MouseEvent) => {
        e.stopPropagation();
        if (!dinastija.id) return;
        navigate(`/dinastija/edit/${dinastija.id}`);
    };

    return (
        <div 
            key={dinastija.id} 
            onClick={() => handleNavigate(dinastija.id)}
            className="dinastija-div w-[350px] h-[400px] flex flex-col items-center justify-center relative 
                       border-2 border-[#3f2b0a] bg-[#e6cda5]/50 p-[20px] m-[20px] rounded-lg 
                       text-center text-[#3f2b0a] shadow-md overflow-visible box-border
                       transition-transform duration-300 hover:scale-105 cursor-pointer will-change-transform"
        >
            <span className='dogadjaj-header text-xl font-bold mt-2'>{dinastija.naziv}</span>
            {dinastija.pocetakVladavineGod && dinastija.krajVladavineGod && (
            <span className='dogadjaj-godina text-l font-bold mt-2 mb-[5px]'>
                {dinastija.pocetakVladavineGod} - {dinastija.krajVladavineGod}.
                {dinastija.krajVladavinePNE ? " p. n. e." : ""}
            </span>
            )}


            <div className="relative w-[300px] h-[355px] m-auto">
                <div className="absolute inset-0 top-0 flex items-center justify-center z-0">
                    <img
                        src={`/images/dinastije/${dinastija?.slika}`}
                        alt={dinastija.naziv}
                        className="max-w-full max-h-full object-contain"
                    />
                </div>
            </div>

            {}
            {showAdminButtons && role === "admin" && (
                <div className="flex gap-4 justify-center mt-[5px]">
                    <button
                        onClick={handleDelete}
                        className="px-[12px] py-[6px] m-[5px] border border-[#e6cda5] 
                                   bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] 
                                   transition-all duration-300 transform hover:scale-105 cursor-pointer"
                    >
                        Obriši
                    </button>

                    <button
                        onClick={handleUpdate}
                        className="px-[12px] py-[6px] m-[5px] border border-[#e6cda5] 
                                   bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] 
                                   transition-all duration-300 transform hover:scale-105 cursor-pointer"
                    >
                        Ažuriraj
                    </button>
                </div>
            )}
        </div>
    );
}
