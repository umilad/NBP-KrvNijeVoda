import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../AuthContext";

interface Zemlja { naziv: string; }
interface Dinastija { naziv: string; }

export default function AzurirajLicnost() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { token } = useAuth();

  type LocationState = { isVladar?: boolean };
  const location = useLocation();
  const state = location.state as LocationState;

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
  const [slikaPreview, setSlikaPreview] = useState<string | null>(null);

  const [dodajVladara, setDodajVladara] = useState(location.state?.isVladar ?? false);
  const [pocetakVladavineGod, setPocetakVladavineGod] = useState<number | "">("");
  const [pocetakVladavinePNE, setPocetakVladavinePNE] = useState(false);
  const [krajVladavineGod, setKrajVladavineGod] = useState<number | "">("");
  const [krajVladavinePNE, setKrajVladavinePNE] = useState(false);
  const [dinastije, setDinastije] = useState<Dinastija[]>([]);
  const [dinastija, setDinastija] = useState<Dinastija | null>(null);
  const [teritorija, setTeritorija] = useState("");

  useEffect(() => {
    const fetchLicnost = async () => {
      try {
        const endpoint = dodajVladara
          ? `http://localhost:5210/api/GetVladar/${id}`
          : `http://localhost:5210/api/GetLicnost/${id}`;
        const res = await axios.get(endpoint, { headers: { Authorization: `Bearer ${token}` } });
        const data = res.data;

        setTitula(data.titula || "");
        setIme(data.ime || "");
        setPrezime(data.prezime || "");
        setPol(data.pol || "M");
        setMestoRodjenja(data.mestoRodjenja || "");
        setGodinaRodjenja(data.godinaRodjenja ?? "");
        setGodinaRodjenjaPNE(data.godinaRodjenjaPNE ?? false);
        setGodinaSmrti(data.godinaSmrti ?? "");
        setGodinaSmrtiPNE(data.godinaSmrtiPNE ?? false);
        setTekst(data.tekst || "");
       setSlikaPreview(
  data.slika
    ? `/images/licnosti/${data.slika}`
    : data.pol === "M"
      ? "/images/licnosti/placeholder_muski.png"
      : "/images/licnosti/placeholder_zenski.png"
);

        const isVladarFromData = data.pocetakVladavineGod !== undefined || data.krajVladavineGod !== undefined;
        const isVladarFinal = location.state?.isVladar ?? isVladarFromData;
        setDodajVladara(isVladarFinal);

        if (isVladarFinal) {
          setPocetakVladavineGod(data.pocetakVladavineGod ?? "");
          setPocetakVladavinePNE(data.pocetakVladavinePNE ?? false);
          setKrajVladavineGod(data.krajVladavineGod ?? "");
          setKrajVladavinePNE(data.krajVladavinePNE ?? false);
          setDinastija(data.dinastija || null);
          setTeritorija(data.teritorija || "");
        }
      } catch (err) {
        console.error(err);
        alert("Greška pri učitavanju podataka!");
      }
    };

    const fetchZemlje = async () => {
      try {
        const res = await axios.get<Zemlja[]>("http://localhost:5210/api/GetAllZemlje");
        setZemlje(res.data);
      } catch (err) { console.error(err); }
    };

    const fetchDinastije = async () => {
      try {
        const res = await axios.get<Dinastija[]>("http://localhost:5210/api/GetAllDinastije");
        setDinastije(res.data);
      } catch (err) { console.error(err); }
    };

    fetchLicnost();
    fetchZemlje();
    fetchDinastije();
  }, [id, token, location.state, dodajVladara]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setSlika(file);
    if (file) setSlikaPreview(URL.createObjectURL(file));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const formData = new FormData();
    formData.append("Titula", titula || "");
    formData.append("Ime", ime || "");
    formData.append("Prezime", prezime || "");
    formData.append("Pol", pol || "M");
    formData.append("GodinaRodjenja", godinaRodjenja?.toString() || "0");
    formData.append("GodinaRodjenjaPNE", godinaRodjenjaPNE.toString());
    formData.append("GodinaSmrti", godinaSmrti?.toString() || "0");
    formData.append("GodinaSmrtiPNE", godinaSmrtiPNE.toString());
    formData.append("MestoRodjenja", mestoRodjenja || "string");
    formData.append("Tekst", tekst || "");

    if (slika) {
  formData.append("Slika", slika); 
} else if (slikaPreview) {
  
  const imeFajla = slikaPreview.split("/").pop(); 
  formData.append("Slika", imeFajla || "");
}

    if (dodajVladara) {
      formData.append("PocetakVladavineGod", (pocetakVladavineGod || 0).toString());
      formData.append("PocetakVladavinePNE", pocetakVladavinePNE.toString());
      formData.append("KrajVladavineGod", (krajVladavineGod || 0).toString());
      formData.append("KrajVladavinePNE", krajVladavinePNE.toString());
      if (dinastija) formData.append("Dinastija", dinastija.naziv);
      formData.append("Teritorija", teritorija || "");
    }

    try {
      const endpoint = dodajVladara
        ? `http://localhost:5210/api/UpdateVladar/${id}`
        : `http://localhost:5210/api/UpdateLicnost/${id}`;

      await axios.put(endpoint, formData, {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "multipart/form-data"
        }
      });

      alert("Uspešno ažurirano!");
      navigate("/licnosti");
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) console.error(err.response?.data);
      alert("Greška pri ažuriranju!");
    }
  };

  return (
    <div className="dodaj-dogadjaj my-[180px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md">
        <h1 className="text-2xl font-bold mb-[15px]">
          Ažuriraj {dodajVladara ? "Vladara" : "Ličnost"}
        </h1>

        <form className="w-full flex flex-col gap-4" onSubmit={handleSubmit}>
          <input type="text" placeholder="Titula" value={titula} onChange={e => setTitula(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a]" />
          <input type="text" placeholder="Ime" value={ime} onChange={e => setIme(e.target.value)} required className="p-[6px] rounded-[3px] border border-[#3f2b0a]" />
          <input type="text" placeholder="Prezime" value={prezime} onChange={e => setPrezime(e.target.value)} required className="p-[6px] rounded-[3px] border border-[#3f2b0a]" />
          <select value={pol} onChange={e => setPol(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a]">
            <option value="M">Muški</option>
            <option value="Ž">Ženski</option>
          </select>

          <div className="flex gap-4 items-center">
            <input type="number" placeholder="Godina rođenja" value={godinaRodjenja} onChange={e => setGodinaRodjenja(Number(e.target.value))} className="p-[6px] rounded-[3px] border border-[#3f2b0a] flex-1" />
            <label className="flex items-center gap-2"><input type="checkbox" checked={godinaRodjenjaPNE} onChange={e => setGodinaRodjenjaPNE(e.target.checked)} /> p. n. e.</label>
          </div>

          <div className="flex gap-4 items-center">
            <input type="number" placeholder="Godina smrti" value={godinaSmrti} onChange={e => setGodinaSmrti(Number(e.target.value))} className="p-[6px] rounded-[3px] border border-[#3f2b0a] flex-1" />
            <label className="flex items-center gap-2"><input type="checkbox" checked={godinaSmrtiPNE} onChange={e => setGodinaSmrtiPNE(e.target.checked)} /> p. n. e.</label>
          </div>

          <select value={mestoRodjenja} onChange={e => setMestoRodjenja(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a]">
            <option value="">-- Odaberi zemlju rođenja --</option>
            {zemlje.map(z => <option key={z.naziv} value={z.naziv}>{z.naziv}</option>)}
          </select>

          <textarea placeholder="Tekst" value={tekst} onChange={e => setTekst(e.target.value)} className="p-[6px] rounded-[3px] border border-[#3f2b0a] h-32 resize-none" />

          <input type="file" accept="image/*" onChange={handleFileChange} />
          {slikaPreview && <img src={slikaPreview} alt="Preview" className="h-[150px] mt-2 mx-auto rounded-lg shadow-md" />}

          {dodajVladara && (
            <>
              <h2 className="text-xl font-semibold mt-4 border-t border-[#3f2b0a] pt-4">Podaci o vladavini</h2>
              <div className="flex gap-4 items-center">
                <input type="number" placeholder="Početak vladavine" value={pocetakVladavineGod} onChange={e => setPocetakVladavineGod(Number(e.target.value))} className="p-[6px] rounded-[3px] border border-[#3f2b0a] flex-1" />
                <label className="flex items-center gap-2"><input type="checkbox" checked={pocetakVladavinePNE} onChange={e => setPocetakVladavinePNE(e.target.checked)} /> p. n. e.</label>
              </div>
              <div className="flex gap-4 items-center">
                <input type="number" placeholder="Kraj vladavine" value={krajVladavineGod} onChange={e => setKrajVladavineGod(Number(e.target.value))} className="p-[6px] rounded-[3px] border border-[#3f2b0a] flex-1" />
                <label className="flex items-center gap-2"><input type="checkbox" checked={krajVladavinePNE} onChange={e => setKrajVladavinePNE(e.target.checked)} /> p. n. e.</label>
              </div>
              <select value={dinastija?.naziv || ""} onChange={e => setDinastija(dinastije.find(d => d.naziv === e.target.value) || null)} className="p-[6px] rounded-[3px] border border-[#3f2b0a]">
                <option value="">-- Odaberi dinastiju --</option>
                {dinastije.map(d => <option key={d.naziv} value={d.naziv}>{d.naziv}</option>)}
              </select>
              
            </>
          )}

          <button type="submit" className="bg-[#3f2b0a] text-[#e6cda5] p-[6px] rounded-[3px] hover:bg-[#2b1d07] transition font-bold">Sačuvaj promene</button>
        </form>
      </div>
    </div>
  );
}
