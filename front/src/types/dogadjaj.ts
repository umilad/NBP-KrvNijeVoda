import type { Godina } from "./godina";

export type TipDogadjaja =
    | "Bitka"
    | "Rat"
    | "Ustanak"
    | "Sporazum"
    | "Savez"
    | "Dokument"
    | "Opsada";

export interface DogadjajBase {
    id: string;
    tip: TipDogadjaja;
    ime: string;
    godina?: Godina | null;
    lokacija?: string | null;
    tekst: string;
}

export interface Bitka extends DogadjajBase {
    tip: "Bitka";
    pobednik: string;
    rat?: string;
    brojZrtava: number;
}

export interface Rat extends DogadjajBase {
    tip: "Rat";
    godinaDo?: Godina | null;
    bitke: string[];
    pobednik: string;
}

export interface OstaliDogadjaj extends DogadjajBase {
    tip: Exclude<TipDogadjaja, "Bitka" | "Rat">;
}

export type DogadjajUnion = Bitka | Rat | OstaliDogadjaj;
