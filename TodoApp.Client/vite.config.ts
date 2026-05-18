import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/authentication': 'http://localhost:5133',
      '/todo-items': 'http://localhost:5133',
      '/todo-lists': 'http://localhost:5133',
      '/reports': 'http://localhost:5133',
      '/jobs': 'http://localhost:5133',
    },
  },
})
