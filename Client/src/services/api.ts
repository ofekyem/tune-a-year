import axios from 'axios';

// server base URL
const BASE_URL = 'http://localhost:5000/api'; 

// create an axios instance with default settings
const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: true, // critical for CORS and Cookies that are set on the server
  headers: {
    'Content-Type': 'application/json',
  },
});

export default api;