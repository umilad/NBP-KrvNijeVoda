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

  // 1. Remove <Searchbar onSearch={handleSearch} /> from your JSX

// 2. Add this useEffect to "watch" the search query
    useEffect(() => {
    // Try to convert the search query to a number
    const yearFromSearch = parseInt(query, 10);

    // If it's a valid year, scroll the timeline
    if (timelineRef.current && !isNaN(yearFromSearch)) {
        // Optional: Only scroll if the year is within your range (0 - 2026)
        if (yearFromSearch >= 0 && yearFromSearch <= 2026) {
        timelineRef.current.scrollToYear(yearFromSearch);
        }
    }
    }, [query]); // This triggers every time the user types in the Navbar searchbar

  const handleSearch = () => {
    const queryNumber = parseInt(query, 10);
    if (timelineRef.current) {
      timelineRef.current.scrollToYear(queryNumber);
    }
  };

  return (
    <div className="home overflow-y-scroll no-scrollbar h-screen my-[100px]">
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
        <p className="text-center text-2xl font-bold mb-4 text-[#3f2b0a] fixed top-20 left-1/2 -translate-x-1/2">Putovanje kroz vreme</p>
        <YearTimeline activeYear={activeYear} setActiveYear={setActiveYear} ref={timelineRef} />
      </div>

      {/* 📜 Događaji za aktivnu godinu */}
      <div className="events-below w-[calc(100%-40px)] mx-[20px] grid grid-cols-3 gap-4 mt-10 min-h-[130px] fixed top-80">
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
