# INFRA-F03_PLAYER_CONTROLLER_INPUT Design Spec

## Meta
- **ID:** `INFRA-F03`
- **Type:** `Feature`
- **Complexity:** `L2`
- **Status:** `Draft`
- **Owner:** Max
- **Last updated:** `2026-09-23`
- **Related:** [Feature Registry](../FEATURE_REGISTRY.md) · [Architecture](../../Architecture.md) · INFRA-F01 · TOOL-F01 ·（后续 AppFlow/Settings）

## TL;DR

统一以 **Unity Input System** 为唯一正式输入路径，引入无玩法逻辑的 **PlayerController 中枢**（Action Map 切换、输入冻结、设备变更、意图出口）。绝对禁止正式业务直接读取旧版 `Input`/`Keyboard.current` 轮询键态。当前 Draft，待评审。

## Problem

- 包已引入 `com.unity.inputsystem`，但无项目级中枢与规矩，Jam 中易退回 `Input.GetKey`。
- 缺少「UI / 过场 / Console 打开时冻结玩法输入」的单一控制点。
- 玩法未定时，仍需先定 **控制层边界**，避免开题后各系统私自读设备。

## Requirements

- 使用 **新版 Input System**；项目设置切换到 Input System（或 Both 仅作迁移期 —— **正式业务路径只走新版**）。
- 提供 `PlayerController`（或 `IPlayerInputHub`）：
  - 持有 `PlayerInput` / `InputActionAsset` 引用
  - Action Map 切换（如 `Gameplay` / `UI` / `Menu`）
  - **Input Freeze** 计数或栈（多源：UI、Console、Pause、过场）
  - 设备丢失/重连的可观察信号（事件或回调）
- **意图出口**：把 Action 变成项目级信号（推荐 `IEventBus` 事件或窄接口 `IGameplayInputReader`），**不含**移动/战斗/交互逻辑。
- **禁止规则**写入 `docs/Asset_And_Code_Conventions.md` + `AGENTS.md` 短条款；提供 Editor/测试样例说明反模式。
- EditMode/或有限 PlayMode：冻结时 Gameplay Map 不回调；Map 切换成功。
- 默认场景中可放置/生成一个 DDOL 或挂在 GameRoot 旁的控制器占位（与 `ApplicationController` 共存策略写清）。

## Constraints

- 不引入旧 `UnityEngine.Input` 作为正式 API（调试后门若需要，仅限 Editor/`DEBUG` 且不得进 Gameplay 程序集公共路径）。
- PlayerController **不得**引用具体玩法类型（敌、枪、背包等）。
- 不与 UI Toolkit/uGUI 抢焦点时的行为需可配置：默认「UI Map 激活时冻结 Gameplay Map」。
- 与现有 `DefaultExecutionOrder(-10000)` 的 ApplicationController 协调，避免输入 Tick 早于服务注册。

## Preferences

- Input Actions 用 `.inputactions` 资产（Unity 标准），版本进仓库。
- 事件用现有 struct `IEvent` 模式发布「输入意图」（如 `MoveIntentEvent` 可延后到有玩法时再加；本 Feature 可先只做 Hub API + Freeze + Map）。
- **第一切片可不发任何玩法事件**，只交付 Hub + 冻结 + Map + 文档门禁。

## Assumptions

- 单本地玩家足够（Splitscreen 非目标）。
- Jam 键鼠 + 手柄；触屏后置。
- 「绝对禁用旧版」靠约定 + 抽检 +（可选）后期分析；完整 Roslyn 分析器非必须。

## Non-Goals

- 不做键位重绑 UI（属 Settings Feature；本 Feature 预留 rebind API 缺口说明即可）。
- 不做玩法移动/相机/战斗。
- 不做网络输入预测。
- 不实现完整 AppFlow 状态机（可提供 `SetMap`/`PushFreeze` 供其调用）。

## Current Architecture

- `ApplicationController` 拥有服务生命周期；Gameplay 目录几乎为空。
- `IEventBus` 同步结构体事件已存在。
- Input System 包在 `manifest.json`；未见项目 Input Actions 资产与 PlayerInput 装配。

## Proposed Architecture

```text
Infrastructure/Input/  （或 Gameplay/Input/ —— 推荐 Infrastructure，因无玩法）
  IPlayerInputHub.cs
  PlayerInputHub.cs          # 或组件名 PlayerController
  InputFreezeSource.cs       # enum/flags：UI, Console, Pause, Cinematic, External
  PlayerInputEvents.cs       # 可选：DeviceLost 等

Assets/Settings/Input/
  StreamRoarInput.inputactions

文档：
  docs/Asset_And_Code_Conventions.md  # 输入门禁
  AGENTS.md 短引用
```

**Freeze 模型（推荐）：** 引用计数 / 栈按 source 位；任一 source 激活 ⇒ Gameplay actions 禁用；Console 自己的 Map 可单独开。

**与 ApplicationController：** Hub 在 Awake 末或 Start 前注册到 Locator；销毁时注销。组件可挂在同一 GameRoot。

## Responsibilities

| Unit | Responsible for | Not responsible for |
|------|-----------------|---------------------|
| `IPlayerInputHub` | Map、Freeze、设备信号、Action 启用策略 | 玩法逻辑、UI 绘制 |
| `Input Action Asset` | 动作与绑定定义 | 运行时策略 |
| `ApplicationController` | 创建/注册时机 | 绑定具体键 |
| 业务系统 | 订阅意图或读 Hub 快照 | 直接读设备 |

## Boundaries

- Allowed：Hub → EventBus；UI/Console → `PushFreeze`/`PopFreeze`；Settings 未来 → rebind。
- Forbidden：Gameplay 代码 `Keyboard.current.*.isPressed`；`Input.GetAxis`；绕过 Hub 启用 Action。
- UI 模块可通过 Hub 切到 UI Map，不得直接 Disable 他人组件。

## Dependencies

- Depends on: Input System 包；可选 EventBus。
- Soft：TOOL-F01 Console 将调用 Freeze；INFRA-F01 测冻结。
- Depended on by: 一切玩法输入、Settings 重绑。

## Ownership & Lifetime

- Hub 组件：与 GameRoot 同生命周期（DDOL）。
- Action Asset：项目资产，只读运行时实例可 `Instantiate` 避免多实例冲突（若多 Hub，Jam 默认单例）。

## Data Flow

`Device → Input System → Action → Hub 策略(Freeze/Map) →（可选）EventBus / 轮询快照 → 未来玩法`

## Control Flow

- `PushFreeze(Console)` → Gameplay Map disable → Console Map enable。
- `PopFreeze(Console)` → 恢复先前 Map（栈式）。
- 设备丢失 → Publish `InputDeviceChangedEvent`（若做事件切片）。

## State & Invariants

- Freeze 源可叠加；计数归零才解冻。
- 同一时刻至多一个「主」Gameplay Map 启用（UI/Console 例外）。
- 未注册 Hub 时业务不得静默读设备（应失败可见）。

## API / Interface Semantics

- `void SetMap(string mapName)` / `void SetMap(InputMapId id)`
- `void PushFreeze(InputFreezeSource source)` / `void PopFreeze(...)`
- `bool IsFrozen { get; }`
- `InputAction FindAction(string)` — 谨慎暴露；更推荐 typed 访问器后置。
- Errors：未知 Map → 抛错或日志+no-op（推荐抛错于 Editor）。
- Sync：主线程。

## Failure Model

- Action Asset 丢失 → 启动失败可见。
- 双 Hub → 第二实例拒绝或警告（`DisallowMultipleComponent` / 单例检查）。

## Alternatives & Trade-offs

### Option A (recommended)

- 单一 `IPlayerInputHub` + Input Actions 资产 + Freeze 栈 + 文档门禁；玩法事件后置。
- Pros：边界清晰，开题前可合并。
- Cons：短期「看不见」输入效果（需 Console/测试证明）。

### Option B

- 各玩法组件自带 `PlayerInput`。
- Why not：冻结与 Map 无法统一，违背中枢目标。

### Option C

- 仅改 Project Settings 禁旧输入，不做 Hub。
- Why not：无法解决多源冻结与规矩落地。

## Integration Impact

- GameRoot 场景改动；可能改 Project Settings（Active Input Handling）。
- Conventions / AGENTS 更新。
- Console、未来 Pause 依赖 Freeze API。

## Migration / Compatibility

- 无旧输入代码则直接切 Input System Only。
- 若 Both：限时，登记 TECH_DEBT 退出条件「Only」。

## Verification Strategy

- EditMode：Freeze 栈逻辑（可纯 C# 测 Hub 策略对象）。
- PlayMode/手工：Launch 后 Hub 存在；PushFreeze 后 Gameplay Action 不触发（可用调试计数）。
- 代码约定：抽检无 `Input.GetKey` / 业务 `Keyboard.current`。

## Open Questions

1. 类型放 `Infrastructure.Input` 还是 `Gameplay`？— **推荐 Infrastructure**。
2. 组件名 `PlayerController` vs `PlayerInputHub`？— **对外可称 PlayerController，类名建议 Hub 以免误解含玩法**。
3. 第一切片是否包含任一 `Move` Action 与事件？— **建议否，仅 Map+Freeze**。
4. Active Input Handling：`Input System Package` only？— **推荐 yes**。

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

**Ready for implementation?** `no` — 待评审。

## Acceptance Checklist

- [ ] Design review complete when required by complexity/profile
- [ ] Implementation plan traces to design decisions (L2+)
- [ ] Progress log updated when status changes
- [ ] Feature registry status synced

## Suggested Slices

| Slice | Content |
|-------|---------|
| `INFRA-F03-S01` | Input Actions 资产 + Hub 组件 + Locator |
| `INFRA-F03-S02` | Freeze 栈 + Map 切换 |
| `INFRA-F03-S03` | 文档门禁 + 测试 |
| `INFRA-F03-S04` | （可选）设备事件 + 最小 Action 出口 |
