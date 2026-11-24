---
name: granska-i-appen
description: 'Steg 4 i arbetsflödet: prova en ändring i den riktiga appen. Starta allt med docker compose (eller dev-servrar), titta på lägesbilden som en operatör, stoppa en sensor och skicka trasig data, ta och ÖPPNA skärmbilder av varje läge (mörkt/ljust, Live/Gammal/Tappad, smal skärm), läs konsolen och kontrollera att inget hämtas från internet. Use after building and before /granska-mot-planen, and again after fixes that show in the UI. Triggers on "prova i appen", "testa hur det blev", "ta skärmbilder", "visuell QA", "granska i UI", "/granska-i-appen".'
---

# Granska i appen

xUnit och Vitest ritar ingen karta och sitter inte i ett mörkt rum. Ett grönt test på vad
som **loggades** säger inget om vad operatören **ser**. Det här steget fångar det.

## 1. Starta

- Kolla först om något redan går (`docker compose ps`, `http://localhost:5173`).
- Hela kedjan: `docker compose up --build` med `run_in_background`. Vänta tills backend
  svarar och simulatorn skickar innan du tittar.
- Bara frontend ändrad: backend och simulator i Docker, `npm run dev` i `frontend/`.
- Databasen är testdata. Kör ändå aldrig `docker compose down -v` utan att säga till.

## 2. Verktyg och skärmbilder

- Använd skillen `browser-automation` (huvudlös webbläsare: konsol, nätverk, DOM,
  skärmbild). Har användaren Claude i Chrome eller den inbyggda webbläsaren går de också.
- Lägg egna skript och skärmbilder i scratchpad-mappen, aldrig i repot.
- **Gissa inte en etikett, slå upp den** i komponenten under `frontend/src/`.

## 3. Vad du provar, som en operatör, inte som koden

- **Rörelse:** rör sig objekten jämnt, utan hopp eller flimmer? Stämmer typ och ikon
  (personal, markfordon, flyg)?
- **Gammal och tappad data:** stoppa en sensor (`docker compose stop simulator`, eller
  simulatorns läge för tappad kontakt). Går den Live → Gammal → Tappad efter de tider som
  står i konfigurationen? Syns det utan att läsa färg (ikon, text, ålder)? Fryser något
  tyst?
- **Trasig data:** låt simulatorn skicka trasiga meddelanden. Kraschar ingenting? Hamnar
  de i händelseloggen? Syns något för operatören där det ska?
- **Latens:** jämför tidsstämpeln från simulatorn med när uppdateringen syns. Under 1 s?
- **Isolerat nät:** läs nätverksfliken under hela körningen. **Varje anrop som inte går
  till localhost är ett fel** (typsnitt, kartplattor, ikoner, telemetri).
- **Mörkt och ljust läge**, och kontrast mot kartan. Statusfärger går att skilja åt.
- **Smal skärm** (390 px) och dator. Sidopanelen täcker inte kartan helt.
- **Återanslutning:** starta om backend. Kommer frontend tillbaka av sig själv, och visas
  det att anslutningen var borta?
- **Havsörn** (från V4): fungerar Havsörns vy med samma komponent?
- **Konsolen:** läs `console` och `pageerror` under hela körningen. Nya varningar räknas.
- **Prestanda** (vid ändringar i rendering eller hubb): mät FPS och CPU med det antal
  objekt kortet gäller. Skriv siffrorna.

## 4. Öppna bilderna

Ta en skärmbild efter varje steg och **öppna den**. En bild som aldrig öppnats är ingen
granskning. Leta efter markörer som överlappar oläsligt, text som bryts konstigt, låg
kontrast, en gammal markör som ser färsk ut.

## Ut

- Skärmbilderna visade i chatten (Markdown-bild med full sökväg).
- En kort lista: **vad jag såg** · **vad som var fel** · **vad jag inte kunde prova** (och varför).
- **Prova själv**, högst sex rader, så att användaren ser det med egna ögon:

  ```
  Prova själv
  • docker compose up --build, öppna http://localhost:5173
  • Stoppa simulatorn: docker compose stop simulator
  • Rätt: <exakt vad som ska synas, t.ex. Örn 2 grå med "Gammal 12 s" efter 10 s, "Tappad" efter 30 s>
  • Starta simulatorn igen, Örn 2 ska bli Live utan omladdning
  ```

- Fel du hittar rättas och provas om innan nästa steg. Steget är klart först när
  användaren sagt att det ser rätt ut.
