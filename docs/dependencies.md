# Beroenden

Varje NuGet- och npm-paket i Nordljus står här, så att säkerhetsavdelningen kan godkänna det. Versionerna är låsta: NuGet i `Directory.Packages.props`, npm med `--save-exact`. Ett nytt paket läggs till enligt skillen `add-dependency`.

Inget paket får anropa internet när systemet körs.

## NuGet

| Paket             | Version | Var                     | Licens     | Varför                                                                                                      | Alternativ som valdes bort                                                                                                         |
| ----------------- | ------- | ----------------------- | ---------- | ----------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `xunit.v3.mtp-v2` | 4.0.3   | `tests/` (bara test)    | Apache-2.0 | Testramverket. Ett paket räcker: det kör testerna via Microsoft Testing Platform, som `dotnet test` i .NET 10 använder. Drar in 15 paket transitivt, bland annat `Microsoft.Testing.Platform` 2.5.2, `Microsoft.Testing.Extensions.Telemetry` 2.5.2 och `Microsoft.ApplicationInsights` 2.23.0 – **telemetri från testkörningen, se nedan**. | Mallens xUnit v2 med VSTest (fyra paket: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`). MSTest hade fungerat, men projektet har valt xUnit. |

### Telemetri från testerna

Microsoft Testing Platform skickar användningsdata (bl.a. hashat programnamn, antal tester, OS och .NET-version) till Microsoft när testerna körs. Det gäller bara `dotnet test` – inget av detta finns i det som körs i drift. Det går inte att stänga av i projektfilen, bara med en miljövariabel:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1   # stänger av både .NET CLI:s och testplattformens telemetri
```

Sätt den på utvecklardatorer, i CI och i Docker-byggen. Källa: [Microsoft Testing Platform telemetry](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-telemetry).

## npm

Installerade med `--save-exact` i `frontend/package.json`. Totalt 194 paket i `node_modules` på Windows (221 i låsfilen, som också har med paket för andra plattformar). Bara 21 av dem används i drift; resten är byggverktyg som aldrig når webbläsaren. Inget paket kör något installskript, utom `fsevents` som bara installeras på macOS. `npm audit`: 0 sårbarheter (2026-11-24).

### I drift (hamnar i webbläsaren)

| Paket                | Version | Licens | Varför                                                                                                                                                                                                                                                                       | Alternativ som valdes bort                                                                                                      |
| -------------------- | ------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| `react`              | 19.3.1  | MIT    | UI-ramverket, valt i arkitekturen. Inga egna beroenden.                                                                                                                                                                                                                      | –                                                                                                                               |
| `react-dom`          | 19.3.1  | MIT    | Ritar React i webbläsaren. Drar in `scheduler`.                                                                                                                                                                                                                              | –                                                                                                                               |
| `@microsoft/signalr` | 10.0.11 | MIT    | Klienten till SignalR-hubben: förhandling, WebSocket, återanslutning. Version 10 för att matcha servern (.NET 10). Drar in `ws`, `node-fetch`, `eventsource`, `fetch-cookie` och `abort-controller`, men **bara för Node** – i webbläsarbygget finns bara namnen i en `isNode`-gren, ingen av koden (kontrollerat i `dist/` 2026-11-24). | Egen WebSocket-klient: protokollet, återanslutning och förhandling för hand – mer kod att underhålla än paketet sparar. 11.0.0 finns, men hör till .NET 11. |

### Bara i utveckling och bygge

| Paket                       | Version | Licens     | Varför                                                                                                                                                         | Alternativ som valdes bort                                                                            |
| --------------------------- | ------- | ---------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `vite`                      | 8.3.8   | MIT        | Dev-server och bygge. Ger CSS Modules och proxy till backend utan fler paket.                                                                                  | –                                                                                                     |
| `@vitejs/plugin-react`      | 6.3.0   | MIT        | JSX och snabb omladdning i Vite.                                                                                                                               | –                                                                                                     |
| `typescript`                | 6.0.3   | Apache-2.0 | Typkontroll (`npm run typecheck`). **Inte 7.x:** typescript-eslint 8.74 kräver TypeScript `<6.1`. Byt när typescript-eslint stöder 7.                        | 7.0.2 (Go-kompilatorn) – inte kompatibel med lintningen än.                                           |
| `@types/react`              | 19.3.0  | MIT        | Typer för React.                                                                                                                                               | –                                                                                                     |
| `@types/react-dom`          | 19.3.0  | MIT        | Typer för react-dom.                                                                                                                                           | –                                                                                                     |
| `vitest`                    | 5.1.1   | MIT        | Testramverket för frontend, använder Vites konfiguration.                                                                                                      | Jest – kräver egen transform för TypeScript och CSS Modules.                                         |
| `eslint`                    | 10.15.0 | MIT        | Lintning (`npm run lint`, och hooken som lintar ändrade filer).                                                                                                | –                                                                                                     |
| `@eslint/js`                | 10.0.1  | MIT        | ESLints rekommenderade regler.                                                                                                                                 | –                                                                                                     |
| `typescript-eslint`         | 8.74.0  | MIT        | ESLint för TypeScript, med `strict`-reglerna (bl.a. ingen `any`).                                                                                              | –                                                                                                     |
| `eslint-plugin-react-hooks` | 7.1.1   | MIT        | Fångar fel i hooks (beroendelistor, regler för hooks).                                                                                                         | –                                                                                                     |
| `prettier`                  | 3.10.1  | MIT        | Formatering (`npm run format:check`, och hooken).                                                                                                              | –                                                                                                     |

Bortvalt ur Vite-mallen: `eslint-plugin-react-refresh` (bara en varning i dev) och `globals` (behövs inte med TypeScript). Inget `jsdom` eller Testing Library: logiken ligger i rena funktioner som testas utan DOM.
