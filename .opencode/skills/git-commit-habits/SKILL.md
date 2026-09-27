---
name: git-commit-habits
description: Generate git commit messages using Conventional Commits. Use when the user asks to commit, write a commit message, review staged changes for a commit, or when preparing a git commit. Never create commits unless the user explicitly requests it.
---

# Git Commit Habits

## When to Apply

- User explicitly asks to commit, draft a commit message, or review staged changes for committing
- User asks for help splitting commits or writing PR-related commit text

## Hard Rules

- **Never run `git commit` or create a commit** unless the user explicitly asks you to commit
- **Never use `--no-verify`, `--amend`, or force-push** unless the user explicitly requests it
- **Never use `git commit --amend`** on your own — only when the user explicitly asks to amend
- **Never split into multiple commits** unless the user explicitly asks to split — default to one commit per request
- **Never commit files that likely contain secrets** (`.env`, credentials, keys); warn the user instead
- Before committing (when asked), run `git status`, `git diff`, and `git log` to understand context and match recent message style in the repo

## Message Format (Conventional Commits)

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<optional scope>): <short summary>

<body — optional, bullet list>
```

### Types (common)

- `feat` — new feature
- `fix` — bug fix
- `docs` — documentation only
- `style` — formatting, no logic change
- `refactor` — code change that is not feat/fix
- `perf` — performance improvement
- `test` — tests only
- `build` — build system or dependencies
- `ci` — CI configuration
- `chore` — maintenance, tooling, misc

### Summary line

- Imperative mood, lowercase after the colon segment (unless proper nouns)
- No trailing period
- Keep the summary under ~72 characters when possible
- **Default language: English**
- **Include a scope** in most commits: `feat(dns-relay): ...`, `fix(renderer): ...`
- Match scope names to module, crate, or top-level package when obvious; follow existing `git log` in the repo otherwise

### Body (when needed)

- **Small changes**: subject line only is fine; add a body only when it helps (non-obvious rationale, breaking change, multiple logical parts)
- **Footers / signatures**: optional — use `Closes #n`, `Fixes #n`, or `Co-authored-by` when useful; no strict requirement
- Use **Markdown unordered lists**: each item on its own line, starting with `-`
- One idea per bullet; be specific about *why*, not just *what*
- Wrap at ~72 characters per line when practical

## Workflow When User Asks to Commit

1. Run `git status` and `git diff` (staged and unstaged as relevant)
2. Run `git log -5 --oneline` to align with repo tone
3. Draft the message; show it to the user if they did not ask you to commit immediately
4. Stage only relevant files; never stage unrelated drive-by changes unless the user agrees
5. Commit only after explicit confirmation or a direct "commit this" request

## What to Avoid

- Vague messages: `fix stuff`, `update`, `WIP`
- Mixing unrelated changes in one commit
- Commit messages in non-English for small/trivial changes (reserve bilingual for large scope)
- Bullets without `-` prefix or run-on paragraphs instead of a list
