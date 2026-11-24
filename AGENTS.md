# Regler för AI-agenter i det här repot

Skriv på **svenska**: UI-texter, kommentarer, loggmeddelanden som operatören kan se, felmeddelanden, kanban-kort, ADR:er och commit-meddelanden. Namn på klasser, metoder, variabler, komponenter och filer är på engelska. Testnamn får vara på svenska (`Trasig_position_loggas_och_kastas_inte`).

## Vad är Nordljus?

En **lägesbild i realtid** för ett påhittat militärt produktteam, Nordljus. Sensorer rapporterar position och status för personal, markfordon och helikoptrar, och operatörer följer dem live på en karta. I dag ser operatörerna en CSV-fil som läses in var 30:e sekund – bilden ligger alltid efter.

Projektet är ett portfolio-case för rollen **Forward Deployed Engineer** (mål: Saab, Linköping). Det bedöms lika mycket på _hur_ det är byggt som på vad det gör: att problemet är förstått, att lösningen går att återanvända av fler team, att den levereras med `docker compose up` och att teamet kan ta över (README, ADR:er, workshop). Användaren ska kunna förklara varje del på en intervju.

Upplägget är sex veckor: förstå (V1), bygga (V2–3), göra återanvändbart med team **Havsörn** (V4), avvikelsedetektering och prestanda (V5), lyfta teamet (V6). Planen ligger på kanban-tavlan (se § Kanban).

| Del         | Teknik                                                                    |
| ----------- | ------------------------------------------------------------------------- |
| Backend     | C# / .NET 10, ASP.NET Core minimal API                                    |
| Realtid     | SignalR (ingår i ASP.NET Core)                                            |
| Ingest      | `BackgroundService` som lyssnar på UDP (Nordljus) och TCP (Havsörn)      |
| Databas     | SQL Server via EF Core – append-only händelselogg                         |
| Loggning    | Serilog, strukturerad JSON                                                |
| Test        | xUnit (backend), Vitest (frontend)                                        |
| Simulator   | .NET-konsolapp                                                            |
| Frontend    | React + TypeScript + Vite, `@microsoft/signalr`                           |
| Karta       | MapLibre GL + lokal PMTiles-fil (eller eget koordinatsystem på Canvas)    |
| Drift       | Docker Compose, GitHub Actions, Dependabot                                |

Paketen installeras när kortet som behöver dem påbörjas, aldrig i förväg (se § Beroenden).

### Struktur

```
Nordljus.slnx
src/
  Nordljus.Contracts/          kontraktet: TrackUpdate, SensorStatus, TrackType – inga beroenden
  Nordljus.Core/               regler utan I/O: validering, staleness, avvikelser + tester
  Nordljus.Backend/            API, SignalR-hubb, ingest, händelselogg (EF Core)
  Nordljus.Simulator/          fejkade sensorer – tappar kontakt, skickar trasig data
  Adapters/
    Nordljus.Adapter.Nordljus/ råformat → TrackUpdate
    Nordljus.Adapter.Havsorn/  råformat → TrackUpdate
tests/                         ett xUnit-projekt per src-projekt (Nordljus.Core.Tests …)
frontend/
  src/situation-map/           delas mellan teamen – får inte importera från app/
    components/                kartan, markörer, statusmärken
    hooks/                     useSituation() m.fl.
    index.ts                   publikt API – andra importerar bara härifrån
  src/app/                     appen för Nordljus (och en vy för Havsörn)
    pages/                     en fil per vy – sätter ihop komponenter
    components/                komponenter som bara appen använder
docs/                          problem, discovery, arkitektur, adr/, dependencies.md, workshop/
docker-compose.yml
```

Mappar som inte finns än skapas när första filen behövs, enligt strukturen ovan.

### Gör så här

- **Kärnan känner bara till kontraktet.** `Nordljus.Core` och `Nordljus.Backend` refererar till `Nordljus.Contracts`, aldrig till en adapter. Råformat (JSON från Nordljus, radformat från Havsörn) tolkas bara i teamets adapter. Ett nytt team kommer igång genom att skriva en adapter – se skillen `add-sensor-adapter`.
- **Kontraktet är records och versioneras.** Ändras ett fält som en adapter redan använder → ny version eller ett nytt valfritt fält, och en ADR. Döp aldrig om eller ta bort ett fält i tysthet.
- **Regler utan I/O** (validering, staleness Live/Gammal/Tappad, avvikelser) → `Nordljus.Core`, med tester. De får aldrig använda `DateTime.Now`, databas, nätverk eller filer – tid kommer in som `TimeProvider`, så att testerna kan styra klockan.
- **Validering sker vid gränsen.** Adaptern returnerar ett giltigt `TrackUpdate` eller ett valideringsfel med orsak. Den kastar aldrig ett undantag för trasig data.
- **Ingest** är en `BackgroundService` per transport. Den tar emot, kör adaptern, skriver till händelseloggen och skickar vidare. Den får aldrig krascha av ett trasigt meddelande.
- **SignalR-hubben är tunn.** Den skickar ut, den räknar inte. Batcha uppdateringar per tick i stället för ett meddelande per objekt (krävs för 5 000 objekt).
- **Minimal API**, inga controllers. Endpoints grupperas per område i egna filer (`SituationEndpoints.cs`).
- **Tider i UTC** (`DateTimeOffset`). Lokal tid först när den visas i UI:t.
- **C#:** nullable på, `TreatWarningsAsErrors`, file-scoped namespaces, primary constructors där det blir tydligare. Inga `static` hjälpklasser med tillstånd.
- **Frontend:** TypeScript `strict`, ingen `any`. Funktionskomponenter och hooks. Komponenter i kebab-case (`track-marker.tsx` → `TrackMarker`). Det som ska delas med Havsörn ligger i `src/situation-map/`, exporteras från dess `index.ts` och får inte importera från `src/app/`. Sidor ligger i `pages/` och sätter ihop komponenter; komponenter ligger i `components/` och vet inte vilken sida de används på. Testet ligger bredvid filen (`track-marker.test.tsx`). Se `docs/architecture.md` § Frontend.
- **Håll det enkelt.** Ingen abstraktion "för framtiden" utöver adapter-mönstret, som är själva poängen.

### UI

- **Mörkt läge är standard** – operatörer jobbar i mörker. Ljust läge finns som val. Färger kommer bara från CSS-variabler i temat (`--color-…` i `frontend/src/theme.css`), aldrig hexkoder i komponenter.
- **Kontrast minst 4,5:1** för text och 3:1 för ikoner och linjer mot kartan (WCAG AA). Räkna kontrasten när du byter färger.
- **Status bärs aldrig av färg enbart.** Live / Gammal / Tappad visas också med ikon, form eller text och med åldern ("12 s sedan"). En tappad sensor ska synas även för den som är färgblind.
- **Gammal data får aldrig se färsk ut.** Markören tonas ner, får en varningsram och visar ålder. Datan får inte bara frysa.
- Förenklad NATO-symbolik (APP-6) med blå ram för egna enheter. Ingen riktig militär symbolik i detalj.

## Domänregler

- **Isolerat nät.** Inget får hämtas från internet vid körning: inga CDN:er, inga externa API:er, inga karttjänster på nätet, inga Google Fonts, ingen telemetri. Typsnitt, ikoner och kartdata ligger i repot eller i containern. `frontend/index.html` laddar bara lokala filer.
- **Under 1 sekund** från sensor till skärm. Ändringar i ingest, hubb eller rendering mäts mot det kravet.
- **Spårbarhet.** Varje mottaget meddelande – även trasigt – skrivs i händelseloggen med mottagningstid, källa, rådata och resultat. Det ska gå att i efterhand se vad operatören såg och när. Rader i händelseloggen **uppdateras och raderas aldrig**.
- **Robusthet.** En sensor som slutar rapportera går Live → Gammal → Tappad efter tider som står i konfigurationen, inte hårdkodat. Backend talar om när statusen byts, frontend räknar inte ut den själv.
- **All data är fiktiv.** Inga riktiga förband, koordinater för militära platser, vapensystem eller prestandadata för verkliga fordon. Plutonen heter Örn, fordonen Järn och Falk.
- **Avvikelsedetektering ska gå att förklara.** Regler och statistik först (omöjliga hopp, hastighet över max för typen, sensor som fryser). Varje larm säger _varför_. Inga moln-API:er och ingen LLM.
- **Fel läcker aldrig ut.** API:t svarar med `ProblemDetails` utan stacktrace. Undantag loggas med Serilog.
- **Hemligheter** (anslutningssträngar, lösenord) ligger i `.env` (gitignore) och i `docker-compose.yml` som variabler – aldrig i `appsettings.json` eller koden. Nya variabler läggs till i `.env.example` och README.

## Beroenden

Säkerhetsavdelningen ska kunna godkänna varje paket. Innan ett NuGet- eller npm-paket läggs till: följ skillen `add-dependency` – går det utan paketet? licens, underhåll, inget nätverk vid körning – och skriv en rad i `docs/dependencies.md`. Lås exakta versioner (`--save-exact`). MediatR, AutoMapper, Redux, UI-komponentbibliotek och CSS-ramverk används inte.

Ett paket som kräver byggskript (`postinstall`) eller drar in många transitiva beroenden: säg till användaren i stället för att lägga till det.

## Dokumentation – gissa inte

Versionerna är nyare än det du lärt dig. Slå upp i dokumentationen innan du använder ett API du är osäker på, och **alltid** när det gäller SignalR, EF Core-migreringar, säkerhet och Docker.

- **`llms.txt` är en innehållsförteckning.** Hämta den, hitta rätt sida och hämta bara den. Hämta aldrig `llms-full.txt`.
- **Kolla versionen.** Stämmer dokumentationen inte med versionen i projektet – leta efter en versionerad sida eller säg till.
- Versionerna fylls i när paketet installeras.

| Bibliotek      | Version  | Dokumentation                                                |
| -------------- | -------- | ------------------------------------------------------------ |
| .NET / ASP.NET | 10.0.201 | https://learn.microsoft.com/aspnet/core/                     |
| SignalR        | 10 (JS-klient 10.0.11) | https://learn.microsoft.com/aspnet/core/signalr/introduction |
| EF Core        | –        | https://learn.microsoft.com/ef/core/                         |
| Serilog        | –        | https://github.com/serilog/serilog-aspnetcore                |
| xUnit v3       | 4.0.3    | https://xunit.net/docs/getting-started/v3/getting-started    |
| React          | 19.3.1   | https://react.dev/llms.txt                                   |
| Vite           | 8.3.8    | https://vite.dev/llms.txt                                    |
| Vitest         | 5.1.1    | https://vitest.dev/llms.txt                                  |
| MapLibre GL JS | –        | https://maplibre.org/maplibre-gl-js/docs/                    |
| PMTiles        | –        | https://docs.protomaps.com/                                  |
| Docker Compose | –        | https://docs.docker.com/compose/                             |
| GitHub Actions | –        | https://docs.github.com/actions                              |

## Kommandon

```bash
docker compose up --build                    # allt: simulator, backend, frontend, SQL Server
dotnet build                                 # varningar = fel
dotnet test
dotnet format --verify-no-changes            # format:check för C#
dotnet ef migrations add <Namn> -p src/Nordljus.Backend

cd frontend
npm run dev                                  # http://localhost:5173
npm run typecheck
npm run lint
npm test
npm run format:check
npm run build
```

## Klart betyder

1. `dotnet build`, `dotnet test` och `dotnet format --verify-no-changes` går igenom. I `frontend/`: `npm run format:check`, `npm run typecheck`, `npm run lint`, `npm test`. Filer du ändrar formateras av hooken.
2. `docker compose up --build` startar fortfarande allt, om du ändrat en Dockerfile, `docker-compose.yml`, beroenden eller konfiguration.
3. Nya eller ändrade regler (validering, staleness, avvikelser, adapters) har tester.
4. Ändringar i UI: säg tydligt vad du **inte** har kunnat testa (t.ex. 5 000 objekt på en svag dator, mörker på riktigt).
5. Ett arkitekturbeslut har en ADR (skillen `write-adr`). Ett nytt paket står i `docs/dependencies.md`.
6. Kanban är uppdaterad (se nedan).
7. Du har skrivit ett commit-meddelande.
8. Du har avslutat med ett "Lär dig"-block (se nedan), om uppgiften var större än en småfix.

## Lärande

Användaren vill fräscha upp och fördjupa sina kunskaper medan hen bygger – inte bara få koden skriven – och förbereda sig för intervjuer på Saab. Avsluta därför varje **större** uppgift (ny funktion, buggfix med en lärdom, säkerhet, databas, arkitektur, drift) med ett kort block, **efter** sammanfattning och commit-meddelande:

```
**Lär dig: <ämne>**
**Fakta:** en sak från koden du just skrev, med länk till filen/raden.
**Varför:** varför man gör så – och vad som händer om man inte gör det.
**Frågor:**
1. en fråga om just den här koden (förstå, förutsäga eller hitta i koden),
2. en fråga till – gärna en vanlig intervjufråga inom samma ämne.
```

- **Max ca 10 rader och 2 frågor.** Hoppa över för småfix (stavfel, formatering, kanban, konfig utan lärdom).
- **Utgå från det som just byggdes.** Rotera ämnen: C#, ASP.NET Core (DI, middleware, minimal API, `BackgroundService`), SignalR och WebSockets, EF Core och SQL Server, React och TypeScript, testning, Docker och CI, säkerhet (DevSecOps, beroenden, isolerade nät), arkitektur (kontrakt, adapters, ADR:er) och FDE-frågor (hur man förstår en kunds problem, lär upp ett team, gör något återanvändbart).
- **Avslöja inte svaren.** Vänta på användarens svar, rätta kort (rätt/delvis/fel + det viktigaste i 2–4 meningar). Säger hen "vet inte": ledtråd först.
- **Logga** varje block i `.claude/larande/logg.md` (privat, gitignore) i samma format som de som redan finns, och uppdatera status när användaren har svarat (`ny` → `rätt`/`delvis`/`fel` + datum). Frågor med `fel` tas upp igen i ett senare block eller i `/quiz`.
- **Nivå:** användaren kan bygga projekt och förstår helheten, har mest jobbat med TypeScript/React/Next.js och är ny på mer avancerad .NET-backend. Svarar hen lätt rätt – gå djupare (avvägningar, alternativ). Svarar hen fel – enklare och mer konkret.
- Användaren styr: "hoppa över" hoppar över ett block, "pausa lärandet" stänger av blocken tills hen säger till, "mer om det här" ger en fördjupning. `/quiz` ger 5 repetitionsfrågor ur loggen.

## Git

- **Committa och pusha aldrig.** Användaren committar själv. En hook stoppar `git commit` och `git push`.
- Avsluta med ett commit-meddelande på svenska: kort rubrik, tom rad, punktlista. Sist: `Co-Authored-By`-raden från systemet.
- Jobbet sker direkt på `main`.
- **CI** (`.github/workflows/ci.yml`) körs vid push och PR mot `main`: samma kontroller som "Klart betyder" punkt 1, bygge av Docker-images och en SBOM. Det som går igenom lokalt ska gå igenom i CI.
- **Dependabot** (`.github/dependabot.yml`) föreslår uppdateringar för NuGet, npm, Docker och Actions varje vecka.

## Databas

- SQL Server körs i Docker (`mcr.microsoft.com/mssql/server`, Developer-utgåvan). Anslutningssträngen kommer från miljövariabel.
- Schemat ändras bara via EF Core-migreringar i `src/Nordljus.Backend/Migrations/`. Backend kör migreringarna vid start i utvecklingsläge.
- **Händelseloggen är append-only.** Ingen migrering får ändra eller radera befintliga rader i den. Nya kolumner är nullable eller har default.
- **Idag finns bara testdata** – databasen får nollställas med `docker compose down -v`. Säg ändå till innan du gör det.

## Kanban

GitHub Project 24 (`knixan`, "Nordljus"): https://github.com/users/knixan/projects/24/views/1. Repo `knixan/nordljus`.

- Korten heter `[V1]`–`[V6]` efter vecka och `[Bonus]` för det som görs sist. Prioritet P0 = krävs för "Klart när" i uppdraget, P1 = bör, P2 = bonus. Bonuskort görs först när alla P0-kort är klara.
- Korten är i dag **draft-kort** på tavlan. När ett kort påbörjas görs det om till en issue i repot (`gh project item-edit` kan inte det – använd "Convert to issue" eller `gh api graphql` med `convertProjectV2DraftIssueItemToIssue`), så att commit-meddelandet kan säga `Closes #<nr>`.
- **Börja en uppgift:** ber användaren dig jobba på ett kort – läs det och flytta kortet till _In progress_.
- **När du blir klar:** flytta kortet till _Done_ om allt i checklistan är gjort, annars låt det stå kvar och skriv en kommentar om vad som återstår.
- **Commit-meddelandet** nämner issuen: `Closes #<nr>` om kortet är klart, annars `Del av #<nr>`.
- Skriv bodies i en fil och använd `--body-file` (långa heredocs i bash går sönder).
- Flytta inte kort, skapa inte kort och stäng inte issues utan att användaren har bett om kanban-uppdatering eller att det ingår i uppgiften. Säg vilka kort du har ändrat.
- `/kanban` (`.claude/skills/kanban`) visar tavlan och hjälper till att flytta och skapa kort.

| Vad         | ID                                                                                                                |
| ----------- | ----------------------------------------------------------------------------------------------------------------- |
| Projekt     | `PVT_kwHODHG13M4Bs_i7`                                                                                            |
| Status-fält | `PVTSSF_lAHODHG13M4Bs_i7zhrLDmA` – Backlog `f75ad846`, Later `61e4505c`, In progress `47fc9ee4`, Done `98236657` |
| Prioritet   | `PVTSSF_lAHODHG13M4Bs_i7zhrLDuU` – P0 `79628723`, P1 `0a877460`, P2 `da944a9c`                                   |
| Storlek     | `PVTSSF_lAHODHG13M4Bs_i7zhrLDuY` – XS `6c6483d2`, S `f784b110`, M `7515a9f1`, L `817d0097`, XL `db339eb2`        |

```bash
# Lista korten (id = PVTI_… används för att byta status)
gh project item-list 24 --owner knixan --format json --limit 200

# Byt status
gh project item-edit --project-id PVT_kwHODHG13M4Bs_i7 --id <item-id> \
  --field-id PVTSSF_lAHODHG13M4Bs_i7zhrLDmA --single-select-option-id 47fc9ee4

# Nytt kort som issue
gh issue create -R knixan/nordljus --title "[V2] Titel" --body-file kort.md
gh project item-add 24 --owner knixan --url <issue-url> --format json --jq .id
```

Nya issues läggs **inte** till i projektet automatiskt – kör alltid `gh project item-add`.
