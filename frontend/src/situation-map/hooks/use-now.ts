import { useEffect, useState } from "react";

/**
 * Klockan som tillstånd, uppdaterad med jämna mellanrum. Används för att visa ålder
 * ("12 s sedan") – en timer för hela kartan, inte en per markör.
 */
export function useNow(intervalMs = 1_000): number {
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    const id = setInterval(() => setNow(Date.now()), intervalMs);
    return () => clearInterval(id);
  }, [intervalMs]);

  return now;
}
