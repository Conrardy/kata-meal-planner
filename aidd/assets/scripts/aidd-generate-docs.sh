#!/usr/bin/env bash

# Generate AIDD documentation files
# Usage: aidd-generate-docs.sh [OPTIONS]

set -e

CURRENT_DIR="$(pwd)"

# Detect if running in dev (cli/assets) or installed (aidd/assets) structure
if [ -d "$CURRENT_DIR/cli/assets" ]; then
  # Dev structure
  CLI="$CURRENT_DIR/cli/assets/scripts/cli"
  TEMPLATE_DIR="$CURRENT_DIR/prompts/templates"
else
  # Installed structure
  CLI="$CURRENT_DIR/aidd/assets/scripts/cli"
  TEMPLATE_DIR="$CURRENT_DIR/aidd/prompts/templates"
fi

DOCS_DIR="$CURRENT_DIR/docs"

# Component flags
GENERATE_TREE=false
GENERATE_MEMORY_BANK=false
GENERATE_RULES=false

# Parse command line arguments
for arg in "$@"; do
  case $arg in
    --tree)
      GENERATE_TREE=true
      shift
      ;;
    --memory-bank)
      GENERATE_MEMORY_BANK=true
      shift
      ;;
    --rules)
      GENERATE_RULES=true
      shift
      ;;
    --docs-dir=*)
      DOCS_DIR="${arg#*=}"
      shift
      ;;
    --all)
      GENERATE_TREE=true
      GENERATE_MEMORY_BANK=true
      GENERATE_RULES=true
      shift
      ;;
    --help|-h)
      echo "Usage: aidd-generate-docs.sh [OPTIONS]"
      echo ""
      echo "Generate AIDD documentation files with selective component generation"
      echo ""
      echo "OPTIONS:"
      echo "  --tree           Generate project tree section"
      echo "  --memory-bank    Generate memory bank section"
      echo "  --rules          Generate coding rules section"
      echo "  --all            Generate all components (default if no flags)"
      echo "      --docs-dir=<path>  Override docs directory (default: ./docs)"
      echo "  -h, --help       Show this help message"
      echo ""
      echo "Examples:"
      echo "  aidd-generate-docs.sh                     # Generate all components"
      echo "  aidd-generate-docs.sh --all               # Generate all components"
      echo "  aidd-generate-docs.sh --tree              # Only project tree"
      echo "  aidd-generate-docs.sh --memory-bank       # Only memory bank"
      echo "  aidd-generate-docs.sh --rules             # Only rules"
      echo "  aidd-generate-docs.sh --rules --memory-bank # Rules and memory bank"
      exit 0
      ;;
    *)
      echo "Unknown option: $arg"
      echo "Use --help for usage information"
      exit 1
      ;;
  esac
done

# If no specific flags are provided, generate all components
if [ "$GENERATE_TREE" = false ] && [ "$GENERATE_MEMORY_BANK" = false ] && [ "$GENERATE_RULES" = false ]; then
  GENERATE_TREE=true
  GENERATE_MEMORY_BANK=true
  GENERATE_RULES=true
fi

# Ensure docs directory exists
mkdir -p "$DOCS_DIR"

# Generate components based on flags
if [ "$GENERATE_TREE" = true ]; then
  echo "Generating project tree..."
  sh "$CLI/tree.sh" --scan-dir="$CURRENT_DIR/.." --output-file="$DOCS_DIR/tree.txt"
else
  echo "Skipping tree generation"
  # Remove existing tree.txt to prevent it from being included
  rm -f "$DOCS_DIR/tree.txt"
fi

if [ "$GENERATE_RULES" = true ]; then
  echo "Generating rules..."
  node "$CLI/merge.cjs" \
    --input-dir="$DOCS_DIR/rules" \
    --output-file="$DOCS_DIR/rules.md"
else
  echo "Skipping rules generation"
  rm -f "$DOCS_DIR/rules.md"
fi

if [ "$GENERATE_MEMORY_BANK" = true ]; then
   echo "Generating AGENTS.md with memory bank..."
   mkdir -p "$DOCS_DIR"


   AGENTS_TEMPLATE="$TEMPLATE_DIR/memory-bank/AGENTS.md"
   AGENTS_HEADERS="$DOCS_DIR/AGENTS_HEADERS.md"

   rm -vf "$DOCS_DIR/AGENTS.md"

   if [ ! -f "$AGENTS_HEADERS" ]; then
     if [ ! -f "$AGENTS_TEMPLATE" ]; then
       echo "❌ Missing AGENTS template at $AGENTS_TEMPLATE"
       exit 1
     fi

     cp "$AGENTS_TEMPLATE" "$AGENTS_HEADERS"
     echo "✅ Created AGENTS headers from template at $AGENTS_HEADERS"
   fi

   cp "$AGENTS_HEADERS" "$DOCS_DIR/AGENTS.md"

   if [ -d "$DOCS_DIR/memory-bank" ] && [ -n "$(find "$DOCS_DIR/memory-bank" -type f \( -name "*.md" -o -name "*.mdc" -o -name "*.mmd" -o -name "*.txt" \) 2>/dev/null)" ]; then
      # Merge all memory-bank entries except the top-level AGENTS.md (already provided by header)
      node "$CLI/merge.cjs" --input-dir="$DOCS_DIR/memory-bank" --ignore="AGENTS.md" --output-file=/dev/stdout >> "$DOCS_DIR/AGENTS.md"
   fi

   if [ -L "$CURRENT_DIR/AGENTS.md" ]; then
      echo "✅ AGENTS.md already symlinked, leaving as-is"
   elif [ -f "$CURRENT_DIR/AGENTS.md" ]; then
      BACKUP_PATH="$CURRENT_DIR/AGENTS.md.backup-$(date +%Y%m%d-%H%M%S)"
      mv "$CURRENT_DIR/AGENTS.md" "$BACKUP_PATH"
      ln -s "docs/AGENTS.md" "$CURRENT_DIR/AGENTS.md"
      echo "✅ Backed up to: $BACKUP_PATH"
   else
      ln -s "docs/AGENTS.md" "$CURRENT_DIR/AGENTS.md"
   fi
   echo "✅ AGENTS.md synced: AGENTS.md -> docs/AGENTS.md"

   if [ -d "$DOCS_DIR/memory-bank" ]; then
     ls -1 "$DOCS_DIR/memory-bank" 2>/dev/null | while IFS= read -r entry; do
       [ -d "$DOCS_DIR/memory-bank/$entry" ] || continue
       node "$CLI/merge.cjs" --input-dir="$DOCS_DIR/memory-bank/$entry" --output-file="$DOCS_DIR/$entry.md"
     done
   fi
else
   echo "Skipping memory bank generation"
fi
