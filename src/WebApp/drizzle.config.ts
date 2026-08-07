// drizzle.config.ts
import 'dotenv/config'
import { defineConfig } from "drizzle-kit";
import { createDbUrl } from "./src/lib/auth/database.js"

const dbUrl = createDbUrl()

export default defineConfig({
  dialect: "postgresql",
  schema: "./src/lib/auth/auth-schema.ts",
  dbCredentials: {
    url: dbUrl
  },
});
