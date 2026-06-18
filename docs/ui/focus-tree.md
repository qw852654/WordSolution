# FocusTree 与业务树规则

本文档定义 V2 前端树结构的抽象边界。树组件的核心目标是注意力管理，而不是把所有层级对象混成一个大树。

## 1. 基本原则

业务树使用 `FocusTree` 能力，但业务树不继承 `FocusTree` 的业务含义。

```text
FocusTree
  提供通用注意力管理能力。

Business Tree
  提供具体业务语义、节点渲染和节点操作。
```

禁止：

```text
把 TeachingTopic、Section、Handout、ContentBlockVersion、GeneratedFile 全部混入同一棵树。
```

## 2. FocusTree 职责

`FocusTree` 提供：

```text
Focus Workspace
Breadcrumb
Back Navigation
Root Navigation
Attention Management
Expand / Collapse
Keyboard Navigation
```

Focus Workspace 表示用户可以临时聚焦一条分支，隐藏无关内容。

典型能力：

- 聚焦某个节点。
- 返回上一级焦点。
- 回到根节点。
- 面包屑显示当前焦点路径。
- 仅展示当前焦点分支的相关节点。

Display Root【显示根节点】是 FocusTree 的通用注意力管理能力。

规则：

- Display Root 只改变前端树的显示范围，不改变业务树真实结构。
- `displayRootNodeId = null` 表示显示完整业务树。
- `displayRootNodeId` 有值时，业务树只显示该节点及其后代。
- `displayRootPath` 用于面包屑、返回上一级和返回完整根。
- FocusTree 只维护机制状态，不判断哪些业务节点允许设为显示根。
- 是否允许设为显示根必须由业务树提供，例如 `canSetDisplayRoot(node)`。

## 3. 业务树职责

业务树提供：

```text
Business Meaning
Node Rendering
Business Actions
Data Loading
Permission / State Display
```

业务树示例：

```text
TeachingStructureTree
SectionTree
ContentBlockTree
HandoutTree
```

### TeachingStructureTree

职责：

- 展示全库教学结构层级。
- 以 TeachingTopic 作为主节点。
- 表达 TeachingTopic 是否绑定 Section。
- 展开绑定 Section 的 TeachingTopic 后，可以显示只读 SectionVariant 列表。
- 定位知识结构位置，并提供进入 Section 的入口。
- 不展示小节内部结构。
- 不展示讲义内部结构。
- 不展示内容块版本和生成记录。

边界：

- 这棵树可以显示 TeachingTopic、绑定的 Section 状态和只读 SectionVariant 列表，但不能展示 SectionItem / ContentBlock / AtomicSection。
- 双击绑定 Section 的 TeachingTopic 节点打开 Section 本身。
- SectionVariant 第一版只读，不在这棵树内新增、重命名、删除或复制。
- 第一版不提供 Section 解绑能力。
- 删除只允许作用于没有子主题且没有绑定 Section 的空 TeachingTopic。

Display Root 规则：

- 有子 TeachingTopic 的 TeachingTopic 可以设为显示根。
- 已绑定 Section 的 TeachingTopic 可以设为显示根，即使它没有子 TeachingTopic、没有 SectionVariant。
- 空 TeachingTopic 不允许设为显示根。
- SectionVariant 不允许设为显示根。
- SectionVariant 不新增 TeachingStructureTree 专属节点类型，复用现有 SectionVariant 节点语义。

### SectionTree

职责：

- 展示当前 Section 的结构。
- 节点来自 SectionItem。
- 可显示 ContentBlock 和 AtomicSection。
- 支持选中、定位、上移、下移、移除引用。

### ContentBlockTree

职责：

- 展示组合内容块的子块结构。
- 节点来自 ContentBlockRelation。
- 支持组合块递归展开。
- 必须显示引用模式和锁定版本状态。

### HandoutTree

职责：

- 展示当前 HandoutVersion 的输出编排结构。
- 顶层节点来自 HandoutVersionItem。
- SectionVariant 节点可展开显示引用展开视图。
- 调整讲义结构不能反向修改源 Section。

## 4. useFocusTree

建议 composable：

```text
useFocusTree
```

输入：

```text
nodes
rootNodeId
initialFocusedNodeId
```

输出：

```text
focusedNodeId
focusedPath
visibleNodes
expandedNodeIds
focusNode(nodeId)
focusParent()
focusRoot()
toggleExpanded(nodeId)
isVisible(nodeId)
```

注意：

- `useFocusTree` 不理解业务对象。
- `useFocusTree` 不调用业务 API。
- 业务树负责把业务 DTO 转换为 FocusTree node。

## 5. 键盘和可访问性

树结构必须支持：

- 上下方向键移动焦点。
- 左方向键收起或回到父节点。
- 右方向键展开或进入子节点。
- Enter 选中节点。
- Escape 退出临时焦点或关闭上下文菜单。

ARIA 要求：

```text
role="tree"
role="treeitem"
aria-expanded
aria-selected
aria-level
```

动态状态必须与真实展开、选中状态同步。

