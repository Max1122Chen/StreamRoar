# Initialization Guide

> StreamRoar 已于 2026-09-23 按本指南部署（serious-engineering + zh + dual-track，多 Agent 适配均保留）。下文保留作参考与再部署 checklist。

Use this guide to adapt this template to any new repository.

## 0) Workflow profile (do this first)

This template ships recommended collaboration presets.

1. Open / create `docs/ai/WORKFLOW_PROFILE.md`.
2. If `status` is `unconfigured`, the agent must ask the **meta-question** once:
   - **configure now** — pick a preset from `docs/ai/templates/WORKFLOW_PRESETS.md` (or customize dimensions)
   - **skip** — apply `serious-engineering` defaults
   - **later** — mark `deferred` and stop asking until `reconfigure workflow`
3. Persist the result in `WORKFLOW_PROFILE.md` and summarize effective behavior.

Why this matters: presets control strictness, ask-vs-act autonomy, agent role, docs language, and verification bar for later sessions.

## 1) Choose agent adapters

Keep only the adapter files for agents you use:

| Agent | File(s) to keep |
|-------|-----------------|
| Any agent | `docs/ai/` (always keep) |
| **opencode** | `AGENTS.md`, `opencode.json`, `.opencode/` |
| **Cursor** | `.cursor/rules/` |
| **Claude Code** | `CLAUDE.md` |
| **GitHub Copilot** | `.github/copilot-instructions.md` |
| **Cline** | `.clinerules` |
| **Windsurf** | `.windsurfrules` |

Delete the adapter files for agents you do **not** use.

## 2) Fill project metadata

Edit:
- `docs/ai/PROJECT_CONTEXT.md`
- `docs/ai/BOOTSTRAP_DIGEST.md`
- `docs/ai/WORKING_WITH_AI.md`
- `docs/ai/WORKFLOW_PROFILE.md` (if not completed in step 0)

Required customization:
- project mission
- architecture summary
- verify/test commands
- owner and collaboration language preferences (or inherit from profile `docs_language`)

## 3) Define domain vocabulary

In your governance docs, define domain codes used by IDs:
- examples: `CORE`, `API`, `UI`, `DATA`, `TEST`, `INFRA`
- keep them short, stable, and **ALL CAPS**

Then register your first feature in `docs/ai/FEATURE_REGISTRY.md`.

Create the domain bucket for new design docs:

```text
docs/ai/<DOMAIN>/<FEATURE_ID>_<SLUG>_DESIGN.md
```

Filenames are ALL CAPS and must include the Feature ID.

## 4) Pick documentation topology

Prefer the value already stored in `WORKFLOW_PROFILE.md`:

- **Single-track mode:** one docs system for product + implementation
- **Dual-track mode:** product truth in another doc tree, implementation workflow in `docs/ai/`

If dual-track, record the product-truth path in both `PROJECT_CONTEXT.md` and `WORKFLOW_PROFILE.md`.

## 5) Set planning trust defaults

Confirm these are treated as high trust:
- `ACTIVE_WORK.md`
- `FEATURE_REGISTRY.md`
- `TECH_DEBT.md`
- recent `PROGRESS_LOG.md`
- code/tests/verify

Mark old plans and snapshots as reference-only.

## 6) Enable/disable optional discipline

- `implementation_discipline` in profile controls whether cleanup/reuse rules are enforced.
- For Cursor: keep `.cursor/rules/implementation-discipline.mdc` aligned with profile (`on`/`off`).
- For other adapters: follow the corresponding section in `AGENTS.md` / profile Effective behavior.

## 7) First bootstrap check

Run a dry bootstrap prompt:

```text
Read docs/ai/WORKFLOW_PROFILE.md, PROJECT_CONTEXT.md, ACTIVE_WORK.md, FEATURE_REGISTRY.md, TECH_DEBT.md, and recent PROGRESS_LOG.md. If profile is unconfigured, ask the meta-question. Otherwise summarize current posture and propose one next step with verification command.
```

If the answer references stale roadmap docs as backlog, refine your trust-tier rule and README guidance.

## 8) Minimum operating loop

1. Assess complexity (L0–L3); register Feature when substantial
2. Design / Design Review / Readiness (per level) + Plan
3. Implement by slices
4. Verify (per profile `verification_bar`)
5. Update progress and registry
6. Prepare commit (draft first, execute only with explicit approval)

## Reconfigure later

Say `reconfigure workflow` to re-run the meta-question and update `WORKFLOW_PROFILE.md`.
