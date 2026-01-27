import type { TimelineCard } from './song'; 

// Standard Player Type
export interface Player {
  id: string; // Guid -> string
  name: string;
  tokens: number;
  timeline: TimelineCard[]; // List -> Array
  hasWon: boolean; 
  baseGameSessionId: string;
  joinOrder: number;
} 

// Online Player Type (extending Standard Player)
export interface OnlinePlayer extends Player {
  connectionId: string | null; // string? -> string | null
  isHost: boolean;
  isConnected: boolean;
}