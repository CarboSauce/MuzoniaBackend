import { A, useNavigate } from "@solidjs/router";
import { createSignal, onMount, Show } from "solid-js";
import { useAuth } from "~/components/BetterAuthProvider";
import { Badge, Button, Card } from "~/components/admin/ui";

type Session = {
  user?: { id: string; email: string; name?: string; role?: string };
};

function getInitials(name?: string | null, email?: string | null) {
  const source = (name ?? email ?? "").trim();
  if (!source) return "?";
  const parts = source.split(/\s+|@/);
  return (parts[0]?.[0] ?? "?").toUpperCase() + (parts[1]?.[0] ?? "").toUpperCase();
}

export default function Home() {
  const auth = useAuth();
  const navigate = useNavigate();

  const [session, setSession] = createSignal<Session | null>(null);
  const [ready, setReady] = createSignal(false);

  onMount(async () => {
    try {
      const res = await auth.getSession();
      setSession((res as { data?: Session | null })?.data ?? null);
    } catch {
      setSession(null);
    } finally {
      setReady(true);
    }
  });

  const signOut = async () => {
    await auth.signOut();
    window.location.href = "/";
  };

  return (
    <main class="mx-auto max-w-3xl space-y-6 p-6">
      <header>
        <h1 class="text-3xl font-bold">Muzonia</h1>
        <p class="text-sm text-muted-foreground">Authentication & admin</p>
      </header>

      <Show
        when={ready()}
        fallback={<p class="text-sm text-muted-foreground">Loading…</p>}
      >
        <Show
          when={session()?.user}
          fallback={
            <Card title="Get started">
              <div class="space-y-4">
                <p class="text-sm">Sign in to manage your account, or create one to get started.</p>
                <div class="flex gap-2">
                  <Button onClick={() => navigate("/login")}>Sign in</Button>
                  <Button variant="outline" onClick={() => navigate("/register")}>
                    Create account
                  </Button>
                </div>
              </div>
            </Card>
          }
        >
          {(user) => (
            <Card title="Signed in">
              <div class="flex items-center justify-between gap-4">
                <div class="flex items-center gap-3">
                  <div class="flex h-10 w-10 items-center justify-center rounded-full bg-muted text-sm font-medium">
                    {getInitials(user().name, user().email)}
                  </div>
                  <div>
                    <div class="font-medium">{user().name ?? user().email}</div>
                    <div class="text-xs text-muted-foreground">{user().email}</div>
                  </div>
                  <Show when={user().role === "admin"}>
                    <Badge tone="info">admin</Badge>
                  </Show>
                </div>
                <Button variant="outline" onClick={signOut}>
                  Sign out
                </Button>
              </div>

              <Show when={user().role === "admin"}>
                <div class="mt-4 border-t border-border pt-4">
                  <A href="/admin" class="text-sm font-medium text-blue-600 hover:underline">
                    Go to admin dashboard →
                  </A>
                </div>
              </Show>
            </Card>
          )}
        </Show>
      </Show>
    </main>
  );
}