import { createResource, createSignal, For, Show } from "solid-js";
import type { Accessor } from "solid-js";
import * as z from "zod/mini";
import { useAppForm } from "~/components/FormField";
import { useAdmin } from "~/lib/adminStore";
import type { UserWithRole } from "better-auth/plugins";
import {
  Badge,
  Button,
  Card,
  ConfirmButton,
  Field,
  Input,
  Modal,
  Select,
} from "~/components/admin/ui";

type ListUsersQuery = {
  limit: number;
  offset: number;
  searchValue: string;
  searchField: "email" | "name";
  searchOperator: "contains" | "starts_with" | "ends_with";
};

type UserSessionsData = {
  sessions: (Record<string, any> & { token: string; expiresAt: Date | string })[];
};

const ROLE_OPTIONS = [
  { value: "user", label: "User" },
  { value: "admin", label: "Admin" },
];

const createUserSchema = z.object({
  name: z.string().check(z.minLength(1, { error: "Name is required" })),
  email: z.email({ error: "A valid email is required" }),
  password: z.string(),
  role: z.string(),
});

function formatDate(value?: Date | string | null) {
  if (!value) return "-";
  return new Date(value).toLocaleString();
}

function formatError(err: any) {
  if (!err) return "Unknown error";
  if (typeof err === "string") return err;
  return err.message ?? JSON.stringify(err);
}

export default function AdminUsers() {
  const admin = useAdmin();

  const [query, setQuery] = createSignal<ListUsersQuery>({
    limit: 50,
    offset: 0,
    searchValue: "",
    searchField: "email",
    searchOperator: "contains",
  });
  const [searchInput, setSearchInput] = createSignal("");
  const [searchField, setSearchField] = createSignal<"email" | "name">("email");

  const [users, { refetch }] = createResource(query, async (q) => {
    const res = await admin.client.admin.listUsers({ query: q });
    if (res.error) throw new Error(formatError(res.error));
    return res.data;
  });

  const [busy, setBusy] = createSignal(false);
  const [message, setMessage] = createSignal<{ tone: "success" | "error"; text: string } | null>(
    null,
  );

  const [banTarget, setBanTarget] = createSignal<UserWithRole | null>(null);
  const [sessionTarget, setSessionTarget] = createSignal<UserWithRole | null>(null);
  const [createOpen, setCreateOpen] = createSignal(false);

  const showMessage = (tone: "success" | "error", text: string) => setMessage({ tone, text });

  const runAction = async (fn: () => Promise<any>, successText: string) => {
    setBusy(true);
    try {
      const res = await fn();
      if (res.error) {
        showMessage("error", formatError(res.error));
        return;
      }
      showMessage("success", successText);
      refetch();
    } catch (e: any) {
      showMessage("error", formatError(e));
    } finally {
      setBusy(false);
    }
  };

  const doSearch = () => {
    setQuery((prev) => ({
      ...prev,
      searchValue: searchInput().trim(),
      searchField: searchField(),
      offset: 0,
    }));
  };

  const changeRole = (user: UserWithRole, role: string) =>
    runAction(
      () => admin.client.admin.setRole({ userId: user.id, role: role as "admin" | "user" }),
      `Role set to ${role}`,
    );

  const unban = (user: UserWithRole) =>
    runAction(() => admin.client.admin.unbanUser({ userId: user.id }), "User unbanned");

  const removeUser = (user: UserWithRole) =>
    runAction(() => admin.client.admin.removeUser({ userId: user.id }), "User deleted");

  const impersonate = async (user: UserWithRole) => {
    setBusy(true);
    try {
      const res = await admin.client.admin.impersonateUser({ userId: user.id });
      if (res.error) {
        showMessage("error", formatError(res.error));
      } else {
        window.location.href = "/";
      }
    } catch (e: any) {
      showMessage("error", formatError(e));
    } finally {
      setBusy(false);
    }
  };

  const ban = (user: UserWithRole, reason: string, expiresIn: number) =>
    runAction(
      () =>
        admin.client.admin.banUser({
          userId: user.id,
          banReason: reason || undefined,
          banExpiresIn: expiresIn || undefined,
        }),
      "User banned",
    );

  const createUser = (payload: { name: string; email: string; password: string; role: string }) =>
    runAction(
      () =>
        admin.client.admin.createUser({
          name: payload.name,
          email: payload.email,
          password: payload.password,
          role: payload.role as "admin" | "user",
        }),
      "User created",
    );

  return (
    <div class="space-y-6">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold">Users</h1>
          <p class="text-sm text-muted-foreground">Manage accounts, roles and sessions.</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>Create user</Button>
      </div>

      <Show when={message()}>
        <div
          class={
            "rounded border px-3 py-2 text-sm " +
            (message()!.tone === "success"
              ? "border-success bg-success text-success-foreground"
              : "border-error bg-error text-error-foreground")
          }
        >
          {message()!.text}
        </div>
      </Show>

      <Card
        title="Users"
        actions={
          <div class="flex flex-wrap items-center gap-2">
            <Select
              value={searchField()}
              options={[
                { value: "email", label: "Email" },
                { value: "name", label: "Name" },
              ]}
              class="w-28"
              onChange={(v) => setSearchField(v as "email" | "name")}
            />
            <Input
              placeholder={`Search by ${searchField()}…`}
              class="w-64"
              value={searchInput()}
              onInput={(e) => setSearchInput(e.currentTarget.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") doSearch();
              }}
            />
            <Button onClick={doSearch}>Search</Button>
          </div>
        }
      >
        <Show when={users.loading}>
          <p class="text-sm text-muted-foreground">Loading users…</p>
        </Show>
        <Show when={users.error}>
          <p class="text-sm text-red-500">Failed to load users: {formatError(users.error)}</p>
        </Show>
        <Show when={!users.loading && !users.error && users()?.users.length === 0}>
          <p class="text-sm text-muted-foreground">No users found.</p>
        </Show>

        <Show when={users()?.users.length}>
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <th class="px-3 py-2">User</th>
                  <th class="px-3 py-2">Role</th>
                  <th class="px-3 py-2">Status</th>
                  <th class="px-3 py-2">Created</th>
                  <th class="px-3 py-2 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                <For each={users()?.users ?? []}>
                  {(user) => (
                    <tr class="border-b border-border last:border-0">
                      <td class="px-3 py-2">
                        <div class="font-medium">{user.name}</div>
                        <div class="text-xs text-muted-foreground">{user.email}</div>
                      </td>
                      <td class="px-3 py-2">
                        <Select
                          value={user.role ?? "user"}
                          options={ROLE_OPTIONS}
                          class="w-28"
                          disabled={busy()}
                          onChange={(v) => changeRole(user, v)}
                        />
                      </td>
                      <td class="px-3 py-2">
                        <Show when={user.banned} fallback={<Badge tone="success">Active</Badge>}>
                          <Badge tone="danger">Banned</Badge>
                        </Show>
                      </td>
                      <td class="px-3 py-2 text-xs text-muted-foreground">
                        {formatDate(user.createdAt)}
                      </td>
                      <td class="px-3 py-2">
                        <div class="flex flex-wrap items-center justify-end gap-1.5">
                          <Show
                            when={!user.banned}
                            fallback={
                              <Button variant="outline" size="sm" onClick={() => unban(user)}>
                                Unban
                              </Button>
                            }
                          >
                            <Button variant="outline" size="sm" onClick={() => setBanTarget(user)}>
                              Ban
                            </Button>
                          </Show>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setSessionTarget(user)}
                          >
                            Sessions
                          </Button>
                          <Button variant="outline" size="sm" onClick={() => impersonate(user)}>
                            Impersonate
                          </Button>
                          <ConfirmButton
                            label="Delete"
                            variant="danger"
                            onConfirm={() => removeUser(user)}
                          />
                        </div>
                      </td>
                    </tr>
                  )}
                </For>
              </tbody>
            </table>
          </div>
          <div class="mt-4 flex items-center justify-between text-sm text-muted-foreground">
            <span>
              {users()?.total ?? 0} user{users()?.total === 1 ? "" : "s"}
            </span>
            <div class="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={query().offset === 0}
                onClick={() =>
                  setQuery((prev) => ({ ...prev, offset: Math.max(0, prev.offset - prev.limit) }))
                }
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={(users()?.total ?? 0) <= query().offset + query().limit}
                onClick={() => setQuery((prev) => ({ ...prev, offset: prev.offset + prev.limit }))}
              >
                Next
              </Button>
            </div>
          </div>
        </Show>
      </Card>

      <BanModal
        user={banTarget()}
        busy={busy()}
        onClose={() => setBanTarget(null)}
        onBan={(reason, expiresIn) => {
          const target = banTarget();
          if (target) {
            ban(target, reason, expiresIn);
            setBanTarget(null);
          }
        }}
      />
      <SessionsModal
        user={sessionTarget()}
        busy={busy()}
        onClose={() => setSessionTarget(null)}
        onRevokeAll={(userId) => {
          runAction(
            () => admin.client.admin.revokeUserSessions({ userId }),
            "All sessions revoked",
          );
        }}
      />
      <CreateUserModal
        open={createOpen()}
        busy={busy()}
        onClose={() => setCreateOpen(false)}
        onCreate={(payload) => {
          createUser(payload);
          setCreateOpen(false);
        }}
      />
    </div>
  );
}

function BanModal(props: {
  user: UserWithRole | null;
  busy: boolean;
  onClose: () => void;
  onBan: (reason: string, expiresIn: number) => void;
}) {
  const [reason, setReason] = createSignal("");
  const [expiresIn, setExpiresIn] = createSignal("");

  return (
    <Modal
      open={props.user != null}
      title={`Ban ${props.user?.name ?? ""}`}
      onClose={props.onClose}
      footer={
        <>
          <Button variant="outline" onClick={props.onClose} disabled={props.busy}>
            Cancel
          </Button>
          <Button
            variant="danger"
            disabled={props.busy || props.user == null}
            onClick={() => props.onBan(reason(), parseInt(expiresIn() || "0", 10))}
          >
            Ban user
          </Button>
        </>
      }
    >
      <Field label="Reason" hint="Shown to the user. Leave empty for no reason.">
        <Input
          value={reason()}
          onInput={(e) => setReason(e.currentTarget.value)}
          placeholder="e.g. Spam"
        />
      </Field>
      <Field label="Ban duration (seconds)" hint="Leave empty for a permanent ban.">
        <Input
          type="number"
          min={1}
          value={expiresIn()}
          onInput={(e) => setExpiresIn(e.currentTarget.value)}
          placeholder="e.g. 86400"
        />
      </Field>
    </Modal>
  );
}

function SessionsModal(props: {
  user: UserWithRole | null;
  busy: boolean;
  onClose: () => void;
  onRevokeAll: (userId: string) => void;
}) {
  const admin = useAdmin();

  const [sessions, { refetch }] = createResource(
    () => (props.user ? props.user.id : undefined),
    async (userId) => {
      const res = await admin.client.admin.listUserSessions({ userId });
      if (res.error) throw new Error(formatError(res.error));
      return res.data as UserSessionsData;
    },
  );

  const runAction = async (fn: () => Promise<any>) => {
    try {
      const res = await fn();
      if (res.error) {
        window.alert(formatError(res.error));
        return;
      }
      refetch();
    } catch (e: any) {
      window.alert(formatError(e));
    }
  };

  const revoke = (token: string) =>
    runAction(() => admin.client.admin.revokeUserSession({ sessionToken: token }));

  return (
    <Modal
      open={props.user != null}
      title={`Sessions · ${props.user?.name ?? ""}`}
      onClose={props.onClose}
      footer={
        <>
          <Button variant="outline" onClick={props.onClose} disabled={props.busy}>
            Close
          </Button>
          <Button
            variant="danger"
            disabled={props.busy || !sessions()?.sessions?.length}
            onClick={() => props.user && props.onRevokeAll(props.user.id)}
          >
            Revoke all
          </Button>
        </>
      }
    >
      <Show when={sessions.loading}>
        <p class="text-sm text-muted-foreground">Loading sessions…</p>
      </Show>
      <Show when={sessions.error}>
        <p class="text-sm text-red-500">Failed to load sessions: {formatError(sessions.error)}</p>
      </Show>
      <Show when={!sessions.loading && !sessions.error && sessions()?.sessions.length === 0}>
        <p class="text-sm text-muted-foreground">No active sessions.</p>
      </Show>
      <Show when={sessions()?.sessions.length}>
        <div class="space-y-2">
          <For each={sessions()?.sessions ?? []}>
            {(session) => (
              <div class="flex items-center justify-between gap-3 rounded border border-border px-3 py-2">
                <div class="min-w-0">
                  <div class="truncate font-mono text-xs">{session.token}</div>
                  <div class="text-xs text-muted-foreground">
                    {session.ipAddress ?? "unknown ip"} ·{" "}
                    {session.userAgent ?? "unknown user agent"}
                    <Show when={session.impersonatedBy}> · impersonated</Show>
                  </div>
                  <div class="text-xs text-muted-foreground">
                    Expires {formatDate(session.expiresAt)}
                  </div>
                </div>
                <ConfirmButton
                  label="Revoke"
                  variant="danger"
                  onConfirm={() => revoke(session.token)}
                />
              </div>
            )}
          </For>
        </div>
      </Show>
    </Modal>
  );
}

function CreateUserModal(props: {
  open: boolean;
  busy: boolean;
  onClose: () => void;
  onCreate: (payload: { name: string; email: string; password: string; role: string }) => void;
}) {
  const form = useAppForm(() => ({
    defaultValues: {
      name: "",
      email: "",
      password: "",
      role: "user",
    },
    validators: {
      onChange: createUserSchema,
    },
    onSubmit: ({ value }) => {
      props.onCreate({
        name: value.name,
        email: value.email,
        password: value.password,
        role: value.role,
      });
      form.reset();
    },
  }));

  return (
    <Modal
      open={props.open}
      title="Create user"
      onClose={props.onClose}
      footer={
        <>
          <Button variant="outline" onClick={props.onClose} disabled={props.busy}>
            Cancel
          </Button>
          <Button variant="default" disabled={props.busy} onClick={() => form.handleSubmit()}>
            Create
          </Button>
        </>
      }
    >
      <form
        onSubmit={(e) => {
          e.preventDefault();
          e.stopPropagation();
          form.handleSubmit();
        }}
        class="space-y-4"
      >
        <form.AppField
          name="name"
          children={(field) => (
            <Field label="Name">
              <Input
                type="text"
                value={field().state.value}
                onInput={(e) => field().handleChange(e.target.value)}
              />
            </Field>
          )}
        />
        <form.AppField
          name="email"
          children={(field) => (
            <Field label="Email">
              <Input
                type="text"
                value={field().state.value}
                onInput={(e) => field().handleChange(e.target.value)}
              />
            </Field>
          )}
        />
        <form.AppField
          name="password"
          children={(field) => (
            <Field label="Password" hint="Leave empty to let the user set it later.">
              <Input
                type="password"
                value={field().state.value}
                onInput={(e) => field().handleChange(e.target.value)}
              />
            </Field>
          )}
        />
        <form.Field
          name="role"
          children={(field) => (
            <Field label="Role">
              <Select
                options={ROLE_OPTIONS}
                value={field().state.value}
                onChange={(v) => field().handleChange(v)}
              />
            </Field>
          )}
        />
      </form>
    </Modal>
  );
}
