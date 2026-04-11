import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  base: '/cvbuilder/',
  plugins: [vue(), tailwindcss()],
  build: {
    outDir: '../wwwroot/cvbuilder',
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: 'assets/[name]_v10.js',
        chunkFileNames: 'assets/[name]_v10.js',
        assetFileNames: 'assets/[name]_v10.[ext]'
      }
    }
  }
})
