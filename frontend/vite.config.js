import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    host: '0.0.0.0',
  },
  build: {
    outDir: 'dist',
  },
  define: {
    'process.env': {}
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/setupTests.js',
    css: false,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      include: [
        'src/components/ReviewCardV2/**/*.{jsx,js}',
        'src/components/DiscussionFeedCard/**/*.{jsx,js}',
        'src/components/ActivityTimelineItem/**/*.{jsx,js}',
        'src/components/HeroBanner/**/*.{jsx,js}',
        'src/components/CategoryPills/**/*.{jsx,js}',
        'src/components/SocialSidebar/**/*.{jsx,js}',
        'src/components/JogoCard/**/*.{jsx,js}',
        'src/components/AvaliacaoCard/**/*.{jsx,js}',
        'src/components/ClassificacaoBadge/**/*.{jsx,js}'
      ],
      thresholds: {
        lines: 85,
        statements: 85,
        branches: 50,
        functions: 50
      }
    }
  }
});
