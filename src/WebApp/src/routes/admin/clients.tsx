import { createResource, createSignal, For, Show } from "solid-js";
import { useAdmin } from "~/lib/adminStore";
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

type OAuthClient = {
  client_id: string;
  client_secret?: string;
  client_secret_expires_at?: number | string;
  client_id_issued_at?: number;
  client_name?: string | null;
  logo_uri?: string | null;
  uri?: string | null;
  redirect_uris: string[];
  post_logout_redirect_uris?: string[];
  token_endpoint_auth_method?: string | null;
  grant_types?: string[];
  response_types?: string[];
  type?: string | null;
  disabled?: boolean | null;
  user_id?: string | null;
  created_at?: number | string | Date | null;
  updated_at?: number | string | Date | null;
};

type CreateClientResponse = {
  client_id: string;
  client_secret: string;
  client_id_issued_at?: number;
  client_secret_expires_at?: number | string;
};

const TOKEN_AUTH_OPTIONS = [
  { value: "client_secret_basic", label: "Confidential (client_secret_basic)" },
  { value: "client_secret_post", label: "Confidential (client_secret_post)" },
  { value: "none", label: "Public (PKCE, no secret)" },
];

function formatDate(value?: number | string | Date | null) {
  if (value == null) return "—";
  const date = typeof value === "number" ? new Date(value * 1000) : new Date(value);
  if (isNaN(date.getTime())) return "—";
  return date.toLocaleString();
}

function formatError(err: any) {
  if (!err) return "Unknown error";
  if (typeof err === "string") return err;
  return err.message ?? JSON.stringify(err);
}

export default function AdminClients() {
  const admin = useAdmin();

  const [clients, { refetch }] = createResource(async () => {
    const res = await admin.client.oauth2.getClients();
    if (res.error) throw new Error(formatError(res.error));
    const data = (res as { data?: OAuthClient[] | null })?.data;
    return Array.isArray(data) ? data : [];
  });

  const [busy, setBusy] = createSignal(false);
  const [message, setMessage] = createSignal<{ tone: "success" | "error"; text: string } | null>(
    null,
  );
  const [createOpen, setCreateOpen] = createSignal(false);
  const [createdSecret, setCreatedSecret] = createSignal<CreateClientResponse | null>(null);

  const showMessage = (tone: "success" | "error", text: string) => setMessage({ tone, text });

  const runAction = async (fn: () => Promise<any>, successText: string) => {
    setBusy(true);
    try {
      const res = await fn();
      if (res?.error) {
        showMessage("error", formatError(res.error));
        return res;
      }
      showMessage("success", successText);
      refetch();
      return res;
    } catch (e: any) {
      showMessage("error", formatError(e));
      return null;
    } finally {
      setBusy(false);
    }
  };

  const removeClient = (client: OAuthClient) =>
    runAction(
      () => admin.client.oauth2.deleteClient({ client_id: client.client_id }),
      "Client deleted",
    );

  return (
    <div class="space-y-6">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold">OAuth Clients</h1>
          <p class="text-sm text-muted-foreground">
            Register OAuth2 client applications that can sign users in. Use these credentials in
            your backend's OAuth config.
          </p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>Create client</Button>
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

      <Card title="Clients">
        <Show when={clients.loading}>
          <p class="text-sm text-muted-foreground">Loading clients…</p>
        </Show>
        <Show when={clients.error}>
          <p class="text-sm text-red-500">Failed to load clients: {formatError(clients.error)}</p>
        </Show>
        <Show when={!clients.loading && !clients.error && (clients()?.length ?? 0) === 0}>
          <p class="text-sm text-muted-foreground">No clients registered yet.</p>
        </Show>

        <Show when={(clients()?.length ?? 0) > 0}>
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <th class="px-3 py-2">Client</th>
                  <th class="px-3 py-2">Redirect URIs</th>
                  <th class="px-3 py-2">Type</th>
                  <th class="px-3 py-2">Created</th>
                  <th class="px-3 py-2 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                <For each={clients() ?? []}>
                  {(client) => (
                    <tr class="border-b border-border last:border-0">
                      <td class="px-3 py-2">
                        <div class="font-medium">{client.client_name ?? "(unnamed)"}</div>
                        <div class="break-all font-mono text-xs text-muted-foreground">
                          {client.client_id}
                        </div>
                        <Show when={client.disabled}>
                          <Badge tone="danger">Disabled</Badge>
                        </Show>
                      </td>
                      <td class="px-3 py-2">
                        <ul class="space-y-0.5">
                          <For each={client.redirect_uris}>
                            {(uri) => <li class="break-all text-xs">{uri}</li>}
                          </For>
                        </ul>
                      </td>
                      <td class="px-3 py-2 text-xs text-muted-foreground">
                        {client.token_endpoint_auth_method ?? "client_secret_basic"}
                      </td>
                      <td class="px-3 py-2 text-xs text-muted-foreground">
                        {formatDate(client.created_at)}
                      </td>
                      <td class="px-3 py-2">
                        <div class="flex flex-wrap items-center justify-end gap-1.5">
                          <ConfirmButton
                            label="Delete"
                            variant="danger"
                            onConfirm={() => removeClient(client)}
                          />
                        </div>
                      </td>
                    </tr>
                  )}
                </For>
              </tbody>
            </table>
          </div>
        </Show>
      </Card>

      <CreateClientModal
        open={createOpen()}
        busy={busy()}
        onClose={() => setCreateOpen(false)}
        onCreate={async (payload) => {
          const res = await runAction(
            () =>
              admin.client.oauth2.createClient({
                redirect_uris: payload.redirect_uris,
                client_name: payload.client_name,
                token_endpoint_auth_method: payload.token_endpoint_auth_method,
              }),
            "Client created",
          );
          if (res?.data) {
            const data = res.data as Partial<CreateClientResponse> & {
              client_id: string;
              client_secret?: string;
            };
            if (data.client_secret) {
              setCreatedSecret({
                client_id: data.client_id,
                client_secret: data.client_secret,
                client_id_issued_at: data.client_id_issued_at,
                client_secret_expires_at: data.client_secret_expires_at,
              });
            }
            setCreateOpen(false);
          }
        }}
      />

      <Show when={createdSecret()}>
        <ClientSecretModal data={createdSecret()!} onClose={() => setCreatedSecret(null)} />
      </Show>
    </div>
  );
}

function CreateClientModal(props: {
  open: boolean;
  busy: boolean;
  onClose: () => void;
  onCreate: (payload: {
    redirect_uris: string[];
    client_name?: string;
    token_endpoint_auth_method?: "none" | "client_secret_basic" | "client_secret_post";
  }) => void;
}) {
  const [name, setName] = createSignal("");
  const [redirectUris, setRedirectUris] = createSignal("");
  const [authMethod, setAuthMethod] = createSignal("client_secret_basic");

  const reset = () => {
    setName("");
    setRedirectUris("");
    setAuthMethod("client_secret_basic");
  };

  const submit = () => {
    const uris = redirectUris()
      .split(/[\n,]+/)
      .map((s) => s.trim())
      .filter(Boolean);
    if (uris.length === 0) return;
    props.onCreate({
      redirect_uris: uris,
      client_name: name().trim() || undefined,
      token_endpoint_auth_method: authMethod() as
        | "none"
        | "client_secret_basic"
        | "client_secret_post",
    });
    reset();
  };

  return (
    <Modal
      open={props.open}
      title="Create OAuth client"
      onClose={() => {
        reset();
        props.onClose();
      }}
      footer={
        <>
          <Button
            variant="outline"
            onClick={() => {
              reset();
              props.onClose();
            }}
            disabled={props.busy}
          >
            Cancel
          </Button>
          <Button disabled={props.busy || redirectUris().trim().length === 0} onClick={submit}>
            Create
          </Button>
        </>
      }
    >
      <Field label="Name" hint="Shown in the consent screen. Optional but recommended.">
        <Input
          type="text"
          value={name()}
          onInput={(e) => setName(e.currentTarget.value)}
          placeholder="e.g. Muzonia Backend"
        />
      </Field>
      <Field
        label="Redirect URIs"
        hint="One per line. The OAuth provider will only redirect to these URIs."
      >
        <textarea
          class="w-full rounded border border-input bg-background px-2 py-1 text-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          rows="3"
          value={redirectUris()}
          onInput={(e) => setRedirectUris(e.currentTarget.value)}
          placeholder="https://api.example.com/oauth/callback"
        />
      </Field>
      <Field
        label="Client type"
        hint="Public clients (PKCE) require no secret and are used for SPAs/mobile."
      >
        <Select
          value={authMethod()}
          options={TOKEN_AUTH_OPTIONS}
          onChange={(v) => setAuthMethod(v)}
        />
      </Field>
    </Modal>
  );
}

function ClientSecretModal(props: { data: CreateClientResponse; onClose: () => void }) {
  const [copied, setCopied] = createSignal<string | null>(null);

  const copy = async (text: string, key: string) => {
    try {
      await navigator.clipboard.writeText(text);
      setCopied(key);
      setTimeout(() => setCopied(null), 2000);
    } catch {
      // ponytail: clipboard may be unavailable (insecure context); user can copy manually.
    }
  };

  return (
    <Modal
      open={true}
      title="Client credentials"
      onClose={props.onClose}
      footer={<Button onClick={props.onClose}>Done</Button>}
    >
      <div class="space-y-3">
        <p class="rounded border border-warning bg-warning px-3 py-2 text-sm text-warning-foreground">
          Copy the client secret now — it will not be shown again.
        </p>
        <Field label="Client ID">
          <div class="flex gap-2">
            <Input readOnly value={props.data.client_id} class="font-mono" />
            <Button variant="outline" size="sm" onClick={() => copy(props.data.client_id, "id")}>
              {copied() === "id" ? "Copied" : "Copy"}
            </Button>
          </div>
        </Field>
        <Field label="Client secret">
          <div class="flex gap-2">
            <Input readOnly type="password" value={props.data.client_secret} class="font-mono" />
            <Button
              variant="outline"
              size="sm"
              onClick={() => copy(props.data.client_secret, "secret")}
            >
              {copied() === "secret" ? "Copied" : "Copy"}
            </Button>
          </div>
        </Field>
      </div>
    </Modal>
  );
}
