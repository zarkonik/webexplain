import { useState } from 'react';
import { createGuide } from '../../api/guideApi';
import { getLiveScreenshotUrl } from '../../api/liveCaptureApi';
import BrowserFrame from '../BrowserFrame/BrowserFrame';
import type { RecordedStepDto } from '../../types/liveCapture';
import type { GuideDto } from '../../types/guide';
import './GuideStepAnnotator.css';

interface GuideStepAnnotatorProps {
  sessionId: string;
  sourceUrl: string;
  steps: RecordedStepDto[];
  onSaved: (guide: GuideDto) => void;
  onCancel: () => void;
}

function GuideStepAnnotator({ sessionId, sourceUrl, steps, onSaved, onCancel }: GuideStepAnnotatorProps) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [instructions, setInstructions] = useState<Record<number, string>>({});
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const actionableSteps = steps;

  async function handleSave() {
    if (!title.trim()) {
      setError('Give the guide a title before saving.');
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      const guide = await createGuide({
        title: title.trim(),
        description: description.trim(),
        sourceUrl,
        sourceCaptureSessionId: sessionId,
        steps: actionableSteps.map((step) => ({
          order: step.order,
          targetSelector: step.selector ?? step.url,
          instruction: instructions[step.order] ?? '',
          actionType: step.actionType,
          inputValue: step.value,
          pageUrl: step.url,
          elementDescription: step.elementDescription,
        })),
      });
      onSaved(guide);
    } catch {
      setError('Could not save the guide. Please try again.');
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="guide-step-annotator">
      <div className="guide-step-annotator__meta">
        <input
          type="text"
          className="guide-step-annotator__title-input"
          placeholder="Guide title, e.g. How to create an invoice"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <textarea
          className="guide-step-annotator__description-input"
          placeholder="Short description (optional)"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={2}
        />
      </div>

      <ul className="guide-step-annotator__list">
        {actionableSteps.map((step) => (
          <li key={step.order} className="guide-step-annotator__row">
            <BrowserFrame url={step.url}>
              <img
                className="guide-step-annotator__screenshot"
                src={getLiveScreenshotUrl(sessionId, step.order)}
                alt={`Step ${step.order}`}
              />
            </BrowserFrame>
            <div className="guide-step-annotator__fields">
              <p className="guide-step-annotator__description">
                {step.elementDescription ?? 'No description available for this step.'}
              </p>
              <input
                type="text"
                className="guide-step-annotator__instruction-input"
                placeholder="Additional explanation (optional)"
                value={instructions[step.order] ?? ''}
                onChange={(e) => setInstructions((prev) => ({ ...prev, [step.order]: e.target.value }))}
              />
              <details className="guide-step-annotator__technical">
                <summary>Technical details</summary>
                <span className="guide-step-annotator__selector">
                  {step.actionType === 'navigate' ? 'Opens page: ' : `${step.actionType} on: `}
                  {step.selector ?? step.url}
                </span>
              </details>
            </div>
          </li>
        ))}
      </ul>

      {error && <p className="guide-step-annotator__error">{error}</p>}

      <div className="guide-step-annotator__actions">
        <button type="button" className="guide-step-annotator__cancel" onClick={onCancel} disabled={isSaving}>
          Discard
        </button>
        <button type="button" className="guide-step-annotator__save" onClick={handleSave} disabled={isSaving}>
          {isSaving ? 'Saving…' : 'Save as guide'}
        </button>
      </div>
    </div>
  );
}

export default GuideStepAnnotator;
