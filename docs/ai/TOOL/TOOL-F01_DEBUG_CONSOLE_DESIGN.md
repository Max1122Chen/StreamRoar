# TOOL-F01_DEBUG_CONSOLE Design Spec

## Meta
- **ID:** `TOOL-F01`
- **Type:** `Feature`
- **Complexity:** `L2`（若引入可脚本化语言则升 L3 —— **本设计固定 L2**）
- **Status:** `Draft`
- **Owner:** Max
- **Last updated:** `2026-09-23`
- **Related:** [Feature Registry](../FEATURE_REGISTRY.md) · INFRA-F02 · INFRA-F03 · INFRA-F01

## TL;DR

引入类 UE 的 **Debug Console**：严格命令集、词法/语法校验、帮助与补全、历史、Shipping 剥离。用编译原理做 **命令语言前端**（Lexer → Parser → 校验 → 执行），**不做**通用脚本语言/AST 解释器后端。当前 Draft，待评审。

## Problem

- Jam / 联调缺少统一作弊与诊断入口；易出现临时 `Debug.Log` 与私货快捷键。
- 需要与 Input Freeze、Tag、Save、Scene 等基础设施可控交互。
- 「严格命令集」要求错误在执行前暴露（未知命令、参数类型不匹配），而非运行中半失败。

## Requirements

- 运行时 UI（IMGUI 或 uGUI/UI Toolkit 其一；**推荐第一版 IMGUI 或简单 uGUI**，降低依赖）可开关 Console。
- **命令注册表**：名称、参数模式、帮助、执行委托、所需权限/可用性（Editor-only / Development-only）。
- **语言前端**：
  - Lexer：标识符、数字、字符串、标志、注释（可选）
  - Parser：`command arg*` 形态；支持引号字符串与转义
  - Semantic check：对照命令签名做类型/ arity 校验
  - Executor：调用委托，捕获异常转为可读错误
- **UX**：历史（上/下）、Tab 补全（命令名；参数补全可后置）、滚动输出、`help` / `help <cmd>`。
- **严格性**：未注册命令失败；多余/缺失参数失败；类型失败；禁止「任意表达式求值」。
- **编译符号**：`DEVELOPMENT_BUILD` / Editor 可用；Shipping 默认剥离或空实现（`NoopConsole`）。
- 打开 Console 时调用 `IPlayerInputHub.PushFreeze(Console)`（若 Hub 已存在；否则本 Feature 内提供可空依赖）。
- 初始命令集（建议最小）：`help`、`clear`、`quit`（Editor 停 Play）、`god`/`timescale` 等可后置；**至少预留** `tag.*`、`scene.load` 的注册点，实现可分切片。

## Constraints

- **不**实现脚本语言（无变量、无函数定义、无控制流）；若未来需要，另开 L3 Feature。
- 命令执行默认主线程；不得在命令里偷偷起无生命周期的永久对象而不登记。
- 输出可能含作弊信息；Shipping 必须不可用。
- 依赖方向：TOOL → Infrastructure 接口；Infrastructure **不**反向依赖 Console UI。

## Preferences

- 命令名 `snake.case` 或 `dot.separated`（贴近 UE `stat fps` / `p.NetShowCorrections`）；推荐 **`dot.separated`**：`tag.list`、`time.set_scale`。
- 解析器手写递归下降即可（命令语法简单）；不强制引入外部 parser generator。
- 中文帮助字符串可接受。

## Assumptions

- 开发期 Console 是主诊断面；正式玩家不可见。
- INFRA-F03 Freeze API 将在或可 stub；Console 与 Input 可同迭代联调。
- 团队接受「新诊断能力优先加命令，不加深链接捷键」。

## Non-Goals

- 完整 UE `CheatManager` 复制、蓝图节点、文件内脚本批处理（可后置 `exec` 读文件）。
- REPL 编程、Lua/C# 热运行。
- 网络同步作弊（多人非目标）。
- 美化成最终游戏 UI 风格。

## Current Architecture

- 有 EventBus、SceneNavigator、GameTime、Save、Tag（规划中）、PlayerInputHub（规划中）。
- UI 系统存在但 Sample 无屏；Console 第一版可独立于 UISystemConfig，避免阻塞。
- 无现有 Console。

## Proposed Architecture

```text
Infrastructure/Diagnostics/  或 Tools/DebugConsole/
  Console/
    IDebugConsole.cs
    DebugConsole.cs              # 开关、输出缓冲、历史
    ConsoleCommandAttribute.cs   # 可选
    Commands/
      IConsoleCommand.cs         # Name, Signature, Execute, Help
      ConsoleCommandRegistry.cs
    Parsing/
      ConsoleToken.cs
      ConsoleLexer.cs
      ConsoleParser.cs           # → ConsoleInvocation (name + args)
      ConsoleArgType.cs          # Bool, Int, Float, String, Tag, Enum...
      ConsoleSemanticAnalyzer.cs
    UI/
      DebugConsoleView.cs        # IMGUI/uGUI

注册：ApplicationController 或独立 Bootstrap 仅在 #if 下创建
```

**执行管线：**

```text
Raw line → Lexer → Tokens → Parser → Invocation
  → Registry lookup → Signature check → Execute → WriteLine
```

## Responsibilities

| Unit | Responsible for | Not responsible for |
|------|-----------------|---------------------|
| Lexer/Parser | 词法语法 | 业务副作用 |
| Registry | 命令发现与签名 | UI |
| Executor/Console | 调用、日志缓冲、生命周期 | 玩法规则 |
| View | 显示与输入框 | 解析 |
| 各模块命令类 | 领域操作 | 解析器 |

## Boundaries

- Allowed：命令调用公共 Infrastructure API；Development 下改 timescale、加载场景。
- Forbidden：命令反射调用任意私有方法；解析器执行表达式树；生产代码在 Shipping 保留作弊。
- Input：仅通过 Hub Freeze；Console 开/关快捷键可走独立 Action Map 或 Editor 键（需在实现中固定一处）。

## Dependencies

- Soft depends：INFRA-F03（Freeze）、INFRA-F02（tag 命令）、ISceneNavigator、IGameTimeService。
- Depends on：Unity UI 或 IMGUI。
- Depended on by：联调、QA、Jam 快速试验。

## Ownership & Lifetime

- Console 服务：DDOL，随应用创建；Shipping 空实现。
- 输出环形缓冲有上限（如 500 行），防内存涨。

## Data Flow

`Key → Open View → User line → Pipeline → Command → Systems → Text result → Buffer → View`

## Control Flow

- Toggle Open → Freeze Push → Focus input。
- Toggle Close → Freeze Pop → 恢复 Map。
- Submit → 同步执行（默认）；长任务命令需自管异步并回报（第一版禁止异步命令，或仅允许显式 `IAsyncConsoleCommand` 后置）。

## State & Invariants

- 关闭时不拦截输入（除 toggle 键）。
- Registry 在运行时只读（启动注册完毕）；热重载命令非目标。
- 同一命令名唯一。

## API / Interface Semantics

- `void Register(IConsoleCommand cmd)`
- `void WriteLine(string)` / `void WriteError(string)`
- `bool TryExecute(string line, out string error)`
- `IDebugConsole` 可 Locator 注册。
- 签名示例：`time.set_scale <float>`、`tag.has <string>`。

## Failure Model

- 解析失败：指出 token 位置。
- 执行异常：捕获，打印异常消息，不崩溃 Play（Editor 可选择 rethrow 开关）。
- 缺失依赖（无 Tag Manager）：命令报告「服务未就绪」。

## Alternatives & Trade-offs

### Option A (recommended)

- 手写 Lexer/Parser + 严格 Registry + Development 剥离 + 简单 View。
- Pros：满足「编译原理前端」目标，范围可控。
- Cons：无脚本化。

### Option B

- 仅字符串 `Split` + switch。
- Why not：难做严格校验/补全/引号字符串，与目标不符。

### Option C

- 嵌入 Lua/MoonSharp 等。
- Why not：L3 复杂度，Jam 前过重，Shipping 风险更大。

## Integration Impact

- ApplicationController `#if` 注册；Input Map 增加 Console。
- 文档：`docs/Testing.md` 或新 `docs/DebugConsole.md` 列出命令公约。
- 可能与 UI 焦点冲突 —— 用 Freeze 规避。

## Migration / Compatibility

- 全新模块。
- 命令命名公约一旦公开，变更需版本说明（Jam 内可破坏性改）。

## Verification Strategy

- EditMode：Lexer/Parser/签名校验用例（不依赖 UI）。
- PlayMode/手工：打开关闭、Freeze、help、故意错误参数、Shipping 宏下无 Console。
- 安全：Release 配置抽检无作弊命令入口。

## Open Questions

1. UI 技术：IMGUI vs UI Toolkit vs 现有 UIManager？— **推荐 IMGUI 首版**，后可换皮。
2. Toggle 键默认？— 建议 `` ` `` / `F1`，待定。
3. 是否第一版就含 `tag.*` / `time.*` / `scene.*`？— **建议 S01 管道+help，S02 接系统命令**。
4. 命令发现：手写 Register vs Attribute 扫描？— **推荐显式 Register（清晰）**；Attribute 可选。

## Design Review

- **Verdict:** `Pending`
- **Reviewer / date:**
- **Link or summary:**

## Implementation Readiness

- [x] Requirements understood
- [x] Constraints identified
- [x] Relevant existing architecture inspected
- [x] Responsibilities and boundaries defined
- [x] Key dependencies understood
- [x] Ownership/lifetime resolved or N/A
- [x] Major state/invariants resolved or N/A
- [x] API semantics sufficiently defined
- [x] Important failure modes considered
- [x] Alternatives considered where meaningful
- [x] Integration impact understood
- [x] Verification strategy exists
- [ ] Open Questions 待确认

**Ready for implementation?** `no` — 待评审；且建议 **在 INFRA-F01 管道与 INFRA-F03 Freeze 之后** 或并行但接口可空。

## Acceptance Checklist

- [ ] Design review complete when required by complexity/profile
- [ ] Implementation plan traces to design decisions (L2+)
- [ ] Progress log updated when status changes
- [ ] Feature registry status synced

## Suggested Slices

| Slice | Content |
|-------|---------|
| `TOOL-F01-S01` | Lexer/Parser/Registry/TryExecute + EditMode 测试 |
| `TOOL-F01-S02` | View + Toggle + Freeze 集成 |
| `TOOL-F01-S03` | `help`/`clear` + `time.*` / `scene.*` |
| `TOOL-F01-S04` | `tag.*` + Shipping 剥离验证 |
