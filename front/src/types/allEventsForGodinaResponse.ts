import type { Dogadjaj, Rat, Bitka, Vladar, Licnost, Dinastija } from "../types";

export interface AllEventsForGodinaResponse {
    dogadjaji: Dogadjaj[];
    bitke: Bitka[];
    ratovi: Rat[];
    vladari: Vladar[];
    licnosti: Licnost[];
    dinastije: Dinastija[];
  }