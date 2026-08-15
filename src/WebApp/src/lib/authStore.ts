import { redirect, useNavigate } from "@solidjs/router";
import { createSignal } from "solid-js";
import { useAuth } from "~/components/BetterAuthProvider";

export type User = {
  email: string,
  name: string,
  avatar?: string | null
};

export type UserState = {
  state: "signedIn",
  user: User,
  token: string
} | {
  state: "unverified"
  user: User,
} | {
  state: "signedOut"
} | {
  state: "invalid"
}

export default function createUserAuth() {
  const auth = useAuth();
  const navigate = useNavigate();
  const [user, setUser] = createSignal<UserState>({state: "signedOut"});

  return {
    async signIn(email: string, password: string) : Promise<boolean> {
      const resp = await auth.signIn.email({
        email,
        password,
        callbackURL: "/",
        rememberMe: true,
      });

      if (resp.data) {
        const user = resp.data.user;
        setUser({
          state: user.emailVerified ? "signedIn" : "unverified",
          token: resp.data.token,
          user: {
            email: user.email,
            name: user.name,
            avatar: user.image
          }
        })
        return true;
      } else {
        setUser({ state: "invalid" })
        return false;
      }
    },

    async signUp(email: string, name: string, password: string) : Promise<boolean> {
      const resp = await auth.signUp.email({
        name: name,
        email: email,
        password: password,
        callbackURL: undefined,
        image: undefined,
      });

      if (resp.data) {
        const user = resp.data.user;
        setUser({
          state: "unverified",
          user: {
            email: user.email,
            name: user.name,
            avatar: user.image
          }
        })
        return true;
      } else {
        setUser({
          state: "invalid"
        })
        return false;
      }
    },

    async sendVerificationOtp(email: string) : Promise<boolean> {
      const { data, error } = await auth.emailOtp.sendVerificationOtp({email, type: "email-verification"});
      if (data?.success) {
        return true
      }
      return false;
    },
    async verifyEmail(email: string, otp: string) : Promise<boolean> {
      const {error} = await auth.emailOtp.verifyEmail({
        email, otp
      });

      return !error;
    },
    user,
    isSignedIn() {
      return user().state == "signedIn";
    },
    requireUnverifiedUser() {
      const usr = user();
      if (usr.state != "unverified") {
        navigate("/login");
      }
      return usr;
    },
    requireUser() {
      const usr = user();
      if (usr.state != "signedIn") {
        navigate("/login");
      }
      return usr;
    }
  }
}
