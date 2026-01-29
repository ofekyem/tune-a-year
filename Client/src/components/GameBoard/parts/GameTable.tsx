import React, { useState } from 'react';
import styles from '../GameBoard.module.css';
import TimelineCard from './TimelineCard';
import ActiveSongArea from './ActiveSongArea';
import type { Player, Song } from '../../../types';

interface Props {
  activePlayer: Player;
  currentSong: Song | null;
  onGuessSubmit: (targetIndex: number, title: string, artist: string) => void;
  lastResult?: any;
  isMyTurn?: boolean;
  isResultShowing: boolean;
}

const GameTable: React.FC<Props> = ({ activePlayer, currentSong, onGuessSubmit, lastResult, isMyTurn, isResultShowing }) => {
  // we use this to highlight the drop slot being hovered
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null); 
  const [showModal, setShowModal] = useState(false);
  const [pendingIndex, setPendingIndex] = useState<number | null>(null);
  const [titleGuess, setTitleGuess] = useState("");
  const [artistGuess, setArtistGuess] = useState(""); 

  // handle when a card is dropped into a slot
  const handleDrop = (index: number) => {
    setDragOverIndex(null);
    setPendingIndex(index);
    setShowModal(true); 
  }; 

  // handle submit of the guess 
  const handleSubmit = () => {
    if (pendingIndex !== null) {
      onGuessSubmit(pendingIndex, titleGuess, artistGuess);
      // set back to initial state
      setShowModal(false);
      setTitleGuess("");
      setArtistGuess("");
      setPendingIndex(null);
    }
  };

  return (
    <div className={styles.gameTable}>
      <div className={styles.topStatusArea}>
        {/* Turn indicator */}
        <div className={styles.turnIndicator}>
        Turn Of:   <strong>{activePlayer.name}</strong>
        </div> 

        {/* Waiting badge for non-active players */}
        {!isMyTurn && (
          <div className={styles.inlineWaitingBadge}>
            <div className={styles.miniLoader}></div>
            <span>Waiting for move...</span>
          </div>
        )} 
      </div>

      {/*Timeline with empty slots in between */}
      <div className={`${styles.timelineWrapper} ${!isMyTurn ? styles.readOnly : ''}`}>
        {/* First slot before the first card (only for active player) */}
        {isMyTurn && (
          <div 
            className={`${styles.dropSlot} ${dragOverIndex === 0 ? styles.slotActive : ''}`}
            onDragOver={(e) => { e.preventDefault(); setDragOverIndex(0); }}
            onDragLeave={() => setDragOverIndex(null)}
            onDrop={() => handleDrop(0)}
          >
            <span>+</span>
          </div>
        )}

        {activePlayer.timeline.map((card, index) => {
          // check if the last guess was correct for this card to trigger animation
          const isNewCard = lastResult?.placementCorrect && 
                          card.releaseYear === lastResult.correctYear &&
                          card.title === lastResult.correctTitle;

          return (
            <React.Fragment key={card.songId || index}>
              <TimelineCard card={card} isNew={isNewCard} />
              
              {/* Slot after each card show only for players that its turn */}
              {isMyTurn && (
                <div 
                  className={`${styles.dropSlot} ${dragOverIndex === index + 1 ? styles.slotActive : ''}`}
                  onDragOver={(e) => { e.preventDefault(); setDragOverIndex(index + 1); }}
                  onDragLeave={() => setDragOverIndex(null)}
                  onDrop={() => handleDrop(index + 1)}
                >
                  <span>+</span>
                </div>
              )}
            </React.Fragment>
          );
        })}
      </div>
      
      

      {/*Active song area (the mysterious card) */}
      <div className={styles.activeArea}>
        <ActiveSongArea currentSong={currentSong} isMyTurn={isMyTurn ?? false} isResultShowing={isResultShowing} />
      </div> 

      {/* Guess modal */}
      {showModal && (
        <div className={styles.modalOverlay}>
          <div className={styles.guessModal}>
            <h3>Are you sure about this position?</h3>
            <p>Bonus token: Guess the details (optional)</p>
            
            <input 
              className={styles.modalInput}
              placeholder="Song Title..." 
              value={titleGuess}
              onChange={(e) => setTitleGuess(e.target.value)}
            />
            <input 
              className={styles.modalInput}
              placeholder="Artist Name..." 
              value={artistGuess}
              onChange={(e) => setArtistGuess(e.target.value)}
            />
            
            <div className={styles.modalActions}>
              <button onClick={() => setShowModal(false)} className={styles.cancelBtn}>Cancel</button>
              <button onClick={handleSubmit} className={styles.confirmBtn}>Confirm Guess</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default GameTable;