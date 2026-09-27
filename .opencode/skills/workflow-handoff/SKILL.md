---
name: workflow-handoff
description: Prepare a complete handoff for session switch or end-of-day. Create session note, append progress entry, mark incomplete slices, and provide next action.
---

# Workflow: Session Handoff

## Trigger

User indicates handoff, session switch, or end of meaningful batch.

## Steps

1. **Write session note** using `docs/ai/templates/session-note.template.md`
   - TL;DR summary for fast recovery
   - Decisions made and rationale
   - Work completed
   - Open issues

2. **Append progress entry** to `docs/ai/PROGRESS_LOG.md`
   - Scope: `<FeatureID>/<SliceID>`
   - Completed items
   - Verification commands and results
   - Docs updated
   - Next action

3. **Mark incomplete slices**
   - Set status to Blocked or Deferred with explicit reason
   - Document unblock condition and next check date

4. **Provide first concrete next action**
   - One clear step the next session should start with

## Output

- Session note saved to `docs/ai/sessions/`
- Progress log updated
- Incomplete slice status updated in relevant docs
