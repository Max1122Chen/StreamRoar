# INFRA-F02_GAMEPLAY_TAG Design Spec

## Meta
- **ID:** `INFRA-F02`
- **Type:** `Feature`
- **Complexity:** `L2`
- **Status:** `Draft`
- **Owner:** Max
- **Last updated:** `2026-09-23`
- **Related:** [Feature Registry](../FEATURE_REGISTRY.md) · [Configuration](../../Configuration_Architecture.md) · INFRA-F01 · TOOL-F01

## TL;DR

需要一套与 UE GameplayTag 类似的分层标签系统，供状态、查询、Console、后续配置共用。本设计先落地 **C# 原生** 的 `GameplayTag` / `GameplayTagContainer` / 注册表（Manager），预留 **外部导表/Luban 导入** 而不在本 Feature 完成导表管线。当前 Draft，待评审。

## Problem

- 玩法与基础设施缺少稳定的「语义标签」原语；易退化为魔法字符串或散落 enum。
- 后续 DebugConsole、Settings、状态查询、伤害类型等都会需要 Tag 集合运算（Has / Any / All / 父子匹配）。
- 配置侧（Luban）尚未接入启动，但 Tag 必须提前定好 ID 与层级规则，避免双轨字符串。

## Requirements

- 提供不可变（或值语义清晰）的 **`GameplayTag`**：层级路径，如 `State.Debuff.Burn`。
- 提供 **`GameplayTagContainer`**：增删查、集合运算（HasTag、HasAny、HasAll；是否含 **显式父匹配 / 子匹配** 必须在 API 中写清）。
- 提供 **`IGameplayTagManager`（或 Registry）**：运行时解析字符串 → Tag；拒绝未注册 Tag（可配置严格模式）；列举/调试。
- **C# 原生注册**：代码生成或静态模块注册表（手写 `GameplayTags.State_Debuff_Burn` 常量 / partial 类均可）；Editor 下可校验重复与非法名。
- **扩展点**：未来从 Luban/JSON 合并进同一 Registry（同一规范化规则）；本 Feature 只定义接口与合并顺序，不实现导表。
- 与 EventBus **无强制耦合**；允许后续用 Tag 过滤，但不在本 Feature 改 EventBus。
- EditMode 测试覆盖：解析、父子查询、Container 运算、未知 Tag 行为（依赖 INFRA-F01 管道；若 F01 未合并可暂用同 PR 最小测试或延后用例）。

## Constraints

- Tag 名字符集：建议 `[A-Za-z][A-Za-z0-9_]*` 分段，以 `.` 分隔；大小写策略 **一种**（推荐保留大小写敏感 + 注册时检测冲突）。
- 不得在热路径上反复字符串 Split 而不缓存；解析结果应可比较（equality by id/index）。
- Shipping 包可剥离 Editor 校验；运行时 Registry 必须可用。
- 不引入 UE 插件或第三方 GameplayTag 资产包（保持自研、可控）。

## Preferences

- API 风格贴近现有 Infrastructure：接口 + 明确实现，由 `ApplicationController` 或惰性静态引导注册。
- 优先 **int 句柄 / 内部索引** 做相等比较，字符串仅用于序列化与调试。
- 中文文档；标识符英文。

## Assumptions

- GameJam 开题前 Tag 数量可控（百～千级），不需要复杂重载/重定向图。
- 外部配置导入会复用同一路径语法，不会另造一套 ID。
- 「父 Tag 匹配」需求存在（查 `State.Debuff` 时命中 `State.Debuff.Burn`）——若评审否定，可降为仅精确匹配。

## Non-Goals

- 不做完整 GameplayAbility / Cue / Tag 复制同步网络。
- 不做 Luban 表导出与 `IConfigProvider` 接入（属 CFG 后续 Feature）。
- 不做 Tag 驱动的通用行为树或效果系统。
- 不在本 Feature 实现 DebugConsole 命令（CONSOLE 侧调用 Registry）。

## Current Architecture

- Infrastructure 模块化；`ServiceLocator` 注册服务。
- Luban 生成代码在 `ScriptsGenerated/Configs/`，启动未注册 Config。
- 无现有 Tag 类型；Save/UI 等尚未依赖 Tag。

## Proposed Architecture

```text
Infrastructure/Tags/
  GameplayTag.cs              # 值类型：Id + 调试名
  GameplayTagContainer.cs     # 集合 + 查询
  IGameplayTagManager.cs      # 解析 / 注册查询
  GameplayTagManager.cs       # 实现
  GameplayTagLiteral.cs       # 可选：编译期常量助手
  Registration/
    IGameplayTagSource.cs     # 原生源 / 未来 Config 源
    NativeGameplayTagSource.cs
```

**匹配语义（推荐写入 API 文档并锁死）：**

| API | 语义 |
|-----|------|
| `Has(tag)` | 精确拥有该 Tag |
| `Has(tag, includeChildren: true)` | 拥有该 Tag **或其任意子 Tag** |
| `HasAny/All` | 对容器内请求集合作上述语义 |

**注册顺序：** Native Source →（未来）Config Source；冲突策略：**同名同路径合并，同名异定义失败**。

## Responsibilities

| Unit | Responsible for | Not responsible for |
|------|-----------------|---------------------|
| `GameplayTag` | 标识与比较 | 字符串解析 |
| `GameplayTagContainer` | 集合状态与查询 | 全局注册 |
| `GameplayTagManager` | 权威注册表、解析、校验 | 玩法效果 |
| `IGameplayTagSource` | 提供待注册条目 | 生命周期 |
| `ApplicationController` | （可选）注册 Manager 到 Locator | Tag 语义 |

## Boundaries

- Allowed：Infrastructure / Gameplay / TOOL 读取 Manager 与 Container。
- Forbidden：业务直接拼接未注册路径当「临时 Tag」绕过 Registry（严格模式）；在 Container 内藏玩法逻辑。
- 序列化：存 **完整路径字符串** 或稳定 Id（若用 Id，必须有版本迁移策略）；推荐路径字符串利于导表对齐。

## Dependencies

- Depends on: 无硬性；测试依赖 INFRA-F01（软依赖）。
- Depended on by: TOOL-F01、未来 Settings/GAS-like、CFG 导入。
- 不依赖 EventBus / Input。

## Ownership & Lifetime

- Manager：建议进程级单例式服务，随 `ApplicationController` 创建/注销；或静态 Registry + 显式 `Initialize`（二选一，推荐 **实例 + Locator**，便于测试）。
- Container：由持有者（Actor/Component/SaveDTO）拥有；值类型复制语义需明确（建议 struct 容器或 class + 文档说明拷贝成本）。

## Data Flow

`Source.Register → Manager 规范化/建树 → 调用方 RequestTag("A.B") → Tag 句柄 → Container.Add/Query`

## Control Flow

- 启动：创建 Manager → 加载 Native Source →（未来）Config Source → Register Locator。
- 运行：只读查询为主；动态 Add 运行时 Tag **默认禁止**（Open Question）。

## State & Invariants

- 每个合法路径至多一个 Tag 节点。
- 父节点可在子注册时隐式创建（或要求显式注册父 —— **推荐隐式创建父节点标记为 Implicit**，查询可用）。
- Container 不包含未注册 Tag。

## API / Interface Semantics

- `GameplayTag RequestTag(string path)`：失败时 — 严格模式抛错/返回 invalid；宽松模式返回 invalid 并打日志（**Jam 推荐严格**）。
- `bool IsValid(GameplayTag)`。
- `GameplayTagContainer`：`Add`/`Remove`/`Clear`/`Has*`；枚举只读视图。
- 线程：仅主线程（与现有服务一致）。

## Failure Model

- 非法字符 / 空段 / 尾部点号 → 注册失败。
- 未知 Tag 查询 → 严格失败。
- 配置源与原生冲突 → 启动失败（Fail Fast）。

## Alternatives & Trade-offs

### Option A (recommended)

- 自研轻量 Tag + Manager + Container；字符串序列化；严格注册。
- Pros：可控、易测、与 Luban 对齐简单。
- Cons：无现成编辑器生态。

### Option B

- 仅用 `enum` + flags。
- Why not：跨系统扩展差，导表困难。

### Option C

- 引入第三方 GameplayTag 包。
- Why not：与现有 Infrastructure 风格/所有权不一致，Jam 定制成本高。

## Integration Impact

- 新增 Infrastructure/Tags；可选 Locator 注册。
- Save/UI 暂不强制迁移。
- Console（TOOL-F01）将增加 `tag.list` / `tag.has` 类命令（属 Console Feature）。

## Migration / Compatibility

- 新系统；无旧数据。
- 预留 Config Source；未实现前 Native only。

## Verification Strategy

- EditMode：路径解析、父子 Has、Container 集合、冲突注册、非法名。
- 手工：Manager 在 Launch 注册后，临时调试日志打印根节点计数。

## Open Questions

1. Container 用 `struct` 还是 `class`？— **建议 class**（避免大容器复制）待确认。
2. 是否允许运行时动态注册 Tag？— **默认否**。
3. 父匹配默认开还是 API 显式参数？— **建议显式参数，避免静默扩大命中**。
4. 是否本 Feature 内做 ScriptableObject 作者表（非 Luban）？— **可选 S 切片，非必须**。

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
- [ ] Open Questions 由你确认

**Ready for implementation?** `no` — 待评审。

## Acceptance Checklist

- [ ] Design review complete when required by complexity/profile
- [ ] Implementation plan traces to design decisions (L2+)
- [ ] Progress log updated when status changes
- [ ] Feature registry status synced

## Suggested Slices

| Slice | Content |
|-------|---------|
| `INFRA-F02-S01` | Tag + Manager + Native 注册 + 精确查询 |
| `INFRA-F02-S02` | Container + HasAny/All + 父子匹配 |
| `INFRA-F02-S03` | Locator 接入 + EditMode 测试 |
| `INFRA-F02-S04` | `IGameplayTagSource` 配置扩展点（空实现/文档） |
