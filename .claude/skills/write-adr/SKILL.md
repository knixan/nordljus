---
name: write-adr
description: Use when an architecture decision is made or questioned in Nordljus (transport, data format, storage, map, anomaly detection approach, etc.). Writes a short Architecture Decision Record in Swedish under docs/adr/.
---

# Skriva en ADR

## Var

`docs/adr/NNN-kort-titel.md`, numrerat i ordning (`001`, `002`, …). Uppdatera listan i `docs/adr/README.md` (skapa den om den saknas).

## Mall

```markdown
# ADR-NNN: <Beslut som en mening>

- **Status:** Föreslagen | Beslutad | Ersatt av ADR-XXX
- **Datum:** ÅÅÅÅ-MM-DD

## Sammanhang
Vilket problem, vilka krav (isolerat nät, < 1 s latens, spårbarhet, robusthet, återanvändning) och vilka begränsningar påverkar beslutet?

## Alternativ
1. **<Alternativ A>** – för- och nackdelar
2. **<Alternativ B>** – för- och nackdelar
3. **<Alternativ C>** – för- och nackdelar

## Beslut
Vad vi valde och det viktigaste skälet.

## Konsekvenser
Vad blir lättare, vad blir svårare, vad måste vi hålla koll på framöver.
```

## Riktlinjer

- Högst en sida. Skriv för en ny utvecklare i teamet, inte för en granskare.
- Koppla alltid till minst ett av kraven i AGENTS.md.
- Var ärlig om nackdelar. En ADR utan nackdelar ser inte genomtänkt ut.
- Ändra aldrig en beslutad ADR i efterhand. Skriv en ny som ersätter den.
