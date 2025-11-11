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
  const location = useLocation<{ isVladar?: boolean }>();

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

  // Vladar
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
        const endpoint = location.state?.isVladar
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
        setSlikaPreview(data.slika || null);

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
      } catch (err) {
        console.error(err);
      }
    };

    const fetchDinastije = async () => {
      try {
        const res = await axios.get<Dinastija[]>("http://localhost:5210/api/GetAllDinastije");
        setDinastije(res.data);
      } catch (err) {
        console.error(err);
      }
    };

    fetchLicnost();
    fetchZemlje();
    fetchDinastije();
  }, [id, token, location.state]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setSlika(file);
    if (file) setSlikaPreview(URL.createObjectURL(file));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    let slikaBase64: string | null = slikaPreview;
    if (slika) {
      slikaBase64 = await new Promise<string>((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(slika);
        reader.onload = () => resolve(reader.result as string);
        reader.onerror = err => reject(err);
      });
    }

    const payload: any = {
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

    if (dodajVladara) {
      payload.PocetakVladavineGod = pocetakVladavineGod || 0;
      payload.PocetakVladavinePNE = pocetakVladavinePNE;
      payload.KrajVladavineGod = krajVladavineGod || 0;
      payload.KrajVladavinePNE = krajVladavinePNE;
      if (dinastija) payload.Dinastija = dinastija;
      payload.Teritorija = teritorija;
    }

    try {
      const endpoint = dodajVladara
        ? `http://localhost:5210/api/UpdateVladar/${id}`
        : `http://localhost:5210/api/UpdateLicnost/${id}`;

      await axios.put(endpoint, payload, { headers: { Authorization: `Bearer ${token}` } });
      alert("Uspešno ažurirano!");
      navigate("/licnosti");
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) console.error(err.response?.data);
      alert("Greška pri ažuriranju!");
    }
  };

  return (
    <div className="flex justify-center mt-[100px]">
      <div className="bg-[#e6cda5] border-2 border-[#3f2b0a] rounded-2xl shadow-lg w-[600px] p-8 text-[#3f2b0a]">
        <h1 className="text-3xl font-bold mb-6 text-center">
          Ažuriraj {dodajVladara ? "Vladara" : "Ličnost"}
        </h1>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <input
            className="p-2 rounded border border-[#3f2b0a]"
            type="text"
            placeholder="Titula"
            value={titula}
            onChange={(e) => setTitula(e.target.value)}
          />
          <input
            className="p-2 rounded border border-[#3f2b0a]"
            type="text"
            placeholder="Ime"
            value={ime}
            onChange={(e) => setIme(e.target.value)}
            required
          />
          <input
            className="p-2 rounded border border-[#3f2b0a]"
            type="text"
            placeholder="Prezime"
            value={prezime}
            onChange={(e) => setPrezime(e.target.value)}
            required
          />

          <select
            className="p-2 rounded border border-[#3f2b0a]"
            value={pol}
            onChange={(e) => setPol(e.target.value)}
          >
            <option value="M">Muški</option>
            <option value="Ž">Ženski</option>
          </select>

          <div className="flex gap-2 items-center">
            <input
              className="p-2 w-full rounded border border-[#3f2b0a]"
              type="number"
              placeholder="Godina rođenja"
              value={godinaRodjenja}
              onChange={(e) => setGodinaRodjenja(Number(e.target.value))}
            />
            <label className="flex items-center gap-1">
              <input
                type="checkbox"
                checked={godinaRodjenjaPNE}
                onChange={(e) => setGodinaRodjenjaPNE(e.target.checked)}
              />
              p.n.e.
            </label>
          </div>

          <div className="flex gap-2 items-center">
            <input
              className="p-2 w-full rounded border border-[#3f2b0a]"
              type="number"
              placeholder="Godina smrti"
              value={godinaSmrti}
              onChange={(e) => setGodinaSmrti(Number(e.target.value))}
            />
            <label className="flex items-center gap-1">
              <input
                type="checkbox"
                checked={godinaSmrtiPNE}
                onChange={(e) => setGodinaSmrtiPNE(e.target.checked)}
              />
              p.n.e.
            </label>
          </div>

          <select
            className="p-2 rounded border border-[#3f2b0a]"
            value={mestoRodjenja}
            onChange={(e) => setMestoRodjenja(e.target.value)}
          >
            <option value="">-- Odaberi zemlju rođenja --</option>
            {zemlje.map((z) => (
              <option key={z.naziv} value={z.naziv}>
                {z.naziv}
              </option>
            ))}
          </select>

          <textarea
            className="p-2 rounded border border-[#3f2b0a]"
            placeholder="Tekst"
            rows={4}
            value={tekst}
            onChange={(e) => setTekst(e.target.value)}
          />

          <input type="file" accept="image/*" onChange={handleFileChange} />
          {slikaPreview && (
            <img
              src={slikaPreview}
              alt="Preview"
              className="h-[150px] mt-2 mx-auto rounded-lg shadow-md"
            />
          )}

          {dodajVladara && (
            <>
              <h2 className="text-xl font-semibold mt-4 border-t border-[#3f2b0a] pt-4">
                Podaci o vladavini
              </h2>

              <div className="flex gap-2 items-center">
                <input
                  className="p-2 w-full rounded border border-[#3f2b0a]"
                  type="number"
                  placeholder="Početak vladavine"
                  value={pocetakVladavineGod}
                  onChange={(e) => setPocetakVladavineGod(Number(e.target.value))}
                />
                <label className="flex items-center gap-1">
                  <input
                    type="checkbox"
                    checked={pocetakVladavinePNE}
                    onChange={(e) => setPocetakVladavinePNE(e.target.checked)}
                  />
                  p.n.e.
                </label>
              </div>

              <div className="flex gap-2 items-center">
                <input
                  className="p-2 w-full rounded border border-[#3f2b0a]"
                  type="number"
                  placeholder="Kraj vladavine"
                  value={krajVladavineGod}
                  onChange={(e) => setKrajVladavineGod(Number(e.target.value))}
                />
                <label className="flex items-center gap-1">
                  <input
                    type="checkbox"
                    checked={krajVladavinePNE}
                    onChange={(e) => setKrajVladavinePNE(e.target.checked)}
                  />
                  p.n.e.
                </label>
              </div>

              <select
                className="p-2 rounded border border-[#3f2b0a]"
                value={dinastija?.naziv || ""}
                onChange={(e) => setDinastija(dinastije.find((d) => d.naziv === e.target.value) || null)}
              >
                <option value="">-- Odaberi dinastiju --</option>
                {dinastije.map((d) => (
                  <option key={d.naziv} value={d.naziv}>
                    {d.naziv}
                  </option>
                ))}
              </select>

              <input
                className="p-2 rounded border border-[#3f2b0a]"
                type="text"
                placeholder="Teritorija"
                value={teritorija}
                onChange={(e) => setTeritorija(e.target.value)}
              />
            </>
          )}

          <button
            type="submit"
            className="px-6 py-3 text-white bg-[#3f2b0a] hover:bg-[#2b1d07] rounded-lg text-lg font-bold mt-6"
          >
            Sačuvaj izmene
          </button>
        </form>
      </div>
    </div>
  );
}
