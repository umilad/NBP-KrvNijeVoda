import { Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar.tsx';
import Home from './pages/Home.tsx';
import Licnosti from './pages/Licnosti.tsx';
import Licnost from './pages/Licnost.tsx';
import Dogadjaji from './pages/Dogadjaji.tsx';
import Dogadjaj from './pages/Dogadjaj.tsx';
import Dinastije from './pages/Dinastije.tsx';
import Dinastija from './pages/Dinastija.tsx';
import Login from './pages/Login.tsx';
import Registracija from './pages/Registracija.tsx';

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
          <Route path="/licnost/:id" element={<Licnost />} />
          <Route path="/dogadjaji" element={<Dogadjaji />} />
          <Route path="/dogadjaj/:id" element={<Dogadjaj />} />
          <Route path="/dinastije" element={<Dinastije />} />
          <Route path="/dinastija/:id" element={<Dinastija />} />
          <Route path="/prijava" element={<Login/>} />
          <Route path="/registracija" element={<Registracija/>} />
          
        </Routes>
        
      </div>
    </>
   
  )
}


export default App; 

