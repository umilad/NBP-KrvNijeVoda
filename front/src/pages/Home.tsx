import { useState, useEffect, useRef } from "react";
import YearTimeline from '../components/YearTimeline.tsx';
import type { YearTimelineHandle } from '../components/YearTimeline.tsx';
import DinastijaPrikaz from "../components/DinastijaPrikaz";
import type { AllEventsForGodinaResponse, Godina } from "../types";
import { useSearch } from "../components/SearchContext";
import axios from "axios";
import Searchbar from "../components/Searchbar";

export default function Home() {
  const timelineRef = useRef<YearTimelineHandle>(null); // could use proper type for forwardRef
  const [activeYear, setActiveYear] = useState<number>(0);
  const [allActiveYearEvents, setAllActiveYearEvents] = useState<AllEventsForGodinaResponse| null>(null);
  const [godine, setGodine] = useState<Godina[]>([]);
  const { query } = useSearch();

  useEffect(() => {
    async function GetAllGodine(){
      try {
        const response = await axios.get<Godina[]>(`http://localhost:5210/api/GetAllGodine`);
        return response.data;
      } catch (error) {
          console.error("Error fetching godine:", error);
          return [];
      }
    }

    async function loadAllGodine(){
      const data = await GetAllGodine();
      setGodine(data);
    }

    loadAllGodine();

    async function GetAllEventsForGodina(){
      try {
        if(!activeYear) return;
        const response = await axios.get<AllEventsForGodinaResponse>(`http://localhost:5210/api/GetAllEventsForGodina/${activeYear}`);
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
    if(timelineRef.current){
      timelineRef.current.scrollToYear(queryNumber);
    }
  };

  return (
    <div className="home overflow-y-scroll no-scrollbar h-screen my-[100px]">
        <div className="my-[303px]">
          <p className="text-center text-2xl font-bold mb-[10px]">Putovanje kroz vreme</p>

          <Searchbar onSearch={handleSearch} />

          <YearTimeline activeYear={activeYear} setActiveYear={setActiveYear} ref={timelineRef} />
        </div>


        {/* Show events for the active year */}
        <div className="events mt-10 px-10">
          {allActiveYearEvents ? (
            <>
              {allActiveYearEvents.dogadjaji.map(d => <p key={d.id}>{d.ime}</p>)}
              {allActiveYearEvents.bitke.map(b => <p key={b.id}>{b.ime} (bitka)</p>)}
              {allActiveYearEvents.ratovi.map(r => <p key={r.id}>{r.ime} (rat)</p>)}
              {allActiveYearEvents.vladari.map(v => <p key={v.id}>{v.ime}</p>)}
              {allActiveYearEvents.licnosti.map(l => <p key={l.id}>{l.ime}</p>)}
              {allActiveYearEvents.dinastije.map(d => (
                <DinastijaPrikaz key={d.id} dinastija={d} />
              ))}
            </>
          ) : (
            <p>Loading events...</p>
          )}
        </div>
      
       
    </div>
  );

}

