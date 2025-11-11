import type Dinastija from './dinastija';

export interface Licnost {
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
    isVladar : boolean;
}

export interface Vladar extends Licnost {
    dinastija?: Dinastija;
    teritorija?: string;
    pocetakVladavineGod: number;
    pocetakVladavinePNE: number;
    krajVladavineGod: number;
    krajVladavinePNE: number;
}