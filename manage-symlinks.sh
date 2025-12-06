#!/usr/bin/env bash

# Cross-platform symlink management script
# Analyzes and creates symlinks based on symlinks.yml configuration file
# Usage: ./manage-symlinks.sh [config_file_path]
#
# NOTE: Target paths in symlinks.yml are RELATIVE to the source location

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default config file
CONFIG_FILE="${1:-symlinks.yml}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$SCRIPT_DIR"

# Check if config file exists
if [[ ! -f "$CONFIG_FILE" ]]; then
    echo -e "${RED}Error: Config file '$CONFIG_FILE' not found${NC}" >&2
    exit 1
fi

# Detect platform
detect_platform() {
    case "$(uname -s)" in
        Linux*)     echo "linux" ;;
        Darwin*)    echo "macos" ;;
        CYGWIN*)    echo "windows" ;;
        MINGW*)     echo "windows" ;;
        MSYS*)      echo "windows" ;;
        *)          echo "unknown" ;;
    esac
}

PLATFORM=$(detect_platform)

# Check if a path is a symlink
is_symlink() {
    local path="$1"
    if [[ -L "$path" ]]; then
        return 0
    fi
    return 1
}

# Get symlink target
get_symlink_target() {
    local path="$1"
    if is_symlink "$path"; then
        readlink "$path" || echo ""
    else
        echo ""
    fi
}

# Create symlink (cross-platform)
create_symlink() {
    local source="$1"
    local target="$2"
    local source_dir
    
    # Get absolute paths
    source_dir=$(dirname "$source")
    source_abs="$REPO_ROOT/$source"
    target_abs="$REPO_ROOT/$source_dir/$target"
    
    # Normalize paths
    source_abs=$(cd "$(dirname "$source_abs")" && pwd)/$(basename "$source_abs")
    target_abs=$(cd "$(dirname "$target_abs")" && pwd)/$(basename "$target_abs")
    
    # Create parent directory if needed
    mkdir -p "$(dirname "$source_abs")"
    
    # Check if target exists
    if [[ ! -e "$target_abs" ]]; then
        echo -e "${YELLOW}Warning: Target '$target_abs' does not exist${NC}"
        return 1
    fi
    
    # Remove existing file/symlink if it exists and is different
    if [[ -e "$source_abs" ]] || [[ -L "$source_abs" ]]; then
        if is_symlink "$source_abs"; then
            current_target=$(readlink "$source_abs")
            if [[ "$current_target" == "$target" ]] || [[ "$current_target" == "$target_abs" ]]; then
                return 0  # Already correctly linked
            fi
        fi
        rm -f "$source_abs"
    fi
    
    # Create relative symlink
    ln -s "$target" "$source_abs"
    return 0
}

# Parse YAML config file (simple parser for this specific format)
parse_config() {
    local config_file="$1"
    local in_symlinks=false
    local current_source=""
    local current_target=""
    local current_desc=""
    local entry_started=false
    
    while IFS= read -r line || [[ -n "$line" ]]; do
        # Skip comments and empty lines
        [[ "$line" =~ ^[[:space:]]*# ]] && continue
        [[ -z "${line// }" ]] && continue
        
        # Check if we're in symlinks section
        if [[ "$line" =~ ^symlinks: ]]; then
            in_symlinks=true
            continue
        fi
        
        if [[ "$in_symlinks" == true ]]; then
            # Check if we hit another top-level key (end of symlinks section)
            if [[ "$line" =~ ^[^[:space:]] ]] && [[ ! "$line" =~ ^[[:space:]]*- ]]; then
                # Process last entry before breaking
                if [[ -n "$current_source" ]] && [[ -n "$current_target" ]]; then
                    echo "$current_source|$current_target|$current_desc"
                fi
                break
            fi
            
            # Check for new entry start (handles both "- source:" inline and standalone "-")
            if [[ "$line" =~ ^[[:space:]]*-[[:space:]]+source:[[:space:]]*(.+)$ ]]; then
                # Process previous entry if it exists
                if [[ "$entry_started" == true ]] && [[ -n "$current_source" ]] && [[ -n "$current_target" ]]; then
                    echo "$current_source|$current_target|$current_desc"
                fi
                # Reset for new entry and capture source from same line
                current_source="${BASH_REMATCH[1]}"
                current_target=""
                current_desc=""
                entry_started=true
                continue
            elif [[ "$line" =~ ^[[:space:]]*- ]]; then
                # Process previous entry if it exists
                if [[ "$entry_started" == true ]] && [[ -n "$current_source" ]] && [[ -n "$current_target" ]]; then
                    echo "$current_source|$current_target|$current_desc"
                fi
                # Reset for new entry
                current_source=""
                current_target=""
                current_desc=""
                entry_started=true
                continue
            fi
            
            # Parse source (with indentation, for separate line format)
            if [[ "$line" =~ ^[[:space:]]+source:[[:space:]]*(.+)$ ]]; then
                current_source="${BASH_REMATCH[1]}"
            fi
            
            # Parse target (with indentation)
            if [[ "$line" =~ ^[[:space:]]+target:[[:space:]]*(.+)$ ]]; then
                current_target="${BASH_REMATCH[1]}"
            fi
            
            # Parse description (with indentation)
            if [[ "$line" =~ ^[[:space:]]+description:[[:space:]]*(.+)$ ]]; then
                current_desc="${BASH_REMATCH[1]}"
            fi
        fi
    done < "$config_file"
    
    # Process last entry
    if [[ -n "$current_source" ]] && [[ -n "$current_target" ]]; then
        echo "$current_source|$current_target|$current_desc"
    fi
}

# Main function
main() {
    echo -e "${BLUE}=== Symlink Management Script ===${NC}"
    echo -e "Platform: ${GREEN}$PLATFORM${NC}"
    echo -e "Config file: ${GREEN}$CONFIG_FILE${NC}"
    echo -e "Repository root: ${GREEN}$REPO_ROOT${NC}"
    echo ""
    
    cd "$REPO_ROOT"
    
    local total=0
    local created=0
    local existing=0
    local errors=0
    local skipped=0
    
    # Parse config and process each symlink
    while IFS='|' read -r source target description; do
        # Trim whitespace
        source=$(echo "$source" | xargs)
        target=$(echo "$target" | xargs)
        description=$(echo "$description" | xargs)
        
        [[ -z "$source" ]] && continue
        
        total=$((total + 1))
        
        source_path="$REPO_ROOT/$source"
        
        echo -e "${BLUE}[$total]${NC} $source"
        if [[ -n "$description" ]]; then
            echo -e "     ${YELLOW}→${NC} $description"
        fi
        
        # Check if symlink exists
        if is_symlink "$source_path"; then
            current_target=$(get_symlink_target "$source_path")
            echo -e "     ${GREEN}✓${NC} Symlink exists: $source → $current_target"
            
            # Verify target matches
            if [[ "$current_target" == "$target" ]] || [[ "$(readlink -f "$source_path")" == "$(readlink -f "$REPO_ROOT/$(dirname "$source")/$target")" ]]; then
                echo -e "     ${GREEN}✓${NC} Target matches configuration"
                existing=$((existing + 1))
            else
                echo -e "     ${YELLOW}⚠${NC} Target mismatch (config: $target, actual: $current_target)"
                echo -e "     ${BLUE}→${NC} Updating symlink..."
                if create_symlink "$source" "$target"; then
                    created=$((created + 1))
                    echo -e "     ${GREEN}✓${NC} Symlink updated"
                else
                    errors=$((errors + 1))
                    echo -e "     ${RED}✗${NC} Failed to update symlink"
                fi
            fi
        elif [[ -e "$source_path" ]]; then
            echo -e "     ${YELLOW}⚠${NC} File exists but is not a symlink"
            echo -e "     ${BLUE}→${NC} Skipping (use --force to replace)"
            skipped=$((skipped + 1))
        else
            echo -e "     ${BLUE}→${NC} Creating symlink..."
            if create_symlink "$source" "$target"; then
                created=$((created + 1))
                echo -e "     ${GREEN}✓${NC} Symlink created: $source → $target"
            else
                errors=$((errors + 1))
                echo -e "     ${RED}✗${NC} Failed to create symlink"
            fi
        fi
        echo ""
    done < <(parse_config "$CONFIG_FILE")
    
    # Summary
    echo -e "${BLUE}=== Summary ===${NC}"
    echo -e "Total symlinks in config: ${BLUE}$total${NC}"
    echo -e "Created: ${GREEN}$created${NC}"
    echo -e "Already exist: ${GREEN}$existing${NC}"
    echo -e "Skipped: ${YELLOW}$skipped${NC}"
    echo -e "Errors: ${RED}$errors${NC}"
    
    if [[ $errors -gt 0 ]]; then
        exit 1
    fi
}

# Run main function
main "$@"
