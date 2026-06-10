# CMS V2 前端架构文档

本文档定义 CMS V2 新前端的总体架构。旧 V1 UI 文档已经移除，后续前端以本目录文档为准。

## 1. 产品定位

CMS V2 前端不是普通题库 CRUD 后台，而是面向备课和讲义生产的教学结构设计工作台。

核心目标：

```text
Teaching Structure Design
```

不是：

```text
Question CRUD
```

用户主要组织的是：

```text
TeachingTopic 教学主题
ContentBlock 内容资产
AtomicSection 原子小节
Section 小节结构
SectionVariant 小节变体
HandoutVersion 讲义版本
OutputForm 输出形式
```

## 2. 前端技术栈

V2 前端固定采用：

```text
Vue 3
Vite
Tailwind CSS
shadcn-vue
Pinia
Vue Router
Vue I18n
```

约束：

- 不引入额外大型框架。
- 不复用 V1 静态页面结构作为新架构基础。
- 不兼容旧前端 API 形状。
- 所有业务请求对接 `/api/cms-v2`。
- 优先使用 shadcn-vue 组件表达按钮、输入框、弹窗、侧栏、表单、菜单、Tabs、Tooltip。
- 图标优先使用 lucide-vue-next。

## 3. 前端目录建议

建议新前端目录独立于旧前端：

```text
frontend-v2/
  index.html
  package.json
  vite.config.ts
  tailwind.config.ts
  src/
    app/
      router.ts
      i18n.ts
      pinia.ts
    pages/
    components/
      presentation/
      business/
      containers/
    stores/
    apis/
    composables/
    types/
    mocks/
    styles/
    labs/
```

目录职责：

```text
pages
  路由页面，负责页面级数据加载、页面级状态、页面布局和业务流程入口。

components/presentation
  纯展示组件，不理解 CMS 业务，不调用 API。

components/business
  业务展示组件，理解业务对象，可使用 composable，但默认不直接调用 API。

components/containers
  可复用业务容器组件，可加载数据，可调用 API 或 composable。

stores
  跨页面共享状态。只有多个页面真实共享时才允许进入 store。

apis
  封装 HTTP 请求。只表达 V2 API 端点和 DTO，不承载 UI 状态。

composables
  复用页面逻辑、查询逻辑、焦点树逻辑、选择逻辑和局部业务交互。

types
  前端 DTO、视图模型和枚举类型。

mocks
  组件开发和 ComponentLab 使用的代表性 mock 数据。

labs
  组件实验和 UI 验证页面。
```

## 4. 状态归属规则

状态应按使用范围放置：

```text
Component State
  只被单个组件使用。

Page State
  被一个页面内多个组件使用。

Store State
  被多个页面共享。
```

如果准备把状态放入 Pinia store，必须先说明：

```text
哪些页面会使用它？
为什么它必须跨页面共享？
```

没有明确跨页面共享需求时，不要默认进 store。

## 5. API 调用边界

API 调用规则：

```text
Presentation Components
  禁止调用 API。

Business Components
  优先通过 composables 获取数据或触发行为。

Business Container Components
  允许调用 API。

Pages
  允许调用 API。

Composables
  允许调用 API。

Stores
  只有跨页面共享状态需要时才允许调用 API。
```

API 封装统一放在 `src/apis/`。页面和 composable 不直接散落 `fetch` URL 字符串。

## 6. 路由建议

第一轮路由建议：

```text
/topics
  教学主题工作台。

/sections/:sectionId
  小节结构编辑页。

/handouts/:handoutVersionId
  讲义版本编排页。

/content-blocks
  内容资产库。

/content-blocks/:contentBlockId
  内容块详情。

/outputs/:outputFormId
  输出形式和生成记录。

/lab
  ComponentLabPage，用于组件开发和 mock 验证。
```

## 7. 视觉方向

CMS V2 是高频工作台，不做营销页和展示站。

设计原则：

- 信息密度适中，优先支持扫描、比较、编排和快速定位。
- 中央工作区始终是视觉重心。
- 左右侧栏用于导航、结构、检查器，不抢主工作区注意力。
- 使用稳定尺寸和明确布局，减少 hover 或动态内容造成的布局跳动。
- 按钮使用清晰图标和短文本，复杂操作进入菜单、Drawer 或 Dialog。
- 不使用装饰性大渐变、营销式 hero、夸张卡片堆叠。

推荐基调：

```text
专业、清晰、安静、有组织感。
主色可使用蓝色系表达结构和可靠性，辅色用于状态与操作区分。
避免整站只有单一蓝紫色调。
```

## 8. 可访问性要求

- 使用语义元素：`button`、`nav`、`main`、`aside`、`section`。
- 所有可点击元素必须可键盘访问。
- 动态展开状态必须绑定 `aria-expanded`。
- Dialog、Drawer、Dropdown 必须具备焦点管理。
- 所有 hover 行为需要有键盘等价操作。
- 遵守 `prefers-reduced-motion`。

