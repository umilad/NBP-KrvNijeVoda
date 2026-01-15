import { useState, useEffect, useRef } from "react";
import axios from "axios";

export default function Home() {
  const scrollRef = useRef<HTMLDivElement>(null); // for carousel scroll
  const [topPages, setTopPages] = useState<{ path: string; count: number; label?: string }[]>([]);

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

  // 🌟 Scroll carousel left/right
  const scrollLeft = () => {
    scrollRef.current?.scrollBy({ left: -300, behavior: "smooth" });
  };
  const scrollRight = () => {
    scrollRef.current?.scrollBy({ left: 300, behavior: "smooth" });
  };

  return (
    <div className="home overflow-y-scroll no-scrollbar h-screen my-[100px]">
      {/* Naslov */}
      <p className="text-3xl font-bold text-center mb-6 text-[#3f2b0a] drop-shadow-md fixed left-10 top-15">
        Najposećenije:
      </p>
      {/* 🔥 Top Pages Carousel */}
      {topPages.length > 0 && (
        <div className="w-full relative overflow-hidden mb-16 h-[240px] px-16 flex items-center">
          {/* Strelica levo */}
          <button
            onClick={scrollLeft}
            className="bg-[#E6CDA5] hover:bg-[#3f2b0a] hover:text-[#d6b889] text-[#3f2b0a] text-[40px] pb-[11px] w-8 h-8 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 ml-[4px] mr-[4px]"
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
                  className="dogadjaj-div flex flex-col text-lg min-w-[180px] font-semibold border-2 border-[#3f2b0a] bg-[#e6cda5]/70 p-[20px] text-[#3f2b0a] rounded-lg text-center items-center justify-center relative mt-[10px] mb-[10px] shadow-md transition-transform hover:scale-110 cursor-pointer"
                  onClick={() => window.location.assign(path)}
                >
                  <p className="text-center text-lg font-semibold mb-2 break-words">{label}</p>
                  <p className="absolute bottom-2 right-2 text-[13px] tracking-wide text-[#3f2b0a] px-2 py-0.5 rounded">{page.count} pregleda</p>
                </div>
              );
            })}
          </div>

          {/* Strelica desno */}
          <button
            onClick={scrollRight}
            className="bg-[#E6CDA5] hover:bg-[#3f2b0a] hover:text-[#d6b889] text-[#3f2b0a] text-[40px] pb-[11px] w-8 h-8 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 ml-[4px] mr-[4px]"
          >
            ›
          </button>
        </div>
      )}
    </div>
  );
}
