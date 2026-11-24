import { HubConnectionBuilder, LogLevel, type IRetryPolicy } from "@microsoft/signalr";
import { useEffect, useMemo, useState } from "react";
import { applyUpdates, type Tracks } from "../situation-state";
import type { ConnectionState, TrackUpdate } from "../types";

export interface Situation {
  /** Senaste position per spår. */
  tracks: readonly TrackUpdate[];
  connection: ConnectionState;
}

/**
 * En lägesbild ska aldrig ge upp. SignalR:s standard slutar efter fyra försök (0, 2, 10, 30 s);
 * den här försöker igen för alltid: direkt, sedan var 2:a sekund och efter tre försök var 5:e.
 */
const retryForever: IRetryPolicy = {
  nextRetryDelayInMilliseconds: ({ previousRetryCount }) =>
    previousRetryCount === 0 ? 0 : previousRetryCount < 3 ? 2_000 : 5_000,
};

const START_RETRY_MS = 2_000;

/**
 * Ansluter till lägesbildens SignalR-hubb och håller senaste position per spår.
 * Backend skickar `TracksUpdated` med en lista positioner.
 */
export function useSituation(hubUrl = "/hubs/situation"): Situation {
  const [tracks, setTracks] = useState<Tracks>(() => new Map());
  const [connection, setConnection] = useState<ConnectionState>("connecting");

  useEffect(() => {
    const hub = new HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect(retryForever)
      .configureLogging(LogLevel.Warning)
      .build();

    let stopped = false;

    // Handlers registreras före start (enligt dokumentationen).
    hub.on("TracksUpdated", (updates: TrackUpdate[]) => {
      setTracks((previous) => applyUpdates(previous, updates));
    });
    hub.onreconnecting(() => setConnection("reconnecting"));
    hub.onreconnected(() => setConnection("connected"));
    hub.onclose(() => {
      if (!stopped) setConnection("disconnected");
    });

    // Automatisk återanslutning gäller inte första starten – den försöker vi själva.
    const starting = (async () => {
      while (!stopped) {
        try {
          await hub.start();
          if (!stopped) setConnection("connected");
          return;
        } catch {
          if (stopped) return;
          // Servern svarar inte: visa det som ett avbrott, inte som en lugn "ansluter".
          setConnection("reconnecting");
          await new Promise((resolve) => setTimeout(resolve, START_RETRY_MS));
        }
      }
    })();

    return () => {
      stopped = true;
      // Vänta in en pågående start innan vi stoppar. Annars avbryts förhandlingen och
      // SignalR loggar ett fel – det händer varje gång i utveckling, där StrictMode kör
      // effekten två gånger.
      void starting.then(() => hub.stop());
    };
  }, [hubUrl]);

  const trackList = useMemo(() => [...tracks.values()], [tracks]);
  return { tracks: trackList, connection };
}
