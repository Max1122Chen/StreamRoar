# AI Collaboration Docs Index

本目录是人类与 Agent 的**实施协作真相**（Feature / 进度 / 债务 / 会话）。  
产品与架构真相在仓库 `docs/` 与根目录 `AGENTS.md`（dual-track）。

## Agent: required planning sources

决定「下一步做什么」时使用：

1. `WORKFLOW_PROFILE.md` — 协作姿态
2. `ACTIVE_WORK.md` — 当前短队列
3. `FEATURE_REGISTRY.md` — 已注册 Feature（关注 In Progress / Planned）
4. `TECH_DEBT.md` — 未关闭债务
5. `PROGRESS_LOG.md` — 近期事实
6. 代码 + 验证（`docs/Verification.md`）— 运行时真相优先于过时文档

信任分级：
- Cursor：`.cursor/rules/docs-trust-tiers.mdc`
- 其他 Agent：`templates/DOC_GOVERNANCE.md` 中 Agent doc trust；总览见 `ADAPTERS.md`

## Core files

- `WORKFLOW_PROFILE.md` — 部署时姿态 / preset
- `PROJECT_CONTEXT.md` — 稳定项目快照与 Domain 表
- `BOOTSTRAP_DIGEST.md` — 会话快速恢复
- `ACTIVE_WORK.md` — 人工短队列
- `FEATURE_REGISTRY.md` — Feature ID 与状态
- `PROGRESS_LOG.md` — 完成事实的时间线
- `TECH_DEBT.md` — 有主债务
- `WORKING_WITH_AI.md` — 提示词与习惯
- `ADAPTERS.md` — 多 Agent 适配与扩展位
- `INIT_GUIDE.md` — 模板初始化说明（参考）

## Templates

见 `templates/`：Design、Implementation Plan、ADR、Bug、Session、Workflow Presets、DOC_GOVERNANCE。

## Domain buckets and naming

- 长设计/实现文档放在 `docs/ai/<DOMAIN>/`（不要堆在根目录）
- 文件名（全大写）：`<FEATURE_ID>_<SLUG>_DESIGN.md`（以及 `_IMPLEMENTATION` / `_ROADMAP` / `_REFACTOR_PLAN`）
- Domain：`INFRA` / `UI` / `GAME` / `AUDIO` / `CFG` / `TOOL` / `ASSET`
- 细则：`templates/DOC_GOVERNANCE.md` §3、§5
- 技能：`.opencode/skills/engineering-design/`、`design-review/`；项目技能在 `.agents/skills/`
