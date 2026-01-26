import React, { useState } from 'react';
import styles from '../Lobby.module.css';
import { GameMode, type MatchConfiguration } from '../../../types';
import { PlayCircle, User } from 'lucide-react';

interface Props {
  config: MatchConfiguration;
  onComplete: (updatedConfig: MatchConfiguration) => void;
}

const MatchConfig: React.FC<Props> = ({ config, onComplete }) => {
  // state for local player names in single device mode
  const [localNames, setLocalNames] = useState<string[]>(
    Array(config.maxPlayers).fill('')
  );
  
  // state for host name in online mode
  const [hostName, setHostName] = useState('');

  const handleLocalNameChange = (index: number, value: string) => {
    const newNames = [...localNames];
    newNames[index] = value;
    setLocalNames(newNames);
  };

  const isFormValid = () => {
    if (config.mode === GameMode.SingleDevice) {
      return localNames.every(name => name.trim() !== '');
    }
    return hostName.trim() !== '';
  };

  const handleSubmit = () => {
    if (!isFormValid()) return;

    const finalConfig: MatchConfiguration = {
      ...config,
      creatorName: config.mode === GameMode.Online ? hostName : null,
      localPlayerNames: config.mode === GameMode.SingleDevice ? localNames : null,
    };

    onComplete(finalConfig);
  };

  return (
    <div className={styles.selectionArea}>
      <h2 className={styles.stepTitle}>
        {config.mode === GameMode.Online ? 'Host Profile' : 'Player Names'}
      </h2>

      <div className={styles.inputArea}>
        {config.mode === GameMode.Online ? (
          <div className={styles.inputWrapper}>
            <label>Your Name (Host)</label>
            <div className={styles.inputWithIcon}>
              <User size={20} />
              <input 
                type="text"
                placeholder="Enter your name..."
                className={styles.textInput}
                value={hostName}
                onChange={(e) => setHostName(e.target.value)}
              />
            </div>
          </div>
        ) : (
          <div className={styles.playersGrid}>
            {localNames.map((name, index) => (
              <div key={index} className={styles.inputWrapper}>
                <label>Player {index + 1}</label>
                <input 
                  type="text"
                  placeholder={`Name for Player ${index + 1}`}
                  className={styles.textInput}
                  value={name}
                  onChange={(e) => handleLocalNameChange(index, e.target.value)}
                />
              </div>
            ))}
          </div>
        )}

        <button 
          className={styles.primaryBtn}
          onClick={handleSubmit}
          disabled={!isFormValid()}
        >
          <PlayCircle size={20} /> Create Room
        </button>
      </div>
    </div>
  );
};

export default MatchConfig;