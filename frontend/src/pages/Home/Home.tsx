import { useEffect, useState } from 'react';
import { getCaptureSessions } from '../../api/captureApi';
import { deleteGuide, getGuides } from '../../api/guideApi';
import CaptureForm from '../../components/CaptureForm/CaptureForm';
import CaptureList from '../../components/CaptureList/CaptureList';
import CaptureViewer from '../../components/CaptureViewer/CaptureViewer';
import GuideDetail from '../../components/GuideDetail/GuideDetail';
import GuideList from '../../components/GuideList/GuideList';
import LiveCaptureRecorder from '../../components/LiveCaptureRecorder/LiveCaptureRecorder';
import type { CaptureSessionDto } from '../../types/capture';
import type { GuideDto } from '../../types/guide';
import './Home.css';

type Mode = 'live' | 'manual' | 'guides';

function Home() {
  const [mode, setMode] = useState<Mode>('live');
  const [sessions, setSessions] = useState<CaptureSessionDto[]>([]);
  const [selectedSession, setSelectedSession] = useState<CaptureSessionDto | null>(null);
  const [guides, setGuides] = useState<GuideDto[]>([]);
  const [selectedGuide, setSelectedGuide] = useState<GuideDto | null>(null);

  useEffect(() => {
    loadSessions();
    loadGuides();
  }, []);

  async function loadSessions() {
    try {
      const data = await getCaptureSessions();
      setSessions(data);
    } catch {
      // Keeping the list empty is an acceptable fallback for the initial load.
    }
  }

  async function loadGuides() {
    try {
      const data = await getGuides();
      setGuides(data);
    } catch {
      // Keeping the list empty is an acceptable fallback for the initial load.
    }
  }

  function handleCaptured(session: CaptureSessionDto) {
    setSessions((prev) => [session, ...prev]);
    setSelectedSession(session);
  }

  function handleGuideSaved(guide: GuideDto) {
    setGuides((prev) => [guide, ...prev]);
    setSelectedGuide(guide);
  }

  async function handleDeleteGuide(id: string) {
    try {
      await deleteGuide(id);
      setGuides((prev) => prev.filter((g) => g.id !== id));
      setSelectedGuide((prev) => (prev?.id === id ? null : prev));
    } catch {
      // Leaving the list unchanged on failure keeps the UI in a consistent state.
    }
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
        <button
          type="button"
          className={`home__mode-button ${mode === 'guides' ? 'home__mode-button--active' : ''}`}
          onClick={() => setMode('guides')}
        >
          My guides
        </button>
      </div>

      {mode === 'live' && <LiveCaptureRecorder onGuideSaved={handleGuideSaved} />}

      {mode === 'manual' && (
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

      {mode === 'guides' && (
        <div className="home__content">
          <aside className="home__sidebar">
            <GuideList guides={guides} selectedId={selectedGuide?.id ?? null} onSelect={setSelectedGuide} />
          </aside>
          <main className="home__main">
            <GuideDetail guide={selectedGuide} onDelete={handleDeleteGuide} />
          </main>
        </div>
      )}
    </div>
  );
}

export default Home;
