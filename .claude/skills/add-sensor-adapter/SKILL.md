---
name: add-sensor-adapter
description: Use when a new team or sensor format (e.g. Havsörn) should feed the Nordljus situation picture. Creates an ISensorAdapter project that maps the raw format to the shared TrackUpdate contract without changing the core.
---

# Lägga till en sensoradapter

Målet är att bevisa att ett nytt team kommer igång **utan att röra kärnan**. Om du märker att kärnan måste ändras: stanna och förklara varför för ägaren, eftersom det i så fall är kontraktet som behöver ses över (och det kräver en ADR).

## Vad som får ändras

- ✅ Ett nytt projekt `src/Adapters/Nordljus.Adapter.<Team>/`
- ✅ Ett nytt testprojekt `tests/Nordljus.Adapter.<Team>.Tests/`
- ✅ Konfiguration (appsettings / docker-compose) som registrerar adaptern
- ✅ Ev. en simulator för teamets format
- ❌ `Nordljus.Contracts`, `Nordljus.Core`, `Nordljus.Backend` och `frontend/src/situation-map/`

## Steg

1. **Förstå formatet.** Be om (eller hitta på, om det är fiktivt) ett exempel på rådata och transport (UDP, TCP, fil). Skriv ner fälten och deras enheter.
2. **Skapa projektet** och referera bara till `Nordljus.Contracts`.
3. **Implementera `ISensorAdapter`**: läs rådata och returnera `TrackUpdate`. Konvertera enheter (t.ex. knop → m/s, grader/minuter → decimalgrader) i adaptern.
4. **Hantera trasig data**: returnera ett valideringsfel med orsak, kasta inte. Trasig data ska loggas i händelseloggen av kärnan.
5. **Tester**: minst ett giltigt meddelande, ett trasigt och ett med orimliga värden.
6. **Registrera adaptern** via konfiguration.
7. **Verifiera**: `git diff --stat` ska inte visa ändringar i kärnprojekten. Kör `dotnet test` och `docker compose up --build`.
8. **Dokumentera** formatet kort i `docs/adapters/<team>.md`.
