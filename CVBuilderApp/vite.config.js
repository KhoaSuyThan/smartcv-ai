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
        entryFileNames: 'assets/[name]_v14.js',
        chunkFileNames: 'assets/[name]_[hash]_v14.js',
        assetFileNames: (assetInfo) => {
          if (assetInfo.name === 'index.css') {
            return 'assets/[name]_v14.[ext]';
          }
          return 'assets/[name]_[hash]_v14.[ext]';
        }
      }
    }
  }
})
