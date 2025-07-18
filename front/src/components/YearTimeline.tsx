import { useRef, useEffect, useState } from "react";

const years = Array.from({ length: 2025 }, (_, i) => 1 + i);

export default function YearTimeline() {
  const containerRef = useRef<HTMLDivElement>(null);
  const isDragging = useRef(false);
  const scrollStart = useRef(0);
  const [activeIndex, setActiveIndex] = useState<number | null>(null);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const handleScroll = () => {
      const yearEls = Array.from(container.querySelectorAll(".year-tick")) as HTMLElement[];
      let closestIndex = 0;
      let closestDistance = Infinity;

      yearEls.forEach((el, index) => {
        const rect = el.getBoundingClientRect();
        const center = rect.left + rect.width / 2;
        const screenCenter = window.innerWidth / 2;
        const distance = Math.abs(center - screenCenter);

        if (distance < closestDistance) {
          closestDistance = distance;
          closestIndex = index;
        }
      });

      setActiveIndex(closestIndex);
    };

    const handleMouseDown = () => {
      isDragging.current = true;
      scrollStart.current = container.scrollLeft;
      document.body.style.cursor = "grabbing";
    };

    const handleMouseMove = (e: MouseEvent) => {
      if (!isDragging.current || !containerRef.current) return;
      const container = containerRef.current;

      const screenCenter = window.innerWidth / 2;
      const distanceFromCenter = e.pageX - screenCenter;

      const speedMultiplier = 0.3; // Tune this value
      const scrollChange = distanceFromCenter * speedMultiplier;

      container.scrollLeft = scrollStart.current + scrollChange;
    };

    const handleMouseUp = () => {
      isDragging.current = false;
      document.body.style.cursor = "default";
    };

    const dot = document.getElementById("center-dot");
    if (dot) dot.addEventListener("mousedown", handleMouseDown);
    window.addEventListener("mousemove", handleMouseMove);
    window.addEventListener("mouseup", handleMouseUp);

    container.addEventListener("scroll", handleScroll, { passive: true });
    window.addEventListener("resize", handleScroll);
    handleScroll();

    return () => {
      if (dot) dot.removeEventListener("mousedown", handleMouseDown);
      window.removeEventListener("mousemove", handleMouseMove);
      window.removeEventListener("mouseup", handleMouseUp);
      container.removeEventListener("scroll", handleScroll);
      window.removeEventListener("resize", handleScroll);
    };
  }, []);

  return (
    <div className="relative w-full py-16 overflow-hidden">
      {/* Center dot */}
      <div
        id="center-dot"
        className="absolute left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2 z-20 cursor-grab"
      >
        <div className="w-4 h-4 bg-black rounded-full" />
      </div>

      {/* Timeline */}
      <div
        ref={containerRef}
        className="overflow-x-auto no-scrollbar relative flex px-20 snap-x snap-mandatory scroll-smooth"
      >
        {/* Line */}
        <div className="absolute top-1/2 left-0 right-0 h-[4px] bg-black z-0" style={{ minWidth: `${years.length * 50}px` }} />

        {/* Spacer before first year */}
        <div className="w-[50vw] shrink-0" />

        {years.map((year, i) => {
          const isActive = i === activeIndex;
          const yearSize = isActive ? "text-3xl" : "text-base";
          const spacing = isActive ? "mx-[20px]" : "mx-[10px]";
          const showLabel = isActive || year % 5 === 0;
          const showTick = isActive || year % 5 === 0;

          return (
            <div
              key={year}
              className={`year-tick flex flex-col items-center justify-center w-auto shrink-0 snap-center relative ${spacing}`}
            >
              <div className={`mt-[29px] h-6 w-[3px] bg-black z-10 ${showTick ? "" : "hidden"}`} />
              {showLabel && (
                <div
                  className={`mt-4 transition-all duration-300 font-semibold ${yearSize} ${
                    isActive ? "text-black" : "text-gray-600"
                  }`}
                >
                  {year}
                </div>
              )}
            </div>
          );
        })}

        {/* Spacer after last year */}
        <div className="w-[50vw] shrink-0" />
      </div>
    </div>
  );
}
