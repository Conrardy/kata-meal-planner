# AIDD Requirements

> **Prerequisites**: Dependencies required before starting development with AIDD (AI-Driven Development).

---

## Quick Install

```bash
# Run the installation script
./install.sh
```

---

## System Requirements

| Requirement | Version | Notes |
|-------------|---------|-------|
| **Node.js** | 20+ | LTS recommended (via nvm) |
| **Git** | 2.x+ | Version control |
| **Bash/Zsh** | - | Shell for scripts |
| **nvm** | latest | Node version manager |

---

## Package Manager

```bash
npm install -g pnpm@10.11.0
```

---

## CLI Tools

### Required

| Tool | macOS | Windows | Purpose |
|------|-------|---------|---------|
| **GitHub CLI** | `brew install gh` | `winget install GitHub.cli` | GitHub operations |
| **Claude CLI** | `npm install -g @anthropic-ai/claude-code` | Same | AI assistant |

> **Note**: Tree functionality is provided by custom `aidd/assets/scripts/cli/tree.sh` script

---

## Node.js Dependencies

Install from `aidd/` directory:

```bash
cd aidd && pnpm install
```

### Installed Packages

| Package | Purpose |
|---------|---------|
| `@musistudio/claude-code-router` | Claude Code router UI |
| `ccusage` | Claude usage monitoring |
| `lefthook` | Git hooks manager |
| `repomix` | Repository mixing |

---

## MCP Servers (Model Context Protocol)

MCPs extend AI capabilities. Configured in `aidd/supports/mcps/`.

| MCP | Package | Purpose |
|-----|---------|---------|
| **Playwright** | `@playwright/mcp` | Browser automation |
| **Sequential Thinking** | `@modelcontextprotocol/server-sequential-thinking` | Structured reasoning |
| **Context7** | HTTP endpoint | Library documentation |

---

## Environment Variables

```bash
export ANTHROPIC_API_KEY="your-api-key"
```

---

## Quick Start

```bash
# 1. Run installer
./install.sh

# 2. Set API key
export ANTHROPIC_API_KEY="your-key"

# 3. Start Claude
claude

# 4. Or use aliases
source aidd/supports/aliases.sh
cc  # Claude with permissions bypass
```

---

## Verification

```bash
# Check installations
nvm --version       # Node version manager
node --version      # Should be 20+
pnpm --version      # Should be 10.11.0
gh --version        # Should be installed
claude --version    # Should be installed
```
