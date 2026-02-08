---
name: contributing
description: Project contribution guidelines template
argument-hint: N/A
---

# Contributing to MealPlanner

Thanks for helping improve MealPlanner. This guide keeps contributions simple, consistent, and fast to review.

Project vision and scope live in the Memory Bank. Start here:
- [docs/memory-bank/PROJECT_BRIEF.md](docs/memory-bank/PROJECT_BRIEF.md)
- [docs/memory-bank/STACK.md](docs/memory-bank/STACK.md)

## 📐 Ways to Contribute

- Report bugs and suggest features (open a GitHub issue; include steps, expected vs actual, screenshots if helpful)
- Improve documentation (fix typos, clarify instructions, add missing references)
- Implement changes (small, focused PRs tied to an issue)

Be respectful and pragmatic. Avoid scope creep; focus on the smallest valuable change.

## 💠 Onboarding

### Useful Documentation
- Commit conventions: [docs/COMMIT.md](docs/COMMIT.md)
- PR template and expectations: [docs/PR_TEMPLATE.md](docs/PR_TEMPLATE.md)
- Project agents context: [docs/AGENTS.md](docs/AGENTS.md)

### Environment Setup (optional but recommended)
If you plan to run project scripts or generate docs:
- Node.js 18+
- pnpm

Install aidd workspace deps:

```bash
# macOS/Linux
cd aidd
pnpm install
```

```powershell
# Windows PowerShell
Set-Location aidd
pnpm install
```

## 🧠 Memory Bank Updates
If your change adds patterns, structure, or non-trivial docs, refresh the Memory Bank.

```bash
# macOS/Linux
sh aidd/assets/scripts/aidd-generate-docs.sh --memory-bank
```

```powershell
# Windows PowerShell
./aidd/assets/scripts/aidd-generate-docs.ps1 --memory-bank
```

Only commit real changes (avoid reformat-only diffs). See the Memory Bank process in [.github/instructions/memory-bank.instructions.md](.github/instructions/memory-bank.instructions.md).

## 🌲 Branch Naming

Use short, purpose-driven names. Include issue ID when available.
- `feat/<scope>-<short>` (e.g., `feat/recipes-filtering`)
- `fix/<scope>-<short>` (e.g., `fix/shopping-list-dedupe`)
- `chore/<scope>-<short>` (e.g., `chore/docs-links`)

## 👌 Commit Guidelines

Follow [docs/COMMIT.md](docs/COMMIT.md). Keep commits atomic and descriptive. Example types: `feat:`, `fix:`, `docs:`, `chore:`, `refactor:`.

## 🔁 Pull Requests

- One focused change per PR; link the related issue
- Fill out [docs/PR_TEMPLATE.md](docs/PR_TEMPLATE.md)
- Add screenshots or terminal output when it clarifies behavior
- Describe trade-offs and alternatives considered when non-trivial

## 🧪 Testing Guidelines

- Prefer small, fast, deterministic tests
- When testing UI or functional components, do not mock functional components
- If your change affects documentation generation or scripts, run the relevant script and include a short note of the result

## 🐛 Filing Issues

Provide clear reproduction steps, expected vs actual behavior, and environment details. Propose a minimal scope for a first PR if possible.

## 🔧 Local Utilities (optional)

- Windows symlink helpers: [manage-symlinks.ps1](manage-symlinks.ps1)
- Unix symlink helpers: [manage-symlinks.sh](manage-symlinks.sh)
- Repo tree previews: `aidd/assets/scripts/cli/tree.sh` or `aidd/assets/scripts/cli/tree.ps1`

## 🚀 Publishing (Maintainers Only)

- Ensure Memory Bank is refreshed (see above)
- Verify docs build/scripts succeed
- Use conventional release notes aligned with commit messages
