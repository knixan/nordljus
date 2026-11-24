---
name: granska-mot-planen
description: "Granska ändringen på två separata axlar, var och en i en egen underagent: Planen (gör diffen det planen och kanban-kortet bad om, utan att något saknas eller gjorts fel?) och Reglerna (bryter den mot en skriven regel i AGENTS.md som ingen lint eller test redan fångar?). Startas bara av användaren med /granska-mot-planen. Not a bug hunt: that is /code-review."
argument-hint: "[planfil | #kortnummer | ingen] [HEAD~N]"
disable-model-invocation: true
---

# Granska mot planen

Två frågor, hålls isär med avsikt:

- **Planen**: är det rätt sak? Gör diffen det planen och kortet bad om?
- **Reglerna**: är det byggt så som `AGENTS.md` säger att saker byggs här?

En ändring kan klara den ena och falla på den andra. Varje axel körs i en egen underagent
så att ingen färgar den andra, och rapporten slår aldrig ihop dem. Buggar ligger på
ingen av axlarna: det är `/code-review`. Allmän städning är `/simplify`.

Bara användaren startar den (`disable-model-invocation`), för att två underagenter kostar.

## 1. Lås diffen, stanna här och inte inne i två agenter

Jobbet sker på `main` och användaren committar själv, så det som granskas är oftast det
som inte är committat.

1. Står ett commit-intervall i argumentet (`HEAD~3`, en sha): diff = `git diff <den>`
   (allt från den committen till arbetskopian, committat och inte).
2. Annars: diff = `git diff HEAD`. Lista också nya filer med
   `git ls-files --others --exclude-standard`; de syns inte i `git diff` och agenterna
   ska läsa dem direkt.
3. Tom diff och inga nya filer → stanna och säg det. Föreslå `HEAD~1` om användaren just
   committat.
4. Lämna ut generad massa ur diffkommandot (agenterna ser filerna i `--stat` ändå):
   `git diff <bas> -- . ':(exclude)frontend/package-lock.json' ':(exclude)src/*/Migrations/*.Designer.cs' ':(exclude)src/*/Migrations/*ModelSnapshot.cs'`
5. `git diff <bas> --stat` och, om basen är en commit, `git log <bas>..HEAD --oneline`.

## 2. Hitta planen och kortet (Planen-axeln)

Argumentet bestämmer:

- en sökväg → den filen är planen;
- `#12` eller `12` → kanban-kortet är underlaget (`gh issue view 12 --comments`); ett
  draft-kort hittas med titeln i `gh project item-list 24 --owner knixan --format json`;
- `ingen` → hoppa över Planen-axeln; rapporten säger "Planen: hoppades över (ingen plan)";
- tomt → den här sessionens planfil, om plan mode gav en (`~/.claude/plans/<namn>.md`).

Leta också efter kortnumret (om det inte är `ingen`): i planfilen, i commit-meddelandena
(`Closes #12`, `Del av #12`) eller i sessionen. Varken plan eller kort → fråga användaren
efter en sökväg, ett kortnummer eller `ingen`. Både plan och kort → lämna ut båda; planen
vinner där de skiljer sig, och granskaren rapporterar skillnaden.

Plocka aldrig en plan ur `~/.claude/plans/` genom att söka: filerna har slumpade namn och
flera kan nämna samma kort.

## 3. Hitta reglerna (Reglerna-axeln)

- `AGENTS.md` (via `CLAUDE.md`) är regelfilen. Lämna ut sökvägen och säg vilka avsnitt
  som rör diffen (§ Gör så här, § UI, § Domänregler, § Beroenden, § Databas …).
- Vad som redan fångas, och därför inte är ett fynd: `dotnet build` (varningar = fel), `dotnet format`, ESLint, Prettier, `tsc`, testerna i
  `tests/` och `frontend/src/**/*.test.ts` (lista dem med `git ls-files tests "*.test.ts"`) och hookarna i
  `.claude/hooks/`. Lämna ut listan.

## 4. Starta båda granskarna, i ett meddelande, båda i förgrunden

Anropa Agent-verktyget två gånger i SAMMA meddelande, med `subagent_type: "plan-granskare"`
och `subagent_type: "regel-granskare"` (`.claude/agents/plan-granskare.md` och
`.claude/agents/regel-granskare.md`). Ge var och en BARA:

- diffkommandot, `--stat`-kommandot, listan med nya filer och ev. commit-listan från steg 1;
- sina egna källor: planens sökväg och/eller kortnumret till plan-granskaren; `AGENTS.md`,
  avsnitten och listan över det som redan fångas till regel-granskaren;
- raden: "Starta inga skills och inga agenter: gör granskningen själv."

Aldrig din egen sammanfattning av arbetet, och aldrig den ena granskarens svar till den
andra: poängen är en läsare som inte skrev koden. Med `ingen` startar bara regel-granskaren.

## 5. Rapport, två block som aldrig slås ihop

```
## Planen
<plan-granskarens rapport, ordagrant eller lätt städad>

## Reglerna
<regel-granskarens rapport, ordagrant eller lätt städad>

**Summering:** Planen N fynd (värst: …) · Reglerna M fynd (värst: …)
```

Ingen vinnare mellan axlarna och ingen omrankning.

## 6. Pröva varje fynd innan du ändrar något

Ett fynd är en ledtråd, inte ett faktum. Per fynd: öppna raderna, kontrollera citatet och
markera ✔ håller · ✘ avvisad (+ varför) · ? ej prövad. Rätta bara ✔. Redovisa varje ✘
med skälet, släpp aldrig ett i tysthet.

- "inte i planen" är inget fel i sig: se till att det nämns i sammanfattningen och
  commit-meddelandet, eller ta bort det om det kom med av misstag.
- Rätta fynden och stanna. Kör inte om för att "få det rent": Reglerna-axeln är bedömning
  och hittar något nytt varje gång. Kör om bara om diffen vuxit med nya steg.
- Sedan: kontrollerna ("Klart betyder" punkt 1 i `AGENTS.md`)
  och steg 6 i `/arbetsflode`.

## Det fungerar om

- Den stannar innan någon agent startar när diffen är tom.
- Exakt två agenter startar; en med `ingen`.
- Rapporten har ett block `## Planen` och ett `## Reglerna`, aldrig ihopslagna.
- Varje Planen-fynd citerar planen eller kortet; varje Reglerna-fynd citerar regeln i
  `AGENTS.md` och pekar på diffen.
- Utan plan säger Planen-blocket det, i stället för att hitta på krav från diffen.
- Varje fynd är markerat ✔, ✘ eller ? innan något ändras.
