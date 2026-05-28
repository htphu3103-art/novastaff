#!/usr/bin/env bash
# POSIX shell script to start the MCP servers using npx and uvx
nohup npx -y pointa-server >/dev/null 2>&1 &
if command -v uvx >/dev/null 2>&1; then
  nohup uvx --from 'semble[mcp]' semble >/dev/null 2>&1 &
  echo "Started pointa-server and uvx/semble."
else
  echo "Started pointa-server. uvx is not installed or not available in PATH."
  echo "Install uvx separately via Python (python -m pip install uv) before starting the second server."
fi
