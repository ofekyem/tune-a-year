import React from 'react';
import styles from '../Lobby.module.css';
import { Users, Play, Copy, Check } from 'lucide-react';

interface Props {
  roomCode: string;
  isHost: boolean;
  onStart: () => void;
}

const WaitingRoom: React.FC<Props> = ({ roomCode, isHost, onStart }) => {
  const [copied, setCopied] = React.useState(false);

  const copyToClipboard = () => {
    navigator.clipboard.writeText(roomCode);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className={styles.selectionArea}>
      <h2 className={styles.stepTitle}>Waiting Room</h2>
      
      <div className={styles.roomCodeBox}>
        <p>Share this code with friends:</p>
        <div className={styles.codeDisplay} onClick={copyToClipboard}>
          <span>{roomCode}</span>
          {copied ? <Check size={20} color="#10b981" /> : <Copy size={20} />}
        </div>
      </div>

      <div className={styles.statusBox}>
        <div className={styles.loaderGroup}>
          <span className={styles.loadingPulse}></span>
          <p>Waiting for players to join...</p>
        </div>
        <div className={styles.playerCount}>
          <Users size={20} />
          <span>At least 2 players required</span>
        </div>
      </div>

      {isHost && (
        <button className={styles.primaryBtn} onClick={onStart}>
          <Play size={20} /> Start Game Now
        </button>
      )}

      {!isHost && (
        <p className={styles.waitingNotice}>Only the host can start the match.</p>
      )}
    </div>
  );
};

export default WaitingRoom;