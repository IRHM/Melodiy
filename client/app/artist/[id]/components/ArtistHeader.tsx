'use client';

import ControlPanel from '@/app/artist/[id]/components/ControlPanel';
import Image from 'next/image';
import { useEffect, useRef } from 'react';
import { BiShuffle } from 'react-icons/bi';
import { BsFillPlayFill } from 'react-icons/bs';
import { twMerge } from 'tailwind-merge';
import './ArtistHeader.css';

export interface IArtistHeader {
  name: string;
  coverUrl?: string;
  listerners: number;
}

const ArtistHeader: React.FC<IArtistHeader> = ({
  name,
  coverUrl,
  listerners,
}) => {
  const headerDiv = useRef<HTMLDivElement>(null);
  const imgUrl = coverUrl ?? '/images/default_playlist.png';
  const GenerateImage = (index: number) => {
    return (
      <div className="relative">
        <Image
          // src={'/images/cover-3.webp'}
          // src={'/images/cover-2.jpeg'}
          src={imgUrl}
          fill={true}
          objectFit="cover"
          alt="Artist Cover"
          quality={100}
          draggable={false}
          className={twMerge(
            index % 2 == 0 ? 'opacity-[75%] mix-blend-lighten blur-[2px]' : ''
          )}
        />
      </div>
    );
  };

  useEffect(() => {
    console.log('UE');
    if (headerDiv.current) {
      const main = document.getElementsByTagName('main')[0];
      main.addEventListener('scroll', () => {
        // For some reason we get zeros returned sometimes, ignore those so we dont get jumpy behaviour
        if (!main.scrollTop) return;
        console.log(main.scrollTop);
        const newh = Math.max(500 - main.scrollTop * 3, 200);
        console.log('New div h:', newh);
        headerDiv.current!.style.height = `${newh}px`;
        if (newh === 200) {
          headerDiv.current!.style.position = 'sticky';
          headerDiv.current!.style.top = '0';
          headerDiv.current!.classList.add('shrunk');
        } else {
          headerDiv.current!.style.position = 'relative';
          headerDiv.current!.style.top = 'unset';
          headerDiv.current!.classList.remove('shrunk');
        }
      });
    }
  }, []);

  return (
    <div
      ref={headerDiv}
      className="artist-header relative h-[500px] overflow-hidden"
    >
      <div className="absolute bottom-0 z-20 h-[60px] w-full -translate-y-20">
        <img
          // src={'/images/cover-3.webp'}
          // src={'/images/cover-2.jpeg'}
          src={imgUrl}
          alt="Artist Cover"
          className={' bottom-0 z-10 w-full object-cover blur-[75px]'}
        />
      </div>
      <div className="relative grid h-full grid-cols-1 grid-rows-1 md:grid-cols-2 xl:grid-cols-3">
        {Array(3)
          .fill(0)
          .map((_, i) => GenerateImage(i))}
        <div className="absolute bottom-0 z-30 flex w-full items-center justify-between px-5 py-2">
          <div className="flex ">
            <div className="">
              <h1 className="text-3xl font-bold transition-all">{name}</h1>
              <span className="monthly-listeners hidden text-base font-medium transition-all md:block">
                {listerners.toLocaleString()} Monthly Listerners
              </span>
            </div>
            <div className="pri-btns mx-10 flex  items-center gap-x-4 align-middle transition-all ">
              <button className="group flex items-center gap-x-1 rounded bg-white px-4 py-2 text-center font-bold text-black hover:bg-opacity-80 disabled:cursor-not-allowed disabled:opacity-50">
                <BsFillPlayFill size={25} className="" />
                Play
              </button>

              <button className="group flex items-center gap-x-1 rounded bg-white px-4 py-2 text-center font-bold text-black hover:bg-opacity-80 disabled:cursor-not-allowed disabled:opacity-50">
                <BiShuffle size={25} className="" />
                Shuffle
              </button>
            </div>
          </div>

          <div className="flex h-full gap-x-8">
            <ControlPanel />
          </div>
        </div>
      </div>
    </div>
  );
};

export default ArtistHeader;
