import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
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
import JoinInput from './steps/JoinInput';
import { gameService } from '../../services/gameService';
import {useSignalR} from '../../hooks/useSignalR';

type LobbyStep = 'HOME' | 'MODE_SELECT' | 'SOURCE_SELECT' | 'URL_INPUT' | 'LANG_SELECT' | 'PLAYERS_COUNT' | 'CONFIG' | 'WAITING' | 'JOIN_INPUT';

const Lobby: React.FC = () => {
  const navigate = useNavigate();
  const [isDark, setIsDark] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [step, setStep] = useState<LobbyStep>('HOME'); 
  const [roomCode, setRoomCode] = useState<string | null>(null);
  const [isHost, setIsHost] = useState(true);
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
  // for signalR
  const [players, setPlayers] = useState<any[]>([]);
  const { connection } = useSignalR(roomCode); 
  
  // SignalR event handlers
  useEffect(() => {
    if (!connection) return;

    // listen for player list updates
    connection.on("PlayerJoined", (newPlayer: any) => {
      setPlayers(prev => {
        if (prev.find(p => p.id === newPlayer.id)) return prev;
        // add the new player and sort by join order
        const updatedList = [...prev, newPlayer];
        return updatedList.sort((a, b) => a.joinOrder - b.joinOrder);
      });
    });

    // listen for game start (for players who are not the host)
    connection.on("GameStarted", (session) => {
      console.log("Game is starting!", session);
      navigate(`/game/${session.id}`);
    });

    // Cleanup: remove listeners when component unmounts
    return () => {
      connection.off("PlayerJoined");
      connection.off("GameStarted");
    };
  }, [connection]);

  const toggleTheme = () => {
    setIsDark(!isDark);
    document.documentElement.setAttribute('data-theme', !isDark ? 'dark' : 'light');
  };

  // go back button handler
  const goBack = () => {
    if (step === 'MODE_SELECT') setStep('HOME');
    else if (step === 'JOIN_INPUT') setStep('HOME');
    else if (step === 'SOURCE_SELECT') setStep('MODE_SELECT');
    else if (step === 'URL_INPUT') setStep('SOURCE_SELECT');
    else if (step === 'LANG_SELECT') setStep('SOURCE_SELECT');
    else if (step === 'PLAYERS_COUNT') {
      if (config.source === MusicSource.SpotifyPlaylist) setStep('URL_INPUT');
      else setStep('LANG_SELECT');
    }
    else if (step === 'CONFIG') setStep('PLAYERS_COUNT');
  }; 

  // handler for music source selection
  const handleSourceSelect = (source: MusicSource) => {
    setConfig(prev => ({ ...prev, source }));
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
    // load mode on
    setIsLoading(true);
    try {
      // create game session on server
      const session = await gameService.createGame(finalConfig);
      
      // save host player ID to local storage
      if (session.players && session.players.length > 0) {
        const host = session.players[0];
        localStorage.setItem('myPlayerId', host.id);
        console.log("Host ID saved:", host.id);
      }
     
      
      // update config state with data returned from server (ID and room code)
      setConfig(prev => ({ 
        ...prev, 
        sessionId: session.id, 
        roomCode: session.roomCode 
      }));

      if (finalConfig.mode === GameMode.SingleDevice) {
        // Local mode - start the first round immediately
        console.log("Local game started! Redirecting to Board...");
        navigate(`/game/${session.id}`);
        // Here comes the function that will take you to the game screen
      } else {
        setRoomCode(session.roomCode);
        setSessionId(session.id);
        if (finalConfig.creatorName) {
          setPlayers([{ name: finalConfig.creatorName, joinOrder: 0 }]);
        }
        // Online mode - move to waiting screen
        setStep('WAITING');
      }
    } catch (error) {
      console.error("Error creating game:", error);
      // here add a Toast or Alert for the user
    }
    finally {
        setIsLoading(false); // stop loading spinner
    }
  }; 

  // Join game handler
  const handleJoinGame = async (code: string, name: string) => {
    try {
      const { session, playerId } = await gameService.joinGame(code, name); 

      // save the viewing player ID to local storage
      localStorage.setItem('myPlayerId', playerId); 
      console.log("Joined successfully. My ID:", playerId); 
      
      // update state with server response
      setRoomCode(session.roomCode);
      setSessionId(session.id);
      setIsHost(false); 
      
      // sort the list of players by join order
      const sortedPlayers = [...session.players].sort(
        (a: any, b: any) => a.joinOrder - b.joinOrder
      );
      setPlayers(sortedPlayers);

      setStep('WAITING');
    } catch (error) {
      console.error("Join error:", error);
      alert("Invalid room code or room is full.");
    }
  };

  // Start game handler for online mode
  const handleStartOnlineGame = async () => {
    if (!sessionId) return;
    try {
      // start game on server
      await gameService.startGame(sessionId);
      
      // here navigate to the game screen
    } catch (error) {
      console.error("Failed to start game:", error);
      alert("Failed to start game. Please try again.");
    }
  };

  return (
    <div className={styles.lobbyContainer}>
      <header className={styles.header}>
        {step !== 'HOME' && (
          <button className={styles.backBtn} onClick={goBack}>
            <ArrowLeft size={30} /> Back
          </button>
        )}
        <div className={styles.headerIcons}>
          <button className={styles.iconBtn} onClick={toggleTheme}>
            {isDark ? <Sun size={30} /> : <Moon size={30} />}
          </button>
          <button className={styles.iconBtn}><Settings size={30} /></button>
        </div>
      </header>

      <main className={styles.mainContent}>
        <div className={styles.logoWrapper}>
          <img src={isDark ? logoDark : logoLight} alt="logo" className={styles.logo} />
        </div>

        {/* home step */}
        {step === 'HOME' && (
          <LobbyHome 
            onCreateClick={() => {
              setIsHost(true);
              setStep('MODE_SELECT');
            }} 
            onJoinClick={() => setStep('JOIN_INPUT')} 
          /> 
        )} 

        {/* Join game step */}
        {step === 'JOIN_INPUT' && (
          <JoinInput onJoin={handleJoinGame} />
        )}

        {/* Game mode selection step */}
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
        {/* Playlist URL input step - relevant only for using Spotify Playlist mode */}
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
            isLoading={isLoading} 
          />
        )} 

        {/* Waiting room step for online mode */}
        {step === 'WAITING' && roomCode && (
          <WaitingRoom 
            roomCode={roomCode} 
            isHost={isHost}
            players={players}
            onStart={handleStartOnlineGame} 
          />
        )}


      </main>
    </div>
  );
};

export default Lobby;