export default interface YearTimelineProps {
  activeYear: number;
  setActiveYear: (year:number) => void;
  scrollToYear?: (year: number) => void;
}