# INFRA-F01_UNITY_TEST_FRAMEWORK Implementation Plan

## Meta
- **ID:** `INFRA-F01`
- **Design:** [INFRA-F01_UNITY_TEST_FRAMEWORK_DESIGN.md](./INFRA-F01_UNITY_TEST_FRAMEWORK_DESIGN.md)
- **Status:** `In Progress`
- **Last updated:** `2026-09-23`

## Decisions locked for implementation

| Open Question | Decision |
|---------------|----------|
| Q1 asmdef 范围 | **最小 Core**：仅 `ServiceLocator` / Events / Save → `StreamRoar.Infrastructure.Core`（避免一次拆全量 Infrastructure 及其包引用） |
| Q2 batchmode | Deferred |
| Q3 PlayMode | Deferred（F01-S04 / 后续） |

## Slices

| Slice | Status | Notes |
|-------|--------|-------|
| `INFRA-F01-S01` | Done | Core asmdef + EditMode 程序集 |
| `INFRA-F01-S02` | Done | `docs/Testing.md` + Verification / 索引更新 |
| `INFRA-F01-S03` | Done | Locator / EventBus / Save 样板测试 |
| `INFRA-F01-S04` | Deferred | PlayMode Launch 烟测 |

## Verification (author)

- [ ] Unity 打开 `StreamRoar/` 编译无错
- [ ] Test Runner → EditMode → `StreamRoar.Tests.EditMode` 全绿
- [ ] 文档链接可点：Testing / Verification / Save README 新路径

## Design traceability

- 样板三类测试 ↔ Design Requirements
- `ClearForTests` + InternalsVisibleTo ↔ Design API（测试钩子）
- PlayMode 未做 ↔ Non-Goals / Deferred S04
