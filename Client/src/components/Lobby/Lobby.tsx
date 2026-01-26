import React, { useState } from 'react';
import styles from './Lobby.module.css';
import logoLight from '../../assets/LightNo.png';
import logoDark from '../../assets/DarkNo.png';
import { Settings, Sun, Moon, ArrowLeft } from 'lucide-react';
import { GameMode, MusicSource, type MatchConfiguration } from '../../types';
import LobbyHome from './steps/LobbyHome';
import ModeSelect from './steps/ModeSelect';

type LobbyStep = 'HOME' | 'MODE_SELECT' | 'SOURCE_SELECT' | 'CONFIG' | 'PLAYERS_INPUT' | 'WAITING';

const Lobby: React.FC = () => {
  const [isDark, setIsDark] = useState(false);
  const [step, setStep] = useState<LobbyStep>('HOME');
  const [config, setConfig] = useState<MatchConfiguration>({
    mode: GameMode.SingleDevice,
    source: MusicSource.LocalDatabase,
    playlistUrl: null,
    languages: [],
    maxPlayers: 2,
    winningScore: 10,
    creatorName: null,
    localPlayerNames: null,
  });

  const toggleTheme = () => {
    setIsDark(!isDark);
    document.documentElement.setAttribute('data-theme', !isDark ? 'dark' : 'light');
  };

  // פונקציה שתעזור לנו לחזור אחורה בשלבים
  const goBack = () => {
    if (step === 'MODE_SELECT') setStep('HOME');
    if (step === 'SOURCE_SELECT') setStep('MODE_SELECT');
    // ... המשך לפי הצורך
  };

  return (
    <div className={styles.lobbyContainer}>
      <header className={styles.header}>
        {step !== 'HOME' && (
          <button className={styles.backBtn} onClick={goBack}>
            <ArrowLeft size={24} /> Back
          </button>
        )}
        <div className={styles.headerIcons}>
          <button className={styles.iconBtn} onClick={toggleTheme}>
            {isDark ? <Sun size={24} /> : <Moon size={24} />}
          </button>
          <button className={styles.iconBtn}><Settings size={24} /></button>
        </div>
      </header>

      <main className={styles.mainContent}>
        <div className={styles.logoWrapper}>
          <img src={isDark ? logoDark : logoLight} alt="logo" className={styles.logo} />
        </div>

        {/* components change by mode */}
        {step === 'HOME' && (
          <LobbyHome onCreateClick={() => setStep('MODE_SELECT')} />
        )}

        {step === 'MODE_SELECT' && (
          <ModeSelect 
            onSelect={(mode) => {
              setConfig(prev => ({ ...prev, mode }));
              setStep('SOURCE_SELECT');
            }} 
          />
        )}
      </main>
    </div>
  );
};

export default Lobby;