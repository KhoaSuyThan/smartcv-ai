import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  base: './',
  plugins: [vue(), tailwindcss()],
  build: {
    outDir: '../wwwroot/cvbuilder',
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: 'assets/[name]_v13.js',
        chunkFileNames: 'assets/[name]_[hash]_v13.js',
        assetFileNames: (assetInfo) => {
          if (assetInfo.name === 'index.css') {
            return 'assets/[name]_v13.[ext]';
          }
          return 'assets/[name]_[hash]_v13.[ext]';
        }
      }
    }
  }
})
