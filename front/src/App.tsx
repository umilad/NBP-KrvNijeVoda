import { Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar.tsx';
import Home from './pages/Home.tsx';
import Licnosti from './pages/Licnosti.tsx';
import Licnost from './pages/Licnost.tsx';
import Dogadjaji from './pages/Dogadjaji.tsx';
import Dogadjaj from './pages/Dogadjaj.tsx';
import DodajDogadjaj from './pages/DogadjajPages/DodajDogadjaj.tsx';
import Dinastije from './pages/Dinastije.tsx';
import Dinastija from './pages/Dinastija.tsx';
import DodajDinastiju from './pages/DinastijaPages/DodajDinastiju.tsx';
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
            <Route path="/" element={<Home />} />
            <Route path="/licnosti" element={<Licnosti />} />
            <Route path="/licnost/:id" element={<Licnost />} />
            <Route path="/dogadjaji" element={<Dogadjaji />} />
            <Route path="/dogadjaj/:id" element={<Dogadjaj />} />
            <Route path="/dodaj-dogadjaj" element={<DodajDogadjaj />} />
            <Route path="/dinastije" element={<Dinastije />} />
            <Route path="/dinastija/:id" element={<Dinastija />} />
            <Route path="/dodaj-dinastiju" element={<DodajDinastiju />} />
            <Route path="/dodaj-licnost" element={<DodajLicnost />} /> 
            <Route path="/prijava" element={<Login />} />
            <Route path="/registracija" element={<Registracija />} />
            <Route path="/profil" element={<Profil />} />
          </Routes>
        </div>
      </SearchProvider>
    </AuthProvider>
  );
}

export default App;
