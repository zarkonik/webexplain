import { getScreenshotUrl } from '../../api/captureApi';
import type { CaptureSessionDto } from '../../types/capture';
import { CaptureStatus } from '../../types/capture';
import './CaptureViewer.css';

interface CaptureViewerProps {
  session: CaptureSessionDto | null;
}

function CaptureViewer({ session }: CaptureViewerProps) {
  if (!session) {
    return (
      <div className="capture-viewer capture-viewer--empty">
        <p>Select a captured page to preview it here.</p>
      </div>
    );
  }

  if (session.status === CaptureStatus.Failed) {
    return (
      <div className="capture-viewer capture-viewer--error">
        <p>Capture failed: {session.errorMessage ?? 'Unknown error'}</p>
      </div>
    );
  }

  if (session.status !== CaptureStatus.Completed) {
    return (
      <div className="capture-viewer capture-viewer--empty">
        <p>Capture in progress…</p>
      </div>
    );
  }

  return (
    <div className="capture-viewer">
      <div className="capture-viewer__header">
        <span className="capture-viewer__url">{session.sourceUrl}</span>
      </div>
      <img
        className="capture-viewer__screenshot"
        src={getScreenshotUrl(session.id)}
        alt={`Screenshot of ${session.sourceUrl}`}
      />
    </div>
  );
}

export default CaptureViewer;
