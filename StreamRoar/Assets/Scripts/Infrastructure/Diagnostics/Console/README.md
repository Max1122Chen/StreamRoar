# Debug Console

中性开发期命令 REPL（`TOOL-F01`）。

- 真实现：`UNITY_EDITOR || DEVELOPMENT_BUILD`
- Shipping：`NoopDebugConsole`
- 元命令：`help` / `clear` / `echo`
- 域命令：各系统 `ServiceLocator.Resolve<IDebugConsole>()?.Register(...)`
- UI：`DebugConsoleView`（OnGUI）；Toggle `` ` ``（IMC 前过渡，见 `DEBT-TOOL-001`）
- 智能提示：命令名阶段前缀实时候选；↑↓ 选中；Tab 接受；有候选时↑↓ 不走历史

设计见 `docs/ai/TOOL/TOOL-F01_DEBUG_CONSOLE_DESIGN.md`。
