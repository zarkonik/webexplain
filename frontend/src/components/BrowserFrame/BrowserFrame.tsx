import type { ReactNode } from 'react';
import './BrowserFrame.css';

interface BrowserFrameProps {
  url: string;
  children: ReactNode;
}

function BrowserFrame({ url, children }: BrowserFrameProps) {
  return (
    <div className="browser-frame">
      <div className="browser-frame__toolbar">
        <div className="browser-frame__dots">
          <span />
          <span />
          <span />
        </div>
        <div className="browser-frame__address-bar">
          <span className="browser-frame__lock">🔒</span>
          <span className="browser-frame__url">{url}</span>
        </div>
      </div>
      <div className="browser-frame__content">{children}</div>
    </div>
  );
}

export default BrowserFrame;
