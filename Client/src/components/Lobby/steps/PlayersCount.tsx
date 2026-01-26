import React, { useState } from 'react';
import styles from '../Lobby.module.css';
import { Users, UserMinus, UserPlus } from 'lucide-react';

interface Props {
  onSelect: (count: number) => void;
  initialCount?: number;
}

const PlayersCount: React.FC<Props> = ({ onSelect, initialCount = 2 }) => {
  const [count, setCount] = useState(initialCount);
  const minPlayers = 2;
  const maxPlayers = 8;

  const handleIncrement = () => {
    if (count < maxPlayers) setCount(count + 1);
  };

  const handleDecrement = () => {
    if (count > minPlayers) setCount(count - 1);
  };

  return (
    <div className={styles.selectionArea}>
      <h2 className={styles.stepTitle}>How many players?</h2>
      
      <div className={styles.counterContainer}>
        <button 
          className={styles.counterBtn} 
          onClick={handleDecrement}
          disabled={count <= minPlayers}
        >
          <UserMinus size={24} />
        </button>
        
        <div className={styles.countDisplay}>
          <span className={styles.countNumber}>{count}</span>
          <Users size={32} className={styles.countIcon} />
        </div>

        <button 
          className={styles.counterBtn} 
          onClick={handleIncrement}
          disabled={count >= maxPlayers}
        >
          <UserPlus size={24} />
        </button>
      </div>

      <button 
        className={styles.primaryBtn}
        onClick={() => onSelect(count)}
      >
        Continue
      </button>
    </div>
  );
};

export default PlayersCount;