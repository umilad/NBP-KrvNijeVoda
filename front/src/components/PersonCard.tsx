import type { LicnostTree } from "../types";

export default function PersonCard({ osoba }: { osoba: LicnostTree }) {
  return (
    <div className="text-center">
      <img
        src={osoba.slika ?? "/src/images/placeholder_muski.png"}
        className="w-[80px] h-[100px] object-cover mx-auto border rounded"
      />
      <p className="font-bold">
        {osoba.titula} {osoba.ime} {osoba.prezime}
      </p>
    </div>
  );
}
