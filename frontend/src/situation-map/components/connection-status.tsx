import type { ConnectionState } from "../types";
import styles from "./connection-status.module.css";

// Status bärs av text och symbol, inte bara färg.
const TEXT: Record<ConnectionState, { symbol: string; label: string }> = {
  connecting: { symbol: "…", label: "Ansluter till servern" },
  connected: { symbol: "●", label: "Ansluten" },
  reconnecting: { symbol: "⚠", label: "Ingen kontakt med servern – återansluter" },
  disconnected: { symbol: "✕", label: "Frånkopplad från servern" },
};

export interface ConnectionStatusProps {
  connection: ConnectionState;
}

/** Visar om lägesbilden är ansluten till backend. Ett avbrott får aldrig se ut som en lugn bild. */
export function ConnectionStatus({ connection }: ConnectionStatusProps) {
  const { symbol, label } = TEXT[connection];
  const alert = connection === "reconnecting" || connection === "disconnected";

  return (
    <p className={alert ? `${styles.status} ${styles.alert}` : styles.status} role="status">
      <span aria-hidden="true">{symbol}</span> {label}
    </p>
  );
}
