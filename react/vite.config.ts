import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'https://localhost:44300',
        changeOrigin: true,
        secure: false,
      },
      '/connect': {
        target: 'https://localhost:44300',
        changeOrigin: true,
        secure: false,
      },
      '/Account': {
        target: 'https://localhost:44300',
        changeOrigin: true,
        secure: false,
      }
    }
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  }
});

