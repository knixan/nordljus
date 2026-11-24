// PostToolUse(Edit|Write): formaterar filen som just ändrades.
//
// - C# (.cs): `dotnet format whitespace --folder` på just den filen. Snabbt, kräver
//   ingen kompilering. Stil och analysatorer kontrolleras av `dotnet build`
//   (TreatWarningsAsErrors) och `dotnet format --verify-no-changes` i CI.
// - Frontend (frontend/**): Prettier och sedan ESLint med frontendens egen config.
//   Lintfel skickas tillbaka till agenten (exit 2 + stderr) medan ändringen är färsk.
//
// Körs direkt av Node (24+) utan byggsteg – typerna strippas bort. Använd därför
// bara typsyntax som går att ta bort (inga enum, namespace eller parameter properties).
// Sätt `process.exitCode` i stället för `process.exit()`: ett abrupt avslut medan
// stdin stängs kan krascha Node i Windows.
//
// Fail-open: allt vi inte kan hantera säkert släpps igenom med exit 0. En hook som
// stoppar allt på grund av egna buggar är värre än det den skyddar mot.
import { execFileSync } from "node:child_process";
import { existsSync, readFileSync } from "node:fs";
import { createRequire } from "node:module";
import path from "node:path";

type HookInput = {
  tool_input?: { file_path?: unknown };
};

type ExecError = {
  status?: number | null;
  stdout?: string;
  stderr?: string;
};

/** Sökväg till ett pakets CLI, löst från en viss mapps package.json. */
function bin(fran: string, paket: string, ...delar: string[]): string | null {
  try {
    const req = createRequire(path.join(fran, "package.json"));
    return path.join(path.dirname(req.resolve(`${paket}/package.json`)), ...delar);
  } catch {
    return null;
  }
}

function formateraCSharp(repo: string, rel: string): number {
  try {
    execFileSync("dotnet", ["format", "whitespace", "--folder", "--include", rel], {
      cwd: repo,
      stdio: "ignore",
      timeout: 45_000,
    });
  } catch {
    // dotnet saknas, syntaxfel o.d. – bygget säger mer.
  }
  return 0;
}

function formateraOchLintaFrontend(frontend: string, abs: string, rel: string): number {
  const relFrontend = path.relative(frontend, abs);

  const prettier = bin(frontend, "prettier", "bin", "prettier.cjs");
  if (prettier) {
    try {
      execFileSync(
        process.execPath,
        [prettier, "--write", "--ignore-unknown", "--log-level", "warn", relFrontend],
        { cwd: frontend, stdio: "ignore" },
      );
    } catch {
      // Syntaxfel o.d. – ESLint nedan (eller typkontrollen) säger mer.
    }
  }

  if (!/\.(ts|tsx|mts)$/.test(abs)) return 0;
  const harEslintConfig = ["eslint.config.js", "eslint.config.mjs", "eslint.config.ts"].some(
    (f) => existsSync(path.join(frontend, f)),
  );
  if (!harEslintConfig) return 0;

  const eslint = bin(frontend, "eslint", "bin", "eslint.js");
  if (!eslint) return 0;

  try {
    execFileSync(process.execPath, [eslint, relFrontend, "--quiet", "--no-warn-ignored"], {
      cwd: frontend,
      stdio: ["ignore", "pipe", "pipe"],
      encoding: "utf8",
    });
    return 0;
  } catch (err) {
    // Exit 1 = lintfel. Annat (krasch, saknad config) släpps igenom.
    const e = err as ExecError;
    if (e?.status !== 1) return 0;
    const ut = `${e.stdout ?? ""}${e.stderr ?? ""}`.trim();
    if (!ut) return 0;
    console.error(`ESLint hittade fel i ${rel}:\n\n${ut}`);
    return 2;
  }
}

function main(): number {
  const repo = process.env.CLAUDE_PROJECT_DIR;
  if (!repo) return 0;

  let fil: string;
  try {
    const input = JSON.parse(readFileSync(0, "utf8")) as HookInput;
    fil = String(input?.tool_input?.file_path ?? "");
  } catch {
    return 0;
  }
  if (!fil || !existsSync(fil)) return 0;

  const abs = path.resolve(fil);
  const rel = path.relative(repo, abs);
  if (rel.startsWith("..") || path.isAbsolute(rel)) return 0;
  if (/(^|[\\/])(node_modules|bin|obj|dist|Migrations)[\\/]/.test(rel)) return 0;

  if (abs.endsWith(".cs")) return formateraCSharp(repo, rel);

  const frontend = path.join(repo, "frontend");
  if (path.relative(frontend, abs).startsWith("..")) return 0;
  return formateraOchLintaFrontend(frontend, abs, rel);
}

process.exitCode = main();
