import React from 'react';
import styles from '../Lobby.module.css';
import { Users, Play, Copy, Check, UserCheck } from 'lucide-react';

interface Player {
  name: string;
  joinOrder: number;
} 

interface Props {
  roomCode: string;
  isHost: boolean;
  players: Player[];
  onStart: () => void;
}

const WaitingRoom: React.FC<Props> = ({ roomCode, isHost, players, onStart }) => {
  const [copied, setCopied] = React.useState(false);

  const sortedPlayers = [...players].sort((a, b) => a.joinOrder - b.joinOrder);
  console.log("Sorted players:", sortedPlayers);

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

      <div className={styles.playersContainer}>
        <h3 className={styles.sectionSubtitle}>
          <Users size={18} /> Players in Lobby ({players.length})
        </h3>
        <div className={styles.playersGrid}>
          {sortedPlayers.map((player, index) => (
            <div key={player.name} className={styles.playerCard}>
              <UserCheck size={16} className={styles.playerIcon} />
              <span>{player.name}</span>
              {index === 0 && <span className={styles.hostBadge}>Host</span>}
            </div>
          ))}
        </div>
      </div>

      <div className={styles.statusBox}>
        {players.length < 2 ? (
          <div className={styles.loaderGroup}>
            <span className={styles.loadingPulse}></span>
            <p>Waiting for more players to join...</p>
          </div>
        ) : (
          <p className={styles.readyText}>Ready to start!</p>
        )}
      </div>

      {isHost && (
        <button 
          className={styles.primaryBtn} 
          onClick={onStart}
          disabled={players.length < 2}>
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