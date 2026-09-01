import { useEffect, useState } from 'react';
import { clearToken, getToken, setUnauthorizedHandler } from './api/client';
import Auth from './pages/Auth/Auth';
import Home from './pages/Home/Home';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(() => getToken() !== null);

  useEffect(() => {
    setUnauthorizedHandler(() => setIsAuthenticated(false));
  }, []);

  function handleLogout() {
    clearToken();
    setIsAuthenticated(false);
  }

  if (!isAuthenticated) {
    return <Auth onAuthenticated={() => setIsAuthenticated(true)} />;
  }

  return <Home onLogout={handleLogout} />;
}

export default App;
