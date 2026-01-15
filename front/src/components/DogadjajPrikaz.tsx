import type { Dogadjaj } from "../types";
import { useNavigate } from 'react-router-dom';
import { isRat, isBitka } from "../utils/typeChecks";
import { useAuth } from '../pages/AuthContext';
import axios from 'axios';

interface DogadjajPrikazProps {
  dogadjaj: Dogadjaj;
  variant?: "full" | "short";
}

export default function DogadjajPrikaz({ dogadjaj, variant = "short" }: DogadjajPrikazProps) {
    const { token, role } = useAuth();
    const navigate = useNavigate();

    console.log("Dogadjaj object:", dogadjaj);
    console.log("Dogadjaj.tip:", dogadjaj.tip);
    console.log("isBitka:", isBitka(dogadjaj));
    console.log("isRat:", isRat(dogadjaj));

    const handleNavigate = (tip: string, id: string) => {
        navigate(`/dogadjaj/${tip}/${id}`);
    };

    const handleDelete = async (e: React.MouseEvent) => {
        e.stopPropagation();

        if (!dogadjaj.id || !token) return;
        if (!confirm("Da li ste sigurni da želite da obrišete ovaj događaj?")) return;

        try {
            let endpoint = "";

            if (dogadjaj.tip === "Bitka") {
                endpoint = `http://localhost:5210/api/DeleteBitka/${dogadjaj.id}`;
            }
            else if (dogadjaj.tip === "Rat") {
                endpoint = `http://localhost:5210/api/DeleteRat/${dogadjaj.id}`;
            }
            else {
                endpoint = `http://localhost:5210/api/DeleteDogadjaj/${dogadjaj.id}`;
            }

            await axios.delete(endpoint, {
                headers: { Authorization: `Bearer ${token}` }
            });

            alert("Događaj obrisan");
            navigate("/dogadjaji");
        } catch (err) {
            console.error(err);
            alert("Greška prilikom brisanja događaja");
        }
    };

    const handleUpdate = (e: React.MouseEvent) => {
        e.stopPropagation();
        if (!dogadjaj.id) return;
        navigate(`/dogadjaj/edit/${dogadjaj.id}`);
    };

    return (
        <div
            key={dogadjaj.id}
            onClick={() => handleNavigate(dogadjaj.tip, dogadjaj.id)}
            className={`dogadjaj-div flex flex-col border-2 border-[#3f2b0a] bg-[#e6cda5]/70 p-[20px] text-[#3f2b0a] rounded-lg text-center ${
                variant === "full"
                    ? "absolute top-30 w-5/6 mx-[100px] mt-4"
                    : "w-[400px] items-center justify-center relative m-[20px] shadow-md overflow-hidden transition-transform hover:scale-110 cursor-pointer"
            }`}
        >
            <span className='dogadjaj-header text-xl font-bold mt-2'>
                {dogadjaj.ime}
            </span>

            <span className='dogadjaj-godina text-l font-bold mt-2'>
                {isRat(dogadjaj)
                    ? `${dogadjaj.godina?.god ?? ""}${dogadjaj.godina?.isPne ? " p. n. e." : ""} - ${dogadjaj.godinaDo?.god ?? ""}. ${dogadjaj.godinaDo?.isPne ? " p. n. e." : ""}`
                    : `${dogadjaj.godina?.god ?? ""}. ${dogadjaj.godina?.isPne ? " p. n. e." : ""}`
                }
            </span>

            {isBitka(dogadjaj) && variant === "full" && (
                <div>
                    {/** poseban prikaz bitke */}
                </div>
            )}

            {isRat(dogadjaj) && variant === "full" && (
                <div>
                    {/** poseban prikaz rata */}
                </div>
            )}

            {variant === "full" && (
                <>
                    <span className='text-lg p-[30px] mt-2 text-justify'>
                        {dogadjaj.tekst}
                    </span>

                    {role === "admin" && (
                        <div className="flex gap-4 justify-center mt-4">
                            <button
                                onClick={handleDelete}
                                className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110 cursor-pointer"
                            >
                                Obriši
                            </button>

                            <button
                                onClick={handleUpdate}
                                className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110 cursor-pointer"
                            >
                                Ažuriraj
                            </button>
                        </div>
                    )}
                </>
            )}
        </div>
    );
}
