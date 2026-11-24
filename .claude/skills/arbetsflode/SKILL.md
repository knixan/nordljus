---
name: arbetsflode
description: 'Arbetsflödet för ett kanban-kort i Nordljus, i sex steg med en grind per steg: plan → samsyn (/grill-me) → bygg och kontroller → prova i appen (/granska-i-appen) → granska mot planen (/granska-mot-planen, startas av användaren) → kritisk granskning (/granska-kritiskt) och fler rundor tills användaren är nöjd. Use at the start of every task that changes the app. Triggers on "börja på #12", "bygg #12", "jobba på kortet", "nästa kort", "ny uppgift", "arbetsflödet", "/arbetsflode".'
---

# Arbetsflödet, sex steg med en grind per steg

Gäller varje kort som ändrar appen. Kör stegen i ordning och gå inte vidare förrän
stegets **grind** håller. Säg i chatten vilket steg du är på ("Steg 3 av 6: …").

## Hur stort är kortet?

| Storlek      | Exempel                                                          | Steg                                     |
| ------------ | ---------------------------------------------------------------- | ---------------------------------------- |
| **Städning** | stavfel, kommentar, beroende, konfig, CI, inget som syns         | bara kontrollerna i steg 3               |
| **Dokument** | problemformulering, discovery, ADR, README, workshop             | 1 (kort plan) → 2 → skriv → 6 (axel E)   |
| **Litet**    | en text, ett värde, en rättning i en fil, ingen ny logik         | 1 (kort plan, utan grill-me) → 3 → 4 → 6 |
| **Vanligt**  | allt annat                                                       | alla sex                                 |

Syns ändringen i appen (kartan, sensorstatus, larm, mörkt läge) är steg 4 alltid med.
Osäker? Välj den större. Säg vilken storlek du valt och varför.

## Steg 1, plan

1. **Läs kortet** (se `AGENTS.md` § Kanban) och flytta det till _In progress_. Är det ett
   draft-kort: gör om det till en issue först. En korttext är ett minne, inte ett facit:
   kontrollera påståendena mot koden.
2. **Är det redan gjort?** `git log --oneline --grep "#<nr>"` och sök i koden innan du
   planerar.
3. **Läs först:** `AGENTS.md` (struktur, domänregler), de filer kortet rör och den
   dokumentation som gäller (tabellen i `AGENTS.md`). Gissa aldrig ett API för SignalR,
   EF Core-migreringar, säkerhet eller Docker.
4. **Planera i plan mode**, gärna med högre effort eller en starkare modell än bygget.
   Planen är det billigaste stället att hitta ett fel på.
5. **Planen innehåller:**
   - **Kortversionen överst** (högst 12 rader): en mening om vad operatören (eller
     utvecklaren i ett annat team) märker, en rad per steg med filnamn, sedan _Senare_ /
     _Inte_.
   - **Läge-raden** (se nedan).
   - **Risker:** per punkt vad som kan gå sönder, vad du kontrollerat i koden och hur
     planen undviker det.
   - **Kraven:** påverkar det isolerat nät, latens under 1 s, spårbarhet, robusthet eller
     att kärnan är teamoberoende? Hur hålls varje krav?
   - **Kontraktet och adapters:** ändras `Nordljus.Contracts`? Behöver Havsörn ändra något?
   - **Beroenden:** nya paket, och raden i `docs/dependencies.md`.
   - **Tester:** vilka regler i `Nordljus.Core` och vilka adapters som får nya tester.
   - **Verifiering:** hur ni ser att det blev rätt (test, `docker compose up`, vad som ska
     synas på kartan, mätvärde).
6. **Riskgranska planen själv** innan du visar den: punkt för punkt, "vad går sönder om
   jag gör detta?".

**Läge-raden.** Överst i planfilen, under kortversionen, står en rad du uppdaterar vid
varje grind. En lång session sammanfattas och glömmer annars var den var:

```
Läge: steg 4 av 6 klart (provat i appen 2026-12-08). Nästa: /granska-mot-planen.
```

**Grind:** planfilen finns med kortversion och Läge-rad överst, och kortversionen står
också i chatten.

## Steg 2, samsyn

Kör `/grill-me` på planen. Uppdatera planfilen efter varje svar och läs den hela innan
du ber om ja.

**Grind:** inga öppna frågor, och användaren har godkänt planen (ExitPlanMode).

## Steg 3, bygg och kontrollera

Bygg enligt planen och reglerna i `AGENTS.md`. Visar det sig svårare än planen: tillbaka
till steg 1 i stället för att improvisera. Förklara kort vad du bygger och varför, i små
steg – användaren ska kunna förklara koden på en intervju.

```bash
dotnet build && dotnet test && dotnet format --verify-no-changes
cd frontend && npm run format:check && npm run typecheck && npm run lint && npm test
docker compose up --build   # vid ändringar i Dockerfile, compose, beroenden eller konfig
```

**Grind:** alla kontroller är gröna, och nya eller ändrade regler har tester.

## Steg 4, prova i appen. Hoppa aldrig över det när något syns

Gröna tester betyder inte att det blev rätt. Kör `/granska-i-appen`: starta allt, titta på
kartan som en operatör, stoppa en sensor, ta skärmbilder och visa dem. Avsluta med
**Prova själv** och vänta på svar.

Går appen inte att starta: säg det rakt ut. Då är steget **inte klart**. Skriv aldrig
"klart" eller "fungerar" utifrån tester och kodläsning.

**Grind:** skärmbilder av varje berört läge är öppnade och visade, inga nya fel i
konsolen, inga anrop till internet, och **användaren har själv öppnat appen och sagt att
det ser rätt ut**.

## Steg 5, granska mot planen

Bara användaren kan starta den, för att den är tung (två granskare i underagenter).
**Stanna** och skriv:

```
Steg 5 av 6: skriv /granska-mot-planen när du vill köra granskningen (tung, några minuter).
```

Starta den aldrig själv, och gå inte vidare utan den, utom när användaren säger att
steget ska hoppas över.

**Grind:** inga "saknas" eller "fel gjort" kvar på Planen-axeln. Det som medvetet inte
gjorts står i sammanfattningen.

## Steg 6, kritisk granskning och fler rundor

1. Kör `/granska-kritiskt`.
2. **Pröva varje fynd innan du rättar:** reproducera, hitta orsaken, rätta. P0 och P1 får
   ett test som faller före rättningen. Ett fynd som inte håller redovisas med skälet.
3. **Leta samma orsak på syskonställena** (andra adaptern, andra transporten, backend och
   frontend).
4. **Byt vinkel varje runda.** Från andra rundan: `/code-review medium`, en läsare som
   inte skrev koden.
5. **Stoppregel:** en runda med 0 bekräftade P0 till P2 är klar. Högst 3 rundor; hittar
   runda 3 fortfarande P0 till P2, stanna och rapportera.
6. Kör om steg 4 efter varje rättning som syns i appen, med en ny _Prova själv_.

**Grind:** användaren säger att hen är nöjd, och kontrollerna i steg 3 är gröna.

## Sist

Följ "Klart betyder" i `AGENTS.md`: ADR om ett arkitekturbeslut togs, kanban uppdaterad
(med användarens ja), ett commit-meddelande på svenska med `Closes #<nr>` eller
`Del av #<nr>`, och ett "Lär dig"-block om kortet var större än en småfix. Committa och
pusha aldrig själv.

## Lärdomar, oftast ingen text alls

Ett fel som är värt ett test blir ett **test** bredvid koden, inte en rad i en regelfil.
Ställ tre frågor i ordning:

1. **Händer det igen på ett annat kort?** Nej → lärdomen stannar i commit-meddelandet.
2. **Kan en dator kontrollera det?** Ja → ett test i `tests/` eller `frontend/src/**/*.test.ts`.
3. **Annars** → föreslå en rad till `AGENTS.md` och skriv in den bara om användaren
   säger ja. Korta hellre en gammal rad än lägg till en ny.

Den här skillen ändras inte av ett enskilt kort.
