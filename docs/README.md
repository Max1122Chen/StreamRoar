# StreamRoar 文档

按任务查阅。文档描述当前基础设施与启动骨架，具体实现以代码、资产和配置为准。
Unity 项目在 `StreamRoar/`，版本见 [ProjectVersion.txt](../StreamRoar/ProjectSettings/ProjectVersion.txt)，
依赖见 [manifest.json](../StreamRoar/Packages/manifest.json)。

| 需要了解 | 查阅 |
| --- | --- |
| 启动顺序、服务装配与能力边界 | [Architecture](Architecture.md) |
| Luban 测试表、导出与运行时接入边界 | [Configuration](Configuration_Architecture.md) |
| UI、资源、事件、时间及服务生命周期 | [UI / Infrastructure](UI_and_Infrastructure.md) |
| 目录、命名与作者数据归属 | [Conventions](Asset_And_Code_Conventions.md) |
| 按改动选择验证路径 | [Verification](Verification.md) |
| 自动化测试约定与跑法 | [Testing](Testing.md) |
| Infrastructure API 调用示例 | [API Reference](../.agents/skills/infrastructure-usage/references/api.md) |
| 模块完整接入契约 | [Audio](../StreamRoar/Assets/Scripts/Infrastructure/Audio/README.md) · [Save](../StreamRoar/Assets/Scripts/Infrastructure/Core/Save/README.md) · [Pooling](../StreamRoar/Assets/Scripts/Infrastructure/Pooling/README.md) |
| Feature / 进度 / 债务（实施协作） | [docs/ai](ai/README.md) |
| 多 Agent 适配与扩展位 | [ADAPTERS](ai/ADAPTERS.md) |

协作与维护默认值放在 [AGENTS.md](../AGENTS.md)。产品说明在本目录；实施队列在 [docs/ai](ai/README.md)（dual-track）。按需更新受影响说明，无需为普通修复维护变更日志。

这些通用规则与文档结构改编自 MotionCore 提交 `d4f111929c2f518cfe0a3728748b24cacb351022`，
内容按 StreamRoar 当前代码与资产调整；原项目的战斗设计、角色契约和运行验证记录不作为本项目依据。
