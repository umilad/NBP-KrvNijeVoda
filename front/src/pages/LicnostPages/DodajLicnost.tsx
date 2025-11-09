import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../AuthContext";

interface Zemlja {
  naziv: string;
}

export default function DodajLicnost() {
  const [titula, setTitula] = useState("");
  const [ime, setIme] = useState("");
  const [prezime, setPrezime] = useState("");
  const [pol, setPol] = useState("M");
  const [godinaRodjenja, setGodinaRodjenja] = useState<number | "">("");
  const [godinaRodjenjaPNE, setGodinaRodjenjaPNE] = useState(false);
  const [godinaSmrti, setGodinaSmrti] = useState<number | "">("");
  const [godinaSmrtiPNE, setGodinaSmrtiPNE] = useState(false);
  const [zemlje, setZemlje] = useState<Zemlja[]>([]);
  const [mestoRodjenja, setMestoRodjenja] = useState("");
  const [tekst, setTekst] = useState("");
  const [slika, setSlika] = useState<File | null>(null);

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

    if (!ime.trim() || !prezime.trim()) {
      alert("Ime i prezime su obavezni!");
      return;
    }

    // --- Ako je fajl odabran, konvertujemo u Base64 ---
    let slikaBase64: string | null = null;
    if (slika) {
      slikaBase64 = await new Promise<string>((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(slika);
        reader.onload = () => resolve(reader.result as string);
        reader.onerror = (err) => reject(err);
      });
    }

    const payload = {
      Titula: titula,
      Ime: ime,
      Prezime: prezime,
      Pol: pol,
      GodinaRodjenja: godinaRodjenja || 0,
      GodinaRodjenjaPNE: godinaRodjenjaPNE,
      GodinaSmrti: godinaSmrti || 0,
      GodinaSmrtiPNE: godinaSmrtiPNE,
      MestoRodjenja: mestoRodjenja || "string",
      Tekst: tekst,
      Slika: slikaBase64
    };

    try {
      const response = await axios.post(
        "http://localhost:5210/api/CreateLicnost",
        payload,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json"
          }
        }
      );
      alert(response.data);
      navigate("/licnosti");
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
    <div className="dodaj-licnost my-[100px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md">
        <h1 className="text-2xl font-bold mb-[15px]">Dodaj Ličnost</h1>

        <form className="w-full flex flex-col gap-4" onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="Titula"
            value={titula}
            onChange={(e) => setTitula(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />
          <input
            type="text"
            placeholder="Ime"
            value={ime}
            onChange={(e) => setIme(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
            required
          />
          <input
            type="text"
            placeholder="Prezime"
            value={prezime}
            onChange={(e) => setPrezime(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
            required
          />

          <select
            value={pol}
            onChange={(e) => setPol(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          >
            <option value="M">Muški</option>
            <option value="Ž">Ženski</option>
          </select>

          <div className="flex gap-4 items-center">
            <input
              type="number"
              placeholder="Godina rođenja"
              value={godinaRodjenja}
              onChange={(e) => setGodinaRodjenja(Number(e.target.value))}
              className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"
            />
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={godinaRodjenjaPNE}
                onChange={(e) => setGodinaRodjenjaPNE(e.target.checked)}
              />
              p. n. e.
            </label>
          </div>

          <div className="flex gap-4 items-center">
            <input
              type="number"
              placeholder="Godina smrti"
              value={godinaSmrti}
              onChange={(e) => setGodinaSmrti(Number(e.target.value))}
              className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"
            />
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={godinaSmrtiPNE}
                onChange={(e) => setGodinaSmrtiPNE(e.target.checked)}
              />
              p. n. e.
            </label>
          </div>

          <select
            value={mestoRodjenja}
            onChange={(e) => setMestoRodjenja(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          >
            <option value="">Izaberi zemlju rođenja</option>
            {zemlje.map((z) => (
              <option key={z.naziv} value={z.naziv}>{z.naziv}</option>
            ))}
            <option value="">Druga lokacija...</option>
          </select>

          <input
            type="text"
            placeholder="Unesi drugo mesto rođenja ako nije na listi"
            value={mestoRodjenja}
            onChange={(e) => setMestoRodjenja(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />

          <textarea
            placeholder="Tekst"
            value={tekst}
            onChange={(e) => setTekst(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none resize-none"
          />

          <input
            type="file"
            accept="image/*"
            onChange={(e) => setSlika(e.target.files ? e.target.files[0] : null)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />

          <button
            type="submit"
            className="bg-[#3f2b0a] text-[#e6cda5] px-8 py-3 rounded-lg shadow-md hover:bg-[#2b1d07] transition font-bold"
          >
            Kreiraj Ličnost
          </button>
        </form>

        {slika && (
          <img
            src={URL.createObjectURL(slika)}
            alt="Preview"
            className="mt-4 w-32 h-32 object-cover border border-[#3f2b0a] rounded"
          />
        )}
      </div>
    </div>
  );
}
