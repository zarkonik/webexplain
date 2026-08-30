import { useEffect, useState } from 'react';
import { getCaptureSessions } from '../../api/captureApi';
import CaptureForm from '../../components/CaptureForm/CaptureForm';
import CaptureList from '../../components/CaptureList/CaptureList';
import CaptureViewer from '../../components/CaptureViewer/CaptureViewer';
import type { CaptureSessionDto } from '../../types/capture';
import './Home.css';

function Home() {
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
        <p>Capture a page and preview it below.</p>
      </header>

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
    </div>
  );
}

export default Home;
