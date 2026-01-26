import React, { useState } from 'react';
import styles from '../Lobby.module.css';

interface Props {
  onConfirm: (url: string) => void;
}

const UrlInput: React.FC<Props> = ({ onConfirm }) => {
  const [url, setUrl] = useState('');

  return (
    <div className={styles.selectionArea}>
      <h2 className={styles.stepTitle}>Paste Playlist Link</h2>
      <div className={styles.inputArea}>
        <input 
          type="text" 
          placeholder="https://open.spotify.com/playlist/..." 
          className={styles.textInput}
          value={url}
          onChange={(e) => setUrl(e.target.value)}
        />
        <button 
          className={styles.primaryBtn}
          onClick={() => onConfirm(url)}
          disabled={!url.includes('spotify.com')}
        >
          Continue
        </button>
      </div>
    </div>
  );
};

export default UrlInput;