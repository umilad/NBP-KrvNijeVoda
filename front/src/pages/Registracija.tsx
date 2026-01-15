import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "./AuthContext";


function validateRegister(
  username: string,
  password: string,
  firstName: string,
  lastName: string
) {
  const errors: Record<string, string> = {};

  if (firstName.trim().length < 2)
    errors.firstName = "Ime mora imati najmanje 2 karaktera";

  if (lastName.trim().length < 2)
    errors.lastName = "Prezime mora imati najmanje 2 karaktera";

  if (username.trim().length < 5)
    errors.username = "Korisničko ime mora imati najmanje 5 karaktera";

  if (password.length < 6)
    errors.password = "Lozinka mora imati najmanje 6 karaktera";

  if (!/[A-Za-z]/.test(password) || !/\d/.test(password))
    errors.password = "Lozinka mora sadržati bar jedno slovo i jedan broj";

  return errors;
}

export default function Registracija() {
  const [showPassword, setShowPassword] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const navigate = useNavigate();
  const { login } = useAuth();

  const errors = validateRegister(username, password, firstName, lastName);
  const isValid = Object.keys(errors).length === 0;

 
  const handleLogin = async (username: string, password: string) => {
    const response = await axios.post(
      "http://localhost:5210/api/Auth/login",
      {
        Username: username,
        Password: password,
        CustomClaims: { Role: "user" },
      }
    );

    const token = response.data.token;
    const role = response.data.role || "user";

    login(username, token, role);
    navigate("/");
  };

  const handleRegister = async () => {
    setSubmitted(true);

    if (!isValid) return; 

    const registerData = {
      Username: username.trim(),
      Password: password,
      FirstName: firstName.trim(),
      LastName: lastName.trim(),
      CustomClaims: { Role: "admin" }, 
    };

    try {
      setLoading(true);

      await axios.post(
        "http://localhost:5210/api/Auth/register",
        registerData,
        { headers: { "Content-Type": "application/json" } }
      );

      await handleLogin(username, password);
    } catch (err: any) {
      alert(err.response?.data || "Registracija nije uspela");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login my-[180px] w-full flex justify-center">
      <div className="pozadinaForme flex flex-col items-center justify-center relative w-1/3 border-2 border-[#3f2b0a] bg-[#e6cda5] p-[20px] rounded-lg text-center text-[#3f2b0a]">

        <p className="text-2xl font-bold mb-[15px]">Registracija</p>

        <form
          className="w-full flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault();
            handleRegister();
          }}
        >
        
          <input
            type="text"
            placeholder="Ime"
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
          />
          {submitted && errors.firstName && (
            <p className="text-red-600 text-xs mt-1">{errors.firstName}</p>
          )}

         
          <input
            type="text"
            placeholder="Prezime"
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
          />
          {submitted && errors.lastName && (
            <p className="text-red-600 text-xs mt-1">{errors.lastName}</p>
          )}

          
          <input
            type="text"
            placeholder="Korisničko ime ili email"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            className="p-[6px] rounded-[3px] border border-[#3f2b0a]"
          />
          {submitted && errors.username && (
            <p className="text-red-600 text-xs mt-1">{errors.username}</p>
          )}

          
          <div className="relative">
            <input
              type={showPassword ? "text" : "password"}
              placeholder="Lozinka"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="p-[6px] rounded-[3px] border border-[#3f2b0a] w-full pr-10"
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-2 top-1/2 -translate-y-1/2"
            >
              {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </div>
          {submitted && errors.password && (
            <p className="text-red-600 text-xs mt-1">{errors.password}</p>
          )}

          <button
            type="submit"
            disabled={loading}
            className="bg-[#3f2b0a] text-[#e6cda5] p-[6px] mb-[15px] rounded-[3px] hover:bg-[#2b1d07] transition disabled:opacity-50"
          >
            {loading ? "Registracija..." : "Registruj se"}
          </button>
        </form>

        <p className="mt-4 text-sm">
          Imaš nalog?{" "}
          <Link to="/prijava" className="font-bold underline">
            Prijavi se
          </Link>
        </p>
      </div>
    </div>
  );
}
