#!/usr/bin/env bash

# AIDD Installation Script
# Detects OS and installs required dependencies for AI-Driven Development

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}╔════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║       AIDD Installation Script         ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════╝${NC}"
echo ""

# Detect OS
detect_os() {
    case "$(uname -s)" in
        Darwin*)    echo "macos" ;;
        Linux*)     echo "linux" ;;
        CYGWIN*|MINGW*|MSYS*) echo "windows" ;;
        *)          echo "unknown" ;;
    esac
}

OS=$(detect_os)
echo -e "${BLUE}→${NC} Detected OS: ${GREEN}$OS${NC}"
echo ""

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Install package manager tools
install_system_deps() {
    echo -e "${YELLOW}[1/5]${NC} Installing system dependencies..."
    
    case "$OS" in
        macos)
            if ! command_exists brew; then
                echo -e "${BLUE}→${NC} Installing Homebrew..."
                /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
            fi
            
            echo -e "${BLUE}→${NC} Installing gh via Homebrew..."
            brew install gh 2>/dev/null || brew upgrade gh 2>/dev/null || true
            ;;
            
        windows)
            echo -e "${BLUE}→${NC} Installing gh via winget..."
            if command_exists winget; then
                winget install --id GitHub.cli -e --silent 2>/dev/null || true
            else
                echo -e "${YELLOW}⚠${NC} winget not found. Please install GitHub CLI manually:"
                echo "   - GitHub CLI: https://cli.github.com/"
            fi
            ;;
            
        linux)
            echo -e "${BLUE}→${NC} Installing gh via apt..."
            if command_exists apt; then
                sudo apt update
                sudo apt install -y gh
            elif command_exists dnf; then
                sudo dnf install -y gh
            else
                echo -e "${YELLOW}⚠${NC} Package manager not detected. Please install gh manually."
            fi
            ;;
            
        *)
            echo -e "${RED}✗${NC} Unknown OS. Please install dependencies manually."
            exit 1
            ;;
    esac
    
    echo -e "${GREEN}✓${NC} System dependencies installed"
}

# Check Node.js version
check_node() {
    echo -e "${YELLOW}[2/5]${NC} Checking Node.js..."
    
    if ! command_exists node; then
        echo -e "${RED}✗${NC} Node.js not found!"
        echo ""
        echo "Please install Node.js 20+ using nvm: https://github.com/nvm-sh/nvm"
        echo "  nvm install 20"
        echo "  nvm use 20"
        exit 1
    fi
    
    NODE_VERSION=$(node -v | cut -d'v' -f2 | cut -d'.' -f1)
    if [ "$NODE_VERSION" -lt 20 ]; then
        echo -e "${RED}✗${NC} Node.js version must be 20+. Current: $(node -v)"
        exit 1
    fi
    
    echo -e "${GREEN}✓${NC} Node.js $(node -v) detected"
}

# Install pnpm
install_pnpm() {
    echo -e "${YELLOW}[3/5]${NC} Installing pnpm..."
    
    if ! command_exists pnpm; then
        npm install -g pnpm@10.11.0
    else
        PNPM_VERSION=$(pnpm -v)
        echo -e "${BLUE}→${NC} pnpm $PNPM_VERSION already installed"
    fi
    
    echo -e "${GREEN}✓${NC} pnpm ready"
}

# Install Claude CLI
install_claude_cli() {
    echo -e "${YELLOW}[4/5]${NC} Installing Claude CLI..."
    
    if ! command_exists claude; then
        npm install -g @anthropic-ai/claude-code
        echo -e "${GREEN}✓${NC} Claude CLI installed"
    else
        echo -e "${GREEN}✓${NC} Claude CLI already installed"
    fi
}

# Install AIDD dependencies
install_aidd_deps() {
    echo -e "${YELLOW}[5/5]${NC} Installing AIDD dependencies..."
    
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    AIDD_DIR="$SCRIPT_DIR/aidd"
    
    if [ -d "$AIDD_DIR" ] && [ -f "$AIDD_DIR/package.json" ]; then
        cd "$AIDD_DIR"
        pnpm install
        cd "$SCRIPT_DIR"
        echo -e "${GREEN}✓${NC} AIDD dependencies installed"
    else
        echo -e "${YELLOW}⚠${NC} aidd/ directory not found, skipping pnpm install"
    fi
}

# Setup symlinks
setup_symlinks() {
    echo ""
    echo -e "${BLUE}→${NC} Setting up symlinks..."
    
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    
    if [ -f "$SCRIPT_DIR/manage-symlinks.sh" ]; then
        bash "$SCRIPT_DIR/manage-symlinks.sh"
    else
        echo -e "${YELLOW}⚠${NC} manage-symlinks.sh not found, skipping"
    fi
}

# Main installation
main() {
    install_system_deps
    echo ""
    check_node
    echo ""
    install_pnpm
    echo ""
    install_claude_cli
    echo ""
    install_aidd_deps
    echo ""
    setup_symlinks
    
    echo ""
    echo -e "${GREEN}╔════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║      Installation Complete! 🎉         ║${NC}"
    echo -e "${GREEN}╚════════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${BLUE}Quick Start:${NC}"
    echo ""
    echo "  1. Set your Anthropic API key:"
    echo "     export ANTHROPIC_API_KEY=\"your-key\""
    echo ""
    echo "  2. Start coding with Claude:"
    echo "     claude"
    echo ""
    echo "  3. Or use aliases (source them first):"
    echo "     source aidd/supports/aliases.sh"
    echo "     cc  # Claude with bypass permissions"
    echo ""
    echo -e "${BLUE}Available Commands:${NC}"
    echo "  cd aidd    # move to aidd folder"
    echo "  pnpm run project:ccr:ui    # Claude Code Router UI"
    echo "  pnpm run project:cc:usage  # Check Claude usage"
    echo ""
}

# Run
main
