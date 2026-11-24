# Problemformulering

_Till team Nordljus och Common Tech. Version 1, vecka 1._

## Nuläget

Operatörerna i Nordljus följer personal (plutonen Örn), markfordon (Järn) och helikoptrar (Falk) i en skrivbordsapplikation. Sensorernas data samlas i en fil som programmet läser in var 30:e sekund. Nya versioner levereras genom att någon kopierar filer till en server för hand.

## Problemet

**Operatörerna fattar beslut på en lägesbild som kan vara 30 sekunder gammal. De kan inte heller se skillnad på en enhet som står still och en sensor som har slutat rapportera.**

Det får fyra följder:

1. **Bilden ligger efter.** En helikopter kan hinna flytta sig mer än en kilometer mellan två inläsningar.
2. **Fel syns inte.** När en sensor tystnar ligger den senaste positionen kvar och ser lika färsk ut som de andra.
3. **Det går inte att följa upp.** Ingen kan i efterhand säga vad operatören såg och när, eller vilka meddelanden som var trasiga.
4. **Varje team bygger eget.** Ett annat team, till exempel Havsörn, med ett annat dataformat måste börja om från början.

## För vem

| Vem                                   | Behöver                                                                       | Förlorar i dag                                          |
| ------------------------------------- | ----------------------------------------------------------------------------- | ------------------------------------------------------- |
| **Operatörer** i Nordljus             | Se var alla är _just nu_ och direkt se vilka sensorer som inte går att lita på | Tid och säkerhet i besluten. Gammal data ser färsk ut.  |
| **Utvecklare** i Nordljus             | Kunna leverera, testa och felsöka utan handpåläggning                         | Tid på manuell leverans. Ingen logg att felsöka i.      |
| **Common Tech** (gemensam teknikavd.) | En lösning som fler team kan använda, med få beroenden och som körs helt lokalt | Samma problem löses om och om igen i varje team.        |

## Hur vi vet att det är löst

| Kriterium            | Mått                                                                                                                 | Hur vi mäter                                                                          | När   |
| -------------------- | -------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- | ----- |
| **Aktuell bild**     | 95 % av uppdateringarna når skärmen på under 1 sekund                                                                | Tidsstämpel när sensorn skickar jämförs med när markören ritas om, med 200 objekt     | V3    |
| **Håller i skala**   | Samma krav med 5 000 objekt                                                                                          | Samma mätning med simulatorn uppskruvad                                               | V5    |
| **Tappad syns**      | En sensor som inte rapporterat på 5 s visas som _Gammal_, efter 30 s som _Tappad_, och det syns utan att man ser färg | Simulatorn stänger av en sensor (t.ex. Falk 1). Vi tittar i både mörkt och ljust läge | V3    |
| **Spårbart**         | Alla mottagna meddelanden, även trasiga, finns i loggen med tid, källa, rådata och resultat. Inget ändras eller raderas | Antal skickade jämförs med antal loggade. Simulatorn skickar trasig data med flit     | V3    |
| **Återanvändbart**   | Havsörn kommer igång genom att skriva en egen översättare för sitt dataformat (en adapter), utan att ändra en enda rad i den gemensamma kärnan              | Ändringarna för Havsörn granskas: inga filer utanför deras egen del                   | V4    |
| **Enkelt att köra**  | Allt startar med ett kommando. När det väl är byggt hämtar det inget från internet                                   | Start med nätverket avstängt, och kontroll att webbläsaren inte gör några anrop utåt  | V3    |

Tiderna 5 och 30 sekunder är startvärden som går att ändra i konfigurationen. De bestäms inte av koden.

## Avgränsningar

- Det här dokumentet beskriver problemet. Teknikval och arkitektur beskrivs i egna dokument och beslut (ADR:er).
- Riktig militär symbolik, riktiga förband och riktiga platser ingår inte. All data är fiktiv.
- Inloggning och behörigheter ingår inte under de sex veckorna.

## Antaganden att pröva

De här antagandena prövas i samtal med operatörer och utvecklare ([discovery.md](discovery.md)):

- Sensorerna rapporterar ungefär en gång per sekund.
- Nätet är isolerat och allt körs lokalt.
- 5 och 30 sekunder är rimliga gränser för _Gammal_ och _Tappad_.
