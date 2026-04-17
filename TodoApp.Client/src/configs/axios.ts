import axios from "axios";

// const api = axios.create({
//   baseURL: "https://localhost:7196",
//   timeout: 60000, // Increase to 60 seconds
//   headers: {
//     'Content-Type': 'application/json',
//   }
// });

const api = axios.create({
  // Dùng relative URL để Nginx proxy
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor for debugging
api.interceptors.request.use(
  (config) => {
    console.log('Request:', config.method?.toUpperCase(), config.url);
    return config;
  },
  (error) => {
    console.error('Request error:', error);
    return Promise.reject(error);
  }
);

// Add response interceptor for better error handling
api.interceptors.response.use(
  (response) => {
    console.log('Response:', response.status, response.config.url);
    return response;
  },
  (error) => {
    console.error('Response error details:', {
      url: error.config?.url,
      method: error.config?.method,
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      code: error.code,
      message: error.message
    });
    
    if (error.code === 'ECONNABORTED') {
      console.error('Request timeout');
    } else if (error.response) {
      console.error('Response error:', error.response.status);
    } else if (error.request) {
      console.error('No response received');
    }
    return Promise.reject(error);
  }
);

export default api;