# Project Context

Last updated: 2026-09-23

## 1) Project goal

One-line mission:
- StreamRoar：基于 Unity 的游戏项目；当前阶段提供可复用的基础设施与启动骨架，玩法按需求增量实现。

Primary success criteria:
- Launch → 基础服务与 UI 就绪 → SampleScene 路径稳定可验证
- Infrastructure 接口清晰、所有权边界明确，新增玩法可沿现有模块扩展而非平行造轮子

## 2) Architecture direction

High-level architecture summary:
- Unity 工程根目录：`StreamRoar/`（Unity 2022.3 LTS）
- 启动：`Assets/Scenes/Launch.unity` → `ApplicationController` 注册服务与 UI → `SampleScene`
- 资源：YooAsset（Launch）；`ResourcesAssetProvider` 仅对应真实 `Assets/Resources` 布局
- 配置：Luban 源表与工具在仓库外 `../config/`、`../tools/`；生成代码在 `Assets/ScriptsGenerated/Configs/`（不手改）；配置服务尚未接入启动

Core modules:
- `ApplicationLifecycle` — `ApplicationController` 服务生命周期
- `Infrastructure` — ServiceLocator、Assets、Audio、Events、Timing、UI、VFX、Save、Scene、Pooling、Cursor、Configuration
- `Gameplay` — 玩法层（起步阶段）
- Editor — `GitHookInstaller`、SceneBootstrapper、UI 配置编辑器等

## 3) Current phase

Current stage:
- 基础设施骨架已落地；玩法与多数 ROADMAP 项按需推进

Next milestone:
- 按实际需求从 `StreamRoar/Assets/Scripts/Infrastructure/ROADMAP.md` 选取（Settings / 场景加载流程 / 资源生命周期等），先在 `docs/ai` 注册 Feature 再实施

## 4) Collaboration conventions

- Planning truth: `ACTIVE_WORK.md`, `FEATURE_REGISTRY.md`, `TECH_DEBT.md`, recent `PROGRESS_LOG.md`
- Product / architecture truth (dual-track): `docs/` + root `AGENTS.md` + module READMEs；**不要**把旧 ROADMAP 当自动 backlog
- Delivery bar: follow `templates/DOC_GOVERNANCE.md` and Slice DoD for L2+；日常修复遵循 `AGENTS.md` 轻文档规则
- Session recovery: use `BOOTSTRAP_DIGEST.md`
- Commit messages: Conventional Commits（中文标题可）；由 `.githooks/commit-msg` 校验
- Agent adapters: see `docs/ai/ADAPTERS.md`

## 5) Verification baseline

- Build/verify command: 打开 Unity 工程 `StreamRoar/`，确认导入/编译无新错误；工具路径必须指向本仓库该目录
- Smoke test command: 从 `Launch.unity` 进入 Play Mode，等待 YooAsset/UI 就绪并进入 SampleScene；退出后检查 Console；细节菜单见 `docs/Verification.md`
- Note: `dotnet build` / 静态搜索 **不能** 证明 Unity 运行行为；无自动化测试套件时 `verification_bar=smoke-required`

## 6) Optional dual-track docs mode

- **Dual-track（已启用）**: 产品/架构真相在 `docs/`（及 `AGENTS.md`）；本文件夹 `docs/ai/` 只存实施协作工作流（Feature/Slice/进度/债务/会话）

## 7) Domain codes

| Code | Meaning |
|------|---------|
| `INFRA` | 基础设施服务与启动骨架 |
| `UI` | UI 系统 / 配置编辑器 / RuntimeBootstrap |
| `GAME` | 玩法与玩法 UI |
| `AUDIO` | 音频服务与 Mixer/Bus |
| `CFG` | Luban 配置导出与运行时接入 |
| `TOOL` | Editor 工具、钩子、导出脚本 |
| `ASSET` | YooAsset / 资源提供与打包 |

新增 Domain 时更新本表与 `FEATURE_REGISTRY.md` 说明，保持短、稳、全大写。
