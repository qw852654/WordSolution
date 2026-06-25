# CMS V2 SectionPage 上线前细节优化开发文档

状态：待用户确认  
创建日期：2026-06-25  
适用范围：`SectionPage`、`AtomicSection`、`AtomicSectionPanel`、`InsertPoint`、`SectionInspector` 的上线前闭环优化。

## 1. 背景

当前 `SectionPage` 已具备 Section 编排、AtomicSectionPanel、标签系统、教学评注系统、多题导入和基础工作台能力。上线前仍有几处体验和业务闭环需要收口：

- `AtomicSection` 内部的新建 panel 入口常驻显示，干扰主工作区阅读。
- `InsertPoint` 已经承担“这里可以插入”的交互语义，但还没有统一承载 AS 内部 panel 创建。
- 新建 `AtomicSection` 后仍然需要用户手动补齐知识点、例题、变式 panel，启动成本偏高。
- `AtomicSection.Status` 目前不是清晰的用户可调状态。
- 需要为“内部有空 panel 的 AS”建立系统标记，为后续集中筛选未完善 AS 做准备。
- 右侧 `SectionInspector` 信息密度偏低，选中 `ContentBlock` 时顶部概览区域容易被压缩到不可用。

本轮只做上线前细节优化，不重写 `SectionPage` 信息架构，不迁移 V1，不触碰 `VSTO`、`Word本地文件操作核心库`、`题库本地服务/wwwroot`。

## 2. 已确认产品口径

### 2.1 InsertPoint 作为上下文操作槽

`InsertPoint` 后续不只表达 Section 顶层“插入 ContentBlock / AtomicSection / 已有块”，而是一个可复用的上下文操作槽。

不同上下文可以提供不同按钮：

- Section 顶层：新建 ContentBlock、新建 AtomicSection、插入已有块。
- CompositeBlock 内部：新建 ContentBlock、插入已有块。
- AtomicSection 未归组区 / panel 内部：新建 ContentBlock、插入已有块。
- AtomicSection panel 列表位置：新建 AtomicSectionPanel。

本轮新增能力聚焦于 `AtomicSection` 内部 panel 列表位置：

- 在第一个 panel 前显示 InsertPoint。
- 在 panel 之间显示 InsertPoint。
- 在最后一个 panel 后显示 InsertPoint。
- 没有 panel 时显示一个空态 InsertPoint。
- 这些位置只提供“新建 panel”类操作，不显示不适用于该位置的 ContentBlock 操作。

`AtomicSection` header 中不再常驻展示“新建 panel”按钮。为了降低入口丢失风险，header 的更多菜单可以保留一个“新建 panel”备用入口，但主路径应是 InsertPoint。

### 2.2 新建 AtomicSection 自动创建三个默认 panel

新建 `AtomicSection` 时，后端必须在同一个事务内自动创建三个空 `AtomicSectionPanel`：

| 顺序 | TeachingRole | UI 语义 | 默认 Title | 默认 Difficulty |
| --- | --- | --- | --- | --- |
| 1 | `Knowledge` | 知识点 | 继承 AS 标题 | 继承 AS 难度 |
| 2 | `Example` | 例题 | 继承 AS 标题 | 继承 AS 难度 |
| 3 | `Variant` | 变式 | 继承 AS 标题 | 继承 AS 难度 |

说明：

- 该规则适用于所有新建 AS 入口，包括 Section 顶层 InsertPoint、SectionTree 右键、普通新建 AS API，以及多个 SectionItem 批量升级为 AS 的后端用例。
- 该规则不由前端连续调用三次 CreatePanel API 实现。
- 任一步失败时必须整体回滚，不留下只有 AS 本体或只创建部分 panel 的半成品。
- 本轮不回填历史 AS。已有 AS 如果没有 panel，仍按现有空态显示。
- 批量升级为 AS 时，原 SectionItem 转成的 AtomicSectionItem 继续遵守现有 role / difficulty / unassigned 规则；默认 panel 的创建不代表强行把既有 item 塞入某个 panel，也不创建默认 ContentBlock。
- 旧文档中“创建 AtomicSection 时只创建自身，不自动创建 panel”的口径，在本专项确认后应被此新规则覆盖。

### 2.3 AtomicSection 状态改为用户可调

`AtomicSection.Status` 继续使用现有枚举：

```text
Draft
Active
Archived
```

本轮不新增状态枚举，不把“待完善”混入 `AtomicSection.Status`。`Status` 表达用户主观维护状态，可在 `SectionInspector` 中编辑。

需要遵守 server-confirmed update：

```text
用户修改状态
-> 前端调用 CMS V2 API
-> 后端确认并返回或触发重新读取
-> 前端刷新当前 Section 数据
```

前端不得 optimistic update。

### 2.4 待完善标记由系统推导

`AtomicSection` 的“待完善”不等同于 `Status`，而是系统根据内部 panel 是否为空推导出来。

第一版规则：

```text
hasEmptyPanel = 当前 AS 内任意 AtomicSectionPanel 的 AtomicSectionItem 数量为 0
```

含义：

- 空 panel 表示该 AS 仍有未补齐的教学职责。
- 只要存在一个空 panel，AS 显示“待完善”。
- 所有 panel 都至少包含一个 item 时，AS 显示“已完善”。
- 未归组区是否有内容不影响 `hasEmptyPanel`。
- 没有 panel 的旧 AS，第一版按 `hasEmptyPanel = false` 处理；UI 只对有 panel 且存在空 panel 的 AS 显示“待完善”，避免历史空 AS 被误判。

本轮只做标记与展示，不做集中筛选页面，不做全局统计，不做批量处理。

展示规则：

- `AtomicSectionBlock` 标题行：`hasEmptyPanel = true` 时显示轻量“待完善”标签。
- `SectionInspector` 选中 AS 时：显示“完善状态：待完善 / 已完善”。
- `SectionTree` 第一版不显示“待完善”，避免结构树噪声过高。

### 2.5 Inspector 第一版紧凑化

本轮不把标签和教学评注从 Inspector 中移出。后续可以单独优化标签、评注入口，但第一版上线前只做紧凑化和稳定性修正。

目标：

- 选中对象概览区域必须始终可见，不能被下方内容挤压到不可读。
- 减少大卡片、大 padding、大按钮和长提示占用。
- 将对象元信息、标签、教学评注、难度 / 状态 / 分类编辑整理成更紧凑的垂直结构。
- 只在适用对象上显示适用字段，避免选中 `ContentBlock` 时仍显示 AS 专属分类控件。
- 保留现有标签和教学评注功能，不改变它们的绑定规则。

建议布局：

```text
SectionInspector
  CompactSelectedSummary
    对象类型 / 标题 / 关键状态
    完善状态（仅 AS）
    小型操作入口

  CompactPropertyGroups
    状态 / 难度 / AS 分类等强相关字段

  TagsSection
    保留第一版标签编辑，压缩布局

  TeachingNotesSection
    保留第一版评注列表、筛选、编辑、删除，压缩标题和间距
```

禁止：

- 不新增大型 tab 体系。
- 不做第二套标签/评注侧栏。
- 不把解释性长文案作为主要视觉内容。
- 不在组件中写死颜色值。
- 不破坏已确认的 Tag / TeachingNote 绑定边界。

## 3. 非目标

本轮不做：

- 不做全局“待完善 AS”筛选页。
- 不做全局标签管理页。
- 不做全局教学评注搜索页。
- 不移动标签和教学评注到独立面板。
- 不新增 `AtomicSectionStatus` 枚举值。
- 不重写 `SectionPage` 三栏布局。
- 不重写 `AtomicSectionPanel` 后端模型。
- 不修改 V1、VSTO、`Word本地文件操作核心库`、`题库本地服务/wwwroot`。
- 不进行 git stage、commit、push、reset、checkout。

## 4. 代码落点建议

实际实现前必须再次核对当前文件。当前预期涉及以下文件或邻近文件：

### 4.1 后端

- `src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionCommands.cs`
  - 如需要补充 `UpdateAtomicSectionStatusCommand` 或等价命令，应放在这里或现有命令文件中。

- `src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionUseCases.cs`
  - 在 `CreateAtomicSectionAsync` 中事务性创建 AS 与三个默认 panel。
  - 增加或复用状态更新用例。
  - 计算默认 panel `SortOrder`，建议为 `0, 10, 20` 或沿用现有 panel 插入排序规则。

- `src-v2/WordSolution.CmsV2.Api/CmsV2ApiRequests.cs`
  - 如需要新增 AS 状态更新请求 DTO，应放在这里。

- `src-v2/WordSolution.CmsV2.Api/CmsV2ApiEndpointExtensions.cs`
  - 如缺少 AS 状态更新端点，新增最小端点。
  - 现有 `/atomic-sections/{id}/title`、`/difficulty` 可作为风格参考。

- `src-v2/WordSolution.CmsV2.Tests/Application/CmsV2ApplicationUseCaseTests.cs`
  - 更新“创建 AS 不创建默认 child blocks”的测试，改为验证默认三个 panel。
  - 保留“不自动创建默认 ContentBlock”的断言。

- `src-v2/WordSolution.CmsV2.Tests/Application/CmsV2AtomicSectionPanelUseCaseTests.cs`
  - 补充默认 panel 创建、顺序、角色、难度继承和事务回滚相关测试。

- `src-v2/WordSolution.CmsV2.Tests/Api/CmsV2ApiIntegrationTests.cs`
  - 补充 API 创建 AS 后可读取默认 panels。
  - 如新增 AS 状态更新 API，补充集成测试。

### 4.2 前端

- `frontend-v2/src/types/index.ts`
  - 扩展 `InsertActionType`，加入 `CreateAtomicSectionPanel`。
  - 明确 `InsertPointPlacementModel` 的上下文语义，避免继续堆叠松散字段。
  - 为 `AtomicSectionModel` 或相关 VM 补充 `hasEmptyPanel`，如果后端暂未直接返回，则由页面映射层推导。

- `frontend-v2/src/apis/cmsV2Client.ts`
  - 如新增 AS 状态更新 API，补充 client 方法与请求类型。
  - 如果后端 DTO 直接返回 `hasEmptyPanel`，同步 DTO 类型。

- `frontend-v2/src/components/presentation/InsertPoint.vue`
  - 让按钮由 `point.allowedActions` 或等价上下文驱动。
  - 支持 `CreateAtomicSectionPanel` 的按钮文案和图标。
  - 保持 presentation 组件边界：只展示、只 emit、不调用 API。

- `frontend-v2/src/components/business/AtomicSectionBlock.vue`
  - 移除常驻“新建 panel”按钮。
  - 在 panel 列表位置渲染 panel-level InsertPoint。
  - header 更多菜单保留可选“新建 panel”备用入口。
  - 标题行显示轻量“待完善”标签。

- `frontend-v2/src/components/business/AtomicSectionPanelBlock.vue`
  - 保留 panel 内部 ContentBlock 插入点，不混用 panel-list InsertPoint。

- `frontend-v2/src/components/business/AtomicSectionUnassignedArea.vue`
  - 保留未归组内容插入点，不混用 panel-list InsertPoint。

- `frontend-v2/src/components/containers/SectionWorkspace.vue`
  - 透传 `CreateAtomicSectionPanel` 插入动作到页面级 action。
  - 继续保持组件只 emit，不直接调用 API。

- `frontend-v2/src/components/business/SectionInspector.vue`
  - 进行紧凑化布局。
  - 加入 AS 状态编辑。
  - 加入 AS 完善状态展示。
  - 隐藏不适用于当前对象的字段组。
  - 保留标签与教学评注功能。

- `frontend-v2/src/pages/SectionPage.vue`
  - 处理新 InsertPoint action。
  - 处理 AS 状态更新 action。
  - 刷新当前 Section 数据并保持 server-confirmed update。
  - 维护或消费 `hasEmptyPanel`。

- `frontend-v2/src/locales/zh-CN.ts`
- `frontend-v2/src/locales/en.ts`
  - 补充新增按钮、标签、错误、状态文案。

- `docs/ui/component-rules.md`
  - 最终实现后同步补充 InsertPoint 上下文操作槽、AS 待完善标记和 Inspector 紧凑化规则。

## 5. 数据与 API 设计

### 5.1 CreateAtomicSection 的新语义

`CreateAtomicSectionAsync` 成功后必须满足：

```text
AtomicSection 已创建
AtomicSectionPanel[0] = Knowledge, Title = AtomicSection.Title, Difficulty = AtomicSection.Difficulty
AtomicSectionPanel[1] = Example,   Title = AtomicSection.Title, Difficulty = AtomicSection.Difficulty
AtomicSectionPanel[2] = Variant,   Title = AtomicSection.Title, Difficulty = AtomicSection.Difficulty
AtomicSectionItem 数量不因默认 panel 创建而变化
ContentBlock 数量不因默认 panel 创建而变化
```

建议响应仍返回 `AtomicSection` 本体；页面创建成功后重新读取当前 Section / AS panels。若后端现有聚合返回已能包含 panels，则前端以重新读取为准。

### 5.2 AS 状态更新 API

若当前只有 title / difficulty 更新端点，则建议补一个最小状态更新端点：

```http
POST /api/cms-v2/atomic-sections/{id}/status
```

请求：

```json
{
  "status": "Draft"
}
```

规则：

- `id` 必须存在。
- UI 允许用户选择现有三个状态 `Draft / Active / Archived`；如果当前页面对 Archived 对象有隐藏或只读规则，执行计划必须列出刷新后的可见性影响。
- 成功后返回更新后的 `AtomicSection` 或由前端重新读取。

### 5.3 hasEmptyPanel 的来源

优先级建议：

1. 如果当前 Section 聚合已经加载 `AtomicSectionPanel.children`，前端可在 `useSectionPageData` 或页面映射层推导 `hasEmptyPanel`，不急于新增后端字段。
2. 如果后续需要全局筛选未完善 AS，应再由后端提供查询字段或专用筛选 API。

本轮为了上线速度，推荐使用前端 VM 推导：

```ts
hasEmptyPanel = panels.some((panel) => panel.children.length === 0)
```

注意：

- 推导只用于展示，不作为持久业务状态。
- 后续全局筛选能力不能依赖前端局部数据，应另做后端查询。

## 6. UI 规则

### 6.1 InsertPoint

`InsertPoint` 必须遵守：

- 只展示当前上下文允许的 action。
- 不直接判断目标是否存在，不调用 API。
- 不直接修改 Section 数据。
- 所有可见文本走 i18n。
- 图标优先使用 `lucide-vue-next`。
- hover / focus 后显示行为保持现有 InsertPoint 规则。

`CreateAtomicSectionPanel` 建议按钮文案：

```text
新建 panel
```

如果界面空间较窄，可以显示为：

```text
新建板块
```

实际文案以 `zh-CN.ts` 为准。

### 6.2 AtomicSectionBlock

AS 标题行建议保留：

- 标题。
- 类型 / 难度短摘要。
- `hasEmptyPanel` 时的“待完善”轻标签。
- 折叠按钮。
- 更多菜单。

不再常驻显示：

- 大号“新建 panel”按钮。
- 多个重复创建入口。

### 6.3 SectionInspector

紧凑化建议：

- 顶部概览固定为小高度，不用大卡片。
- 对象类型使用小型 badge，不要占据整行。
- 状态、难度、完善状态使用紧凑 field row。
- 标签区域减少说明文字，保留搜索和保存。
- 教学评注区域保留筛选，但缩小标题、间距和空状态占位。
- 长说明文案改为较小字号提示，或只在必要错误状态出现。

## 7. 测试与验证要求

### 7.1 后端自动化测试

必须覆盖：

- 创建 AS 自动创建三个默认 panel。
- 三个 panel 的 `TeachingRole` 顺序为 Knowledge / Example / Variant。
- 三个 panel 的 `Title` 继承 AS 标题。
- 三个 panel 的 `Difficulty` 继承 AS 难度。
- 创建 AS 不创建默认 `ContentBlock`。
- 创建失败整体回滚，不留下半成品 panel。
- AS 状态更新成功。
- AS 状态更新非法 id / 非法状态失败。

建议命令：

```powershell
dotnet test src-v2/WordSolution.CmsV2.sln
```

### 7.2 前端验证

必须覆盖：

- `npm run typecheck`
- `npm run build`
- SectionPage browser smoke：
  - AS 内部 panel 创建入口由 InsertPoint 触发。
  - AS header 不再常驻显示新建 panel 按钮。
  - 新建 AS 后显示三个空 panel。
  - 空 panel 的 AS 显示“待完善”。
  - 给每个默认 panel 加入至少一个 item 后，待完善标记消失。
  - Inspector 选中 `ContentBlock` 时顶部概览可见。
  - Inspector 选中 AS 时可修改状态，并在服务端确认后刷新。
  - 标签和教学评注仍可正常展示和操作。

建议命令：

```powershell
Set-Location frontend-v2
npm run typecheck
npm run build
```

### 7.3 文档验证

实现完成后必须同步检查：

- `docs/ui/component-rules.md`
- `docs/ui/section-page.md`
- `docs/cms-v2/backend/后端数据模型开发文档.md`
- `docs/cms-v2/backend/领域模型结构说明.md`

重点是将“创建 AS 只创建自身”的旧口径更新为：

```text
创建 AS 时自动创建 Knowledge / Example / Variant 三个默认 AtomicSectionPanel，但不创建默认 ContentBlock。
```

## 8. 人工验收清单

用户最终验收时建议检查：

- 在 Section 顶层新建 AS 后，AS 内自动出现知识点、例题、变式三个空 panel。
- 三个 panel 的标题和难度与 AS 一致。
- AS 内部 panel 插入入口与现有 InsertPoint 行为一致，hover / focus 后可用。
- AS header 不再长期占用一个新建 panel 按钮。
- 在 panel 前、panel 间、panel 后创建 panel 时位置正确。
- 空 panel 存在时 AS 显示“待完善”。
- 所有 panel 都有内容后，AS 显示“已完善”或不再显示“待完善”。
- Inspector 选中 ContentBlock 时顶部概览清楚可见。
- Inspector 选中 AS 时可以修改 status。
- Inspector 选中 AS 时可以看到完善状态。
- 标签保存仍然作用于 `ContentBlock / AtomicSection / Section`。
- 教学评注仍然按原规则绑定六类对象。
- 多题导入和 Word 输出不受本轮 UI 优化影响。

## 9. 推荐实施阶段

确认本文档后，再编写具体修改计划。推荐分为四轮，避免单轮过大：

1. 后端闭环：默认 panel、AS status API、后端测试。
2. InsertPoint 抽象：新增 `CreateAtomicSectionPanel` action，AS panel list 接入。
3. Inspector 紧凑化与 AS 状态 / 待完善展示。
4. 测试、文档同步和浏览器验收。

每轮执行都应在独立执行线程中完成；总控线程只负责对齐、读取结果、判断和调度。
