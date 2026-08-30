import type { GuideDto } from '../../types/guide';
import './GuideList.css';

interface GuideListProps {
  guides: GuideDto[];
  selectedId: string | null;
  onSelect: (guide: GuideDto) => void;
}

function GuideList({ guides, selectedId, onSelect }: GuideListProps) {
  if (guides.length === 0) {
    return <p className="guide-list__empty">No guides saved yet.</p>;
  }

  return (
    <ul className="guide-list">
      {guides.map((guide) => (
        <li key={guide.id}>
          <button
            type="button"
            className={`guide-list__item ${guide.id === selectedId ? 'guide-list__item--active' : ''}`}
            onClick={() => onSelect(guide)}
          >
            <span className="guide-list__title">{guide.title}</span>
            <span className="guide-list__url">{guide.sourceUrl}</span>
            <span className="guide-list__step-count">{guide.steps.length} step(s)</span>
          </button>
        </li>
      ))}
    </ul>
  );
}

export default GuideList;
