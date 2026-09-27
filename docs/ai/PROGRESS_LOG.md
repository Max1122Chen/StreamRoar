# Progress Log

只追加、按时间顺序的项目事实记录。

## 2026-09-27

- Scope: `TOOL-F01`（Tab OOM 修复）
- Completed:
  - 根因：`GetCandidates(..., int.MaxValue)` 用作 `new List<string>(capacity)` 触发 OOM
  - 改为有界收集 + 小初始容量；补回归用例
- Verification:
  - 待用户再试 Tab
- Next action:
  - 用户确认后可 prepare commit

## 2026-09-27

- Scope: `TOOL-F01`（智能提示）
- Completed:
  - 设计补充：输入即命令候选提示；Tab 接受；有候选时 ↑↓ 浏览
  - `ConsoleCompletion.GetCandidates` / View 候选条 / 相关 EditMode 用例
- Verification:
  - 待用户 Play 输入 `he` 确认见 `help` 提示
- Next action:
  - 用户确认后可 prepare commit

## 2026-09-27

- Scope: `TOOL-F01` / `S01–S03`（实现）
- Completed:
  - `Infrastructure/Diagnostics/Console`：Tokenizer / Registry / Executor / History / Completion / Meta（help/clear/echo）/ DebugConsole / Noop
  - OnGUI `DebugConsoleView`；`ApplicationController` Editor/Development 注册，Shipping 用 Noop
  - EditMode：`DebugConsoleTests`；TECH_DEBT `DEBT-TOOL-001`（IMC Toggle）
  - Design Review `Ready with deferred items`；Implementation Plan 已挂
- Verification:
  - 代码与文档已齐；**待用户** Unity Test Runner 跑 `DebugConsoleTests`，Play Mode 按 `` ` `` 烟测
- Next action:
  - 用户确认后标 Done；可 prepare commit

## 2026-09-27

- Scope: `TOOL-F01`（设计口径：中性内核）
- Completed:
  - 本 Feature 仅元命令（`help`/`clear`/`echo`）；去掉 time/tag/scene/quit
  - 域命令约定由各系统日后 `Register`；文件改为 `MetaConsoleCommands.cs`
- Verification:
  - 文档自洽；未写代码
- Next action:
  - 用户继续评审；确认后 Design Review

## 2026-09-27

- Scope: `TOOL-F01`（设计口径微调）
- Completed:
  - CVar 改为「本轮不做、既定后续 TOOL-F02」并写明与 Command 边界
  - 内置命令改为单文件 `BuiltinConsoleCommands.cs`，去掉 Commands/ 碎文件方案
- Verification:
  - 文档自洽；未写代码
- Next action:
  - 用户继续评审；确认后 Design Review

## 2026-09-27

- Scope: `TOOL-F01`（设计修订）
- Completed:
  - 重写 `TOOL-F01_DEBUG_CONSOLE_DESIGN.md`：UI 锁定 Unity `OnGUI`；对齐 minEngine 命令 REPL；CVar 明确后置
  - 输入集成改为 F03 IMC_Console（可空/过渡读键 + TECH_DEBT），去掉旧 Freeze 假设
- Verification:
  - 文档自洽；未写代码
- Next action:
  - 用户审阅 Draft，讨论 Open Questions 后给 Design Review

## 2026-09-25

- Scope: `INFRA-F03`（设计修订：IA/IMC/每 Player）
- Completed:
  - 核心模型改为对齐 UE Enhanced Input：IA + IMC 优先级栈 + `IPlayerInputUser`（per-player）
  - 弃用「全局单 Map + Freeze」为核心方案；后续层（Reader/设备事件/迁出）本轮不展开
  - 待审：资产组织方案 A/B、同 priority 次序、开 UI 是否 Remove Gameplay
- Verification:
  - 设计文档自洽；未写代码
- Next action:
  - 用户确认本层 Open Questions 后再进入实现或下一层设计

## 2026-09-25

- Scope: `INFRA-F03`（设计扩写）
- Completed:
  - （已被上一条 IMC 栈修订取代）原 Hub/Freeze 类型扩写
- Verification:
  - —
- Next action:
  - 见上

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
