import { useAuth } from "~/components/BetterAuthProvider";
import { createForm } from "@tanstack/solid-form";
import * as z from "zod/mini";
import { useNavigate } from "@solidjs/router";
import { createSignal, Show } from "solid-js";
import { useUser } from "~/components/AppProvider";

// const loginSchema = z.object({
//   email: z.email(),
//   password: z
//     .string()
//     .check(
//       z.minLength(8, { error: "Password must be at least 8 characters long" }),
//       z.regex(/[0-9]/, { error: "Password must contain at least one number" }),
//       z.regex(/[A-Z]/, { error: "Password must contain at least one uppercase letter" }),
//       z.regex(/[^A-Za-z0-9]/, { error: "Password must contain at least one special character" }),
//     ),
// });

const loginSchema = z.object({
  email: z.email(),
  password: z.string(),
});

export default function Login() {
  const { signIn } = useUser();
  const navigate = useNavigate();
  const [isFailedLogin, setFailedLogin] = createSignal(false);
  const form = createForm(() => ({
    defaultValues: {
      email: "",
      password: "",
    },
    onSubmit: async ({ value: { email, password } }) => {
      const result = await signIn(email, password);

      if (result) {
        navigate("/");
      } else {
        setFailedLogin(true);
      }
    },
    validators: {
      onChange: loginSchema,
    },
  }));

  return (
    <main class="p-6 max-w-lg mx-auto">
      <h1 class="text-2xl font-bold mb-4">Login</h1>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          e.stopPropagation();
          form.handleSubmit();
        }}
        class="space-y-4"
      >
        <form.Field
          name={"email"}
          children={(field) => (
            <div>
              <label class="block text-sm font-medium">Email</label>
              <input
                class="mt-1 w-full rounded border px-2 py-1"
                type="text"
                onInput={(e) => field().handleChange(e.target.value)}
                onBlur={field().handleBlur}
                value={field().state.value}
              />
              <p class="text-red-500">{field().state.meta.errorMap.onDynamic}</p>
            </div>
          )}
        />
        <form.Field
          name={"password"}
          children={(field) => (
            <div>
              <label class="block text-sm font-medium">Password</label>
              <input
                class="mt-1 w-full rounded border px-2 py-1"
                type="password"
                onInput={(e) => field().handleChange(e.target.value)}
                onBlur={field().handleBlur}
                value={field().state.value}
              />
              <p class="text-red-500">{field().state.meta.errorMap.onDynamic}</p>
            </div>
          )}
        />
        <button class="px-4 py-2 bg-blue-600 text-white rounded" type="submit">
          Login
        </button>
      </form>
      <Show when={isFailedLogin()}>
        <span>Failed to login</span>
      </Show>
    </main>
  );
}
