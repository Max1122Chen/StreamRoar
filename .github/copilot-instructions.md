# Agent Workflow Rules for GitHub Copilot — StreamRoar

## Dual-track docs

- Product / architecture: `docs/` + root `AGENTS.md`
- Collaboration (Feature / progress / debt): `docs/ai/`
- Adapters index: `docs/ai/ADAPTERS.md`

## Required reading order

1. `docs/ai/WORKFLOW_PROFILE.md` — if `unconfigured`, ask meta-question once
2. `docs/ai/PROJECT_CONTEXT.md`
3. `AGENTS.md`
4. `docs/ai/PROGRESS_LOG.md`
5. `docs/ai/ACTIVE_WORK.md`
6. `docs/ai/FEATURE_REGISTRY.md`

Preset catalog: `docs/ai/templates/WORKFLOW_PRESETS.md`.  
Phrase `reconfigure workflow` re-runs profile setup.

## Non-negotiable rules

- Never run `git commit` or push without explicit user instruction.
- Plan from trusted sources only; do not infer backlog from old roadmap or archived docs.
- Register Feature IDs in `FEATURE_REGISTRY.md` before substantial implementation.
- After meaningful work, update progress log, then propose "prepare commit" as draft only.
- Honor `WORKFLOW_PROFILE.md` (serious-engineering, zh, dual-track, smoke-required); it cannot disable the rules above.
- L2+: Design Spec → Design Review → readiness before large coding (`.opencode/skills/engineering-design`, `design-review`).
- `Draft` or `Not ready` does not authorize large-scale coding.
- Daily small fixes may stay lightweight per `AGENTS.md` (no forced ADR).

## ID conventions

- Feature: `<DOMAIN>-F<nn>` (domains: INFRA, UI, GAME, AUDIO, CFG, TOOL, ASSET)
- Slice: `<FeatureID>-S<nn>`
- Bug: `BUG-<DOMAIN>-<nnn>`
- ADR: `ADR-<yyyyMMdd>-<nn>`
- Design/Implementation: ALL CAPS under `docs/ai/<DOMAIN>/`
