import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../wwwroot/js/react-beta', // Output to ASP.NET wwwroot
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: 'app.js',       // Fixed name for the JS bundle
        assetFileNames: 'styles.css',   // Fixed name for the CSS bundle
      }
    }
  }
})
