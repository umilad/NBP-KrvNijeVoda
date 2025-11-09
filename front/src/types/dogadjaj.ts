import type Godina  from './godina';

export type TipDogadjaja =
    | "Bitka"
    | "Rat"
    | "Ustanak"
    | "Sporazum"
    | "Savez"
    | "Dokument"
    | "Opsada";

export interface Dogadjaj {
    id: string;
    tip: TipDogadjaja;
    ime: string;
    godina?: Godina | null;
    lokacija?: string | null;
    tekst: string;
}

export interface Bitka extends Dogadjaj {
    pobednik: string;
    rat?: string;
    brojZrtava: number;
}

export interface Rat extends Dogadjaj {    
    godinaDo?: Godina | null;
    bitke: string[];
    pobednik: string;
}