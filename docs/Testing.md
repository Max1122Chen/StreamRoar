# StreamRoar 测试约定

本文件定义自动化测试分层与规则。手工验证菜单仍见 [Verification](Verification.md)。

## 如何运行

1. 用 Unity 打开工程根目录 `StreamRoar/`
2. 菜单 **Window → General → Test Runner**
3. 选择 **EditMode**，运行 `StreamRoar.Tests.Editor` 下用例（位于 `Assets/Tests/Editor/`）

说明：测试放在 `Editor` 文件夹，编入 Editor 程序集，可直接访问现有运行时脚本，**不拆动** Infrastructure 原有目录。若 Test Runner 未列出用例，在 Test Runner 面板开启对现有程序集的 EditMode 测试扫描（或确认已安装 `com.unity.test-framework`）。

命令行 batchmode 跑测留待后续。

## 目录

| 路径 | 用途 |
|------|------|
| `Assets/Scripts/Infrastructure/` | 生产基础设施（保持伙伴既有布局） |
| `Assets/Tests/Editor/` | EditMode 回归（仅 Editor，不进 Player） |

## 分层

1. **EditMode（默认门禁）** — 纯逻辑 / 弱 Unity 依赖契约；新增 Infrastructure 公共 API 应带用例或显式豁免。
2. **PlayMode** — 需要场景、服务启动或真实资源时再用；须尊重 Launch 所有权。
3. **手工 Verification** — 序列化资产、观感、完整启动路径；见 Verification.md。

## 命名与 Category

- 测试类 / 方法：英文 `PascalCase`
- `[Category("Infrastructure")]` 等短标签
- 断言消息优先说清期望

## 夹具规则

- 每个用例独立；`[SetUp]`/`[TearDown]` 清理静态与临时目录
- Save 测试使用 `Path.GetTempPath()` 下唯一子目录，禁止本机用户存档路径
- `ServiceLocator.ClearForTests()` 仅供测试隔离；正式业务禁止调用
- 能测实例则测实例（如 `EventBus`、`JsonSaveService`）
- 禁止在生产代码用 `#if UNITY_INCLUDE_TESTS` 塞玩法逻辑

## 何时升到 PlayMode

- 必须经过 `ApplicationController` / YooAsset / 场景加载才能证明时
- EditMode 无法构造的 Unity 生命周期或序列化行为

## 与协作工作流

- 改动 Infrastructure 契约：跑相关 EditMode 后再准备提交
- 验证基线见 `docs/ai/PROJECT_CONTEXT.md`
