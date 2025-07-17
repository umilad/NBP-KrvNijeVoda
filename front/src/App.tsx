import { Routes, Route } from 'react-router-dom';
import { Link } from 'react-router-dom';
import Home from './pages/Home.tsx';
import Licnosti from './pages/Licnosti.tsx';
import Dogadjaji from './pages/Dogadjaji.tsx';
import Dinastije from './pages/Dinastije.tsx';

import './App.css'
/*
        <div className="bg-[#e6cda5f0] h-12 z-20  flex items-center justify-center">
          <h1 className="text-4xl text-center text-[#3f2b0af0] font-bold mt-10">KRV NIJE VODA</h1>
        </div>
        */
function App() {

  return (
    <>  
      <div className="relative">
        <img 
          src="/background.jpg" 
          alt="background" 
          className="fixed top-0 left-0 w-screen h-screen object-cover -z-10 opacity-50 pointer-events-none" 
          style={{ transform: 'scale(1)', transformOrigin: 'top left' }}
        />


        <Navbar />

        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/licnosti" element={<Licnosti />} />
          <Route path="/dogadjaji" element={<Dogadjaji />} />
          <Route path="/dinastije" element={<Dinastije />} />
        </Routes>
        <div className=" pt-96 mt-96"> FNJMDABAsvr</div>
        
      </div>
    </>
   
  )
}

function Navbar() {
  return (
    <nav className="relative bg-[#3f2b0af0] h-12 z-20 text-white">
    <div className="flex w-full h-full items-center">
        <ul className="flex flex-grow h-full">
          <li className="flex-1 text-center">
            <Link 
              to="/"
              className="flex items-center justify-center w-full h-full hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"
            >
              Početna
            </Link>
          </li>
          <li className="flex-1 text-center">
            <Link 
              to="/licnosti"
              className="flex items-center justify-center w-full h-full hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"
            >
              Ličnosti
            </Link>
          </li>
          <li className="flex-1 text-center">
            <Link 
              to="/dogadjaji"
              className="flex items-center justify-center w-full h-full hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"
            >
              Događaji
            </Link>
          </li>
          <li className="flex-1 text-center">
            <Link 
              to="/dinastije"
              className="flex items-center justify-center w-full h-full hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"
            >
              Dinastije
            </Link>
          </li>
        </ul>


        
          

        <div className="relative start-1.5 w-55 h-8 rounded-full border border-[#e6cda5f0] rounded-l-md">
          <div className="absolute inset-y-0 start-2 flex items-center ps-3 pointer-events-none">
            <svg className="w-4 h-4 text-[#e6cda5f0]" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 20 20">
                <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m19 19-4-4m0-7A7 7 0 1 1 1 8a7 7 0 0 1 14 0Z"/>
            </svg>
          </div>
          <input 
              type="search"
              className="absolute start-8 w-50 h-full px-4 py-2 border-none focus:outline-none "
              placeholder="Pretraži..."
            />
        </div>



          

       

        <div className="relative h-full w-20">
          <Link 
            to="/login"
            className="absolute start-4 h-full w-15 flex items-center justify-center rounded-full bg-[#e6cda5f0] text-[#3f2b0af0] px-4 py-2 hover:bg-[#3f2b0af0] hover:text-white transition duration-300"
          >
            Log in 
          </Link>
        </div>
        


    </div>
    </nav>


  );
}

export default App; 

