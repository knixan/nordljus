/**
 * Hur gammal en position är, för operatören: "3 s sedan", "2 min sedan", "1 h sedan".
 * `now` är millisekunder sedan epoch (`Date.now()`). En tid i framtiden (klockor som går
 * lite olika) visas som "0 s sedan".
 */
export function formatAge(observedAt: string, now: number): string {
  const observed = Date.parse(observedAt);
  if (Number.isNaN(observed)) {
    return "okänd ålder";
  }

  const seconds = Math.max(0, Math.floor((now - observed) / 1000));
  if (seconds < 60) {
    return `${seconds} s sedan`;
  }

  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) {
    return `${minutes} min sedan`;
  }

  return `${Math.floor(minutes / 60)} h sedan`;
}
