import { auth } from "~/lib/auth/auth";
import { toSolidStartHandler } from "better-auth/solid-start";
import { createMiddleware } from "@solidjs/start/middleware"

export const { GET, POST } = toSolidStartHandler(auth);
