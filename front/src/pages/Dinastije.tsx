export default function Dinastije() {
    return (
        <div className="dinastije my-[120px]">
            <div className="pozadinaStabla flex flex-col items-center justify-center relative mx-[100px] p-[20px] border-2 border-[#3f2b0a] bg-[#e6cda5] p-4 rounded-lg text-center text-[#3f2b0a]">
                <p className="text-2xl font-bold">Naziv dinastije</p>
                <p className="text-xl font-bold mb-[10px]">(od-do)</p>
                <div className="stablo">
                    {/* Tree row - person container */}
                    <div className="relative flex justify-center mt-[60px]">
                        
                        {/* Jedna osoba u stablu */}
                        <div className="flex-col relative group block text-center text-[#3f2b0a] ">
                            {/* Vertical line from the title block to person AKO NEMA RODITELJ VEZU NEMA JE*/}
                            <div className="absolute top-[-36px] left-1/2 transform -translate-x-1/2 w-[2px] h-[40px] bg-[#3f2b0a] z-0" />
                            {/* HORIZONTAL line from the title block to person AKO JE ZENSO I IMA MUZA S LEVE STRANE*/}
                            <div className="absolute top-[54px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            {/* HORIZONTAL line from the title block to person AKO JE MUSKO I IMA ZENU S DESNE STRANE*/}
                            <div className="absolute top-[54px] translate-x-[121px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            {/* Vertical line from OD RODITELJA LINIJA ZA DECU al samo od muskog nek ide jer ako nema decu nema ni vezu sa zenom jer zenu dobijamo preko detetove veze s majkom */}
                            <div className="absolute -right-[1px] top-[55px] w-[2px] h-[130px] bg-[#3f2b0a] z-0" />

                            <div className="px-[4px] hover:scale-110 transition-transform duration-300">
                                <img
                                    src="/src/images/download.jpeg"
                                    alt="Historical Figure"
                                    className="w-[80px] h-[100px] object-cover mx-auto my-[4px] border-2 border-[#3f2b0a] rounded-lg"
                                />
                                <p className="text-lg font-bold mt-2">Titula Ime Prezime</p>
                                <p className="text-md mt-1">(od-do)</p>
                            </div>
                            
                            {/* Tooltip Info Box */}
                            <div className="absolute -top-10 -left-50 w-56 p-3 bg-[#e6cda5] text-[#3f2b0a] border border-[#3f2b0a] rounded-lg shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-300 z-20">
                                <p className="font-semibold">Dodatne informacije:</p>
                                <p>- Rođen: 1234</p>
                                <p>- Umro: 1299</p>
                                <p>- Bio poznat po: nečemu važnom</p>
                            </div>
                        </div>
                        {/* Jedna osoba u stablu */}
                        <div className="flex-col relative group block text-center text-[#3f2b0a] ">
                            {/* Vertical line from the title block to person AKO NEMA RODITELJ VEZU NEMA JE*/}
                            <div className="absolute top-[-36px] left-1/2 transform -translate-x-1/2 w-[2px] h-[40px] bg-[#3f2b0a] z-0" />
                            {/* HORIZONTAL line from the title block to person AKO JE ZENSO I IMA MUZA S LEVE STRANE*/}
                            <div className="absolute top-[54px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            {/* HORIZONTAL line from the title block to person AKO JE MUSKO I IMA ZENU S DESNE STRANE*/}
                            <div className="absolute top-[54px] translate-x-[121px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            {/* Vertical line from OD RODITELJA LINIJA ZA DECU al samo od muskog nek ide jer ako nema decu nema ni vezu sa zenom jer zenu dobijamo preko detetove veze s majkom */}
                            <div className="absolute -right-[1px] top-[55px] w-[2px] h-[130px] bg-[#3f2b0a] z-0" />

                            <div className="px-[4px] hover:scale-110 transition-transform duration-300">
                                <img
                                    src="/src/images/download.jpeg"
                                    alt="Historical Figure"
                                    className="w-[80px] h-[100px] object-cover mx-auto my-[4px] border-2 border-[#3f2b0a] rounded-lg"
                                />
                                <p className="text-lg font-bold mt-2">Titula Ime Prezime</p>
                                <p className="text-md mt-1">(od-do)</p>
                            </div>
                            
                            {/* Tooltip Info Box */}
                            <div className="absolute -top-10 -left-50 w-56 p-3 bg-[#e6cda5] text-[#3f2b0a] border border-[#3f2b0a] rounded-lg shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-300 z-20">
                                <p className="font-semibold">Dodatne informacije:</p>
                                <p>- Rođen: 1234</p>
                                <p>- Umro: 1299</p>
                                <p>- Bio poznat po: nečemu važnom</p>
                            </div>
                        </div>     
                          
                    </div>


                    {/* Tree row - person container */}
                    <div className="relative flex justify-center mt-[60px]">
                        
                        {/* Jedna osoba u stablu */}
                        <div className="flex-col relative group block text-center text-[#3f2b0a] ">
                            {/* Vertical line from the title block to person AKO NEMA RODITELJ VEZU NEMA JE*/}
                            <div className="absolute top-[-36px] left-1/2 transform -translate-x-1/2 w-[2px] h-[40px] bg-[#3f2b0a] z-0" />
                            {/* HORIZONTAL line from the title block to person AKO JE ZENSO I IMA MUZA S LEVE STRANE*/}
                            <div className="absolute top-[54px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            {/* HORIZONTAL line from the title block to person AKO JE MUSKO I IMA ZENU S DESNE STRANE*/}
                            <div className="absolute top-[54px] translate-x-[121px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            {/* Vertical line from OD RODITELJA LINIJA ZA DECU al samo od muskog nek ide jer ako nema decu nema ni vezu sa zenom jer zenu dobijamo preko detetove veze s majkom */}
                            <div className="absolute -right-[1px] top-[55px] w-[2px] h-[130px] bg-[#3f2b0a] z-0" />

                            <div className="px-[4px] hover:scale-110 transition-transform duration-300">
                                <img
                                    src="/src/images/download.jpeg"
                                    alt="Historical Figure"
                                    className="w-[80px] h-[100px] object-cover mx-auto my-[4px] border-2 border-[#3f2b0a] rounded-lg"
                                />
                                <p className="text-lg font-bold mt-2">Titula Ime Prezime</p>
                                <p className="text-md mt-1">(od-do)</p>
                            </div>
                            
                            {/* Tooltip Info Box */}
                            <div className="absolute -top-10 -left-50 w-56 p-3 bg-[#e6cda5] text-[#3f2b0a] border border-[#3f2b0a] rounded-lg shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-300 z-20">
                                <p className="font-semibold">Dodatne informacije:</p>
                                <p>- Rođen: 1234</p>
                                <p>- Umro: 1299</p>
                                <p>- Bio poznat po: nečemu važnom</p>
                            </div>
                        </div>
                        {/* Jedna osoba u stablu */}
                        <div className="flex-col relative group block text-center text-[#3f2b0a] ">
                            {/* Vertical line from the title block to person AKO NEMA RODITELJ VEZU NEMA JE*/}
                            <div className="absolute top-[-36px] left-1/2 transform -translate-x-1/2 w-[2px] h-[40px] bg-[#3f2b0a] z-0" />
                            {/* HORIZONTAL line from the title block to person AKO JE ZENSO I IMA MUZA S LEVE STRANE*/}
                            <div className="absolute top-[54px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            {/* HORIZONTAL line from the title block to person AKO JE MUSKO I IMA ZENU S DESNE STRANE*/}
                            <div className="absolute top-[54px] translate-x-[121px] w-[45px] h-[2px] bg-[#3f2b0a] z-0" />
                            {/* Vertical line from OD RODITELJA LINIJA ZA DECU al samo od muskog nek ide jer ako nema decu nema ni vezu sa zenom jer zenu dobijamo preko detetove veze s majkom */}
                            <div className="absolute -right-[1px] top-[55px] w-[2px] h-[130px] bg-[#3f2b0a] z-0" />

                            <div className="px-[4px] hover:scale-110 transition-transform duration-300">
                                <img
                                    src="/src/images/download.jpeg"
                                    alt="Historical Figure"
                                    className="w-[80px] h-[100px] object-cover mx-auto my-[4px] border-2 border-[#3f2b0a] rounded-lg"
                                />
                                <p className="text-lg font-bold mt-2">Titula Ime Prezime</p>
                                <p className="text-md mt-1">(od-do)</p>
                            </div>
                            
                            {/* Tooltip Info Box */}
                            <div className="absolute -top-10 -left-50 w-56 p-3 bg-[#e6cda5] text-[#3f2b0a] border border-[#3f2b0a] rounded-lg shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-300 z-20">
                                <p className="font-semibold">Dodatne informacije:</p>
                                <p>- Rođen: 1234</p>
                                <p>- Umro: 1299</p>
                                <p>- Bio poznat po: nečemu važnom</p>
                            </div>
                        </div>     
                          
                    </div>




                    
                </div>
                    
                    
                </div>
            </div>
                
);
}