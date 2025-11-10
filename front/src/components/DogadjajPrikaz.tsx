import type { Dogadjaj } from "../types";
import { useNavigate } from 'react-router-dom';

interface DogadjajPrikazProps {
  dogadjaj: Dogadjaj;
}

export default function DogadjajPrikaz({ dogadjaj }: DogadjajPrikazProps) {
    const navigate = useNavigate();
    const handleNavigate = (id: string) => navigate(`/dogadjaj/${id}`);

    return (
        <div key={dogadjaj.id} onClick={() => handleNavigate(dogadjaj.id)}
            className="dogadjaj-div w-[400px] flex flex-col items-center justify-center relative border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] m-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md overflow-hidden transition-transform hover:scale-110 cursor-pointer">
            
            <span className='dogadjaj-header text-xl font-bold mt-2'>{dogadjaj.ime}</span>
            <span className='dogadjaj-godina text-l font-bold mt-2'>
                {dogadjaj.godina ? `${dogadjaj.godina.god}` : ""}
                {dogadjaj
                    ? (("godinaDo" in dogadjaj && dogadjaj.godinaDo)
                        ? ` - ${dogadjaj.godinaDo}. ${dogadjaj.godinaDo ? " p.n.e." : ""}`
                        : dogadjaj.godina
                            ? `${dogadjaj.godina ? " p. n. e." : ""}`
                            : "")
                    : ""}
            </span>
            <span className='text-justify'>
                {dogadjaj.tekst}
            </span>
        </div>
    );

}