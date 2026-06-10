# SectionPage Open Questions

## 1. 新前端工程目录如何落地？

当前结论：

- 不存在 `frontend-v2/`
- 当前只有 `src-v2/` 后端项目

需确认：

- 是否按 `docs/ui/ui-architecture.md` 新建独立 V2 前端工程？
- 目录名是否继续采用建议名 `frontend-v2/`？

## 2. ComponentLabPage 是否已存在？

当前结论：

- 不存在 `/lab`
- 不存在 `src/labs/`
- 不存在 `ComponentLabPage`

需确认：

- 是否把 `/lab` 作为 SectionPage 前置基础能力？

## 3. ContentBlockDisplay 与 ContentBlockCard 的命名边界是否要补入 component-rules？

当前结论：

- 文档存在边界差异
- 资源卡片和文档流展示组件语义不同

需确认：

- 后续是否更新 `docs/ui/component-rules.md`，明确：
  - `ContentBlockCard` 只用于资源库/选择器
  - `ContentBlockDisplay` 只用于文档流工作区

## 4. InsertPoint 应归类为 Presentation 还是 Business？

当前建议：

- `Presentation`

理由：

- 只表达插入位置
- 不理解业务
- 不调 API

需确认：

- 是否接受这一分类？

## 5. AtomicSectionBlock 是否允许创建空 AtomicSection？

当前冲突：

- `docs/ui/section-page.md` 暗示 Toolbar 可直接新建 AtomicSection
- `docs/ui/小节页面-需求文档.md` 更偏向第一版先支持“连续块升级”

需确认：

- 第一版是否允许直接创建空 AtomicSection？
- 还是只支持从连续块升级？

## 6. Root Promotion 规则由什么提供？

候选方案：

- 节点 metadata
- 回调 `canPromoteToRoot(node)`
- 业务配置表

当前建议：

- 业务树提供 `canPromoteToRoot(node)`

需确认：

- 是否采用 callback 方案作为主接口？

## 7. difficulty 当前是否已有后端字段？

当前结论：

- `Section`
- `SectionVariant`
- `ContentBlock`

都已有 `Difficulty`

缺口：

- `AtomicSection` 当前领域模型没有 `Difficulty`

需确认：

- SectionPage 第一版里的 AtomicSection difficulty 是：
  - 先不做
  - 前端 mock 预留
  - 还是要求先补后端模型

## 8. Word 编辑入口第一版只显示按钮，还是要接现有编辑链路？

当前结论：

- 前端不能再复刻 V1 编辑链路
- 必须等待或补齐 V2 API 对应能力

需确认：

- SectionPage 第一版：
  - 只显示按钮占位
  - 还是必须先补 V2 后端编辑会话 API

## 9. SectionPage 第一版是否真实写入？

当前建议：

- `mock first`
- `真实只读`
- `写入继续 mock 或占位`

原因：

- 写入相关 API 不完整
- 当前任务也明确禁止直接接真实写入 API

需确认：

- 后续正式实现第一轮是否接受“只读真实 + 写入 mock”？

## 10. 是否需要支持多个 Section 同时打开？

当前建议：

- UI 第一版只打开一个 Section
- 状态结构预留多 Section

需确认：

- 是否接受这种折中方案？

## 11. docs/ui/section-page.md 与 docs/ui/小节页面-需求文档.md 是否存在冲突？

当前结论：

- 存在冲突

主要包括：

- Toolbar 对 `SectionVariant` 的强调程度
- 空 AtomicSection 创建方式
- 插入能力第一版范围
- `ContentBlockCard` vs `ContentBlockDisplay` 命名边界

需确认：

- 这些冲突是否按准备文档中的建议处理？

## 12. SectionItem 第一版支持的 targetType 具体有哪些？

当前 V2 后端：

- `ContentBlock`
- `AtomicSection`

需求文档还提到：

- CompositeBlock / GroupBlock 作为工作区结构

需确认：

- 第一版 SectionItem.targetType 是否严格只保留：
  - `ContentBlock`
  - `AtomicSection`

如果是这样：

- CompositeBlock 只能作为 `ContentBlockType.ExampleGroup / ExerciseGroup / VariantGroup` 表达，而不是独立 targetType。

## 13. AtomicSection 内部是否允许 CompositeBlock / QuestionGroup？

当前需求倾向：

- 允许“其他允许的非 AtomicSection 块”

当前需确认：

- 第一版 AtomicSection 内是否允许引用：
  - `ExampleGroup`
  - `ExerciseGroup`
  - `VariantGroup`

## 14. QuestionGroup / ExampleGroup 是否只作为普通结构块，不允许 Root Promotion？

当前建议：

- 不允许 Root Promotion

依据：

- `docs/ui/小节页面-需求文档.md` 明确说 `QuestionGroup / ExampleGroup` 不应提升为根节点

需确认：

- 是否把这条写成 SectionPage 第一版固定规则？

## 15. SectionWorkspace 是否必须在第一版支持 Teaching Note Mode 双列？

当前文档差异：

- `docs/ui/section-page.md` 更强调三段式工作台
- `docs/ui/小节页面-需求文档.md` 里 Teaching Note Mode 是重要扩展方向，但当前任务是准备阶段

需确认：

- 第一版实现是否必须包含：
  - `MainContentColumn | TeachingNoteColumn`
  - 还是只做状态和布局预留

## 16. SectionStructurePanel 的三种模式第一版是否都要落地？

文档要求：

- `Hidden`
- `Docked`
- `Focused`

需确认：

- 第一版是否必须三种都实现？
- 还是先做 `Docked + Focused`，`Hidden` 作为简单折叠状态？

## 17. 真实读取接口是否直接使用当前 V2 后端现状？

当前结论：

- `/api/cms-v2` 已有读取基础
- 但缺少 SectionPage 聚合读取接口

需确认：

- 第一版是否接受前端自行聚合：
  - `sections`
  - `section items`
  - `content blocks`
  - `atomic sections`
  - `section variants`

还是需要后端先提供专用聚合接口？

## 18. 连续块升级 AtomicSection 是否必须有后端专用用例？

当前建议：

- 必须有后端专用用例

原因：

- 前端自行多步重排和重挂接风险太高

需确认：

- 后续实现阶段是否先补这个用例，再做前端交互？
