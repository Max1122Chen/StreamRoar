# Agent Adapters

本仓库采用 **核心协作文档 + 多 Agent 适配层**。产品规则以根目录 `AGENTS.md` 为准；协作流程以 `docs/ai/` 为准。

## Layers

| Layer | Path | Role |
|-------|------|------|
| Product rules | `AGENTS.md` | 项目所有权、验证、轻文档、中文交付 |
| Collaboration core | `docs/ai/` | Profile、Feature/Slice、DoD、handoff、信任分级 |
| Project skills | `.agents/skills/` | StreamRoar 专用（Infrastructure API、pre-commit-review） |
| Workflow skills | `.opencode/skills/` | 可移植流程技能（bootstrap / design / DoD / handoff…） |
| Commit lint | `.githooks/commit-msg` | Conventional Commits 标题校验（Unity 可自动安装） |

## Supported adapters (keep in sync)

| Agent | Adapter file(s) | Status |
|-------|-----------------|--------|
| Any / opencode | `AGENTS.md` + `opencode.json` + `.opencode/` | ✅ |
| Cursor | `.cursor/rules/` | ✅ |
| Claude Code | `CLAUDE.md` | ✅ |
| GitHub Copilot | `.github/copilot-instructions.md` | ✅ |
| Cline | `.clinerules` | ✅ |
| Windsurf | `.windsurfrules` | ✅ |

所有适配器应指向同一套真相：`WORKFLOW_PROFILE` → `PROJECT_CONTEXT` → `ACTIVE_WORK` / registry / debt / progress，并遵守 commit gate。

## Adaptation room (给新 Agent / 新伙伴)

新增一种 Agent 时，按此清单扩展，**不要**把规则只写进单一适配器：

1. **硬约束**（commit gate、信任源、Draft≠大改、L3 安全门禁）先落在 `docs/ai/templates/DOC_GOVERNANCE.md` 与 `BOOTSTRAP_DIGEST.md`
2. 新增适配器文件（或目录），用该 Agent 能读取的格式**引用**上述文档，而不是复制长篇细则
3. 在本表增加一行；在 `opencode.json` 的 `instructions`（若适用）或等价配置中挂上核心路径
4. 项目专用行为继续放 `.agents/skills/`；通用流程放 `.opencode/skills/`（或该 Agent 的 skills 目录，并在此注明映射）
5. 改流程时同步：Cursor `.mdc`、CLAUDE/Cline/Windsurf/Copilot、以及本文件

可选未来扩展位（尚未落地，预留命名）：

- `.cursor/hooks/` — Cursor 钩子（若启用）
- `.github/workflows/` — CI（与 Agent 协作文档分离）
- `.agents/skills/<new-skill>/` — 玩法/网络等项目技能
- `docs/ai/<DOMAIN>/` — 按 Domain 放置设计长文（见 `PROJECT_CONTEXT` §7）

## Dual-track reminder

- 改架构说明 → `docs/`
- 改 Feature 状态 / 进度 / 债务 → `docs/ai/`
- 两者冲突时：代码 + 验证结果优先；再修正失真文档
