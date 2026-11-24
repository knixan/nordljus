// Publikt API för lägesbilden. Andra team (Havsörn) importerar bara härifrån.
export { ConnectionStatus, type ConnectionStatusProps } from "./components/connection-status";
export { SituationMap, type SituationMapProps } from "./components/situation-map";
export { useSituation, type Situation } from "./hooks/use-situation";
export type { ConnectionState, GeoPoint, MapArea, TrackType, TrackUpdate } from "./types";
