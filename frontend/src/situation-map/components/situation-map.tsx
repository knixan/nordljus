import { useRef } from "react";
import { formatAge } from "../format-age";
import { useElementSize } from "../hooks/use-element-size";
import { useNow } from "../hooks/use-now";
import { toLocalMeters } from "../projection";
import type { MapArea, TrackUpdate } from "../types";
import styles from "./situation-map.module.css";

const GRID_STEP_METERS = 100;
// Markörer och text har fast storlek på skärmen, som på en riktig karta – de krymper inte med ytan.
const MARKER_RADIUS_PX = 8;
const LABEL_FONT_PX = 14;
const LABEL_GAP_PX = 6;

export interface SituationMapProps {
  tracks: readonly TrackUpdate[];
  /** Området som visas. Allt i SVG:n är i meter från områdets centrum. */
  area: MapArea;
}

/**
 * Lägesbilden: en enkel yta med rutnät och en markör per spår, med namn och ålder.
 * Ersätts av en riktig karta i V3; komponentens props är tänkta att räcka även då.
 */
export function SituationMap({ tracks, area }: SituationMapProps) {
  const now = useNow();
  const svgRef = useRef<SVGSVGElement>(null);
  const size = useElementSize(svgRef);
  const halfWidth = area.widthMeters / 2;
  const halfHeight = area.heightMeters / 2;

  // SVG:n skalar ytan så att den får plats (preserveAspectRatio "meet"), alltså styr den
  // led som har minst plats. Före första mätningen, och när ytan är 0 px (dold flik),
  // antas 1 m per pixel – annars blir storlekarna Infinity.
  const metersPerPx =
    size && size.width > 0 && size.height > 0
      ? Math.max(area.widthMeters / size.width, area.heightMeters / size.height)
      : 1;
  const markerRadius = MARKER_RADIUS_PX * metersPerPx;
  const labelOffset = (MARKER_RADIUS_PX + LABEL_GAP_PX) * metersPerPx;

  return (
    <div className={styles.container}>
      <svg
        ref={svgRef}
        className={styles.map}
        viewBox={`${-halfWidth} ${-halfHeight} ${area.widthMeters} ${area.heightMeters}`}
        role="img"
        aria-label={`Lägesbild med ${tracks.length} enheter`}
      >
        <rect
          className={styles.terrain}
          x={-halfWidth}
          y={-halfHeight}
          width={area.widthMeters}
          height={area.heightMeters}
        />
        <Grid halfWidth={halfWidth} halfHeight={halfHeight} />

        {tracks.map((track) => {
          const { x, y } = toLocalMeters(track, area.center);
          // På östra halvan hamnar etiketten till vänster, så att den inte klipps vid kanten.
          const labelLeft = x > 0;
          return (
            <g key={track.trackId} transform={`translate(${x} ${y})`}>
              <circle className={styles.marker} r={markerRadius} />
              <text
                className={styles.label}
                x={labelLeft ? -labelOffset : labelOffset}
                textAnchor={labelLeft ? "end" : "start"}
                dy="0.35em"
                fontSize={LABEL_FONT_PX * metersPerPx}
                strokeWidth={3 * metersPerPx}
              >
                {track.name} · {formatAge(track.observedAt, now)}
              </text>
            </g>
          );
        })}
      </svg>

      {tracks.length === 0 && <p className={styles.empty}>Väntar på positioner…</p>}
    </div>
  );
}

function Grid({ halfWidth, halfHeight }: { halfWidth: number; halfHeight: number }) {
  const lines = [];
  for (let x = 0; x <= halfWidth; x += GRID_STEP_METERS) {
    for (const sx of x === 0 ? [0] : [x, -x]) {
      lines.push(<line key={`x${sx}`} x1={sx} x2={sx} y1={-halfHeight} y2={halfHeight} />);
    }
  }
  for (let y = 0; y <= halfHeight; y += GRID_STEP_METERS) {
    for (const sy of y === 0 ? [0] : [y, -y]) {
      lines.push(<line key={`y${sy}`} x1={-halfWidth} x2={halfWidth} y1={sy} y2={sy} />);
    }
  }
  return <g className={styles.grid}>{lines}</g>;
}
