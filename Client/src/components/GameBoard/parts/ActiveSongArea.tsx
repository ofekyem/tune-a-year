import React, { useEffect, useRef, useState } from 'react';
import styles from '../GameBoard.module.css';
import { Music, Play, Pause, Volume2 } from 'lucide-react';
import type { Song } from '../../../types/song';

interface Props {
  currentSong: Song | null;
  isMyTurn: boolean;
  isResultShowing: boolean;
}

const ActiveSongArea: React.FC<Props> = ({ currentSong, isMyTurn, isResultShowing }) => {
  const [isPlaying, setIsPlaying] = useState(false);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  // Handle audio playback when currentSong changes
  useEffect(() => {
    // Stop any existing audio
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current = null;
      setIsPlaying(false);
    } 
    // Start new audio if there's a current song and results are not showing
    if (currentSong?.previewUrl && !isResultShowing) {
      audioRef.current = new Audio(currentSong.previewUrl);
      audioRef.current.loop = true;
      
      audioRef.current.play()
        .then(() => setIsPlaying(true))
        .catch(() => setIsPlaying(false));
    }

    return () => {
      // Cleanup: stop the audio when the component unmounts or the song changes
      audioRef.current?.pause();
      audioRef.current = null;
    };
  }, [currentSong, isResultShowing]);

  const togglePlay = () => {
    if (!audioRef.current) return;
    
    if (isPlaying) {
      audioRef.current.pause();
    } else {
      audioRef.current.play();
    }
    setIsPlaying(!isPlaying);
  }; 

  // Drag start handler for the mystery card
  const handleDragStart = (e: React.DragEvent) => {
    e.dataTransfer.setData("text/plain", "mystery_card");
    e.dataTransfer.effectAllowed = "move";
  };

  if (!currentSong) return null;

  return (
    <div className={styles.activeSongArea}>
      <div className={styles.mysteryCardContainer}>
        {/* The hidden card that will be dragged later */}
        <div 
          className={styles.mysteryCard}
          draggable={isMyTurn}
          onDragStart={isMyTurn ? handleDragStart : (e) => e.preventDefault()}
        >
          <div className={styles.cardPattern}>
            <Music size={60} className={styles.musicIcon} />
          </div>
          
          <div className={styles.audioOverlay}>
            <button className={styles.playControlBtn} onClick={isMyTurn ? togglePlay : undefined} style={{ cursor: isMyTurn ? 'pointer' : 'not-allowed' }}>
              {isPlaying ? <Pause fill="currentColor" size={32} /> : <Play fill="currentColor" size={32} />}
            </button>
          </div>
          
          {/* Visual effect of sound waves when the song is playing */}
          {isPlaying && <div className={styles.soundWaves}>
            <span></span><span></span><span></span><span></span>
          </div>}
        </div>

        <div className={styles.activeSongInstruction}>
          <div className={styles.instructionIcon}><Volume2 size={18} /></div>
          <p>The song is playing... Listen and drag to the correct position</p>
        </div>
      </div>
    </div>
  );
};

export default ActiveSongArea;