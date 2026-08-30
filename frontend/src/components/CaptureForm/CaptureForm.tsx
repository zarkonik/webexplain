import { useState } from 'react';
import type { FormEvent } from 'react';
import { createCaptureSession } from '../../api/captureApi';
import CaptureStepEditor from '../CaptureStepEditor/CaptureStepEditor';
import type { CaptureSessionDto, CaptureStepInput } from '../../types/capture';
import './CaptureForm.css';

interface CaptureFormProps {
  onCaptured: (session: CaptureSessionDto) => void;
}

function validateSteps(steps: CaptureStepInput[]): string | null {
  for (let i = 0; i < steps.length; i++) {
    const step = steps[i];
    if ((step.actionType === 'click' || step.actionType === 'fill') && !step.selector?.trim()) {
      return `Step ${i + 1}: a CSS selector is required for "${step.actionType}".`;
    }
    if ((step.actionType === 'fill' || step.actionType === 'navigate') && !step.value?.trim()) {
      return `Step ${i + 1}: a value is required for "${step.actionType}".`;
    }
  }
  return null;
}

function CaptureForm({ onCaptured }: CaptureFormProps) {
  const [url, setUrl] = useState('');
  const [steps, setSteps] = useState<CaptureStepInput[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!url.trim()) return;

    const validationError = validateSteps(steps);
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const session = await createCaptureSession(url.trim(), steps);
      onCaptured(session);
      setUrl('');
      setSteps([]);
    } catch {
      setError('Capture failed. Check the URL and step selectors, then try again.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="capture-form" onSubmit={handleSubmit}>
      <div className="capture-form__row">
        <input
          type="url"
          className="capture-form__input"
          placeholder="https://example.com"
          value={url}
          onChange={(event) => setUrl(event.target.value)}
          required
        />
        <button type="submit" className="capture-form__button" disabled={isSubmitting}>
          {isSubmitting ? 'Capturing…' : 'Capture page'}
        </button>
      </div>

      <CaptureStepEditor steps={steps} onChange={setSteps} />

      {error && <p className="capture-form__error">{error}</p>}
    </form>
  );
}

export default CaptureForm;
