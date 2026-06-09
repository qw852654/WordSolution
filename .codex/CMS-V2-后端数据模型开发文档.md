# CMS V2 后端数据模型开发文档

> 本文档用于指导当前仓库重建 CMS / 内容管理系统后端数据模型。
> 本次重建只覆盖后端模型、数据库结构、仓储、应用用例、控制器 API 和生成记录能力；前端页面不在本轮范围内，后续可以推倒重新开发。

## 0. 文档定位

### 0.1 来源

本文档根据外部设计文档 `E:/下载/CMS-V2-后端数据模型设计.md` 整理，并结合当前仓库分层约束形成开发规格。

外部设计文档的核心要求应尽量完整保留：

- 不把 Word / DOCX 当作结构源头。
- 数据库负责结构、关系、版本、选择和生成记录。
- 建立 `TeachingTopic -> Section -> SectionVariant -> HandoutVersion -> OutputForm -> GeneratedFile` 的后端模型链路。
- `Section` 是上帝版本内容池，不是某个具体讲法版本。
- `SectionVariant` 是从 `Section` 中切出的具体小节子版本。
- `Handout` 是讲义项目入口，不是一份具体可输出讲义。
- `HandoutVersion` 是一份具体讲义内容组合。
- `OutputForm` 表示同一讲义版本的一种输出形式。
- `GeneratedFile` 记录某一次实际生成结果。

### 0.2 与现有仓库文档的关系

当前仓库旧文档中，`SectionPlan`、`HandoutPlan` 仍被描述为目标概念，并且阶段 5.6 明确“不实现 `SectionPlan` / `HandoutPlan` 模型”。本 V2 文档代表一次新的后端模型重建决策：

- 不再沿用旧的 `SectionPlan` / `HandoutPlan` 命名作为本轮后端模型。
- 使用外部设计文档中的 `SectionVariant` / `HandoutVersion` / `OutputForm` 作为新模型主线。
- 现有 `Section`、`Handout` 的语义需要调整，不再与旧 API 语义完全兼容。
- 当前前端页面可以暂时失效，本轮不为现有静态页面保持兼容。

如果后续任务说明与本文档冲突，应先停下说明冲突，再由用户确认。

### 0.3 分层落点

仍遵守仓库主线分层：

```text
浏览器管理端 / Word 加载项
    -> 题库本地服务
        -> 题库应用
            -> 题库核心

题库基础设施 -> 题库核心
题库本地服务 -> 题库应用 + 题库基础设施
```

后端 V2 模型落点：

- `题库核心`：领域对象、枚举、领域规则、仓储契约、文档处理契约。
- `题库应用`：围绕 V2 模型的用例编排、结果 DTO、请求 DTO、生成流程、引用分析。
- `题库基础设施`：SQLite 映射、仓储实现、文件存储、DOCX/HTML/纯文本处理、Word 生成实现。
- `题库本地服务`：控制器、路由、依赖注入、题库初始化。

禁止事项：

- 不新增 `TagRunner.*` 项目、命名空间或目录。
- 不把新 CMS 模型写入 `Core.QuestionBank`。
- 不让 `题库应用` 直接引用 `题库基础设施`。
- 不因本次重建修改 `VSTO` 和 `Word本地文件操作核心库` 的定位。
- 不在本轮修改前端页面。

## 1. 系统定位

本系统不是普通题库系统，也不是普通 Word 文件管理器。

V2 后端模型服务的核心工作流是：

```text
教学主题
↓
小节 / 上帝版本
↓
原子小节 / 内容块
↓
小节子版本
↓
讲义版本
↓
输出形式
↓
生成文件
```

真实工作流：

```text
1. 教师围绕某个教学主题建立多个小节。
2. 每个小节是一套“上帝版本”的内容池。
3. 小节中可以放置内容块，也可以放置原子小节。
4. 原子小节内部可以放置内容块，但不能继续放置原子小节。
5. 小节可以从自己的上帝版本内容池中切出多个小节子版本。
6. 讲义版本由多个小节子版本或内容块组合而成。
7. 同一个讲义版本可以通过不同输出形式生成学生版、教师版、A3版、PDF、Word 等文件。
8. DOCX 负责富文本正文和输出模板。
9. 数据库负责结构、关系、版本、引用、选择和生成记录。
```

## 2. 总体建模原则

### 2.1 不把 Word 当结构源头

Word / DOCX 负责：

```text
1. 题目正文
2. 知识点正文
3. 解析正文
4. 公式
5. 图片
6. 复杂排版
7. 输出模板
```

数据库负责：

```text
1. 内容属于哪个教学主题。
2. 小节里有什么。
3. 小节子版本选择了什么。
4. 讲义版本组合了什么。
5. 输出形式使用什么模板。
6. 某次生成使用了哪些内容版本。
```

约束：

- 结构信息不能写入 Word 正文。
- 元数据不能只靠 Word 正文解析得到。
- 版本关系不能写入 Word 正文。
- 生成关系不能写入 Word 正文。
- DOCX 内可以保留富文本内容和模板格式，但不作为长期结构管理的唯一来源。

### 2.2 默认不保存 CreatedTime

核心对象默认不保存 `CreatedTime`。

如需时间字段，优先只保存：

```text
UpdatedTime
```

例外：

```text
GeneratedFile.GeneratedTime
```

`GeneratedFile.GeneratedTime` 表示某个文件实际生成的时间，是业务事实，可以保留。

当前旧模型里大量对象已有 `CreatedTime`。V2 重建时应优先移除 CMS 核心对象的 `CreatedTime`，除非有明确业务事实需要保留。

### 2.3 难度字段原则

难度字段只允许放在：

```text
ContentBlock
Section
SectionVariant
```

难度字段不允许放在：

```text
ContentBlockVersion
SectionItem
AtomicSection
AtomicSectionItem
ContentBlockRelation
HandoutVersion
HandoutVersionItem
OutputForm
GeneratedFile
TeachingNote
```

含义：

```text
ContentBlock.Difficulty：内容块本体难度。
Section.Difficulty：这套小节上帝版本的总体难度定位。
SectionVariant.Difficulty：这个小节子版本的总体难度定位。
```

### 2.4 内容块版本不保存内容属性

`ContentBlockVersion` 只表示正文版本。

`ContentBlockVersion` 不保存：

```text
Difficulty
BlockType
QuestionType
```

如果某次正文修改导致难度、内容类型或题型发生本质变化，应新建 `ContentBlock`，不要让同一个 `ContentBlock` 的不同版本承担不同内容属性。

### 2.5 教学备注独立建模

教学备注不归入 `ContentBlock`。

原因：

```text
ContentBlock 是可输出内容资产。
TeachingNote 是教师经验、反思、修改建议、未来提醒。
```

教学备注单独建模为 `TeachingNote`，通过 `TargetType + TargetId` 挂载到不同对象。

### 2.6 内容块可以包含内容块

`ContentBlock` 可以直接包含其他 `ContentBlock`。

不拆分为：

```text
AtomicContentBlock
CompositeContentBlock
```

统一仍然叫：

```text
ContentBlock
```

包含关系通过：

```text
ContentBlockRelation
```

表达。

### 2.7 原子小节是 Section 内部的最小讲解结构

`AtomicSection` 是介于 `Section` 和 `ContentBlock` 之间的教学组织单元。

约束：

```text
1. Section 下可以放 ContentBlock。
2. Section 下可以放 AtomicSection。
3. AtomicSection 内部只能放 ContentBlock。
4. AtomicSection 内部不能再放 AtomicSection。
5. HandoutVersionItem 不允许直接引用 AtomicSection。
```

## 3. 数据对象总清单

V2 后端模型包含：

```text
TeachingTopic【教学主题】
Section【小节 / 上帝版本】
SectionItem【小节内容项】
AtomicSection【原子小节】
AtomicSectionItem【原子小节内容项】
SectionVariant【小节子版本】
SectionVariantItem【小节子版本项】
ContentBlock【内容块】
ContentBlockVersion【内容块版本】
ContentBlockRelation【内容块包含关系】
Handout【讲义项目】
HandoutVersion【讲义版本】
HandoutVersionItem【讲义版本项】
OutputTemplate【输出模板】
OutputForm【输出形式】
GeneratedFile【生成文件】
TeachingNote【教学备注】
```

建议枚举：

```text
TeachingTopicStatus
SectionType
SectionStatus
DifficultyLevel
SectionItemTargetType
ReferenceMode
SectionItemStatus
AtomicSectionType
AtomicSectionStatus
SectionVariantType
SectionVariantStatus
ContentBlockType
QuestionType
ContentBlockStatus
HandoutStatus
HandoutVersionType
HandoutVersionStatus
HandoutVersionItemTargetType
OutputTemplateStatus
OutputAudience
OutputFormat
VisibilityMode
OutputFormStatus
TeachingNoteTargetType
TeachingNoteType
TeachingNoteStatus
```

## 4. 总体关系

```text
TeachingTopic【教学主题】
    └── Section【小节 / 上帝版本】
            ├── SectionItem【小节内容项】
            │       ├── TargetType = ContentBlock
            │       │       └── ContentBlock【内容块】
            │       │               ├── ContentBlockVersion【内容块版本】
            │       │               └── ContentBlockRelation【内容块包含关系】
            │       │                       └── ContentBlock【子内容块】
            │       │
            │       └── TargetType = AtomicSection
            │               └── AtomicSection【原子小节】
            │                       └── AtomicSectionItem【原子小节内容项】
            │                               └── ContentBlock【内容块】
            │
            └── SectionVariant【小节子版本】
                    └── SectionVariantItem【小节子版本项】
                            └── SectionItem【小节内容项】

Handout【讲义项目】
    └── HandoutVersion【讲义版本】
            ├── HandoutVersionItem【讲义版本项】
            │       ├── SectionVariant【小节子版本】
            │       └── ContentBlock【内容块】
            │
            └── OutputForm【输出形式】
                    ├── OutputTemplate【输出模板 DOCX】
                    └── GeneratedFile【生成文件】

TeachingNote【教学备注】
    ├── 可挂到 TeachingTopic
    ├── 可挂到 Section
    ├── 可挂到 SectionVariant
    ├── 可挂到 SectionItem
    ├── 可挂到 AtomicSection
    ├── 可挂到 ContentBlock
    └── 可挂到 HandoutVersion
```

## 5. TeachingTopic【教学主题】

### 5.1 职责

`TeachingTopic` 表示知识体系中的一个教学主题位置。

它回答：

```text
这个教学内容属于知识体系中的哪里？
```

示例：

```text
力学
功和能
机械能守恒
功能关系
圆周运动
带电粒子在磁场中的运动
```

它不是：

```text
课时
讲义
内容块
输出版本
小节子版本
```

### 5.2 关系

```text
TeachingTopic 1 - N Section
TeachingTopic 可以有父级 TeachingTopic
TeachingNote 可以挂到 TeachingTopic
```

### 5.3 建议字段

```text
Id
ParentId
Name
Description
SortOrder
Status
UpdatedTime
```

### 5.4 字段说明

- `Id`：教学主题唯一标识。
- `ParentId`：父级教学主题 ID，用于构建知识树。
- `Name`：教学主题名称。
- `Description`：主题说明，只描述该主题的知识定位，不承载具体讲法。
- `SortOrder`：同级主题排序。
- `Status`：主题状态，建议：`Active`、`Archived`。
- `UpdatedTime`：更新时间。

### 5.5 约束

- `ParentId` 不能指向自身。
- 教学主题树必须防止循环父级。
- 同级 `SortOrder` 用于排序，不要求全局唯一。
- `TeachingTopic` 不保存题目正文、讲义正文、讲课流程、输出规则和讲义版本。

## 6. Section【小节 / 上帝版本】

### 6.1 职责

`Section` 表示某个教学主题下的一套完整内容池。

它本身就是一套“上帝版本”。

也就是说，`Section` 不是以下具体版本：

```text
基础版
提高版
一轮复习版
拔尖班版
课后作业版
```

这些具体版本应该由 `SectionVariant` 表达。

示例：

```text
TeachingTopic：机械能守恒

Section：
- 机械能守恒·常规新课总库
- 机械能守恒·一轮复习总库
- 机械能守恒·模型专题总库
```

### 6.2 关系

```text
TeachingTopic 1 - N Section
Section 1 - N SectionItem
Section 1 - N SectionVariant
TeachingNote 可以挂到 Section
```

### 6.3 建议字段

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

### 6.4 字段说明

- `Id`：小节唯一标识。
- `TeachingTopicId`：所属教学主题 ID。
- `Title`：小节标题，例如“常规新课总库”“一轮复习总库”“模型专题总库”。
- `Description`：说明这套内容池的定位。
- `Type`：小节类型，建议：`NormalCourse`、`FirstRoundReview`、`SpecialTopic`、`ExamTraining`、`Custom`。
- `Difficulty`：小节主难度，表示这套上帝版本整体面向的难度层级。建议：`Unset`、`Basic`、`Medium`、`Advanced`、`Top`。
- `Status`：状态，建议：`Draft`、`Active`、`Archived`。
- `SortOrder`：同一个 `TeachingTopic` 下的 `Section` 排序。
- `UpdatedTime`：更新时间。

### 6.5 约束

- `Section` 不直接保存 Word 正文。
- `Section` 通过 `SectionItem` 引用 `ContentBlock` 或 `AtomicSection`。
- `Section.Difficulty` 表示上帝版本内容池整体定位。
- `Section` 不能直接作为讲义版本项被引用；讲义版本项应引用 `SectionVariant`。
- 一个 `TeachingTopic` 下允许多个 `Section`。

## 7. SectionItem【小节内容项】

### 7.1 职责

`SectionItem` 表示某个目标对象被放入某个 `Section` 中的那一次。

它是 `Section` 与 `ContentBlock / AtomicSection` 之间的关系对象。

它回答：

```text
这个内容或原子小节在这个小节内容池中放在哪里？
```

### 7.2 为什么需要 SectionItem

不能让 `Section` 直接保存 `ContentBlockId` 或 `AtomicSectionId` 列表，因为系统需要表达：

```text
1. 目标对象在小节中的排序。
2. 目标对象在小节中的层级结构。
3. 引用内容块时是否锁定某个版本。
4. 同一个内容块是否可以在同一个小节中出现多次。
5. 小节子版本选择的是该目标对象的哪一次出现。
```

### 7.3 关系

```text
Section 1 - N SectionItem
SectionItem N - 1 ContentBlock 或 AtomicSection
SectionItem 可以有父级 SectionItem
SectionVariantItem N - 1 SectionItem
TeachingNote 可以挂到 SectionItem
```

### 7.4 建议字段

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

### 7.5 字段说明

- `Id`：小节内容项唯一标识。
- `SectionId`：所属 `Section`。
- `TargetType`：目标类型，只允许 `ContentBlock`、`AtomicSection`。
- `TargetId`：目标对象 ID。
- `ReferenceMode`：引用版本模式，只在 `TargetType = ContentBlock` 时有意义。建议：`FollowLatest`、`LockedVersion`。
- `LockedContentBlockVersionId`：当 `TargetType = ContentBlock` 且 `ReferenceMode = LockedVersion` 时使用。
- `TitleOverride`：标题覆盖。
- `ParentItemId`：父级 `SectionItem`，用于让小节内部形成树形组织。
- `SortOrder`：同级排序。
- `SelectionLayer`：选用层级，不是难度。例如：基础必讲、提高补充、拔尖拓展、课堂备用、课后作业、一轮复习、模型专题。
- `TeachingUseOverride`：教学用途覆盖，可空。
- `Status`：状态，建议：`Active`、`Hidden`、`Archived`。
- `Note`：小节局部备注，只放简短说明。
- `UpdatedTime`：更新时间。

### 7.6 重要约束

```text
1. SectionItem 不保存 Difficulty。
2. 难度属于 ContentBlock、Section 或 SectionVariant。
3. SectionItem 主要保存“放置关系”和“小节内部组织信息”。
4. SectionItem 可以指向 ContentBlock 或 AtomicSection。
5. SectionItem 不能直接指向 ContentBlockVersion。
6. 锁定版本通过 LockedContentBlockVersionId 表达。
```

补充约束：

- 当 `TargetType = AtomicSection` 时，`ReferenceMode` 和 `LockedContentBlockVersionId` 应为空或忽略。
- `ParentItemId` 必须属于同一个 `Section`。
- `ParentItemId` 不能指向自身。
- 小节内容项树必须防止循环。
- 允许同一个 `ContentBlock` 在同一个 `Section` 中出现多次，因为 `SectionVariantItem` 选择的是某一次出现。
- 删除 `SectionItem` 只删除放置关系，不删除目标对象。

## 8. AtomicSection【原子小节】

### 8.1 职责

`AtomicSection` 表示 `Section` 内部的最小讲解结构。

它用于表达一个小节中的最小讲解单元。

示例：

```text
守恒条件的建立
守恒条件的判断
从能量转化到机械能守恒
机械能守恒常见误区
```

它与 `ContentBlock` 很接近，但职责不同：

```text
ContentBlock 是可复用内容资产。
AtomicSection 是小节内部的讲解组织单元。
```

### 8.2 关系

```text
SectionItem 可以引用 AtomicSection
AtomicSection 1 - N AtomicSectionItem
AtomicSectionItem N - 1 ContentBlock
TeachingNote 可以挂到 AtomicSection
```

### 8.3 建议字段

```text
Id
Title
Description
Type
Status
UpdatedTime
```

### 8.4 字段说明

- `Id`：原子小节唯一标识。
- `Title`：原子小节标题。
- `Description`：原子小节说明。
- `Type`：原子小节类型，建议：`ConceptBuild`、`MethodExplain`、`ExampleExplain`、`MistakeAnalysis`、`ExerciseArrange`、`Custom`。
- `Status`：状态，建议：`Draft`、`Active`、`Archived`。
- `UpdatedTime`：更新时间。

### 8.5 重要约束

```text
1. AtomicSection 内部可以放置 ContentBlock。
2. AtomicSection 内部不能放置 AtomicSection。
3. AtomicSection 不能被 HandoutVersionItem 直接引用。
4. AtomicSection 只能通过 SectionItem 放入 Section。
5. AtomicSection 不保存 Difficulty。
```

补充约束：

- `AtomicSection` 不直接保存 DOCX 正文。
- `AtomicSection` 通过 `AtomicSectionItem` 引用 `ContentBlock`。
- 删除 `AtomicSection` 前需要检查是否仍被 `SectionItem` 引用。

## 9. AtomicSectionItem【原子小节内容项】

### 9.1 职责

`AtomicSectionItem` 表示某个 `ContentBlock` 被放入某个 `AtomicSection` 中。

它是 `AtomicSection` 和 `ContentBlock` 之间的关系对象。

### 9.2 关系

```text
AtomicSection 1 - N AtomicSectionItem
AtomicSectionItem N - 1 ContentBlock
```

### 9.3 建议字段

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

### 9.4 字段说明

- `Id`：原子小节内容项唯一标识。
- `AtomicSectionId`：所属 `AtomicSection`。
- `ContentBlockId`：引用的 `ContentBlock`。
- `ReferenceMode`：引用版本模式，建议：`FollowLatest`、`LockedVersion`。
- `LockedContentBlockVersionId`：锁定版本 ID。
- `TitleOverride`：标题覆盖。
- `SortOrder`：排序。
- `Note`：简短局部说明。
- `UpdatedTime`：更新时间。

### 9.5 重要约束

```text
1. AtomicSectionItem 只能引用 ContentBlock。
2. AtomicSectionItem 不能引用 AtomicSection。
3. AtomicSectionItem 不需要 ParentItemId。
4. AtomicSection 内部不允许形成多层 AtomicSection 结构。
```

补充约束：

- 当 `ReferenceMode = LockedVersion` 时，必须提供有效的 `LockedContentBlockVersionId`。
- `LockedContentBlockVersionId` 必须属于 `ContentBlockId` 对应内容块。
- 删除 `AtomicSectionItem` 只删除引用关系，不删除内容块。

## 10. ContentBlock【内容块】

### 10.1 职责

`ContentBlock` 是系统中的核心内容资产，表示一个可复用的教学内容。

示例：

```text
知识点
讲解
题目
答案
解析
方法总结
易错点
类比
图示说明
例题组
练习组
变式题组
普通说明
```

### 10.2 关系

```text
ContentBlock 1 - N ContentBlockVersion
ContentBlock 可以通过 ContentBlockRelation 包含其他 ContentBlock
ContentBlock 可以被多个 SectionItem 引用
ContentBlock 可以被多个 AtomicSectionItem 引用
ContentBlock 可以被多个 HandoutVersionItem 直接引用
TeachingNote 可以挂到 ContentBlock
```

### 10.3 建议字段

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

### 10.4 字段说明

- `Id`：内容块唯一标识。
- `Title`：内容块标题。
- `Summary`：内容摘要。
- `BlockType`：内容块类型。建议：`KnowledgePoint`、`Explanation`、`Question`、`Answer`、`Analysis`、`MethodSummary`、`CommonMistake`、`Analogy`、`DiagramNote`、`ExampleGroup`、`ExerciseGroup`、`VariantGroup`、`GeneralText`。
- `Difficulty`：内容块本体难度。建议：`Unset`、`Basic`、`Medium`、`Advanced`、`Top`。
- `QuestionType`：题型，只对 `BlockType = Question` 有意义。建议：`Choice`、`Blank`、`Calculation`、`Experiment`、`Diagram`、`Composite`。
- `Status`：状态，建议：`Draft`、`Active`、`Archived`。
- `CurrentVersionId`：当前版本 ID，指向 `ContentBlockVersion`。
- `UpdatedTime`：更新时间。

### 10.5 不负责的内容

`ContentBlock` 不负责：

```text
1. 在小节中的排序。
2. 在原子小节中的排序。
3. 在讲义中的排序。
4. 进入哪个 SectionVariant。
5. 输出成学生版还是教师版。
6. 教学备注。
```

### 10.6 约束

- `QuestionType` 只在 `BlockType = Question` 时有意义。
- `QuestionType` 不合并进 `BlockType`。
- `CurrentVersionId` 可以为空，表示内容块已创建但正文版本尚未生成。
- `ContentBlock` 可以包含其他 `ContentBlock`，关系由 `ContentBlockRelation` 表达。
- `ContentBlockVersion` 不保存难度、内容类型、题型。
- 内容块正文主资产仍为 `.docx`。

## 11. ContentBlockVersion【内容块版本】

### 11.1 职责

`ContentBlockVersion` 保存某个 `ContentBlock` 的某一版正文。

```text
ContentBlock = 内容资产本体
ContentBlockVersion = 该资产的正文历史版本
```

### 11.2 关系

```text
ContentBlock 1 - N ContentBlockVersion
SectionItem / AtomicSectionItem / ContentBlockRelation 可以选择跟随最新版本或锁定某个版本
GeneratedFile 需要记录实际使用的 ContentBlockVersion
```

### 11.3 建议字段

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

### 11.4 字段说明

- `Id`：版本唯一标识。
- `ContentBlockId`：所属内容块。
- `VersionNumber`：版本号，同一个 `ContentBlock` 内递增。
- `DocxPath`：正文 DOCX 文件路径。
- `HtmlPreviewPath`：HTML 预览文件路径，可以为空。
- `PlainText`：纯文本内容，用于搜索、摘要、索引，不是正文主来源。
- `IsCurrent`：是否当前版本。
- `UpdatedTime`：更新时间。

### 11.5 重要约束

```text
1. ContentBlockVersion 不保存 Difficulty。
2. ContentBlockVersion 不保存 BlockType。
3. ContentBlockVersion 不保存 QuestionType。
4. 如果某次正文修改导致难度或内容类型发生本质变化，应该新建 ContentBlock。
```

补充约束：

- 同一 `ContentBlockId` 下 `VersionNumber` 唯一。
- 同一 `ContentBlockId` 下最多一个 `IsCurrent = true`。
- `DocxPath` 必须指向题库根目录下的内容块文件。
- `HtmlPreviewPath` 可以为空，但健康检查应能识别缺失预览。

## 12. ContentBlockRelation【内容块包含关系】

### 12.1 职责

`ContentBlockRelation` 用来实现 `ContentBlock` 直接包含 `ContentBlock`。

它不是另一种内容块，只是包含关系。

示例：

```text
ContentBlock：机械能守恒例题组
    ├── ContentBlock：题目1
    ├── ContentBlock：题目2
    └── ContentBlock：题目3
```

### 12.2 关系

```text
Parent ContentBlock 1 - N ContentBlockRelation
ContentBlockRelation N - 1 Child ContentBlock
```

### 12.3 建议字段

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

### 12.4 字段说明

- `Id`：内容块包含关系唯一标识。
- `ParentBlockId`：父内容块 ID。
- `ChildBlockId`：子内容块 ID。
- `ReferenceMode`：引用版本模式，建议：`FollowLatest`、`LockedVersion`。
- `LockedContentBlockVersionId`：锁定版本 ID。
- `TitleOverride`：子内容块在父内容块中显示的标题覆盖。
- `SortOrder`：子内容块排序。
- `Note`：简短局部说明。
- `UpdatedTime`：更新时间。

### 12.5 重要约束

```text
1. ContentBlockRelation 不保存教学用途。
2. 教学用途优先由父 ContentBlock 决定。
3. 例如几个题目构成一个例题组，则“例题组”由父 ContentBlock 的 BlockType 决定。
4. 必须防止循环包含，例如 A 包含 B，B 包含 C，C 又包含 A。
```

补充约束：

- `ParentBlockId` 不能等于 `ChildBlockId`。
- 当 `ReferenceMode = LockedVersion` 时，必须提供有效的 `LockedContentBlockVersionId`。
- `LockedContentBlockVersionId` 必须属于 `ChildBlockId` 对应内容块。
- 删除关系只删除包含关系，不删除子内容块。
- 同一父块下允许重复引用同一子块，但应由后续 UI 明确提示。
- 默认最大展开深度建议为 10 层，超出时生成和健康检查应报错。

## 13. SectionVariant【小节子版本】

### 13.1 职责

`SectionVariant` 表示从一个 `Section` 上帝版本中切出的一个具体可用版本。

示例：

```text
Section：机械能守恒·常规新课总库

SectionVariant：
- 基础讲解版
- 提高班版
- 拔尖班版
- 课后作业版
```

它不复制正文，也不直接引用 `ContentBlock` 或 `AtomicSection`。

它通过 `SectionVariantItem` 选择 `SectionItem`。

### 13.2 关系

```text
Section 1 - N SectionVariant
SectionVariant 1 - N SectionVariantItem
SectionVariantItem N - 1 SectionItem
HandoutVersionItem 可以引用 SectionVariant
TeachingNote 可以挂到 SectionVariant
```

### 13.3 建议字段

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

### 13.4 字段说明

- `Id`：小节子版本唯一标识。
- `SectionId`：所属 `Section`。
- `Title`：子版本标题，例如“基础讲解版”“提高班版”“拔尖班版”“一轮复习版”。
- `Description`：子版本说明。
- `Type`：子版本类型，建议：`Lecture`、`Exercise`、`Homework`、`Review`、`ExamTraining`、`Custom`。
- `Difficulty`：子版本整体难度定位。
- `Status`：状态，建议：`Draft`、`Active`、`Archived`。
- `SortOrder`：同一 `Section` 下的子版本排序。
- `UpdatedTime`：更新时间。

### 13.5 约束

- `SectionVariant` 不直接引用 `ContentBlock`。
- `SectionVariant` 不直接引用 `AtomicSection`。
- `SectionVariant` 通过 `SectionVariantItem` 选择 `SectionItem`。
- `Difficulty` 允许保存在 `SectionVariant`，表示这一子版本整体难度。
- 删除 `SectionVariant` 前需要检查是否仍被 `HandoutVersionItem` 引用。

## 14. SectionVariantItem【小节子版本项】

### 14.1 职责

`SectionVariantItem` 表示某个 `SectionVariant` 选择了哪个 `SectionItem`。

也就是说：

```text
SectionItem：目标对象被放进上帝版本。
SectionVariantItem：上帝版本中的某个内容项被选进子版本。
```

### 14.2 关系

```text
SectionVariant 1 - N SectionVariantItem
SectionVariantItem N - 1 SectionItem
```

### 14.3 建议字段

```text
Id
SectionVariantId
SectionItemId
SortOrder
Note
UpdatedTime
```

### 14.4 字段说明

- `Id`：小节子版本项唯一标识。
- `SectionVariantId`：所属小节子版本。
- `SectionItemId`：被选中的小节内容项。
- `SortOrder`：该内容项在子版本中的排序。子版本中的排序可以和上帝版本中的排序一致，也可以局部调整。
- `Note`：该内容项在当前子版本中的简短备注。
- `UpdatedTime`：更新时间。

### 14.5 重要约束

```text
1. 第一版只保存被选中的 SectionItem。
2. 不保存 IsIncluded。
3. 如果某个 SectionItem 没有出现在 SectionVariantItem 中，就表示它没有被选入该子版本。
```

补充约束：

- `SectionItemId` 必须属于 `SectionVariant.SectionId` 对应的 `Section`。
- 删除 `SectionVariantItem` 只表示该内容项不再进入这个子版本，不删除源 `SectionItem`。
- 允许子版本排序与上帝版本排序不一致。

## 15. Handout【讲义项目】

### 15.1 职责

`Handout` 表示一组相关讲义的总入口，不是某一份具体可打印讲义。

示例：

```text
机械能守恒专题讲义
功能关系专题讲义
圆周运动单元讲义
带电粒子磁场模型讲义
```

### 15.2 关系

```text
Handout 1 - N HandoutVersion
```

### 15.3 建议字段

```text
Id
Title
Description
Status
UpdatedTime
```

### 15.4 字段说明

- `Id`：讲义项目唯一标识。
- `Title`：讲义项目标题。
- `Description`：讲义项目说明。
- `Status`：状态，建议：`Draft`、`Active`、`Archived`。
- `UpdatedTime`：更新时间。

### 15.5 约束

- `Handout` 不直接保存讲义内容项。
- `Handout` 不直接生成文件。
- 具体讲义内容组合由 `HandoutVersion` 表达。
- 一个 `Handout` 可以拥有多个 `HandoutVersion`。

## 16. HandoutVersion【讲义版本】

### 16.1 职责

`HandoutVersion` 表示某一套具体讲义内容组合。

示例：

```text
机械能守恒专题讲义 / 基础版
机械能守恒专题讲义 / 提高版
机械能守恒专题讲义 / 一轮复习版
```

注意：

```text
讲义版本不保存难度字段。
```

讲义版本的层级或定位由标题、类型、引用的 `SectionVariant` 和 `ContentBlock` 决定。

### 16.2 关系

```text
Handout 1 - N HandoutVersion
HandoutVersion 1 - N HandoutVersionItem
HandoutVersion 1 - N OutputForm
TeachingNote 可以挂到 HandoutVersion
```

### 16.3 建议字段

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

### 16.4 字段说明

- `Id`：讲义版本唯一标识。
- `HandoutId`：所属讲义项目 ID。
- `Title`：讲义版本标题，例如“基础版”“提高版”“一轮复习版”“模型专题版”。
- `Description`：讲义版本说明。
- `Type`：讲义版本类型，建议：`Normal`、`Review`、`SpecialTopic`、`ExamTraining`、`Custom`。
- `Status`：状态，建议：`Draft`、`Active`、`Archived`。
- `SortOrder`：同一 `Handout` 下的版本排序。
- `UpdatedTime`：更新时间。

### 16.5 约束

- `HandoutVersion` 不保存 `Difficulty`。
- `HandoutVersion` 通过 `HandoutVersionItem` 引用 `SectionVariant` 或 `ContentBlock`。
- `HandoutVersion` 通过 `OutputForm` 定义输出形式。
- 删除 `HandoutVersion` 前需要检查是否已有 `OutputForm` 或生成文件记录。

## 17. HandoutVersionItem【讲义版本项】

### 17.1 职责

`HandoutVersionItem` 表示某个 `HandoutVersion` 引用了哪些内容。

第一版只允许引用：

```text
1. SectionVariant
2. ContentBlock
```

主要引用 `SectionVariant`。

允许直接引用 `ContentBlock` 是为了插入：

```text
封面说明
单独题组
补充说明
临时内容
```

### 17.2 关系

```text
HandoutVersion 1 - N HandoutVersionItem
HandoutVersionItem N - 1 SectionVariant 或 ContentBlock
```

### 17.3 建议字段

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

### 17.4 字段说明

- `Id`：讲义版本项唯一标识。
- `HandoutVersionId`：所属讲义版本。
- `TargetType`：引用目标类型，只允许 `SectionVariant`、`ContentBlock`。
- `TargetId`：引用目标 ID。
- `SortOrder`：排序。
- `TitleOverride`：标题覆盖。
- `Note`：简短备注。
- `UpdatedTime`：更新时间。

### 17.5 重要约束

```text
1. HandoutVersionItem 不允许直接引用 AtomicSection。
2. AtomicSection 只能通过 Section → SectionItem → SectionVariant 间接进入讲义。
3. HandoutVersionItem 不允许直接引用 Section。
4. HandoutVersionItem 主要引用 SectionVariant，直接引用 ContentBlock 只用于补充内容。
```

补充约束：

- `HandoutVersionItem` 不保存难度。
- 直接引用 `ContentBlock` 时，第一版不在 `HandoutVersionItem` 上记录锁定版本模式；生成时默认解析当前版本。若未来需要锁定直接内容块版本，应作为扩展设计。
- 删除 `HandoutVersionItem` 只删除讲义版本中的引用，不删除目标对象。

## 18. OutputTemplate【输出模板】

### 18.1 职责

`OutputTemplate` 表示一个 DOCX 输出模板。

它负责承载格式规范，而不是让系统用字段描述复杂排版。

示例：

```text
A4 学生讲义模板.docx
A4 教师讲义模板.docx
A3 课堂讲义模板.docx
专题讲义模板.docx
周测卷模板.docx
```

模板 DOCX 内部负责：

```text
纸张大小
页边距
页眉页脚
字体
段落样式
标题样式
题号样式
答案区样式
分栏
表格
水印
Logo
版心
```

系统不应该把这些格式规格全部拆成数据库字段。

### 18.2 关系

```text
OutputForm N - 1 OutputTemplate
```

### 18.3 建议字段

```text
Id
Title
Description
TemplateDocxPath
Status
UpdatedTime
```

### 18.4 字段说明

- `Id`：输出模板唯一标识。
- `Title`：模板名称。
- `Description`：模板说明。
- `TemplateDocxPath`：模板 DOCX 文件路径。
- `Status`：状态，建议：`Active`、`Archived`。
- `UpdatedTime`：更新时间。

### 18.5 后续扩展

第一版暂不做模板占位符对象。

后续如果需要，可以增加：

```text
TemplateSlot
Placeholder
Bookmark
ContentControl
```

## 19. OutputForm【输出形式】

### 19.1 职责

`OutputForm` 表示同一个 `HandoutVersion` 的一种输出形式。

它不负责讲义内容选择，也不负责复杂排版规则。

它负责：

```text
1. 属于哪个 HandoutVersion。
2. 使用哪个 OutputTemplate。
3. 面向什么受众。
4. 输出 Word / PDF / WordAndPdf。
5. 使用什么可见性规则。
```

示例：

```text
提高版 - 学生版 PDF
提高版 - 教师版 PDF
提高版 - A3 课堂版 PDF
提高版 - 可编辑 Word
```

### 19.2 关系

```text
HandoutVersion 1 - N OutputForm
OutputForm N - 1 OutputTemplate
OutputForm 1 - N GeneratedFile
```

### 19.3 建议字段

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

### 19.4 字段说明

- `Id`：输出形式唯一标识。
- `HandoutVersionId`：所属讲义版本。
- `OutputTemplateId`：使用的 DOCX 输出模板。
- `Title`：输出形式标题，例如“学生版”“教师版”“A3课堂版”“可编辑 Word”。
- `Audience`：受众，建议：`Student`、`Teacher`、`Mixed`。
- `OutputFormat`：输出格式，建议：`Word`、`Pdf`、`WordAndPdf`。
- `VisibilityMode`：可见性模式，第一版可以先用：`StudentNoAnswer`、`TeacherWithAnswer`、`Classroom`、`Custom`。后续可以扩展为独立 `VisibilityProfile`。
- `Status`：状态，建议：`Active`、`Archived`。
- `SortOrder`：同一 `HandoutVersion` 下的输出形式排序。
- `UpdatedTime`：更新时间。

### 19.5 约束

- `OutputForm` 不负责内容选择。
- `OutputForm` 不保存难度。
- `OutputForm` 不保存复杂排版字段。
- `OutputTemplateId` 可以在早期实现中允许为空，表示使用系统默认模板；如果这样做，应在用例层明确默认模板解析规则。
- 生成文件必须挂到 `OutputForm`。

## 20. GeneratedFile【生成文件】

### 20.1 职责

`GeneratedFile` 记录某一次实际生成出来的文件。

示例：

```text
机械能守恒专题讲义-提高版-学生版-20260604.pdf
机械能守恒专题讲义-提高版-教师版-20260604.docx
```

### 20.2 关系

```text
OutputForm 1 - N GeneratedFile
```

### 20.3 建议字段

```text
Id
OutputFormId
FilePath
VersionManifestJson
GeneratedTime
```

### 20.4 字段说明

- `Id`：生成文件唯一标识。
- `OutputFormId`：所属输出形式。
- `FilePath`：生成文件路径。
- `VersionManifestJson`：本次生成实际使用的内容版本清单。例如记录 `ContentBlock 101 使用 Version 3`。
- `GeneratedTime`：生成时间。这是业务事实，可以保留。

### 20.5 约束

- `GeneratedFile` 只记录实际生成的文件。
- `GeneratedTime` 是允许保留的时间字段。
- `VersionManifestJson` 必须记录可追溯的内容版本。
- 后续如需同时生成 Word 和 PDF，可以为同一 `OutputForm` 创建多条 `GeneratedFile`，或在扩展设计中加入文件格式字段；第一版按文档字段保持最小模型。

## 21. TeachingNote【教学备注】

### 21.1 职责

`TeachingNote` 单独保存教学经验、讲解反思、修改建议、未来提醒。

它不归入 `ContentBlock`。

原因：

```text
ContentBlock 主要是可输出内容资产。
TeachingNote 主要面向教师自己。
```

### 21.2 典型内容

```text
学生容易误解什么
这个知识点怎么讲更自然
这道题放在这里太早了
下次讲义要加一个反例
这个比喻效果很好
这个小节的例题顺序要调整
这一版讲义不适合基础弱的学生
```

### 21.3 关系

`TeachingNote` 可以挂到不同目标对象上。

第一版建议允许挂到：

```text
TeachingTopic
Section
SectionVariant
SectionItem
AtomicSection
ContentBlock
HandoutVersion
```

### 21.4 建议字段

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

### 21.5 字段说明

- `Id`：教学备注唯一标识。
- `TargetType`：备注挂载目标类型。建议：`TeachingTopic`、`Section`、`SectionVariant`、`SectionItem`、`AtomicSection`、`ContentBlock`、`HandoutVersion`。
- `TargetId`：挂载目标 ID。
- `NoteType`：备注类型。建议：`TeachingReflection`、`RevisionSuggestion`、`CommonMistake`、`TeachingLogic`、`ExampleAdvice`、`QuestionReplacement`、`General`。
- `Title`：备注标题。
- `Content`：备注正文。
- `Status`：状态，建议：`Active`、`Resolved`、`Archived`。
- `UpdatedTime`：更新时间。

### 21.6 约束

- `TeachingNote` 不参与讲义正文输出，除非未来显式设计“输出备注”能力。
- `TeachingNote` 不归入 `ContentBlockVersion`。
- `TeachingNote` 通过 `TargetType + TargetId` 做多态挂载。
- 删除目标对象前，应考虑是否级联归档或保留备注。

## 22. 引用版本模式

统一引用版本模式：

```text
FollowLatest
LockedVersion
```

使用位置：

```text
SectionItem.TargetType = ContentBlock
AtomicSectionItem
ContentBlockRelation
```

不使用位置：

```text
SectionVariantItem
HandoutVersionItem 第一版
OutputForm
GeneratedFile
```

规则：

- `FollowLatest`：生成、预览或展开时解析目标内容块当前版本。
- `LockedVersion`：生成、预览或展开时解析指定版本。
- 锁定版本字段必须属于对应内容块。
- 旧版本引用审查应覆盖所有可锁定版本的位置。

## 23. 生成展开规则

### 23.1 HandoutVersion 展开

讲义生成从 `HandoutVersion` 开始。

```text
HandoutVersion
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

### 23.2 SectionVariant 展开

`SectionVariant` 不复制正文。

它只选择 `Section` 上帝版本中的部分 `SectionItem`：

```text
SectionVariant
  -> SectionVariantItem
      -> SectionItem
```

当 `SectionItem.TargetType = AtomicSection` 时：

```text
AtomicSection
  -> AtomicSectionItem
      -> ContentBlock
```

当 `SectionItem.TargetType = ContentBlock` 时：

```text
ContentBlock
```

### 23.3 ContentBlockRelation 递归展开

`ContentBlock` 可以包含其他 `ContentBlock`。

生成或预览时应递归展开：

```text
ContentBlock
  -> ContentBlockRelation
      -> Child ContentBlock
```

必须防止：

```text
循环包含
最大深度超限
锁定版本缺失
目标内容块缺失
目标内容块版本缺失
```

### 23.4 版本清单

每次生成 `GeneratedFile` 时，必须保存 `VersionManifestJson`。

至少应记录：

```text
HandoutId
HandoutVersionId
OutputFormId
OutputTemplateId
GeneratedTime
使用到的 ContentBlockId
使用到的 ContentBlockVersionId
引用来源路径
引用模式
是否锁定版本
```

`VersionManifestJson` 是后续审查“这份文件当时用了哪些内容版本”的依据。

## 24. 建议数据库表

### 24.1 表清单

建议 CMS V2 表：

```text
TeachingTopics
Sections
SectionItems
AtomicSections
AtomicSectionItems
SectionVariants
SectionVariantItems
ContentBlocks
ContentBlockVersions
ContentBlockRelations
Handouts
HandoutVersions
HandoutVersionItems
OutputTemplates
OutputForms
GeneratedFiles
TeachingNotes
```

可以保留非 CMS 历史表：

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

当前旧 CMS 表如 `ContentBlockChildren`、`HandoutItems`、`HandoutGenerations` 在 V2 中语义被替代：

```text
ContentBlockChildren -> ContentBlockRelations
HandoutItems -> HandoutVersionItems
HandoutGenerations -> GeneratedFiles
```

如果确认没有正式 CMS 数据，可以直接重建表结构；如果有正式数据，应另做迁移计划。

### 24.2 索引建议

```text
TeachingTopics(ParentId, SortOrder)
TeachingTopics(Status)

Sections(TeachingTopicId, SortOrder)
Sections(Status)

SectionItems(SectionId, ParentItemId, SortOrder)
SectionItems(TargetType, TargetId)

AtomicSectionItems(AtomicSectionId, SortOrder)
AtomicSectionItems(ContentBlockId)

SectionVariants(SectionId, SortOrder)
SectionVariants(Status)

SectionVariantItems(SectionVariantId, SortOrder)
SectionVariantItems(SectionItemId)

ContentBlocks(BlockType, Status)
ContentBlocks(CurrentVersionId)
ContentBlocks(Difficulty)

ContentBlockVersions(ContentBlockId, VersionNumber) UNIQUE
ContentBlockVersions(ContentBlockId, IsCurrent)

ContentBlockRelations(ParentBlockId, SortOrder)
ContentBlockRelations(ChildBlockId)
ContentBlockRelations(ParentBlockId, ChildBlockId)

HandoutVersions(HandoutId, SortOrder)
HandoutVersions(Status)

HandoutVersionItems(HandoutVersionId, SortOrder)
HandoutVersionItems(TargetType, TargetId)

OutputForms(HandoutVersionId, SortOrder)
OutputForms(OutputTemplateId)

GeneratedFiles(OutputFormId, GeneratedTime)

TeachingNotes(TargetType, TargetId)
TeachingNotes(Status)
```

## 25. 文件存储建议

保留当前题库根目录和旧题目目录语义：

```text
{题库根目录}
  question-bank.db
  source\
  html\
```

新增 CMS V2 专用目录时，不替换旧 `source` 和 `html`。

建议新目录：

```text
{题库根目录}
  content-blocks\
    source\
      {ContentBlockId}\
        v1.docx
        v2.docx
    html\
      {ContentBlockId}\
        v1.html
        v2.html
    text\
      {ContentBlockId}\
        v1.txt
        v2.txt

  output-templates\
    {OutputTemplateId}\
      template.docx

  generated-files\
    {OutputFormId}\
      {GeneratedFileId}.docx
      {GeneratedFileId}.pdf

  temp\
    edit-sessions\
```

约束：

- 不改变现有题目录入、题目预览、试卷导入使用的旧目录语义。
- 新能力需要路径时，在 `题库路径提供器` 或专用文件存储服务中新增方法。
- 输出模板由 DOCX 文件承载复杂版式，不拆成数据库字段。

## 26. 后端 API 建议

本轮不需要兼容现有前端静态页面。

建议按模型分组重建 API：

```text
/api/题库实例/{题库键}/教学主题
/api/题库实例/{题库键}/小节
/api/题库实例/{题库键}/小节/{id}/项目
/api/题库实例/{题库键}/原子小节
/api/题库实例/{题库键}/原子小节/{id}/项目
/api/题库实例/{题库键}/小节子版本
/api/题库实例/{题库键}/小节子版本/{id}/项目
/api/题库实例/{题库键}/内容块
/api/题库实例/{题库键}/内容块/{id}/版本
/api/题库实例/{题库键}/内容块/{id}/包含关系
/api/题库实例/{题库键}/讲义
/api/题库实例/{题库键}/讲义/{id}/版本
/api/题库实例/{题库键}/讲义版本
/api/题库实例/{题库键}/讲义版本/{id}/项目
/api/题库实例/{题库键}/输出模板
/api/题库实例/{题库键}/输出形式
/api/题库实例/{题库键}/输出形式/{id}/生成
/api/题库实例/{题库键}/生成文件
/api/题库实例/{题库键}/教学备注
/api/题库实例/{题库键}/引用关系
```

最小后端闭环优先级：

```text
1. TeachingTopic CRUD
2. Section CRUD
3. SectionItem CRUD / 排序
4. AtomicSection CRUD
5. AtomicSectionItem CRUD / 排序
6. SectionVariant CRUD
7. SectionVariantItem CRUD / 排序
8. ContentBlock CRUD
9. ContentBlockVersion 创建 / 查询
10. ContentBlockRelation CRUD / 排序 / 循环检测
11. Handout CRUD
12. HandoutVersion CRUD
13. HandoutVersionItem CRUD / 排序
14. OutputTemplate CRUD
15. OutputForm CRUD
16. Generate OutputForm -> GeneratedFile
17. TeachingNote CRUD
```

## 27. 与旧模型相比的关键变化

```text
1. Section 不再只是普通小节，而是某个教学主题下的一套上帝版本内容池。
2. 一个 TeachingTopic 下允许多个 Section。
3. Section 下可以放置 ContentBlock，也可以放置 AtomicSection。
4. AtomicSection 是 Section 内部的最小讲解结构。
5. AtomicSection 内部只能放置 ContentBlock，不能再放置 AtomicSection。
6. SectionVariant 是从 Section 中选择 SectionItem 形成的子版本。
7. Handout 和 HandoutVersion 分开。
8. 基础版、提高版、一轮复习版属于 HandoutVersion，不属于 OutputForm。
9. 学生版、教师版、A3版、PDF、Word 属于 OutputForm。
10. 输出格式细节由 OutputTemplate 的 DOCX 模板承担，不由数据库字段承担。
11. ContentBlock 可以包含 ContentBlock，通过 ContentBlockRelation 表达。
12. ContentBlockVersion 只保存正文版本，不保存难度。
13. Difficulty 只放在 ContentBlock、Section、SectionVariant 上。
14. SectionItem 不保存 Difficulty。
15. AtomicSection 不保存 Difficulty。
16. TeachingNote 独立建模，不归入 ContentBlock。
17. ContentBlockRelation 不保存教学用途，教学用途优先由父 ContentBlock 决定。
18. QuestionType 不合并进 ContentBlockType。
19. HandoutVersionItem 不允许直接引用 AtomicSection。
20. AtomicSection 只能通过 SectionItem 放入 Section，并通过 SectionVariant 间接进入讲义。
```

## 28. 后端重建实施阶段

本节用于后续 Codex 实施时拆分任务。

### 阶段 1：写入本开发文档

目标：

- 新增本文档。
- 不改代码。
- 不改前端。

### 阶段 2：扫描旧 CMS 后端并输出重建计划

目标：

- 对照现有 `内容块模块`、`小节模块`、`讲义模块`、`引用关系模块`。
- 列出旧模型删除、替换、保留范围。
- 明确是否需要数据迁移。
- 不改代码。

### 阶段 3：重建领域对象

目标：

- 在 `题库核心` 中新增或替换 V2 领域对象。
- 新增枚举。
- 新增领域校验。
- 保证解决方案编译通过。

范围：

```text
TeachingTopic
Section
SectionItem
AtomicSection
AtomicSectionItem
SectionVariant
SectionVariantItem
ContentBlock
ContentBlockVersion
ContentBlockRelation
Handout
HandoutVersion
HandoutVersionItem
OutputTemplate
OutputForm
GeneratedFile
TeachingNote
```

### 阶段 4：重建 DbContext 和初始化建表逻辑

目标：

- 在 `题库基础设施` 中重建 CMS V2 表映射。
- 修改 `题库实例初始化器` 的 CMS 相关建表逻辑。
- 保留非 CMS 历史表。
- 保证编译通过。

重点：

- 不继续扩展旧 `HandoutItems` / `HandoutGenerations` 语义。
- 如果无正式 CMS 数据，可以直接建立新表。
- 如果有正式 CMS 数据，先做迁移脚本文档。

### 阶段 5：重建仓储层

目标：

- 在 `题库核心` 定义 V2 仓储契约。
- 在 `题库基础设施` 实现仓储。
- 保证编译通过。

仓储建议：

```text
I教学主题仓储
I小节仓储
I原子小节仓储
I小节子版本仓储
I内容块仓储
I讲义仓储
I输出仓储
I教学备注仓储
```

### 阶段 6：重建应用用例

目标：

- 在 `题库应用` 中重建 V2 用例。
- 先跑通核心链路，不追求一次性完整高级能力。

最小链路：

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

### 阶段 7：重建控制器 API

目标：

- 在 `题库本地服务` 中重建控制器。
- 更新依赖注入注册。
- 不兼容现有前端也可以。
- 保证服务可启动。

### 阶段 8：重建生成链路和引用分析

目标：

- 从 `OutputForm` 生成 `GeneratedFile`。
- 生成过程展开 `HandoutVersionItem -> SectionVariant -> SectionItem -> AtomicSectionItem / ContentBlockRelation`。
- 保存 `VersionManifestJson`。
- 实现旧版本引用检查和影响范围分析的 V2 版本。

### 阶段 9：前端另行重建

本轮不做。

前端后续可以基于 V2 API 重新设计。

## 29. 当前实现注意事项

### 29.1 可以保留的能力

```text
项目分层结构
题库实例体系
题库路径提供器思路
SQLite 本地数据库
内容块 docx 文件存储能力
HTML 预览生成能力
纯文本提取能力
Aspose 授权与 Word 合并能力
本地服务启动结构
标签体系
题目和试卷导入历史能力
```

### 29.2 应重建的能力

```text
CMS 领域对象
CMS DbContext 表结构
CMS 初始化建表逻辑
CMS 仓储
CMS 应用用例
CMS 控制器 API
CMS 引用分析
CMS 讲义生成记录
```

### 29.3 不在本轮范围

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

## 30. 验收标准

阶段性验收标准：

```text
1. 文档阶段：本文档存在，能作为后续实现依据。
2. 领域阶段：核心领域模型和枚举完整，解决方案可编译。
3. 数据库阶段：CMS V2 表结构可初始化，旧非 CMS 表不受破坏。
4. 仓储阶段：每个 V2 聚合至少具备基础创建、查询、更新、关系查询能力。
5. 应用阶段：能跑通 TeachingTopic -> Section -> SectionVariant -> HandoutVersion -> OutputForm 的最小链路。
6. API 阶段：本地服务可启动，V2 API 可调用。
7. 生成阶段：能从 OutputForm 生成文件并记录 GeneratedFile + VersionManifestJson。
```

硬性约束：

- 不破坏题目录入、题目预览、试卷导入等旧流程。
- 不把 CMS V2 新逻辑写进非主线项目。
- 不引入错误依赖方向。
- 不为兼容旧前端牺牲 V2 数据模型语义。
