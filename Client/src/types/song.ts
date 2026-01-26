// Song Type
export interface Song {
  id: string; // Guid -> string
  title: string;
  artist: string; 
  releaseYear: number; 
  spotifyId: string;
  previewUrl: string; 
  language: string;
}

// TimelineCard Type
export interface TimelineCard {
  id: string; // Guid -> string
  songId: string; // Guid -> string
  title: string;
  artist: string;
  releaseYear: number;
}