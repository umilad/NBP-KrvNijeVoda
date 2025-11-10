import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../AuthContext";
import type { Dogadjaj, Bitka, Rat, TipDogadjaja } from "../../types/dogadjaj";

interface Zemlja {
  id: string;
  naziv: string;
}

interface RatDropdown {
  id: string;
  ime: string;
  godina?: number;
}

export default function AzurirajDogadjaj() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { token } = useAuth();

  // Opšta polja Dogadjaj
  const [ime, setIme] = useState("");
  const [tip, setTip] = useState<TipDogadjaja>("Bitka");
  const [lokacija, setLokacija] = useState<string>("");
  const [godina, setGodina] = useState<string>(""); // string za input controlled
  const [isPNE, setIsPNE] = useState(false);
  const [tekst, setTekst] = useState("");

  // Polja za Bitku
  const [pobednik, setPobednik] = useState<string>("");
  const [brojZrtava, setBrojZrtava] = useState<string>(""); // string
  const [rat, setRat] = useState<string>("");

  // Polja za Rat
  const [godinaDo, setGodinaDo] = useState<string>(""); // string
  const [bitke, setBitke] = useState<string>("");

  // Dropdown-i
  const [zemlje, setZemlje] = useState<Zemlja[]>([]);
  const [ratovi, setRatovi] = useState<RatDropdown[]>([]);

  // Učitavanje postojećeg dogadjaja i dropdown-a
  useEffect(() => {
    async function loadDogadjaj() {
      if (!id) return;
      try {
        const res = await axios.get<Dogadjaj | Bitka | Rat>(
          `http://localhost:5210/api/GetDogadjaj/${id}`
        );
        const d = res.data;

        setIme(d.ime);
        setTip(d.tip);
        setLokacija(d.lokacija ?? "");
        setGodina(d.godina?.God?.toString() ?? "");
        setIsPNE(d.godina?.IsPNE ?? false);
        setTekst(d.tekst);

        if (d.tip === "Bitka") {
          const b = d as Bitka;
          setPobednik(b.pobednik ?? "");
          setBrojZrtava(b.brojZrtava?.toString() ?? "");
          setRat(b.rat ?? "");
        }

        if (d.tip === "Rat") {
          const r = d as Rat;
          setPobednik(r.pobednik ?? "");
          setGodinaDo(r.godinaDo?.God?.toString() ?? "");
          setBitke(r.bitke.join(", ") ?? "");
        }
      } catch (err) {
        console.error("Greška pri učitavanju događaja:", err);
      }
    }

    async function fetchZemlje() {
      try {
        const res = await axios.get<Zemlja[]>("http://localhost:5210/api/GetAllZemlje");
        setZemlje(res.data);
      } catch (err) {
        console.error("Greška pri učitavanju zemalja:", err);
      }
    }

    async function fetchRatovi() {
      try {
        const res = await axios.get<RatDropdown[]>("http://localhost:5210/api/GetAllRatovi");
        setRatovi(res.data);
      } catch (err) {
        console.error("Greška pri učitavanju ratova:", err);
      }
    }

    loadDogadjaj();
    fetchZemlje();
    fetchRatovi();
  }, [id]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!ime.trim() || !token || !id) return;

    const payload: any = {
      Ime: ime,
      Tip: tip,
      Tekst: tekst || undefined,
      Lokacija: lokacija || undefined,
      Godina: godina ? { God: Number(godina), IsPNE: isPNE } : undefined,
    };

    if (tip === "Bitka") {
      payload.Pobednik = pobednik || undefined;
      payload.BrojZrtava = brojZrtava ? Number(brojZrtava) : undefined;
      payload.Rat = rat || undefined;
    }

    if (tip === "Rat") {
      payload.Pobednik = pobednik || undefined;
      payload.GodinaDo = godinaDo ? { God: Number(godinaDo), IsPNE: isPNE } : undefined;
      payload.Bitke = bitke ? bitke.split(",").map((b) => b.trim()) : undefined;
    }

    try {
      const response = await axios.put(
        `http://localhost:5210/api/UpdateDogadjaj/${id}`,
        payload,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      alert(response.data);
      navigate("/dogadjaji");
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        alert(`Greška: ${err.response?.data || err.message}`);
      } else {
        alert("Greška: " + (err as Error).message);
      }
    }
  };

  return (
    <div className="dodaj-dogadjaj my-[180px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md">
        <h1 className="text-2xl font-bold mb-[15px]">Ažuriraj događaj</h1>
        <form className="w-full flex flex-col gap-4" onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="Ime događaja"
            value={ime}
            onChange={(e) => setIme(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
            required
          />

          {/* Dropdown za zemlje */}
          <select
            value={lokacija}
            onChange={(e) => setLokacija(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          >
            <option value="">Izaberi lokaciju</option>
            {zemlje.map((z) => (
              <option key={z.id} value={z.naziv}>
                {z.naziv}
              </option>
            ))}
          </select>

          <div className="flex gap-4 items-center">
            <input
              type="number"
              placeholder="Godina"
              value={godina}
              onChange={(e) => setGodina(e.target.value)}
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

          {tip === "Bitka" && (
            <>
              <input
                type="text"
                placeholder="Pobednik"
                value={pobednik}
                onChange={(e) => setPobednik(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
              />
              <input
                type="number"
                placeholder="Broj žrtava"
                value={brojZrtava}
                onChange={(e) => setBrojZrtava(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
              />
              <select
                value={rat}
                onChange={(e) => setRat(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
              >
                <option value="">Izaberi rat (opciono)</option>
                {ratovi.map((r) => (
                  <option key={r.id} value={r.ime}>
                    {r.ime}
                  </option>
                ))}
              </select>
            </>
          )}

          {tip === "Rat" && (
            <>
              <input
                type="text"
                placeholder="Pobednik"
                value={pobednik}
                onChange={(e) => setPobednik(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
              />
              <input
                type="number"
                placeholder="Godina do"
                value={godinaDo}
                onChange={(e) => setGodinaDo(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
              />
              <input
                type="text"
                placeholder="Bitke (odvojene zarezom)"
                value={bitke}
                onChange={(e) => setBitke(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
              />
            </>
          )}

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
            Sačuvaj promene
          </button>
        </form>
      </div>
    </div>
  );
}
