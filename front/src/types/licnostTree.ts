export interface LicnostTree {
  id: string;
  titula: string;
  ime: string;
  prezime: string;
  godinaRodjenja: number;
  godinaRodjenjaPNE: boolean;
  godinaSmrti: number;
  godinaSmrtiPNE: boolean;
  pol: string;
  mestoRodjenja?: string;
  slika?: string;
  tekst?: string;
  //porodice: FamilyNode[];
  deca: LicnostTree[];
}

export interface FamilyNode {
  otac?: LicnostTree;
  majka?: LicnostTree;
  deca: LicnostTree[];
}
