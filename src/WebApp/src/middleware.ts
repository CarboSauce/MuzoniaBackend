import { createMiddleware } from "@solidjs/start/middleware";

export default createMiddleware([
  async (event, next) => {
    const start = Date.now();
    const { method } = event.req;
    const url = new URL(event.req.url);

    const auth = event.req.headers.get("authorization");
    const authLog = auth ? ` auth=${auth.slice(0, 12)}…` : "";

    console.log(
        `[${new Date().toISOString()}] -> ${method} ${url.pathname}${url.search}${authLog}`
    );

    await next();

    const duration = Date.now() - start;
    console.log(
        `[${new Date().toISOString()}] <- ${method} ${url.pathname}${url.search} ${duration}ms`
    );
  },
]);
