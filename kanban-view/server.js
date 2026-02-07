const http = require("http");
const fs = require("fs");
const path = require("path");

const port = Number(process.env.PORT) || 5173;
const staticRoot = __dirname;
const mikadoDir = process.env.MIKADO_DIR || path.join(__dirname, "mikado");

const contentTypes = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".json": "application/json; charset=utf-8"
};

function sendJson(res, statusCode, data) {
  res.writeHead(statusCode, { "Content-Type": contentTypes[".json"] });
  res.end(JSON.stringify(data, null, 2));
}

function listGraphs() {
  return new Promise((resolve) => {
    fs.readdir(mikadoDir, (error, files) => {
      if (error) {
        resolve({ graphs: [], errors: [{ file: mikadoDir, message: error.message }] });
        return;
      }

      const jsonFiles = files.filter((file) => file.toLowerCase().endsWith(".json"));
      const graphs = [];
      const errors = [];

      if (jsonFiles.length === 0) {
        resolve({ graphs, errors });
        return;
      }

      let pending = jsonFiles.length;
      jsonFiles.forEach((file) => {
        const filePath = path.join(mikadoDir, file);
        fs.readFile(filePath, "utf-8", (readError, content) => {
          if (readError) {
            errors.push({ file, message: readError.message });
          } else {
            try {
              const parsed = JSON.parse(content);
              const graph = parsed.mikado_graph || parsed;
              if (!graph || !graph.nodes) {
                errors.push({ file, message: "Missing mikado_graph.nodes" });
              } else {
                graphs.push({ name: file.replace(/\.json$/i, ""), graph });
              }
            } catch (parseError) {
              errors.push({ file, message: parseError.message });
            }
          }

          pending -= 1;
          if (pending === 0) {
            graphs.sort((a, b) => a.name.localeCompare(b.name));
            resolve({ graphs, errors });
          }
        });
      });
    });
  });
}

const server = http.createServer(async (req, res) => {
  const urlPath = (req.url || "/").split("?")[0];

  if (urlPath === "/api/graphs") {
    const payload = await listGraphs();
    sendJson(res, 200, payload);
    return;
  }

  const safePath = urlPath === "/" ? "/index.html" : urlPath;
  const filePath = path.join(staticRoot, safePath);

  if (!filePath.startsWith(staticRoot)) {
    res.writeHead(403);
    res.end("Forbidden");
    return;
  }

  fs.readFile(filePath, (error, content) => {
    if (error) {
      res.writeHead(404);
      res.end("Not found");
      return;
    }

    const ext = path.extname(filePath).toLowerCase();
    res.writeHead(200, { "Content-Type": contentTypes[ext] || "text/plain" });
    res.end(content);
  });
});

server.listen(port, () => {
  console.log(`Mikado Kanban server running on http://localhost:${port}`);
  console.log(`Using mikado directory: ${mikadoDir}`);
});
