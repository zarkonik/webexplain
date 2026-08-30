import type { CaptureSessionDto } from '../../types/capture';
import { CaptureStatus } from '../../types/capture';
import './CaptureList.css';

interface CaptureListProps {
  sessions: CaptureSessionDto[];
  selectedId: string | null;
  onSelect: (session: CaptureSessionDto) => void;
}

const statusLabels: Record<CaptureStatus, string> = {
  [CaptureStatus.Pending]: 'Pending',
  [CaptureStatus.Running]: 'Running',
  [CaptureStatus.Completed]: 'Completed',
  [CaptureStatus.Failed]: 'Failed',
};

function CaptureList({ sessions, selectedId, onSelect }: CaptureListProps) {
  if (sessions.length === 0) {
    return <p className="capture-list__empty">No captured pages yet.</p>;
  }

  return (
    <ul className="capture-list">
      {sessions.map((session) => (
        <li key={session.id}>
          <button
            type="button"
            className={`capture-list__item ${session.id === selectedId ? 'capture-list__item--active' : ''}`}
            onClick={() => onSelect(session)}
          >
            <span className="capture-list__url">{session.sourceUrl}</span>
            <span className={`capture-list__status capture-list__status--${session.status}`}>
              {statusLabels[session.status]}
            </span>
          </button>
        </li>
      ))}
    </ul>
  );
}

export default CaptureList;
