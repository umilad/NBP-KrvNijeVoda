import { Link } from "react-router-dom";
import type { LicnostTree } from "../types";

export default function PersonCard({ licnost }: { licnost: LicnostTree }) {
  return (
    <div className="relative group text-center text-[#3f2b0a] mb-[30px]">
      <div className="px-[4px] hover:scale-110 transition-transform duration-300 w-[150px] h-[170px]">
        <img
          src={`/src/images/${licnost.slika}`}
          className="w-[80px] h-[100px] object-cover mx-auto border-2 border-[#3f2b0a] rounded-lg"
        />
        <p className="font-bold">
          {licnost.titula} {licnost.ime} {licnost.prezime}
        </p>
        <p className="text-sm">
          {licnost.godinaRodjenja} – {licnost.godinaSmrti}.
        </p>
      </div>

      {licnost.tekst && (
        <Link
          to={`/licnost/${licnost.id}`}
          className="absolute -top-40 left-50 -translate-x-1/2
                     w-[180px] h-[150px] p-[6px]
                     bg-[#e6cda5] border border-[#3f2b0a]
                     rounded-lg shadow-md
                     opacity-0 group-hover:opacity-100
                     transition z-50 overflow-hidden line-clamp-6"
        >
          {licnost.tekst}
        </Link>
      )}
    </div>
  );
}
