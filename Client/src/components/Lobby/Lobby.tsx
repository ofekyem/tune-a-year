import React, { useState } from 'react';
import styles from './Lobby.module.css';
import logoLight from '../../assets/LightNo.png';
import logoDark from '../../assets/DarkNo.png';
import { Settings, Sun, Moon, ArrowLeft } from 'lucide-react';
import { GameMode, MusicSource, type MatchConfiguration } from '../../types';
import LobbyHome from './steps/LobbyHome';
import ModeSelect from './steps/ModeSelect';
import SourceSelect from './steps/SourceSelect';
import UrlInput from './steps/UrlInput'; 
import LanguageSelect from './steps/LanguageSelect';
import PlayersCount from './steps/PlayersCount'; 
import MatchConfig from './steps/MatchConfig'; 
import WaitingRoom from './steps/WaitingRoom';
import { gameService } from '../../services/gameService';

type LobbyStep = 'HOME' | 'MODE_SELECT' | 'SOURCE_SELECT' | 'URL_INPUT' | 'LANG_SELECT' | 'PLAYERS_COUNT' | 'CONFIG' | 'WAITING' | 'JOIN_INPUT';

const Lobby: React.FC = () => {
  const [isDark, setIsDark] = useState(false);
  const [step, setStep] = useState<LobbyStep>('HOME'); 
  const [roomCode, setRoomCode] = useState<string | null>(null);
  const [sessionId, setSessionId] = useState<string | null>(null);
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

  // go back button handler
  const goBack = () => {
    if (step === 'MODE_SELECT') setStep('HOME');
    else if (step === 'SOURCE_SELECT') setStep('MODE_SELECT');
    else if (step === 'URL_INPUT') setStep('SOURCE_SELECT');
    else if (step === 'LANG_SELECT') setStep('SOURCE_SELECT');
    else if (step === 'PLAYERS_COUNT') {
      if (config.source === MusicSource.SpotifyPlaylist) setStep('URL_INPUT');
      else setStep('LANG_SELECT');
    }
    else if (step === 'CONFIG') setStep('PLAYERS_COUNT');
  }; 

  // helper functions to update Config
  const handleSourceSelect = (source: MusicSource) => {
    setConfig(prev => ({ ...prev, source }));
    // split logic based on your choice
    if (source === MusicSource.SpotifyPlaylist) {
      setStep('URL_INPUT');
    } else {
      setStep('LANG_SELECT');
    }
  }; 

  // URL confirm handler
  const handleUrlConfirm = (url: string) => {
    setConfig(prev => ({ ...prev, playlistUrl: url }));
    setStep('PLAYERS_COUNT');
  }; 

  // Language select handler
  const handleLanguageSelect = (selectedLangs: string[]) => {
    setConfig(prev => ({ ...prev, languages: selectedLangs }));
    setStep('PLAYERS_COUNT');
  }; 

  // Players count select handler
  const handlePlayersCountSelect = (count: number) => {
    setConfig(prev => ({ ...prev, maxPlayers: count }));
    setStep('CONFIG');
  }; 

  // Match configuration complete handler
  const handleCreateGame = async (finalConfig: MatchConfiguration) => {
    try {
      // create game session on server
      const session = await gameService.createGame(finalConfig);
      
      // update config state with data returned from server (ID and room code)
      setConfig(prev => ({ 
        ...prev, 
        sessionId: session.id, 
        roomCode: session.roomCode 
      }));

      if (finalConfig.mode === GameMode.SingleDevice) {
        // Local mode - start the first round immediately
        console.log("Local game started! Redirecting to Board...");
        // Here comes the function that will take you to the game screen
        // onGameStart(session.id); 
      } else {
        setRoomCode(session.roomCode);
        setSessionId(session.id);
        // Online mode - move to waiting screen
        setStep('WAITING');
      }
    } catch (error) {
      console.error("Error creating game:", error);
      // here it is recommended to add a Toast or Alert for the user
    }
  }; 

  const handleStartOnlineGame = async () => {};

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

        {/* Music Source selection step */}
        {step === 'SOURCE_SELECT' && (
          <SourceSelect onSelect={handleSourceSelect} />
        )} 

        {step === 'URL_INPUT' && (
          <UrlInput onConfirm={handleUrlConfirm} />
        )} 

        {/* Language selection step - relevant only for Local Database */}
        {step === 'LANG_SELECT' && (
          <LanguageSelect onSelect={handleLanguageSelect} />
        )} 

        {/* Players count selection step */}
        {step === 'PLAYERS_COUNT' && (
          <PlayersCount 
            initialCount={config.maxPlayers}
            onSelect={handlePlayersCountSelect} 
          />
        )} 

        {/* Match configuration step */}
        {step === 'CONFIG' && (
          <MatchConfig 
            config={config} 
            onComplete={handleCreateGame} 
          />
        )} 

        {step === 'WAITING' && roomCode && (
          <WaitingRoom 
            roomCode={roomCode} 
            isHost={true} 
            onStart={handleStartOnlineGame} // פונקציה שתקרא ל-startGame בשרת
          />
        )}


      </main>
    </div>
  );
};

export default Lobby;