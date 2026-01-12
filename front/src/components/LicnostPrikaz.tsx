import type { Licnost } from "../types";
import { useNavigate } from 'react-router-dom';
//import { useAuth } from '../pages/AuthContext';
//import axios from 'axios';

interface LicnostPrikazProps {
  licnost: Licnost;
  //variant?: "full" | "short";
}

export default function LicnostPrikaz({ licnost }: LicnostPrikazProps){
    //const { token, role } = useAuth();  
    const navigate = useNavigate();
    const handleNavigate = (id: string) => navigate(`/licnost/${id}`);

    return(
        <div key={licnost.id} onClick={() => handleNavigate(licnost.id)}
            className="licnost-div w-[300px] flex flex-col items-center justify-center relative p-[20px] m-[20px] text-center text-[#3f2b0a] overflow-hidden transition-transform hover:scale-110 cursor-pointer">
            
            {/* Slika */}
            <div className="relative w-[259px] h-[300px] m-auto">
                <img
                    src="/src/images/picture-frame.png"
                    alt="Ram"
                    className="absolute top-0 left-0 w-full h-full z-10 pointer-events-none"
                />
                <div className="absolute inset-0 flex items-center justify-center z-0">
                    <img
                        src={`/src/images/${licnost?.slika}`}
                        alt={`${licnost.titula} ${licnost.ime} ${licnost.prezime}`}
                        className="max-w-[80%] max-h-[80%] object-contain"
                    />
                </div>
            </div>

            {/* Podaci */}
            <p className="text-2xl font-bold mt-2">{licnost?.titula} {licnost?.ime} {licnost?.prezime}</p>
            <p className="text-xl font-bold mt-2">
                {licnost.godinaRodjenja ? `${licnost.godinaRodjenja}${licnost.godinaRodjenjaPNE ? " p.n.e." : ""}` : ""}
                {licnost.godinaSmrti ? ` - ${licnost.godinaSmrti}. ${licnost.godinaSmrtiPNE ? " p.n.e." : ""}` : ""}
            </p>
        </div>
    );
}