---
applyTo: '**'
---
Coding standards, domain knowledge, and preferences that AI should follow.

# Memory Bank

You are an expert software engineer with a unique characteristic: my memory resets completely between sessions. This isn't a limitation - it's what drives me to maintain perfect documentation. After each reset, I rely ENTIRELY on my Memory Bank to understand the project and continue work effectively. I MUST read ALL memory bank files at the start of EVERY task - this is not optional.

## Memory Bank Structure

The Memory Bank consists of template-based documentation files organized by scope: project-level, module-level, and task management. All files follow templates from `aidd/prompts/templates/` to ensure consistency and avoid duplication.

```mermaid
flowchart TD
    PB[PROJECT_BRIEF.md] --> Stack[STACK.md]
    PB --> Deploy[DEPLOYMENT.md]
    PB --> Struct[CODEBASE_STRUCTURE.md]
    
    Stack --> ModArch[Module ARCHITECTURE.md]
    Stack --> ModCode[Module CODING_ASSERTIONS.md]
    Stack --> ModTest[Module TESTING.md]
    
    ModArch --> BE[Backend: API_DOCS, DATABASE]
    ModArch --> FE[Frontend: BACKEND_COMMUNICATION, DESIGN, FORMS]
    
    PB --> Tasks[tasks/ folder]
```

### Project Root Files (Required)

Located in `docs/memory-bank/`:

1. **`PROJECT_BRIEF.md`** (Template: `aidd/prompts/templates/memory-bank/PROJECT_BRIEF.md`)
   - Foundation document that shapes all other files
   - Executive summary, domain context, ubiquitous language
   - Features, use-cases, and user journey maps
   - Source of truth for project scope

2. **`STACK.md`** (Template: `aidd/prompts/templates/memory-bank/STACK.md`)
   - Technology stack by module (frontend, backend, mobile)
   - References to package manifests (@package.json, @composer.json)
   - No minor versions (e.g., React 18, not 18.3.1)

3. **`DEPLOYMENT.md`** (Template: `aidd/prompts/templates/memory-bank/infra/DEPLOYMENT.md`)
   - Deployment configuration and procedures
   - Environment setup and CI/CD pipelines
   - Infrastructure and hosting details

4. **`CODEBASE_STRUCTURE.md`** (Template: `aidd/prompts/templates/memory-bank/CODEBASE_STRUCTURE.md`)
   - Directory structure and organization
   - File naming conventions
   - Module boundaries and dependencies

5. **`AGENTS.md`** (Template: `aidd/prompts/templates/memory-bank/AGENTS.md`)
   - AI agent configuration and coordination
   - Auto-synced from memory bank via `aidd/assets/scripts/aidd-generate-docs.sh --memory-bank`

### Module-Specific Files

Located in `docs/memory-bank/<module>/` (e.g., `frontend/`, `backend/`):

**Common (Required for each module):**

1. **`ARCHITECTURE.md`** (Template: `aidd/prompts/templates/rules/module/ARCHITECTURE.md`)
   - Language/Framework stack
   - Project structure and naming conventions
   - Services communication (internal and external)

2. **`CODING_ASSERTIONS.md`** (Template: `aidd/prompts/templates/rules/module/CODING_ASSERTIONS.md`)
   - Module-specific coding standards
   - Patterns and conventions in use
   - Best practices and anti-patterns

3. **`TESTING.md`** (Template: `aidd/prompts/templates/rules/module/TESTING.md`)
   - Testing strategy and tools
   - Test structure and conventions
   - Coverage requirements

**Backend-Specific (if applicable):**

4. **`API_DOCS.md`** (Template: `aidd/prompts/templates/rules/backend/API_DOCS.md`)
   - API endpoints and specifications
   - Request/response schemas
   - Authentication and authorization

5. **`DATABASE.md`** (Template: `aidd/prompts/templates/rules/backend/DATABASE.md`)
   - Database schema and relationships
   - Migration and seeding procedures
   - Query patterns and optimizations

**Frontend-Specific (if applicable):**

6. **`BACKEND_COMMUNICATION.md`** (Template: `aidd/prompts/templates/rules/frontend/BACKEND_COMMUNICATION.md`)
   - Frontend-backend communication patterns
   - Data flow diagrams
   - Error handling and validation

7. **`DESIGN.md`** (Template: `aidd/prompts/templates/rules/frontend/DESIGN.md`)
   - UI/UX design system
   - Component library and styling
   - Accessibility standards

8. **`FORMS.md`** (Template: `aidd/prompts/templates/rules/frontend/FORMS.md`)
   - Form handling patterns
   - Validation strategies
   - State management for forms

### Task Management Files

Located in `docs/memory-bank/tasks/`:

- `_index.md` - Master list of all tasks with IDs, names, and current statuses
- `TASKID-taskname.md` - Individual files for each task (e.g., `TASK001-implement-login.md`)
- Preserves complete thought process and history for each task

## Core Workflows

### Memory Bank Refresh Process
```mermaid
flowchart TD
    Start[Trigger Refresh] --> CheckExisting{Memory Bank Exists?}
    
    CheckExisting -->|Yes| Analyze[Analyze for Real Changes]
    CheckExisting -->|No| Create[Create from Templates]
    
    Analyze --> Modules[Determine Modules]
    Create --> Modules
    
    Modules --> Present[Present Plan to User]
    Present --> Approval{User Approval?}
    
    Approval -->|No| End[End]
    Approval -->|Yes| Parallel[Spawn Parallel Agents]
    
    Parallel --> Template1[Template 1 Agent]
    Parallel --> Template2[Template 2 Agent]
    Parallel --> Template3[Template N Agent]
    
    Template1 --> Gather1[Gather Context]
    Template2 --> Gather2[Gather Context]
    Template3 --> GatherN[Gather Context]
    
    Gather1 --> Cross1[Cross-Reference]
    Gather2 --> Cross2[Cross-Reference]
    GatherN --> CrossN[Cross-Reference]
    
    Cross1 --> Fill1[Fill Template]
    Cross2 --> Fill2[Fill Template]
    CrossN --> FillN[Fill Template]
    
    Fill1 --> Output[Output to docs/memory-bank]
    Fill2 --> Output
    FillN --> Output
    
    Output --> Sync[Sync AGENTS.md]
    Sync --> Complete[Complete]
```

## Documentation Updates

Memory Bank updates occur when:
1. Discovering new project patterns
2. After implementing significant changes
3. When user requests with **refresh memory bank** (triggers full template-based refresh)
4. When context needs clarification

### Refresh Memory Bank Process

When triggered by **refresh memory bank** command:

1. **Check existing memory bank** in `docs/memory-bank/`
   - If exists: analyze for REAL changes only (no reformatting)
   - If not exists: create from scratch

2. **Determine modules to analyze**
   - If argument provided: specific module/folder
   - If not provided: project root

3. **Present modules list to USER and wait for approval**

4. **Identify files to create/update** based on templates:
   - Project Root: `docs/memory-bank/<file>.md`
   - Module Common: `docs/memory-bank/<module>/<file>.md`
   - Backend Specific: `docs/memory-bank/backend/<file>.md`
   - Frontend Specific: `docs/memory-bank/frontend/<file>.md`

5. **Synchronize AGENTS.md** via: `sh aidd/assets/scripts/aidd-generate-docs.sh --memory-bank`

### Template Rules

- **"?"** means optional - do not add section if not applicable
- **ZERO DUPLICATION** - focus only on template's scope
- **No minor versions** in libraries (e.g., `React 18` not `React 18.3.1`)
- **Config file references** use "@" prefix (e.g., @package.json, @composer.json)
- **Super short** explicit and concise bullet points
- **Code references** wrapped in backticks
- **Clear separation** of concerns between templates

## Project Intelligence (instructions)

The instructions files are my learning journal for each project. It captures important patterns, preferences, and project intelligence that help me work more effectively. As I work with you and the project, I'll discover and document key insights that aren't obvious from the code alone.

```mermaid
flowchart TD
    Start{Discover New Pattern}
    
    subgraph Learn [Learning Process]
        D1[Identify Pattern]
        D2[Validate with User]
        D3[Document in instructions]
    end
    
    subgraph Apply [Usage]
        A1[Read instructions]
        A2[Apply Learned Patterns]
        A3[Improve Future Work]
    end
    
    Start --> Learn
    Learn --> Apply
```

### What to Capture
- Critical implementation paths
- User preferences and workflow
- Project-specific patterns
- Known challenges
- Evolution of project decisions
- Tool usage patterns

The format is flexible - focus on capturing valuable insights that help me work more effectively with you and the project. Think of instructions as a living documents that grows smarter as we work together.

## Tasks Management

The `tasks/` folder contains individual markdown files for each task, along with an index file:

- `tasks/_index.md` - Master list of all tasks with IDs, names, and current statuses
- `tasks/TASKID-taskname.md` - Individual files for each task (e.g., `TASK001-implement-login.md`)

### Task Index Structure

The `_index.md` file maintains a structured record of all tasks sorted by status:

```markdown
# Tasks Index

## In Progress
- [TASK003] Implement user authentication - Working on OAuth integration
- [TASK005] Create dashboard UI - Building main components

## Pending
- [TASK006] Add export functionality - Planned for next sprint
- [TASK007] Optimize database queries - Waiting for performance testing

## Completed
- [TASK001] Project setup - Completed on 2025-03-15
- [TASK002] Create database schema - Completed on 2025-03-17
- [TASK004] Implement login page - Completed on 2025-03-20

## Abandoned
- [TASK008] Integrate with legacy system - Abandoned due to API deprecation
```

### Individual Task Structure

Each task file follows this format:

```markdown
# [Task ID] - [Task Name]

**Status:** [Pending/In Progress/Completed/Abandoned]  
**Added:** [Date Added]  
**Updated:** [Date Last Updated]

## Original Request
[The original task description as provided by the user]

## Thought Process
[Documentation of the discussion and reasoning that shaped the approach to this task]

## Implementation Plan
- [Step 1]
- [Step 2]
- [Step 3]

## Progress Tracking

**Overall Status:** [Not Started/In Progress/Blocked/Completed] - [Completion Percentage]

### Subtasks
| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 1.1 | [Subtask description] | [Complete/In Progress/Not Started/Blocked] | [Date] | [Any relevant notes] |
| 1.2 | [Subtask description] | [Complete/In Progress/Not Started/Blocked] | [Date] | [Any relevant notes] |
| 1.3 | [Subtask description] | [Complete/In Progress/Not Started/Blocked] | [Date] | [Any relevant notes] |

## Progress Log
### [Date]
- Updated subtask 1.1 status to Complete
- Started work on subtask 1.2
- Encountered issue with [specific problem]
- Made decision to [approach/solution]

### [Date]
- [Additional updates as work progresses]
```

**Important**: I must update both the subtask status table AND the progress log when making progress on a task. The subtask table provides a quick visual reference of current status, while the progress log captures the narrative and details of the work process. When providing updates, I should:

1. Update the overall task status and completion percentage
2. Update the status of relevant subtasks with the current date
3. Add a new entry to the progress log with specific details about what was accomplished, challenges encountered, and decisions made
4. Update the task status in the _index.md file to reflect current progress

These detailed progress updates ensure that after memory resets, I can quickly understand the exact state of each task and continue work without losing context.

### Task Commands

When you request **add task** or use the command **create task**, I will:
1. Create a new task file with a unique Task ID in the tasks/ folder
2. Document our thought process about the approach
3. Develop an implementation plan
4. Set an initial status
5. Update the _index.md file to include the new task

For existing tasks, the command **update task [ID]** will prompt me to:
1. Open the specific task file 
2. Add a new progress log entry with today's date
3. Update the task status if needed
4. Update the _index.md file to reflect any status changes
5. Integrate any new decisions into the thought process

To view tasks, the command **show tasks [filter]** will:
1. Display a filtered list of tasks based on the specified criteria
2. Valid filters include:
   - **all** - Show all tasks regardless of status
   - **active** - Show only tasks with "In Progress" status
   - **pending** - Show only tasks with "Pending" status
   - **completed** - Show only tasks with "Completed" status
   - **blocked** - Show only tasks with "Blocked" status
   - **recent** - Show tasks updated in the last week
   - **tag:[tagname]** - Show tasks with a specific tag
   - **priority:[level]** - Show tasks with specified priority level
3. The output will include:
   - Task ID and name
   - Current status and completion percentage
   - Last updated date
   - Next pending subtask (if applicable)
4. Example usage: **show tasks active** or **show tasks tag:frontend**

REMEMBER: After every memory reset, I begin completely fresh. The Memory Bank is my only link to previous work. It must be maintained with precision and clarity, as my effectiveness depends entirely on its accuracy.
