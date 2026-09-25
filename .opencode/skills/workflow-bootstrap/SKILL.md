---
name: workflow-bootstrap
description: Recover collaboration context at session start. Load workflow profile, project context, recent progress, active work, and feature registry. Use when starting a new session or when context may be stale.
---

# Workflow: Session Bootstrap

## Read Order

1. `docs/ai/WORKFLOW_PROFILE.md` — collaboration posture
2. `docs/ai/PROJECT_CONTEXT.md` — stable project snapshot
3. `docs/ai/PROGRESS_LOG.md` (recent entries only)
4. `docs/ai/ACTIVE_WORK.md` — current short backlog
5. `docs/ai/FEATURE_REGISTRY.md` (In Progress / Planned)
6. `docs/ai/TECH_DEBT.md` (Open)
7. Task-specific design doc (if linked by ACTIVE_WORK or explicitly named by user)

## Profile gate

- If profile `status` is `unconfigured`: ask meta-question once (configure now / skip / later). Catalog: `docs/ai/templates/WORKFLOW_PRESETS.md`.
- If `deferred`: do not re-ask unless user says `reconfigure workflow`.
- If `configured` or `default-applied`: apply Effective behavior silently.

## Output

After reading, produce a summary covering:
- Current workflow posture (preset / key dimensions)
- Current project phase and milestone
- Active feature/slice being worked on
- Last verification state
- First concrete next action with verification command

## When to Use

- Starting a new agent session
- Context has become stale (long idle, many external changes)
- User explicitly asks "where were we?"
