import React, { useState } from 'react';
import styles from '../Lobby.module.css';

interface Props {
  onJoin: (code: string, name: string) => void;
}

const JoinInput: React.FC<Props> = ({ onJoin }) => {
  const [code, setCode] = useState('');
  const [name, setName] = useState('');

  return (
    <div className={styles.selectionArea}>
      <h2 className={styles.stepTitle}>Join a Game</h2>
      <div className={styles.inputArea}>
        <input 
          type="text" 
          placeholder="Enter 4-digit Room Code" 
          className={styles.textInput}
          value={code}
          onChange={(e) => setCode(e.target.value.toUpperCase())}
          maxLength={4}
        />
        <input 
          type="text" 
          placeholder="Enter your name" 
          className={styles.textInput}
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <button 
          className={styles.primaryBtn}
          onClick={() => onJoin(code, name)}
          disabled={code.length !== 4 || !name.trim()}
        >
          Join Room
        </button>
      </div>
    </div>
  );
};

export default JoinInput;