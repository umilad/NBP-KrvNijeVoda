import axios from "axios";
import { useEffect, useState, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import type { Licnost } from "../types";

export default function LicnostPage() {
  const [licnost, setLicnost] = useState<Licnost | null>(null);
  const [isVladar, setIsVladar] = useState<boolean | null>(null);

  const { id } = useParams<{ id: string }>();
  const { token, role } = useAuth();
  const hasTracked = useRef(false);
  const navigate = useNavigate();

  useEffect(() => {
    async function loadLicnost() {
      if (!id) return;

      try {
        const vladarRes = await axios
          .get<Licnost>(`http://localhost:5210/api/GetVladar/${id}`)
          .catch(() => null);

        if (vladarRes?.data) {
          setLicnost(vladarRes.data);
          setIsVladar(true);
          return;
        }

        const licnostRes = await axios.get<Licnost>(
          `http://localhost:5210/api/GetLicnost/${id}`
        );

        setLicnost(licnostRes.data);
        setIsVladar(false);
      } catch (err) {
        console.error(err);
      }
    }

    loadLicnost();
  }, [id]);

  useEffect(() => {
    if (!token || !licnost || hasTracked.current) return;

    hasTracked.current = true;

    const path = `/licnost/${licnost.id}`;
    const label = `${licnost.titula || ""} ${licnost.ime} ${licnost.prezime}`.trim();

    (async () => {
      try {
        await axios.post(
          "http://localhost:5210/api/Auth/track",
          { path, label },
          { headers: { Authorization: `Bearer ${token}` } }
        );

        await axios.post(
          "http://localhost:5210/api/Auth/track-visit",
          { path, label },
          { headers: { Authorization: `Bearer ${token}` } }
        );
      } catch (err) {
        console.error(err);
      }
    })();
  }, [licnost, token]);

  const handleDelete = async () => {
    if (!id || !token || isVladar === null) return;
    if (!confirm("Da li ste sigurni da želite da obrišete ovu ličnost?")) return;

    try {
      const endpoint = isVladar
        ? `http://localhost:5210/api/DeleteVladar/${id}`
        : `http://localhost:5210/api/DeleteLicnost/${id}`;

      await axios.delete(endpoint, {
        headers: { Authorization: `Bearer ${token}` },
      });

      alert("Uspešno obrisano!");
      navigate("/licnosti");
    } catch (err) {
      console.error(err);
      alert("Greška prilikom brisanja");
    }
  };

  const handleUpdate = () => {
    if (!id || isVladar === null) return;
    navigate(`/licnost/edit/${id}`, { state: { isVladar } });
  };

  if (!licnost) {
    return <div className="text-center mt-20">Učitavanje...</div>;
  }

  return (
    <div className="licnosti-container flex flex-col items-center justify-center text-white my-[100px]">
      <div className="relative w-[300px] h-[355px] m-auto">
        <img
          src="/src/images/picture-frame.png"
          alt="Ram"
          className="absolute top-0 left-0 w-full h-full z-10 pointer-events-none"
        />

        <div className="absolute inset-0 flex items-center justify-center z-0">
          <img
            src={`/images/licnosti/${licnost?.slika}`}
            alt={`${licnost.titula} ${licnost.ime} ${licnost.prezime}`}
            className="w-[190px] h-[235px] object-cover"
          />
        </div>
      </div>

      <div className="absolute top-100 mx-[100px] p-[20px] block border-2 border-[#3f2b0a] bg-[#e6cda5] rounded-lg text-center text-[#3f2b0a] mt-4">
        <p className="text-2xl font-bold mt-2">
          {licnost.titula} {licnost.ime} {licnost.prezime}
        </p>

        <p className="text-xl font-bold mt-2">
          {licnost.godinaRodjenja ?? ""}
          {licnost.godinaSmrti ? ` - ${licnost.godinaSmrti}.` : ""}
          {(licnost.godinaRodjenjaPNE || licnost.godinaSmrtiPNE) && " p.n.e."}
        </p>

        <p className="text-lg p-[30px] mt-2 text-justify">
          {licnost.tekst}
        </p>

        {role === "admin" && (
          <div className="flex gap-4 justify-center mt-4">
            <button
              onClick={handleDelete}
              className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110"
            >
              Obriši
            </button>

            <button
              onClick={handleUpdate}
              className="px-[12px] py-[6px] border border-[#e6cda5] bg-[#3f2b0a] text-[#e6cda5] hover:bg-[#e6cda5] hover:text-[#3f2b0a] transition-all duration-300 transform hover:scale-110"
            >
              Ažuriraj
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
