# Document Governance

Last updated: 2026-09-22
Status: Active

## 1) Purpose

Ensure any contributor (human or agent) can answer:
1. What problem is being solved?
2. What is done vs pending?
3. What is the next verifiable action?
4. Why was work paused, deferred, or cancelled?

## 2) Document types

- Roadmap: sequencing and priorities
- Design Spec: scope, approach, risks
- Implementation Plan: slices and acceptance checks
- ADR: architectural trade-off decision
- Progress Log: factual timeline
- Bug Record: defect lifecycle and regression safety

## 3) ID conventions

- Feature: `<DOMAIN>-F<nn>`
- Slice: `<FeatureID>-S<nn>`
- Bug: `BUG-<DOMAIN>-<nnn>`
- ADR: `ADR-<yyyyMMdd>-<nn>`

New features must be registered in `docs/ai/FEATURE_REGISTRY.md` before implementation work.

Domain codes are ALL CAPS, short, and stable (examples: `TMPL`, `CORE`, `API`, `UI`, `DATA`, `TEST`, `INFRA`). Not a closed enum — add domains as needed, do not reuse retired codes.

## 3.1) File naming (required)

Long collaboration docs that belong to a Feature use:

```text
<FEATURE_ID>_<SLUG>_<DOC_TYPE>.md
```

Rules:
- Filename is **ALL CAPS** (Feature ID, slug, and doc type)
- `<FEATURE_ID>` must match the registered Feature ID exactly
- `<SLUG>` is `A-Z0-9_` only (words separated by `_`)
- `<DOC_TYPE>` is one of: `DESIGN` | `IMPLEMENTATION` | `ROADMAP` | `REFACTOR_PLAN`

Examples:
- `TMPL-F03_DOC_NAMING_AND_BUCKETING_DESIGN.md`
- `CORE-F01_EVENT_BUS_IMPLEMENTATION.md`
- `RND-F02_MODERN_RHI_REFACTOR_PLAN.md`

ADR and Bug filenames:

```text
ADR-<yyyyMMdd>-<nn>_<SLUG>.md
BUG-<DOMAIN>-<nnn>_<SLUG>.md
```

Also ALL CAPS.

## 3.2) Domain bucketing (required)

Design Spec, Implementation Plan, Roadmap, Refactor Plan, and ADR files **must** live under:

```text
docs/ai/<DOMAIN>/
```

where `<DOMAIN>` matches the Feature/ADR/Bug domain segment (ALL CAPS).

Do **not** place these long docs in `docs/ai/` root.

Bug records may use either:
- `docs/ai/bugs/`
- `docs/ai/<DOMAIN>/bugs/`

### Root exceptions

Only core collaboration truth stays at `docs/ai/` root, for example:
`README.md`, `WORKFLOW_PROFILE.md`, `PROJECT_CONTEXT.md`, `BOOTSTRAP_DIGEST.md`, `ACTIVE_WORK.md`, `FEATURE_REGISTRY.md`, `PROGRESS_LOG.md`, `TECH_DEBT.md`, `WORKING_WITH_AI.md`

### Other reserved folders

- `docs/ai/templates/` — templates and governance only
- `docs/ai/sessions/` — session notes (`YYYY-MM-DD-<topic>.md`; topic may be lowercase)

## 4) Required Meta block for long docs

Design, Roadmap, Implementation, and ADR files should include:

```markdown
## Meta
- **ID:** <FeatureID or N/A>
- **Status:** Draft | Planned | In Progress | Review | Done | Blocked | Deferred | Cancelled | Snapshot | Archived | Reference
- **Owner:** <name>
- **Last updated:** YYYY-MM-DD
- **Related:** [link1](./...), [link2](./...)
```

## 5) Agent doc trust

Planning sources:
- `WORKFLOW_PROFILE.md` (for posture; not backlog)
- `ACTIVE_WORK.md`
- `FEATURE_REGISTRY.md` (In Progress / Planned)
- `TECH_DEBT.md` (Open)
- Recent `PROGRESS_LOG.md`
- Code/tests/verify scripts

Reference-only sources:
- old roadmaps
- Snapshot/Archived/Reference docs
- stale checklist fragments

If docs conflict with code/tests, code/tests win.

## 5.1) Workflow profile

- Deploy-time posture lives in `docs/ai/WORKFLOW_PROFILE.md`.
- Preset catalog: `templates/WORKFLOW_PRESETS.md`.
- If profile status is `unconfigured`, ask meta-question once before large work.
- Profile may tune strictness/autonomy/role/language/verification/design ceremony; it cannot disable commit gate, planning trust tiers, or Level-3 architectural safety checks.

## 5.2) Design complexity levels

Agents must assess complexity before large edits and state the level.

| Level | Typical scope | Required path |
|-------|---------------|---------------|
| **L0 Trivial** | typo, obvious one-line fix, local constant | Implement → Verify (no design review) |
| **L1 Local** | single-module logic | Short design optional; Feature ID if substantial |
| **L2 Feature** | multi-module, API, state, or lifetime | Design Spec → Design Review → Implementation Plan → Implement |
| **L3 Architectural** | core abstraction, cross-module deps, persistence, concurrency, public API, migration, major refactor | Full Design → Design Review → **Implementation Readiness** → Plan → Implement |

Skills:
- Design behavior: `.opencode/skills/engineering-design/SKILL.md`
- Design review: `.opencode/skills/design-review/SKILL.md`

Target flow (skip stages allowed only by level):

```text
Request → Discovery → Complexity Assessment
  → Design → Design Review → Implementation Readiness
  → Implementation → Verification → Done
```

Examples:
- L0: `Request → Implement → Verify`
- L2: `Request → Discovery → Design → Review → Plan → Implement → Verify`
- L3: same as L2 with explicit readiness gate before coding

## 5.3) Implementation readiness gate

For L2+ (always for L3), do not start large-scale coding until Design Review verdict is `Ready` or `Ready with deferred items`, and the Design Spec readiness checklist is satisfied:

- requirements understood; constraints identified
- relevant existing architecture inspected
- responsibilities and boundaries defined; key dependencies understood
- ownership/lifetime and major state/invariants resolved **or** marked N/A with reason
- API semantics sufficiently defined; important failure modes considered
- alternatives considered where meaningful; integration impact understood
- verification strategy exists
- unresolved questions resolved **or** explicitly accepted/deferred

`Draft` design never authorizes large-scale coding.  
`Not ready` blocks large implementation until gaps are closed.

## 6) Slice Done Definition (DoD)

### Docs DoD
- Progress entry appended
- Feature and slice status synchronized
- Design/Plan updated when scope or status changed
- For L2+: design review verdict recorded; implementation slices trace to design decisions
- ADR updated for meaningful architectural trade-off
- Bug record updated for defect fixes
- New design/impl docs follow §3.1 naming and §3.2 domain bucketing

### Engineering DoD
- Verification command executed and recorded (per profile `verification_bar`)
- No unrecorded blocking defect discovered during work
- Public API changes reflected in callers or explicitly documented
- Implementation matches reviewed design decisions (or documents approved deviation)

## 7) Handoff requirements

On handoff/session switch:
1. Create or update a session note
2. Append one progress entry
3. Mark incomplete slice as Blocked/Deferred with reason and unblock condition
4. Provide first concrete next action

## 8) Work boundary

After completing a meaningful batch:
- finish DoD updates
- propose "prepare commit"
- do not start unrelated new feature work unless explicitly requested
