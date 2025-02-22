'use client';
import { revalidatePathClient } from '@/app/action';
import { AXIOS } from '@/utils/network/axios';
import * as ContextMenu from '@radix-ui/react-context-menu';
import { useParams } from 'next/navigation';
import toast from 'react-hot-toast';

interface IRemoveSongContextItem {
  trackId: string;
  ownerId?: number;
}

const RemoveSongContextItem: React.FC<IRemoveSongContextItem> = ({
  trackId,
}) => {
  const { id: playlistId } = useParams();

  const handleRemove = async () => {
    try {
      //TODO: Return a list of playlistIds the song was in and then revalidate? would this be optimal or should playlists be client sided?
      await AXIOS.delete(`/playlist/${playlistId}?song=${trackId}`);
      revalidatePathClient(`/playlist/${playlistId}`);
      toast.success('Removed from playlist');
    } catch (err) {
      console.log(err);
      toast.error(err as string);
    }
  };

  return (
    <ContextMenu.Item
      onClick={handleRemove}
      className="group relative flex h-[25px] items-center rounded-[3px] px-2 py-4 text-sm leading-none text-red-500 outline-none data-[highlighted]:bg-neutral-700/80"
    >
      Remove from playlist
    </ContextMenu.Item>
  );
};

export default RemoveSongContextItem;
