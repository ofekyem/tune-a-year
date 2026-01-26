import React, { useState } from 'react';
import styles from './Lobby.module.css';
import logoLight from '../../assets/LightNo.png';
import logoDark from '../../assets/DarkNo.png';
import { Settings, Sun, Moon } from 'lucide-react';

const Lobby: React.FC = () => {
    // Theme state to toggle between light and dark modes
  const [isDark, setIsDark] = useState(false);
  // Step state to manage different lobby views
  const [step, setStep] = useState<'HOME' | 'MODE_SELECT'>('HOME');

  // Function to toggle theme
  const toggleTheme = () => {
    setIsDark(!isDark);
    document.documentElement.setAttribute('data-theme', !isDark ? 'dark' : 'light');
  };

  return (
    <div className={styles.lobbyContainer}>
      {/* header with settings buttons */}
      <header className={styles.header}>
        <button className={styles.iconBtn} onClick={toggleTheme}>
          {isDark ? <Sun size={24} /> : <Moon size={24} />}
        </button>
        <button className={styles.iconBtn}>
          <Settings size={24} />
        </button>
      </header>

      {/* logo and title area */}
      <main className={styles.mainContent}>
        <div className={styles.logoWrapper}>
          <img 
            src={isDark ? logoDark : logoLight} 
            alt="tune a year logo" 
            className={styles.logo} 
          />
        </div>

        {step === 'HOME' && (
          <div className={styles.buttonGroup}>
            <button 
              className={styles.primaryBtn}
              onClick={() => setStep('MODE_SELECT')}
            >
             Create A Game
            </button>
            <button className={styles.secondaryBtn}>
             Join A Game
            </button>
          </div>
        )}

        {/* other steps will come here according to your plan */}
      </main>
    </div>
  );
};

export default Lobby;