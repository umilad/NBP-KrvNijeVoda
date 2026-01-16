import { useState, useEffect, useRef } from "react";
import YearTimeline from "../components/YearTimeline";
import type { YearTimelineHandle } from "../components/YearTimeline";
import DinastijaPrikaz from "../components/DinastijaPrikaz";
import DogadjajPrikaz from "../components/DogadjajPrikaz";
import LicnostPrikaz from "../components/LicnostPrikaz";
import type { AllEventsForGodinaResponse } from "../types";
import { useSearch } from "../components/SearchContext";
import axios from "axios";

import type {
  Dogadjaj,
  Licnost,
  Dinastija
} from "../types";


type CarouselItem =
  | { type: "dogadjaj"; data: Dogadjaj }
  | { type: "licnost"; data: Licnost }
  | { type: "dinastija"; data: Dinastija };

function Carousel({ events }: { events: AllEventsForGodinaResponse | null }) {
  const scrollRef = useRef<HTMLDivElement>(null);

  if (!events) return null;

  const scrollLeft = () =>
    scrollRef.current?.scrollBy({ left: -300, behavior: "smooth" });

  const scrollRight = () =>
    scrollRef.current?.scrollBy({ left: 300, behavior: "smooth" });

  const items: CarouselItem[] = [
    ...events.dogadjaji.map(d => ({
      type: "dogadjaj" as const,
      data: d,
    })),
    ...events.bitke.map(b => ({
      type: "dogadjaj" as const,
      data: b,
    })),
    ...events.ratovi.map(r => ({
      type: "dogadjaj" as const,
      data: r,
    })),
    ...events.vladari.map(v => ({
      type: "licnost" as const,
      data: v,
    })),
    ...events.licnosti.map(l => ({
      type: "licnost" as const,
      data: l,
    })),
    ...events.dinastije.map(d => ({
      type: "dinastija" as const,
      data: d,
    })),
  ];


  return (
    <div className="w-full relative overflow-hidden h-[340px] px-16 flex items-center">
      <button
        onClick={scrollLeft}
        className="bg-[#E6CDA5] hover:bg-[#3f2b0a] hover:text-[#d6b889] text-[#3f2b0a] text-[40px] pb-[11px] w-8 h-8 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 ml-[4px] mr-[4px]"
      >
        ‹
      </button>

      <div
        ref={scrollRef}
        className="flex gap-6 overflow-x-auto overflow-y-hidden scroll-smooth no-scrollbar
                   h-[300px] items-center flex-1"
      >
        {items && items.length > 0 ? (
        items.map(item => {
          switch (item.type) {
            case "dogadjaj":
              return (
                <div key={`d-${item.data.id}`} className="scale-90">
                  <DogadjajPrikaz dogadjaj={item.data} variant="short" />
                </div>
              );

            case "licnost":
              return (
                <div key={`l-${item.data.id}`} className="scale-70">
                  <LicnostPrikaz licnost={item.data} />
                </div>
              );

            case "dinastija":
              return (
                <div key={`di-${item.data.id}`} className="scale-70">
                  <DinastijaPrikaz dinastija={item.data} />
                </div>
              );

            default:
              return null;
          }
        })
        ) : (
        <span className="text-center w-full text-lg font-semibold text-gray-600">
          Nema događaja u ovoj godini
        </span>
      )}
      </div>

      <button
        onClick={scrollRight}
        className="bg-[#E6CDA5] hover:bg-[#3f2b0a] hover:text-[#d6b889] text-[#3f2b0a] text-[40px] pb-[11px] w-8 h-8 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 ml-[4px] mr-[4px]"
      >
        ›
      </button>
    </div>
  );
}

export default function Home() {
  const timelineRef = useRef<YearTimelineHandle>(null);
  const [activeYear, setActiveYear] = useState(1);
  const [events, setEvents] = useState<AllEventsForGodinaResponse | null>(null);
  const { query } = useSearch();

  useEffect(() => {
    async function load() {
      try {
        const res = await axios.get<AllEventsForGodinaResponse>(
          `http://localhost:5210/api/GetAllEventsForGodina/${activeYear}`
        );

        setEvents(
          res.data ?? {
            dogadjaji: [],
            bitke: [],
            ratovi: [],
            vladari: [],
            licnosti: [],
            dinastije: [],
          }
        );
      } catch (err) {
        console.error(err);
      }
    }

    load();
  }, [activeYear]);

  useEffect(() => {
    const year = parseInt(query, 10);
    if (!isNaN(year)) {
      timelineRef.current?.scrollToYear(year);
    }
  }, [query]);

  return (
    <div className="home overflow-y-scroll no-scrollbar h-screen">
 
      <div className="fixed top-20 left-0 w-full z-50">
        <p className="text-center text-2xl font-bold mb-4 text-[#3f2b0a]">
          Putovanje kroz vreme
        </p>
        <YearTimeline
          activeYear={activeYear}
          setActiveYear={setActiveYear}
          ref={timelineRef}
        />
      </div>

      <div className="h-[360px]" />

      <Carousel events={events} />
    </div>
  );
}
