# Workflow Profile

Last updated: 2026-09-23  
Status: `configured`  
<!-- Allowed status: unconfigured | configured | deferred | default-applied -->

This file is the **single runtime truth** for collaboration posture in this repo.  
Agents must read it during bootstrap. If `status` is `unconfigured`, ask the meta-question once before large work.

## Meta-question (ask once when unconfigured)

> This template includes recommended workflow presets. Configuring them sets how strict the agent is, how often it asks before acting, what role it plays, and which language docs use.
>
> Do you want to configure now?
> 1. **configure now** — pick a preset (or answer a few dimensions)
> 2. **skip** — apply `serious-engineering` defaults and continue
> 3. **later** — mark deferred; do not ask again until user says `reconfigure workflow`

## Current selection

| Field | Value |
|-------|-------|
| `preset_id` | `serious-engineering` |
| `repo_posture` | `serious-engineering` |
| `autonomy` | `propose-then-act` |
| `agent_role` | `partner` |
| `docs_language` | `zh` |
| `docs_topology` | `dual-track` |
| `verification_bar` | `smoke-required` |
| `implementation_discipline` | `on` |
| `product_truth_path` | `docs/` + root `AGENTS.md` |

## Effective behavior (derived)

- Challenge skipped workflow: `high`
- Pre-flight before large feature/refactor: `required`
- Pause for approval when: scope expands beyond the approved slice; irreversible/destructive git ops; broad multi-module refactors without Feature ID / Design Spec
- Default explanation depth: `normal` (partner tone; 中文简述结果、验证与未决项)
- New docs/progress language: `zh`（专有名词与 ID 可保留英文）
- Engineering DoD bar: L2+ 需 Design Spec → Design Review → readiness → Implementation Plan；L3 架构安全门禁不可跳过；验证按 `docs/Verification.md` 选择路径，至少完成与改动相称的 smoke（Unity 导入/编译或 Launch 相关路径）
- Docs topology: 产品/架构真相在 `docs/` 与 `AGENTS.md`；实施协作（Feature/Slice/进度/债务）在 `docs/ai/`
- Daily fixes: 仍遵循 `AGENTS.md`——日常修复不强制 ADR/多文档同步；仅 L2+ 或跨模块取舍进入 `docs/ai` 仪式

## Non-negotiable constraints (never disabled by profile)

- Prepare commit ≠ execute commit
- Plan only from trusted sources (`ACTIVE_WORK`, registry In Progress/Planned, open TECH_DEBT, recent progress, code/tests)
- `Draft` design does not authorize large-scale coding
- Level-3 architectural safety checks (ownership/contracts/migration/failure when relevant) cannot be skipped by preset

## Reconfigure

User phrase: `reconfigure workflow`  
Agent action: re-run meta-question + preset/dimension flow; update this file; summarize what changed.
