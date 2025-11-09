import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "./AuthContext";

export default function Registracija() {
  const [showPassword, setShowPassword] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");

  const navigate = useNavigate();
  const { login } = useAuth();

  const handleLogin = async (username: string, password: string) => {
    try {
      const response = await axios.post("http://localhost:5210/api/Auth/login", {
        Username: username,
        Password: password,
        CustomClaims: { Role: "user" }
      });

      const token = response.data.token;
      const role = response.data.role || "user"; // uzmi role iz odgovora ili default

      // Poziv globalnog login iz AuthContext sa 3 parametra
      login(username, token, role);

      navigate("/"); 
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        console.error("Axios login error:", err.response?.status, err.response?.data);
        alert(`Login failed: ${err.response?.data || err.message}`);
      } else {
        const error = err as Error;
        console.error(error.message);
        alert("Login failed: " + error.message);
      }
    }
  };

  const handleRegister = async () => {
    const registerData = {
      Username: username,
      Password: password,
      FirstName: firstName,
      LastName: lastName,
      CustomClaims: { Role: "admin" },
    };

    try {
      const response = await axios.post("http://localhost:5210/api/Auth/register", registerData, {
        headers: { "Content-Type": "application/json" },
      });

      console.log("Registration successful", response.data);

      // odmah login nakon registracije
      await handleLogin(username, password);
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        console.error("Axios registration error:", err.response?.status, err.response?.data);
        alert(`Registration failed: ${err.response?.data || err.message}`);
      } else {
        const error = err as Error;
        console.error(error.message);
        alert("Registration failed: " + error.message);
      }
    }
  };

  return (
    <div className="login my-[180px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a]">
        <p className="text-2xl font-bold mb-[15px]">Registracija</p>

        <form className="w-full flex flex-col gap-4" onSubmit={(e) => { e.preventDefault(); handleRegister(); }}>
          <input
            type="text"
            placeholder="Ime"
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />
          <input
            type="text"
            placeholder="Prezime"
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />
          <input
            type="text"
            placeholder="Korisničko ime ili email"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />

          <div className="relative">
            <input
              type={showPassword ? "text" : "password"}
              placeholder="Lozinka"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none w-full pr-10"
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-2 top-1/2 -translate-y-1/2 text-[#3f2b0a] hover:opacity-70"
            >
              {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </div>

          <button
            type="submit"
            className="bg-[#3f2b0a] text-[#e6cda5] p-[6px] mb-[15px] rounded-[3px] hover:bg-[#2b1d07] transition"
          >
            Registruj se
          </button>
        </form>

        <p className="mt-4 text-sm">
          Imaš nalog? <Link to="/prijava" className="font-bold underline cursor-pointer">Prijavi se</Link>
        </p>
      </div>
    </div>
  );
}
