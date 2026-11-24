---
name: kanban
description: 'Visa och uppdatera kanban-tavlan för Nordljus (GitHub Project 24): vad som pågår, vad som är nästa kort, flytta kort mellan Backlog/Later/In progress/Done, göra om draft-kort till issues och skapa nya kort. Use when the user writes /kanban, "vad ska jag göra nu", "nästa kort", "uppdatera kanban", "flytta kortet", "skapa ett kort" or asks about the board/project.'
---

# /kanban – tavlan för Nordljus

ID:n, kommandon och regler står i `AGENTS.md` § Kanban. Läs den delen först.

## Användning

| Användaren skriver       | Gör                                                                                        |
| ------------------------ | ------------------------------------------------------------------------------------------ |
| `/kanban`                | Visa tavlan (se nedan).                                                                    |
| `/kanban nästa`          | Föreslå nästa kort att jobba på, med motivering.                                           |
| `/kanban börja <sökord>` | Gör om till issue om det är ett draft-kort, flytta till _In progress_, sammanfatta kortet. |
| `/kanban klar <sökord>`  | Flytta kortet till _Done_.                                                                 |
| `/kanban nytt <titel>`   | Skapa en issue och lägg den i Backlog. Saknas `[V1]`–`[V6]`/`[Bonus]` – föreslå ett.       |
| `/kanban städa`          | Jämför _In progress_ med koden och föreslå vad som kan flyttas.                            |

`<sökord>` är ett issuenummer (`12`, `#12`) eller matchas mot titeln (skiftlägesokänsligt). Matchar flera kort – fråga vilket.

## Visa tavlan

1. Hämta alla kort: `gh project item-list 24 --owner knixan --format json --limit 200`.
2. Visa kort och tydligt, i den här ordningen:
   - **In progress** – alla kort.
   - **Backlog** – grupperat per vecka (`[V1]` … `[V6]`), med prioritet, antal per vecka och titlarna.
   - **Later** – titlarna (bonuskort).
   - **Done** – bara antal, och hur många P0 som är kvar till "Klart när".
3. Avsluta med ett förslag på nästa kort (se nedan) – en mening.

## Föreslå nästa kort

- Avsluta det som pågår först. Högst två kort i _In progress_.
- Lägsta veckan först, inom veckan P0 före P1. `[Bonus]` först när alla P0-kort är klara.
- Följ beroendena: V1 (förstå, repo) → tunn skiva (en prick rör sig hela vägen) → simulator, backend, frontend → Docker och CI → kontrakt och adapters → Havsörn → avvikelser och prestanda → dokumentation och workshop.
- Överraskningskorten (beroenden V3, mörkt läge V4, 5 000 objekt V5) tas den vecka de står i – det är meningen att de stör.
- Föredra kort som är små nog för en session. Är ett kort stort – föreslå att dela upp det.

## Ändra kort

- Visa vad du tänker ändra innan du ändrar fler än ett kort i taget.
- `id` (`PVTI_…`) används för status. För issues används `content.number` för `gh issue view/edit/comment`.
- **Draft-kort → issue** när arbetet börjar, så att commit-meddelandet kan säga `Closes #<nr>`:

  ```bash
  gh api graphql -f query='mutation($id:ID!,$repo:ID!){convertProjectV2DraftIssueItemToIssue(input:{itemId:$id,repositoryId:$repo}){item{content{... on Issue{number url}}}}}' \
    -f id=<PVTI_…> -f repo=$(gh repo view knixan/nordljus --json id --jq .id)
  ```

  Status, prioritet, storlek och datum följer med kortet.
- Skriv långa bodies och kommentarer i en fil i scratchpad och använd `--body-file`.
- Bekräfta efteråt med en rad per kort: `#12 [V2] Titel: Backlog → In progress`.
