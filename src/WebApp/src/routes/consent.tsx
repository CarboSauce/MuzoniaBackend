import { useNavigate, useSearchParams } from "@solidjs/router";
import { createSignal, Show } from "solid-js";
import { Button, Card } from "~/components/admin/ui";
import { useAuth } from "~/components/BetterAuthProvider";

function parseScopes(scope: string | string[] | undefined): string[] {
  if (!scope) return [];
  if (Array.isArray(scope)) return scope.flatMap((s) => s.split(/\s+/)).filter(Boolean);
  return scope.split(/\s+/).filter(Boolean);
}

export default function Consent() {
  const [params] = useSearchParams();
  const auth = useAuth();
  const navigate = useNavigate();

  const [busy, setBusy] = createSignal(false);
  const [error, setError] = createSignal<string | null>(null);

  const clientId = () => (Array.isArray(params.client_id) ? params.client_id[0] : params.client_id) ?? "";
  const scopes = () => parseScopes(params.scope);

  const submit = async (accept: boolean) => {
    setBusy(true);
    setError(null);
    try {
      const res = await auth.oauth2.consent({ accept, scope: scopes().join(" ") });
      const data = (res as { data?: { redirect: boolean; url: string } })?.data;
      if (data?.url) {
        window.location.href = data.url;
      } else {
        navigate("/", { replace: true });
      }
    } catch (e: any) {
      setError(e?.message ?? "Failed to submit consent");
    } finally {
      setBusy(false);
    }
  };

  return (
    <main class="mx-auto max-w-lg p-6">
      <Card title="Authorize application">
        <div class="space-y-4">
          <p class="text-sm">
            A third-party application is requesting access to your account.
          </p>

          <Show when={clientId()}>
            <div>
              <p class="text-xs uppercase tracking-wide text-muted-foreground">Client ID</p>
              <p class="break-all font-mono text-sm">{clientId()}</p>
            </div>
          </Show>

          <Show when={scopes().length}>
            <div>
              <p class="text-xs uppercase tracking-wide text-muted-foreground">Requested scopes</p>
              <ul class="mt-1 list-disc space-y-0.5 pl-5 text-sm">
                {scopes().map((s) => (
                  <li>{s}</li>
                ))}
              </ul>
            </div>
          </Show>

          <Show when={error()}>
            <p class="text-sm text-red-500">{error()}</p>
          </Show>

          <div class="flex justify-end gap-2 pt-2">
            <Button variant="outline" disabled={busy()} onClick={() => submit(false)}>
              Deny
            </Button>
            <Button disabled={busy()} onClick={() => submit(true)}>
              Allow
            </Button>
          </div>
        </div>
      </Card>
    </main>
  );
}
