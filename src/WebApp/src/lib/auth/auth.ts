import { betterAuth } from "better-auth";
import { emailOTP, admin, openAPI, jwt } from "better-auth/plugins"
import { oauthProvider } from "@better-auth/oauth-provider";
import { drizzleAdapter } from "@better-auth/drizzle-adapter";
import { db } from "./database.js";
import * as schema from "./auth-schema.js"

export const auth = betterAuth({
  database: drizzleAdapter(db, {
    provider: "pg",
    schema: schema,
    usePlural: true
  }),
  emailAndPassword: {
    enabled: true,
    autoSignIn: false
  },
  advanced: {
    database: {
      generateId: "uuid"//() => randomUUIDv7(),
    }
  },
  emailVerification: {
    sendVerificationEmail: async ({ user, url, token }, request) => {
      console.log({
        to: user.email,
        subject: 'Verify your email address',
        text: `Click the link to verify your email: ${url}`
      })
    }
  },
  logger: {
    disabled: false,
    level: "debug",
    log: (level, message, ...args) => {
      // Custom logging implementation
      console.log(`[${level}] ${message}`, ...args);
    }
  },
  disabledPaths: [
    "/token"
  ],
  plugins: [
    oauthProvider({
      loginPage: "/sign-in",
      consentPage: "/consent",
    }),
    jwt({
      disableSettingJwtHeader: true,
    }),
    openAPI(),
    admin(),
    emailOTP({
      async sendVerificationOTP({ email, otp, type }) {
        console.log(`Sending ${type} OTP to ${email}: ${otp}`);
      }
    })
  ],
});
