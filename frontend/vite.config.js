import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

const backendUrl = process.env.VITE_PROXY_TARGET || 'http://localhost:5118'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    host: '0.0.0.0',
    proxy: {
      '/api': backendUrl,
      '/r': backendUrl,
      '/logos': backendUrl,
    },
  },
})
