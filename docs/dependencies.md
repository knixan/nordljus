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

Inga paket än. Frontend skapas i vecka 3.
