# INFRA-F01_UNITY_TEST_FRAMEWORK Implementation Plan

## Meta
- **ID:** `INFRA-F01`
- **Design:** [INFRA-F01_UNITY_TEST_FRAMEWORK_DESIGN.md](./INFRA-F01_UNITY_TEST_FRAMEWORK_DESIGN.md)
- **Status:** `In Progress`
- **Last updated:** `2026-09-23`

## Decisions locked for implementation

| Open Question | Decision |
|---------------|----------|
| Q1 asmdef / 目录 | **不拆 Core、不改伙伴 Infrastructure 布局**；测试放在 `Assets/Tests/Editor/`，编入 Editor 程序集直接访问运行时脚本 |
| Q2 batchmode | Deferred |
| Q3 PlayMode | Deferred（S04） |

## Slices

| Slice | Status | Notes |
|-------|--------|-------|
| `INFRA-F01-S01` | Done | `Assets/Tests/Editor` + 样板可发现（无生产 asmdef） |
| `INFRA-F01-S02` | Done | `docs/Testing.md` + Verification / 索引 |
| `INFRA-F01-S03` | Done | Locator / EventBus / Save 样板；`ClearForTests` 仅测试用 |
| `INFRA-F01-S04` | Deferred | PlayMode Launch 烟测 |

## Verification (author)

- [ ] Unity 打开 `StreamRoar/` 编译无错
- [ ] Test Runner → EditMode → 三类样板全绿
- [ ] Infrastructure 目录结构与伙伴原布局一致（无 `Core/`）

## Design traceability

- 样板三类测试 ↔ Design Requirements
- 不拆生产目录 ↔ 尊重既有结构（相对 Design「最小 asmdef」的落地调整）
- PlayMode 未做 ↔ Deferred S04
