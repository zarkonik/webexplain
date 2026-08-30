import { getScreenshotUrl } from '../../api/captureApi';
import type { CapturedPageDto } from '../../types/capture';
import './CaptureFilmstrip.css';

interface CaptureFilmstripProps {
  sessionId: string;
  pages: CapturedPageDto[];
  selectedOrder: number;
  onSelect: (order: number) => void;
}

function CaptureFilmstrip({ sessionId, pages, selectedOrder, onSelect }: CaptureFilmstripProps) {
  if (pages.length <= 1) {
    return null;
  }

  return (
    <ul className="capture-filmstrip">
      {pages.map((page) => (
        <li key={page.id}>
          <button
            type="button"
            className={`capture-filmstrip__item ${page.order === selectedOrder ? 'capture-filmstrip__item--active' : ''}`}
            onClick={() => onSelect(page.order)}
          >
            <img
              className="capture-filmstrip__thumbnail"
              src={getScreenshotUrl(sessionId, page.order)}
              alt={`Step ${page.order}: ${page.url}`}
            />
            <span className="capture-filmstrip__label">Step {page.order}</span>
          </button>
        </li>
      ))}
    </ul>
  );
}

export default CaptureFilmstrip;
