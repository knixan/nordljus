import { describe, expect, it } from "vitest";
import { applyUpdates, type Tracks } from "./situation-state";
import type { TrackUpdate } from "./types";

function jarn1(observedAt: string, latitude = 57.9): TrackUpdate {
  return {
    trackId: "jarn-1",
    name: "Järn 1",
    type: "groundVehicle",
    latitude,
    longitude: 15.3,
    observedAt,
  };
}

const empty: Tracks = new Map();

describe("applyUpdates", () => {
  it("lägger till ett nytt spår", () => {
    const next = applyUpdates(empty, [jarn1("2026-11-24T10:00:00Z")]);

    expect(next.get("jarn-1")?.observedAt).toBe("2026-11-24T10:00:00Z");
  });

  it("ersätter med en nyare position", () => {
    const first = applyUpdates(empty, [jarn1("2026-11-24T10:00:00Z", 57.9)]);
    const next = applyUpdates(first, [jarn1("2026-11-24T10:00:01Z", 57.91)]);

    expect(next.get("jarn-1")?.latitude).toBe(57.91);
  });

  it("ignorerar en äldre position som kommer i fel ordning", () => {
    const first = applyUpdates(empty, [jarn1("2026-11-24T10:00:05Z", 57.95)]);
    const next = applyUpdates(first, [jarn1("2026-11-24T10:00:04Z", 57.94)]);

    expect(next.get("jarn-1")?.latitude).toBe(57.95);
    expect(next).toBe(first);
  });

  it("tar den nyaste när samma spår finns flera gånger i en lista", () => {
    const next = applyUpdates(empty, [
      jarn1("2026-11-24T10:00:02Z", 57.92),
      jarn1("2026-11-24T10:00:03Z", 57.93),
      jarn1("2026-11-24T10:00:01Z", 57.91),
    ]);

    expect(next.get("jarn-1")?.latitude).toBe(57.93);
  });

  it("ändrar inte det gamla tillståndet", () => {
    const first = applyUpdates(empty, [jarn1("2026-11-24T10:00:00Z", 57.9)]);
    applyUpdates(first, [jarn1("2026-11-24T10:00:01Z", 57.91)]);

    expect(first.get("jarn-1")?.latitude).toBe(57.9);
  });

  it("returnerar samma objekt för en tom lista", () => {
    expect(applyUpdates(empty, [])).toBe(empty);
  });
});
