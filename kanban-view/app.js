const fileInput = document.getElementById("fileInput");
const loadSample = document.getElementById("loadSample");
const downloadJson = document.getElementById("downloadJson");
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

function setDownloadState(entry) {
  if (!downloadJson) return;
  if (!entry) {
    downloadJson.hidden = true;
    return;
  }

  downloadJson.hidden = false;
  downloadJson.textContent = entry.source === "server" ? "Download JSON" : "Download updated JSON";
}

function downloadGraph(entry) {
  if (!entry || !entry.graph) return;
  const payload = JSON.stringify({ mikado_graph: entry.graph }, null, 2);
  const blob = new Blob([payload], { type: "application/json" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `${entry.name}.json`;
  link.click();
  URL.revokeObjectURL(url);
}

async function persistStatusChange(entry, nodeId, status) {
  if (!entry || entry.source !== "server") return true;

  try {
    const response = await fetch(
      `/api/graphs/${encodeURIComponent(entry.name)}/nodes/${encodeURIComponent(nodeId)}/status`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status })
      }
    );

    if (!response.ok) {
      showError(`Failed to update ${nodeId} (${response.status})`);
      return false;
    }

    const payload = await response.json();
    if (payload && payload.graph) {
      entry.graph = payload.graph;
    }
    return true;
  } catch (error) {
    showError(`Failed to update ${nodeId}: ${error.message || "network error"}`);
    return false;
  }
}

function getActiveEntry() {
  if (activeIndex < 0) return null;
  return loadedGraphs[activeIndex] || null;
}

function isNodeBlocked(node, nodeMap) {
  return node.depends_on.some((dep) => nodeMap.get(dep)?.status !== "done");
}

async function updateNodeStatus(nodeId, status) {
  const entry = getActiveEntry();
  if (!entry || !entry.graph || !entry.graph.nodes) return;

  const node = entry.graph.nodes[nodeId];
  if (!node || node.status === status) return;

  const didPersist = await persistStatusChange(entry, nodeId, status);
  if (!didPersist) return;

  const now = new Date().toISOString();
  node.status = status;
  node.updated_at = now;
  entry.graph.updated_at = now;
  renderBoard(entry.graph);
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
  setDownloadState(loadedGraphs[index]);
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
    return isNodeBlocked(node, nodeMap);
  }).length;
  const readyCount = nodes.filter((node) => {
    if (node.status === "done") return false;
    return !isNodeBlocked(node, nodeMap);
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

      const actions = document.createElement("div");
      actions.className = "status-actions";

      if (isNodeBlocked(node, nodeMap)) {
        const blockedNote = document.createElement("span");
        blockedNote.className = "status-blocked";
        blockedNote.textContent = "blocked by deps";
        actions.appendChild(blockedNote);
      } else {
        statusOrder.forEach((statusOption) => {
          const button = document.createElement("button");
          button.type = "button";
          button.className =
            statusOption === node.status ? "status-button active" : "status-button";
          button.textContent = statusOption.replace(/-/g, " ");
          button.addEventListener("click", () => updateNodeStatus(node.id, statusOption));
          actions.appendChild(button);
        });
      }

      card.append(cardTitle, cardDesc, tagList, meta, actions);
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

    loadedGraphs = graphs.map((entry) => ({ ...entry, source: "server" }));
    activeIndex = 0;
    renderTabs();
    setDownloadState(loadedGraphs[0]);
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
  setDownloadState(null);
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
          graph: normalized.graph,
          source: "local"
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
  setDownloadState(loadedGraphs[0]);
  renderBoard(loadedGraphs[0].graph);
}

fileInput.addEventListener("change", (event) => {
  handleFiles(event.target.files);
});

loadSample.addEventListener("click", () => {
  clearError();
  loadedGraphs = [{ name: "sample", graph: normalizeGraph(sampleData), source: "local" }];
  activeIndex = 0;
  renderTabs();
  setDownloadState(loadedGraphs[0]);
  renderBoard(loadedGraphs[0].graph);
});

if (downloadJson) {
  downloadJson.addEventListener("click", () => downloadGraph(getActiveEntry()));
}

window.addEventListener("DOMContentLoaded", () => {
  tryAutoLoad();
});
