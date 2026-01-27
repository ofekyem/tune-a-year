import React, { useState } from 'react';
import styles from '../Lobby.module.css';
import { Check, Languages } from 'lucide-react';

interface Props {
  onSelect: (languages: string[]) => void;
}

const LanguageSelect: React.FC<Props> = ({ onSelect }) => {
  // State to track selected languages
  const [selected, setSelected] = useState<string[]>([]);

  const options = [
    { id: 'Hebrew', label: 'Hebrew' },
    { id: 'English', label: 'English' }
  ];

  const toggleLanguage = (id: string) => {
    setSelected(prev => 
      prev.includes(id) 
        ? prev.filter(lang => lang !== id) 
        : [...prev, id]
    );
  };

  return (
    <div className={styles.selectionArea}>
      <h2 className={styles.stepTitle}>Select Songs Languages</h2>
      
      <div className={styles.languagesList}>
        {options.map(option => (
          <div 
            key={option.id}
            className={`${styles.langItem} ${selected.includes(option.id) ? styles.activeLang : ''}`}
            onClick={() => toggleLanguage(option.id)}
          >
            <div className={styles.checkbox}>
              {selected.includes(option.id) && <Check size={18} />}
            </div>
            <span>{option.label}</span>
          </div>
        ))}
      </div>

      <button 
        className={styles.primaryBtn}
        onClick={() => onSelect(selected)}
        disabled={selected.length === 0}
      >
        Continue
      </button>

      <p className={styles.infoText}>
        <Languages size={16} /> Choose at least one language for the database
      </p>
    </div>
  );
};

export default LanguageSelect;