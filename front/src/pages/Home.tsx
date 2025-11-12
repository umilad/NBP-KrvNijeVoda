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
  const timelineRef = useRef<YearTimelineHandle>(null); // could use proper type for forwardRef
  const [activeYear, setActiveYear] = useState<number>(1);
  const [allActiveYearEvents, setAllActiveYearEvents] = useState<AllEventsForGodinaResponse| null>(null);
  //const [godine, setGodine] = useState<Godina[]>([]);
  const { query } = useSearch();

  useEffect(() => {

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
        <div className="events-above w-[calc(100%-200px)] mx-[20px] flex justify-around flex-wrap gap-2">
          {allActiveYearEvents ? (
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
          ) : (
            <></>
          )}
        </div>

        <div className="relative min-h-[303px]"> {/**my-[303px]  */}
          <p className="text-center text-2xl font-bold mb-[10px] ">Putovanje kroz vreme</p>

          <Searchbar onSearch={handleSearch} />

          <YearTimeline activeYear={activeYear} setActiveYear={setActiveYear} ref={timelineRef} />
        </div>


        {/* Show events for the active year */}
        <div className="events-below w-[calc(100%-40px)] mx-[20px] grid grid-cols-3 gap-2 mt-[40px] min-h-[130px]">
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

