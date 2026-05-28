# PowerShell script to start the MCP servers using npx and uvx
# Starts both servers in separate processes, if installed.
Start-Process -NoNewWindow -FilePath "npx" -ArgumentList "-y","pointa-server"
if (Get-Command uvx -ErrorAction SilentlyContinue) {
    Start-Process -NoNewWindow -FilePath "uvx" -ArgumentList "--from","semble[mcp]","semble"
    Write-Output "Started pointa-server and uvx/semble."
} else {
    Write-Output "Started pointa-server. uvx is not installed or not available in PATH."
    Write-Output "Install uvx separately via Python (python -m pip install uv) before starting the second server."
}
