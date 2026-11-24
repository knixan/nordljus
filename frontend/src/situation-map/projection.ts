import type { GeoPoint } from "./types";

// Ungefär så många meter går det på en breddgrad. Tillräckligt noggrant för några kilometer.
const METERS_PER_DEGREE_LATITUDE = 111_320;

/** En punkt i meter från kartans centrum: x österut, y söderut (som i SVG). */
export interface LocalPoint {
  x: number;
  y: number;
}

/**
 * Räknar om en position i grader till meter från centrum (ekvirektangulär projektion).
 * En längdgrad är kortare ju längre norrut man är, därför `cos(latitud)`.
 * Ersätts av kartbibliotekets projektion när riktig karta kommer (V3).
 */
export function toLocalMeters(point: GeoPoint, center: GeoPoint): LocalPoint {
  const metersPerDegreeLongitude =
    METERS_PER_DEGREE_LATITUDE * Math.cos((center.latitude * Math.PI) / 180);

  return {
    x: (point.longitude - center.longitude) * metersPerDegreeLongitude,
    y: -(point.latitude - center.latitude) * METERS_PER_DEGREE_LATITUDE,
  };
}
