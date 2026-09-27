# StreamRoar — Claude Code

Unity 基础设施 + 启动骨架。产品规则见根目录 `AGENTS.md`；协作流程见 `docs/ai/`。

## Session Start

按序阅读：
1. `docs/ai/WORKFLOW_PROFILE.md` — 若 `unconfigured`，只问一次 meta-question
2. `docs/ai/PROJECT_CONTEXT.md`
3. `AGENTS.md` — 所有权、验证、轻文档
4. `docs/ai/PROGRESS_LOG.md`（近期）
5. `docs/ai/ACTIVE_WORK.md`
6. `docs/ai/FEATURE_REGISTRY.md`（In Progress / Planned）

Preset：`docs/ai/templates/WORKFLOW_PRESETS.md`。  
重配：`reconfigure workflow`。多 Agent：`docs/ai/ADAPTERS.md`。

## Hard Rules

- 「Prepare commit」仅起草。未经明确指示不要 `git commit` / push。
- 只从可信源规划：`ACTIVE_WORK`、`FEATURE_REGISTRY`、`TECH_DEBT`、近期 `PROGRESS_LOG`、代码与验证。
- 旧 ROADMAP / Snapshot 仅参考，不自动变 backlog。
- 实质性工作：评估 L0–L3；注册 Feature；L2+ 先设计再大改。
- L2+：Design → Design Review → readiness → Implementation Plan。L3 readiness 必做。
- `Draft` / `Not ready` 不授权大规模编码。
- 有意义批次结束：更新 docs/ai，再提出 prepare commit。
- 遵守 `WORKFLOW_PROFILE.md`（当前：严肃工程 + 中文 + dual-track + smoke-required）。
- Profile 不能关闭 commit gate、信任分级或 L3 架构安全检查。
- 日常小修复仍按 `AGENTS.md`：不强制 ADR / 多文档同步。

## Engineering Design

- Skills: `.opencode/skills/engineering-design/SKILL.md`、`.opencode/skills/design-review/SKILL.md`
- 项目技能: `.agents/skills/infrastructure-usage`、`.agents/skills/pre-commit-review`
- Process: `docs/ai/templates/DOC_GOVERNANCE.md` §5.2–5.3
- 需求与方案分离；先检查现有架构

## ID Scheme

- Feature: `<DOMAIN>-F<nn>`（INFRA/UI/GAME/AUDIO/CFG/TOOL/ASSET）
- Slice: `<FeatureID>-S<nn>`
- Bug: `BUG-<DOMAIN>-<nnn>`
- ADR: `ADR-<yyyyMMdd>-<nn>`
- 长文档：`docs/ai/<DOMAIN>/<FEATURE_ID>_<SLUG>_DESIGN.md`（全大写）

## Session End

追加 `docs/ai/PROGRESS_LOG.md`，写 session note，标记未完成 Slice。
