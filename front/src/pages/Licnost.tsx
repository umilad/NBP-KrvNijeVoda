import axios from 'axios';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom'; //useNavigate
import type { Licnost } from "../types";



export default function Licnost() { 
    const [licnost, setLicnost] = useState<Licnost | null>(null);
    const { id } = useParams();
    //const navigate = useNavigate();

    useEffect(() => {
        async function GetLicnost(){
            try {
                const response = await axios.get<Licnost>(`http://localhost:5210/api/GetLicnost/${id}`);
                return response.data;
            }
            catch(error) {
                console.error("Error fetching licnost:", error);
                return null;
            }            
        }
        async function loadLicnost(){
            const data = await GetLicnost();
            setLicnost(data);
        }
        loadLicnost();
    }, [id]);

    //const handleBack = () => navigate("/licnosti");

    return (
        <div className="licnosti-container flex flex-col items-center justify-center text-white my-[100px]"> 
            {/* Slika */}
            <div className="relative w-[300px] h-[355px] m-auto">
                {/*ram*/}
                <img
                    src="/src/images/picture-frame.png"
                    alt="Frame"
                    className="absolute top-0 left-0 w-full h-full z-10 pointer-events-none"
                />
                {/*slika u ramu*/}
                <div className="absolute inset-0 top-0 flex items-center justify-center z-0">
                    <img
                        src={`/src/images/${licnost?.slika}`}
                        alt="Historical Figure"
                        className="w-[190px] h-[235px] object-cover"
                    />
                </div>
            </div>

            {/* Podaci */}
            <div className="absolute top-100 mx-[100px] p-[20px] block border-2 border-[#3f2b0a] bg-[#e6cda5] p-4 rounded-lg text-center text-[#3f2b0a] mt-4">
                <p className="text-2xl font-bold mt-2">{licnost?.titula} {licnost?.ime} {licnost?.prezime}</p>
                <p className="text-xl font-bold mt-2">
                    {licnost?.godinaRodjenja ? `${licnost.godinaRodjenja}` : ""}
                    {licnost?.godinaSmrti ? ` - ${licnost.godinaSmrti}. ${licnost.godinaSmrtiPNE ? "p.n.e." : ""}` : licnost?.godinaRodjenja ? `${licnost.godinaRodjenjaPNE ? "p. n. e." : ""}` : ""}
                        
                </p>
                <div>
                    <p className="text-lg p-[30px] mt-2 text-justify">
                        {licnost?.tekst}
                    </p>
                </div>
                
            </div>

        </div>
        
    );
}