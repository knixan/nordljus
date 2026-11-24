/** Speglar `TrackType` i Nordljus.Contracts. Backend skickar enum som camelCase-text. */
export type TrackType = "personnel" | "groundVehicle" | "helicopter";

/** Speglar `TrackUpdate` i Nordljus.Contracts, så som SignalR skickar det (camelCase JSON). */
export interface TrackUpdate {
  trackId: string;
  name: string;
  type: TrackType;
  latitude: number;
  longitude: number;
  /** När sensorn mätte positionen, ISO 8601 i UTC. */
  observedAt: string;
}

/** Anslutningen till backend, så som operatören behöver se den. */
export type ConnectionState = "connecting" | "connected" | "reconnecting" | "disconnected";

/** En position i grader (WGS 84). */
export interface GeoPoint {
  latitude: number;
  longitude: number;
}

/** Området som kartan visar: ett centrum och en storlek i meter. */
export interface MapArea {
  center: GeoPoint;
  widthMeters: number;
  heightMeters: number;
}
