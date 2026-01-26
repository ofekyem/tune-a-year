import type { Player } from './player';
import type { Song } from './song'; 

// GameMode
export const GameMode = {
  SingleDevice: 0,
  Online: 1,
} as const;
export type GameMode = typeof GameMode[keyof typeof GameMode];

// MusicSource
export const MusicSource = {
  LocalDatabase: 0,
  SpotifyPlaylist: 1,
  YouTubePlaylist: 2,
} as const;
export type MusicSource = typeof MusicSource[keyof typeof MusicSource];

// LanguageOption
export const LanguageOption = {
  Hebrew: 0,
  English: 1,
  Both: 2,
} as const;
export type LanguageOption = typeof LanguageOption[keyof typeof LanguageOption];

// SessionStatus
export const SessionStatus = {
  Lobby: 0,
  InProgress: 1,
  Finished: 2,
} as const;
export type SessionStatus = typeof SessionStatus[keyof typeof SessionStatus]; 

// GuessResult Type
export interface GuessResult {
  placementCorrect: boolean;
  bonusEarned: boolean;
  correctTitle: string;
  correctArtist: string;
  correctYear: number;
  playerId: string; // Guid
}

// MatchConfiguration Type
export interface MatchConfiguration {
  mode: GameMode;
  source: MusicSource;
  playlistUrl: string | null;
  languages: string[]; // List<string> -> string[]
  maxPlayers: number;
  winningScore: number;
  creatorName: string | null; // only for online games
  localPlayerNames: string[] | null; // optional for local games
} 

// BaseGameSession Type
export interface BaseGameSession {
  id: string;
  roomCode: string;
  status: SessionStatus;
  config: MatchConfiguration;
  players: Player[];
  currentPlayerIndex: number;
  playedSongIds: string[];
  currentActiveSong: Song | null;
  createdAt: string; // in C# DateTime ISO format
} 

// ExternalPlaylistSession Type (extending BaseGameSession)
export interface ExternalPlaylistSession extends BaseGameSession {
  sessionPlaylist: Song[];
  externalPlaylistId: string;
} 

// LocalDatabaseSession Type (extending BaseGameSession)
export interface LocalDatabaseSession extends BaseGameSession {
} 

export type GameSession = ExternalPlaylistSession | LocalDatabaseSession;