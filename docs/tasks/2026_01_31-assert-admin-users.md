---
goal: Assert admin users feature
version: 1.0
date_created: 2026-01-31
last_updated: 2026-01-31
owner: AI Assistant
status: Completed
tags: [assert, admin, frontend, backend, testing]
---

# Task [Assert admin users feature]

Validate US-024 admin users feature against coding assertions.

## Assertions to validate

- [x] Admin-only endpoints return 403 for non-admin users.
- [x] Admin can list users (id, username).
- [x] Admin can create user; duplicate username/weak password returns validation errors.
- [x] Admin UI route `/admin/users` is guarded and not accessible to non-admins.
- [x] Sidebar shows Admin link only when `isAdmin` is true.
- [x] Admin users page shows list and handles form success/error states.
- [x] All required build/test checks pass (backend + frontend).
