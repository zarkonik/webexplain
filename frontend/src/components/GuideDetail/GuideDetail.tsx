import { getScreenshotUrl } from '../../api/captureApi';
import { getGuideWordExportUrl } from '../../api/guideApi';
import BrowserFrame from '../BrowserFrame/BrowserFrame';
import type { GuideDto } from '../../types/guide';
import './GuideDetail.css';

interface GuideDetailProps {
  guide: GuideDto | null;
  onDelete: (id: string) => void;
}

function GuideDetail({ guide, onDelete }: GuideDetailProps) {
  if (!guide) {
    return (
      <div className="guide-detail guide-detail--empty">
        <p>Select a guide to see its steps.</p>
      </div>
    );
  }

  return (
    <div className="guide-detail">
      <div className="guide-detail__header">
        <div>
          <h2 className="guide-detail__title">{guide.title}</h2>
          <a className="guide-detail__url" href={guide.sourceUrl} target="_blank" rel="noreferrer">
            {guide.sourceUrl}
          </a>
        </div>
        <div className="guide-detail__actions">
          <a className="guide-detail__export" href={getGuideWordExportUrl(guide.id)}>
            Export to Word
          </a>
          <button type="button" className="guide-detail__delete" onClick={() => onDelete(guide.id)}>
            Delete
          </button>
        </div>
      </div>

      {guide.description && <p className="guide-detail__description">{guide.description}</p>}

      <ol className="guide-detail__steps">
        {guide.steps.map((step) => (
          <li key={step.id} className="guide-detail__step">
            <div className="guide-detail__step-header">
              <div className="guide-detail__step-badge">{step.order}</div>
              <div className="guide-detail__step-header-text">
                <p className="guide-detail__step-instruction">
                  {step.elementDescription ?? <em>No description available for this step.</em>}
                </p>
                {step.instruction && (
                  <span className="guide-detail__step-action-description">{step.instruction}</span>
                )}
              </div>
            </div>

            {guide.sourceCaptureSessionId ? (
              <BrowserFrame url={step.pageUrl ?? guide.sourceUrl}>
                <img
                  className="guide-detail__step-screenshot"
                  src={getScreenshotUrl(guide.sourceCaptureSessionId, step.order)}
                  alt={`Step ${step.order} screenshot`}
                />
              </BrowserFrame>
            ) : (
              <div className="guide-detail__step-screenshot guide-detail__step-screenshot--placeholder">
                No screenshot available for this step.
              </div>
            )}

            <details className="guide-detail__step-technical">
              <summary>Technical details</summary>
              <span className="guide-detail__step-meta">
                {step.actionType === 'navigate' ? 'Opens page: ' : `${step.actionType} on: `}
                {step.targetSelector}
              </span>
            </details>
          </li>
        ))}
      </ol>
    </div>
  );
}

export default GuideDetail;
