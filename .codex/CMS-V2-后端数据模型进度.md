# CMS V2 后端数据模型进度

> 本文档用于记录 CMS V2 后端数据模型重建进度。后续任何涉及 V2 后端模型、表结构、仓储、用例、控制器、生成链路或引用分析的开发，都必须同步更新本文档。

## 0. 使用规则

### 0.1 维护要求

后续开发过程中，凡是完成或调整以下内容，必须更新本文档：

- 领域对象、枚举、领域规则。
- 仓储契约。
- `DbContext` 映射。
- 初始化建表逻辑。
- SQLite 表名、列名、索引。
- 基础设施仓储实现。
- 应用层用例、请求 DTO、结果 DTO。
- 控制器 API。
- 依赖注入注册。
- 生成链路。
- 引用关系分析。
- 文件存储路径。
- 与 V2 模型有关的设计决策变更。

本文档不是需求文档，而是进度台账。详细设计依据见：

```text
.codex/CMS-V2-后端数据模型开发文档.md
```

### 0.2 状态标记

使用以下状态：

```text
未开始：尚未实现。
进行中：已有部分代码或文档，但未达到可验收状态。
已完成：已实现、已接入对应分层、已能编译或通过必要验证。
延期：本阶段明确不做。
阻塞：因外部决策、数据迁移或依赖问题无法继续。
```

### 0.3 当前总览

更新时间：

```text
2026-06-09
```

当前状态：

```text
V2 后端数据模型开发文档已建立。
V2 后端代码尚未开始重建。
前端页面不纳入本轮后端模型重建范围。
```

已完成：

```text
.codex/CMS-V2-后端数据模型开发文档.md
.codex/CMS-V2-后端数据模型进度.md
```

未开始：

```text
领域对象重建
DbContext 重建
初始化建表逻辑重建
仓储契约与实现重建
应用用例重建
控制器 API 重建
输出生成链路重建
引用关系分析重建
```

## 1. 本轮后端重建边界

### 1.1 本轮要做

```text
CMS V2 领域模型
CMS V2 枚举
CMS V2 仓储契约
CMS V2 SQLite 表结构
CMS V2 DbContext 映射
CMS V2 初始化建表逻辑
CMS V2 基础设施仓储
CMS V2 应用用例
CMS V2 控制器 API
CMS V2 输出生成记录
CMS V2 引用分析
CMS V2 文件存储路径
```

### 1.2 本轮不做

```text
现有前端静态页面适配
新增独立前端项目
Word 加载项深度集成
多人协作
权限系统
云端 SaaS 能力
模板占位符复杂对象
独立媒体资源库
```

### 1.3 必须保留

```text
项目分层结构
题库实例体系
题库路径提供器思路
SQLite 本地数据库
Aspose 授权与 Word 合并能力
本地服务启动结构
标签体系
题目和试卷导入历史能力
source / html 旧题目文件目录语义
```

### 1.4 分层落点

```text
题库核心
  - 领域对象
  - 枚举
  - 领域规则
  - 仓储契约
  - 文件存储和文档处理契约

题库应用
  - 用例
  - 请求 DTO
  - 结果 DTO
  - 生成编排
  - 引用分析

题库基础设施
  - DbContext 映射
  - SQLite 建表实现
  - 仓储实现
  - 文件存储
  - DOCX / HTML / 纯文本处理
  - Word 生成实现

题库本地服务
  - 控制器
  - 路由
  - 依赖注入
  - 题库初始化调用
```

## 2. 领域对象进度

### 2.1 总表

| 对象 | 状态 | 目标文件位置 | 表名 | 说明 |
|---|---|---|---|---|
| `TeachingTopic` | 未开始 | `题库核心/教学主题模块/领域` | `TeachingTopics` | 教学主题树，知识体系定位 |
| `Section` | 未开始 | `题库核心/小节模块/领域` | `Sections` | 小节上帝版本内容池 |
| `SectionItem` | 未开始 | `题库核心/小节模块/领域` | `SectionItems` | 小节内容项，引用内容块或原子小节 |
| `AtomicSection` | 未开始 | `题库核心/小节模块/领域` | `AtomicSections` | Section 内部最小讲解结构 |
| `AtomicSectionItem` | 未开始 | `题库核心/小节模块/领域` | `AtomicSectionItems` | 原子小节内部内容块引用 |
| `SectionVariant` | 未开始 | `题库核心/小节模块/领域` | `SectionVariants` | 从 Section 切出的子版本 |
| `SectionVariantItem` | 未开始 | `题库核心/小节模块/领域` | `SectionVariantItems` | 子版本选择的 SectionItem |
| `ContentBlock` | 未开始 | `题库核心/内容块模块/领域` | `ContentBlocks` | 可复用内容资产 |
| `ContentBlockVersion` | 未开始 | `题库核心/内容块模块/领域` | `ContentBlockVersions` | 内容块正文版本 |
| `ContentBlockRelation` | 未开始 | `题库核心/内容块模块/领域` | `ContentBlockRelations` | 内容块包含内容块 |
| `Handout` | 未开始 | `题库核心/讲义模块/领域` | `Handouts` | 讲义项目入口 |
| `HandoutVersion` | 未开始 | `题库核心/讲义模块/领域` | `HandoutVersions` | 具体讲义版本 |
| `HandoutVersionItem` | 未开始 | `题库核心/讲义模块/领域` | `HandoutVersionItems` | 讲义版本内容项 |
| `OutputTemplate` | 未开始 | `题库核心/输出模块/领域` | `OutputTemplates` | DOCX 输出模板 |
| `OutputForm` | 未开始 | `题库核心/输出模块/领域` | `OutputForms` | 输出形式 |
| `GeneratedFile` | 未开始 | `题库核心/输出模块/领域` | `GeneratedFiles` | 实际生成文件记录 |
| `TeachingNote` | 未开始 | `题库核心/教学备注模块/领域` | `TeachingNotes` | 教学备注 |

### 2.2 TeachingTopic

职责：

```text
表示知识体系中的教学主题位置，回答“这个教学内容属于知识体系中的哪里”。
```

目标字段：

```text
Id
ParentId
Name
Description
SortOrder
Status
UpdatedTime
```

关系：

```text
TeachingTopic 1 - N Section
TeachingTopic 可以有父级 TeachingTopic
TeachingNote 可以挂到 TeachingTopic
```

约束：

```text
ParentId 不能指向自身。
教学主题树必须防止循环父级。
TeachingTopic 不保存题目正文、讲义正文、讲课流程、输出规则和讲义版本。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 未开始 | `TeachingTopicStatus` |
| 仓储契约 | 未开始 | `I教学主题仓储` |
| DbContext | 未开始 | `TeachingTopics` |
| 初始化建表 | 未开始 | 创建表和索引 |
| 仓储实现 | 未开始 | SQLite 实现 |
| 应用用例 | 未开始 | CRUD、树查询、排序 |
| 控制器 | 未开始 | `/教学主题` |

### 2.3 Section

职责：

```text
表示某个 TeachingTopic 下的一套完整内容池，本身是“上帝版本”。
```

目标字段：

```text
Id
TeachingTopicId
Title
Description
Type
Difficulty
Status
SortOrder
UpdatedTime
```

关系：

```text
TeachingTopic 1 - N Section
Section 1 - N SectionItem
Section 1 - N SectionVariant
TeachingNote 可以挂到 Section
```

约束：

```text
Section 不直接保存 Word 正文。
Section 通过 SectionItem 引用 ContentBlock 或 AtomicSection。
Section 不能直接作为 HandoutVersionItem 被引用。
Difficulty 可以保存在 Section，表示上帝版本整体难度。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需替换旧 `小节` 语义 |
| 枚举 | 未开始 | `SectionType`、`SectionStatus`、`DifficultyLevel` |
| 仓储契约 | 未开始 | `I小节仓储` |
| DbContext | 未开始 | `Sections` 新语义 |
| 初始化建表 | 未开始 | 增加 `TeachingTopicId`、`Type`、`Difficulty`、`SortOrder` |
| 仓储实现 | 未开始 | SQLite 实现 |
| 应用用例 | 未开始 | CRUD、按主题查询 |
| 控制器 | 未开始 | `/小节` |

### 2.4 SectionItem

职责：

```text
表示某个 ContentBlock 或 AtomicSection 被放入某个 Section 中的那一次。
```

目标字段：

```text
Id
SectionId
TargetType
TargetId
ReferenceMode
LockedContentBlockVersionId
TitleOverride
ParentItemId
SortOrder
SelectionLayer
TeachingUseOverride
Status
Note
UpdatedTime
```

关系：

```text
Section 1 - N SectionItem
SectionItem N - 1 ContentBlock 或 AtomicSection
SectionItem 可以有父级 SectionItem
SectionVariantItem N - 1 SectionItem
TeachingNote 可以挂到 SectionItem
```

约束：

```text
SectionItem 不保存 Difficulty。
TargetType 只允许 ContentBlock、AtomicSection。
SectionItem 不能直接指向 ContentBlockVersion。
锁定版本通过 LockedContentBlockVersionId 表达。
TargetType = AtomicSection 时 ReferenceMode 和 LockedContentBlockVersionId 应为空或忽略。
ParentItemId 必须属于同一个 Section。
SectionItem 树必须防止循环。
允许同一个 ContentBlock 在同一 Section 中出现多次。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需替换旧 `小节项` 只能引用内容块的语义 |
| 枚举 | 未开始 | `SectionItemTargetType`、`ReferenceMode`、`SectionItemStatus` |
| 仓储契约 | 未开始 | 小节仓储或独立项目仓储 |
| DbContext | 未开始 | `SectionItems` 新字段 |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | 查询、排序、父子层级 |
| 应用用例 | 未开始 | 添加、更新、移除、排序、层级调整 |
| 控制器 | 未开始 | `/小节/{id}/项目` |

### 2.5 AtomicSection

职责：

```text
表示 Section 内部的最小讲解结构，介于 Section 与 ContentBlock 之间。
```

目标字段：

```text
Id
Title
Description
Type
Status
UpdatedTime
```

关系：

```text
SectionItem 可以引用 AtomicSection
AtomicSection 1 - N AtomicSectionItem
AtomicSectionItem N - 1 ContentBlock
TeachingNote 可以挂到 AtomicSection
```

约束：

```text
AtomicSection 不直接保存 DOCX 正文。
AtomicSection 内部只能放置 ContentBlock。
AtomicSection 内部不能放置 AtomicSection。
AtomicSection 不能被 HandoutVersionItem 直接引用。
AtomicSection 不保存 Difficulty。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 未开始 | `AtomicSectionType`、`AtomicSectionStatus` |
| 仓储契约 | 未开始 | `I原子小节仓储` |
| DbContext | 未开始 | `AtomicSections` |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | CRUD |
| 应用用例 | 未开始 | CRUD |
| 控制器 | 未开始 | `/原子小节` |

### 2.6 AtomicSectionItem

职责：

```text
表示某个 ContentBlock 被放入某个 AtomicSection 中。
```

目标字段：

```text
Id
AtomicSectionId
ContentBlockId
ReferenceMode
LockedContentBlockVersionId
TitleOverride
SortOrder
Note
UpdatedTime
```

关系：

```text
AtomicSection 1 - N AtomicSectionItem
AtomicSectionItem N - 1 ContentBlock
```

约束：

```text
AtomicSectionItem 只能引用 ContentBlock。
AtomicSectionItem 不能引用 AtomicSection。
AtomicSectionItem 不需要 ParentItemId。
ReferenceMode = LockedVersion 时必须提供 LockedContentBlockVersionId。
LockedContentBlockVersionId 必须属于 ContentBlockId 对应内容块。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 未开始 | 复用 `ReferenceMode` |
| 仓储契约 | 未开始 | 原子小节仓储或独立项目仓储 |
| DbContext | 未开始 | `AtomicSectionItems` |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | 查询、排序 |
| 应用用例 | 未开始 | 添加、移除、排序 |
| 控制器 | 未开始 | `/原子小节/{id}/项目` |

### 2.7 SectionVariant

职责：

```text
表示从 Section 上帝版本中切出的具体可用版本。
```

目标字段：

```text
Id
SectionId
Title
Description
Type
Difficulty
Status
SortOrder
UpdatedTime
```

关系：

```text
Section 1 - N SectionVariant
SectionVariant 1 - N SectionVariantItem
SectionVariantItem N - 1 SectionItem
HandoutVersionItem 可以引用 SectionVariant
TeachingNote 可以挂到 SectionVariant
```

约束：

```text
SectionVariant 不复制正文。
SectionVariant 不直接引用 ContentBlock 或 AtomicSection。
SectionVariant 通过 SectionVariantItem 选择 SectionItem。
Difficulty 可以保存在 SectionVariant。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 未开始 | `SectionVariantType`、`SectionVariantStatus`、`DifficultyLevel` |
| 仓储契约 | 未开始 | `I小节子版本仓储` |
| DbContext | 未开始 | `SectionVariants` |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | CRUD、按 Section 查询 |
| 应用用例 | 未开始 | CRUD、从 Section 创建默认子版本 |
| 控制器 | 未开始 | `/小节子版本` |

### 2.8 SectionVariantItem

职责：

```text
表示某个 SectionVariant 选择了哪个 SectionItem。
```

目标字段：

```text
Id
SectionVariantId
SectionItemId
SortOrder
Note
UpdatedTime
```

关系：

```text
SectionVariant 1 - N SectionVariantItem
SectionVariantItem N - 1 SectionItem
```

约束：

```text
第一版只保存被选中的 SectionItem。
不保存 IsIncluded。
未出现在 SectionVariantItem 中表示未被选入子版本。
SectionItemId 必须属于 SectionVariant.SectionId 对应的 Section。
允许子版本排序与上帝版本排序不一致。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 延期 | 无独立枚举 |
| 仓储契约 | 未开始 | 小节子版本仓储或独立项目仓储 |
| DbContext | 未开始 | `SectionVariantItems` |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | 查询、排序 |
| 应用用例 | 未开始 | 添加、移除、排序 |
| 控制器 | 未开始 | `/小节子版本/{id}/项目` |

### 2.9 ContentBlock

职责：

```text
表示一个可复用的教学内容资产。
```

目标字段：

```text
Id
Title
Summary
BlockType
Difficulty
QuestionType
Status
CurrentVersionId
UpdatedTime
```

关系：

```text
ContentBlock 1 - N ContentBlockVersion
ContentBlock 可以通过 ContentBlockRelation 包含其他 ContentBlock
ContentBlock 可以被多个 SectionItem 引用
ContentBlock 可以被多个 AtomicSectionItem 引用
ContentBlock 可以被多个 HandoutVersionItem 直接引用
TeachingNote 可以挂到 ContentBlock
```

约束：

```text
QuestionType 只对 BlockType = Question 有意义。
QuestionType 不合并进 BlockType。
ContentBlock 不负责在小节、原子小节或讲义中的排序。
ContentBlock 不负责进入哪个 SectionVariant。
ContentBlock 不负责输出成学生版还是教师版。
教学备注不归入 ContentBlock。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需按 V2 语义替换旧 `内容块` |
| 枚举 | 未开始 | `ContentBlockType`、`QuestionType`、`ContentBlockStatus`、`DifficultyLevel` |
| 仓储契约 | 未开始 | `I内容块仓储` |
| DbContext | 未开始 | `ContentBlocks` V2 字段 |
| 初始化建表 | 未开始 | 建表或迁移旧表 |
| 仓储实现 | 未开始 | CRUD、版本查询、引用查询 |
| 应用用例 | 未开始 | CRUD、版本创建、详情查询 |
| 控制器 | 未开始 | `/内容块` |

### 2.10 ContentBlockVersion

职责：

```text
保存某个 ContentBlock 的某一版正文。
```

目标字段：

```text
Id
ContentBlockId
VersionNumber
DocxPath
HtmlPreviewPath
PlainText
IsCurrent
UpdatedTime
```

关系：

```text
ContentBlock 1 - N ContentBlockVersion
SectionItem / AtomicSectionItem / ContentBlockRelation 可以锁定某个 ContentBlockVersion
GeneratedFile.VersionManifestJson 需要记录实际使用的 ContentBlockVersion
```

约束：

```text
不保存 Difficulty。
不保存 BlockType。
不保存 QuestionType。
同一 ContentBlockId 下 VersionNumber 唯一。
同一 ContentBlockId 下最多一个 IsCurrent = true。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需按 V2 语义替换旧 `内容块版本` |
| 枚举 | 延期 | 无独立枚举 |
| 仓储契约 | 未开始 | 内容块仓储承担 |
| DbContext | 未开始 | `ContentBlockVersions` |
| 初始化建表 | 未开始 | `UpdatedTime` 替代旧 `CreatedTime` |
| 仓储实现 | 未开始 | 当前版本、版本列表、创建版本 |
| 应用用例 | 未开始 | 创建正文版本、查询版本 |
| 控制器 | 未开始 | `/内容块/{id}/版本` |

### 2.11 ContentBlockRelation

职责：

```text
表示 ContentBlock 直接包含另一个 ContentBlock 的关系。
```

目标字段：

```text
Id
ParentBlockId
ChildBlockId
ReferenceMode
LockedContentBlockVersionId
TitleOverride
SortOrder
Note
UpdatedTime
```

关系：

```text
Parent ContentBlock 1 - N ContentBlockRelation
ContentBlockRelation N - 1 Child ContentBlock
```

约束：

```text
不保存教学用途。
教学用途优先由父 ContentBlock 决定。
ParentBlockId 不能等于 ChildBlockId。
必须防止循环包含。
ReferenceMode = LockedVersion 时必须提供 LockedContentBlockVersionId。
LockedContentBlockVersionId 必须属于 ChildBlockId 对应内容块。
默认最大展开深度建议为 10 层。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需替换旧 `内容块子项` 语义 |
| 枚举 | 未开始 | 复用 `ReferenceMode` |
| 仓储契约 | 未开始 | 内容块仓储承担 |
| DbContext | 未开始 | `ContentBlockRelations` |
| 初始化建表 | 未开始 | 新表或替换 `ContentBlockChildren` |
| 仓储实现 | 未开始 | 查询父子、排序、循环检测支持 |
| 应用用例 | 未开始 | 添加、移除、排序、结构树 |
| 控制器 | 未开始 | `/内容块/{id}/包含关系` |

### 2.12 Handout

职责：

```text
表示一组相关讲义的总入口，不是某一份具体可打印讲义。
```

目标字段：

```text
Id
Title
Description
Status
UpdatedTime
```

关系：

```text
Handout 1 - N HandoutVersion
```

约束：

```text
Handout 不直接保存讲义内容项。
Handout 不直接生成文件。
具体讲义内容组合由 HandoutVersion 表达。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需替换旧 `讲义` 语义 |
| 枚举 | 未开始 | `HandoutStatus` |
| 仓储契约 | 未开始 | `I讲义仓储` |
| DbContext | 未开始 | `Handouts` V2 语义 |
| 初始化建表 | 未开始 | 删除旧 `CreatedTime` 语义 |
| 仓储实现 | 未开始 | CRUD、版本查询 |
| 应用用例 | 未开始 | CRUD |
| 控制器 | 未开始 | `/讲义` |

### 2.13 HandoutVersion

职责：

```text
表示某一套具体讲义内容组合。
```

目标字段：

```text
Id
HandoutId
Title
Description
Type
Status
SortOrder
UpdatedTime
```

关系：

```text
Handout 1 - N HandoutVersion
HandoutVersion 1 - N HandoutVersionItem
HandoutVersion 1 - N OutputForm
TeachingNote 可以挂到 HandoutVersion
```

约束：

```text
HandoutVersion 不保存 Difficulty。
基础版、提高版、一轮复习版属于 HandoutVersion，不属于 OutputForm。
HandoutVersion 通过 HandoutVersionItem 引用 SectionVariant 或 ContentBlock。
HandoutVersion 通过 OutputForm 定义输出形式。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 未开始 | `HandoutVersionType`、`HandoutVersionStatus` |
| 仓储契约 | 未开始 | 讲义仓储承担 |
| DbContext | 未开始 | `HandoutVersions` |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | CRUD、按 Handout 查询 |
| 应用用例 | 未开始 | CRUD |
| 控制器 | 未开始 | `/讲义/{id}/版本` 或 `/讲义版本` |

### 2.14 HandoutVersionItem

职责：

```text
表示某个 HandoutVersion 引用了哪些内容。
```

目标字段：

```text
Id
HandoutVersionId
TargetType
TargetId
SortOrder
TitleOverride
Note
UpdatedTime
```

关系：

```text
HandoutVersion 1 - N HandoutVersionItem
HandoutVersionItem N - 1 SectionVariant 或 ContentBlock
```

约束：

```text
TargetType 只允许 SectionVariant、ContentBlock。
不允许直接引用 AtomicSection。
不允许直接引用 Section。
主要引用 SectionVariant，直接引用 ContentBlock 只用于补充内容。
HandoutVersionItem 不保存 Difficulty。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需替换旧 `讲义项` 语义 |
| 枚举 | 未开始 | `HandoutVersionItemTargetType` |
| 仓储契约 | 未开始 | 讲义仓储承担 |
| DbContext | 未开始 | `HandoutVersionItems` |
| 初始化建表 | 未开始 | 替换旧 `HandoutItems` |
| 仓储实现 | 未开始 | 查询、排序 |
| 应用用例 | 未开始 | 添加、移除、排序 |
| 控制器 | 未开始 | `/讲义版本/{id}/项目` |

### 2.15 OutputTemplate

职责：

```text
表示一个 DOCX 输出模板，承载复杂格式规范。
```

目标字段：

```text
Id
Title
Description
TemplateDocxPath
Status
UpdatedTime
```

关系：

```text
OutputForm N - 1 OutputTemplate
```

约束：

```text
复杂排版保留在 DOCX 模板中。
第一版不做 TemplateSlot、Placeholder、Bookmark、ContentControl。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 未开始 | `OutputTemplateStatus` |
| 仓储契约 | 未开始 | `I输出仓储` |
| DbContext | 未开始 | `OutputTemplates` |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | CRUD |
| 应用用例 | 未开始 | CRUD、模板文件保存 |
| 控制器 | 未开始 | `/输出模板` |

### 2.16 OutputForm

职责：

```text
表示同一个 HandoutVersion 的一种输出形式。
```

目标字段：

```text
Id
HandoutVersionId
OutputTemplateId
Title
Audience
OutputFormat
VisibilityMode
Status
SortOrder
UpdatedTime
```

关系：

```text
HandoutVersion 1 - N OutputForm
OutputForm N - 1 OutputTemplate
OutputForm 1 - N GeneratedFile
```

约束：

```text
OutputForm 不负责讲义内容选择。
OutputForm 不保存 Difficulty。
学生版、教师版、A3版、PDF、Word 属于 OutputForm。
生成文件必须挂到 OutputForm。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 未开始 | `OutputAudience`、`OutputFormat`、`VisibilityMode`、`OutputFormStatus` |
| 仓储契约 | 未开始 | 输出仓储承担 |
| DbContext | 未开始 | `OutputForms` |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | CRUD、按讲义版本查询 |
| 应用用例 | 未开始 | CRUD、生成入口 |
| 控制器 | 未开始 | `/输出形式` |

### 2.17 GeneratedFile

职责：

```text
记录某一次实际生成出来的文件。
```

目标字段：

```text
Id
OutputFormId
FilePath
VersionManifestJson
GeneratedTime
```

关系：

```text
OutputForm 1 - N GeneratedFile
```

约束：

```text
GeneratedFile 只记录实际生成的文件。
GeneratedTime 是允许保留的时间字段。
VersionManifestJson 必须记录可追溯的内容版本。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需替换旧 `讲义生成记录` 语义 |
| 枚举 | 延期 | 第一版无独立枚举 |
| 仓储契约 | 未开始 | 输出仓储承担 |
| DbContext | 未开始 | `GeneratedFiles` |
| 初始化建表 | 未开始 | 替换旧 `HandoutGenerations` |
| 仓储实现 | 未开始 | 新增、查询、下载路径 |
| 应用用例 | 未开始 | 从 OutputForm 生成 |
| 控制器 | 未开始 | `/生成文件`、`/输出形式/{id}/生成` |

### 2.18 TeachingNote

职责：

```text
单独保存教学经验、讲解反思、修改建议、未来提醒。
```

目标字段：

```text
Id
TargetType
TargetId
NoteType
Title
Content
Status
UpdatedTime
```

关系：

```text
TeachingNote 可挂到 TeachingTopic
TeachingNote 可挂到 Section
TeachingNote 可挂到 SectionVariant
TeachingNote 可挂到 SectionItem
TeachingNote 可挂到 AtomicSection
TeachingNote 可挂到 ContentBlock
TeachingNote 可挂到 HandoutVersion
```

约束：

```text
TeachingNote 不归入 ContentBlock。
TeachingNote 不归入 ContentBlockVersion。
TeachingNote 不参与讲义正文输出，除非未来显式设计。
```

分层进度：

| 分层 | 状态 | 说明 |
|---|---|---|
| 领域对象 | 未开始 | 需新增 |
| 枚举 | 未开始 | `TeachingNoteTargetType`、`TeachingNoteType`、`TeachingNoteStatus` |
| 仓储契约 | 未开始 | `I教学备注仓储` |
| DbContext | 未开始 | `TeachingNotes` |
| 初始化建表 | 未开始 | 建表和索引 |
| 仓储实现 | 未开始 | CRUD、按目标查询 |
| 应用用例 | 未开始 | CRUD、按目标查询 |
| 控制器 | 未开始 | `/教学备注` |

## 3. 表结构命名进度

### 3.1 V2 CMS 表

| 表名 | 状态 | 对象 | 当前说明 |
|---|---|---|---|
| `TeachingTopics` | 未开始 | `TeachingTopic` | 新表 |
| `Sections` | 未开始 | `Section` | 旧表存在但语义需重建 |
| `SectionItems` | 未开始 | `SectionItem` | 旧表存在但字段和语义需重建 |
| `AtomicSections` | 未开始 | `AtomicSection` | 新表 |
| `AtomicSectionItems` | 未开始 | `AtomicSectionItem` | 新表 |
| `SectionVariants` | 未开始 | `SectionVariant` | 新表 |
| `SectionVariantItems` | 未开始 | `SectionVariantItem` | 新表 |
| `ContentBlocks` | 未开始 | `ContentBlock` | 旧表存在但字段和语义需调整 |
| `ContentBlockVersions` | 未开始 | `ContentBlockVersion` | 旧表存在但时间字段需调整 |
| `ContentBlockRelations` | 未开始 | `ContentBlockRelation` | 替代旧 `ContentBlockChildren` |
| `Handouts` | 未开始 | `Handout` | 旧表存在但语义需调整 |
| `HandoutVersions` | 未开始 | `HandoutVersion` | 新表 |
| `HandoutVersionItems` | 未开始 | `HandoutVersionItem` | 替代旧 `HandoutItems` |
| `OutputTemplates` | 未开始 | `OutputTemplate` | 新表 |
| `OutputForms` | 未开始 | `OutputForm` | 新表 |
| `GeneratedFiles` | 未开始 | `GeneratedFile` | 替代旧 `HandoutGenerations` |
| `TeachingNotes` | 未开始 | `TeachingNote` | 新表 |

### 3.2 旧 CMS 表处理

| 旧表 | 状态 | V2 处理方式 |
|---|---|---|
| `ContentBlockChildren` | 未开始 | 替换为 `ContentBlockRelations` |
| `HandoutItems` | 未开始 | 替换为 `HandoutVersionItems` |
| `HandoutGenerations` | 未开始 | 替换为 `GeneratedFiles` |

### 3.3 非 CMS 历史表

以下表默认保留，不因 V2 CMS 重建删除：

```text
Questions
QuestionTypes
Tags
TagKinds
QuestionTags
Papers
PaperSourceFiles
PaperQuestions
KnowledgeMappings
```

## 4. DbContext 与初始化进度

### 4.1 DbContext

目标文件：

```text
题库基础设施/数据访问/题库DbContext.cs
```

当前状态：

```text
未开始
```

待办：

- 增加 V2 `DbSet`。
- 替换旧 CMS 映射语义。
- 保留非 CMS 历史表映射。
- 明确枚举存储方式。
- 明确 DateTime 存储方式。

### 4.2 初始化建表逻辑

目标文件：

```text
题库基础设施/初始化/题库实例初始化器.cs
```

当前状态：

```text
未开始
```

待办：

- 新增或替换 CMS V2 建表方法。
- 建立索引。
- 保留旧题目、标签、试卷导入相关表。
- 不破坏 `source` / `html` 旧题目目录语义。

### 4.3 索引进度

| 索引 | 状态 |
|---|---|
| `TeachingTopics(ParentId, SortOrder)` | 未开始 |
| `TeachingTopics(Status)` | 未开始 |
| `Sections(TeachingTopicId, SortOrder)` | 未开始 |
| `Sections(Status)` | 未开始 |
| `SectionItems(SectionId, ParentItemId, SortOrder)` | 未开始 |
| `SectionItems(TargetType, TargetId)` | 未开始 |
| `AtomicSectionItems(AtomicSectionId, SortOrder)` | 未开始 |
| `AtomicSectionItems(ContentBlockId)` | 未开始 |
| `SectionVariants(SectionId, SortOrder)` | 未开始 |
| `SectionVariants(Status)` | 未开始 |
| `SectionVariantItems(SectionVariantId, SortOrder)` | 未开始 |
| `SectionVariantItems(SectionItemId)` | 未开始 |
| `ContentBlocks(BlockType, Status)` | 未开始 |
| `ContentBlocks(CurrentVersionId)` | 未开始 |
| `ContentBlocks(Difficulty)` | 未开始 |
| `ContentBlockVersions(ContentBlockId, VersionNumber) UNIQUE` | 未开始 |
| `ContentBlockVersions(ContentBlockId, IsCurrent)` | 未开始 |
| `ContentBlockRelations(ParentBlockId, SortOrder)` | 未开始 |
| `ContentBlockRelations(ChildBlockId)` | 未开始 |
| `ContentBlockRelations(ParentBlockId, ChildBlockId)` | 未开始 |
| `HandoutVersions(HandoutId, SortOrder)` | 未开始 |
| `HandoutVersions(Status)` | 未开始 |
| `HandoutVersionItems(HandoutVersionId, SortOrder)` | 未开始 |
| `HandoutVersionItems(TargetType, TargetId)` | 未开始 |
| `OutputForms(HandoutVersionId, SortOrder)` | 未开始 |
| `OutputForms(OutputTemplateId)` | 未开始 |
| `GeneratedFiles(OutputFormId, GeneratedTime)` | 未开始 |
| `TeachingNotes(TargetType, TargetId)` | 未开始 |
| `TeachingNotes(Status)` | 未开始 |

## 5. 仓储进度

| 仓储契约 | 状态 | 核心职责 |
|---|---|---|
| `I教学主题仓储` | 未开始 | 教学主题 CRUD、树查询、父级调整 |
| `I小节仓储` | 未开始 | Section、SectionItem 查询与保存 |
| `I原子小节仓储` | 未开始 | AtomicSection、AtomicSectionItem 查询与保存 |
| `I小节子版本仓储` | 未开始 | SectionVariant、SectionVariantItem 查询与保存 |
| `I内容块仓储` | 未开始 | ContentBlock、Version、Relation 查询与保存 |
| `I讲义仓储` | 未开始 | Handout、HandoutVersion、HandoutVersionItem 查询与保存 |
| `I输出仓储` | 未开始 | OutputTemplate、OutputForm、GeneratedFile 查询与保存 |
| `I教学备注仓储` | 未开始 | TeachingNote CRUD、按目标查询 |

分层落点：

```text
契约：题库核心
实现：题库基础设施
注册：题库本地服务
```

## 6. 应用用例进度

### 6.1 最小链路

目标最小链路：

```text
TeachingTopic
-> Section
-> SectionItem / AtomicSection
-> SectionVariant
-> Handout
-> HandoutVersion
-> HandoutVersionItem
-> OutputTemplate
-> OutputForm
-> GeneratedFile
```

当前状态：

```text
未开始
```

### 6.2 用例清单

| 用例组 | 状态 | 说明 |
|---|---|---|
| 教学主题用例 | 未开始 | 新建、更新、树查询、归档 |
| 小节用例 | 未开始 | 新建、更新、按主题查询 |
| 小节项用例 | 未开始 | 添加、移除、排序、层级调整 |
| 原子小节用例 | 未开始 | 新建、更新、查询 |
| 原子小节项用例 | 未开始 | 添加、移除、排序 |
| 小节子版本用例 | 未开始 | 新建、更新、按 Section 查询 |
| 小节子版本项用例 | 未开始 | 选择 SectionItem、移除、排序 |
| 内容块用例 | 未开始 | 新建、更新、查询 |
| 内容块版本用例 | 未开始 | 创建版本、查询版本、设置当前版本 |
| 内容块包含关系用例 | 未开始 | 添加、移除、排序、循环检测、深度检测 |
| 讲义用例 | 未开始 | 新建、更新、查询 |
| 讲义版本用例 | 未开始 | 新建、更新、按讲义查询 |
| 讲义版本项用例 | 未开始 | 添加 SectionVariant / ContentBlock、移除、排序 |
| 输出模板用例 | 未开始 | 新建、更新、查询、模板文件管理 |
| 输出形式用例 | 未开始 | 新建、更新、按讲义版本查询 |
| 生成文件用例 | 未开始 | 从 OutputForm 生成文件、查询生成记录、下载 |
| 教学备注用例 | 未开始 | 新建、更新、按目标查询 |
| 引用关系用例 | 未开始 | 影响范围、旧版本引用审查 |

## 7. 控制器 API 进度

| API 分组 | 状态 | 路由建议 |
|---|---|---|
| 教学主题 | 未开始 | `/api/题库实例/{题库键}/教学主题` |
| 小节 | 未开始 | `/api/题库实例/{题库键}/小节` |
| 小节项目 | 未开始 | `/api/题库实例/{题库键}/小节/{id}/项目` |
| 原子小节 | 未开始 | `/api/题库实例/{题库键}/原子小节` |
| 原子小节项目 | 未开始 | `/api/题库实例/{题库键}/原子小节/{id}/项目` |
| 小节子版本 | 未开始 | `/api/题库实例/{题库键}/小节子版本` |
| 小节子版本项目 | 未开始 | `/api/题库实例/{题库键}/小节子版本/{id}/项目` |
| 内容块 | 未开始 | `/api/题库实例/{题库键}/内容块` |
| 内容块版本 | 未开始 | `/api/题库实例/{题库键}/内容块/{id}/版本` |
| 内容块包含关系 | 未开始 | `/api/题库实例/{题库键}/内容块/{id}/包含关系` |
| 讲义 | 未开始 | `/api/题库实例/{题库键}/讲义` |
| 讲义版本 | 未开始 | `/api/题库实例/{题库键}/讲义版本` |
| 讲义版本项目 | 未开始 | `/api/题库实例/{题库键}/讲义版本/{id}/项目` |
| 输出模板 | 未开始 | `/api/题库实例/{题库键}/输出模板` |
| 输出形式 | 未开始 | `/api/题库实例/{题库键}/输出形式` |
| 输出生成 | 未开始 | `/api/题库实例/{题库键}/输出形式/{id}/生成` |
| 生成文件 | 未开始 | `/api/题库实例/{题库键}/生成文件` |
| 教学备注 | 未开始 | `/api/题库实例/{题库键}/教学备注` |
| 引用关系 | 未开始 | `/api/题库实例/{题库键}/引用关系` |

## 8. 生成链路进度

目标生成链路：

```text
OutputForm
  -> HandoutVersion
      -> HandoutVersionItem
          -> SectionVariant
              -> SectionVariantItem
                  -> SectionItem
                      -> ContentBlock
                      -> AtomicSection
                          -> AtomicSectionItem
                              -> ContentBlock
          -> ContentBlock
              -> ContentBlockRelation
                  -> ContentBlock
```

当前状态：

```text
未开始
```

待办：

- 从 `OutputForm` 解析 `HandoutVersion`。
- 展开 `HandoutVersionItem`。
- 展开 `SectionVariant`。
- 展开 `AtomicSection`。
- 递归展开 `ContentBlockRelation`。
- 解析 `ReferenceMode`。
- 检查锁定版本缺失。
- 检查循环包含。
- 检查最大深度。
- 合并 DOCX。
- 保存 `GeneratedFile`。
- 保存 `VersionManifestJson`。

## 9. 引用关系分析进度

目标：

```text
分析 ContentBlock 被哪些 ContentBlockRelation、SectionItem、AtomicSectionItem、SectionVariant、HandoutVersionItem、OutputForm/GeneratedFile 间接影响。
分析锁定旧版本引用。
```

当前状态：

```text
未开始
```

待办：

- 内容块直接父级关系分析。
- 内容块递归父级关系分析。
- 小节上帝版本引用分析。
- 原子小节引用分析。
- 小节子版本引用分析。
- 讲义版本引用分析。
- 生成文件版本清单分析。
- 旧版本引用审查。

## 10. 文件存储进度

### 10.1 旧目录保留

必须保留：

```text
{题库根目录}
  question-bank.db
  source\
  html\
```

状态：

```text
已存在，V2 重建不得破坏。
```

### 10.2 V2 新目录

| 目录 | 状态 | 用途 |
|---|---|---|
| `content-blocks/source/{ContentBlockId}` | 未开始 | 内容块 DOCX 正文 |
| `content-blocks/html/{ContentBlockId}` | 未开始 | 内容块 HTML 预览 |
| `content-blocks/text/{ContentBlockId}` | 未开始 | 内容块纯文本索引 |
| `output-templates/{OutputTemplateId}` | 未开始 | 输出模板 DOCX |
| `generated-files/{OutputFormId}` | 未开始 | 输出生成文件 |
| `temp/edit-sessions` | 已存在 | 当前已有编辑会话目录思路，需确认 V2 复用方式 |

## 11. 依赖注入进度

目标文件：

```text
题库本地服务/依赖注入/题库服务注册扩展.cs
```

当前状态：

```text
未开始
```

待办：

- 注册 V2 仓储实现。
- 注册 V2 应用用例。
- 注册 V2 生成服务。
- 注册 V2 引用分析服务。
- 清理或停用旧 CMS 用例注册，避免 API 误用旧模型。

## 12. 编译与验证记录

| 日期 | 验证项 | 结果 | 说明 |
|---|---|---|---|
| 2026-06-09 | 文档创建 | 已完成 | 新增本文档 |

当前尚未执行代码编译，因为本次只新增文档。

## 13. 决策记录

### 13.1 2026-06-09：前端不纳入本轮

决策：

```text
本轮只重建 V2 后端数据模型和后端能力。
现有前端页面不做适配。
后续前端会基于 V2 API 推倒重新开发。
```

影响：

```text
后端 API 不需要兼容 cms.html / sections.html / handouts.html / references.html。
旧前端可能在 V2 后端重建过程中失效，这是可接受结果。
```

### 13.2 2026-06-09：采用 SectionVariant / HandoutVersion / OutputForm

决策：

```text
V2 后端模型采用外部设计文档中的 SectionVariant、HandoutVersion、OutputForm。
不再沿用旧文档中的 SectionPlan / HandoutPlan 作为本轮后端模型名称。
```

影响：

```text
Section 语义调整为上帝版本内容池。
Handout 语义调整为讲义项目入口。
具体小节版本由 SectionVariant 表达。
具体讲义内容组合由 HandoutVersion 表达。
学生版、教师版、A3、Word、PDF 等由 OutputForm 表达。
```

## 14. 后续开发更新模板

每完成一个对象或分层，请按以下格式追加记录：

```text
### YYYY-MM-DD：完成 XXX

变更范围：
- ...

完成内容：
- ...

涉及文件：
- ...

验证：
- ...

后续待办：
- ...
```
