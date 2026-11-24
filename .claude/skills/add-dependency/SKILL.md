---
name: add-dependency
description: Use before adding any NuGet or npm package to Nordljus. Checks whether the package is needed, records the justification in docs/dependencies.md and pins the version, so every dependency can be approved by a security review.
---

# Lägga till ett beroende

Alla beroenden ska kunna godkännas av en säkerhetsavdelning. Följ stegen i ordning.

## 1. Behövs paketet?

Fråga först:
- Finns det redan i .NET eller i webbläsaren? (t.ex. `System.Text.Json`, `fetch`, `crypto.randomUUID`, SignalR i ASP.NET Core)
- Går det att skriva på under ~50 rader själv?
- Täcker ett befintligt paket i `docs/dependencies.md` redan behovet?

Om svaret är ja på något: använd inte paketet. Säg vad du gör i stället.

## 2. Kontrollera paketet

- **Licens**: MIT, Apache-2.0, BSD eller liknande är okej. Kommersiell eller oklar licens: stoppa och fråga ägaren. (MediatR och AutoMapper är uteslutna av detta skäl.)
- **Underhåll**: senaste release inom ungefär ett år, aktivt repo.
- **Nätverk**: paketet får inte anropa internet vid körning (telemetri, CDN, externa API:er).
- **Transitiva beroenden**: nämn om paketet drar in många.

## 3. Installera med låst version

```bash
dotnet add src/<Projekt> package <Paket> --version <exakt version>
cd frontend && npm install --save-exact <paket>@<exakt version>
```

## 4. Dokumentera

Lägg till en rad i `docs/dependencies.md` (skapa filen med tabellhuvudet om den saknas):

```markdown
| Paket | Version | Var | Licens | Varför | Alternativ som valdes bort |
|---|---|---|---|---|---|
```

## 5. Rapportera

Berätta kort för ägaren vilket paket som lades till, varför och vad alternativet hade varit.
