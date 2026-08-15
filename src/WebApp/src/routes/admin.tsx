import { A, Navigate } from "@solidjs/router";
import { For, Show } from "solid-js";
import { AdminProvider, useAdmin } from "~/lib/adminStore";
import { Button } from "~/components/admin/ui";

function AdminShell(props: { children?: any }) {
  const admin = useAdmin();
  const navItems = [
    { href: "/admin", label: "Dashboard", end: true },
    { href: "/admin/users", label: "Users" },
    { href: "/admin/clients", label: "OAuth Clients" },
  ];

  return (
    <Show
      when={admin.state() !== "loading"}
      fallback={<div class="p-6 text-sm text-muted-foreground">Loading session…</div>}
    >
      <Show when={admin.isAdmin()} fallback={<Navigate href="/login" />}>
        <div class="flex min-h-screen">
          <aside class="flex w-56 shrink-0 flex-col border-r border-border bg-muted/40">
            <div class="border-b border-border p-4">
              <h1 class="text-lg font-bold">Muzonia Admin</h1>
              <p class="truncate text-xs text-muted-foreground">{admin.user()?.email}</p>
            </div>
            <nav class="flex-1 space-y-1 p-3">
              <For each={navItems}>
                {(item) => (
                  <A
                    href={item.href}
                    end={item.end}
                    class="block rounded px-3 py-2 text-sm hover:bg-accent"
                    activeClass="bg-accent font-medium"
                  >
                    {item.label}
                  </A>
                )}
              </For>
            </nav>
            <div class="border-t border-border p-3">
              <Button
                variant="outline"
                size="sm"
                class="w-full"
                onClick={() => {
                  admin.client.signOut();
                  window.location.href = "/";
                }}
              >
                Sign out
              </Button>
            </div>
          </aside>
          <main class="flex-1 overflow-x-auto p-6">{props.children}</main>
        </div>
      </Show>
    </Show>
  );
}

export default function AdminLayout(props: { children?: any }) {
  return (
    <AdminProvider>
      <AdminShell>{props.children}</AdminShell>
    </AdminProvider>
  );
}
