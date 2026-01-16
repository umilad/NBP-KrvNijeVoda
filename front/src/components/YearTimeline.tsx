
import {
  useRef,
  useImperativeHandle,
  forwardRef,
  useEffect,
  useState,
  useLayoutEffect,
} from "react";

const years = Array.from({ length: 2026 }, (_, i) => i + 1);

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
    const [activeIndex, setActiveIndex] = useState<number>(0);

    const isProgrammaticScroll = useRef(false);

    useImperativeHandle(ref, () => ({
      scrollToYear(year: number) {
        const container = containerRef.current;
        if (!container) return;

        const index = years.findIndex((y) => y === year);
        if (index === -1) return;

        const yearEls = container.querySelectorAll(".year-tick");
        const targetEl = yearEls[index] as HTMLElement;
        if (!targetEl) return;

        const targetCenter = targetEl.offsetLeft + targetEl.offsetWidth / 2;
        const scrollTo = targetCenter - container.clientWidth / 2;

        isProgrammaticScroll.current = true;
        container.scrollTo({ left: scrollTo, behavior: "smooth" });

        setActiveIndex(index);
        setActiveYear(years[index]);

        setTimeout(() => {
          isProgrammaticScroll.current = false;
        }, 700);
      },
    }));

    useLayoutEffect(() => {
      const container = containerRef.current;
      if (!container) return;

      const startYear = 2026;
      const index = years.findIndex(y => y === startYear);
      const yearEls = container.querySelectorAll(".year-tick");
      const targetEl = yearEls[index] as HTMLElement;
      if (!targetEl) return;

      const targetCenter = targetEl.offsetLeft + targetEl.offsetWidth / 2;
      container.scrollLeft = targetCenter - container.clientWidth / 2;

      setActiveIndex(index);
      setActiveYear(startYear);
    }, [setActiveYear]);

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


      container.addEventListener("scroll", handleScroll, { passive: true });
      window.addEventListener("resize", handleScroll);

      handleScroll();

      return () => {
        container.removeEventListener("scroll", handleScroll);
        window.removeEventListener("resize", handleScroll);
      };
    }, [activeIndex, setActiveYear]);

    return (
      <div className="fixed w-full py-16 overflow-hidden top-40">
        <div
          id="center-dot"
          className="absolute left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2 z-20 cursor-grab"
        >
          <div className="w-4 h-4 bg-[#3f2b0a] rounded-full" />
        </div>

        <div
          ref={containerRef}
          className="overflow-x-auto no-scrollbar relative flex snap-x snap-mandatory"
        >
          <div
            className="absolute top-1/2 left-0 right-0 h-[4px] bg-[#3f2b0a] z-0"
            style={{ minWidth: `${years.length * 50}px` }}
          />

          <div className="w-[49.5vw] shrink-0" />

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
                <div className={`mt-[29px] h-6 w-[3px] bg-[#3f2b0a] z-10 ${showTick ? "" : "hidden"}`} />
                {showLabel && (
                  <div className={`mt-4 transition-all duration-300 font-semibold ${yearSize} ${isActive ? "text-[#3f2b0a]" : "text-gray-600"}`}>
                    {year}.
                  </div>
                )}
              </div>
            );
          })}

          <div className="w-[49.5vw] shrink-0" />
        </div>
      </div>
    );
  }
);

export default YearTimeline;
