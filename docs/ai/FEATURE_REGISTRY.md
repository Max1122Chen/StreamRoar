# Feature Registry

在实施规划前注册每一个新的 Feature ID。

Domain 词汇表见 `PROJECT_CONTEXT.md` §7：`INFRA` / `UI` / `GAME` / `AUDIO` / `CFG` / `TOOL` / `ASSET`。

| Feature ID | Title | Domain | Status | Design Doc | Implementation Plan | Owner | Notes |
|------------|-------|--------|--------|------------|---------------------|-------|-------|
| `INFRA-F01` | Unity Test 框架与测试规则 | INFRA | Done | [design](./INFRA/INFRA-F01_UNITY_TEST_FRAMEWORK_DESIGN.md) | [plan](./INFRA/INFRA-F01_UNITY_TEST_FRAMEWORK_IMPLEMENTATION.md) | Max | `Assets/Tests/Editor`；用户确认测试全绿 |
| `INFRA-F02` | GameplayTag 生态 | INFRA | In Progress | [design](./INFRA/INFRA-F02_GAMEPLAY_TAG_DESIGN.md) | [plan](./INFRA/INFRA-F02_GAMEPLAY_TAG_IMPLEMENTATION.md) | Max | Tags/ + Locator；待用户跑测 review |
| `INFRA-F03` | PlayerController / Input System 中枢 | INFRA | Draft | [design](./INFRA/INFRA-F03_PLAYER_CONTROLLER_INPUT_DESIGN.md) | — | Max | 下一项 |
| `TOOL-F01` | DebugConsole（严格命令集） | TOOL | Draft | [design](./TOOL/TOOL-F01_DEBUG_CONSOLE_DESIGN.md) | — | Max | 建议接在 F03 后 |

## Status guidance

- Draft: 有概念，尚未可实施
- Planned: 已批准实施
- In Progress: 开发中
- Review: 待验证/评审
- Done: 已接受
- Blocked / Deferred / Cancelled: 在相关文档写明原因与后续条件

## Notes

- 建议实施顺序：`INFRA-F01` → `INFRA-F02` → `INFRA-F03` → `TOOL-F01`
