# Discovery – frågor till operatörer och utvecklare

_Till team Nordljus och Common Tech. Version 1, vecka 1._

Innan vi bygger något vill vi förstå hur operatörerna jobbar och hur utvecklarna levererar i dag. Här står tio frågor – fem till operatörer och fem till utvecklare – och vad svaren betyder för lösningen. Frågorna prövar också antagandena i [problem.md](problem.md).

> **Om svaren:** Nordljus är ett övningscase, så svaren nedan kommer inte från riktiga intervjuer. De är rimliga antaganden och står som _antagna_. Med ett riktigt team ställs frågorna i samtal, helst vid operatörens skärm, och svaren skrivs in här.

## Frågor till operatörer

### O1. Vad måste du se direkt när du tittar på skärmen?

**Varför vi frågar:** Det som syns utan att man klickar bestämmer hur kartan ritas.

**Svar (antaget):** Var våra egna enheter är just nu, och vilka jag inte kan lita på. Falk först, eftersom helikoptrarna rör sig snabbast. Namn och typ behöver jag se, resten kan jag klicka fram.

**Det betyder:** Varje markör visar typ, namn och status (Live/Gammal/Tappad) direkt på kartan. Detaljer som hastighet och sensor-id ligger bakom ett klick.

### O2. Hur märker du i dag att en sensor har slutat rapportera?

**Varför vi frågar:** Det är kärnan i problemet. Vi vill veta hur det går till i dag, inte hur det borde gå till.

**Svar (antaget):** Oftast märker jag det inte. Markören ligger kvar och ser ut som de andra. Jag märker det när någon anropar på radio, eller när en enhet inte har rört sig på länge – men det kan lika gärna betyda att den står still. När jag väl ser det anropar jag enheten och frågar var den är.

**Det betyder:** Backend avgör statusen och talar om när den byts. En tappad enhet ligger kvar på sin senaste position, tonas ner och visar ikon, text och ålder ("Tappad, 42 s sedan"), så att operatören kan säga "senast sedd där, för 42 sekunder sedan" på radion.

### O3. Hur gammal får en position vara innan du slutar lita på den?

**Varför vi frågar:** Gränserna för _Gammal_ och _Tappad_ ska komma från användarna, inte från koden.

**Svar (antaget):** Det beror på vad det är. En helikopter som inte hörts av på fem sekunder kan redan vara flera hundra meter bort. Personal till fots kan stå still i en minut utan att det är konstigt. Men en gräns för allt är lättare att lära sig.

**Det betyder:** 5 och 30 sekunder är rimliga startvärden. De ligger i konfigurationen. Om gränserna ska skilja sig mellan typerna är en öppen fråga (se nedan).

### O4. Vad behöver du kunna visa i efterhand?

**Varför vi frågar:** Spårbarhet är ett krav, men vi behöver veta vem som frågar efter den och varför.

**Svar (antaget):** Efter en övning går vi igenom vad som hände. Då frågar någon "varför skickade du inte Järn 2 dit?" och jag kan inte visa vad jag såg just då. Och när en sensor har visat fel position vill teknikerna veta vad den faktiskt skickade.

**Det betyder:** Varje mottaget meddelande, även trasiga, skrivs i händelseloggen med mottagningstid, källa, rådata och resultat. Raderna ändras och raderas aldrig. Uppspelning av en övning ingår inte under de sex veckorna, men loggen gör den möjlig senare.

### O5. Hur ser din arbetsplats ut, och hur många enheter följer du?

**Varför vi frågar:** Ljus, skärmar och antal objekt styr både utseendet och prestandakraven.

**Svar (antaget):** Ett mörkt rum, ofta nattskift, två skärmar. I vanliga fall runt 200 enheter. Vid samövningar med andra team kan det bli flera tusen. Ibland sitter jag i en lokal med dagsljus, och då syns ett mörkt tema dåligt.

**Det betyder:** Mörkt läge är standard och ljust läge finns som val. Kontrasten räknas mot WCAG AA. Status får inte bäras av färg enbart, eftersom färger är svåra att skilja i mörker. Lösningen byggs för 200 objekt och mäts med 5 000 i vecka 5.

## Frågor till utvecklare

### U1. Hur ser datan från sensorerna ut, och vad händer med trasiga meddelanden?

**Varför vi frågar:** Formatet och frekvensen avgör hur vi tar emot datan och vad adaptern ska klara av.

**Svar (antaget):** Sensorerna skickar ett JSON-meddelande ungefär en gång per sekund över UDP till en insamlingstjänst. Tjänsten skriver om CSV-filen var 30:e sekund och sparar bara den senaste positionen per enhet. Meddelanden som inte går att tolka kastas utan att loggas – fält saknas, tiden står i fel tidszon eller koordinaterna är 0,0. Team Havsörn har ett eget radformat över TCP.

**Det betyder:** Backend lyssnar på samma UDP-ström, så att inget behöver ändras i sensorerna. Adaptern översätter JSON till kontraktet `TrackUpdate` och returnerar ett valideringsfel med orsak i stället för att kasta undantag. Alla tider lagras i UTC. Havsörns format tas upp med dem i vecka 4.

### U2. Hur levereras en ny version i dag?

**Varför vi frågar:** Manuell leverans är ett av problemen. Vi behöver veta vad som tar tid och vad som går fel.

**Svar (antaget):** En utvecklare kopierar de nya filerna till servern för hand, ungefär en gång i månaden. Det tar en halv dag. Går något fel kopierar man tillbaka den gamla versionen – om någon har sparat den. Ingen vet säkert vilken version som körs.

**Det betyder:** Allt startar med `docker compose up`. CI bygger och testar vid varje push och bygger images. En image med versionsnummer går att peka ut, och den förra versionen finns kvar om man behöver gå tillbaka.

### U3. Hur testar ni i dag?

**Varför vi frågar:** De fall som är svårast att framkalla – en sensor som tystnar, ett trasigt meddelande – är de som är viktigast för operatören.

**Svar (antaget):** För hand, mot en sparad CSV-fil. Vi kan inte få en sensor att tystna eller skicka trasig data när vi vill, så de fallen testas inte.

**Det betyder:** En simulator skickar fejkade positioner och tappar kontakten eller skickar trasig data med flit. Reglerna för validering och staleness ligger utan I/O och får tiden via `TimeProvider`, så att ett test kan flytta klockan 30 sekunder utan att vänta. Testerna körs i CI.

### U4. Hur ser nätet ut där systemet körs?

**Varför vi frågar:** Det styr vad vi får hämta, både när vi bygger och när systemet körs.

**Svar (antaget):** Driftnätet har inget internet. Paket hämtas från en intern spegel efter att säkerhetsavdelningen har godkänt dem, och det kan ta veckor per paket. Containrar byggs utanför och förs in.

**Det betyder:** Inget hämtas från internet vid körning: inga CDN:er, typsnitt, karttjänster eller telemetri. Kartdata ligger som en lokal fil. Antalet paket hålls nere, och varje paket motiveras i `docs/dependencies.md`. Vi provar att starta allt med nätverket avstängt.

### U5. Vad kan teamet i dag, och vem tar över när konsulten går?

**Varför vi frågar:** Lösningen är bara bra om teamet kan förvalta den efter sex veckor.

**Svar (antaget):** Vi är fyra utvecklare som skriver C# – skrivbordsappen är byggd i .NET. En har jobbat med React. Ingen har jobbat med Docker eller SignalR. När konsulten har gått är det vi som förvaltar.

**Det betyder:** Backend byggs i .NET, där teamet redan är starkt. Frontend hålls enkel, utan extra bibliotek för state. Besluten förklaras i ADR:er, och i vecka 6 håller vi en workshop där teamet själva lägger till en sensortyp.

## Antagandena från problemformuleringen

| Antagande                                   | Resultat                                                  | Följd                                                    |
| ------------------------------------------- | --------------------------------------------------------- | -------------------------------------------------------- |
| Sensorerna rapporterar ungefär 1 gång per s | Stämmer för Nordljus (U1). Havsörn är inte tillfrågat.    | Kravet på under 1 sekund är rimligt. Havsörn i vecka 4.  |
| Nätet är isolerat och allt körs lokalt      | Stämmer (U4)                                              | Inget hämtas vid körning. Få beroenden.                  |
| 5 och 30 s är rimliga gränser               | Delvis (O3). Rimliga som start, men beror på typen.       | Gränserna ligger i konfigurationen. Se öppna frågor.     |

## Öppna frågor

1. **Ska gränserna skilja sig mellan personal, fordon och helikoptrar?** Vi börjar med en gemensam gräns. Frågan avgörs när staleness byggs (vecka 2–3), och delar vi upp den skriver vi en ADR.
2. **Hur märker operatören att en enhet blir _Tappad_ om den ligger utanför den del av kartan hen tittar på?** Kanske en räknare eller lista över Gammal och Tappad. Avgörs när lägesbilden byggs.
3. **Hur ser Havsörns data ut – format, frekvens och trasiga meddelanden?** Tas upp med team Havsörn i vecka 4.
4. **Hur förs nya images in i driftnätet, och vem godkänner dem?** Ligger utanför de sex veckorna, men beskrivs i README.
