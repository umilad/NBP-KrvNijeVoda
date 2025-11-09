import axios from 'axios';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import type { Dogadjaj } from "../types";

export default function Dogadjaj() { 
    const [dogadjaj, setDogadjaj] = useState<Dogadjaj | null>(null);
    const { id } = useParams();

    useEffect(() => {
        async function GetDogadjaj(){
            try {
                const response = await axios.get<Dogadjaj>(`http://localhost:5210/api/GetDogadjaj/${id}`);
                return response.data;
            }
            catch(error) {
                console.error("Error fetching dogadjaj:", error);
                return null;
            }            
        }
        async function loadDogadjaj(){
            const data = await GetDogadjaj();
            setDogadjaj(data);
        }
        loadDogadjaj();
    }, [id]);


    return (
        <div className="dogadjaj-container flex flex-col items-center justify-center text-white"> 
            {/* Podaci */}
            <div className="absolute top-30 w-5/6 mx-[100px] p-[20px] block border-2 border-[#3f2b0a] bg-[#e6cda5] p-4 rounded-lg text-center text-[#3f2b0a] mt-4">
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
                    {/*{("godinaDo" in dogadjaj && dogadjaj.godinaDo) ? ` - ${dogadjaj?.godinaDo.god}. ${dogadjaj?.godinaDo.isPne ? "p.n.e." : ""}` : dogadjaj?.godina ? `${dogadjaj?.godina.isPne ? "p. n. e." : ""}` : ""}*/}
                </span>
                <div>
                    <p className="text-lg p-[30px] mt-2 text-justify">{dogadjaj?.tekst}</p>
                </div>
                
            </div>

        </div>
        
    );
}