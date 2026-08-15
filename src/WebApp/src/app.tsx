import { MetaProvider, Title } from "@solidjs/meta";
import { Router } from "@solidjs/router";
import { FileRoutes } from "@solidjs/start/router";
import { Suspense } from "solid-js";
import "./app.css";
import { AppProvider } from "./components/AppProvider";

export default function App() {
  return (
    <Router
      root={(props) => (
        <AppProvider>
          <MetaProvider>
            <Title>Muzonia Web</Title>
            <Suspense>{props.children}</Suspense>
          </MetaProvider>
        </AppProvider>
      )}
    >
      <FileRoutes />
    </Router>
  );
}
