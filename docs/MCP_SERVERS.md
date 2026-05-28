MCP Servers: pointa and semble

Files added:
- package.json — devDependencies and npm scripts
- scripts/run-mcp.ps1 — PowerShell start script
- scripts/run-mcp.sh — POSIX shell start script

Usage

1. Install dependencies (optional; npx will download on demand):

```
npm run mcp:install
```

2. On Windows (PowerShell):

```
npm run mcp:run:ps
```

3. On macOS / Linux (bash):

```
npm run mcp:run:sh
```

Notes

- The repo root contains `mcp_config.json` which lists the commands used; no changes were made to it.
- `pointa-server` is installed via npm, while `uvx` is a Python-based runtime installed through `python -m pip install uv`.
- If `uvx` is not available on PATH, install it via Python and then rerun `npm run mcp:run:ps` or `npm run mcp:run:sh`.
- The `uvx` command is not an npm package and cannot be installed with `npm install uvx`.
