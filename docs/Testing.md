# StreamRoar 测试约定

本文件定义自动化测试分层与规则。手工验证菜单仍见 [Verification](Verification.md)。

## 如何运行

1. 用 Unity 打开工程根目录 `StreamRoar/`
2. 菜单 **Window → General → Test Runner**
3. 选择 **EditMode**，运行 `StreamRoar.Tests.EditMode`

命令行 batchmode 跑测留待后续；当前以 Editor Test Runner 为准。

## 目录与程序集

| 路径 | 程序集 | 用途 |
|------|--------|------|
| `Assets/Scripts/Infrastructure/Core/` | `StreamRoar.Infrastructure.Core` | 可 EditMode 测的核心契约（Locator / EventBus / Save） |
| `Assets/Tests/EditMode/` | `StreamRoar.Tests.EditMode` | EditMode 回归（仅 Editor） |
| `Assets/Tests/PlayMode/` | （未建） | Launch 烟测后置 |

生产程序集不引用 Tests。Tests → Core 单向依赖。

## 分层

1. **EditMode（默认门禁）** — 纯逻辑 / 弱 Unity 依赖契约；新增 Infrastructure 公共 API 应带用例或显式豁免。
2. **PlayMode** — 需要场景、服务启动或真实资源时再用；须尊重 Launch 所有权，禁止伪造 Boot。
3. **手工 Verification** — 序列化资产、观感、完整启动路径；见 Verification.md。

## 命名与 Category

- 测试类 / 方法：英文 `PascalCase`（例：`Register_ThenResolve_ReturnsSameInstance`）
- `[Category("Infrastructure")]` 等短标签，便于过滤
- 断言消息优先说清期望

## 夹具规则

- 每个用例独立；`[SetUp]`/`[TearDown]` 清理静态与临时目录
- 禁止依赖本机用户存档路径；Save 测试使用 `Path.GetTempPath()` 下唯一子目录
- `ServiceLocator`：优先测注册语义时用 `ClearForTests()`（`internal`，仅 Tests 可见）；能测实例则测实例（如 `EventBus`）
- 禁止在生产代码用 `#if UNITY_INCLUDE_TESTS` 塞玩法逻辑

## 何时升到 PlayMode

- 必须经过 `ApplicationController` / YooAsset / 场景加载才能证明时
- EditMode 无法构造的 Unity 生命周期或序列化行为

## 与协作工作流

- 改动 Infrastructure Core 契约：跑相关 EditMode 后再准备提交
- 验证基线见 `docs/ai/PROJECT_CONTEXT.md`；profile 对已覆盖模块按「有测则测」执行
