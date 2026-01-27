import React from 'react';
import styles from '../GameBoard.module.css';
import type { TimelineCard as TimelineCardType } from '../../../types/song';

interface Props {
  card: TimelineCardType;
  isNew?: boolean; //animation flag 
}

const TimelineCard: React.FC<Props> = ({ card, isNew }) => {
  return (
    <div className={`${styles.timelineCard} ${isNew ? styles.animatePop : ''}`}>
      <div className={styles.cardYear}>{card.releaseYear}</div>
      <div className={styles.cardDetails}>
        <div className={styles.songTitle} title={card.title}>{card.title}</div>
        <div className={styles.artistName} title={card.artist}>{card.artist}</div>
      </div>
    </div>
  );
};

export default TimelineCard;