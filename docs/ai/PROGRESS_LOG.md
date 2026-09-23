# Progress Log

只追加、按时间顺序的项目事实记录。

## 2026-09-23

- Scope: `INFRA-F02` / `S01–S04`
- Completed:
  - 新增 `Infrastructure/Tags`：Tag / Manager / Container / Native Source / Source 扩展点
  - `ApplicationController` 注册 `IGameplayTagManager`
  - EditMode：`GameplayTagTests`
  - 文档：Tags README、Architecture / Conventions / API 索引；F01 标 Done
- Verification:
  - 待用户 Unity Test Runner 跑 `GameplayTagTests`
- Next action:
  - 用户 review F02；通过后开始 `INFRA-F03`

## 2026-09-23

- Scope: `INFRA-F01` 调整
- Completed:
  - 撤销 `Infrastructure/Core` 拆分，恢复伙伴原有 Events/Save/ServiceLocator 布局
  - 测试改挂 `Assets/Tests/Editor/`（Editor 程序集），去掉生产 asmdef
  - `ClearForTests` 保留为测试专用公开 API
- Verification:
  - 用户确认 EditMode 测试全绿
- Next action:
  - 开始 `INFRA-F02`

## 2026-09-23

- Scope: `INFRA-F01` / `INFRA-F01-S01..S03`
- Completed:
  - （已由后续调整取代：原 Core asmdef 方案）
  - 新增 EditMode 样板测试与 Testing 文档
- Verification:
  - 见上一条调整说明
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
