import { useNavigate, useParams } from "@solidjs/router";
import { createEffect, createSignal, Match, onCleanup, onMount, Switch } from "solid-js";
import { useUser } from "~/components/AppProvider";
import {
  OTPField,
  OTPFieldGroup,
  OTPFieldInput,
  OTPFieldSeparator,
  OTPFieldSlot,
} from "~/components/ui/otp-field";
import { z } from "zod/mini";

export default function VerifyEmail() {
  const params = useParams();
  const { sendVerificationOtp, verifyEmail } = useUser();
  const [otp, setOtp] = createSignal("");
  const [isSent, setIsSent] = createSignal<boolean | null>(null);
  const navigate = useNavigate();

  onMount(async () => {
    const res = z.email().safeParse(params.email);
    if (res.data === undefined) {
      navigate("/login", { replace: true });
      return;
    }
    const result = await sendVerificationOtp(res.data);
    if (!result) {
      navigate("/login", { replace: true });
    }
  });

  let timer: NodeJS.Timeout;
  createEffect(() => {
    if (isSent() == true) {
      timer = setTimeout(() => {
        navigate("/login");
      }, 3000);

      onCleanup(() => {
        clearTimeout(timer);
      });
    }
  });

  return (
    <main class="p-6 max-w-lg mx-auto">
      <div class="space-y-1">
        <span>One time code has been sent over to {params.email}</span>
        <OTPField maxLength={6} onComplete={(val) => setOtp(val)}>
          <OTPFieldInput />
          <OTPFieldGroup>
            <OTPFieldSlot index={0} />
            <OTPFieldSlot index={1} />
            <OTPFieldSlot index={2} />
          </OTPFieldGroup>
          <OTPFieldSeparator />
          <OTPFieldGroup>
            <OTPFieldSlot index={3} />
            <OTPFieldSlot index={4} />
            <OTPFieldSlot index={5} />
          </OTPFieldGroup>
        </OTPField>
        <button
          class="px-4 py-2 bg-blue-600 text-white rounded"
          onClick={async () => {
            if (otp().length == 6) {
              const res = await verifyEmail(params.email as string, otp());
              if (res) {
                setIsSent(true);
              } else {
                setIsSent(false);
              }
            } else {
              setIsSent(false);
            }
          }}
        >
          CONFIRM
        </button>
        <span>
          <Switch>
            <Match when={isSent() == false}>"Can't verify"</Match>
            <Match when={isSent() == true}>"Succesfully verified"</Match>
          </Switch>
        </span>
      </div>
    </main>
  );
}
