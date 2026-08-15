import { defineConfig } from "vite";
import { nitro } from "nitro/vite";
import "dotenv/config"
import { solidStart } from "@solidjs/start/config";
import tailwindcss from "@tailwindcss/vite";
import path from "node:path";

export default defineConfig({
  plugins: [
    solidStart(),
    nitro(),
    tailwindcss()
  ],
  resolve: {
    alias: {
      "~": path.resolve(__dirname, "./src")
    }
  },
  environments: {


  },
  server: {
    port: process.env.APP_PORT ? parseInt(process.env.APP_PORT) : undefined,
    strictPort: true
  }
});
