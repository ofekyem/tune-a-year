# 🎵 Tune-A-Year

**Tune-A-Year** is a full-stack musical timeline game inspired by the board game Hitster. Players compete to build a chronological timeline of songs by accurately placing them based on their release years.

## 🎮 Game Modes
* **Local Version:** Support for single-device play for players sharing a screen.
* **Online Multiplayer:** Real-time remote play via Room Codes, powered by **SignalR** for instant synchronization between players.

## 🛠️ Tech Stack & Implementation

### **Backend (.NET 10 & C#)**
* **Architecture:** Developed using **Object-Oriented Programming (OOP)** principles and design patterns to manage game sessions and logic.
* **Database:** **PostgreSQL** managed via **Entity Framework Core** using a "Code-First" approach.
* **Real-Time:** **SignalR** for persistent WebSocket connections to handle live updates and multiplayer interactions.

### **Frontend (React & TypeScript)**
* **Audio Engine:** Leveraging the **HTML5 Audio API** for playback control and game event integration.
* **Drag & Drop:** Developed using the **Native HTML5 Drag & Drop API** for an intuitive card-placement experience.
* **UI/UX:** A responsive, component-based architecture designed for interactive gameplay.

## 🔧 Setup

### Backend
1. Update `ConnectionStrings` in `appsettings.json`.
2. Run `dotnet ef database update`.
3. Run `dotnet run`.

### Frontend
1. Run `npm install`.
2. Run `npm start`.
