---
name: learn
description: Update memory bank or rules with new information or requirements.
---

# Learn Prompt

## Goal

Capture and store new learnings from recently implemented feature in memory bank or coding rules.

## Resources

### Rules

Individual rule files in `docs/rules/`:

- docs/rules/api-design.md
- docs/rules/concurrency.md
- docs/rules/craft.md
- docs/rules/ddd.md
- docs/rules/functional.md
- docs/rules/observability.md
- docs/rules/performance.md

### Documentation (conventions & architecture)

- docs/conventions/backend.md
- docs/conventions/frontend.md
- docs/conventions/design.md
- docs/conventions/testing.md
- docs/conventions/coding-assertions.md
- docs/architecture/overview.md
- docs/architecture/stack.md

## Steps

**IMPORTANT** : Documentation and Rules MIGHT BE OUTDATED! This is why we need to update them.

1. Gather all changes that have been made.
2. List discrepancies between initial task and final implementation.
3. Ask user which new rules/conventions to save and in which files.
4. Imagine implementing those new rationals, give user a preview of which files.
5. Write new rules on confirmation.

## Example

```text
Here are the changes that differ from initial rules / documentation:
-
-
-
-
...

## Suggestions

### docs/conventions/design.md
+ "Incorporate user feedback into the design process."
- "Design should be based solely on initial requirements."

### docs/rules/craft.md
+ "New rule about X"

...
```
