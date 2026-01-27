import React, { useEffect, useRef, useState } from 'react';
import styles from '../GameBoard.module.css';
import { Music, Play, Pause, Volume2 } from 'lucide-react';
import type { Song } from '../../../types/song';

interface Props {
  currentSong: Song | null;
}

const ActiveSongArea: React.FC<Props> = ({ currentSong }) => {
  const [isPlaying, setIsPlaying] = useState(false);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  // Handle audio playback when currentSong changes
  useEffect(() => {
    if (currentSong?.previewUrl) {
      // crate new audio object
      audioRef.current = new Audio(currentSong.previewUrl);
      audioRef.current.loop = true;
      
      // playing the audio automatically
      audioRef.current.play()
        .then(() => setIsPlaying(true))
        .catch(() => setIsPlaying(false));
    }

    return () => {
      // Cleanup: stop the audio when the component unmounts or the song changes
      audioRef.current?.pause();
      audioRef.current = null;
    };
  }, [currentSong]);

  const togglePlay = () => {
    if (!audioRef.current) return;
    
    if (isPlaying) {
      audioRef.current.pause();
    } else {
      audioRef.current.play();
    }
    setIsPlaying(!isPlaying);
  };

  if (!currentSong) return null;

  return (
    <div className={styles.activeSongArea}>
      <div className={styles.mysteryCardContainer}>
        {/* The hidden card that will be dragged later */}
        <div className={styles.mysteryCard}>
          <div className={styles.cardPattern}>
            <Music size={60} className={styles.musicIcon} />
          </div>
          
          <div className={styles.audioOverlay}>
            <button className={styles.playControlBtn} onClick={togglePlay}>
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