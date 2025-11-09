import { NavLink } from 'react-router-dom';
import Searchbar from './Searchbar.tsx';

export default function Navbar() {
  return (
    <div className="fixed top-0 left-0 w-full z-50">        
        <nav className="relative bg-[#3f2b0af0] h-12 z-20 text-white">
            <div className="flex w-full h-full items-center ">
                <ul className="flex flex-grow h-full">
                <li className="flex-1 text-center">
                    <NavLink 
                    to="/"                      
                    className={({isActive}) =>
                        `flex items-center justify-center w-full h-full 
                        ${isActive ? "bg-[#e6cda5f0] text-[#3f2b0af0]" : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"}`
                    }
                    >
                    Početna
                    </NavLink>
                </li>

                <li className="flex-1 text-center">
                    <NavLink 
                    to="/licnosti"
                    className={({isActive}) =>
                        `flex items-center justify-center w-full h-full 
                        ${isActive ? "bg-[#e6cda5f0] text-[#3f2b0af0]" : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"}`
                    }
                    >
                    Ličnosti
                    </NavLink>
                </li>

                <li className="flex-1 text-center">
                    <NavLink 
                    to="/dogadjaji"
                    className={({isActive}) =>
                        `flex items-center justify-center w-full h-full 
                        ${isActive ? "bg-[#e6cda5f0] text-[#3f2b0af0]" : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"}`
                    }
                    >
                    Događaji
                    </NavLink>
                </li>

                <li className="flex-1 text-center">
                    <NavLink 
                    to="/dinastije"
                    className={({isActive}) =>
                        `flex items-center justify-center w-full h-full 
                        ${isActive ? "bg-[#e6cda5f0] text-[#3f2b0af0]" : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"}`
                    }
                    >
                    Dinastije
                    </NavLink>
                </li>
                </ul>

                <div className="relative start-1.5 w-55 h-8 rounded-full border border-[#e6cda5f0] rounded-l-md">
                <div className="absolute inset-y-0 start-2 flex items-center ps-3 pointer-events-none">
                    <svg className="w-4 h-4 text-[#e6cda5f0]" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 20 20">
                        <path stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="m19 19-4-4m0-7A7 7 0 1 1 1 8a7 7 0 0 1 14 0Z"/>
                    </svg>
                </div>
                <Searchbar />
                </div>

                <div className="relative h-full w-23">
                <NavLink 
                    to="/prijava" //rounded-full sam sklonila
                    className={({isActive}) =>
                        `absolute start-4 h-full w-20 flex items-center justify-center
                        ${isActive ? "bg-[#e6cda5f0] text-[#3f2b0af0]" : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"}`
                    }>
                    Prijavi se 
                </NavLink>
                </div>

            </div>
        </nav>
    </div>
  );
}

