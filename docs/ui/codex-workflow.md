# CMS V2 前端 Codex 开发流程

本文档约束后续使用 Codex 开发 V2 前端时的工作方式。

当前前提：

- V1 前端已经废弃。
- V1 后端也已经废弃。
- 新前端只能对接 CMS V2 后端。
- 旧静态页面和旧接口只能作为历史参考，不能继续作为实现底座。

当前阶段 1 只允许做文档清理，不创建前端工程，不开始实现页面或组件。

## 1. 开发前必读

所有任务必须读取：

```text
AGENTS.md
CONTRIBUTING.md
.codex/内容管理系统详细架构.md
.codex/内容管理系统升级路线.md
```

涉及 UI / 前端 / 页面 / 交互 / 布局 / 样式时额外读取：

```text
docs/ui/ui-architecture.md
docs/ui/component-rules.md
docs/ui/section-page.md
docs/ui/focus-tree.md
docs/ui/i18n.md
docs/ui/codex-workflow.md
```

涉及后端数据模型时读取：

```text
docs/cms-v2/backend/后端数据模型开发文档.md
docs/cms-v2/backend/领域模型结构说明.md
docs/cms-v2/backend/后端数据模型进度.md
docs/cms-v2/backend/后端重建阶段计划.md
```

## 2. 文档优先

当前 V2 前端重写采用文档优先：

```text
先确认页面目标
先确认组件边界
先确认 DTO 和 mock 数据
再实现组件
最后接 API
```

不要一上来直接写复杂页面。

## 2.1 每轮最小开发规则

后续 UI 开发按最小轮次推进：

- 每一轮只完成用户已确认计划中的最小开发目标。
- 不顺手开发后续组件、后续页面、后续交互或真实 API 接入。
- 不因为发现相邻问题就直接扩展修改范围；相邻问题先记录到汇报或下一轮计划。
- 不做计划外视觉精修、结构重排、抽象升级或重构。
- 如果本轮需要新增组件，Codex 必须先简要说明组件职责、非职责、输入数据、事件边界和 ComponentLab 验收场景，待用户确认后再开始实现。
- 每轮完成后，必须说明本轮开发了什么、访问哪个地址验收、重点检查哪些区域、哪些仍是占位。

## 3. Mock Data First

组件开发流程：

```text
Define DTO
↓
Create Mock Data
↓
Build Component
↓
Verify in ComponentLabPage
↓
Connect API
```

任何可复用业务组件都应先有 mock 场景。

这里的 mock 仅用于 V2 前端开发与组件验证，不得回退到 V1 页面里做拼接式开发。

## 3.1 ComponentLab 独立验收规则

ComponentLab 是当前开发轮次的独立验收页面。

规则：

- `/lab` 必须作为独立验收路由使用，不应包在带主导航或左侧导航的 AppShell 中。
- 用户验收组件或页面原型时，应直接访问 `/lab`，而不是通过主应用导航窗口查看。
- 每轮只把本轮需要验收的组件、页面或完整页面原型放入 ComponentLab。
- 页面级开发也可以在 ComponentLab 中放入完整页面进行 mock 验收。
- 上一轮无关的组件和场景应从当前 ComponentLab 视图中移除。
- ComponentLab 不承担业务导航、真实数据工作台或永久组件展览馆职责。

## 3.2 视觉实现约束

在用户详细描述视觉样式修改之前，允许 UI 视觉不精修，但禁止随意发挥样式。

必须遵守：

- 不在组件中随意写一次性颜色值。
- 不使用大面积阴影、渐变、装饰性背景。
- 不为相似组件重复写多套样式。
- 优先使用 shadcn-vue、Tailwind spacing、border、text、background token。
- 布局结构、组件层级、状态类必须稳定。
- 如果视觉细节不确定，先使用最简样式，不要自行发挥。
- 所有可复用组件必须先在 ComponentLab 中用 mock 数据验收。

## 4. 状态决策流程

新增状态前先判断：

```text
只给一个组件用？
  放组件内部。

一个页面内多个组件用？
  放页面状态或页面 composable。

多个页面共享？
  说明共享页面和原因，再放 Pinia store。
```

不要默认把状态放进 Pinia。

## 5. API 接入流程

API 接入顺序：

```text
阅读 docs/cms-v2/backend/后端数据模型开发文档.md 中 API 端点
定义前端 DTO
在 src/apis 中封装请求
在 composable 或 page 中调用
用 loading / empty / error 状态覆盖 UI
```

禁止：

- 在组件里散落硬编码 URL。
- 为旧前端接口做兼容层。
- 从前端读取旧 `question-bank.db` 相关概念。
- 对接 `/api/题库实例/...`。
- 在 `题库本地服务/wwwroot` 上继续堆叠 V2 页面逻辑。

## 6. UI 验证

每个页面完成前至少检查：

- 1440px 宽屏布局。
- 1024px 中等宽度布局。
- 768px 窄屏布局。
- 375px 移动宽度退化。
- 文本不溢出按钮和面板。
- 左右侧栏折叠后主区域可用。
- 键盘可操作主要交互。
- 空状态和错误状态可见。

## 7. 不做事项

V2 前端第一阶段不做：

- 兼容 V1 静态页面。
- 复用 V1 CSS 组件约定。
- 继续扩写 V1 前端文档作为当前要求。
- Word 深度加载项联动。
- 多人协作和权限。
- PDF 输出 UI。
- 变量替换和条件内容复杂编辑器。

这些能力后续按阶段补充。

