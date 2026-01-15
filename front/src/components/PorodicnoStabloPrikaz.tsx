import type { LicnostTree } from "../types";
import PersonCard from "./PersonCard";


interface Props {
  licnost: LicnostTree;
}

export default function PorodicnoStabloPrikaz({ licnost }: Props) {
  const hasSpouse = licnost.supruznici && licnost.supruznici.length > 0;
  const hasChildren = licnost.deca && licnost.deca.length > 0;
  const hasOnlyOneChild = licnost.deca.length == 1; 
  const hasParents = licnost.roditeljiID && licnost.roditeljiID.length > 0;
  const numOfChildren = licnost.deca.length;
  const childrenLineWidth =
                            hasOnlyOneChild
                              ? 0
                              : (numOfChildren - 1) * (150);

  return (
    <div className="flex flex-col items-center relative">

      <div className="flex items-center gap-6 relative">
        <PersonCard licnost={licnost} />
        

        {hasSpouse && licnost.supruznici.map(s => (
          <PersonCard key={s.id} licnost={s} />
        ))}

        {hasSpouse && (
          <div className="absolute top-[54px] translate-x-[115px] w-[95px] h-[2px] bg-[#3f2b0a] z-0" />
        )}

      </div>

      {hasSpouse && hasChildren && (
        <div className="absolute top-[55px] left-1/2 transform -translate-x-1/2 w-[3px] h-[146px] bg-[#3f2b0a] z-0" />
      )}

      {!hasSpouse && hasChildren && !hasOnlyOneChild && (
        <div className="absolute top-[169px] left-1/2 transform -translate-x-1/2 w-[3px] h-[33px] bg-[#3f2b0a] z-0" />
      )}

      {hasParents && (
        <div className="absolute top-[-37px] left-1/2 transform -translate-x-1/2 w-[3px] h-[38px] bg-[#3f2b0a] z-0" />
      )}

      

      {licnost.deca.length > 0 &&(
        <div className="flex relative"
        style={{
          top: hasOnlyOneChild ?
          15 : 37
        }}>
          {!hasOnlyOneChild && (
            <div>           

               <div
              className="absolute top-[-37px] left-1/2 h-[3px] bg-[#3f2b0a]"
              style={{
                width: childrenLineWidth,
                transform: "translateX(-50%)",
              }}
            />
            <div className="absolute top-[-37px] left-19 h-[3px] bg-[#3f2b0a]"
                style= {{
                  right: numOfChildren*50
                }}
            />
            </div>
          )}
          
          {licnost.deca.map(child => (
            <PorodicnoStabloPrikaz key={child.id} licnost={child} />
          ))}
        </div>
      )}
    </div>
  );
}