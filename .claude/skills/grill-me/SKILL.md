---
name: grill-me
description: 'Grilla användaren om en plan, ett beslut, en idé eller ett kanban-kort tills ni förstår den på samma sätt. Use when the user says "grill" in any form ("grill me", "grilla mig", "/grill-me"), or brings a vague idea or feature without clear scope, behaviour, edge cases or a way to tell when it is done. Also step 2 in /arbetsflode. Not for a task that is already clearly specified.'
---

# Grill me

Fortsätt ställa frågor tills ni båda förstår planen på samma sätt. Skriv på svenska.

## Varje runda

1. Lista de öppna frågor du kan ställa nu, alltså de som inte hänger på ett svar du
   fortfarande väntar på. Ställ **högst 4 per runda**, först de vars svar öppnar eller
   stänger flest andra frågor. Resten väntar till nästa runda.
2. Varje fråga förklarar saken först: vad det är (en mening), hur det fungerar i appen
   idag, ett konkret exempel (Falk 1 tappar kontakten i 40 s, Havsörn skickar position i
   grader och minuter, 5 000 objekt på kartan) och alternativen. Använd aldrig ett filnamn eller en
   komponent som om användaren kände till den. Numrera och ge ditt förslag, formulerat
   så att "ja" godtar det:

   ❓ F1, <rubrik>: <frågan>

   ➡️ <föreslaget svar>

3. Vänta på svaren. Räkna ut vilka nya frågor svaren öppnat, och ställ nästa runda.

Slå upp fakta själv i koden, i `AGENTS.md`, i `docs/`, i kanban-kortet (`gh issue view <nr>`) eller
i dokumentationen. Fråga bara användaren om **beslut**.

## Frågor som ofta glöms i den här appen

Ta med de som berör planen, hoppa över resten:

- **Operatören:** vad ska synas direkt, och vad får ta ett klick? Hur ser det ut i mörker?
- **Gammal och saknad data:** vad händer om sensorn tystnar, skickar trasig data eller
  skickar meddelanden i fel ordning? Syns det utan att läsa färg?
- **Tomt läge:** inga sensorer anslutna än, backend nere, SignalR återansluter.
- **Spårbarhet:** hamnar det i händelseloggen? Kan man i efterhand se vad operatören såg?
- **Isolerat nät:** hämtar något från internet (typsnitt, kartor, paket vid körning)?
- **Nästa team:** måste Havsörn (eller ett tredje team) ändra något i kärnan? Ändras
  kontraktet?
- **Latens och skala:** håller det under 1 s, och med 5 000 objekt?
- **Beroenden:** behövs ett nytt paket, och går det att motivera för säkerhetsavdelningen?
- **Fiktivt:** inga riktiga förband, platser eller fordonsdata.

## Klart

Klart när inga öppna frågor finns kvar och inget antas utan att det sägs. Skriv då ner
vad ni kommit överens om under tre rubriker: **Det här gör vi** · **Det här gör vi inte**
· **Det här antar vi**. Högst 15 rader, så kort som möjligt. Finns en planfil: uppdatera
den och läs den från början till slut, en plan som ändrats i omgångar säger lätt emot
sig själv. Gör ingenting av planen förrän användaren bekräftat att ni är överens.
