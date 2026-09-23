# INFRA-F01_UNITY_TEST_FRAMEWORK Design Spec

## Meta
- **ID:** `INFRA-F01`
- **Type:** `Feature`
- **Complexity:** `L2`
- **Status:** `In Progress`
- **Owner:** Max
- **Last updated:** `2026-09-23`
- **Related:** [Feature Registry](../FEATURE_REGISTRY.md) · [Verification](../../Verification.md) · [WORKFLOW_PROFILE](../WORKFLOW_PROFILE.md) · [Implementation](./INFRA-F01_UNITY_TEST_FRAMEWORK_IMPLEMENTATION.md)

## TL;DR

项目缺少可重复的自动化验证层，`verification_bar` 只能停在 smoke。本设计引入 Unity Test Framework（EditMode 为主、PlayMode 选择性），并定义测试分层、命名、夹具与「什么必须测」的规则，使 Infrastructure 契约可回归。当前为 Draft，待评审。

## Problem

- 无测试程序集与约定时，服务契约（EventBus、Save、Tag、Input 冻结等）只能靠手工 Play Mode。
- `docs/Verification.md` 是菜单式说明，不能在 CI/本地一键复跑。
- 后续 INFRA-F02/F03、TOOL-F01 的验收缺少统一挂点。

## Requirements

- 使用 Unity 官方 **Test Framework**（`com.unity.test-framework`，随 development feature 已可能存在；实现时核对 `manifest.json`）。
- 至少支持 **EditMode** 测试：纯 C# / 弱依赖 UnityEngine 的基础设施契约。
- 定义并文档化：**测试分层、命名、目录、夹具、禁止事项、何时升到 PlayMode**。
- 为现有模块提供 **样板测试**（建议：`ServiceLocator` 注册/注销、`EventBus` 订阅发布与 scope、`JsonSaveService` 读写/损坏路径 —— 以临时目录隔离）。
- 更新 `docs/Verification.md` 与 `docs/ai/PROJECT_CONTEXT.md` 的验证基线；profile 在具备稳定 EditMode 套件后可将相关模块视为 `tests-required`。
- 测试失败信息需可读；不依赖外部网络与真实用户存档路径。

## Constraints

- Unity 工程根为 `StreamRoar/`；测试程序集必须能被该工程编译与 Test Runner 发现。
- 不得把 `Library/`、生成缓存提交进仓库。
- PlayMode 测试若触及服务启动，必须从 **Launch 语义** 出发或使用经批准的测试引导夹具，禁止「假装已 Boot」却跳过所有权规则。
- 静态 `rg` / `dotnet build` 仍不能替代 Unity 测试结果（与 `AGENTS.md` 一致）。
- 提交钩子与 Conventional Commits 规则不变。

## Preferences

- 先 EditMode 覆盖面，再少量 PlayMode 烟测；避免一上来全量场景测试。
- 测试代码中文注释可选；**测试名与断言消息英文或双语均可，优先清晰**（建议：方法名英文 `PascalCase`，`[Category]` 用短标签）。
- 与 serious-engineering 姿态一致：改契约必须有对应用例或显式豁免说明。

## Assumptions

- `com.unity.feature.development` / Test Framework 可在现有 2022.3 工程启用（实现切片需确认包版本）。
- GameJam 期间未必有 CI Agent；本地 Test Runner 与文档命令即可，CI 为后续可选。
- 团队接受「Infrastructure 公共 API 无测试不合并」的门禁（至少对新增公共类型）。

## Non-Goals

- 不做完整游戏玩法自动化、不做截图对比、不做性能基准框架。
- 不做第三方测试框架替换（NUnit/Unity Test 以外）。
- 本 Feature **不**实现 GameplayTag / Input / Console 的全套用例（由对应 Feature 自带；本 Feature 只提供样板与规则）。
- 不强制本阶段接入 GitHub Actions。

## Current Architecture

- 服务由 `ApplicationController` 在 Awake 创建并 `ServiceLocator.Register`，OnDestroy 注销并 Dispose。
- 文档验证入口：`docs/Verification.md`；协作验证条：`WORKFLOW_PROFILE.verification_bar = smoke-required`。
- 已有模块：EventBus、Timer/GameTime、Save、Assets、Audio、UI、VFX、Scene 等；无 `Tests` 程序集。
- Editor 已有 `GitHookInstaller`、`SceneBootstrapper` 等，与测试无关但说明 Editor 程序集模式可参考。

## Proposed Architecture

```text
StreamRoar/Assets/
  Scripts/           # 生产代码（现有）
  Tests/
    EditMode/        # 程序集：StreamRoar.Tests.EditMode
      Infrastructure/
      ...
    PlayMode/        # 程序集：StreamRoar.Tests.PlayMode（可选，后置切片）
      Smoke/
docs/
  Verification.md    # 增加「自动化」章节
  Testing.md         # 新建：规则与如何跑
```

- **EditMode**：`asmdef` 引用生产 asmdef（若尚无生产 asmdef，本 Feature 可顺带为 `Infrastructure` 等建立最小 asmdef，或 Tests 引用全量 Scripts —— 实现时选「最小扰动」方案并在 Implementation Plan 写明）。
- **规则文档** `docs/Testing.md`：分层、命名、夹具、Category、禁止直接改生产单例残留等。
- **样板**：3 组 EditMode 测试证明管道可用。

## Responsibilities

| Unit | Responsible for | Not responsible for |
|------|-----------------|---------------------|
| `docs/Testing.md` | 规则与跑法 | 具体业务断言 |
| `StreamRoar.Tests.EditMode` | 契约/纯逻辑回归 | 场景美术验收 |
| `StreamRoar.Tests.PlayMode` | Launch/服务烟测（后置） | 替代 EditMode |
| 各 Feature 作者 | 为本 Feature 公共 API 补测 | 改测试框架本身 |
| `Verification.md` | 手工 + 自动菜单索引 | 实现测试 |

## Boundaries

- Allowed：测试调用公共 API；使用临时目录；`Category("Infrastructure")` 等标签。
- Forbidden：测试依赖本机绝对路径用户存档；测试间共享可变静态脏状态不清理；在生产代码中 `#if UNITY_INCLUDE_TESTS` 塞入玩法逻辑（仅允许明确的测试钩子且需评审）。
- 生产代码不引用 Tests 程序集。

## Dependencies

- Depends on: Unity Test Framework；现有 Infrastructure 公共类型。
- Depended on by: INFRA-F02/F03、TOOL-F01 的验收；未来 CI。
- New dependency direction：Tests → Production（单向）。

## Ownership & Lifetime

- 测试夹具自行创建/销毁被测对象；禁止泄漏到后续用例。
- `ServiceLocator` 若被测，每个用例前后必须清空或隔离（若当前 API 不足，本 Feature 可增加 **仅测试可见** 的 reset 钩子，或测试避免走全局 Locator 而测具体实例 —— **优先测实例**）。

## Data Flow

`作者改 API → 写/更新 EditMode 用例 → Test Runner → 失败信息 → 修复`

## Control Flow

- 本地：Unity Editor → Window → General → Test Runner → EditMode → Run。
- 可选后置：命令行 batchmode 跑测（实现计划写具体参数；本设计只要求「可文档化」）。

## State & Invariants

- 每个测试独立；失败不得污染后续用例。
- `ServiceLocator` 全局状态：若测试触及，结束时不变成「脏 Locator」。

## API / Interface Semantics

本 Feature 以约定与样板为主；若需测试钩子：

- `ServiceLocator` 测试隔离：优先不测静态 Locator，改测可注入实例；若必须，增加内部/测试程序集可见的 `ClearForTests()`，**Shipping 剥离或 InternalsVisibleTo**。
- 错误：钩子误用于生产 → 代码审查拒绝。

## Failure Model

- 包缺失 / asmdef 环依赖 → 编译失败，实现切片先修工程结构。
- PlayMode 不稳定 → 标 `[Explicit]` 或移出默认套件，不阻塞 EditMode 门禁。

## Alternatives & Trade-offs

### Option A (recommended)

- Unity Test Framework + EditMode 优先 + 规则文档 + 3 样板。
- Pros：官方、与 Editor 一体、成本低。
- Cons：CI 需自建；PlayMode 仍贵。

### Option B

- 仅用外部 `dotnet test` 测纯 C#。
- Why not：大量类型依赖 UnityEngine；与现有验证叙事不一致。

### Option C

- 先只写文档不定程序集。
- Why not：无法证明管道，后续 Feature 仍无挂点。

## Integration Impact

- 可能新增 asmdef，影响编译图与 IDE。
- 更新 Verification / PROJECT_CONTEXT；评审通过后调整 `verification_bar` 说明（不必立刻改 profile 枚举值，可在 Effective behavior 注明「Infrastructure 公共 API 需 EditMode」）。

## Migration / Compatibility

- 无运行时兼容问题。
- 旧手工验证路径保留。

## Verification Strategy

- Core：EditMode 样板全绿；`docs/Testing.md` 与目录一致。
- Failure：故意失败用例在开发中验证输出可读（不提交）。
- Integration：从干净工程打开 Test Runner 能发现程序集。

## Open Questions

1. 生产代码是否立即拆 asmdef，还是 Tests 暂时引用较宽程序集？— **Resolved：最小 Core asmdef（Locator/Events/Save）**
2. 是否本阶段写 batchmode 脚本？— **Deferred**
3. PlayMode 烟测是否纳入 F01 同一 PR？— **Deferred（S04）**

## Design Review

- **Verdict:** `Ready with deferred items`（PlayMode / batchmode 后置；实现已按 Core 方案推进）
- **Reviewer / date:** Max / 2026-09-23（用户确认设计大体无问题后进入实现）
- **Link or summary:** 用户指示开始逐 Feature 实现；Open Questions 按推荐决议落地

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
- [x] Unresolved questions resolved or explicitly deferred/accepted

**Ready for implementation?** `yes`

## Acceptance Checklist

- [ ] Design review complete when required by complexity/profile
- [ ] Implementation plan traces to design decisions (L2+)
- [ ] Progress log updated when status changes
- [ ] Feature registry status synced

## Suggested Slices (for later Implementation Plan)

| Slice | Content |
|-------|---------|
| `INFRA-F01-S01` | asmdef + EditMode 空套件可跑 |
| `INFRA-F01-S02` | `docs/Testing.md` + Verification 索引 |
| `INFRA-F01-S03` | ServiceLocator/EventBus/Save 样板测试 |
| `INFRA-F01-S04` | （可选）PlayMode Launch 烟测 |
