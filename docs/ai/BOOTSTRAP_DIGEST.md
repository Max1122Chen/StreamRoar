# Bootstrap Digest

Last updated: 2026-09-23  
Purpose: 两分钟内恢复协作上下文。

## Read order for a new session

1. `WORKFLOW_PROFILE.md` — 协作姿态（若 `unconfigured`，只问一次 meta-question）
2. `PROJECT_CONTEXT.md`
3. 根目录 `AGENTS.md` — 项目所有权、验证与轻文档规则（产品层）
4. `PROGRESS_LOG.md`（只读近期）
5. `ACTIVE_WORK.md`
6. `FEATURE_REGISTRY.md`（In Progress / Planned）
7. `TECH_DEBT.md`（Open）
8. 任务相关设计文档（若 ACTIVE_WORK 已链接，或用户点名）

## Workflow profile gate

- Catalog: `templates/WORKFLOW_PRESETS.md`
- 当前：`configured` / `serious-engineering` / `docs_language=zh` / `dual-track` / `smoke-required`
- 用户说 `reconfigure workflow` 时才重跑配置

## Non-negotiable collaboration rules

- 只从可信规划源计划；不要从旧 roadmap / Snapshot 推断 backlog
- 实质性新工作：评估 L0–L3；需要时注册 Feature ID；L2+ 先设计再大改
- `Draft` 或 design-review `Not ready` 不授权大规模编码
- L3 架构安全检查不可被 preset 关闭
- 有意义批次结束：更新 docs/ai，再提出 “prepare commit”
- Prepare commit 只起草；执行 commit 需用户明确指示
- Profile 可调严格度，不可关闭上述硬约束

## Engineering design (short)

- Skills: `.opencode/skills/engineering-design/`、`.opencode/skills/design-review/`
- Process: `templates/DOC_GOVERNANCE.md` §5.2–5.3
- L0 可跳过设计；L2+ Design+Review；L3 + readiness 门禁
- 项目技能：`.agents/skills/infrastructure-usage`、`.agents/skills/pre-commit-review`

## ID scheme

- Feature: `<DOMAIN>-F<nn>`（Domain 见 `PROJECT_CONTEXT.md` §7）
- Slice: `<FeatureID>-S<nn>`
- Bug: `BUG-<DOMAIN>-<nnn>`
- ADR: `ADR-<yyyyMMdd>-<nn>`
- 长文档：`docs/ai/<DOMAIN>/<FEATURE_ID>_<SLUG>_DESIGN.md`（全大写）

## Verification baseline

- Verify: Unity 打开 `StreamRoar/`，导入/编译干净
- Smoke: `Launch.unity` Play Mode → SampleScene；见 `docs/Verification.md`
- Enforce bar from profile: `smoke-required`

## Dual-track reminder

- 产品真相：`docs/` + `AGENTS.md`
- 协作真相：`docs/ai/`
- 多 Agent 适配：`docs/ai/ADAPTERS.md`

## Handoff trigger cues

当用户表示交接/换会话时：
- 按模板写 session note（`docs/ai/sessions/`）
- 追加一条 PROGRESS_LOG
- 未完成 Slice 标 Blocked/Deferred，写明原因与解锁条件
