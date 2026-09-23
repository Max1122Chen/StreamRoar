# Progress Log

只追加、按时间顺序的项目事实记录。

## 2026-09-23

- Scope: `INFRA-F01` / `INFRA-F01-S01..S03`
- Completed:
  - 抽出 `StreamRoar.Infrastructure.Core`（ServiceLocator / Events / Save）
  - 新增 `StreamRoar.Tests.EditMode` 与三类样板测试；`ServiceLocator.ClearForTests` + InternalsVisibleTo
  - 文档：`docs/Testing.md`，更新 Verification / README / PROJECT_CONTEXT / AGENTS / API 路径
  - Implementation Plan 落地；Design Open Questions 按 Core 方案决议
- Verification:
  - 静态：路径与 asmdef/测试源文件就位
  - **待用户本地**：Unity 编译 + Test Runner EditMode 全绿（本环境未启动 Unity）
- Docs updated:
  - F01 design/impl、FEATURE_REGISTRY、ACTIVE_WORK、PROGRESS_LOG
- Next action:
  - 用户 review F01；通过后开始 `INFRA-F02`

## 2026-09-23

- Scope: `INFRA-F01` / `INFRA-F02` / `INFRA-F03` / `TOOL-F01`（设计稿）
- Completed:
  - 注册四项预 GameJam 基础设施 Feature
  - 撰写 Design Spec（均为 `Draft`，Review `Pending`）：
    - `docs/ai/INFRA/INFRA-F01_UNITY_TEST_FRAMEWORK_DESIGN.md`
    - `docs/ai/INFRA/INFRA-F02_GAMEPLAY_TAG_DESIGN.md`
    - `docs/ai/INFRA/INFRA-F03_PLAYER_CONTROLLER_INPUT_DESIGN.md`
    - `docs/ai/TOOL/TOOL-F01_DEBUG_CONSOLE_DESIGN.md`
  - 更新 `FEATURE_REGISTRY.md`、`ACTIVE_WORK.md`
- Verification:
  - 文档路径与交叉链接检查；未写代码
- Docs updated:
  - 如上
- Next action:
  - 用户评审四份设计；确认各文 Open Questions 后给 Design Review Verdict

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
