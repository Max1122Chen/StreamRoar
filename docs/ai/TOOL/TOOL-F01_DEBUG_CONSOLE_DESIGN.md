# TOOL-F01_DEBUG_CONSOLE Design Spec

## Meta
- **ID:** `TOOL-F01`
- **Type:** `Feature`
- **Complexity:** `L2`（若引入可脚本化语言或完整 CVar 系统则升 L3 —— **本 Feature 固定 L2**）
- **Status:** `Review`
- **Owner:** Max
- **Last updated:** `2026-09-27`（补：输入即智能提示）
- **Related:** [Feature Registry](../FEATURE_REGISTRY.md) · INFRA-F01 · INFRA-F02 · INFRA-F03 ·（参考 minEngine Debug Console；后续 CVar → 候选 `TOOL-F02`）

## TL;DR

引入开发期 **Debug Console 中性内核**：严格命令注册与执行管线、引号分词、签名校验、帮助/历史/补全、OnGUI View、Shipping 剥离。  
**本 Feature 只提供元命令**（`help` / `clear` 等），**不**绑定 Tag / Time / Scene / 玩法等具体域命令——由各系统日后自行 `Register`。  
CVar 本轮不做、既定后续（候选 `TOOL-F02`）。实现已挂，待 Unity 验证。

## Problem

- Jam / 联调缺少统一诊断入口，易出现临时 `Debug.Log`、私货快捷键、散落作弊码。
- 需要可扩展的「严格命令」通道：未知命令与参数错误在执行前暴露。
- 正式玩家包不得保留可调用的作弊面。
- 若 Console Feature 一开始就耦合各基础设施命令，会拖垮中性与依赖边界。

## Requirements

### 功能（MVP）

- 运行时可开关 Console；**View 使用 Unity 内置 Immediate Mode GUI（`OnGUI`）**。
- **命令注册表**：名称、参数签名、帮助、执行委托、可用性（Editor-only / Development-only）。
- **扩展点**：对外稳定的 `Register(IConsoleCommand)`（及 Locator 上的 `IDebugConsole` / Registry），供其他 Feature / 模块在自己的启动路径注册域命令。
- **执行管线**（对齐 minEngine，不做通用脚本语言）：
  1. Tokenize（空白分词 + 引号字符串；可选简单转义）
  2. 首 token = 命令 id；其余为参数
  3. Registry 查找
  4. 按签名校验 arity / 类型
  5. Execute；结果写入输出缓冲
- **UX**：
  - 输出滚动区、输入行、历史（上/下）、元命令可用。
  - **智能提示（输入即提示）**：在输入**命令名阶段**（行内尚无空白分隔的参数）时，按前缀（不区分大小写）实时列出匹配命令候选（含短帮助摘要更佳）；候选条显示在输入框附近。
  - **Tab**：接受当前选中候选（或唯一匹配 / 最长公共前缀）；多候选时可用上/下在候选间移动（此时上/下不走历史）。
  - 进入参数阶段后隐藏命令候选；参数补全可后置。
- **严格性**：未注册命令失败；参数多余/缺失失败；类型失败；**禁止任意表达式求值**。
- **编译剥离**：仅 `UNITY_EDITOR || DEVELOPMENT_BUILD` 提供真实实现；Shipping 注册 `NoopDebugConsole`（或根本不创建 View）。

### 本 Feature 提供的命令（仅元命令）

| 命令 | 行为 |
|------|------|
| `help` / `help <cmd>` | 列出已注册命令，或打印某命令签名/帮助 |
| `clear` | 清输出缓冲 |
| `echo <text…>`（建议） | 回显参数，便于管线/自动化冒烟，无域依赖 |

**明确不在本 Feature：** `time.*`、`tag.*`、`scene.*`、`quit`、玩法作弊等。由对应系统（或后续小切片）在持有服务的一侧注册，保持 Console 内核中性。

## Constraints

- **不**实现脚本语言（无变量、无函数定义、无控制流、无 `;` 批命令）。
- **本 Feature 不实现 CVar**（见「后续：CVar」）；不得用临时全局字典假扮 CVar。
- **本 Feature 不注册任何域/系统命令**（Time / Tag / Scene / Audio / 玩法等）；只注册元命令。
- **不**做 PropertyPath / 反射改任意序列化字段。
- 命令默认**主线程同步**执行；第一版禁止隐式异步命令。
- 依赖方向：Console 内核 **不**硬依赖具体玩法或业务服务接口；域命令反向依赖 Console 的注册 API。
- View **不**接入现有 `UIManager` / `UISystemConfig`。

## Preferences

- 域命令命名约定仍推荐 `dot.separated`（`tag.list` 等），但由各域自守；内核不强制前缀表。
- 分词 + 签名校验即可；**不**强制完整 Lexer/Parser/AST 工程。
- 显式 `Register` 优于 Attribute 扫描。
- **元命令集中在单一源文件**（如 `MetaConsoleCommands.cs`）+ `RegisterAll`；域命令由各模块自管文件，避免塞进 Console 目录造成「假中性」。
- 帮助文案可用中文；标识符英文。
- Toggle 键建议 `` ` ``（Backquote）或 `F1`，实现时固定一处并写进帮助。

## Assumptions

- 开发期 Console 是主诊断面；正式玩家不可见。
- INFRA-F03（IMC 栈）可能尚未落地；Toggle/IMC 用可空依赖或临时 DEV 读键（见 Integration）。
- 团队接受「新诊断能力优先加命令，不加深链接捷键」；**域命令归属对应 Feature，不堆积进 TOOL-F01**。
- **CVar 是路线图上的既定后续**；本轮只保住接缝。
- 参考实现心智来自 minEngine：Registry + Tokenize + Validate + Execute + History + Completion。

## Non-Goals（本 Feature）

- Dear ImGui / uGUI / UI Toolkit 首版换皮（以后可换 View，Executor 不变）。
- 完整 UE `CheatManager`、蓝图、文件批 `exec`。
- REPL 编程、Lua/C# 热运行。
- 网络同步作弊。
- 美化成最终游戏 UI。
- **实现** CVar / ShowFlag / `stat` 叠层 —— **非永久放弃**；见「后续：CVar」。
- **绑定** Tag / Time / Scene / 玩法等具体诊断命令 —— 由各系统后续接入。

## 后续：CVar（规划口径，非本轮交付）

UE / 诊断习惯里，**Command = 动作**，**CVar = 可查询/可设置的命名状态旋钮**（如 `r.ShadowQuality 1`）。minEngine 目前缺这块；StreamRoar **需要，但不塞进 TOOL-F01 MVP**。

| 项 | 口径 |
|----|------|
| 何时做 | 本 Feature 验收后，注册候选 **`TOOL-F02`（Console Variables）**（名可再定） |
| 与 Command 关系 | CVar Registry 独立；可用通用命令（如 `cvar.list` / 或「未知名且命中 CVar → get/set」）挂到同一 Executor，**不**把每个 CVar 写成独立 `IConsoleCommand` 类文件 |
| 本轮要保住的 | `IConsoleCommand` + Registry + Executor 边界清晰，后续能「加一层 CVar 解析」而不推翻 REPL |
| 本轮不要做的 | 不写 `IConsoleVariable` 类型、不做半吊子全局 `Dictionary<string,object>` |

复杂度预期：完整 CVar（类型、Cheat 标志、变更回调、Shipping 剥离）偏 **L2–L3**，故单独 Feature + Design。

## Current Architecture

- 已有：`ServiceLocator`、应用生命周期在 `ApplicationController`；各类基础设施服务可供**日后**域命令调用，但本 Feature **不**在启动时为其注册命令。
- UI 系统存在但 Sample 无业务屏；诊断 UI 不应阻塞在 UI 装配上。
- INFRA-F03：设计为每 Player **IMC 优先级栈**（开 Console = `AddMappingContext(IMC_Console)`）；代码可能尚未实现。
- 仓库内无现有 Console。

## Proposed Architecture

```text
Assets/Scripts/Infrastructure/Diagnostics/Console/   # 建议挂 Diagnostics 以贴近 Locator
  IDebugConsole.cs                 # IsOpen, Open/Close/Toggle, WriteLine/WriteError, TryExecute
  DebugConsole.cs                  # 缓冲、历史、编排 Executor
  NoopDebugConsole.cs              # Shipping / 未编译诊断时
  IConsoleCommand.cs               # Name, Help, ArgSchema, Execute(args, writer)
  ConsoleCommandRegistry.cs
  ConsoleTokenizer.cs              # quote-aware split → tokens
  ConsoleExecutor.cs               # lookup + validate + invoke
  ConsoleHistory.cs
  ConsoleCompletion.cs             # 候选列表 + Tab 接受；参数补全可后置
  MetaConsoleCommands.cs           # 仅元命令：help / clear / echo + RegisterAll
  UI/
    DebugConsoleView.cs            # MonoBehaviour，OnGUI 绘制；#if 包裹

# 域命令示例（不在本 Feature 目录/范围）：
# Infrastructure/Tags/.../TagConsoleCommands.cs          ← 未来由 Tag 侧注册
# ApplicationLifecycle 或各系统 Bootstrap 中 Register
```

**元命令组织：** `MetaConsoleCommands.RegisterAll(registry, console)` 只挂内核自用命令（`help` 需读 Registry；`clear`/`echo` 写 Console）。目录内不堆碎文件。

**域命令接入约定（中性扩展）：**

```text
各系统自己的启动 / 模块初始化
  → Resolve IDebugConsole 或 IConsoleCommandRegistry（Development 下）
  → Register(本域 IConsoleCommand…)
```

Console 内核**不** `Resolve` Tag/Time/Scene 仅为了塞命令。

**注册：** `ApplicationController`（或薄 Bootstrap）仅在 `#if UNITY_EDITOR || DEVELOPMENT_BUILD` 下创建 Console、`MetaConsoleCommands.RegisterAll`、挂 View。

**执行管线：**

```text
Raw line
  → Tokenizer → tokens[]
  → Registry.Find(tokens[0])
  → Validate(schema, tokens[1..])
  → command.Execute → WriteLine / WriteError
```

**与产品 UI 的关系：** View 是独立 `MonoBehaviour`（可挂在 Application 根或自建 DDOL 对象），只读 `IDebugConsole` 状态；不经过 `IUIService`。

## Responsibilities

| Unit | Responsible for | Not responsible for |
|------|-----------------|---------------------|
| Tokenizer | 分词与引号规则 | 业务副作用 |
| Registry | 命令发现、唯一名 | UI、领域逻辑 |
| Executor | 校验并调用 | 拥有玩法规则 |
| DebugConsole | 开/关、缓冲、历史、对外 API | 画 UI |
| DebugConsoleView | OnGUI 输入/输出/快捷键转发 | 解析与执行细节 |
| `MetaConsoleCommands` | 元命令与 `RegisterAll` | 任何域逻辑、CVar 系统 |
| 各域 `*ConsoleCommands`（后续） | 本域命令注册与执行 | Console 内核 / UI |

## Boundaries

- **Allowed：** 元命令只碰 Console 自身状态（缓冲/Registry 列表）；域命令（他处）可调本域公共 API；View 仅在诊断宏下编译。
- **Forbidden：** 本 Feature 内硬编码域服务命令；反射调用任意私有方法；解析器求值表达式；Shipping 保留可执行作弊面；Infrastructure 核心模块反向引用 `DebugConsoleView`。
- **Input：**
  - **目标态（F03 就绪后）：** Toggle / 提交 / 历史键走 `IMC_Console`；打开时由 Console 对本地 Player `AddMappingContext(IMC_Console, highPriority)`，关闭时 `Remove`（是否同时 Remove Gameplay 由调用方策略决定，本 Feature 默认「加高优先 Console IMC」即可）。
  - **过渡态（F03 未就绪）：** View 内临时读 Input System 键盘做 Toggle（`#if`），并在 `TECH_DEBT` 登记「迁到 IMC_Console」退出条件。

## Dependencies

| 方向 | 项 |
|------|-----|
| Soft | INFRA-F03（IMC_Console）；无则过渡读键 |
| Hard | Unity 内置 GUI（无额外包） |
| 被依赖 | 后续各域命令注册方、联调、QA、Jam；CVar Feature |

本 Feature **不**依赖 F02 Tag / GameTime / SceneNavigator 才能交付。域命令接入时由该域自带依赖。

## Ownership & Lifetime

- `IDebugConsole` 实现：由应用启动创建，建议挂 DDOL 或与 `ApplicationController` 同寿；Shipping 为 Noop 或不注册。
- 输出环形缓冲上限（建议 256–512 行），防止内存涨。
- 历史条数上限（建议 50–100）。
- View 与 Console 服务同生命周期；关闭 Console ≠ 销毁服务（仅 `IsOpen=false`，停止拦截/绘制输入区）。

## Data Flow

```text
Toggle → Open →（可选）Add IMC_Console
User line → Tokenizer → Validate → Command → Infra APIs
  → text → ring buffer → OnGUI 重绘
Close → Remove IMC_Console
```

## Control Flow

- **Toggle Open：** `IsOpen=true` → 可选挂 IMC → View 开始 `OnGUI` 绘制并吞提交键。
- **Toggle Close：** `IsOpen=false` → 卸 IMC → 不再绘制面板（Toggle 键仍监听）。
- **Submit：** 同步 `TryExecute`；异常捕获为错误行，默认不打断 Play（Editor 可加「调试再抛出」开关，非 MVP）。

## State & Invariants

- 关闭时：除 Toggle 外不消费 Console 专用输入。
- 命令名全局唯一（大小写策略：**建议不敏感查找、注册时规范化为小写**，或敏感二选一，实现前锁定）。
- Registry 在启动注册完成后运行时只读；热重载命令非目标。
- Shipping：无真实 Registry 或空 Registry + 无 View。

## API / Interface Semantics

### `IDebugConsole`

| 成员 | 语义 |
|------|------|
| `bool IsOpen { get; }` | 面板是否打开 |
| `void Open()` / `Close()` / `Toggle()` | 生命周期；幂等 |
| `void WriteLine(string)` / `WriteError(string)` | 追加缓冲；Error 可带前缀着色（OnGUI 用不同 `GUI.color`） |
| `bool TryExecute(string line, out string error)` | 整行执行；失败时 `error` 有说明且已 WriteError；成功 `error=null` |
| `IReadOnlyList<string> GetOutputSnapshot()` 或事件 | 供 View 拉取（也可 View 直持缓冲引用，需明确所有权：缓冲属 Console） |

### `IConsoleCommand`

- `string Name`、`string Help`
- `ConsoleArgSchema`（有序参数：类型 + 可选/重复规则；MVP 用固定 arity + 类型枚举即可）
- `void Execute(IReadOnlyList<string> args, IConsoleWriter writer)`  
  - 参数已通过类型校验时可再提供 typed 重载；MVP 字符串 + 命令内 `TryParse` 亦可，但 **Registry 层必须先做 arity/类型门禁**。

### Registry

- `void Register(IConsoleCommand command)` — 重复名抛错或返回 false（选一种，推荐启动期抛错）
- `bool TryGet(string name, out IConsoleCommand command)`
- `IEnumerable<IConsoleCommand> GetAll()` — 供 `help` 与补全

### Tokenizer

- 空白分隔；`"..."` 为单 token；非法未闭合引号 → 解析失败（带位置信息更佳）。
- 空行 / 仅空白 → 成功空操作（不报错）。

**线程：** 仅主线程。

## Failure Model

| 失败 | 行为 |
|------|------|
| 分词失败 | WriteError，指出原因（如未闭合引号） |
| 未知命令 | WriteError：`unknown command: x`；可提示相近名（可选） |
| 签名不符 | WriteError：期望签名 + 实际 token 数 |
| 执行异常 | 捕获，WriteError 消息；不崩溃 Play |
| 依赖服务缺失 | 命令内报告 `service not ready`，不抛到未处理 |

## Alternatives & Trade-offs

### Option A（推荐）— minEngine 同构命令 REPL + OnGUI

- Tokenizer + Schema Validate + Registry + OnGUI View；**CVar 紧随其后的独立 Feature**。
- **Pros：** 与已验证心智一致；零 UI 包依赖；Shipping 好剥；Executor 可单测；不挡后续 CVar。
- **Cons：** OnGUI 观感旧；本 Feature 交付时尚无「改旋钮」体验（由 TOOL-F02 补）。

### Option B — 仅 `Split` + switch

- **Why not：** 引号、补全、严格校验与帮助签名难以做干净，和「严格命令集」目标不符。

### Option C — 嵌入 Lua / 完整 Lexer+AST 脚本

- **Why not：** L3；Jam 过重；Shipping 面更大。

### Option D — uGUI / UI Toolkit View

- **Why not（首版）：** 已拍板用内置 GUI；换皮不改 Executor，可后置。

### Option E — 本 Feature 顺带做完整 CVar

- **Why not selected for MVP：** 范围翻倍、评审面变大；**不是否定需求**。拆成 `TOOL-F02` 可在命令核稳定后专注类型/标志/回调/剥离。

## Integration Impact

- `ApplicationController`：`#if` 下创建 Console + `MetaConsoleCommands.RegisterAll` + 挂 View。
- 不修改 `UISystemConfig`；不在本 Feature 改 Tag/Time/Scene 模块。
- F03 就绪后：增加 `IMC_Console` 与 Open/Close Add/Remove；删除过渡读键。
- 域命令：在对应 Feature 的实现计划里写「注册到 `IDebugConsole`」，不回流进 TOOL-F01 范围膨胀。

## Migration / Compatibility

- 全新模块；无旧 API。
- 命令名一旦在 Jam 中公开，变更需在进度里说明（Jam 内允许破坏性改）。

## Verification Strategy

- **EditMode（INFRA-F01）：** Tokenizer（含引号）、未知命令、arity/类型失败、`help`/`clear`/`echo`、测试用假命令 `Register` 后 Execute；不依赖 OnGUI，不依赖 Tag/Scene。
- **手工 / PlayMode：** 打开关闭、元命令、输入 `he` 见 `help` 候选、Tab 接受、历史上翻、错误参数可读；Development 宏下可见。
- **Shipping 抽检：** 非 Development 玩家包无面板（或 Noop）。
- **扩展冒烟（可选）：** 测试里 `Register` 一条假域命令，验证中性扩展路径。

## Open Questions

1. **Toggle 默认键：** `` ` `` vs `F1`？— 建议 `` ` ``。（blocking? 否）
2. **命令名大小写：** 不敏感 vs 敏感？— 建议 **查找不敏感、显示用注册名**。（blocking? 否）
3. **目录名：** `Infrastructure/Diagnostics/Console` vs 顶层 `Tools/DebugConsole`？— 建议前者。（blocking? 否）
4. **F03 未合并时是否允许合并 TOOL-F01？** — **建议允许**：内核 + OnGUI 可先合；IMC 接线作切片或 TECH_DEBT。（blocking? 否）
5. **是否提供 `echo`？** — 建议要，便于无域依赖的验收。（blocking? 否）

## Design Review

- **Verdict:** `Ready with deferred items`
- **Reviewer / date:** Max / 2026-09-27（用户确认中性内核 + 元命令后授权实现）
- **Link or summary:** Open Questions 按建议锁定（`` ` ``、查找不敏感、Diagnostics 目录、允许先于 F03、保留 echo）；IMC 接线记 `DEBT-TOOL-001`

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
- [x] Unresolved questions resolved or explicitly deferred/accepted

**Ready for implementation?** `yes` — deferred：IMC_Console（`DEBT-TOOL-001`）

## Acceptance Checklist

- [x] Design review complete when required by complexity/profile
- [x] Implementation plan traces to design decisions (L2+)
- [x] Progress log updated when status changes
- [x] Feature registry status synced

## Suggested Slices

| Slice | Content |
|-------|---------|
| `TOOL-F01-S01` | Tokenizer / Registry / Executor / History + `MetaConsoleCommands` + EditMode 测试（无 UI） |
| `TOOL-F01-S02` | `DebugConsoleView`（OnGUI）+ Toggle + Application 接线 |
| `TOOL-F01-S03` | Shipping/Noop 验证；若 F03 已就绪则接 `IMC_Console`，否则记 TECH_DEBT |

**既定后续（不在本 Feature）：**

- 各域按需注册自己的命令（Tag / Time / Scene / 玩法等）
- 候选 `TOOL-F02` Console Variables（CVar）
