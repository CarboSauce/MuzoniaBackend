import { createContext, ParentComponent, useContext } from "solid-js";
import AuthProvider from "./BetterAuthProvider";
import createUserAuth from "~/lib/authStore";

const UserContext = createContext<ReturnType<typeof createUserAuth>>();

export function useUser() {
  const ctx = useContext(UserContext);
  if (ctx == undefined) throw new Error("User context is empty");
  return ctx;
}

const UserProvider: ParentComponent = (props) => {
  return <UserContext.Provider value={createUserAuth()}>{props.children}</UserContext.Provider>;
};

export const AppProvider: ParentComponent = (props) => {
  return (
    <AuthProvider>
      <UserProvider>{props.children}</UserProvider>
    </AuthProvider>
  );
};
