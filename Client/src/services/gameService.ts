import api from './api';
import type { MatchConfiguration, BaseGameSession } from '../types';

export const gameService = {
  // create a new game session
  createGame: async (config: MatchConfiguration): Promise<BaseGameSession> => {
    const response = await api.post<BaseGameSession>('/game/create', config);
    return response.data;
  },

  // start the game after players have joined
  startGame: async (sessionId: string): Promise<BaseGameSession> => {
    const response = await api.post<BaseGameSession>(`/game/${sessionId}/start`);
    return response.data;
  },

  // join an existing game room (for online multiplayer)
  joinGame: async (roomCode: string, playerName: string): Promise<BaseGameSession> => {
    // we send room code and player name as query parameters
    const response = await api.post<BaseGameSession>('/game/join', null, { 
      params: { roomCode, playerName } 
    });
    return response.data;
  },

  // get the current state of the game session by session ID
  getSession: async (sessionId: string): Promise<BaseGameSession> => {
    const response = await api.get<BaseGameSession>(`/game/${sessionId}`);
    return response.data;
  },


  // method after player submits a guess
  submitGuess: async (
      sessionId: string, 
      playerId: string, 
      targetIndex: number, 
      titleGuess?: string, 
      artistGuess?: string
  ): Promise<{ session: BaseGameSession, result: any }> => {
      const response = await api.post<{ session: BaseGameSession, result: any }>(
        `/game/${sessionId}/guess`, 
        null, // no body needed
        { 
          params: { playerId, targetIndex, titleGuess, artistGuess } 
        }
      );
      return response.data;
    },
};