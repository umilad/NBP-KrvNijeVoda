import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "./AuthContext";

export default function Login() {
  const [showPassword, setShowPassword] = useState(false);
  const [usernameInput, setUsernameInput] = useState("");
  const [passwordInput, setPasswordInput] = useState("");
  const [loading, setLoading] = useState(false); 
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleLogin = async () => {
    try {
      setLoading(true); 

      const response = await axios.post(
        "http://localhost:5210/api/Auth/login",
        {
          Username: usernameInput,
          Password: passwordInput,
          CustomClaims: { Role: "admin" }
        },
        {
          headers: { "Content-Type": "application/json" }
        }
      );

      const token = response.data.token;
      const role = response.data.role || "user";

      login(usernameInput, token, role);

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
    } finally {
      setLoading(false); 
    }
  };

  return (
    <div className="login my-[180px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a]">
        <p className="text-2xl font-bold mb-[15px]">Prijava</p>

        <form
          className="w-full flex flex-col gap-4"
          onSubmit={(e) => { e.preventDefault(); handleLogin(); }}
        >
          <input
            type="text"
            placeholder="Korisničko ime"
            value={usernameInput}
            onChange={(e) => setUsernameInput(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a] focus:outline-none"
          />

          <div className="relative">
            <input
              type={showPassword ? "text" : "password"}
              placeholder="Lozinka"
              value={passwordInput}
              onChange={(e) => setPasswordInput(e.target.value)}
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
            disabled={loading}
            className="bg-[#3f2b0a] text-[#e6cda5] p-[6px] mb-[15px] rounded-[3px] hover:bg-[#2b1d07] transition disabled:opacity-50"
          >
            {loading ? "Prijavljivanje..." : "Prijavi se"} 
          </button>
        </form>

        <p className="mt-4 text-sm">
          Nemaš nalog?{" "}
          <Link to="/registracija" className="font-bold underline cursor-pointer">
            Registruj se
          </Link>
        </p>
      </div>
    </div>
  );
}
