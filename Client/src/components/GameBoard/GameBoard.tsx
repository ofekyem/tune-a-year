import { useParams, useNavigate } from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import type { BaseGameSession } from '../../types';
import { gameService } from '../../services/gameService';
import { useSignalR } from '../../hooks/useSignalR';
import TopBar from './parts/TopBar';
import GameTable from './parts/GameTable';
import styles from './GameBoard.module.css';

const GameBoard: React.FC = () => {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();
  const [lastResult, setLastResult] = useState<any>(null);
  const [showResultOverlay, setShowResultOverlay] = useState(false);
  const [displayPlayerIndex, setDisplayPlayerIndex] = useState<number | null>(null);
  // state for the player that is currently viewing the game
  const [localPlayerId, setLocalPlayerId] = useState<string | null>(localStorage.getItem('myPlayerId'));
  
  
  // state for game session data
  const [session, setSession] = useState<BaseGameSession | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  

  // SignalR connection
  const { connection } = useSignalR(session?.roomCode || null);
  

  useEffect(() => {
    // save theme on reloads
    const savedTheme = document.documentElement.getAttribute('data-theme');
    if (savedTheme) {
      // make sure to reapply the saved theme
      document.documentElement.setAttribute('data-theme', savedTheme);
    }
  }, []);

  useEffect(() => {
    const initGame = async () => {
      if (!sessionId) return;

      try {
        setLoading(true);
        // Fetch data from server (port 5149)
        const data = await gameService.getSession(sessionId);
        
        // if online mode, sort players by join order
        if (data.config.mode === 1 && data.players) { 
            data.players.sort((a: any, b: any) => (a.joinOrder ?? 0) - (b.joinOrder ?? 0));
        }
        
        setSession(data);
      } catch (err) {
        console.error("Error fetching session:", err);
        setError("game session not found or an error occurred.");
      } finally {
        setLoading(false);
      }
    };

    initGame();
  }, [sessionId]);

  // Listen for live updates from the server during the game
  useEffect(() => {
    if (!connection) return;

    connection.on("GameUpdated", (updatedSession: BaseGameSession) => {
        console.log("Received update from server!", updatedSession); 


        // Sorting for online mode
        if (updatedSession.config.mode === 1 && updatedSession.players) {
            updatedSession.players.sort((a: any, b: any) => (a.joinOrder ?? 0) - (b.joinOrder ?? 0));
        }
      setSession(updatedSession);
    }); 

    connection.on("GuessResultReceived", (result: any) => {
        console.log("Someone made a guess!", result);
        // the player who made the guess
        const guessingPlayerIdx = session?.players.findIndex(p => p.id === result.playerId); 

        if (guessingPlayerIdx !== -1 && guessingPlayerIdx !== undefined) {
          setDisplayPlayerIndex(guessingPlayerIdx); // everyone sees who made the guess
        }
        setLastResult(result);
        setShowResultOverlay(true);

        // hide overlay after delay
        setTimeout(() => {
            setShowResultOverlay(false);
            setLastResult(null);
            setDisplayPlayerIndex(null);
        }, 4000);
    });

    return () => {
      connection.off("GameUpdated");
    };
  }, [connection]); 


  // function for handle guess submit 
  const handleGuessSubmit = async (targetIndex: number, title: string, artist: string) => {
    if (!sessionId || !session) return;
    
    // save the current index before update
    const currentIndexBeforeUpdate = session.currentPlayerIndex;
    
    try {
      // send guess to server
      const response = await gameService.submitGuess(
        sessionId,
        session.players[currentIndexBeforeUpdate].id,
        targetIndex,
        title,
        artist
      ); 

      setSession(response.session); 

      // update display player index to show correct overlay
      setDisplayPlayerIndex(currentIndexBeforeUpdate);

      // save last result and show overlay
      setLastResult(response.result);
      setShowResultOverlay(true);
      
      // delay so player can see the result after guess
      setTimeout(() => {
        setShowResultOverlay(false);
        // update session state with latest data after delay
        setDisplayPlayerIndex(null);
      }, 4000);
      
      
    } catch (err) {
      console.error("Failed to submit guess:", err);
    } 
    
  }; 
  

  // Initial display states
  if (loading) return <div className="loader">loading...</div>;
  if (error) return <div className="error">{error} <button onClick={() => navigate('/')}>back to lobby</button></div>;
  if (!session) return null; 

  // the active player whose turn it is
  const activePlayer = session.players[displayPlayerIndex ?? session.currentPlayerIndex];
  // players who are not the active player and show on top bar
  const inactivePlayers = session.players.filter((_, idx) => idx !== session.currentPlayerIndex); 
  const isLocalMode = session?.config.mode === 0;
  const isMyTurn = isLocalMode || (activePlayer.id === localPlayerId);

  return (
    <div className={styles.gameContainer}>
      <TopBar 
        players={inactivePlayers} 
        onExit={() => navigate('/')} 
      /> 

      <main className={styles.playArea}>
        {/* Table of the active Player*/}
        <GameTable 
          activePlayer={activePlayer}
          currentSong={session.currentActiveSong}
          onGuessSubmit={handleGuessSubmit}
          lastResult={lastResult}
          isMyTurn={isMyTurn}
        />
      </main> 


      {showResultOverlay && lastResult && (
        <div className={`${styles.resultOverlay} ${lastResult.placementCorrect ? styles.correct : styles.wrong}`}>
          <div className={styles.resultContent}>
            <h2>{lastResult.placementCorrect ? 'Congratulations! Correct Placement' : 'Oops... Wrong Placement'}</h2>
            
            <div className={styles.resultSongInfo}>
              <p>This is: <strong>{lastResult.correctArtist} - {lastResult.correctTitle}</strong></p>
              <span className={styles.resultYear}>{lastResult.correctYear}</span>
            </div>

            {lastResult.bonusEarned && (
              <div className={styles.bonusBadge}>
                ✨ Bonus Token! You also got the details right ✨
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default GameBoard;