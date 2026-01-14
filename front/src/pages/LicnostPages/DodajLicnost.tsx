import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../AuthContext";

interface Zemlja {
  naziv: string;
}

interface Dinastija {
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

  const [dodajVladara, setDodajVladara] = useState(false);
  const [pocetakVladavineGod, setPocetakVladavineGod] = useState<number | "">("");
  const [pocetakVladavinePNE, setPocetakVladavinePNE] = useState(false);
  const [krajVladavineGod, setKrajVladavineGod] = useState<number | "">("");
  const [krajVladavinePNE, setKrajVladavinePNE] = useState(false);
  const [dinastije, setDinastije] = useState<Dinastija[]>([]);
  const [dinastija, setDinastija] = useState<Dinastija | null>(null);

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
    async function fetchDinastije() {
      try {
        const res = await axios.get<Dinastija[]>("http://localhost:5210/api/GetAllDinastije");
        setDinastije(res.data);
      } catch (err) {
        console.error("Greška pri učitavanju dinastija:", err);
      }
    }
    fetchZemlje();
    fetchDinastije();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!ime.trim() || !prezime.trim()) {
      alert("Ime i prezime su obavezni!");
      return;
    }

    // === FormData za file upload ===
    const formData = new FormData();
    formData.append("Titula", titula);
    formData.append("Ime", ime);
    formData.append("Prezime", prezime);
    formData.append("Pol", pol);
    formData.append("GodinaRodjenja", (godinaRodjenja || 0).toString());
    formData.append("GodinaRodjenjaPNE", godinaRodjenjaPNE.toString());
    formData.append("GodinaSmrti", (godinaSmrti || 0).toString());
    formData.append("GodinaSmrtiPNE", godinaSmrtiPNE.toString());
    formData.append("MestoRodjenja", mestoRodjenja || "string");
    formData.append("Tekst", tekst);

    if (slika) {
      formData.append("slika", slika);
    }

    if (dodajVladara) {
      formData.append("PocetakVladavineGod", (pocetakVladavineGod || 0).toString());
      formData.append("PocetakVladavinePNE", pocetakVladavinePNE.toString());
      formData.append("KrajVladavineGod", (krajVladavineGod || 0).toString());
      formData.append("KrajVladavinePNE", krajVladavinePNE.toString());
      if (dinastija) formData.append("Dinastija", dinastija.naziv);
    }

    try {
      const endpoint = dodajVladara
        ? "http://localhost:5210/api/CreateVladar"
        : "http://localhost:5210/api/CreateLicnost";

      const response = await axios.post(endpoint, formData, {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "multipart/form-data"
        }
      });

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
          <label className="flex items-center gap-2">
            <input type="checkbox" checked={dodajVladara} onChange={(e) => setDodajVladara(e.target.checked)} />
            Dodaj kao Vladara
          </label>

          <input type="text" placeholder="Titula" value={titula} onChange={(e) => setTitula(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"/>
          <input type="text" placeholder="Ime" value={ime} onChange={(e) => setIme(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none" required/>
          <input type="text" placeholder="Prezime" value={prezime} onChange={(e) => setPrezime(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none" required/>

          <select value={pol} onChange={(e) => setPol(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none">
            <option value="M">Muški</option>
            <option value="Ž">Ženski</option>
          </select>

          <div className="flex gap-4 items-center">
            <input type="number" placeholder="Godina rođenja" value={godinaRodjenja} onChange={(e) => setGodinaRodjenja(Number(e.target.value))} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"/>
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={godinaRodjenjaPNE} onChange={(e) => setGodinaRodjenjaPNE(e.target.checked)}/> p. n. e.
            </label>
          </div>

          <div className="flex gap-4 items-center">
            <input type="number" placeholder="Godina smrti" value={godinaSmrti} onChange={(e) => setGodinaSmrti(Number(e.target.value))} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"/>
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={godinaSmrtiPNE} onChange={(e) => setGodinaSmrtiPNE(e.target.checked)}/> p. n. e.
            </label>
          </div>

          <select value={mestoRodjenja} onChange={(e) => setMestoRodjenja(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none">
            <option value="">Izaberi zemlju rođenja</option>
            {zemlje.map((z) => <option key={z.naziv} value={z.naziv}>{z.naziv}</option>)}
          </select>

          <input type="text" placeholder="Drugo mesto rođenja" value={mestoRodjenja} onChange={(e) => setMestoRodjenja(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"/>
          <textarea placeholder="Tekst" value={tekst} onChange={(e) => setTekst(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none resize-none"/>

          <input type="file" accept="image/*" onChange={(e) => setSlika(e.target.files ? e.target.files[0] : null)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"/>

          {dodajVladara && (
            <>
              <div className="flex gap-4 items-center">
                <input type="number" placeholder="Početak vladavine" value={pocetakVladavineGod} onChange={(e) => setPocetakVladavineGod(Number(e.target.value))} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"/>
                <label className="flex items-center gap-2">
                  <input type="checkbox" checked={pocetakVladavinePNE} onChange={(e) => setPocetakVladavinePNE(e.target.checked)}/> p. n. e.
                </label>
              </div>

              <div className="flex gap-4 items-center">
                <input type="number" placeholder="Kraj vladavine" value={krajVladavineGod} onChange={(e) => setKrajVladavineGod(Number(e.target.value))} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"/>
                <label className="flex items-center gap-2">
                  <input type="checkbox" checked={krajVladavinePNE} onChange={(e) => setKrajVladavinePNE(e.target.checked)}/> p. n. e.
                </label>
              </div>

              <select value={dinastija?.naziv || ""} onChange={(e) => setDinastija({ naziv: e.target.value })} className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none">
                <option value="">Izaberi dinastiju</option>
                {dinastije.map((d) => <option key={d.naziv} value={d.naziv}>{d.naziv}</option>)}
              </select>
            </>
          )}

          <button type="submit" className="bg-[#3f2b0a] text-[#e6cda5] px-8 py-3 rounded-lg shadow-md hover:bg-[#2b1d07] transition font-bold">
            Kreiraj {dodajVladara ? "Vladara" : "Lićnost"}
          </button>
        </form>

        {slika && (
          <img src={URL.createObjectURL(slika)} alt="Preview" className="mt-4 w-32 h-32 object-cover border border-[#3f2b0a] rounded"/>
        )}
      </div>
    </div>
  );
}
