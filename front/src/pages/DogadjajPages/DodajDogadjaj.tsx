import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../AuthContext";

interface Zemlja {
  naziv: string;
}

export default function DodajDogadjaj() {
  const [ime, setIme] = useState("");
  const [tip, setTip] = useState("Bitka");
  const [lokacija, setLokacija] = useState("");
  const [zemlje, setZemlje] = useState<Zemlja[]>([]);
  const [godina, setGodina] = useState<number | "">("");
  const [isPNE, setIsPNE] = useState(false);
  const [tekst, setTekst] = useState("");

  const navigate = useNavigate();
  const { token } = useAuth();

  useEffect(() => {
    async function fetchZemlje() {
      try {
        const res = await axios.get<Zemlja[]>("http://localhost:5210/api/GetAllZemlje");
        setZemlje(res.data);
      } catch (err) {
        console.error("Greška pri učitavanju zemalja:", err);
      }
    }
    fetchZemlje();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!ime.trim()) {
      alert("Ime događaja je obavezno!");
      return;
    }

    const payload = {
      Ime: ime,
      Tip: tip,
      Lokacija: lokacija || null,
      Godina: godina === "" ? null : { God: godina, IsPNE: isPNE },
      Tekst: tekst || null
    };

    try {
      const response = await axios.post(
        "http://localhost:5210/api/CreateDogadjaj",
        payload,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json"
          }
        }
      );
      alert(response.data);
      navigate("/dogadjaji");
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        console.error("Axios error:", err.response?.data);
        alert(`Greška: ${err.response?.data || err.message}`);
      } else {
        const error = err as Error;
        alert("Greška: " + error.message);
      }
    }
  };

  return (
    <div className="dodaj-dogadjaj my-[180px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md">
        <h1 className="text-2xl font-bold mb-[15px]">Dodaj događaj</h1>

        <form className="w-full flex flex-col gap-4" onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="Ime događaja"
            value={ime}
            onChange={(e) => setIme(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
            required
          />

          <select
            value={tip}
            onChange={(e) => setTip(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          >
            <option>Bitka</option>
            <option>Rat</option>
            <option>Ustanak</option>
            <option>Sporazum</option>
            <option>Savez</option>
            <option>Dokument</option>
          </select>

          <select
            value={lokacija}
            onChange={(e) => setLokacija(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          >
            <option value="">Izaberi zemlju</option>
            {zemlje.map((z) => (
              <option key={z.naziv} value={z.naziv}>{z.naziv}</option>
            ))}
            <option value="">Druga lokacija...</option>
          </select>

          <input
            type="text"
            placeholder="Unesi drugu lokaciju ako nije na listi"
            value={lokacija}
            onChange={(e) => setLokacija(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />

          <div className="flex gap-4 items-center">
            <input
              type="number"
              placeholder="Godina"
              value={godina}
              onChange={(e) => setGodina(Number(e.target.value))}
              className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"
            />
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={isPNE}
                onChange={(e) => setIsPNE(e.target.checked)}
              />
              p. n. e.
            </label>
          </div>

          <textarea
            placeholder="Tekst događaja"
            value={tekst}
            onChange={(e) => setTekst(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none h-32 resize-none"
          />

          <button
            type="submit"
            className="bg-[#3f2b0a] text-[#e6cda5] p-[6px] mb-[15px] rounded-[3px] hover:bg-[#2b1d07] transition font-bold"
          >
            Kreiraj događaj
          </button>
        </form>
      </div>
    </div>
  );
}
