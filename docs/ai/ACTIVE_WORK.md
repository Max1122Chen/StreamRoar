# Active Work

人工维护的短待办（建议同时 1–5 项）。

## Rules

- 只放当前可执行工作
- 每项关联已注册的 Feature ID
- 状态保持新鲜；过期行移出或归档

## Current queue

| Priority | Feature ID | Slice ID | Status | Owner | Next action |
|----------|------------|----------|--------|-------|-------------|
| 1 | `INFRA-F01` | `S01–S03` | Review | Max | Unity Test Runner 跑绿后用户 code review |
| 2 | `INFRA-F02` | — | Draft | Max | F01 通过后开实现 |
| 3 | `INFRA-F03` | — | Draft | Max | F01 通过后开实现 |
| 4 | `TOOL-F01` | — | Draft | Max | F03 后实施 |

## Blocked / waiting

- `INFRA-F01-S04` PlayMode 烟测 Deferred
- F02+ 等 F01 review 通过
