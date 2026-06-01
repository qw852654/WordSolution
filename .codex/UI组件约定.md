# UI 组件约定

本文件记录浏览器管理端已经抽象出的前端 UI 组件。后续开发小节编排、讲义编排、内容资源选择器、内容库等页面时，应优先复用这些组件的 DOM 结构、CSS 类名和交互语义。

## 1. 复用原则

- 已抽象的组件优先复用，不为同一类 UI 重新发明新的结构和样式。
- 新页面需要展示同一业务对象时，应先查本文件，再决定是否新增样式。
- 如果现有组件无法满足新场景，优先扩展组件变体，而不是复制一套相似 CSS。
- 组件 CSS 放在 `题库本地服务/wwwroot` 下的共享样式文件中；页面 CSS 只做页面布局和少量上下文适配。
- 组件 DOM 类名保持稳定，避免页面之间出现同一对象多套表达。
- 每次新增或抽象 UI 组件时，必须同步更新本文件，记录组件定位、使用场景、DOM 结构、状态类和禁止事项。

## 2. 内容块卡片

### 2.1 组件定位

内容块卡片用于展示 `ContentBlock`，包括内容资源选择器、内容库列表、组合块子块选择、讲义编排中插入单个内容块等场景。

当前共享样式文件：

```text
题库本地服务/wwwroot/content-block-card.css
```

当前已接入页面：

```text
题库本地服务/wwwroot/sections.html
题库本地服务/wwwroot/cms.html
```

当前已接入渲染函数：

```text
题库本地服务/wwwroot/sections.js
题库本地服务/wwwroot/cms.js
renderContentBlockCard(block, options)
```

后续若多个页面需要复用渲染函数，应再提取到共享 JS 文件；在提取前，各页面至少应复用同一套 `content-block-card` DOM 结构和 CSS 类名。

### 2.2 显示顺序

内容块卡片必须按以下顺序组织信息：

1. 第一行突出显示内容块名称。
2. 名称右侧显示彩色长方形内容类型，例如 `练习`、`例题`、`知识点`。
3. 内容类型右侧紧接彩色结构类型块，例如 `原子块`、`组合块`。
4. 如果存在备注或摘要，则在下一行显示；没有备注或摘要时不显示这一行。
5. 最后一行显示其他属性，例如状态、版本、版本状态、标签摘要。

### 2.3 DOM 结构

标准结构如下：

```html
<button class="content-block-card" type="button">
  <div class="content-block-card__top">
    <strong class="content-block-card__title">内容块名称</strong>
    <span class="content-block-card__right">
      <span class="content-block-card__type content-block-card__type--exercise">练习</span>
      <span class="content-block-card__structure content-block-card__structure--atomic">原子块</span>
      <span class="content-block-card__action">选择</span>
    </span>
  </div>
  <div class="content-block-card__remark">内容块备注或摘要</div>
  <div class="content-block-card__meta">
    <span class="content-block-card__property">
      <span class="content-block-card__property-label">状态</span>
      可复用
    </span>
  </div>
</button>
```

如果卡片只是展示，不承担点击动作，可以把外层元素换成 `div`，但必须保留 `content-block-card` 类名和内部结构。

### 2.4 状态类

可使用以下状态类：

```text
content-block-card is-selected
content-block-card is-disabled
```

- `is-selected` 表示当前内容块已被选中或待加入。
- `is-disabled` 表示不可选，例如已废弃、已在目标中、锁定版本但没有当前版本。

### 2.5 类型颜色类

内容类型颜色类命名：

```text
content-block-card__type--knowledge
content-block-card__type--example
content-block-card__type--exercise
content-block-card__type--method
content-block-card__type--mistake
content-block-card__type--note
content-block-card__type--question
content-block-card__type--group
content-block-card__type--default
```

结构类型颜色类命名：

```text
content-block-card__structure--atomic
content-block-card__structure--composite
```

新增内容类型时，优先补充这一组件的颜色类，而不是在页面局部写一次性颜色。

### 2.6 数据字段映射

标准字段优先使用：

```text
标题
类型
结构类型
状态
摘要
备注
标签摘要
当前版本ID
当前版本号
版本状态
```

如果后端返回字段在不同上下文中命名不同，前端适配函数可以兼容别名，但卡片最终展示语义保持一致。

### 2.7 禁止事项

- 不要再把内容块展示成单行表格。
- 不要在页面内复制一套与 `content-block-card` 近似但类名不同的卡片。
- 不要把类型、结构类型、状态混在同一个纯文本串里。
- 不要为了某个页面临时改变 `content-block-card` 的核心信息顺序。

## 3. 小节编排内容卡片

### 3.1 组件定位

`section-content-card` 用于小节编排页、讲义编排页等“结构编排工作区”中展示已经被引用到编排结构里的节点。它不是内容资源候选卡片，不替代 `content-block-card`；资源库、选择器、内容库列表仍优先使用 `content-block-card`。

当前接入位置：

```text
题库本地服务/wwwroot/sections.html
题库本地服务/wwwroot/sections.css
题库本地服务/wwwroot/sections.js
renderSectionContentCard(card, depth, hasChildren)
```

### 3.2 显示内容

小节编排内容卡片必须按以下顺序显示：

1. 卡片名称。
2. 右侧显示内容类型彩色长方形，例如 `知识点`、`例题组`、`练习`、`模型`。
3. 紧接着显示结构类型彩色方块，例如 `原子块`、`组合块`、`入口卡片`。
4. 正文区域显示该节点对应的 HTML 预览内容；静态原型中可用假数据模拟 docx 生成的 HTML。
5. 功能按钮固定在卡片右侧；移动端可折到卡片底部。

### 3.3 DOM 结构

标准结构如下：

```html
<article class="section-content-card is-selected" data-depth="0">
  <div class="section-content-card__fold">
    <button class="fold-button" type="button" aria-expanded="true"></button>
  </div>
  <div class="section-content-card__body" role="button" tabindex="0">
    <div class="section-content-card__header">
      <strong class="section-content-card__title">机械能守恒例题组</strong>
      <span class="section-content-card__badges">
        <span class="section-content-card__type section-content-card__type--group">例题组</span>
        <span class="section-content-card__structure section-content-card__structure--composite">组合块</span>
      </span>
    </div>
    <div class="section-content-card__html"></div>
  </div>
  <div class="section-content-card__actions"></div>
</article>
```

父级卡片和子级卡片应使用 `section-card-group` 包裹在同一个边框内。父级卡片左侧显示折叠按钮，子级区域使用 `section-card-group__children`。

### 3.4 插入区域

卡片之间的添加入口使用 `insert-zone`：

```html
<div class="insert-zone">
  <span class="insert-zone__line">插入到此处</span>
  <div class="insert-zone__menu">
    <button class="insert-zone__button" type="button">插入卡片</button>
    <button class="insert-zone__button" type="button">新建卡片</button>
  </div>
</div>
```

默认透明；鼠标悬停或键盘聚焦时变为彩色区域；悬停超过 1 秒显示操作浮层。浮层固定包含两个按钮：`插入卡片` 和 `新建卡片`。静态原型中只展示搜索框和候选项，不写入小节。

### 3.5 禁止事项

- 不要用 `content-block-card` 直接充当编排区结构节点。
- 不要在编排区复制一套临时卡片样式；应复用 `section-content-card`。
- 不要把父子卡片拆成互不相关的散列卡片；组合块、例题组等父子结构必须能看出同属于一个框。
- 不要在静态原型阶段接真实写入 API。

## 4. 教学主题导航树

### 4.1 组件定位

`teaching-topic-nav` / `teaching-tree` 用于展示 `TeachingTopic` 教学主题层级，职责是定位教学内容在知识结构中的位置。它不是小节结构树，也不是讲义结构树。

教学主题导航树只应承载这类节点：

```text
功能关系
机械能守恒
竖直圆轨道
杆模型
球模型
```

它不应直接混入小节方案、讲义方案、内容块版本、讲义版本或生成记录。

当前接入位置：

```text
题库本地服务/wwwroot/sections.html
题库本地服务/wwwroot/sections.css
题库本地服务/wwwroot/sections.js
renderTree()
renderTreeNode(node, level)
```

### 4.2 DOM 结构

标准结构如下：

```html
<aside class="teaching-topic-nav">
  <div class="panel-heading">
    <h2>教学主题导航</h2>
    <button class="nav-collapse-button" type="button"></button>
  </div>
  <nav class="teaching-tree">
    <div class="teaching-tree-item">
      <div class="teaching-tree-row" style="--tree-level: 0">
        <button class="tree-fold-button" type="button"></button>
        <button class="tree-node is-active" type="button">
          <span class="tree-dot"></span>
          <span class="tree-label">机械能守恒</span>
        </button>
      </div>
      <div class="teaching-tree-children"></div>
    </div>
  </nav>
</aside>
```

### 4.3 交互规则

- 父级节点必须有 `tree-fold-button`，用于展开 / 收起子级。
- 当前选中节点使用 `tree-node is-active`。
- 在教学主题工作台中，教学主题导航树是主要左侧导航。
- 在小节编辑器和讲义编辑器中，教学主题导航树默认折叠为窄栏或面包屑入口。
- 整个导航栏必须支持收缩成一个窄按钮，页面主体随之释放横向空间。
- 树节点缩进使用 `--tree-level` 控制，避免在 HTML 中写死多个缩进类。

### 4.4 禁止事项

- 不要把层级树做成平铺按钮列表。
- 不要只在文字前手写空格模拟层级。
- 不要把小节内部结构、讲义内部结构、版本记录或讲义生成记录挂进教学主题导航树。
- 不要在讲义编排页用 `teaching-topic-nav` 承载讲义结构；讲义结构应使用 `object-outline-tree`。

## 5. 当前对象结构树

### 5.1 组件定位

`object-outline-tree` 用于展示当前正在编辑的业务对象自身结构。它可以复用共享树的视觉和折叠交互，但业务语义与 `teaching-topic-nav` 不同。

使用场景：

- 小节编辑器：显示当前小节结构。
- 讲义编辑器：显示当前讲义结构。

职责：

- 快速预览当前对象整体结构。
- 点击节点后定位中间展开内容。
- 与中间内容区和右侧上下文详情面板联动。

### 5.2 小节结构树

小节编辑器中的 `object-outline-tree` 显示小节内部结构，例如：

```text
小节：机械能守恒 - 基础讲解版
- 知识点
- 例题组
  - 例题 1
  - 例题 2
- 练习
- 下级模型
```

点击节点时，中间内容区滚动到对应内容块，对应内容块高亮，右侧上下文面板显示该内容块详情、预览、Word 编辑入口、版本与引用关系。

### 5.3 讲义结构树

讲义编辑器中的 `object-outline-tree` 显示讲义内部结构，例如：

```text
讲义：机械能守恒专题讲义
- 导入
- 机械能守恒基础小节
  - 知识点
  - 例题组
  - 练习
- 竖直圆轨道模型
- 课堂练习
```

讲义结构树中展开小节时，展开出的知识点、例题、练习默认是引用展开视图，不是讲义真实拥有的子节点。

### 5.4 共享树约束

`shared-tree` 可以复用视觉样式、缩进、展开/收起、选中态、空状态和节点操作扩展位，但不能混淆业务语义。

在编辑器页面中，`object-outline-tree` 必须使用高密度大纲树表现，而不是卡片列表表现。推荐约束：

```text
普通节点行高：28px - 34px
左右内边距：6px - 10px
层级缩进：14px - 18px
展开按钮：16px - 20px
```

实现时可以继续复用 `ContentTree.render()` / `ContentTree.bind()`，但应通过 `.object-outline-tree .content-tree-*` 上下文样式压缩行高、缩进、按钮和标签尺寸。

禁止把 `object-outline-tree` 渲染成每个节点一个大圆角卡片。中间展开内容区才使用 `section-content-card` 卡片化展示。

禁止事项：

- 不要把教学主题树、小节结构树和讲义结构树都叫成同一个“左侧树”。
- 不要把教学主题节点渲染进 `object-outline-tree`。
- 不要把小节项、讲义项、版本记录渲染进 `teaching-topic-nav`。

## 6. 上下文面板

`context-panel` 只用于编辑器模式，不用于教学主题工作台。

使用规则：

- 小节编辑器右侧上下文面板显示当前选中内容块详情、预览、Word 编辑入口、版本和引用关系。
- 讲义编辑器右侧上下文面板显示当前选中节点来源、引用方式、版本状态、预览和 Word 编辑入口。
- 教学主题工作台无右侧常驻上下文面板。

## 7. 主题工作台

`topic-workspace` 用于教学主题工作台。它的核心是两个水平排列的主卡片区：

- `section-workspace-card`：显示当前教学主题下的小节方案，提供进入小节编辑器和新建小节方案入口。
- `handout-workspace-card`：显示当前教学主题下的关联讲义，提供进入讲义编辑器和新建讲义入口。

主题工作台的次级信息可以包括内容资源摘要、引用关系摘要、最近编辑记录、旧版本引用提醒，但这些信息不应抢占小节/讲义两个主卡片区，也不应做成右侧常驻详情面板。

## 8. 题库实例切换器

`question-bank-switcher` 用于浏览器管理端切换当前题库实例。它是跨页面共享组件，不属于某一个业务页面。

当前共享文件：

```text
题库本地服务/wwwroot/question-bank-context.css
题库本地服务/wwwroot/question-bank-context.js
```

职责：

- 调用 `GET /api/题库实例` 读取题库实例列表。
- 默认使用 `TEST` 题库。
- 将用户当前选择保存到 `localStorage` 的 `wordSolution.currentQuestionBankKey`。
- 暴露 `QuestionBankContext.getCurrentQuestionBankKey()` 和 `QuestionBankContext.apiBase()`，页面 API 路径必须从这里读取当前题库键。
- 提供最小新建题库入口，调用 `POST /api/题库实例`。
- 切换题库后通知当前页面刷新数据。

DOM 挂载点：

```html
<div data-question-bank-switcher></div>
```

当前题库文本可以通过以下占位同步：

```html
<p data-current-question-bank-label>当前题库：加载中</p>
```

禁止事项：

- 不要在 `cms.js`、`sections.js`、`handouts.js`、`references.js` 等页面脚本中直接写死题库键。
- 不要在各页面重复实现一套题库列表读取、localStorage 保存和创建题库逻辑。
- 不要把题库实例切换器和教学主题导航树混用；题库实例是数据源上下文，教学主题树是业务定位结构。

## 9. 小节文档流编辑工作台组件

小节编排页进入真实编辑器后，优先使用这一组轻量组件构建 UI。它们对应当前真实小节编辑器的页面结构，页面脚本负责 API 和状态，组件负责渲染和交互语义。

当前接入位置：

```text
题库本地服务/wwwroot/sections.html
题库本地服务/wwwroot/sections.css
题库本地服务/wwwroot/sections.js
```

### 9.1 workspace-shell

`section-workspace-shell` 是小节编辑器外壳：

```text
顶部：固定工具栏
左侧：object-outline-tree 小节结构树
中间：document-flow 小节展开内容
右侧：inspector-panel 选中块详情
左侧边缘：topic-drawer-tab 教学主题抽屉入口
```

约束：

- `body` 不是主滚动容器。
- 顶部工具栏固定在页面外壳顶部。
- 左侧结构树、中间文档流、右侧检查器分别独立滚动。
- 教学主题入口不放在顶部工具栏中。

### 9.2 topic-drawer

`topic-drawer` 是教学主题导航树在编辑器页面中的悬浮抽屉形态。

约束：

- 关闭状态只显示一个左侧小标签 `topic-drawer-tab`。
- 打开时以浮层覆盖当前工作区，不挤占左侧结构树、中间文档流或右侧检查器。
- 点击“展开教学主题树”只展开抽屉，不重置当前小节编辑器。
- 点击具体教学主题节点后，才允许进入对应主题工作台。

### 9.3 document-flow

`document-flow` 是小节正文展开区。它显示 `Section -> SectionItem -> ContentBlock` 的展开结果，不显示伪造的小节结构。

节点组件为 `content-node`：

```html
<article class="content-node">
  <div class="node-fold-cell"></div>
  <div class="node-body">
    <div class="node-header"></div>
    <div class="node-preview"></div>
    <div class="node-quick-meta"></div>
  </div>
  <div class="node-actions"></div>
</article>
```

约束：

- 中间区域以正文预览为主，只显示必要的标题、角色、难度、题型、版本等轻量信息。
- 详细字段放到右侧 `inspector-panel`。
- 组合块使用 `content-node-group` 包裹父子节点，父子结构必须在一个整体框内。
- 操作按钮默认弱化，悬停或选中时再明显显示。
- 折叠状态由同一份 `collapsedBlockIds` 同步给左侧结构树和中间文档流。

### 9.4 inline-insert-handle

`insert-handle` 是卡片之间和组合块子块之间的插入入口。

约束：

- 默认只是一条透明间隔，不抢占正文视觉。
- 鼠标停留约 1 秒后才变为彩色长条并显示操作。
- 操作固定为 `插入卡片` 和 `新建卡片`。
- `插入卡片` 表示选择已有内容块并建立引用。
- `新建卡片` 表示创建新的 `ContentBlock` 后插入当前位置。

### 9.5 inspector-panel

`inspector-panel` 是右侧选中块详情面板。

职责：

- 显示选中内容块的显示名、类型、结构、角色、难度、用途、题型、默认选入、版本和备注。
- 显示当前节点来源是小节引用还是组合块子项引用。
- 提供 Word 编辑、插入子块、新建子块、打开内容库、移除引用等操作。

禁止把“小节级导出 Word”放到 `inspector-panel` 中；它属于顶部小节级操作。
