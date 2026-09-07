import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'https://abpguessgame-api.runasp.net',
        changeOrigin: true,
        secure: true,
      },
      '/connect': {
        target: 'https://abpguessgame-api.runasp.net',
        changeOrigin: true,
        secure: true,
      },
      '/Account': {
        target: 'https://abpguessgame-api.runasp.net',
        changeOrigin: true,
        secure: true,
      }
    }
  },
  preview: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'https://abpguessgame-api.runasp.net',
        changeOrigin: true,
        secure: true,
      },
      '/connect': {
        target: 'https://abpguessgame-api.runasp.net',
        changeOrigin: true,
        secure: true,
      },
      '/Account': {
        target: 'https://abpguessgame-api.runasp.net',
        changeOrigin: true,
        secure: true,
      }
    }
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  }
});

