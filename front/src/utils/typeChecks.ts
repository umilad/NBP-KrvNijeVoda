import type { Dogadjaj, Rat, Bitka } from "../types/dogadjaj";

export function isRat(d: Dogadjaj): d is Rat {
  return d.tip === "Rat";
}

export function isBitka(d: Dogadjaj): d is Bitka {
  return d.tip === "Bitka";
}