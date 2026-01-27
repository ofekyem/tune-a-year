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
  
  // state for game session data
  const [session, setSession] = useState<BaseGameSession | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // SignalR connection
  const { connection } = useSignalR(session?.roomCode || null);

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

    return () => {
      connection.off("GameUpdated");
    };
  }, [connection]); 


  // function for handle guess submit 
  const handleGuessSubmit = async (targetIndex: number, title: string, artist: string) => {
    if (!sessionId || !session) return;
    
    const activePlayer = session.players[session.currentPlayerIndex];
    
    try {
      // send guess to server
      const response = await gameService.submitGuess(
        sessionId,
        activePlayer.id,
        targetIndex,
        title,
        artist
      );
      
      // update state with the new session returned from the server
      setSession(response.session);
      
      // here we can later trigger animations based on response.result
      console.log("Guess Result:", response.result);
    } catch (err) {
      console.error("Failed to submit guess:", err);
    }
  };

  // Initial display states
  if (loading) return <div className="loader">loading...</div>;
  if (error) return <div className="error">{error} <button onClick={() => navigate('/')}>back to lobby</button></div>;
  if (!session) return null; 

  // the active player whose turn it is
  const activePlayer = session.players[session.currentPlayerIndex];
  // players who are not the active player and show on top bar
  const inactivePlayers = session.players.filter((_, idx) => idx !== session.currentPlayerIndex);

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
        />
      </main>
    </div>
  );
};

export default GameBoard;