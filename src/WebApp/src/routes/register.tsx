import { useAuth } from "~/components/BetterAuthProvider";
import * as z from "zod/mini";
import { useAppForm } from "~/components/FormField";
import { useUser } from "~/components/AppProvider";
import { redirect, useNavigate, useParams } from "@solidjs/router";

const loginSchema = z
  .object({
    username: z
      .string()
      .check(z.minLength(3, { error: "Username must be at least 3 chracters long" })),
    email: z.email({ error: "Field must contain valid e-mail address" }),
    password: z
      .string()
      .check(
        z.minLength(8, { error: "Password must be at least 8 characters long" }),
        z.regex(/[0-9]/, { error: "Password must contain at least one number" }),
        z.regex(/[A-Z]/, { error: "Password must contain at least one uppercase letter" }),
        z.regex(/[^A-Za-z0-9]/, { error: "Password must contain at least one special character" }),
      ),
    confirmPassword: z.string(),
  })
  .check(
    z.refine((data) => data.password == data.confirmPassword, { error: "Password's dont match" }),
  );

export default function Register() {
  const user = useUser();
  const navigate = useNavigate();

  const form = useAppForm(() => ({
    defaultValues: {
      email: "",
      password: "",
      username: "",
      confirmPassword: "",
    },
    onSubmit: async ({ value: { username, email, password } }) => {
      await user.signUp(email, username, password);
      navigate(`/verify-email/${encodeURIComponent(email)}`);
    },
    validators: {
      onChange: loginSchema,
    },
    asyncDebounceMs: 300,
  }));

  return (
    <main class="p-6 max-w-lg mx-auto">
      <h1 class="text-2xl font-bold mb-4">Register</h1>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          e.stopPropagation();
          form.handleSubmit();
        }}
        class="space-y-4"
      >
        <form.AppField
          name="email"
          children={(field) => (
            <field.TextField
              errorClass="text-red-500"
              inputClass="mt-1 w-full rounded border px-2 py-1"
              labelClass="block text-sm font-medium"
              label="Email"
              type="text"
            />
          )}
        />
        <form.AppField
          name="username"
          children={(field) => (
            <field.TextField
              errorClass="text-red-500"
              inputClass="mt-1 w-full rounded border px-2 py-1"
              labelClass="block text-sm font-medium"
              label="Username"
              type="text"
            />
          )}
        />
        <form.AppField
          name="password"
          children={(field) => (
            <field.TextField
              errorClass="text-red-500"
              inputClass="mt-1 w-full rounded border px-2 py-1"
              labelClass="block text-sm font-medium"
              label="Password"
              type="password"
            />
          )}
        />
        <form.AppField
          name="confirmPassword"
          children={(field) => (
            <field.TextField
              errorClass="text-red-500"
              inputClass="mt-1 w-full rounded border px-2 py-1"
              labelClass="block text-sm font-medium"
              label="Confirm password"
              type="password"
            />
          )}
        />
        <form.Subscribe
          selector={(state) => ({
            canSubmit: state.canSubmit,
            isSubmitting: state.isSubmitting,
          })}
          children={(state) => (
            <button
              class="px-4 py-2 bg-blue-600 text-white rounded"
              type="submit"
              disabled={!state().canSubmit}
            >
              Register
            </button>
          )}
        ></form.Subscribe>
      </form>
    </main>
  );
}
