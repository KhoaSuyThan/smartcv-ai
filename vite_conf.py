import os

vite_config = '''import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../wwwroot/js/react-beta', // Xuất file thẳng vào wwwroot của ASP.NET
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: 'beta-builder.js',
        assetFileNames: 'beta-builder.[ext]'
      }
    }
  }
})
'''

with open(r'c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\ReactCVBuilder\vite.config.js', 'w', encoding='utf-8') as f:
    f.write(vite_config)

print("Configured vite.config.js")
