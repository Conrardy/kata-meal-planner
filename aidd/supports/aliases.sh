#!/bin/sh

## IDE

alias cdx="codex -m gpt-5 --yolo -c model_reasoning_effort=\"high\""
alias oc="opencode"
alias cc="claude --dangerously-skip-permissions --permission-mode bypassPermissions --allowedTools \"*\""
alias ss="specstory run"

## Productivity
alias ll="ls -la"

## Custom
alias mm="md_merge"
alias def="delete_empty_folders"
alias osxu="osx_up"
alias nmu="npm_up"

## Git
alias gc="git commit --no-verify"
alias gp="git push --no-verify"

## AIDD (managed)

# AIDD_ROOT: Defaults to ~/.aidd
export AIDD_ROOT="${AIDD_ROOT:-${HOME}/.aidd}"

aidd_tree() {
  bash "$AIDD_ROOT/assets/scripts/cli/tree.sh" "$@"
}

## Functions

### Updates

npm_up () {
  npm outdated -g --depth=0 ; npm install -g npm ; npm -g update
}

osx_up () {
  brew update
  brew cleanup --prune=all
  brew outdated --greedy
  brew upgrade --greedy
  brew doctor
  npm_up
}

### Utils

delete_empty_folders () {
  find . -type d -empty -delete
}

### Markdown

md_merge () {
  find . -type f -print0 | \
    grep -zE '\.(md|mdx|rst)$' | \
    grep -vzZ 'all.md' | \
    sort -zV | \
    while IFS= read -r -d '' file; do
      echo "$file"  # Affiche uniquement le chemin du fichier
      {
        printf "\n---\nFile: %s\n---\n\n" "$file"
        cat "$file"
        printf "\n"  # Ajoute une ligne vide après chaque fichier
      } >> all.md
    done
}
