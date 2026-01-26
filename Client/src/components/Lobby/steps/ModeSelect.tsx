import React from 'react';
import styles from '../Lobby.module.css';
import { Users, Globe } from 'lucide-react';
import { GameMode } from '../../../types';

interface Props {
  onSelect: (mode: GameMode) => void;
}

const ModeSelect: React.FC<Props> = ({ onSelect }) => {
  return (
    <div className={styles.selectionArea}>
      <h2 className={styles.stepTitle}>Choose Game Mode</h2>
      <div className={styles.buttonGroup}>
        <button 
          className={styles.modeBtn} 
          onClick={() => onSelect(GameMode.SingleDevice)}
        >
          <Users size={32} />
          <div className={styles.btnText}>
            <span>Local Party</span>
            <small>Play with friends on this device (Offline)</small>
          </div>
        </button>
        
        <button 
          className={styles.modeBtn} 
          onClick={() => onSelect(GameMode.Online)}
        >
          <Globe size={32} />
          <div className={styles.btnText}>
            <span>Online Game</span>
            <small>Play with friends remotely</small>
          </div>
        </button>
      </div>
    </div>
  );
};

export default ModeSelect;