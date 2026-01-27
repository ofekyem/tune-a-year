import axios from 'axios';

// server base URL
const BASE_URL = 'http://localhost:5149/api'; 

// create an axios instance with default settings
const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: true, // critical for CORS and Cookies that are set on the server
  headers: {
    'Content-Type': 'application/json',
  },
}); 

api.interceptors.response.use(
  (response) => {
    // if the response is successful, just return it
    return response;
  },
  (error) => {
    // here we handle errors coming from the server (4xx, 5xx)
    if (error.response) {
      const { status, data } = error.response;

      switch (status) {
        case 400:
          console.error('Bad Request:', data.message || 'Validation error');
          break;
        case 404:
          console.error('Not Found:', 'The requested resource was not found');
          // You can add automatic navigation to the lobby here if the game is not found
          break;
        case 500:
          console.error('Server Error:', 'Something went wrong on the server');
          break;
        default:
          console.error('API Error:', data.message || 'An unknown error occurred');
      }
    } else if (error.request) {
      // The error occurred before receiving a response from the server (network issue)
      console.error('Network Error:', 'Could not connect to the server');
    }

    return Promise.reject(error);
  }
);

export default api;