import { betterAuth } from "better-auth";
import { emailOTP, admin, openAPI, jwt } from "better-auth/plugins"
import { oauthProvider } from "@better-auth/oauth-provider";
import { drizzleAdapter } from "@better-auth/drizzle-adapter";
import { db } from "./database.js";
import * as schema from "./auth-schema.js"
import { sendMail } from "./mail.js"

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
      await sendMail({
        from: "muzonia@muzonia.com",
        to: user.email,
        subject: "Muzonia verification email",
        text: `Verify your account by clicking this link ${url}`,
        html: `<p>Verify your account by clicking this link <a href="${url}">Verify Email</a></p>`
      })
    }
  },
  logger: {
    disabled: false,
    level: "info",
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
      allowDynamicClientRegistration: true,
      validAudiences: process.env.AUTH_AUDIENCE
        ? [process.env.AUTH_AUDIENCE]
        : undefined,
    }),
    jwt({
      jwks: {
        // disablePrivateKeyEncryption: true,
        // keyPairConfig: {
        //   alg: "EdDSA",
        // }
      }
    }),
    openAPI(),
    admin(),
    emailOTP({
      async sendVerificationOTP({ email, otp, type }) {
        await sendMail({
          from: "muzonia@muzonia.com",
          to: email,
          subject: "Muzonia verification email",
          text: `Your verification OTP is: ${otp}`,
          html: `<p>Your verification OTP is: <strong>${otp}</strong></p>`
        });
      }
    })
  ],
});
