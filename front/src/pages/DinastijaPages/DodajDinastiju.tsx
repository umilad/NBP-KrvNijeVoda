import { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../AuthContext";

export default function DodajDinastiju() {
  const [naziv, setNaziv] = useState("");
  const [pocetakGod, setPocetakGod] = useState<number | "">("");
  const [pocetakPNE, setPocetakPNE] = useState(false);
  const [krajGod, setKrajGod] = useState<number | "">("");
  const [krajPNE, setKrajPNE] = useState(false);
  const [slika, setSlika] = useState<File | null>(null);

  const navigate = useNavigate();
  const { token } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!naziv.trim()) {
      alert("Naziv dinastije je obavezan!");
      return;
    }
    if (pocetakGod === "" || pocetakGod === 0) {
      alert("Godina početka vladavine mora biti veća od 0!");
      return;
    }
    if (krajGod === "" || krajGod === 0) {
      alert("Godina kraja vladavine mora biti veća od 0!");
      return;
    }
    if (Number(pocetakGod) > Number(krajGod)) {
      alert("Godina početka vladavine ne može biti veća od godine kraja!");
      return;
    }

    // === FormData za [FromForm] upload ===
    const formData = new FormData();
    formData.append("Naziv", naziv);
    formData.append("PocetakVladavineGod", (pocetakGod || 0).toString());
    formData.append("PocetakVladavinePNE", pocetakPNE.toString());
    formData.append("KrajVladavineGod", (krajGod || 0).toString());
    formData.append("KrajVladavinePNE", krajPNE.toString());
    if (slika) {
      formData.append("slika", slika);
    }

    try {
      const response = await axios.post(
        "http://localhost:5210/api/CreateDinastija",
        formData,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "multipart/form-data",
          },
        }
      );
      alert(response.data);
      navigate("/dinastije");
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
    <div className="dodaj-dinastiju my-[180px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a] shadow-md">
        <h1 className="text-2xl font-bold mb-[15px]">Dodaj Dinastiju</h1>

        <form className="w-full flex flex-col gap-4" onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="Naziv dinastije"
            value={naziv}
            onChange={(e) => setNaziv(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
            required
          />

          <div className="flex gap-4 items-center">
            <input
              type="number"
              placeholder="Početak vladavine"
              value={pocetakGod}
              onChange={(e) => setPocetakGod(Number(e.target.value))}
              className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"
              required
            />
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={pocetakPNE}
                onChange={(e) => setPocetakPNE(e.target.checked)}
              />
              p. n. e.
            </label>
          </div>

          <div className="flex gap-4 items-center">
            <input
              type="number"
              placeholder="Kraj vladavine"
              value={krajGod}
              onChange={(e) => setKrajGod(Number(e.target.value))}
              className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none flex-1"
              required
            />
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={krajPNE}
                onChange={(e) => setKrajPNE(e.target.checked)}
              />
              p. n. e.
            </label>
          </div>

          <input
            type="file"
            accept="image/*"
            onChange={(e) => setSlika(e.target.files ? e.target.files[0] : null)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />

          <button
            type="submit"
            className="bg-[#3f2b0a] text-[#e6cda5] px-6 py-2 rounded-lg shadow-md hover:bg-[#2b1d07] transition font-bold"
          >
            Kreiraj Dinastiju
          </button>
        </form>

        {/* Prikaz preview slike ako je odabrana */}
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
