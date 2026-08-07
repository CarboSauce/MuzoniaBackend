import { defineConfig } from "vite";
import { nitro } from "nitro/vite";
import "dotenv/config"

import { solidStart } from "@solidjs/start/config";

export default defineConfig({
  plugins: [solidStart(),
    nitro()
  ],
  server: {
    port: process.env.APP_PORT ? parseInt(process.env.APP_PORT) : undefined,
    strictPort: true
  }
});
