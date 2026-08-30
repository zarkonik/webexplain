import { useState } from 'react';
import type { MouseEvent } from 'react';
import { clickLiveCapture, finishLiveCapture, getLiveScreenshotUrl, startLiveCapture } from '../../api/liveCaptureApi';
import GuideStepAnnotator from '../GuideStepAnnotator/GuideStepAnnotator';
import type { RecordedStepDto } from '../../types/liveCapture';
import type { GuideDto } from '../../types/guide';
import './LiveCaptureRecorder.css';

type Phase = 'idle' | 'recording' | 'reviewing' | 'saved';

function LiveCaptureRecorder() {
  const [phase, setPhase] = useState<Phase>('idle');
  const [url, setUrl] = useState('');
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [currentOrder, setCurrentOrder] = useState(1);
  const [log, setLog] = useState<RecordedStepDto[]>([]);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [savedGuide, setSavedGuide] = useState<GuideDto | null>(null);

  async function handleStart() {
    if (!url.trim()) return;

    setIsBusy(true);
    setError(null);

    try {
      const result = await startLiveCapture(url.trim());
      setSessionId(result.sessionId);
      setCurrentOrder(result.order);
      setLog([{ order: result.order, actionType: 'navigate', selector: null, url: result.url }]);
      setPhase('recording');
    } catch {
      setError('Could not open that page. Check the URL and try again.');
    } finally {
      setIsBusy(false);
    }
  }

  async function handleImageClick(event: MouseEvent<HTMLImageElement>) {
    if (!sessionId || isBusy) return;

    const rect = event.currentTarget.getBoundingClientRect();
    const xRatio = (event.clientX - rect.left) / rect.width;
    const yRatio = (event.clientY - rect.top) / rect.height;

    setIsBusy(true);
    setError(null);

    try {
      const step = await clickLiveCapture(sessionId, xRatio, yRatio);
      setCurrentOrder(step.order);
      setLog((prev) => [...prev, step]);
    } catch {
      setError('That click could not be recorded. The page may still be loading — try again.');
    } finally {
      setIsBusy(false);
    }
  }

  async function handleFinish() {
    if (!sessionId) return;

    setIsBusy(true);
    setError(null);

    try {
      const steps = await finishLiveCapture(sessionId);
      setLog(steps);
      setPhase('reviewing');
    } catch {
      setError('Could not finish the session. Please try again.');
    } finally {
      setIsBusy(false);
    }
  }

  function handleReset() {
    setPhase('idle');
    setUrl('');
    setSessionId(null);
    setCurrentOrder(1);
    setLog([]);
    setError(null);
    setSavedGuide(null);
  }

  if (phase === 'idle') {
    return (
      <div className="live-capture-recorder">
        <div className="live-capture-recorder__start-row">
          <input
            type="url"
            className="live-capture-recorder__input"
            placeholder="https://example.com"
            value={url}
            onChange={(e) => setUrl(e.target.value)}
          />
          <button type="button" className="live-capture-recorder__button" onClick={handleStart} disabled={isBusy}>
            {isBusy ? 'Opening…' : 'Start browsing'}
          </button>
        </div>
        {error && <p className="live-capture-recorder__error">{error}</p>}
      </div>
    );
  }

  if (phase === 'recording' && sessionId) {
    return (
      <div className="live-capture-recorder">
        <p className="live-capture-recorder__hint">
          Click anywhere on the page below to record that step. When you're done, click "Finish".
        </p>
        <div className="live-capture-recorder__stage">
          <img
            className="live-capture-recorder__screenshot"
            src={getLiveScreenshotUrl(sessionId, currentOrder)}
            alt="Live page preview"
            onClick={handleImageClick}
          />
          {isBusy && <div className="live-capture-recorder__overlay">Working…</div>}
        </div>
        {error && <p className="live-capture-recorder__error">{error}</p>}
        <div className="live-capture-recorder__footer">
          <span className="live-capture-recorder__step-count">{log.length} step(s) recorded</span>
          <button type="button" className="live-capture-recorder__button" onClick={handleFinish} disabled={isBusy}>
            Finish
          </button>
        </div>
      </div>
    );
  }

  if (phase === 'reviewing' && sessionId) {
    return (
      <div className="live-capture-recorder">
        <GuideStepAnnotator
          sessionId={sessionId}
          sourceUrl={log[0]?.url ?? url}
          steps={log}
          onSaved={(guide) => {
            setSavedGuide(guide);
            setPhase('saved');
          }}
          onCancel={handleReset}
        />
      </div>
    );
  }

  if (phase === 'saved' && savedGuide) {
    return (
      <div className="live-capture-recorder">
        <p className="live-capture-recorder__success">
          Guide "{savedGuide.title}" saved with {savedGuide.steps.length} step(s).
        </p>
        <button type="button" className="live-capture-recorder__button" onClick={handleReset}>
          Record another guide
        </button>
      </div>
    );
  }

  return null;
}

export default LiveCaptureRecorder;
