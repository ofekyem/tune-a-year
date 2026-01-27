import React from 'react';
import styles from '../GameBoard.module.css';
import { User, Coins, Layers } from 'lucide-react';

interface Props {
  name: string;
  tokens: number;
  timelineSize: number;
}

const PlayerMiniCard: React.FC<Props> = ({ name, tokens, timelineSize }) => {
  return (
    <div className={styles.playerMiniCard}>
      <div className={styles.playerInfo}>
        <User size={16} className={styles.icon} />
        <span className={styles.playerName}>{name}</span>
      </div>
      <div className={styles.playerStats}>
        <div className={styles.statItem} title="Cards">
          <Layers size={14} />
          <span>{timelineSize}</span>
        </div>
        <div className={styles.statItem} title="Tokens">
          <Coins size={14} />
          <span>{tokens}</span>
        </div>
      </div>
    </div>
  );
};

export default PlayerMiniCard;