import React, { useState } from 'react';
import styles from '../GameBoard.module.css';
import TimelineCard from './TimelineCard';
import ActiveSongArea from './ActiveSongArea';
import type { Player, Song } from '../../../types';

interface Props {
  activePlayer: Player;
  currentSong: Song | null;
  onGuessSubmit: (targetIndex: number, title: string, artist: string) => void;
}

const GameTable: React.FC<Props> = ({ activePlayer, currentSong, onGuessSubmit }) => {
  // we use this to highlight the drop slot being hovered
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);

  return (
    <div className={styles.gameTable}>
      {/* Turn indicator */}
      <div className={styles.turnIndicator}>
       Turn Of: <strong>{activePlayer.name}</strong>
      </div>

      {/*Timeline with empty slots in between */}
      <div className={styles.timelineWrapper}>
        {/* First slot (before the first card) */}
        <div 
          className={`${styles.dropSlot} ${dragOverIndex === 0 ? styles.slotActive : ''}`}
          onDragOver={(e) => { e.preventDefault(); setDragOverIndex(0); }}
          onDragLeave={() => setDragOverIndex(null)}
          onDrop={() => { /* drop logic will go here */ }}
        >
          <span>+</span>
        </div>

        {activePlayer.timeline.map((card, index) => (
          <React.Fragment key={card.songId || index}>
            <TimelineCard card={card} />
            
            {/* Slot after each card */}
            <div 
              className={`${styles.dropSlot} ${dragOverIndex === index + 1 ? styles.slotActive : ''}`}
              onDragOver={(e) => { e.preventDefault(); setDragOverIndex(index + 1); }}
              onDragLeave={() => setDragOverIndex(null)}
            >
              <span>+</span>
            </div>
          </React.Fragment>
        ))}
      </div>

      {/*Active song area (the mysterious card) */}
      <div className={styles.activeArea}>
        <ActiveSongArea currentSong={currentSong} />
      </div>
    </div>
  );
};

export default GameTable;