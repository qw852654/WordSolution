# CONTRIBUTING.md

## 当前架构依据

当前主线已经切换到 CMS V2。

第一版前端与第一版后端都视为历史实现，不再作为当前开发依据。后续新前端只能对接 V2 后端。

本仓库当前主线以 `.codex/内容管理系统详细架构.md` 和 `.codex/内容管理系统升级路线.md` 为准。

历史 `TagRunner`、`Core/Data/Index/Tools` 分层、`TagRunner.Tests`、`src/TagRunner.*` 迁移脚本等描述不再作为当前 WordSolution 的开发依据。除非任务明确要求迁移历史代码，不要新增 `TagRunner.*` 项目、命名空间或目录。

## 主线项目

当前主线项目是：

- `src-v2/WordSolution.CmsV2.Domain`
- `src-v2/WordSolution.CmsV2.Application`
- `src-v2/WordSolution.CmsV2.Infrastructure`
- `src-v2/WordSolution.CmsV2.Api`
- `src-v2/WordSolution.CmsV2.Tests`
- 经确认后新增的 V2 前端项目目录

保留但不作为新内容管理系统核心架构组成部分：

- `题库核心`
- `题库应用`
- `题库基础设施`
- `题库本地服务`
- `题库本地服务/wwwroot`
- `question-bank-office-addin`
- `VSTO`
- `Word本地文件操作核心库`
- `Core.QuestionBank`
- `tools/旧题库迁移工具`

这些保留项目可以按需维护，但新功能默认不要写入这些项目。

## 分层与依赖方向

当前依赖方向保持为：

```text
V2 Frontend
  -> WordSolution.CmsV2.Api
  -> WordSolution.CmsV2.Application
  -> WordSolution.CmsV2.Domain

WordSolution.CmsV2.Api
  -> WordSolution.CmsV2.Application
  -> WordSolution.CmsV2.Infrastructure
  -> WordSolution.CmsV2.Domain

WordSolution.CmsV2.Infrastructure
  -> WordSolution.CmsV2.Domain
```

约束：

- `WordSolution.CmsV2.Domain` 放领域模型和接口契约，不依赖应用层、基础设施层或 API。
- `WordSolution.CmsV2.Application` 承载用例和业务编排，默认只引用 `WordSolution.CmsV2.Domain`。
- `WordSolution.CmsV2.Infrastructure` 实现 `WordSolution.CmsV2.Domain` 中的接口契约，负责 SQLite、文件存储、预览生成、Aspose/OpenXML 封装等实现细节。
- `WordSolution.CmsV2.Api` 是 API 入口和依赖注入组合根，负责把应用用例与基础设施实现注册到容器。
- 不要给 `WordSolution.CmsV2.Application` 新增对 `WordSolution.CmsV2.Infrastructure` 的项目引用。需要基础设施能力时，先在 `WordSolution.CmsV2.Domain` 定义契约，再由 `WordSolution.CmsV2.Infrastructure` 实现，并在 `WordSolution.CmsV2.Api` 注册。

现有 `题库应用/试卷导入模块/试卷解析` 中直接使用 Aspose 的历史实现暂时保留。不要把这个历史例外扩散到新的内容块、讲义或浏览器管理端能力中，也不要为了清理它发起无关重构。

## 阶段边界

开发时优先按 `.codex/内容管理系统升级路线.md` 的阶段推进。每次任务只处理当前阶段范围，避免顺手实现后续阶段能力。

特别注意：

- 当前阶段 1 是“文档清理阶段”，只做 V1 文档与开发要求清理。
- 当前阶段 1 不创建前端工程，不开始页面实现，不进入组件开发。
- 当前阶段 1 不修改 `VSTO`、`Word本地文件操作核心库`、V1 后端业务代码与项目文件。
- 阶段 2 才开始建立独立 V2 前端工程结构。
- V2 浏览器管理端按独立前端项目建设，不再继续扩展 `题库本地服务/wwwroot` 的旧静态页面。
- 新前端只能对接 `/api/cms-v2`，不再对接 `/api/题库实例/...` 等旧接口。
- 后续进入 Word 深度集成时，仍不得反向把新前端建立在 V1 前后端之上。

## 文件存储边界

历史 `题库路径提供器` 已围绕以下结构工作：

```text
{题库根目录}
  question-bank.db
  source\
  html\
```

上述结构只作为历史参考，不再作为 CMS V2 新前端和新后端的实现目标。V2 以 `cms-v2.db` 与 `src-v2` 后端为准。

`E:\Desktop\题库中心` 暂时保持硬编码。除非任务明确要求配置化，否则不要改成配置文件。

## 命名与代码风格

- 对外可见的类、接口、方法和属性名称优先使用现有中文命名风格，例如 `I标签仓储`、`获取标签树用例`、`题目文件存储`。
- 局部变量、私有字段可以使用中文或英文，优先与所在文件已有风格一致。
- 文件名通常与主要类或接口名保持一致。
- 新代码优先延续当前项目的目录、命名空间和用例组织方式，不引入新的架构风格。
- 只在复杂逻辑前添加必要注释，避免解释显而易见的代码。

## 测试

当前不指定独立测试项目。新增或修改业务行为时，应根据现有项目结构补充贴近用例的验证，测试范围随风险扩大：

- 只改文档或纯样式时，可以不加测试。
- 修改用例、仓储、文件存储、预览生成、导入流程时，应优先补测试。
- 修改共享契约、路径结构或依赖注入注册时，应检查相关集成测试或增加覆盖。

## 提交与 PR 规范

- 小步提交，描述清楚变更目的和影响范围。
- 避免把架构调整、功能实现、格式化、生成产物混在一个提交里。
- 重大重构或破坏性改动需要先明确设计决策，不要在普通功能任务中顺手展开。

## AI 协作约束

为减少 Codex 后续误改范围，默认遵守：

- 优先保留现有 V2 结构。
- 优先补充说明和小范围修正。
- 不因远期架构目标提前实现后续阶段能力。
- 不把 V1 保留项目当作新主线。
- 不为追求“更干净”而迁移大量历史代码。
