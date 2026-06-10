# SectionPage 小节结构编辑页

SectionPage 是 CMS V2 前端的核心页面之一。它的目标是教学结构编辑，不是材料收集页，也不是题目列表页。

## 1. 页面目的

SectionPage 回答的问题：

```text
这个教学主题应该怎么讲？
```

页面核心任务：

- 编排 Section 内部结构。
- 组织 ContentBlock 和 AtomicSection。
- 选择或创建 SectionVariant。
- 管理 SectionItem 的顺序、层级和引用模式。
- 在结构编辑过程中快速查看内容块预览和元数据。

## 2. 内容模型理解

### ContentBlock

可编辑内容单元。

示例：

```text
Question
Knowledge Point
Summary
Note
Method Explanation
```

特点：

- 长期保存为 `.docx` 内容资产。
- 可以打开 Word 精细编辑。
- 可以生成 HTML 预览。
- 可以被 Section、AtomicSection、Handout 引用。

### AtomicSection

最小教学结构单元。

特点：

- 组织多个 ContentBlock。
- 自身不直接包含可编辑文档正文。
- 不嵌套 AtomicSection。
- 适合沉淀可复用的讲解片段。

### Section

教学组织结构。

特点：

- 属于 TeachingTopic。
- 由 SectionItem 组成。
- SectionItem 可以引用 ContentBlock 或 AtomicSection。
- Section 不直接编辑文档内容，它编辑教学结构。

## 3. 页面布局

SectionPage 使用三段式工作台：

```text
Toolbar

Left:
SectionStructurePanel

Center:
SectionWorkspace

Right:
SectionInspector
```

左侧 `SectionStructurePanel`：

```text
固定宽度
可折叠
可停靠
显示当前 Section 的结构树
用于快速定位和调整结构
```

中间 `SectionWorkspace`：

```text
弹性宽度
主工作区
占用最大空间
显示展开后的教学内容结构
承载主要编辑操作
```

右侧 `SectionInspector`：

```text
固定宽度
可折叠
可停靠
显示当前选中节点详情
提供版本、预览、备注、引用模式等上下文操作
```

原则：

- 中间工作区必须获得最多注意力。
- 左右侧栏服务于结构定位和细节检查，不应成为主操作区域。
- 页面不应出现四栏常驻布局。

## 4. Toolbar 职责

Toolbar 提供页面级命令：

```text
返回主题工作台
切换 SectionVariant
新建 AtomicSection
插入 ContentBlock
打开内容选择器
打开二级工作流 Drawer
保存结构
刷新预览
```

Toolbar 不承载大量筛选器。复杂筛选进入 Drawer 或 Dialog。

## 5. Secondary Workflows

以下区域不应成为 SectionPage 的常驻主区域：

```text
Question Staging Area
Pending Atomic Sections
Temporary Collections
Content Import Queue
```

它们属于二级工作流，应通过以下方式进入：

```text
Toolbar Entry
Drawer
Dialog
Secondary Panel
```

原因：

- SectionPage 的主任务是编辑教学结构。
- 暂存区和材料收集会分散结构编辑注意力。
- 用户应能在需要时打开辅助流程，用完后关闭。

## 6. 交互规则

### 6.1 选择节点

用户点击结构树或工作区中的节点时：

```text
更新当前 selectedNode
中间工作区滚动到对应节点
右侧 Inspector 显示节点详情
```

### 6.2 插入内容

插入内容使用明确入口：

```text
在选中节点前插入
在选中节点后插入
作为子项插入
追加到末尾
```

如果目标是 ContentBlock，打开 `ContentBlockPicker`。  
如果目标是 AtomicSection，打开 `AtomicSectionPicker` 或新建 Dialog。

### 6.3 调整顺序

第一版优先支持稳定操作：

```text
上移
下移
缩进
取消缩进
移除引用
```

拖拽排序可以后续再做，不作为第一版必要条件。

### 6.4 引用模式

ContentBlock 引用必须显示：

```text
FollowLatest
LockedVersion
```

当锁定版本时，必须显示版本号或版本 ID。  
AtomicSection 引用不应用 ContentBlock 版本锁定。

## 7. 页面状态建议

Page State：

```text
currentSection
sectionItems
sectionVariants
activeVariantId
selectedNodeId
selectedNodeType
expandedNodeIds
dirtyState
loadingState
errorState
```

Store State 只有在多个页面共享时才考虑，例如：

```text
currentTeachingTopicId
globalBankStatus
userUiPreferences
```

如需引入 store，必须先说明跨页面使用场景。

