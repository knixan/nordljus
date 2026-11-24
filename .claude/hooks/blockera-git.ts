// PreToolUse(Bash|PowerShell): stoppar `git commit` och `git push` från AI-agenter.
// Användaren committar själv (se AGENTS.md § Git). Exit 2 = blockera, och
// texten på stderr skickas tillbaka till agenten.
//
// Körs direkt av Node (24+) utan byggsteg – typerna strippas bort. Använd därför
// bara typsyntax som går att ta bort (inga enum, namespace eller parameter properties).
// Sätt `process.exitCode` i stället för `process.exit()`: ett abrupt avslut medan
// stdin stängs kan krascha Node i Windows.
//
// Fail-open: kan vi inte läsa indata släpps kommandot igenom – en hook som
// stoppar allt på grund av egna buggar är värre än det den skyddar mot.
import { readFileSync } from "node:fs";

type HookInput = {
  tool_input?: { command?: unknown };
};

// `git commit`, `git push`, även efter && / ; / | och med flaggor före (git -C x commit).
const blockerat: RegExp =
  /(^|[;&|(]\s*|\s)git(\s+-[^\s]+(\s+[^\s-][^\s]*)?)*\s+(commit|push)\b/;

function main(): number {
  let command: string;
  try {
    const input = JSON.parse(readFileSync(0, "utf8")) as HookInput;
    command = String(input?.tool_input?.command ?? "");
  } catch {
    return 0;
  }

  if (!blockerat.test(command)) return 0;

  console.error(
    [
      "Stoppat: agenter får inte committa eller pusha i det här repot (AGENTS.md § Git).",
      "Skriv i stället ett commit-meddelande till användaren, som committar själv.",
    ].join("\n"),
  );
  return 2;
}

process.exitCode = main();
