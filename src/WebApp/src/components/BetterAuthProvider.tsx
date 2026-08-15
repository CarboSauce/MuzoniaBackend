import { createAuthClient } from "better-auth/client";
import { adminClient, emailOTPClient } from "better-auth/client/plugins";
import { oauthProviderClient } from "@better-auth/oauth-provider/client";
import { createContext, ParentComponent, useContext } from "solid-js";

const Context = createContext<ReturnType<typeof createClient>>();

function createClient() {
  const baseUrl = import.meta.env.VITE_URL;
  return createAuthClient({
    baseURL: baseUrl,
    plugins: [adminClient(), emailOTPClient(), oauthProviderClient()],
  });
}

export function useAuth() {
  const ctx = useContext(Context);
  if (ctx == undefined) {
    throw new Error("Auth context is undefined");
  }
  return ctx;
}

const AuthProvider: ParentComponent = (props) => {
  const authClient = createClient();
  return <Context.Provider value={authClient}>{props.children}</Context.Provider>;
};

export default AuthProvider;
