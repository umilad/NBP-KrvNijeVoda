import type { LicnostTree } from "../types";

interface Props {
  osoba: LicnostTree;
}

export default function PorodicnoStabloPrikaz({ osoba }: Props) {
  return (
    <div className="flex flex-col items-center relative">
      
      {/* osoba CARD */}
      <div className="group text-center hover:scale-110 transition">
        <img
          src={osoba.slika ?? "/src/images/placeholder_muski.png"}
          className="w-[80px] h-[100px] object-cover mx-auto border-2 border-[#3f2b0a] rounded-lg"
        />
        <p className="font-bold">
          {osoba.titula} {osoba.ime} {osoba.prezime}
        </p>
        <p className="text-sm">
          {osoba.godinaRodjenja} – {osoba.godinaSmrti}
        </p>

        {/* TOOLTIP */}
        {osoba.tekst && (
          <div className="absolute top-full mt-2 w-56 p-3 bg-[#e6cda5] border border-[#3f2b0a] rounded opacity-0 group-hover:opacity-100 transition z-20">
            {osoba.tekst}
          </div>
        )}
      </div>

      {/* CHILDREN */}
      {osoba.deca && osoba.deca.length > 0 && (
        <>
          {/* vertical line */}
          <div className="w-[2px] h-[30px] bg-[#3f2b0a]" />

          <div className="flex gap-10 mt-4 relative">
            {/* horizontal line */}
            <div className="absolute top-0 left-0 right-0 h-[2px] bg-[#3f2b0a]" />

            {osoba.deca.map(child => (
              <PorodicnoStabloPrikaz key={child.id} osoba={child} />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
