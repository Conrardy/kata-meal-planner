# <AIDD>
printf "[AIDD] Refreshing documentation before commit...\n"

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null)"
if [ -z "$REPO_ROOT" ]; then
  printf "[AIDD] Unable to determine repository root. Skipping documentation refresh.\n"
  return 0 2>/dev/null || exit 0
fi

SCRIPT_PATH="$REPO_ROOT/aidd/assets/scripts/aidd-generate-docs.sh"

if [ ! -f "$SCRIPT_PATH" ]; then
  printf "[AIDD] Script not found at %s. Skipping documentation refresh.\n" "$SCRIPT_PATH"
  return 0 2>/dev/null || exit 0
fi

cd "$REPO_ROOT" || return 0 2>/dev/null || exit 0

if sh "$SCRIPT_PATH" --all; then
  git add AGENTS.md docs/ 2>/dev/null || true
  printf "[AIDD] Documentation refreshed and staged.\n"
else
  printf "[AIDD] Documentation script failed (commit will continue).\n"
fi
# </AIDD>
