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
  deca: LicnostTree[];
  supruzniciId: string;
  supruznici: LicnostTree[];
  roditeljiID: string[];
}


