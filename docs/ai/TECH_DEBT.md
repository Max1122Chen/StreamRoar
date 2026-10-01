# Tech Debt

跟踪有意保留的债务。过渡代码不得无记录遗留。

| Debt ID | Status | Area | Summary | Impact | Owner | Opened | Revisit date | Exit condition |
|---------|--------|------|---------|--------|-------|--------|--------------|----------------|
| `DEBT-TOOL-001` | Open | TOOL-F01 / Input | DebugConsoleView 用 Input System `Keyboard` 读 `` ` `` Toggle，未走 IMC_Console | 与 INFRA-F03 规矩临时不一致；Console 开时未 AddMappingContext | Max | 2026-09-27 | F03 落地后 | F03 提供 `IMC_Console` 后，Toggle/历史/提交改走 IMC；删除 View 内过渡读键 |

## Status

- Open
- In Progress
- Resolved
- Rejected
