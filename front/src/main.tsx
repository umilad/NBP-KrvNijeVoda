import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <div className="bg-red-500 text-white p-6 text-xl">
      Tailwind CSS is working!
    </div>
    <App />
  </StrictMode>,
)
