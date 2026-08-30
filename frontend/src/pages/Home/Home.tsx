import { useEffect, useState } from 'react';
import { getCaptureSessions } from '../../api/captureApi';
import CaptureForm from '../../components/CaptureForm/CaptureForm';
import CaptureList from '../../components/CaptureList/CaptureList';
import CaptureViewer from '../../components/CaptureViewer/CaptureViewer';
import LiveCaptureRecorder from '../../components/LiveCaptureRecorder/LiveCaptureRecorder';
import type { CaptureSessionDto } from '../../types/capture';
import './Home.css';

type Mode = 'live' | 'manual';

function Home() {
  const [mode, setMode] = useState<Mode>('live');
  const [sessions, setSessions] = useState<CaptureSessionDto[]>([]);
  const [selectedSession, setSelectedSession] = useState<CaptureSessionDto | null>(null);

  useEffect(() => {
    loadSessions();
  }, []);

  async function loadSessions() {
    try {
      const data = await getCaptureSessions();
      setSessions(data);
    } catch {
      // Keeping the list empty is an acceptable fallback for the initial load.
    }
  }

  function handleCaptured(session: CaptureSessionDto) {
    setSessions((prev) => [session, ...prev]);
    setSelectedSession(session);
  }

  return (
    <div className="home">
      <header className="home__header">
        <h1>WebExplain</h1>
        <p>Browse a page live and turn what you click into a guide, or capture pages manually below.</p>
      </header>

      <div className="home__mode-switch">
        <button
          type="button"
          className={`home__mode-button ${mode === 'live' ? 'home__mode-button--active' : ''}`}
          onClick={() => setMode('live')}
        >
          Live browsing
        </button>
        <button
          type="button"
          className={`home__mode-button ${mode === 'manual' ? 'home__mode-button--active' : ''}`}
          onClick={() => setMode('manual')}
        >
          Manual capture
        </button>
      </div>

      {mode === 'live' ? (
        <LiveCaptureRecorder />
      ) : (
        <>
          <CaptureForm onCaptured={handleCaptured} />

          <div className="home__content">
            <aside className="home__sidebar">
              <CaptureList
                sessions={sessions}
                selectedId={selectedSession?.id ?? null}
                onSelect={setSelectedSession}
              />
            </aside>
            <main className="home__main">
              <CaptureViewer session={selectedSession} />
            </main>
          </div>
        </>
      )}
    </div>
  );
}

export default Home;
