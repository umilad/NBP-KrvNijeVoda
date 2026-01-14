import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../AuthContext";
import type { Dinastija } from "../../types/dinastija";

export default function AzurirajDinastiju() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { token } = useAuth();

  const [dinastija, setDinastija] = useState<Dinastija | null>(null);
  const [naziv, setNaziv] = useState("");
  const [pocetakGod, setPocetakGod] = useState<number | "">("");
  const [pocetakPNE, setPocetakPNE] = useState(false);
  const [krajGod, setKrajGod] = useState<number | "">("");
  const [krajPNE, setKrajPNE] = useState(false);
  const [slikaFile, setSlikaFile] = useState<File | null>(null);
  const [slikaURL, setSlikaURL] = useState("");

  const placeholderSlika = "/images/placeholder_dinastija.png"; // promeni putanju ako treba

  useEffect(() => {
    async function loadDinastija() {
      if (!id) return;
      try {
        const res = await axios.get<Dinastija>(
          `http://localhost:5210/api/GetDinastija/${id}`
        );
        const d = res.data;
        setDinastija(d);
        setNaziv(d.naziv);
        setPocetakGod(d.pocetakVladavineGod || "");
        setPocetakPNE(Boolean(d.pocetakVladavinePNE));
        setKrajGod(d.krajVladavineGod || "");
        setKrajPNE(Boolean(d.krajVladavinePNE));
        setSlikaURL(d.slika || "");
      } catch (err) {
        console.error("Greška pri učitavanju dinastije", err);
      }
    }
    loadDinastija();
  }, [id]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setSlikaFile(e.target.files[0]);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !token) return;

    const formData = new FormData();
    formData.append("Naziv", naziv);
    formData.append("PocetakVladavineGod", (pocetakGod || 0).toString());
    formData.append("PocetakVladavinePNE", pocetakPNE.toString());
    formData.append("KrajVladavineGod", (krajGod || 0).toString());
    formData.append("KrajVladavinePNE", krajPNE.toString());
    if (slikaFile) formData.append("slika", slikaFile);

    try {
      await axios.put(
        `http://localhost:5210/api/UpdateDinastija/${id}`,
        formData,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "multipart/form-data",
          },
        }
      );
      alert("Dinastija uspešno ažurirana!");
      navigate(`/dinastija/${id}`);
    } catch (err: any) {
      console.error("Greška pri ažuriranju:", err);
      alert(err.response?.data || "Došlo je do greške!");
    }
  };

  return (
    <div className="dodaj-dogadjaj my-[180px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md">
        <h1 className="text-2xl font-bold mb-[15px]">Ažuriraj Dinastiju</h1>
        <form className="w-full flex flex-col gap-4" onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="Naziv dinastije"
            value={naziv}
            onChange={(e) => setNaziv(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
            required
          />

          <div className="flex gap-4">
            <label className="flex flex-col flex-1">
              Početak vladavine:
              <input
                type="number"
                value={pocetakGod}
                onChange={(e) => setPocetakGod(Number(e.target.value))}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
              />
              <label className="mt-1 flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={pocetakPNE}
                  onChange={(e) => setPocetakPNE(e.target.checked)}
                  className="mr-1"
                />
                p. n. e.
              </label>
            </label>

            <label className="flex flex-col flex-1">
              Kraj vladavine:
              <input
                type="number"
                value={krajGod}
                onChange={(e) => setKrajGod(Number(e.target.value))}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
              />
              <label className="mt-1 flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={krajPNE}
                  onChange={(e) => setKrajPNE(e.target.checked)}
                  className="mr-1"
                />
                p. n. e.
              </label>
            </label>
          </div>

          <label className="flex flex-col">
            Slika dinastije:
            <input
              type="file"
              accept="image/*"
              onChange={handleFileChange}
              className="mt-1"
            />
            {slikaFile ? (
              <p className="mt-2 text-sm">Izabrana slika: {slikaFile.name}</p>
            ) : (
              <img
                src={slikaURL || placeholderSlika}
                alt="Dinastija"
                className="w-32 h-40 mt-2 border"
              />
            )}
          </label>

          <button
            type="submit"
            className="bg-[#3f2b0a] text-[#e6cda5] p-[6px] mb-[15px] rounded-[3px] hover:bg-[#2b1d07] transition font-bold"
          >
            Sačuvaj promene
          </button>
        </form>
      </div>
    </div>
  );
}
