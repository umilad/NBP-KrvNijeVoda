import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Dogadjaj} from "../types";

export default function Dogadjaji() {

    const [dogadjaji, setDogadjaji] = useState<Dogadjaj[]>([]);
    const navigate = useNavigate();

    async function GetAllDogadjaji(){
        try {
            const response = await axios.get<Dogadjaj[]>("http://localhost:5210/api/GetAllDogadjaji");
            //console.log("API data:", response.data);
            return response.data;
        }
        catch(error) {
            console.error("Error fetching dogadjaji:", error);
            return [];
        }        
    }

    useEffect(() => {
        async function loadAllDogadjaji(){
            const data = await GetAllDogadjaji();
            setDogadjaji(data);
        }
        loadAllDogadjaji();
    }, []);

    const handleNavigate = (id: string) => navigate(`/dogadjaj/${id}`);

    return (
        <div className="dogadjaji my-[100px]">
            <div className='dogadjaji-grid grid grid-cols-[repeat(auto-fit,minmax(400px,1fr))] gap-6 justify-items-center'>
                {dogadjaji.map((dogadjaj) => (
                    <div key={dogadjaj.id} onClick={() => handleNavigate(dogadjaj.id)}
                    className="dogadjaj-div w-[400px] flex flex-col items-center justify-center relative border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] m-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md overflow-hidden transition-transform hover:scale-110">
                        <span className='dogadjaj-header text-xl font-bold mt-2'>{dogadjaj.ime}</span>
                        <span className='dogadjaj-godina text-l font-bold mt-2'>
                            {dogadjaj.godina ? `${dogadjaj.godina.god}` : ""}
                            {dogadjaj
                            ? (("godinaDo" in dogadjaj && dogadjaj.godinaDo)
                                ? ` - ${dogadjaj.godinaDo}. ${dogadjaj.godinaDo ? "p.n.e." : "" }`
                                : dogadjaj.godina
                                    ? `${dogadjaj.godina ? "p. n. e." : ""}`
                                    : "")
                            : ""}
                        </span>
                        <span className='text-justify'>
                            {dogadjaj.tekst}
                        </span>
                    </div>
                ))}
            </div>
        </div>
    );
}