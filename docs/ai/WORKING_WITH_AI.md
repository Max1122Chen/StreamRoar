# Working With AI

Last updated: 2026-09-23

## Profile

当前已配置：`serious-engineering` + partner + 中文 + dual-track。  
重配请说：`reconfigure workflow`。

## Session start prompt

建议提示词：

```text
继续本仓库。先读 docs/ai/WORKFLOW_PROFILE.md、PROJECT_CONTEXT.md、PROGRESS_LOG.md、ACTIVE_WORK.md，以及根目录 AGENTS.md。总结姿态与当前状态，并提出下一步与验证命令。
```

## Session end prompt

```text
请把今日工作追加到 docs/ai/PROGRESS_LOG.md，并给出下一会话的第一个具体动作。
```

## Design / review prompts

```text
评估复杂度 L0–L3。若 L2+，先走 engineering-design 再 design-review，通过后再写代码。
```

```text
按 implementation readiness 评审这份 Design Spec。若 Not ready，不要开始大规模实现。
```

## Workflow habits

- 实质性新工作：大改前先在 `FEATURE_REGISTRY.md` 注册 Feature ID
- 评估复杂度；L0 保持轻量（可只改代码 + 按 `AGENTS.md` 验证）
- L2+/L3：Design Spec → Design Review → readiness → Implementation Plan
- 需求与方案分开写；先看现有架构再发明抽象
- 架构或范围决策：更新 Design Spec，必要时 ADR
- 顺手发现的跨模块缺陷：先建 bug 记录再大范围改
- 交接：session note + progress + 未完成 Slice 状态
- 提交：先 prepare（草稿），用户明确说执行后再 commit
- 遵守 `WORKFLOW_PROFILE.md` 的 Effective behavior（含中文交付）
- 多 Agent：见 `ADAPTERS.md`；专用 API 见 `.agents/skills/infrastructure-usage`

## First deploy / unconfigured profile

若 `WORKFLOW_PROFILE.md` 为 `unconfigured`，只问一次 meta-question（见 `templates/WORKFLOW_PRESETS.md`）。用户未选 configure 时不要展开长问卷。
