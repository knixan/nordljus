import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

// Backend kör på 5076 i utveckling (src/Nordljus.Backend/Properties/launchSettings.json).
// Vite skickar /hubs vidare dit, så att webbläsaren bara pratar med en origin och ingen CORS behövs.
const backend = "http://localhost:5076";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      "/hubs": { target: backend, ws: true },
    },
  },
  test: {
    environment: "node",
  },
});
