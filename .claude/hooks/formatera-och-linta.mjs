// PostToolUse(Edit|Write): formaterar filen som just ändrades.
//
// - C# (.cs): `dotnet format whitespace --folder` på just den filen. Snabbt, kräver
//   ingen kompilering. Stil och analysatorer kontrolleras av `dotnet build`
//   (TreatWarningsAsErrors) och `dotnet format --verify-no-changes` i CI.
// - Frontend (frontend/**): Prettier och sedan ESLint med frontendens egen config.
//   Lintfel skickas tillbaka till agenten (exit 2 + stderr) medan ändringen är färsk.
//
// Fail-open: allt vi inte kan hantera säkert släpps igenom med exit 0. En hook som
// stoppar allt på grund av egna buggar är värre än det den skyddar mot.
import { execFileSync } from "node:child_process";
import { existsSync, readFileSync } from "node:fs";
import { createRequire } from "node:module";
import path from "node:path";

const repo = process.env.CLAUDE_PROJECT_DIR;
if (!repo) process.exit(0);

let fil = "";
try {
  const input = JSON.parse(readFileSync(0, "utf8"));
  fil = String(input?.tool_input?.file_path ?? "");
} catch {
  process.exit(0);
}
if (!fil || !existsSync(fil)) process.exit(0);

const abs = path.resolve(fil);
const rel = path.relative(repo, abs);
if (rel.startsWith("..") || path.isAbsolute(rel)) process.exit(0);
if (/(^|[\\/])(node_modules|bin|obj|dist|Migrations)[\\/]/.test(rel)) process.exit(0);

// 1. C#
if (/\.cs$/.test(abs)) {
  try {
    execFileSync(
      "dotnet",
      ["format", "whitespace", "--folder", "--include", rel],
      { cwd: repo, stdio: "ignore", timeout: 45_000 },
    );
  } catch {
    // dotnet saknas, syntaxfel o.d. – bygget säger mer.
  }
  process.exit(0);
}

// 2. Frontend
const frontend = path.join(repo, "frontend");
const iFrontend = !path.relative(frontend, abs).startsWith("..");
if (!iFrontend) process.exit(0);

/** Sökväg till ett pakets CLI, löst från frontendens package.json. */
function bin(paket, ...delar) {
  try {
    const req = createRequire(path.join(frontend, "package.json"));
    return path.join(path.dirname(req.resolve(`${paket}/package.json`)), ...delar);
  } catch {
    return null;
  }
}

const relFrontend = path.relative(frontend, abs);

const prettier = bin("prettier", "bin", "prettier.cjs");
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

if (!/\.(ts|tsx|mts|mjs)$/.test(abs)) process.exit(0);
if (!existsSync(path.join(frontend, "eslint.config.js")) && !existsSync(path.join(frontend, "eslint.config.mjs")))
  process.exit(0);

const eslint = bin("eslint", "bin", "eslint.js");
if (!eslint) process.exit(0);

try {
  execFileSync(process.execPath, [eslint, relFrontend, "--quiet", "--no-warn-ignored"], {
    cwd: frontend,
    stdio: ["ignore", "pipe", "pipe"],
    encoding: "utf8",
  });
  process.exit(0);
} catch (err) {
  // Exit 1 = lintfel. Annat (krasch, saknad config) släpps igenom.
  if (err?.status !== 1) process.exit(0);
  const ut = `${err.stdout ?? ""}${err.stderr ?? ""}`.trim();
  if (!ut) process.exit(0);
  console.error(`ESLint hittade fel i ${rel}:\n\n${ut}`);
  process.exit(2);
}
