import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Forwards to the API directly (bypassing Docker) during `npm run dev`.
      // Override with VITE_API_PROXY_TARGET if the API is running somewhere
      // other than the default LocalDB dev port.
      '/api': {
        target: process.env.VITE_API_PROXY_TARGET ?? 'https://localhost:5443',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
