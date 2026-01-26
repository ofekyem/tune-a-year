import React from 'react';
import styles from '../Lobby.module.css';
import { Database, Music } from 'lucide-react';
import { MusicSource } from '../../../types';

interface Props {
  onSelect: (source: MusicSource) => void;
}

const SourceSelect: React.FC<Props> = ({ onSelect }) => {
  return (
    <div className={styles.selectionArea}>
      <h2 className={styles.stepTitle}>Select Music Source</h2>
      <div className={styles.buttonGroup}>
        <button 
          className={styles.modeBtn} 
          onClick={() => onSelect(MusicSource.LocalDatabase)}
        >
          <Database size={32} />
          <div className={styles.btnText}>
            <span>Game Database</span>
            <small>Play with our curated song collection</small>
          </div>
        </button>
        
        <button 
          className={styles.modeBtn} 
          onClick={() => onSelect(MusicSource.SpotifyPlaylist)}
        >
          <Music size={32} />
          <div className={styles.btnText}>
            <span>Spotify Playlist</span>
            <small>Import songs from a public playlist</small>
          </div>
        </button>
      </div>
    </div>
  );
};

export default SourceSelect;