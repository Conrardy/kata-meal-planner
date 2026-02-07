const fileInput = document.getElementById("fileInput");
const loadSample = document.getElementById("loadSample");
const board = document.getElementById("board");
const errorBox = document.getElementById("error");
const tabs = document.getElementById("tabs");

const goalEl = document.getElementById("goal");
const rootEl = document.getElementById("root");
const updatedEl = document.getElementById("updated");
const statsEl = document.getElementById("stats");

const statusOrder = ["todo", "doing", "in-progress", "blocked", "done"];
let loadedGraphs = [];
let activeIndex = -1;
const autoLoadUrl = "api/graphs";

const sampleData = {
  mikado_graph: {
    version: "1.0",
    goal: "Use Firebase as persistence to replace PostgreSQL",
    created_at: "2026-02-07T10:19:39.3312612+01:00",
    updated_at: "2026-02-07T10:19:39.3312612+01:00",
    nodes: {
      "migration-firebase": {
        id: "migration-firebase",
        description: "Use Firebase as persistence to replace PostgreSQL",
        status: "todo",
        depends_on: [
          "decider-produit-firebase",
          "modeler-donnees-et-migrations",
          "implementer-repositories-firebase",
          "config-env-secrets",
          "mettre-a-jour-tests",
          "mettre-a-jour-docs-et-compose"
        ],
        notes: "",
        created_at: "2026-02-07T10:19:39.3312612+01:00",
        updated_at: "2026-02-07T10:19:39.3312612+01:00"
      },
      "decider-produit-firebase": {
        id: "decider-produit-firebase",
        description: "Choose Firebase product and auth strategy",
        status: "todo",
        depends_on: [],
        notes: "",
        created_at: "2026-02-07T10:19:39.3312612+01:00",
        updated_at: "2026-02-07T10:19:39.3312612+01:00"
      }
    },
    root: "migration-firebase"
  }
};

function showError(message) {
  errorBox.textContent = message;
  errorBox.hidden = false;
}

function clearError() {
  errorBox.textContent = "";
  errorBox.hidden = true;
}

function formatDate(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

function normalizeGraph(data) {
  if (!data) return null;
  if (data.mikado_graph) return data.mikado_graph;
  return data;
}

function normalizeGraphSafe(data, fileName) {
  const graph = normalizeGraph(data);
  if (!graph || !graph.nodes) {
    return { ok: false, error: `Missing mikado_graph.nodes in ${fileName}` };
  }
  return { ok: true, graph };
}

function buildColumns(nodes) {
  const statusSet = new Set(nodes.map((node) => node.status || "todo"));
  const ordered = statusOrder.filter((status) => statusSet.has(status));
  const extras = [...statusSet].filter((status) => !statusOrder.includes(status));
  extras.sort();
  return [...ordered, ...extras];
}

function updateSummary(graph, stats) {
  goalEl.textContent = graph.goal || "-";
  rootEl.textContent = graph.root || "-";
  updatedEl.textContent = formatDate(graph.updated_at || graph.created_at);
  statsEl.textContent = `${stats.totals} nodes, ${stats.done} done, ${stats.blocked} blocked, ${stats.ready} ready`;
}

function resetSummary() {
  goalEl.textContent = "-";
  rootEl.textContent = "-";
  updatedEl.textContent = "-";
  statsEl.textContent = "-";
}

function renderTabs() {
  tabs.innerHTML = "";
  if (loadedGraphs.length <= 1) {
    tabs.hidden = true;
    return;
  }

  loadedGraphs.forEach((entry, index) => {
    const button = document.createElement("button");
    button.type = "button";
    button.className = index === activeIndex ? "tab active" : "tab";
    button.textContent = entry.name;
    button.addEventListener("click", () => selectGraph(index));
    tabs.appendChild(button);
  });

  tabs.hidden = false;
}

function selectGraph(index) {
  if (!loadedGraphs[index]) return;
  activeIndex = index;
  renderTabs();
  renderBoard(loadedGraphs[index].graph);
}

function renderBoard(graph) {
  if (!graph || !graph.nodes) {
    showError("Invalid JSON: missing mikado_graph.nodes");
    return;
  }

  const nodeEntries = Object.values(graph.nodes);
  const nodes = nodeEntries.map((node) => ({
    ...node,
    status: node.status || "todo",
    depends_on: Array.isArray(node.depends_on) ? node.depends_on : []
  }));

  const nodeMap = new Map(nodes.map((node) => [node.id, node]));
  const columns = buildColumns(nodes);

  const totals = nodes.length;
  const doneCount = nodes.filter((node) => node.status === "done").length;
  const blockedCount = nodes.filter((node) => {
    const unmet = node.depends_on.filter((dep) => nodeMap.get(dep)?.status !== "done");
    return unmet.length > 0;
  }).length;
  const readyCount = nodes.filter((node) => {
    if (node.status === "done") return false;
    const unmet = node.depends_on.filter((dep) => nodeMap.get(dep)?.status !== "done");
    return unmet.length === 0;
  }).length;

  updateSummary(graph, {
    totals,
    done: doneCount,
    blocked: blockedCount,
    ready: readyCount
  });

  board.innerHTML = "";

  columns.forEach((status) => {
    const column = document.createElement("div");
    column.className = "column";

    const header = document.createElement("div");
    header.className = "column-header";

    const title = document.createElement("div");
    title.className = "column-title";
    title.textContent = status.replace(/-/g, " ");

    const count = document.createElement("div");
    count.className = "column-count";

    const items = nodes.filter((node) => node.status === status);
    count.textContent = `${items.length} items`;

    header.append(title, count);
    column.appendChild(header);

    items.forEach((node, index) => {
      const card = document.createElement("div");
      card.className = "card";
      card.style.animationDelay = `${index * 0.05}s`;

      const cardTitle = document.createElement("div");
      cardTitle.className = "card-title";
      cardTitle.textContent = node.id;

      const cardDesc = document.createElement("div");
      cardDesc.className = "card-desc";
      cardDesc.textContent = node.description || "";

      const tagList = document.createElement("div");
      tagList.className = "tag-list";

      if (node.depends_on.length === 0) {
        const tag = document.createElement("span");
        tag.className = "tag";
        tag.textContent = "no deps";
        tagList.appendChild(tag);
      } else {
        node.depends_on.forEach((dep) => {
          const depNode = nodeMap.get(dep);
          const isMet = depNode && depNode.status === "done";
          const tag = document.createElement("span");
          tag.className = isMet ? "tag" : "tag blocked";
          tag.textContent = dep;
          tagList.appendChild(tag);
        });
      }

      const meta = document.createElement("div");
      meta.className = "meta";
      meta.textContent = formatDate(node.updated_at || node.created_at);

      card.append(cardTitle, cardDesc, tagList, meta);
      column.appendChild(card);
    });

    board.appendChild(column);
  });
}

async function tryAutoLoad() {
  try {
    const response = await fetch(autoLoadUrl, { cache: "no-store" });
    if (!response.ok) return;

    const payload = await response.json();
    const graphs = Array.isArray(payload.graphs) ? payload.graphs : [];
    const errors = Array.isArray(payload.errors) ? payload.errors : [];

    if (graphs.length === 0) {
      if (errors.length > 0) {
        showError("No valid mikado_graph JSON found in auto-load.");
      }
      return;
    }

    loadedGraphs = graphs;
    activeIndex = 0;
    renderTabs();
    renderBoard(loadedGraphs[0].graph);

    if (errors.length > 0) {
      showError(`Some files were skipped: ${errors.map((item) => item.file).join(", ")}`);
    }
  } catch (error) {
    // Auto-load is optional, fallback to manual selection.
  }
}

function readFileAsJson(file) {
  return new Promise((resolve) => {
    const reader = new FileReader();
    reader.onload = () => {
      try {
        const parsed = JSON.parse(reader.result);
        resolve({ ok: true, file, data: parsed });
      } catch (error) {
        resolve({ ok: false, file, error: "Invalid JSON" });
      }
    };
    reader.onerror = () => resolve({ ok: false, file, error: "Read error" });
    reader.readAsText(file);
  });
}

async function handleFiles(fileList) {
  clearError();
  resetSummary();
  loadedGraphs = [];
  activeIndex = -1;
  board.innerHTML = "";

  const files = Array.from(fileList || []).filter((file) => file.name.endsWith(".json"));
  if (files.length === 0) {
    showError("No JSON files selected.");
    renderTabs();
    return;
  }

  const results = await Promise.all(files.map(readFileAsJson));
  const invalidFiles = results.filter((result) => !result.ok).map((result) => result.file.name);

  const validGraphs = [];
  const invalidGraphs = [];

  results
    .filter((result) => result.ok)
    .forEach((result) => {
      const normalized = normalizeGraphSafe(result.data, result.file.name);
      if (normalized.ok) {
        validGraphs.push({
          name: result.file.name.replace(/\.json$/i, ""),
          graph: normalized.graph
        });
      } else {
        invalidGraphs.push(result.file.name);
      }
    });

  if (invalidFiles.length || invalidGraphs.length) {
    const allInvalid = [...new Set([...invalidFiles, ...invalidGraphs])];
    showError(`Some files were skipped: ${allInvalid.join(", ")}`);
  }

  if (validGraphs.length === 0) {
    showError("No valid mikado_graph JSON found.");
    renderTabs();
    return;
  }

  loadedGraphs = validGraphs;
  activeIndex = 0;
  renderTabs();
  renderBoard(loadedGraphs[0].graph);
}

fileInput.addEventListener("change", (event) => {
  handleFiles(event.target.files);
});

loadSample.addEventListener("click", () => {
  clearError();
  loadedGraphs = [{ name: "sample", graph: normalizeGraph(sampleData) }];
  activeIndex = 0;
  renderTabs();
  renderBoard(loadedGraphs[0].graph);
});

window.addEventListener("DOMContentLoaded", () => {
  tryAutoLoad();
});
