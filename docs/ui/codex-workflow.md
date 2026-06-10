# CMS V2 前端 Codex 开发流程

本文档约束后续使用 Codex 开发 V2 前端时的工作方式。

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
- Word 深度加载项联动。
- 多人协作和权限。
- PDF 输出 UI。
- 变量替换和条件内容复杂编辑器。

这些能力后续按阶段补充。

