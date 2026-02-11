---
name: Memory Manager
description: Invoked when Writing or Updating memory bank documentation.
model: GPT-5.2-Codex (copilot)
---

# Refresh Memory Bank

Means to create or update the documentation files that make up the memory bank of the project.

Only change existing files if there is REAL CHANGES in the codebase, do not change files just to reformat or reword things.

If $ARGUMENTS is provided, it will be the module/folder to analyze, otherwise use project root.

## Output Structure

Documentation files are organized under `docs/` following this structure:

| Template Category | Output Path |
|-------------------|-------------|
| Architecture (overview) | `docs/architecture/overview.md` |
| Stack | `docs/architecture/stack.md` |
| Codebase Structure | `docs/architecture/codebase-structure.md` |
| Backend conventions | `docs/conventions/backend.md` |
| Frontend conventions | `docs/conventions/frontend.md` |
| Design system | `docs/conventions/design.md` |
| Testing strategy | `docs/conventions/testing.md` |
| Coding assertions | `docs/conventions/coding-assertions.md` |
| Deployment | `docs/deployment/deployment.md` |
| API documentation | `docs/api/endpoints.md` |
| Project brief | `docs/project/brief.md` |
| Coding rules | `docs/rules/*.md` |

## Steps

1. Check if documentation already exists in the `docs/` folder structure above:
   1. If exists, update it with newer information.
   2. If not exist, create them from scratch.
2. Determine modules to analyze:
   1. If $ARGUMENTS is provided, it will be the module/folder to analyze.
   2. If not provided, use project root.
3. Provide the modules list to USER.
4. **Wait for user approval** before proceeding.
5. For each module, identify which files to create/update using the output structure above.
6. Spawn a new task agent for each template file to analyze the codebase and fill its own template (in parallel) based on rules below.
7. Output the generated files in proper dir.

## Rules

- "?" means optional, do not add section if not applicable
- Templates give optional sections, feel free to add or remove sections as needed
- ZERO DUPLICATION: Focus only on the sections in template to avoid duplication across files
- No minor versions in libs (e.g. `Next.js 15.3.4` → `Next.js 15` )
- Templates follow clear separation of concerns
- For config files (e.g. `package.json`, API schema etc...), please include relative based path using "@" (do not surrounded path with backticks)
- SUPER SHORT explicit and concise bullet points
- Mention code using backticks
