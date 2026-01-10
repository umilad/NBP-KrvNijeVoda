import PersonCard from "./PersonCard";
import TreeNode from "../components/TreeNode";
import type { FamilyNode } from "../types";

export default function FamilyView({ family }: { family: FamilyNode }) {
  return (
    <div className="mt-8 text-center">
      <div className="flex justify-center gap-6">
        {family.otac && <PersonCard osoba={family.otac} />}
        {family.majka && <PersonCard osoba={family.majka} />}
      </div>

      <div className="flex justify-center gap-10 mt-6">
        {family.deca.map(d => (
          <TreeNode key={d.id} osoba={d} />
        ))}
      </div>
    </div>
  );
}
