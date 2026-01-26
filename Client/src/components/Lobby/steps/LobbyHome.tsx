import React from 'react';
import styles from '../Lobby.module.css';

interface Props {
  onCreateClick: () => void;
  onJoinClick: () => void;
}

const LobbyHome: React.FC<Props> = ({ onCreateClick, onJoinClick }) => {
  return (
    <div className={styles.buttonGroup}>
      <button className={styles.primaryBtn} onClick={onCreateClick}>
        Create A Game
      </button>
      <button className={styles.secondaryBtn} onClick={onJoinClick}>
        Join A Game
      </button>
    </div>
  );
};

export default LobbyHome;