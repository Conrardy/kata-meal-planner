# Prompts Documentation

This document provides an overview of all prompts organized by category.

## CHAT

| Name | Description | Argument Hint |
|------|-------------|---------------|
| [evaluate](chat/evaluate.md) | Evaluate AI's answers and improve the next resp... | N/A |
| [md](chat/md-output.md) | Format AI's response into markdown. | N/A |
| [research](chat/research.md) | Create a detailed, hierarchical outline for a c... | N/A |

## examples

No files found.
## IDE

| Name | Description | Argument Hint |
|------|-------------|---------------|
| `00_flows`/[auto_implement](ide/00_flows/auto_implement.md) | Automate all steps of the AI-driven development... | N/A |
| `01_onboard`/[generate_agent](ide/01_onboard/generate_agent.md) | Generates a customized agent based on user-defi... | N/A |
| `01_onboard`/[generate_prompt](ide/01_onboard/generate_prompt.md) | Generate optimized, action-oriented prompts usi... | The command details to generate the prompt for |
| `01_onboard`/[generate_rules](ide/01_onboard/generate_rules.md) | Generate or modify coding rules for the project... | module (optional) |
| `02_context`/[create-user-stories](ide/02_context/create_user_stories.md) | Create user stories through iterative questioning | Feature description or requirements for user story generation |
| `02_context`/[extract_flow](ide/02_context/extract_flow.md) | Extract step-by-step implementation flows from ... | N/A |
| `03_plan`/[elaborate](ide/03_plan/elaborate.md) | Create and refine feature requests through inte... | N/A |
| `03_plan`/[image_extract_details](ide/03_plan/image_extract_details.md) | Analyze image to identify and extract main comp... | the image to analyze |
| `03_plan`/[plan](ide/03_plan/plan.md) | Generate technical implementation plans from re... | requirements (GitHub issue URL or raw text) |
| `04_code`/[assert](ide/04_code/assert.md) | Assert that a feature must work as intended. | Feature or behavior to assert and validate |
| `04_code`/[implement](ide/04_code/implement.md) | Implement plan following project rules with val... | The technical plan to implement |
| `04_code`/[run_projection](ide/04_code/run_projection.md) | Project the solution you mentioned on a part of... | N/A |
| `05_review`/[review_code](ide/05_review/review_code.md) | Ensure code quality and rules compliance | N/A |
| `05_review`/[review_functional](ide/05_review/review_functional.md) | Use this agent when you need to browse current ... | The technical plan to base the review on |
| `06_tests`/[list_untested](ide/06_tests/list_untested.md) | List all untested behaviors in codebase | N/A |
| `06_tests`/[tdd](ide/06_tests/tdd.md) | Use this agent when explicitly asked to perform... | N/A |
| `06_tests`/[write](ide/06_tests/write.md) | Iterate on test creation and improvement until ... | N/A |
| `07_documentation`/[jira_info](ide/07_documentation/jira_info.md) | Get JIRA ticket info from current branch | Jira URL or number |
| `07_documentation`/[learn](ide/07_documentation/learn.md) | Update memory bank or rules with new informatio... | N/A |
| `07_documentation`/[mermaid](ide/07_documentation/mermaid.md) | Generate high-quality Mermaid diagrams from mar... | N/A |
| `07_documentation`/[refresh_memory_bank](ide/07_documentation/refresh_memory_bank.md) | Refresh (aka create or update) the memory bank ... | <module> (optional) - default is project root |
| `08_deploy`/[commit](ide/08_deploy/commit.md) | Create git commit with proper message format | N/A |
| `08_deploy`/[create-github-pull-request](ide/08_deploy/create_github_pull_request.md) | Create GitHub PR with filled template | N/A |
| `08_deploy`/[tag](ide/08_deploy/tag.md) | Create and push git tag with semantic versioning | N/A |
| `09_refactor`/[performance](ide/09_refactor/performance.md) | Optimize code for better performance | N/A |
| `09_refactor`/[security-refactor](ide/09_refactor/security.md) | Identify and fix security vulnerabilities | N/A |
| `10_maintenance`/[codebase-audit](ide/10_maintenance/audit.md) | Perform deep codebase analysis for technical de... | Scope or module to audit (optional - defaults to full codebase) |
| `10_maintenance`/[debug](ide/10_maintenance/debug.md) | Debug issue to find root cause. | N/A |
| `10_maintenance`/[new_issue](ide/10_maintenance/new_issue.md) | Create GitHub issues with interactive template ... | Describe the problem you want to create an issue for |
| `10_maintenance`/[reflect_issue](ide/10_maintenance/reflect_issue.md) | Reflect on possible sources, identify most like... | N/A |
| `10_maintenance`/[reproduce](ide/10_maintenance/reproduce.md) | Fix bugs with test-driven workflow from issue t... | Bug description or issue number |

## SYSTEM

| Name | Description | Argument Hint |
|------|-------------|---------------|
| [elite-webdesigner](system/elite-webdesigner.md) | Transform user vision into fully-coded, visuall... | N/A |
| [Technical Strategy Agent](system/technical-strategy-agent.md) | Helps engineering teams create comprehensive te... | N/A |

## TEMPLATES

| Group | Template |
|-------|----------|
| `.github` | [issue_template.md](templates/.github/issue_template.md) |
| `.github` | [pull_request_template.md](templates/.github/pull_request_template.md) |
| `.github` | [release_template.md](templates/.github/release_template.md) |
| `-` | [agent.md](templates/agent.md) |
| `-` | [code_review.md](templates/code_review.md) |
| `-` | [command.md](templates/command.md) |
| `-` | [CONTRIBUTING.md](templates/CONTRIBUTING.md) |
| `-` | [flow.md](templates/flow.md) |
| `-` | [issue.md](templates/issue.md) |
| `memory-bank` | [AGENTS_COORDINATION.md](templates/memory-bank/AGENTS_COORDINATION.md) |
| `memory-bank` | [AGENTS.md](templates/memory-bank/AGENTS.md) |
| `memory-bank` | [CODEBASE_STRUCTURE.md](templates/memory-bank/CODEBASE_STRUCTURE.md) |
| `memory-bank` | [DEPLOYMENT.md](templates/memory-bank/infra/DEPLOYMENT.md) |
| `memory-bank` | [PR_TEMPLATE.md](templates/memory-bank/PR_TEMPLATE.md) |
| `memory-bank` | [PROJECT_BRIEF.md](templates/memory-bank/PROJECT_BRIEF.md) |
| `memory-bank` | [STACK.md](templates/memory-bank/STACK.md) |
| `-` | [plan.md](templates/plan.md) |
| `-` | [prd.md](templates/prd.md) |
| `prompts` | [example.md](templates/prompts/example.md) |
| `prompts` | [prompt.assistant.txt](templates/prompts/prompt.assistant.txt) |
| `prompts` | [prompt.system.txt](templates/prompts/prompt.system.txt) |
| `prompts` | [prompt.user.txt](templates/prompts/prompt.user.txt) |
| `-` | [README.md](templates/README.md) |
| `-` | [rule.md](templates/rule.md) |
| `-` | [slide.md](templates/slide.md) |
| `starters` | [claude.md](templates/starters/claude.md) |
| `starters` | [typescript-next-tailwind.md](templates/starters/typescript-next-tailwind.md) |
| `sub-agents` | [checker.md](templates/sub-agents/checker.md) |
| `sub-agents` | [dev.md](templates/sub-agents/dev.md) |
| `sub-agents` | [memory-manager.md](templates/sub-agents/memory-manager.md) |
| `-` | [task.md](templates/task.md) |
| `-` | [tech-choice.md](templates/tech-choice.md) |
| `-` | [user_story.md](templates/user_story.md) |

