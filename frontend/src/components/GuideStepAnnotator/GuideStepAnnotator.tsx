import { useState } from 'react';
import { createGuide } from '../../api/guideApi';
import { getLiveScreenshotUrl } from '../../api/liveCaptureApi';
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

  const actionableSteps = steps.filter((step) => step.selector);

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
        steps: actionableSteps.map((step, index) => ({
          order: index + 1,
          targetSelector: step.selector!,
          instruction: instructions[step.order] ?? '',
          actionType: step.actionType,
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
            <img
              className="guide-step-annotator__thumbnail"
              src={getLiveScreenshotUrl(sessionId, step.order)}
              alt={`Step ${step.order}`}
            />
            <div className="guide-step-annotator__fields">
              <span className="guide-step-annotator__selector">{step.selector}</span>
              <input
                type="text"
                className="guide-step-annotator__instruction-input"
                placeholder="What should the user do here?"
                value={instructions[step.order] ?? ''}
                onChange={(e) => setInstructions((prev) => ({ ...prev, [step.order]: e.target.value }))}
              />
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
