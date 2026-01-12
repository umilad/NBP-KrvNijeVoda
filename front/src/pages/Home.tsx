import { useState, useEffect, useRef } from "react";
import YearTimeline from '../components/YearTimeline.tsx';
import type { YearTimelineHandle } from '../components/YearTimeline.tsx';
import DinastijaPrikaz from "../components/DinastijaPrikaz";
import DogadjajPrikaz from "../components/DogadjajPrikaz";
import LicnostPrikaz from "../components/LicnostPrikaz";
import type { AllEventsForGodinaResponse } from "../types";
import { useSearch } from "../components/SearchContext";
import axios from "axios";
import Searchbar from "../components/Searchbar";

export default function Home() {
  const timelineRef = useRef<YearTimelineHandle>(null); // for timeline
  const scrollRef = useRef<HTMLDivElement>(null); // for carousel scroll
  const [activeYear, setActiveYear] = useState<number>(1);
  const [allActiveYearEvents, setAllActiveYearEvents] = useState<AllEventsForGodinaResponse | null>(null);
  const [topPages, setTopPages] = useState<{ path: string; count: number; label?: string }[]>([]);
  const { query } = useSearch();

  // 🌟 Load top 10 global pages
  useEffect(() => {
    async function fetchTopPages() {
      try {
        const token = localStorage.getItem("token");

        const response = await axios.get(
          "http://localhost:5210/api/auth/global-top-pages",
          { headers: { Authorization: `Bearer ${token}` } }
        );

        setTopPages(response.data);
      } catch (err) {
        console.error("Error fetching top pages:", err);
      }
    }

    fetchTopPages();
  }, []);

  // 🌟 Load events for active year
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

  const handleSearch = () => {
    const queryNumber = parseInt(query, 10);
    if (timelineRef.current) {
      timelineRef.current.scrollToYear(queryNumber);
    }
  };

  // 🌟 Scroll carousel left/right
  const scrollLeft = () => {
    scrollRef.current?.scrollBy({ left: -300, behavior: "smooth" });
  };
  const scrollRight = () => {
    scrollRef.current?.scrollBy({ left: 300, behavior: "smooth" });
  };

  return (
    <div className="home overflow-y-scroll no-scrollbar h-screen my-[100px]">

      {/* 🔥 Top Pages Carousel */}
{topPages.length > 0 && (
  <div className="w-full relative overflow-hidden mb-16 h-[260px] px-16 flex items-center">
    {/* Strelica levo */}
    <button
      onClick={scrollLeft}
      className="bg-[#E6CDA5] hover:bg-[#d6b889] text-[#3f2b0a] w-12 h-12 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 mr-4"
    >
      ‹
    </button>

    {/* Carousel container */}
    <div
      ref={scrollRef}
      className="flex gap-6 overflow-x-auto scroll-smooth no-scrollbar h-[200px] relative pt-4 pb-4 flex-1"
    >
      {topPages.slice(0, 15).map((page) => {
        const path = page.path;
        const label = page.label || path;

        return (
          <div
            key={path}
            className="flex-shrink-0 w-[180px] h-[180px] cursor-pointer bg-[#E6CDA5] rounded-2xl shadow-md border border-[#3f2b0a] flex flex-col justify-between p-4 overflow-hidden relative transition-all duration-300 hover:shadow-2xl hover:scale-[1.03]"
            onClick={() => window.location.assign(path)}
          >
            <p className="text-center text-lg font-semibold mb-2 break-words">{label}</p>
            <p className="text-center text-sm text-gray-700">Pregleda: {page.count}</p>
          </div>
        );
      })}
    </div>

    {/* Strelica desno */}
    <button
      onClick={scrollRight}
      className="bg-[#E6CDA5] hover:bg-[#d6b889] text-[#3f2b0a] w-12 h-12 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 ml-4"
    >
      ›
    </button>
  </div>
)}

{/* Naslov */}
<h2 className="text-4xl font-extrabold text-center mb-6 text-[#3f2b0a] drop-shadow-md">
  🔥 Najposećenije svih vremena
</h2>








      {/* 👑 Vladari/Licnosti/Dinastije */}
      <div className="events-above w-[calc(100%-200px)] mx-[20px] flex justify-around flex-wrap gap-2">
        {allActiveYearEvents && (
          <>
            <div className="scale-80">
              {allActiveYearEvents.vladari.map(v =>
                <LicnostPrikaz key={v.id} licnost={v} />
              )}
            </div>
            <div className="scale-80">
              {allActiveYearEvents.licnosti.map(l =>
                <LicnostPrikaz key={l.id} licnost={l} />
              )}
            </div>
            <div className="scale-60">
              {allActiveYearEvents.dinastije.map(d => (
                <DinastijaPrikaz key={d.id} dinastija={d} />
              ))}
            </div>
          </>
        )}
      </div>

      {/* 🕰 Timeline */}
      <div className="relative min-h-[303px] mt-8">
        <p className="text-center text-2xl font-bold mb-4">Putovanje kroz vreme</p>
        <Searchbar onSearch={handleSearch} />
        <YearTimeline activeYear={activeYear} setActiveYear={setActiveYear} ref={timelineRef} />
      </div>

      {/* 📜 Događaji za aktivnu godinu */}
      <div className="events-below w-[calc(100%-40px)] mx-[20px] grid grid-cols-3 gap-4 mt-10 min-h-[130px]">
        {allActiveYearEvents && [
          ...allActiveYearEvents.dogadjaji,
          ...allActiveYearEvents.bitke,
          ...allActiveYearEvents.ratovi
        ].map(dogadjaj => (
          <DogadjajPrikaz key={dogadjaj.id} dogadjaj={dogadjaj} variant="short" />
        ))}
      </div>

    </div>
  );
}
