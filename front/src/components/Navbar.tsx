import { NavLink, useLocation, useNavigate } from "react-router-dom";
import Searchbar from "./Searchbar.tsx";
import { useAuth } from "../pages/AuthContext";

export default function Navbar() {
  const { username, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/"); // preusmeri na početnu stranu
  };

  // Sakrij search bar na /profil stranici
  const hideSearch = location.pathname === "/profil";

  return (
    <div className="fixed top-0 left-0 w-full z-50">
      <nav className="relative bg-[#3f2b0af0] h-12 z-20 text-white flex items-center px-4">
        {/* Navigacija */}
        <ul className="flex flex-grow h-full">
          <li className="flex-1 text-center">
            <NavLink
              to="/"
              className={({ isActive }) =>
                `flex items-center justify-center w-full h-full ${
                  isActive
                    ? "bg-[#e6cda5f0] text-[#3f2b0af0]"
                    : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"
                }`
              }
            >
              Početna
            </NavLink>
          </li>
          <li className="flex-1 text-center">
            <NavLink
              to="/licnosti"
              className={({ isActive }) =>
                `flex items-center justify-center w-full h-full ${
                  isActive
                    ? "bg-[#e6cda5f0] text-[#3f2b0af0]"
                    : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"
                }`
              }
            >
              Ličnosti
            </NavLink>
          </li>
          <li className="flex-1 text-center">
            <NavLink
              to="/dogadjaji"
              className={({ isActive }) =>
                `flex items-center justify-center w-full h-full ${
                  isActive
                    ? "bg-[#e6cda5f0] text-[#3f2b0af0]"
                    : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"
                }`
              }
            >
              Događaji
            </NavLink>
          </li>
          <li className="flex-1 text-center">
            <NavLink
              to="/dinastije"
              className={({ isActive }) =>
                `flex items-center justify-center w-full h-full ${
                  isActive
                    ? "bg-[#e6cda5f0] text-[#3f2b0af0]"
                    : "hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition duration-300"
                }`
              }
            >
              Dinastije
            </NavLink>
          </li>
        </ul>

        {/* Search bar */}
        {!hideSearch && (
          <div className="relative w-56 h-8 rounded-full border border-[#e6cda5f0] flex items-center ml-4">
            <div className="absolute inset-y-0 left-2 flex items-center pointer-events-none">
              <svg
                className="w-4 h-4 text-[#e6cda5f0]"
                aria-hidden="true"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 20 20"
              >
                <path
                  stroke="currentColor"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth="2"
                  d="m19 19-4-4m0-7A7 7 0 1 1 1 8a7 7 0 0 1 14 0Z"
                />
              </svg>
            </div>
            <Searchbar />
          </div>
        )}

        {/* Profil i Logout dugmad */}
        <div className="ml-4 h-full flex items-center gap-2">
          {username ? (
            <>
             <NavLink
  to="/profil"
  className="px-4 h-10 flex items-center justify-center rounded-lg bg-[#3f2b0af0] text-[#e6cda5] font-bold text-lg hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition-colors duration-300 ease-in-out"
>
  Profil
</NavLink>
<button
  onClick={handleLogout}
  className="px-4 h-10 flex items-center justify-center rounded-lg bg-[#3f2b0af0] text-[#e6cda5] font-bold text-lg hover:bg-[#e6cda5f0] hover:text-[#3f2b0af0] transition-colors duration-300 ease-in-out"
>
  Logout
</button>
            </>
          ) : (
            <NavLink
              to="/prijava"
              className={({ isActive }) =>
                `px-4 h-10 flex items-center justify-center rounded-lg bg-[#3f2b0af0] text-[#e6cda5] font-bold text-lg hover:bg-[#e6cda5] hover:text-[#3f2b0af0] transition-all ${
                  isActive ? "underline" : ""
                }`
              }
            >
              Prijavi se
            </NavLink>
          )}
        </div>
      </nav>
    </div>
  );
}
