# INFRA-F02_GAMEPLAY_TAG Implementation Plan

## Meta
- **ID:** `INFRA-F02`
- **Design:** [INFRA-F02_GAMEPLAY_TAG_DESIGN.md](./INFRA-F02_GAMEPLAY_TAG_DESIGN.md)
- **Status:** `In Progress`
- **Last updated:** `2026-09-23`

## Decisions locked

| Open Question | Decision |
|---------------|----------|
| Container | `class` |
| 运行时动态注册 | 禁止（密封） |
| 父子匹配 | `includeChildren` 显式参数 |
| SO 作者表 | Deferred |

## Slices

| Slice | Status |
|-------|--------|
| S01 Tag + Manager + Native | Done |
| S02 Container + HasAny/All + 父子 | Done |
| S03 Locator 接入 + EditMode 测试 | Done |
| S04 Config Source 扩展点 | Done（接口 + README；无 Luban 实现） |

## Verification

- [ ] Unity 编译无错
- [ ] `GameplayTagTests` EditMode 全绿
- [ ] Launch 后 `ServiceLocator.Resolve<IGameplayTagManager>()` 非 null（可选手工）
