import { ConnectionStatus, SituationMap, useSituation, type MapArea } from "../../situation-map";
import styles from "./situation-page.module.css";

// Samma fiktiva område som simulatorn kör i (src/Nordljus.Simulator/Program.cs).
// Försvinner när riktig karta finns (V3).
const SIMULATOR_AREA: MapArea = {
  center: { latitude: 57.9, longitude: 15.3 },
  widthMeters: 600,
  heightMeters: 600,
};

/** Operatörens lägesbild för team Nordljus. */
export function SituationPage() {
  const { tracks, connection } = useSituation();

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1 className={styles.title}>Nordljus – lägesbild</h1>
        <ConnectionStatus connection={connection} />
      </header>
      <main className={styles.main}>
        <SituationMap tracks={tracks} area={SIMULATOR_AREA} />
      </main>
    </div>
  );
}
