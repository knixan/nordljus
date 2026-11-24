import { describe, expect, it } from "vitest";
import { toLocalMeters } from "./projection";

const center = { latitude: 57.9, longitude: 15.3 };

describe("toLocalMeters", () => {
  it("centrum blir origo", () => {
    const point = toLocalMeters(center, center);

    expect(point.x).toBeCloseTo(0);
    expect(point.y).toBeCloseTo(0);
  });

  it("norrut blir negativt y, som i SVG", () => {
    const point = toLocalMeters({ latitude: 57.901, longitude: 15.3 }, center);

    expect(point.x).toBeCloseTo(0);
    expect(point.y).toBeCloseTo(-111.32, 1);
  });

  it("österut blir positivt x och kortare än en breddgrad på 58° N", () => {
    const point = toLocalMeters({ latitude: 57.9, longitude: 15.301 }, center);

    // 111 320 m · cos(57,9°) ≈ 59 160 m per grad → 0,001° ≈ 59,2 m.
    expect(point.x).toBeCloseTo(59.16, 1);
    expect(point.y).toBeCloseTo(0);
  });
});
