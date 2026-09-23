# Feature Registry

在实施规划前注册每一个新的 Feature ID。

Domain 词汇表见 `PROJECT_CONTEXT.md` §7：`INFRA` / `UI` / `GAME` / `AUDIO` / `CFG` / `TOOL` / `ASSET`。

| Feature ID | Title | Domain | Status | Design Doc | Implementation Plan | Owner | Notes |
|------------|-------|--------|--------|------------|---------------------|-------|-------|
| `INFRA-F01` | Unity Test 框架与测试规则 | INFRA | Draft | [design](./INFRA/INFRA-F01_UNITY_TEST_FRAMEWORK_DESIGN.md) | — | Max | EditMode 优先；为后续 Feature 提供验收挂点 |
| `INFRA-F02` | GameplayTag 生态 | INFRA | Draft | [design](./INFRA/INFRA-F02_GAMEPLAY_TAG_DESIGN.md) | — | Max | C# 原生优先；Luban 源为扩展点 |
| `INFRA-F03` | PlayerController / Input System 中枢 | INFRA | Draft | [design](./INFRA/INFRA-F03_PLAYER_CONTROLLER_INPUT_DESIGN.md) | — | Max | 无玩法逻辑；禁旧版键态轮询 |
| `TOOL-F01` | DebugConsole（严格命令集） | TOOL | Draft | [design](./TOOL/TOOL-F01_DEBUG_CONSOLE_DESIGN.md) | — | Max | Lexer/Parser 前端；非脚本语言；建议接在 F03 Freeze 后 |

## Status guidance

- Draft: 有概念，尚未可实施
- Planned: 已批准实施
- In Progress: 开发中
- Review: 待验证/评审
- Done: 已接受
- Blocked / Deferred / Cancelled: 在相关文档写明原因与后续条件

## Notes

- `StreamRoar/Assets/Scripts/Infrastructure/ROADMAP.md` 与 `docs/` 为**参考**；要做某项时先在此表注册 Feature，再写入 `ACTIVE_WORK.md`
- 建议实施顺序：`INFRA-F01` → `INFRA-F02` → `INFRA-F03` → `TOOL-F01`（F02/F03 可部分并行；Console 依赖 Freeze 接口可 stub）
- AppFlow/Pause、Settings、Config 启动接入等尚未注册，评审通过本批后再开
