import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../AuthContext";
import type { DogadjajUnion, TipDogadjaja, Bitka, Rat } from "../../types/dogadjaj";

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
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { token } = useAuth();

  // Opšta polja Dogadjaj
  const [ime, setIme] = useState("");
  const [tip, setTip] = useState<TipDogadjaja>("Bitka");
  const [lokacija, setLokacija] = useState("");
  const [godina, setGodina] = useState("");
  const [isPNE, setIsPNE] = useState(false);
  const [tekst, setTekst] = useState("");

  // Polja za Bitku
  const [pobednik, setPobednik] = useState("");
  const [brojZrtava, setBrojZrtava] = useState("");
  const [rat, setRat] = useState("");

  // Polja za Rat
  const [godinaDo, setGodinaDo] = useState("");
  const [isPNEDo, setIsPNEDo] = useState(false); // Checkbox za GodinaDo

  // Dropdown-i
  const [zemlje, setZemlje] = useState<Zemlja[]>([]);
  const [ratovi, setRatovi] = useState<RatDropdown[]>([]);

  useEffect(() => {
    if (!id) return;

    const fetchZemlje = async () => {
      try {
        const res = await axios.get<Zemlja[]>("http://localhost:5210/api/GetAllZemlje");
        setZemlje(res.data);
      } catch (err) {
        console.error("Greška pri učitavanju zemalja:", err);
      }
    };

    const fetchRatovi = async () => {
      try {
        const res = await axios.get<RatDropdown[]>("http://localhost:5210/api/GetAllRatovi");
        setRatovi(res.data);
      } catch (err) {
        console.error("Greška pri učitavanju ratova:", err);
      }
    };

    const loadDogadjaj = async () => {
      try {
        const tipRes = await axios.get<{ tip: string }>(`http://localhost:5210/api/GetDogadjaj/${id}`);
        const tipRaw = tipRes.data.tip;
        const tipDogadjaja: TipDogadjaja =
          tipRaw === "Rat" ? "Rat" :
          tipRaw === "Bitka" ? "Bitka" :
          tipRaw === "Ustanak" ? "Ustanak" :
          tipRaw === "Sporazum" ? "Sporazum" :
          tipRaw === "Savez" ? "Savez" :
          tipRaw === "Dokument" ? "Dokument" :
          "Opsada";
        setTip(tipDogadjaja);

        if (tipDogadjaja === "Bitka") {
          const resBitka = await axios.get<Bitka>(`http://localhost:5210/api/GetBitka/${id}`);
          const bitka = resBitka.data;
          setIme(bitka.ime);
          setLokacija(bitka.lokacija ?? "");
          setGodina(bitka.godina?.god.toString() ?? "");
          setIsPNE(bitka.godina?.isPne ?? false);
          setTekst(bitka.tekst ?? "");
          setPobednik(bitka.pobednik);
          setBrojZrtava(bitka.brojZrtava.toString());
          setRat(bitka.rat ?? "");
        } else if (tipDogadjaja === "Rat") {
          const resRat = await axios.get<Rat>(`http://localhost:5210/api/GetRat/${id}`);
          const ratData = resRat.data;
          setIme(ratData.ime);
          setLokacija(ratData.lokacija ?? "");
          setGodina(ratData.godina?.god.toString() ?? "");
          setIsPNE(ratData.godina?.isPne ?? false);
          setTekst(ratData.tekst ?? "");
          setPobednik(ratData.pobednik);
          setGodinaDo(ratData.godinaDo?.god.toString() ?? "");
          setIsPNEDo(ratData.godinaDo?.isPne ?? false); // automatski štiklirano ako je true
        } else {
          const resDog = await axios.get<DogadjajUnion>(`http://localhost:5210/api/GetDogadjaj/${id}`);
          const dog = resDog.data;
          setIme(dog.ime);
          setLokacija(dog.lokacija ?? "");
          setGodina(dog.godina?.god.toString() ?? "");
          setIsPNE(dog.godina?.isPne ?? false);
          setTekst(dog.tekst ?? "");
        }
      } catch (err) {
        console.error("Greška pri učitavanju događaja:", err);
        alert("Greška pri učitavanju događaja!");
      }
    };

    fetchZemlje();
    fetchRatovi();
    loadDogadjaj();
  }, [id]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!ime.trim() || !token || !id) return;

    const payload: any = {
      Ime: ime,
      Tip: tip,
      Tekst: tekst || undefined,
      Lokacija: lokacija || undefined,
      Godina: godina ? { god: Number(godina), isPne: isPNE } : undefined,
    };

    if (tip === "Bitka") {
      payload.Pobednik = pobednik || undefined;
      payload.BrojZrtava = brojZrtava ? Number(brojZrtava) : undefined;
      payload.Rat = rat || undefined;
    }

    if (tip === "Rat") {
      payload.Pobednik = pobednik || undefined;
      payload.GodinaDo = godinaDo ? { god: Number(godinaDo), isPne: isPNEDo } : undefined;
      // polje Bitke uklonjeno
    }

    try {
      const endpoint =
        tip === "Bitka"
          ? `http://localhost:5210/api/UpdateBitka/${id}`
          : tip === "Rat"
          ? `http://localhost:5210/api/UpdateRat/${id}`
          : `http://localhost:5210/api/UpdateDogadjaj/${id}`;

      await axios.put(endpoint, payload, {
        headers: { Authorization: `Bearer ${token}` },
      });

      alert("Uspešno ažurirano!");
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
            className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
            required
          />

          <select
            value={lokacija}
            onChange={(e) => setLokacija(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
          >
            <option value="">Izaberi lokaciju</option>
            {zemlje.map((z) => (
              <option key={z.naziv} value={z.naziv}>
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
              className="p-[6px] rounded-[3px] border border-[#3f2b0a] flex-1"
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
                className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
              />
              <input
                type="number"
                placeholder="Broj žrtava"
                value={brojZrtava}
                onChange={(e) => setBrojZrtava(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
              />
              <select
                value={rat}
                onChange={(e) => setRat(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
              >
                <option value="">Izaberi rat (opciono)</option>
                {ratovi.map((r) => (
                  <option key={r.ime} value={r.ime}>
                    {r.ime}
                  </option>
                ))}
              </select>
            </>
          )}

          {tip === "Rat" && (
            <div className="flex flex-col gap-4">
              <input
                type="text"
                placeholder="Pobednik"
                value={pobednik}
                onChange={(e) => setPobednik(e.target.value)}
                className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
              />

              <div className="flex gap-4 items-center">
                <input
                  type="number"
                  placeholder="Godina do"
                  value={godinaDo}
                  onChange={(e) => setGodinaDo(e.target.value)}
                  className="p-[6px] rounded-[3px] border border-[#3f2b0a] flex-1"
                />
                <label className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={isPNEDo}
                    onChange={(e) => setIsPNEDo(e.target.checked)}
                  />
                  p. n. e.
                </label>
              </div>
            </div>
          )}

          <textarea
            placeholder="Tekst događaja"
            value={tekst}
            onChange={(e) => setTekst(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] h-32 resize-none"
          />

          <button
            type="submit"
            className="bg-[#3f2b0a] text-[#e6cda5] p-[6px] rounded-[3px] hover:bg-[#2b1d07] transition font-bold"
          >
            Sačuvaj promene
          </button>
        </form>
      </div>
    </div>
  );
}
