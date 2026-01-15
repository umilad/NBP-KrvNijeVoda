import { useState, useEffect, useRef } from "react";
import YearTimeline from '../components/YearTimeline.tsx';
import type { YearTimelineHandle } from '../components/YearTimeline.tsx';
import DinastijaPrikaz from "../components/DinastijaPrikaz";
import DogadjajPrikaz from "../components/DogadjajPrikaz";
import LicnostPrikaz from "../components/LicnostPrikaz";
import type { AllEventsForGodinaResponse } from "../types";
import { useSearch } from "../components/SearchContext";
import axios from "axios";

function Carousel({ events }: { events: AllEventsForGodinaResponse | null}) {
  const scrollRef = useRef<HTMLDivElement>(null);

  const scrollLeft = () => {
    scrollRef.current?.scrollBy({ left: -300, behavior: "smooth" });
  };

  const scrollRight = () => {
    scrollRef.current?.scrollBy({ left: 300, behavior: "smooth" });
  };

  if (!events) return null;

  return (
    <div className="mb-20">

      <div className="w-full relative overflow-hidden h-[400px] px-16 flex items-center">
        <button
          onClick={scrollLeft}
          className="bg-[#E6CDA5] hover:bg-[#3f2b0a] hover:text-[#d6b889] text-[#3f2b0a] text-[40px] pb-[11px] w-8 h-8 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 ml-[4px] mr-[4px]"
        >
          ‹
        </button>

        <div
          ref={scrollRef}
          className="flex scale-60 left-30 gap-8 overflow-x-auto scroll-smooth no-scrollbar h-[350px] items-center pt-4 pb-4 flex-1 max-h-350 min-h-350 min-w-fit"
        >
            
                {events && [
                    ...events.dogadjaji,
                    ...events.bitke,
                    ...events.ratovi
                    ].map(dogadjaj => (
                    <DogadjajPrikaz key={dogadjaj.id} dogadjaj={dogadjaj} variant="short" />
                    ))}
                {events.vladari.map(v =>
                    <LicnostPrikaz key={v.id} licnost={v} />
                )}
                {events.licnosti.map(l =>
                    <LicnostPrikaz key={l.id} licnost={l} />
                )}
                {events.dinastije.map(d => (
                    <DinastijaPrikaz key={d.id} dinastija={d} />
                ))}
            
          
        
        </div>

        <button
          onClick={scrollRight}
          className="bg-[#E6CDA5] hover:bg-[#3f2b0a] hover:text-[#d6b889] text-[#3f2b0a] text-[40px] pb-[11px] w-8 h-8 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 ml-[4px] mr-[4px]"
        >
          ›
        </button>
      </div>
    </div>
  );
}

export default function Home() {
  const timelineRef = useRef<YearTimelineHandle>(null); 
  const [activeYear, setActiveYear] = useState<number>(1);
  const [allActiveYearEvents, setAllActiveYearEvents] = useState<AllEventsForGodinaResponse | null>(null);
  const { query } = useSearch();


  useEffect(() => {
    async function GetAllEventsForGodina() {
      try {
        if (!activeYear) return;
        const response = await axios.get<AllEventsForGodinaResponse>(
          `http://localhost:5210/api/GetAllEventsForGodina/${activeYear}`
        );
        return response.data;
      } catch (error) {
        console.error("Error fetching all events:", error);
        return [];
      }
    }

    async function loadAllEventsForGodina() {
      const data = await GetAllEventsForGodina();
      const safeData: AllEventsForGodinaResponse = Array.isArray(data) && data.length === 0
        ? { dogadjaji: [], bitke: [], ratovi: [], vladari: [], licnosti: [], dinastije: [] }
        : (data as AllEventsForGodinaResponse);
      setAllActiveYearEvents(safeData);
    }

    loadAllEventsForGodina();
  }, [activeYear]);


    useEffect(() => {
    const yearFromSearch = parseInt(query, 10);

    if (timelineRef.current && !isNaN(yearFromSearch)) {
        if (yearFromSearch >= 0 && yearFromSearch <= 2026) {
        timelineRef.current.scrollToYear(yearFromSearch);
        }
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

        <div className="h-[400px]" />
        <Carousel events={allActiveYearEvents} />
      

    </div>
  );
}
