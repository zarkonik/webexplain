import { useState } from 'react';
import type { FormEvent } from 'react';
import { createCaptureSession } from '../../api/captureApi';
import type { CaptureSessionDto } from '../../types/capture';
import './CaptureForm.css';

interface CaptureFormProps {
  onCaptured: (session: CaptureSessionDto) => void;
}

function CaptureForm({ onCaptured }: CaptureFormProps) {
  const [url, setUrl] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!url.trim()) return;

    setIsSubmitting(true);
    setError(null);

    try {
      const session = await createCaptureSession(url.trim());
      onCaptured(session);
      setUrl('');
    } catch {
      setError('Snimanje stranice nije uspelo. Proveri URL i pokušaj ponovo.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="capture-form" onSubmit={handleSubmit}>
      <input
        type="url"
        className="capture-form__input"
        placeholder="https://example.com"
        value={url}
        onChange={(event) => setUrl(event.target.value)}
        required
      />
      <button type="submit" className="capture-form__button" disabled={isSubmitting}>
        {isSubmitting ? 'Snimanje...' : 'Snimi stranicu'}
      </button>
      {error && <p className="capture-form__error">{error}</p>}
    </form>
  );
}

export default CaptureForm;
