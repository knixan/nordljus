---
name: granska-kritiskt
description: 'Steg 6 i arbetsflödet: granska en ändring kritiskt på fem axlar: A realtid och robusthet (latens, staleness, trasig data, återanslutning), B spårbarhet och säkerhet (händelselogg, isolerat nät, beroenden, hemligheter), C återanvändning (kontrakt, adapters, delad komponent), D UI och tillgänglighet (mörker, kontrast, status utan färg, prestanda), E dokumentation och teamet (README, ADR, workshop). Graderar fynd P0–P3 och prövar varje fynd innan rättning. Use after /granska-mot-planen, or when the user asks to review critically. Triggers on "granska kritiskt", "kritisk granskning", "red team", "är det här rätt", "/granska-kritiskt". Not a general bug hunt: that is /code-review.'
---

# Granska kritiskt

`/granska-mot-planen` frågar "gjorde vi det vi sa?". Det här steget frågar "är det vi sa
**rätt**, för operatören, för nästa team och för den som ska ta över?". En plan kan vara
uppfylld och lägesbilden ändå visa gammal data som färsk.

## 1. Vad granskas

Jobbet sker på `main` och användaren committar själv. Diffen är därför:

- argumentet om det finns (`HEAD~3`, en commit, `#12`: commits med `#12` i meddelandet),
- annars det som inte är committat: `git status --short` och `git diff HEAD`. Nya filer
  syns inte i `git diff`, läs dem direkt.

Läs planfilen om det finns en. Kör bara de axlar diffen rör:

| Axel                              | Rörs när diffen har …                                                                     |
| --------------------------------- | ----------------------------------------------------------------------------------------- |
| **A. Realtid och robusthet**      | ingest, adapters, `Nordljus.Core` (validering, staleness), hubben, `useSituation`, simulatorn |
| **B. Spårbarhet och säkerhet**    | händelseloggen, EF Core, migreringar, endpoints, beroenden, Docker, CI, konfig, `index.html` |
| **C. Återanvändning**             | `Nordljus.Contracts`, adapters, `frontend/src/situation-map/`, något Havsörn använder     |
| **D. UI och tillgänglighet**      | `.tsx`, `theme.css`, ikoner, kartan, rendering                                            |
| **E. Dokumentation och teamet**   | `docs/`, README, ADR, workshop                                                            |

## 2. Axlarna, vad som granskas och mot vad

**A. Realtid och robusthet, mot `AGENTS.md` § Domänregler**

- Följ ett meddelande hela vägen: simulator → adapter → validering → logg → hubb → markör.
  Var kan det fastna, tappas eller komma i fel ordning? Vad händer med ett meddelande som
  är äldre än det senaste?
- Staleness: styrs tiderna av konfigurationen och av `TimeProvider`? Skriv ett test med
  styrd klocka om det saknas. Kan en sensor fastna i Live?
- Trasig data (fel JSON, saknade fält, NaN, position utanför området, tid i framtiden):
  kraschar ingest? Loggas det?
- Återanslutning: vad ser operatören medan SignalR är nere, och efteråt? Hämtas läget om?
- Latens: lägger ändringen till något synkront i den varma vägen (databasanrop per
  meddelande, ett meddelande per objekt)?

**B. Spårbarhet och säkerhet, mot `AGENTS.md` § Domänregler, § Beroenden, § Databas**

- Varje mottaget meddelande, även trasigt, hamnar i händelseloggen med tid, källa, rådata
  och resultat. Inget uppdaterar eller raderar i den.
- Inget hämtas från internet vid körning. Sök i diffen efter `http://` och `https://`.
- Nya paket står i `docs/dependencies.md` med låst version, licens och motivering.
- Inga hemligheter i `appsettings.json`, koden eller images. `ProblemDetails` utan stacktrace.
- Docker: kör containrarna som icke-root? Exponeras bara de portar som behövs?

**C. Återanvändning, mot `AGENTS.md` § Gör så här**

- Kör `git diff --stat` mot kärnprojekten när en adapter lagts till: tomt?
- Ändras kontraktet: bakåtkompatibelt, versionerat, ADR skriven? Vad händer med Havsörn?
- Importerar `situation-map/` något Nordljus-specifikt? Skulle ett tredje team kunna använda
  komponenten utan att ändra den?

**D. UI och tillgänglighet, mot `AGENTS.md` § UI**

- Mörkt läge som standard, färger bara från temat, kontrast räknad (4,5:1 text, 3:1
  ikoner mot kartan).
- Status syns utan färg. Gammal data ser aldrig färsk ut.
- Skärmbilderna från `/granska-i-appen` är underlaget: titta på dem igen med de här
  frågorna.
- Rendering: håller det vid 5 000 objekt? Skapas en React-komponent per objekt i onödan?

**E. Dokumentation och teamet**

- README: kan en ny utvecklare köra igång på under 15 minuter med bara den? Följ den
  ordagrant.
- ADR: sammanhang, alternativ, beslut, konsekvenser – och ärliga nackdelar?
- Stämmer dokumentationen med koden efter ändringen?

## 3. Underagenter, bara när diffen är stor

Är diffen stor (många filer, flera axlar) kan en underagent (`general-purpose`) ta en
axel. Högst tre samtidigt. Ge den bara: diffkommandot, planfilens sökväg, axelns frågor
ovan och raden "Granska bara, ändra inga filer, starta inga skills eller agenter."
Aldrig din egen sammanfattning av arbetet: poängen är en läsare som inte skrev koden.

## 4. Gradera, och pröva varje fynd

- **P0** gammal data visas som färsk, ett meddelande försvinner utan att loggas, något
  hämtas från internet, en hemlighet i repot, kärnan beror på en adapter
- **P1** ingest eller frontend kraschar på trasig data, ingen återanslutning, latens över
  1 s, `docker compose up` går sönder, Havsörn måste ändra kärnan
- **P2** synligt fel som operatören märker (kontrast, etikett, layout), paket utan
  motivering, dokumentation som inte stämmer
- **P3** kantfall, kosmetik, förbättring

Ett fynd är **bekräftat** först när du reproducerat det och hittat varför. Avvisar du ett
fynd: säg mekanismen som gör att det fungerar, inte bara att det fungerade där du tittade.
Behövs ett beslut som bara användaren kan ta: lägg det under "Frågor".

## Ut

```
## Granska kritiskt, <axlarna som kördes>
P0/P1/P2/P3 per fynd: vad · var (fil:rad eller skärmbild) · regel (AGENTS.md §) · bekräftat ja/nej
Avvisade: <fynd>, <varför det inte håller>
Frågor: <beslut som behövs från användaren>
Inte granskat: <axel>, <varför>
```

Rätta bekräftade P0 till P2. P0 och P1 får ett test som faller före rättningen, P2 bara
om samma fel kommit tillbaka. Sedan steg 6 i `/arbetsflode`.

Ett fel som är värt ett test blir ett test, inte en rad här. Den här skillen ska vara
lika kort om ett år.
