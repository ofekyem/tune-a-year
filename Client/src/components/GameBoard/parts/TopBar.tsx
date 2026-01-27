import React from 'react';
import styles from '../GameBoard.module.css';
import PlayerMiniCard from './PlayerMiniCard';
import { LogOut } from 'lucide-react';

interface Props {
  players: any[];
  onExit: () => void;
}

const TopBar: React.FC<Props> = ({ players, onExit }) => {
  return (
    <div className={styles.topBar}>
      <div className={styles.inactivePlayersGroup}>
        {players.map((player) => (
          <PlayerMiniCard 
            key={player.id}
            name={player.name}
            tokens={player.tokens}
            timelineSize={player.timeline?.length || 0}
          />
        ))}
      </div>

      <button className={styles.exitBtn} onClick={onExit} title="Exit Game">
        <LogOut size={22} />
        <span>Exit</span>
      </button>
    </div>
  );
};

export default TopBar;