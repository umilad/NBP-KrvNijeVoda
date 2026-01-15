import { useState, useEffect, useRef } from "react";
import axios from "axios";

type PageStat = {
  path: string;
  count: number;
  label?: string;
};

function Carousel({ title, pages }: { title: string; pages: PageStat[] }) {
  const scrollRef = useRef<HTMLDivElement>(null);

  const scrollLeft = () => {
    scrollRef.current?.scrollBy({ left: -300, behavior: "smooth" });
  };

  const scrollRight = () => {
    scrollRef.current?.scrollBy({ left: 300, behavior: "smooth" });
  };

  if (!pages || pages.length === 0) return null;

  return (
    <div className="mb-20">
      <p className="text-3xl font-bold text-center mb-6 text-[#3f2b0a] drop-shadow-md">
        {title}
      </p>

      <div className="w-full relative overflow-hidden h-[240px] px-16 flex items-center">
        {/* ◀ */}
        <button
          onClick={scrollLeft}
          className="bg-[#E6CDA5] hover:bg-[#3f2b0a] hover:text-[#d6b889]
                     text-[#3f2b0a] text-[40px] w-8 h-8 rounded-full
                     shadow-lg flex items-center justify-center
                     transition-transform duration-300 hover:scale-110"
        >
          ‹
        </button>

        <div
          ref={scrollRef}
          className="flex gap-6 overflow-x-auto scroll-smooth no-scrollbar
                     h-[200px] relative pt-4 pb-4 flex-1"
        >
          {pages.slice(0, 15).map((page) => {
            const label = page.label || page.path;
            return (
              <div
                key={page.path}
                className="flex flex-col text-lg min-w-[180px] font-semibold
                           border-2 border-[#3f2b0a] bg-[#e6cda5]/70 p-[20px]
                           text-[#3f2b0a] rounded-lg text-center items-center
                           justify-center relative shadow-md
                           transition-transform hover:scale-110 cursor-pointer"
                onClick={() => window.location.assign(page.path)}
              >
                <p className="text-center text-lg font-semibold mb-2 break-words">
                  {label}
                </p>
                <p className="absolute bottom-2 right-2 text-[13px] px-2 py-0.5 bg-white/80 rounded">
                  {page.count} pregleda
                </p>
              </div>
            );
          })}
        </div>

        <button
          onClick={scrollRight}
          className="bg-[#E6CDA5] hover:bg-[#3f2b0a] hover:text-[#d6b889]
                     text-[#3f2b0a] text-[40px] w-8 h-8 rounded-full
                     shadow-lg flex items-center justify-center
                     transition-transform duration-300 hover:scale-110"
        >
          ›
        </button>
      </div>
    </div>
  );
}

export default function Home() {
  const [allTime, setAllTime] = useState<PageStat[]>([]);
  const [dogadjaji, setDogadjaji] = useState<PageStat[]>([]);
  const [licnosti, setLicnosti] = useState<PageStat[]>([]);
  const [dinastije, setDinastije] = useState<PageStat[]>([]);

  useEffect(() => {
    const token = localStorage.getItem("token");
    const headers = { Authorization: `Bearer ${token}` };

    async function load() {
      try {
        const [allRes, dogRes, licRes, dinRes] = await Promise.all([
          axios.get<PageStat[]>("http://localhost:5210/api/auth/global-top-pages", { headers }),
          axios.get<PageStat[]>("http://localhost:5210/api/auth/global-top-dogadjaji", { headers }),
          axios.get<PageStat[]>("http://localhost:5210/api/auth/global-top-licnosti", { headers }),
          axios.get<PageStat[]>("http://localhost:5210/api/auth/global-top-dinastije", { headers }),
        ]);

        setAllTime(allRes.data);
        setDogadjaji(dogRes.data);
        setLicnosti(licRes.data);
        setDinastije(dinRes.data);
      } catch (err) {
        console.error("Greška pri učitavanju statistike:", err);
      }
    }

    load();
  }, []);

  return (
    <div className="home overflow-y-scroll no-scrollbar h-screen my-[100px] px-4">
      <Carousel title="Najposećenije svih vremena" pages={allTime} />
      <Carousel title="Najposećeniji događaji" pages={dogadjaji} />
      <Carousel title="Najposećenije ličnosti" pages={licnosti} />
      <Carousel title="Najposećenije dinastije" pages={dinastije} />
    </div>
  );
}
