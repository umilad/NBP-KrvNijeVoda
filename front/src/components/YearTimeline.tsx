// components/YearTimeline.tsx
import {
  useRef,
  useImperativeHandle,
  forwardRef,
  useEffect,
  useState,
} from "react";

const years = Array.from({ length: 2025 }, (_, i) => 1 + i);

interface YearTimelineProps {
  activeYear: number;
  setActiveYear: (year: number) => void;
}

export interface YearTimelineHandle {
  scrollToYear: (year: number) => void;
}

const YearTimeline = forwardRef<YearTimelineHandle, YearTimelineProps>(
  function YearTimeline({ setActiveYear }, ref) {
    const containerRef = useRef<HTMLDivElement | null>(null);
    const isDragging = useRef(false);
    const scrollStart = useRef(0);
    const [activeIndex, setActiveIndex] = useState<number>(0);

    // Flag to prevent scroll handler from interfering with programmatic scroll
    const isProgrammaticScroll = useRef(false);

    // Expose scrollToYear to parent
    useImperativeHandle(ref, () => ({
      scrollToYear(year: number) {
        const container = containerRef.current;
        if (!container) return;

        const index = years.findIndex((y) => y === year);
        if (index === -1) return;

        const yearEls = container.querySelectorAll(".year-tick");
        const targetEl = yearEls[index] as HTMLElement;
        if (!targetEl) return;

        // Center target element
        const targetCenter = targetEl.offsetLeft + targetEl.offsetWidth / 2;
        const scrollTo = targetCenter - container.clientWidth / 2;

        isProgrammaticScroll.current = true;
        container.scrollTo({ left: scrollTo, behavior: "smooth" });

        // Update state to mark active year
        setActiveIndex(index);
        setActiveYear(years[index]);

        // Release programmatic scroll flag after scroll finishes
        setTimeout(() => {
          isProgrammaticScroll.current = false;
        }, 700);
      },
    }));

    useEffect(() => {
      const container = containerRef.current;
      if (!container) return;

      const handleScroll = () => {
        if (isProgrammaticScroll.current) return;

        const yearEls = Array.from(container.querySelectorAll(".year-tick")) as HTMLElement[];
        let closestIndex = 0;
        let closestDistance = Infinity;

        const containerCenter = container.getBoundingClientRect().left + container.clientWidth / 2;

        yearEls.forEach((el, index) => {
          const rect = el.getBoundingClientRect();
          const elCenter = rect.left + rect.width / 2;
          const distance = Math.abs(elCenter - containerCenter);
          if (distance < closestDistance) {
            closestDistance = distance;
            closestIndex = index;
          }
        });

        if (closestIndex !== activeIndex) {
          setActiveIndex(closestIndex);
          setActiveYear(years[closestIndex]);
        }
      };

      const handleMouseDown = () => {
        isDragging.current = true;
        scrollStart.current = container.scrollLeft;
        document.body.style.cursor = "grabbing";
      };

      const handleMouseMove = (e: MouseEvent) => {
        if (!isDragging.current || !containerRef.current) return;
        const screenCenter = window.innerWidth / 2;
        const distanceFromCenter = e.pageX - screenCenter;
        const speedMultiplier = 0.8;
        container.scrollLeft = scrollStart.current + distanceFromCenter * speedMultiplier;
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

      handleScroll(); // initial highlight

      return () => {
        if (dot) dot.removeEventListener("mousedown", handleMouseDown);
        window.removeEventListener("mousemove", handleMouseMove);
        window.removeEventListener("mouseup", handleMouseUp);
        container.removeEventListener("scroll", handleScroll);
        window.removeEventListener("resize", handleScroll);
      };
    }, [activeIndex, setActiveYear]);

    return (
      <div className="relative w-full py-16 overflow-hidden">
        {/* Center dot */}
        <div
          id="center-dot"
          className="absolute left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2 z-20 cursor-grab"
        >
          <div className="w-4 h-4 bg-black rounded-full" />
        </div>

        {/* Timeline container */}
        <div
          ref={containerRef}
          className="overflow-x-auto no-scrollbar relative flex px-20 snap-x snap-mandatory scroll-smooth"
        >
          {/* Timeline line */}
          <div
            className="absolute top-1/2 left-0 right-0 h-[4px] bg-black z-0"
            style={{ minWidth: `${years.length * 50}px` }}
          />

          {/* Spacer before first year */}
          <div className="w-[50vw] shrink-0" />

          {/* Year ticks */}
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
                  <div className={`mt-4 transition-all duration-300 font-semibold ${yearSize} ${isActive ? "text-black" : "text-gray-600"}`}>
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
);

export default YearTimeline;
