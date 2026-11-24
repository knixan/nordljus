import { describe, expect, it } from "vitest";
import { formatAge } from "./format-age";

const observedAt = "2026-11-24T10:00:00Z";
const t0 = Date.parse(observedAt);

describe("formatAge", () => {
  it.each([
    [0, "0 s sedan"],
    [999, "0 s sedan"],
    [1_000, "1 s sedan"],
    [59_999, "59 s sedan"],
    [60_000, "1 min sedan"],
    [40 * 60_000, "40 min sedan"],
    [60 * 60_000, "1 h sedan"],
    [5 * 60 * 60_000, "5 h sedan"],
  ])("%i ms gammal → %s", (ageMs, expected) => {
    expect(formatAge(observedAt, t0 + ageMs)).toBe(expected);
  });

  it("en tid i framtiden visas som 0 s sedan", () => {
    expect(formatAge(observedAt, t0 - 2_000)).toBe("0 s sedan");
  });

  it("en ogiltig tid ger okänd ålder", () => {
    expect(formatAge("igår", t0)).toBe("okänd ålder");
  });
});
