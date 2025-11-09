import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Licnost} from "../types";

export default function Licnosti() {

    const [licnosti, setLicnosti] = useState<Licnost[]>([]);
    const navigate = useNavigate();
    

    async function GetAllLicnosti(){
        try {
            const response = await axios.get<Licnost[]>("http://localhost:5210/api/GetAllLicnosti");
            //console.log("API data:", response.data);
            return response.data;
        }
        catch(error) {
            console.error("Error fetching licnosti:", error);
            return [];
        }
    }



    useEffect(() => {
        async function loadAllLicnosti(){
            const data = await GetAllLicnosti();
            setLicnosti(data);
        }
        loadAllLicnosti();
    }, []);

    //promeni da radi 
    const handleNavigate = (id: string) => navigate(`/licnost/${id}`);

    return (
        <div className="licnosti my-[100px]">
            <div className='licnosti-grid grid grid-cols-[repeat(auto-fit,minmax(300px,1fr))] gap-6 justify-items-center'>
                {licnosti.map((licnost) => (
                    <div key={licnost.id} onClick={() => handleNavigate(licnost.id)}
                        className="licnost-div w-[300px] flex flex-col items-center justify-center relative p-[20px] m-[20px] text-center text-[#3f2b0a] overflow-hidden transition-transform hover:scale-110">
                        {/* Slika */}
                        <div className="relative w-[259px] h-[300px] m-auto">
                            {/*ram*/}
                            <img
                                src="/src/images/picture-frame.png"
                                alt="Frame"
                                className="absolute top-0 left-0 w-full h-full z-10 pointer-events-none"
                            />
                            {/*slika u ramu*/}
                            <div className="absolute inset-0 flex items-center justify-center z-0">
                                <img
                                src={`/src/images/${licnost?.slika}`}
                                alt="Historical Figure"
                                className="max-w-[80%] max-h-[80%] object-contain"
                                />
                            </div>
                            {/*<div className="absolute inset-0 top-0 flex items-center justify-center z-0">
                                <img
                                    src={licnost?.slika ?? `/src/images/${licnost?.slika}`}
                                    alt="Historical Figure"
                                    className="w-[190px] h-[235px] object-cover"
                                />
                            </div>*/}
                        </div>

                        {/* Podaci */}
                        <p className="text-2xl font-bold mt-2">{licnost?.titula} {licnost?.ime} {licnost?.prezime}</p>
                        <p className="text-xl font-bold mt-2">
                            {licnost?.godinaRodjenja ? `${licnost.godinaRodjenja}` : ""}
                            {licnost?.godinaSmrti ? ` - ${licnost.godinaSmrti}. ${licnost.godinaSmrtiPNE ? "p.n.e." : ""}` : licnost?.godinaRodjenja ? `${licnost.godinaRodjenjaPNE ? "p. n. e." : ""}` : ""}
                                
                        </p>
                    </div>
                ))}
            </div>            
        </div>
    );
}