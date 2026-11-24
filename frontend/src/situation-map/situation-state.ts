import type { TrackUpdate } from "./types";

/** Senaste kända position per spår, nyckel = `trackId`. */
export type Tracks = ReadonlyMap<string, TrackUpdate>;

/**
 * Lägger in nya positioner. En position som är äldre än den som redan visas för samma spår
 * ignoreras – UDP lovar inte ordning, och pricken får aldrig hoppa bakåt.
 * Returnerar samma objekt om inget ändrades, så att React kan hoppa över omritning.
 */
export function applyUpdates(previous: Tracks, updates: readonly TrackUpdate[]): Tracks {
  let next: Map<string, TrackUpdate> | null = null;

  for (const update of updates) {
    const current = (next ?? previous).get(update.trackId);
    if (current && Date.parse(update.observedAt) < Date.parse(current.observedAt)) {
      continue;
    }
    next ??= new Map(previous);
    next.set(update.trackId, update);
  }

  return next ?? previous;
}
