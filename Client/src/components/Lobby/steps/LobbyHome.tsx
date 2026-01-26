import React from 'react';
import styles from '../Lobby.module.css';

interface Props {
  onCreateClick: () => void;
}

const LobbyHome: React.FC<Props> = ({ onCreateClick }) => {
  return (
    <div className={styles.buttonGroup}>
      <button className={styles.primaryBtn} onClick={onCreateClick}>
        Create A Game
      </button>
      <button className={styles.secondaryBtn}>
        Join A Game
      </button>
    </div>
  );
};

export default LobbyHome;