import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

const backendTarget = "http://localhost:8081";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "src"),
    },
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      "/api": {
        target: backendTarget,
        changeOrigin: true,
        secure: false,
      },
      "/chathub": {
        target: backendTarget,
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
});
