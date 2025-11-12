// App.tsx
import { Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar.tsx';
import Home from './pages/Home.tsx';
import Licnosti from './pages/Licnosti.tsx';
import Licnost from './pages/Licnost.tsx';
import AzurirajLicnost from './pages/LicnostPages/AzurirajLicnost.tsx';
import Dogadjaji from './pages/Dogadjaji.tsx';
import Dogadjaj from './pages/Dogadjaj.tsx';
import DodajDogadjaj from './pages/DogadjajPages/DodajDogadjaj.tsx';
import AzurirajDogadjaj from './pages/DogadjajPages/AzurirajDogadjaj.tsx'; 
import Dinastije from './pages/Dinastije.tsx';
import Dinastija from './pages/Dinastija.tsx';
import DodajDinastiju from './pages/DinastijaPages/DodajDinastiju.tsx';
import AzurirajDinastiju from './pages/DinastijaPages/AzurirajDinastiju.tsx';
import DodajLicnost from './pages/LicnostPages/DodajLicnost.tsx';
import Login from './pages/Login.tsx';
import Registracija from './pages/Registracija.tsx';
import Profil from './pages/Profil.tsx';

import { AuthProvider } from './pages/AuthContext';
import { SearchProvider } from './components/SearchContext.tsx';

import './App.css';

function App() {
  return (
    <AuthProvider>
      <SearchProvider>
        <div className="relative">
          <img 
            src="/background.jpg" 
            alt="background" 
            className="fixed top-0 left-0 w-screen h-screen object-cover -z-10 opacity-50 pointer-events-none" 
            style={{ transform: 'scale(1)', transformOrigin: 'top left' }}
          />

          <Navbar />

          <Routes>
            {/* Početne stranice */}
            <Route path="/" element={<Home />} />
            <Route path="/licnosti" element={<Licnosti />} />
            <Route path="/licnost/:id" element={<Licnost />} />
            
            {/* Ažuriranje ličnosti / vladara */}
            <Route path="/licnost/edit/:id" element={<AzurirajLicnost />} />

            {/* Dodavanje ličnosti */}
            <Route path="/dodaj-licnost" element={<DodajLicnost />} /> 

            {/* Dogadjaji */}
            <Route path="/dogadjaji" element={<Dogadjaji />} />
            <Route path="/dogadjaj/:id" element={<Dogadjaj />} />
            <Route path="/dogadjaj/:tip/:id" element={<Dogadjaj />} />
            <Route path="/dogadjaj/edit/:id" element={<AzurirajDogadjaj />} />
            <Route path="/dodaj-dogadjaj" element={<DodajDogadjaj />} />

            {/* Dinastije */}
            <Route path="/dinastije" element={<Dinastije />} />
            <Route path="/dinastija/:id" element={<Dinastija />} />
            <Route path="/dinastija/edit/:id" element={<AzurirajDinastiju />} />
            <Route path="/dodaj-dinastiju" element={<DodajDinastiju />} />

            {/* Autentikacija */}
            <Route path="/prijava" element={<Login />} />
            <Route path="/registracija" element={<Registracija />} />
            <Route path="/profil" element={<Profil />} />

            {/* Fallback 404 */}
            <Route path="*" element={<div className="text-center mt-20 text-2xl">404 Not Found</div>} />
          </Routes>
        </div>
      </SearchProvider>
    </AuthProvider>
  );
}

export default App;
