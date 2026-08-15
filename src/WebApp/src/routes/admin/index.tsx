import { A } from "@solidjs/router";
import { For, Show } from "solid-js";
import { useAdmin } from "~/lib/adminStore";
import { Badge } from "~/components/admin/ui";

export default function AdminDashboard() {
  const admin = useAdmin();
  const user = admin.user();

  const links = [
    { href: "/admin/users", title: "Users", description: "Search, ban, change roles and revoke sessions." },
    { href: "/admin/clients", title: "OAuth Clients", description: "Register and manage OAuth2 client applications." },
  ];

  return (
    <div class="mx-auto max-w-3xl space-y-6">
      <div>
        <h1 class="text-2xl font-bold">Dashboard</h1>
        <p class="text-sm text-muted-foreground">
          Signed in as {user?.name} · {user?.email}
        </p>
      </div>

      <div class="flex items-center gap-2">
        <span class="text-sm text-muted-foreground">Role</span>
        <Badge tone="info">{user?.role}</Badge>
        <Show when={user?.banned}>
          <Badge tone="danger">Banned</Badge>
        </Show>
      </div>

      <div class="grid gap-4 sm:grid-cols-2">
        <For each={links}>
          {(link) => (
            <A
              href={link.href}
              class="rounded-lg border border-border p-4 transition-colors hover:bg-accent"
            >
              <h2 class="font-semibold">{link.title}</h2>
              <p class="mt-1 text-sm text-muted-foreground">{link.description}</p>
            </A>
          )}
        </For>
      </div>
    </div>
  );
}
