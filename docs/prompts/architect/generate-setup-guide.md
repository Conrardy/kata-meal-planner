
**Goal**: Generate a **step-by-step setup guide** in **Markdown format** for a project. The guide will be used by an automated agent to configure a development environment from scratch.

**Requirements**:
- **Project Details**: [Language: Python], [Framework: FastAPI], [Target OS: Ubuntu 22.04].
- **Sections**:
  1. **Prerequisites**: List all required tools (e.g., Docker, Python 3.10), with version constraints and download links.
  2. **Installation Steps**: Commands to install dependencies, CLI tools, and project-specific packages. Flag optional tools.
  3. **Configuration**: Environment variables, config files, and permissions.
  4. **Verification**: Commands/tests to confirm the setup works (e.g., `pytest`, `docker compose up`).
  5. **Troubleshooting**: Common errors and fixes (e.g., permission issues, version conflicts).
- **Constraints**:
  - Use **technical, concise language**.
  - Limit each step to **3 sentences max**.
  - Include **code blocks** for commands.
  - Add **notes** for non-standard configurations.
- **Output Format**:
  ```markdown
  # Project Setup Guide: [Project Name]
  ## Prerequisites
  - [Tool]: [Version] ([Download Link])
  ## Installation
  1. Step: `command`
  ## Verification
  Run `test_command` to validate.
  ## Troubleshooting
  - Error: [Description] → Fix: [Solution]
