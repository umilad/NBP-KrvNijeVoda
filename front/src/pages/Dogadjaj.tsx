import axios from 'axios';
import { useEffect, useState, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { useAuth } from './AuthContext';
import type { Dogadjaj, Rat, Bitka } from "../types";
import DogadjajPrikaz from '../components/DogadjajPrikaz';

export default function Dogadjaj() { 
    const [dogadjaj, setDogadjaj] = useState<Dogadjaj | null>(null);
    const { tip, id } = useParams();
    const { token } = useAuth(); 
    const hasTracked = useRef(false);

    useEffect(() => {
        async function loadDogadjaj() {
            if (!id || !tip) return;
            try {
                let response;
                switch (tip){
                    case "Rat":
                        response = await axios.get<Rat>(`http://localhost:5210/api/GetRat/${id}`);
                        break;
                    case "Bitka":
                        response = await axios.get<Bitka>(`http://localhost:5210/api/GetBitka/${id}`);
                        break;
                    default:
                        response = await axios.get<Dogadjaj>(`http://localhost:5210/api/GetDogadjaj/${id}`);
                        break;                    
                }
                setDogadjaj(response.data);

                if (token && !hasTracked.current) {
                    hasTracked.current = true;

                    const path = `/dogadjaj/${tip}/${id}`;

                    await axios.post(
                    "http://localhost:5210/api/Auth/track",
                    { path, label: response.data.ime || "" },
                    { headers: { Authorization: `Bearer ${token}` } }
                    );

                    await axios.post(
                    "http://localhost:5210/api/Auth/track-visit",
                    { path, label: response.data.ime || "" },
                    { headers: { Authorization: `Bearer ${token}` } }
                    );
                }
            } catch (err) {
                console.error(err);
            }
        }

        loadDogadjaj();
    }, [id, tip, token]);

    return (
        <div className="dogadjaj-container flex flex-col items-center justify-center text-white"> 
            {dogadjaj && 
                <DogadjajPrikaz key={dogadjaj?.id}
                                dogadjaj={dogadjaj}
                                variant="full" />
            }
        </div>
    );
}
