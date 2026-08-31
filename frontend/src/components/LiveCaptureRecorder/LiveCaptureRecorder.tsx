import { useEffect, useRef, useState } from 'react';
import type { MouseEvent } from 'react';
import {
  clickLiveCapture,
  fillLiveCapture,
  finishLiveCapture,
  getLiveScreenshotUrl,
  inspectLiveCapture,
  scrollLiveCapture,
  startLiveCapture,
} from '../../api/liveCaptureApi';
import BrowserFrame from '../BrowserFrame/BrowserFrame';
import GuideStepAnnotator from '../GuideStepAnnotator/GuideStepAnnotator';
import type { RecordedStepDto } from '../../types/liveCapture';
import type { GuideDto } from '../../types/guide';
import './LiveCaptureRecorder.css';

type Phase = 'idle' | 'recording' | 'reviewing' | 'saved';

interface PendingFill {
  xRatio: number;
  yRatio: number;
}

interface LiveCaptureRecorderProps {
  onGuideSaved?: (guide: GuideDto) => void;
}

function LiveCaptureRecorder({ onGuideSaved }: LiveCaptureRecorderProps) {
  const [phase, setPhase] = useState<Phase>('idle');
  const [url, setUrl] = useState('');
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [currentOrder, setCurrentOrder] = useState(1);
  const [log, setLog] = useState<RecordedStepDto[]>([]);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [savedGuide, setSavedGuide] = useState<GuideDto | null>(null);
  const [pendingFill, setPendingFill] = useState<PendingFill | null>(null);
  const [fillValue, setFillValue] = useState('');
  const [screenshotVersion, setScreenshotVersion] = useState(0);
  const isScrollingRef = useRef(false);
  const stageRef = useRef<HTMLDivElement>(null);
  const handleWheelRef = useRef<(event: globalThis.WheelEvent) => void>(() => {});

  async function handleStart() {
    if (!url.trim()) return;

    setIsBusy(true);
    setError(null);

    try {
      const result = await startLiveCapture(url.trim());
      setSessionId(result.sessionId);
      setCurrentOrder(result.order);
      setLog([
        {
          order: result.order,
          actionType: 'navigate',
          selector: null,
          value: null,
          elementDescription: 'Open the starting page.',
          url: result.url,
          targetX: null,
          targetY: null,
          targetWidth: null,
          targetHeight: null,
        },
      ]);
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
      const probe = await inspectLiveCapture(sessionId, xRatio, yRatio);
      if (probe.isFillable) {
        setPendingFill({ xRatio, yRatio });
        setFillValue('');
        return;
      }

      const step = await clickLiveCapture(sessionId, xRatio, yRatio);
      setCurrentOrder(step.order);
      setLog((prev) => [...prev, step]);
    } catch {
      setError('That action could not be recorded. The page may still be loading — try again.');
    } finally {
      setIsBusy(false);
    }
  }

  async function handleFillSubmit() {
    if (!sessionId || !pendingFill) return;

    setIsBusy(true);
    setError(null);

    try {
      const step = await fillLiveCapture(sessionId, pendingFill.xRatio, pendingFill.yRatio, fillValue);
      setCurrentOrder(step.order);
      setLog((prev) => [...prev, step]);
      setPendingFill(null);
    } catch {
      setError('That value could not be entered. Please try again.');
    } finally {
      setIsBusy(false);
    }
  }

  async function handleFillSkip() {
    if (!sessionId || !pendingFill) return;

    setIsBusy(true);
    setError(null);

    try {
      const step = await clickLiveCapture(sessionId, pendingFill.xRatio, pendingFill.yRatio);
      setCurrentOrder(step.order);
      setLog((prev) => [...prev, step]);
      setPendingFill(null);
    } catch {
      setError('That click could not be recorded. Please try again.');
    } finally {
      setIsBusy(false);
    }
  }

  // React attaches wheel listeners as passive by default, so calling preventDefault() from
  // an onWheel prop silently does nothing (and logs a console warning on every tick) - the
  // page would still scroll natively underneath the screenshot. A manually-registered native
  // listener with { passive: false } is the only way to actually take over wheel events.
  handleWheelRef.current = (event: globalThis.WheelEvent) => {
    event.preventDefault();
    if (!sessionId || isScrollingRef.current || pendingFill) return;

    isScrollingRef.current = true;
    scrollLiveCapture(sessionId, event.deltaY)
      .then((order) => {
        if (order > 0) {
          setCurrentOrder(order);
          setScreenshotVersion((v) => v + 1);
        }
      })
      .catch(() => {
        // A missed scroll tick isn't worth surfacing - the user can just keep scrolling.
      })
      .finally(() => {
        isScrollingRef.current = false;
      });
  };

  useEffect(() => {
    const stage = stageRef.current;
    if (!stage) return;

    const listener = (event: globalThis.WheelEvent) => handleWheelRef.current(event);
    stage.addEventListener('wheel', listener, { passive: false });
    return () => stage.removeEventListener('wheel', listener);
  }, [phase]);

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
    setPendingFill(null);
    setFillValue('');
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
    const currentUrl = log[log.length - 1]?.url ?? url;

    return (
      <div className="live-capture-recorder">
        <p className="live-capture-recorder__hint">
          Click anywhere on the page below to record that step. When you're done, click "Finish".
        </p>
        <BrowserFrame url={currentUrl}>
          <div className="live-capture-recorder__stage" ref={stageRef}>
            <img
              className="live-capture-recorder__screenshot"
              src={getLiveScreenshotUrl(sessionId, currentOrder, screenshotVersion)}
              alt="Live page preview"
              onClick={handleImageClick}
            />
            {isBusy && <div className="live-capture-recorder__overlay">Working…</div>}
          </div>
        </BrowserFrame>

        {pendingFill && (
          <div className="live-capture-recorder__fill-prompt">
            <input
              type="text"
              className="live-capture-recorder__fill-input"
              placeholder="What should be typed here?"
              value={fillValue}
              onChange={(e) => setFillValue(e.target.value)}
              autoFocus
              onKeyDown={(e) => {
                if (e.key === 'Enter') handleFillSubmit();
              }}
            />
            <button
              type="button"
              className="live-capture-recorder__button"
              onClick={handleFillSubmit}
              disabled={isBusy || !fillValue.trim()}
            >
              Type &amp; continue
            </button>
            <button
              type="button"
              className="live-capture-recorder__skip-button"
              onClick={handleFillSkip}
              disabled={isBusy}
            >
              Just click
            </button>
          </div>
        )}

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
            onGuideSaved?.(guide);
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
