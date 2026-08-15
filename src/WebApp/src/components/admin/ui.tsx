import { For, Show, createSignal, splitProps } from "solid-js";
import type { JSX } from "solid-js";
import { cn } from "~/lib/utils";

type ButtonVariant = "default" | "secondary" | "outline" | "danger" | "ghost";
type ButtonSize = "sm" | "md";

export function Button(props: JSX.ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant;
  size?: ButtonSize;
}) {
  const [local, rest] = splitProps(props, ["variant", "size", "class", "type"]);
  return (
    <button
      type={local.type ?? "button"}
      class={cn(
        "inline-flex items-center justify-center gap-1 rounded border font-medium transition-colors disabled:pointer-events-none disabled:opacity-50",
        local.variant === "default" && "border-foreground bg-foreground text-background hover:opacity-90",
        local.variant === "secondary" && "border-border bg-secondary text-secondary-foreground hover:bg-accent",
        local.variant === "outline" && "border-border bg-transparent text-foreground hover:bg-accent",
        local.variant === "danger" && "border-destructive bg-destructive text-destructive-foreground hover:opacity-90",
        local.variant === "ghost" && "border-transparent bg-transparent text-foreground hover:bg-accent",
        local.size === "sm" ? "px-2 py-1 text-xs" : "px-3 py-1.5 text-sm",
        local.class,
      )}
      {...rest}
    />
  );
}

export function Input(props: JSX.InputHTMLAttributes<HTMLInputElement>) {
  const [local, rest] = splitProps(props, ["class"]);
  return (
    <input
      class={cn(
        "w-full rounded border border-input bg-background px-2 py-1 text-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        local.class,
      )}
      {...rest}
    />
  );
}

export function Select(props: {
  value?: string;
  options: { value: string; label: string }[];
  placeholder?: string;
  class?: string;
  disabled?: boolean;
  onChange?: (value: string) => void;
}) {
  return (
    <select
      class={cn(
        "w-full rounded border border-input bg-background px-2 py-1 text-sm disabled:opacity-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        props.class,
      )}
      value={props.value}
      disabled={props.disabled}
      onChange={(e) => props.onChange?.(e.currentTarget.value)}
    >
      <Show when={props.placeholder}>
        <option value="" disabled>
          {props.placeholder}
        </option>
      </Show>
      <For each={props.options}>
        {(option) => <option value={option.value}>{option.label}</option>}
      </For>
    </select>
  );
}

export function Field(props: { label: string; hint?: string; error?: string; children: JSX.Element }) {
  return (
    <div class="space-y-1">
      <label class="block text-sm font-medium">{props.label}</label>
      {props.children}
      <Show when={props.hint}>
        <p class="text-xs text-muted-foreground">{props.hint}</p>
      </Show>
      <Show when={props.error}>
        <p class="text-xs text-red-500">{props.error}</p>
      </Show>
    </div>
  );
}

export function Badge(props: {
  tone: "default" | "success" | "danger" | "warning" | "info";
  children: JSX.Element;
}) {
  return (
    <span
      class={cn(
        "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium",
        props.tone === "default" && "bg-muted text-muted-foreground",
        props.tone === "success" && "bg-success text-success-foreground",
        props.tone === "danger" && "bg-error text-error-foreground",
        props.tone === "warning" && "bg-warning text-warning-foreground",
        props.tone === "info" && "bg-info text-info-foreground",
      )}
    >
      {props.children}
    </span>
  );
}

export function Card(props: { title?: string; actions?: JSX.Element; children: JSX.Element }) {
  return (
    <div class="rounded-lg border border-border bg-background">
      <div class="flex items-center justify-between border-b border-border px-4 py-3">
        <h2 class="font-semibold">{props.title}</h2>
        <div class="flex items-center gap-2">{props.actions}</div>
      </div>
      <div class="p-4">{props.children}</div>
    </div>
  );
}

export function Modal(props: {
  open: boolean;
  title: string;
  onClose: () => void;
  children: JSX.Element;
  footer?: JSX.Element;
}) {
  return (
    <Show when={props.open}>
      <div
        class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/40 p-4"
        onClick={props.onClose}
      >
        <div
          class="mt-8 w-full max-w-lg rounded-lg border border-border bg-background p-5 shadow-lg"
          onClick={(e) => e.stopPropagation()}
        >
          <div class="mb-4 flex items-center justify-between">
            <h2 class="text-lg font-semibold">{props.title}</h2>
            <Button variant="ghost" size="sm" onClick={props.onClose} aria-label="Close">
              &times;
            </Button>
          </div>
          <div class="space-y-4">{props.children}</div>
          <Show when={props.footer}>
            <div class="mt-4 flex justify-end gap-2 border-t border-border pt-4">{props.footer}</div>
          </Show>
        </div>
      </div>
    </Show>
  );
}

export function ConfirmButton(props: {
  label: string;
  confirmLabel?: string;
  class?: string;
  variant?: ButtonVariant;
  disabled?: boolean;
  onConfirm: () => void;
}) {
  const [armed, setArmed] = createSignal(false);
  let timer: ReturnType<typeof setTimeout> | undefined;
  return (
    <Button
      variant={props.variant}
      size="sm"
      class={props.class}
      disabled={props.disabled}
      onClick={() => {
        if (!armed()) {
          setArmed(true);
          timer = setTimeout(() => setArmed(false), 3000);
        } else {
          if (timer) clearTimeout(timer);
          setArmed(false);
          props.onConfirm();
        }
      }}
    >
      {armed() ? props.confirmLabel ?? "Confirm?" : props.label}
    </Button>
  );
}
