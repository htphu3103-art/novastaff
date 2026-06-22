const express = require("express");
const cors = require("cors");

const app = express();

app.use(cors());
app.use(express.json());

// lưu context UI gần nhất
let lastContext = null;

// nhận dữ liệu từ browser / extension
app.post("/context", (req, res) => {
  lastContext = req.body;

  console.log("\n=== UI CONTEXT RECEIVED ===");
  console.log(JSON.stringify(lastContext, null, 2));

  res.json({ ok: true });
});

// Windsurf / agent đọc dữ liệu ở đây
app.get("/context", (req, res) => {
  res.json(lastContext || { message: "no context yet" });
});

app.listen(3000, () => {
  console.log("MCP server running on http://localhost:3000");
});