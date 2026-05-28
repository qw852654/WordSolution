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

`teaching-topic-nav` / `teaching-tree` 用于展示教学主题、小节、模型、讲义结构等层级导航。小节编排页和讲义编排页后续应复用同一套树结构与折叠交互，不要各自实现一套近似的左侧树。

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
- 整个导航栏必须支持收缩成一个窄按钮，页面主体随之释放横向空间。
- 树节点缩进使用 `--tree-level` 控制，避免在 HTML 中写死多个缩进类。
- 讲义编排页复用时，节点数据仍优先保持 `id`、`title`、`children`、`badge`、`meta`、`data` 这类通用字段。

### 4.4 禁止事项

- 不要把层级树做成平铺按钮列表。
- 不要只在文字前手写空格模拟层级。
- 不要在讲义编排页重新复制一套树样式；应扩展 `teaching-tree`。
