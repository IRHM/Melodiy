import usePlayer from '@/hooks/stores/usePlayer';
import * as ContextMenu from '@radix-ui/react-context-menu';
import toast from 'react-hot-toast';

interface IQueueContextItem {
  trackId: string;
}

const QueueContextItem: React.FC<IQueueContextItem> = ({ trackId }) => {
  const player = usePlayer();

  const onQueue = () => {
    const curIds = player.ids;
    curIds.splice(1, 0, trackId);
    toast.success('Added to queue');
  };

  return (
    <ContextMenu.Item
      onClick={onQueue}
      className={
        'group relative flex h-[25px] items-center rounded-[3px] px-2 py-4 text-sm leading-none outline-none  data-[highlighted]:bg-neutral-700/80 data-[disabled]:text-inactive'
      }
    >
      Add to queue
    </ContextMenu.Item>
  );
};

export default QueueContextItem;
