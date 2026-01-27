import { useParams, useNavigate } from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import type { BaseGameSession } from '../../types';
import { gameService } from '../../services/gameService';
import { useSignalR } from '../../hooks/useSignalR';
import TopBar from './parts/TopBar';
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
      {/* top bar with inactive players */}
      <TopBar 
        players={inactivePlayers} 
        onExit={() => navigate('/')} 
      />

      <main className={styles.playArea}>
        {/* the table where we see the active player*/}
        <div className={styles.tableCenter}>
          <div className={styles.turnIndicator}>
            turn of: <strong>{activePlayer.name}</strong>
          </div>

          {/* the table component */}
          <div className={styles.placeholderTable}>
             [כאן נבנה את ה-GameTable עם ה-Timeline של {activePlayer.name}]
          </div>

          {/* the active song card */}
          {session.currentActiveSong && (
            <div className={styles.activeSongSection}>
              <p>השיר המתנגן כרגע...</p>
              {/* כאן תבוא קומפוננטת ה-ActiveSongCard */}
            </div>
          )}
        </div>
      </main>
    </div>
  );
};

export default GameBoard;