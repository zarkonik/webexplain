import { getScreenshotUrl } from '../../api/captureApi';
import { getGuideWordExportUrl } from '../../api/guideApi';
import BrowserFrame from '../BrowserFrame/BrowserFrame';
import type { GuideDto, GuideStepDto } from '../../types/guide';
import './GuideDetail.css';

interface GuideDetailProps {
  guide: GuideDto | null;
  onDelete: (id: string) => void;
}

// Fixed size of the headless browser Playwright captured the screenshot at (see
// LiveCaptureManager.StartAsync) - NOT the screen of whoever is viewing this guide. Target
// coordinates were recorded in these CSS pixels, so the highlight box is positioned as a
// percentage of this reference size. That keeps it correctly aligned with the image no matter
// how large or small the image is actually rendered - including on a phone.
const CAPTURE_VIEWPORT_WIDTH = 1280;
const CAPTURE_VIEWPORT_HEIGHT = 800;

function hasTargetBox(step: GuideStepDto): boolean {
  return step.targetX !== null && step.targetY !== null && step.targetWidth !== null && step.targetHeight !== null;
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
        <div className="guide-detail__header-text">
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
        {guide.steps.map((step) => {
          // The screenshot that shows WHERE to act is the state before this step's own
          // action ran - i.e. the previous step's screenshot - since this step's own
          // screenshot was taken after clicking/typing already changed the page.
          const screenshotOrder = step.order - 1;

          return (
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

              {guide.sourceCaptureSessionId && screenshotOrder >= 1 ? (
                <BrowserFrame url={step.pageUrl ?? guide.sourceUrl}>
                  <div className="guide-detail__screenshot-wrapper">
                    <img
                      className="guide-detail__step-screenshot"
                      src={getScreenshotUrl(guide.sourceCaptureSessionId, screenshotOrder)}
                      alt={`Step ${step.order} screenshot`}
                    />
                    {hasTargetBox(step) && (
                      <div
                        className="guide-detail__highlight"
                        style={{
                          left: `${(step.targetX! / CAPTURE_VIEWPORT_WIDTH) * 100}%`,
                          top: `${(step.targetY! / CAPTURE_VIEWPORT_HEIGHT) * 100}%`,
                          width: `${(step.targetWidth! / CAPTURE_VIEWPORT_WIDTH) * 100}%`,
                          height: `${(step.targetHeight! / CAPTURE_VIEWPORT_HEIGHT) * 100}%`,
                        }}
                      />
                    )}
                  </div>
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
          );
        })}
      </ol>
    </div>
  );
}

export default GuideDetail;
