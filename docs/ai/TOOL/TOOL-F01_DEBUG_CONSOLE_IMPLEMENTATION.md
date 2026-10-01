# TOOL-F01_DEBUG_CONSOLE Implementation Plan

## Meta
- **ID:** `TOOL-F01`
- **Status:** `Review`
- **Owner:** Max
- **Last updated:** `2026-09-27`
- **Related:** [Design Spec](./TOOL-F01_DEBUG_CONSOLE_DESIGN.md)
- **Design readiness:** `Ready with deferred items`（IMC_Console 接线记 TECH_DEBT）

## Goal

落地中性 Debug Console 内核：分词/注册/执行/元命令/OnGUI/启动接线；域命令与 CVar 不在范围。

## Design Decisions Being Implemented

| Decision | Design section | Notes |
|----------|----------------|-------|
| D1 OnGUI View | Proposed Architecture | 不进 UISystemConfig |
| D2 仅元命令 | Requirements | help / clear / echo |
| D3 显式 Register | Preferences | 域命令后接 |
| D4 查找不敏感 | Open Questions #2 | 显示用注册名 |
| D5 Toggle `` ` `` | Open Questions #1 | 过渡读 Keyboard |
| D6 Shipping 剥离 | Constraints | Editor/Development 真实现；否则 Noop |

## Implementation Slices

| Slice ID | Summary | Verification | Status |
|----------|---------|--------------|--------|
| `TOOL-F01-S01` | Tokenizer / Registry / Executor / History / Meta + EditMode | Test Runner | Done（待用户跑测） |
| `TOOL-F01-S02` | OnGUI View + ApplicationController | 手工 Play | Done（待用户烟测） |
| `TOOL-F01-S03` | Noop Shipping 路径 + TECH_DEBT(IMC) | 代码审查 | Done |

## Integration Points

- `ApplicationController` `#if` 注册 `IDebugConsole` 与 View
- `ServiceLocator.Resolve<IDebugConsole>()` 供域命令后接

## Risks / Mitigations

- Risk: OnGUI 与 Input System 焦点冲突 → 打开时 TextField 抢焦点；关闭释放
- Risk: F03 未就绪 → TECH_DEBT 过渡读键

## Out of Scope

- 域命令、CVar、IMC_Console 资产
