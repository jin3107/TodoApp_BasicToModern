import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    chunkSizeWarningLimit: 1500,
    rollupOptions: {
      output: {
        manualChunks: {
          react: ['react', 'react-dom', 'react-router-dom'],
          antd: ['antd', '@ant-design/icons'],
          charts: ['@ant-design/charts'],
          axios: ['axios'],
        },
      },
    },
  },
  server: {
    open: true,
    proxy: {
      '/authentication': 'http://localhost:5133',
      '/todo-items': 'http://localhost:5133',
      '/todo-lists': 'http://localhost:5133',
      '/reports': 'http://localhost:5133',
      '/jobs': 'http://localhost:5133',
    },
  },
})