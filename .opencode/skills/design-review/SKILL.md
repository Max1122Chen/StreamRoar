---
name: design-review
description: Review a Design Spec for completeness and implementation readiness before coding. Focus on ambiguity, missing decisions, ownership/lifetime gaps, failure gaps, integration risk, and overengineering — not typos or formatting. Use after engineering-design for Level 2+ work, or when asked to review a design.
---

# Design Review

This is **not** code review.

| Review type | Primary question |
|-------------|------------------|
| Design review | Is this system design complete, unambiguous, implementable, and maintainable? |
| Code review | Does the implementation match intent and quality bars? |

Use after a Design Spec exists for Complexity Level **2+** (and always for Level **3**).  
Authority for gates: `docs/ai/templates/DOC_GOVERNANCE.md`.  
Companion skill: `engineering-design`.

## How to review

Actively hunt for:

- ambiguity and missing decisions
- hidden assumptions
- contradictory requirements
- undefined ownership / incomplete lifetime
- missing failure behavior
- integration / migration gaps
- unverifiable acceptance criteria
- unnecessary complexity

Do **not** spend the review on spelling, formatting, or template cosmetics unless they hide real ambiguity.

## Checklist (apply what is relevant)

### Requirements and scope

- Does the design solve the real problem?
- Are critical requirements missing?
- Are proposed solutions mislabeled as requirements?
- Are In / Non-goals clear?

### Architecture

- Are responsibilities and boundaries clear?
- Are dependencies reasonable? Any cycles or responsibility leakage?

### Ownership / lifetime / state

- Who owns state? Who creates/destroys/mutates?
- Are states, transitions, and invariants clear when relevant?

### API semantics

- Are ownership, mutation, error, and sync/async semantics clear enough to implement without guessing?

### Failure and integration

- Are important failure paths considered?
- Does the design fit the existing system?
- Are compatibility / migration issues addressed?

### Alternatives and complexity

- Were meaningful alternatives considered?
- Are trade-offs real (not ceremonial)?
- Is the design over-engineered? What can be deleted?

### Verification and readiness

- Can acceptance criteria be observed/tested?
- Is verification strategy present?
- Is the design **implementation-ready**?

## Implementation readiness verdict

End with one verdict:

| Verdict | Meaning |
|---------|---------|
| `Ready` | Large implementation may proceed (still honor profile autonomy pauses) |
| `Ready with deferred items` | Proceed only if deferred items are explicit, accepted, and non-blocking |
| `Not ready` | Block large coding; list concrete gaps to resolve |

For `Not ready`, list actionable gaps (not vague “needs more detail”).

## Output format

```markdown
## Design Review

- **Design doc:** <path>
- **Complexity level:** L0–L3
- **Verdict:** Ready | Ready with deferred items | Not ready

### Findings
- [severity] <finding> — <why it matters> — <suggested resolution>

### Deferred / accepted risks
- <item> — owner/acceptance

### Next action
- <single concrete next step>
```

## Profile interaction

- `prototype-partner`: keep reviews lean; still block on unclear ownership/public contracts for L3
- `serious-engineering` / `commercial` / `executor-tight`: enforce readiness for L2+/L3; executor may be terse but cannot skip architectural safety gaps
- `learning-mentor`: explain *why* findings matter; still require readiness for L2+/L3

## Domain neutrality

Keep findings domain-agnostic. Do not inject product-domain assumptions not present in the design.
