import YearTimeline from '../components/YearTimeline.tsx';

export default function Home() {
  return (
    <div className="home overflow-y-scroll no-scrollbar h-screen my-[100px]">
        <div className="my-[303px]">
          <p className="text-center text-2xl font-bold mb-[10px]">Putovanje kroz vreme</p>
          <YearTimeline />
        </div>
      
       
    </div>
  );

}

