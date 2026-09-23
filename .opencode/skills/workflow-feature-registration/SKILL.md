---
name: workflow-feature-registration
description: Register a new Feature ID before implementation work. Follow the ID scheme and create or update design/plan docs. Use when starting a new feature or refactor.
---

# Workflow: Feature Registration

## Steps

1. **Check existing registry** — read `docs/ai/FEATURE_REGISTRY.md` for the next available number in the target domain
2. **Assign Feature ID** — `<DOMAIN>-F<nn>` (e.g. `CORE-F01`, `API-F02`)
3. **Assess complexity** — L0–L3 per `docs/ai/templates/DOC_GOVERNANCE.md` §5.2
4. **Register in FEATURE_REGISTRY.md** — add row with Feature ID, Title, Domain, Status (Draft), Design Doc link (TBD), Owner
5. **Create Design Spec** (L1+ when substantial; required L2+) — copy template to:
   `docs/ai/<DOMAIN>/<FEATURE_ID>_<SLUG>_DESIGN.md` (ALL CAPS; domain bucket required)
   Use `.opencode/skills/engineering-design/SKILL.md`
6. **Design Review** (L2+) — `.opencode/skills/design-review/SKILL.md`; record verdict
7. **When ready for implementation** — create:
   `docs/ai/<DOMAIN>/<FEATURE_ID>_<SLUG>_IMPLEMENTATION.md` with slices that trace to design decisions
   Do not large-code while Design is `Draft` or review is `Not ready`

## Slice IDs

- `<FeatureID>-S<nn>` (e.g. `CORE-F01-S01`)
- Each slice should be independently verifiable and map to a design decision when L2+

## Status Guidance

- Draft: concept exists, not implementation-ready
- Planned: approved for implementation (readiness met when required)
- In Progress: active development
- Review: awaiting validation/review
- Done: accepted
- Blocked / Deferred / Cancelled: include reason and follow-up condition
