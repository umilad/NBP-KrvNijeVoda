export default function Licnosti() { 
    return (
        <div className="licnosti-container flex flex-col items-center justify-center text-white"> 
            {/* Slika */}
            <div className="relative w-[300px] h-[355px] m-auto">
                {/*ram*/}
                <img
                    src="/src/images/picture-frame.png"
                    alt="Frame"
                    className="absolute top-10 left-0 w-full h-full z-10 pointer-events-none"
                />
                {/*slika u ramu*/}
                <div className="absolute inset-0 top-20 flex items-center justify-center z-0">
                    <img
                        src="/src/images/download.jpeg"
                        alt="Historical Figure"
                        className="w-[190px] h-[235px] object-cover"
                    />
                </div>
            </div>

            {/* Podaci */}
            <div className="absolute top-120 mx-[100px] p-[20px] block border-2 border-[#3f2b0a] bg-[#e6cda5] p-4 rounded-lg text-center text-[#3f2b0a] mt-4">
                <p className="text-2xl font-bold mt-2">titula ime prezime</p>
                <p className="text-xl font-bold mt-2">(rodjen-umro)</p>
                <div>
                    <p className="text-lg p-[30px] mt-2">tekst Get more updates...
Do you want to get notified when a new component is added to Flowbite? Sign up for our newsletter and you'll be among the first to find out about new features, components, versions, and tools.Get more updates...
Do you want to get notified when a new component is added to Flowbite? Sign up for our newsletter and you'll be among the first to find out about new features, components, versions, and tools.Get more updates...
Do you want to get notified when a new component is added to Flowbite? Sign up for our newsletter and you'll be among the first to find out about new features, components, versions, and tools.Get more updates...
Do you want to get notified when a new component is added to Flowbite? Sign up for our newsletter and you'll be among the first to find out about new features, components, versions, and tools.Get more updates...
Do you want to get notified when a new component is added to Flowbite? Sign up for our newsletter and you'll be among the first to find out about new features, components, versions, and tools.Get more updates...
Do you want to get notified when a new component is added to Flowbite? Sign up for our newsletter and you'll be among the first to find out about new features, components, versions, and tools.Get more updates...
Do you want to get notified when a new component is added to Flowbite? Sign up for our newsletter and you'll be among the first to find out about new features, components, versions, and tools.</p>
                </div>
                
            </div>

        </div>
        
    );
}