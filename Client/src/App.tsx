import { Routes, Route, Navigate } from 'react-router-dom';
import Lobby from './components/Lobby/Lobby';
import GameBoard from './components/GameBoard/GameBoard'; 
import './App.css';

function App() {
  return (
    <div className="App">
      <Routes>
        {/* homePage is lobby*/}
        <Route path="/" element={<Lobby />} />
        
        {/* game page receives session ID in URL */}
        <Route path="/game/:sessionId" element={<GameBoard />} />
        
        {/* Fallback - if user navigates to a non-existent route, redirect to lobby */}
        <Route path="*" element={<Navigate to="/" />} />
      </Routes>
    </div>
  );
}

export default App;