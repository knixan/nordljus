import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { App } from "./app/app";
import "./theme.css";

const root = document.getElementById("root");
if (!root) {
  throw new Error("Hittar inte #root i index.html.");
}

createRoot(root).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
