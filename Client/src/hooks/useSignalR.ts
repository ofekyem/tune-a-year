import { useEffect, useState} from 'react';
import * as signalR from '@microsoft/signalr';

export const useSignalR = (roomCode: string | null) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    if (!roomCode) return;

    // create SignalR connection
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5149/gameHub")
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    setConnection(newConnection);
  }, [roomCode]);


  useEffect(() => {
    if (connection) {
      // if someone connects, start the connection
      connection.start()
        .then(() => {
          setIsConnected(true);
          console.log("Connected to SignalR Hub!");
          
          // join the room group
          connection.invoke("JoinRoom", roomCode);
        })
        .catch(err => console.error("SignalR Connection Error: ", err));

      // stop the connection when component unmounts
      return () => {
        connection.stop();
      };
    }
  }, [connection, roomCode]);

  return { connection, isConnected };
};