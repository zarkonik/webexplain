import { useEffect, useState } from 'react';
import { getScreenshotUrl } from '../../api/captureApi';
import BrowserFrame from '../BrowserFrame/BrowserFrame';
import CaptureFilmstrip from '../CaptureFilmstrip/CaptureFilmstrip';
import type { CaptureSessionDto } from '../../types/capture';
import { CaptureStatus } from '../../types/capture';
import './CaptureViewer.css';

interface CaptureViewerProps {
  session: CaptureSessionDto | null;
}

function CaptureViewer({ session }: CaptureViewerProps) {
  const [selectedOrder, setSelectedOrder] = useState(1);

  useEffect(() => {
    setSelectedOrder(1);
  }, [session?.id]);

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

  const selectedPage = session.pages.find((p) => p.order === selectedOrder) ?? session.pages[0];

  return (
    <div className="capture-viewer">
      {selectedPage && (
        <BrowserFrame url={selectedPage.url ?? session.sourceUrl}>
          <img
            className="capture-viewer__screenshot"
            src={getScreenshotUrl(session.id, selectedPage.order)}
            alt={`Screenshot of ${selectedPage.url}`}
          />
        </BrowserFrame>
      )}
      <CaptureFilmstrip
        sessionId={session.id}
        pages={session.pages}
        selectedOrder={selectedOrder}
        onSelect={setSelectedOrder}
      />
    </div>
  );
}

export default CaptureViewer;
