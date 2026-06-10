# AGENTS.md

本文件是 Codex 在本仓库中的最高优先级项目指令。  
任何任务开始前，必须先阅读本文件，再按任务类型阅读相关文档。

## 1. 项目定位

本仓库当前主线已经切换到 CMS V2。

第一版前端与第一版后端都视为已废弃的历史实现，不再作为当前开发依据。

当前目标是：

- 以 V2 后端为唯一后端主线。
- 以独立重建的 V2 前端为唯一浏览器前端主线。
- 从项目结构开始逐步建设新的前后端工作台。

旧题库系统与旧静态页面只作为历史参考，不再承接新需求。

当前阶段 1 定义为：

- 只做 V1 文档与开发要求清理。
- 不创建前端工程。
- 不进入具体页面实现。
- 不修改 VSTO 相关项目。
- 不修改 V1 后端业务代码与项目文件。

目标形态：

- 以浏览器为主要工作台。
- 以 Word 作为精细编辑器和最终讲义承载格式。
- 以 `.docx` 内容块作为核心内容资产。
- 以小节 Section 作为教学组织单元。
- 以讲义 Handout 作为最终编排与输出单元。
- 本系统是本地个人工作系统，不按多人 SaaS 系统设计。

## 2. 必读文档

每次开始任务前，必须根据任务类型读取以下文档：

### 所有任务都必须读取

1. `CONTRIBUTING.md`
2. `.codex/内容管理系统详细架构.md`
3. `.codex/内容管理系统升级路线.md`

### 涉及 UI / 前端 / 页面 / 交互 / 布局 / 样式的任务必须额外读取

4. `docs/ui/ui-architecture.md`
5. `docs/ui/component-rules.md`
6. `docs/ui/section-page.md`
7. `docs/ui/focus-tree.md`
8. `docs/ui/i18n.md`
9. `docs/ui/codex-workflow.md`

### 涉及 CMS V2 后端 / 前后端接口边界 / V2 数据模型的任务必须额外读取

10. `docs/cms-v2/backend/后端重建阶段计划.md`
11. `docs/cms-v2/backend/后端数据模型开发文档.md`
12. `docs/cms-v2/backend/领域模型结构说明.md`

如果任务说明和上述文档冲突，必须先停止并说明冲突，不要自行选择其中一个继续实现。

## 3. 当前主线项目

新增主线功能默认只能进入以下项目或目录：

- `src-v2/WordSolution.CmsV2.Domain`
- `src-v2/WordSolution.CmsV2.Application`
- `src-v2/WordSolution.CmsV2.Infrastructure`
- `src-v2/WordSolution.CmsV2.Api`
- `src-v2/WordSolution.CmsV2.Tests`
- 经确认后新增的 V2 前端项目目录

当前阶段 1 只允许修改文档，不允许在这些主线项目里直接开始实现。

## 4. 非主线项目限制

以下项目或目录只作为历史代码参考，默认不接收新主线功能：

- `题库核心`
- `题库应用`
- `题库基础设施`
- `题库本地服务`
- `题库本地服务/wwwroot`
- `question-bank-office-addin`
- `Core.QuestionBank`
- `TagRunner`
- `tools/旧题库迁移工具`

其中以下内容在当前阶段 1 完全冻结：

- `VSTO`
- `Word本地文件操作核心库`
- `题库核心`
- `题库应用`
- `题库基础设施`
- `题库本地服务`
- `question-bank-office-addin`

除非任务明确要求迁移旧逻辑，否则不要向这些项目新增主线功能。

不得新增以下内容：

- `TagRunner.*` 项目
- `TagRunner.*` 命名空间
- 新的旧题库架构包装层

## 5. 分层依赖规则

必须保持以下依赖方向：

```text
V2 前端
    -> WordSolution.CmsV2.Api
        -> WordSolution.CmsV2.Application
            -> WordSolution.CmsV2.Domain

WordSolution.CmsV2.Infrastructure -> WordSolution.CmsV2.Domain
WordSolution.CmsV2.Api -> WordSolution.CmsV2.Application + WordSolution.CmsV2.Infrastructure
```

## 6. UI 组件复用规则

涉及 UI / 前端 / 页面 / 交互 / 布局 / 样式的任务，必须优先查看并遵守 `docs/ui/component-rules.md` 中的组件分层、状态归属、API 调用和演示验证规则。

同时必须遵守：

- 新前端不得继续构建在 `题库本地服务/wwwroot` 的 V1 静态页面上。
- 新前端只能对接 CMS V2 后端，不得再新增对 V1 后端接口的依赖。
- 若文档中仍出现 `cms.html`、`sections.html`、`handouts.html`、`references.html`、`/api/题库实例/...` 等旧入口，应视为历史记录，不得作为新实现目标。

后续开发中：

- 同一业务对象不要重复设计多套卡片、列表项或状态标签样式。
- 内容块展示优先按 `docs/ui/component-rules.md` 中的 `ContentBlockCard` 业务组件规则实现。
- 每次新增或抽象 UI 组件时，必须同步写入或更新 `docs/ui/component-rules.md`。
- 如果现有组件无法满足新场景，优先扩展组件文档和共享样式，再在页面中使用。
- 不要在单个页面里临时复制一套近似组件，除非任务明确要求做一次性实验原型。

### 6.1 Theme Token Rule

后续所有业务组件，包括：

- `ContentBlockDisplay`
- `AtomicSectionBlock`
- `CompositeBlock`
- `SectionTree`
- `SectionInspector`
- `Toolbar`
- `StatusTag`

禁止直接写死颜色。

统一通过 Theme Token 引用颜色。

如果当前缺少 Token：

- 先提出需要新增什么 Token。
- 不要直接写颜色值。

## 7. Git 工作流规则

### 7.1 主线保护

`master` 分支视为稳定版本。

禁止直接在 `master` 上进行功能开发。

允许：

- 查看代码
- 编译测试
- 合并已审核的功能分支

禁止：

- 直接修改代码
- 直接提交 Commit

### 7.2 新功能开发

每当开始一个新功能时，必须先创建功能分支。

命名规则：

```text
feature/功能名称
```

例如：

```text
feature/backend-model
feature/section-page
feature/tag-tree
```

### 7.3 AI 开发流程

开始任何开发任务前：

1. 检查当前所在分支。
2. 如果当前为 `master`：
   - 停止开发。
   - 创建新的 `feature` 分支。
   - 切换到该分支。
3. 在 `feature` 分支上完成开发。

### 7.4 提交前检查

每次 Commit 前：

- 查看 Diff
- 确认修改内容符合当前任务
- 避免提交无关文件

### 7.5 合并流程

`feature` 分支开发完成后：

1. Push 到远程仓库。
2. 创建 Pull Request。
3. 审查 Diff。
4. 经确认后合并到 `master`。
5. 删除功能分支。

### 7.6 AI 特殊要求

Codex 在开始修改代码前必须：

- 报告当前分支名称
- 如果当前为 `master`，先询问是否创建功能分支
- 未获得确认前不得直接修改 `master`
