# Symlink Management

This repository uses symbolic links to maintain consistency across different directories. The symlinks are documented in `symlinks.yml` and can be managed using the provided scripts.

## Configuration

All symlinks are defined in `symlinks.yml` at the root of the repository. The file contains:

- Source paths (where the symlink should be created, relative to repo root)
- Target paths (what the symlink points to, **relative to source location**)
- Descriptions (what the symlink is used for)

## Scripts

### Bash Script (Linux/macOS/Git Bash)

```bash
# Analyze and create symlinks from default config (symlinks.yml)
./manage-symlinks.sh

# Use a custom config file
./manage-symlinks.sh path/to/custom-symlinks.yml
```

### PowerShell Script (Windows)

```powershell
# Analyze and create symlinks from default config (symlinks.yml)
.\manage-symlinks.ps1

# Use a custom config file
.\manage-symlinks.ps1 -ConfigFile path/to/custom-symlinks.yml
```

## Features

Both scripts:

- ✅ Analyze existing symlinks and compare with configuration
- ✅ Create missing symlinks automatically
- ✅ Detect and report mismatched symlinks
- ✅ Skip existing files that are not symlinks (to prevent data loss)
- ✅ Provide colored output for easy reading
- ✅ Show summary statistics

## Current Symlinks

The repository currently has 9 symlinks:

1. **Root level:**
   - `CLAUDE.md` → `AGENTS.md`

2. **`.claude/` directory:**
   - `.claude/agents` → `../docs/agents`
   - `.claude/commands/custom` → `../../docs/prompts`
   - `.claude/commands/flows` → `../../docs/flows`
   - `.claude/commands/ide` → `../../aidd/prompts/ide`

3. **`.github/` directory:**
   - `.github/commit-message-instructions.md` → `../docs/COMMIT.md`
   - `.github/pull-request-description-instructions.md` → `../docs/PR_TEMPLATE.md`
   - `.github/review-instructions.md` → `../aidd/prompts/templates/code_review.md`

4. **`.cursor/` directory:**
   - `.cursor/commands/ide` → `../../aidd/prompts/ide`

## Updating Symlinks

To add or modify symlinks:

1. Edit `symlinks.yml` to add/update entries
2. Run the appropriate script for your platform
3. The script will create missing symlinks and report any issues

## Notes

- The scripts exclude `node_modules` from analysis
- On Windows, you may need to run PowerShell as Administrator to create symlinks
- Git Bash on Windows can also use the bash script
- The scripts preserve existing symlinks that match the configuration
