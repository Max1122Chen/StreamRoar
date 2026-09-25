# Progress Log

只追加、按时间顺序的项目事实记录。

## 2026-09-23

- Scope: 工作流初始化（非玩法 Feature）
- Completed:
  - 从 `min-agent-workflows`（`feat/engineering-design-workflow`）拷入 `docs/ai/` 核心与多 Agent 适配层
  - 配置 `WORKFLOW_PROFILE`：`serious-engineering` + partner + `docs_language=zh` + dual-track + `smoke-required`
  - 填写 `PROJECT_CONTEXT` / `BOOTSTRAP_DIGEST`；清空模板 `TMPL-*` 注册与样例队列
  - 保留并扩展根 `AGENTS.md`（产品规则 + 协作工作流入口）
  - 保留 Cursor / opencode / Claude / Copilot / Cline / Windsurf 适配器，并增加 `ADAPTERS.md`
- Verification:
  - 文件树与交叉链接检查；未启动 Unity（纯文档/配置部署）
- Docs updated:
  - `docs/ai/*`、`AGENTS.md`、`docs/README.md`、各 adapter 入口
- Next action:
  - 用户确认后可 prepare commit；选定下一 Infra/玩法里程碑时注册首个 Feature
