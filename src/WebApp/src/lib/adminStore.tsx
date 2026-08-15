import { createContext, createMemo, createSignal, JSX, onMount, useContext } from "solid-js";
import { useAuth } from "~/components/BetterAuthProvider";
import type { UserWithRole } from "better-auth/plugins";

export type AdminSession = {
  session: Record<string, any> & {
    id: string;
    userId: string;
    token: string;
    expiresAt: Date | string;
  };
  user: UserWithRole;
};

export type AdminState = "loading" | "signedOut" | "notAdmin" | "admin";

export function createAdminAuth() {
  const client = useAuth();
  const [session, setSession] = createSignal<AdminSession | null>(null);
  const [ready, setReady] = createSignal(false);

  onMount(async () => {
    try {
      const res = await client.getSession();
      // const data = (res as { data?: AdminSession | null })?.data ?? null;
      const session = res.data?.session;
      const user = res.data?.user;
      const role = user?.role ?? undefined;
      const banned = user?.banned ?? null;

      setSession(user && session ? { user: { ...user, role, banned }, session } : null);
    } catch {
      setSession(null);
    } finally {
      setReady(true);
    }
  });

  const state = createMemo<AdminState>(() => {
    if (!ready()) return "loading";
    const user = session()?.user;
    if (!user) return "signedOut";
    return user.role === "admin" ? "admin" : "notAdmin";
  });

  return {
    client,
    session,
    state,
    user: () => session()?.user ?? null,
    isAdmin: () => state() === "admin",
  };
}

export type AdminAuth = ReturnType<typeof createAdminAuth>;

const AdminContext = createContext<AdminAuth>();

export function AdminProvider(props: { children: JSX.Element }) {
  return <AdminContext.Provider value={createAdminAuth()}>{props.children}</AdminContext.Provider>;
}

export function useAdmin(): AdminAuth {
  const ctx = useContext(AdminContext);
  if (ctx == undefined) throw new Error("Admin context is undefined");
  return ctx;
}
