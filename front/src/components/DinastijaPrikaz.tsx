import type { Dinastija } from "../types";
import { useNavigate } from 'react-router-dom';


interface DinastijaPrikazProps {
  dinastija: Dinastija;
}


export default function DinastijaPrikaz({ dinastija }: DinastijaPrikazProps){
    const navigate = useNavigate();
    const handleNavigate = (id: string) => navigate(`/dinastija/${id}`);

    return (
        <div 
            key={dinastija.id} 
            onClick={() => handleNavigate(dinastija.id)}
            className="dinastija-div w-[350px] h-[400px] flex flex-col items-center justify-center relative border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] m-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md overflow-hidden transition-transform hover:scale-110 cursor-pointer"
        >
            <span className='dogadjaj-header text-xl font-bold mt-2'>{dinastija.naziv}</span>
            <span className='dogadjaj-godina text-l font-bold mt-2'>
                {dinastija.pocetakVladavineGod} - {dinastija.krajVladavineGod} 
                {dinastija.krajVladavinePNE ? " p. n. e." : ""}
            </span>

            <div className="relative w-[300px] h-[355px] m-auto">
                <div className="absolute inset-0 top-0 flex items-center justify-center z-0">
                    <img
                        src={`/src/images/${dinastija?.slika}`}
                        alt={dinastija.naziv}
                        className="w-[190px] h-[235px] object-cover"
                    />
                </div>
            </div>
        </div>
    );
}