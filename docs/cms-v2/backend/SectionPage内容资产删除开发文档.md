# SectionPage 内容资产删除开发文档

## 1. 背景

当前 SectionPage / Section 配置页面中的删除操作更接近“删除引用”：从当前 Section 或 AtomicSection 的编排中移除一个 item，但不一定清理其背后的内容资产。

接下来一段时间，主要工作会集中在批量补充、筛选、调整题库内容。此阶段经常会出现“导入了一些质量较差的题目，想直接从系统资产中清理掉”的场景。因此需要提供一版面向 SectionPage 的内容资产删除能力。

本能力后续大概率会随内容治理、资产回收站、引用图、批量清理工具一起重构。第一版必须保持边界清晰、实现简单、行为可解释，不做泛化删除中心。

## 2. 第一版目标

第一版只解决一个核心问题：

- 在 SectionPage 的内容配置中，删除某个题目 / 内容块时，优先删除当前引用；如果该 ContentBlock 已不再被其他独立位置引用，则同步删除 ContentBlock 资产及其递归子资产。

第一版不解决 AtomicSection 资产删除问题：

- Section 中的 AtomicSection 暂时不提供“删除资产”能力。
- 不允许在 SectionPage 上直接删除 AtomicSection 资产。
- 如果后续需要删除 AtomicSection，必须单独设计“只移除当前 Section 引用 / 删除 AS 资产 / 迁移 AS 内容 / 清理 Variant”的完整业务规则。

## 3. 术语

- 引用删除：只删除当前编排关系，例如 `SectionItem` 或 `AtomicSectionItem`，不删除其指向的资产。
- 资产删除：删除 `ContentBlock`、`ContentBlockVersion`、内容块关系、版本文件等资产数据。
- 当前引用：用户本次点击删除的那个 `SectionItem` 或 `AtomicSectionItem`。
- 独立保护引用：当前引用之外，能够说明某个 ContentBlock 仍被系统其他业务使用的引用。
- 派生引用：由 Section 原始结构派生出来的引用，例如 `SectionVariantItem -> SectionItem`。它不独立保护 ContentBlock 资产。

## 4. 第一版范围

### 4.1 纳入范围

只处理以下两类入口：

1. 删除 Section 直属 ContentBlock
   - 当前项是 `SectionItem`
   - `SectionItem.TargetType = ContentBlock`
   - 删除当前 `SectionItem`
   - 删除引用该 `SectionItem` 的 `SectionVariantItem`
   - 如 ContentBlock 无独立保护引用，则删除 ContentBlock 资产

2. 删除 AtomicSection 内部 ContentBlock
   - 当前项是 `AtomicSectionItem`
   - `AtomicSectionItem.ContentBlockId` 指向 ContentBlock
   - 删除当前 `AtomicSectionItem`
   - 如 ContentBlock 无独立保护引用，则删除 ContentBlock 资产

### 4.2 明确不纳入范围

第一版不得实现以下能力：

- 不删除 AtomicSection 资产。
- 不删除 Section 资产。
- 不删除 TeachingTopic 资产。
- 不修改 HandoutPage 的删除逻辑。
- 不修改 OutputForm / GeneratedFile 删除逻辑。
- 不新增回收站。
- 不新增软删除字段。
- 不新增批量删除入口。
- 不新增“资产引用图”完整页面。
- 不做跨题库删除。
- 不自动清理 Word 模板、导出记录或历史生成文件。
- 不把现有全局 ContentBlock 删除接口改成 SectionPage 专用语义。
- 不改动 V1、VSTO、Word 本地文件操作核心库、题库本地服务/wwwroot。

## 5. 删除行为规则

### 5.1 总体行为

SectionPage 的内容块删除采用“两步语义”：

1. 必定删除当前引用。
2. 仅当资产没有独立保护引用时，才继续删除资产。

也就是说：

- 如果 ContentBlock 只被当前 item 使用：删除当前 item，并删除 ContentBlock 资产。
- 如果 ContentBlock 还被其他独立位置使用：删除当前 item，但保留 ContentBlock 资产。
- 前端必须向用户反馈本次是否真的删除了资产，不能只显示“删除成功”。

### 5.2 SectionVariantItem 的处理

`SectionVariantItem` 是 `SectionItem` 的派生编排，不作为 ContentBlock 的独立保护引用。

删除 Section 直属 ContentBlock 时：

- 删除当前 `SectionItem`。
- 同步删除所有指向该 `SectionItem.Id` 的 `SectionVariantItem`。
- 这些 `SectionVariantItem` 不阻止 ContentBlock 资产删除。

删除 AtomicSection 内部 ContentBlock 时：

- 不直接处理 `SectionVariantItem`。
- AtomicSection 是否出现在 Variant 中，不影响内部 ContentBlock 的资产删除判断。

### 5.3 HandoutVersionItem 的处理

`HandoutVersionItem` 如果直接引用某个 ContentBlock，视为独立保护引用。

规则：

- 删除 SectionPage 当前引用时，不删除直接被 HandoutVersionItem 引用的 ContentBlock 资产。
- 仍可删除当前 SectionItem / AtomicSectionItem。
- 返回结果中必须说明资产被 Handout 直接引用，因此已保留资产。

### 5.4 其他 Section / AtomicSection 引用

以下引用均视为独立保护引用：

- 其他 `SectionItem` 指向同一个 ContentBlock。
- 其他 `AtomicSectionItem` 指向同一个 ContentBlock。

注意：

- 当前正在删除的那个 `SectionItem` 或 `AtomicSectionItem` 不算保护引用。
- 如果同一 ContentBlock 同时出现在两个 AS 中，删除其中一个 AS 里的 item 时，只删除当前 item，保留 ContentBlock 资产。

### 5.5 ContentBlockRelation 递归删除

如果删除的 ContentBlock 有子 ContentBlock，需要递归处理。

规则：

- 删除根 ContentBlock 时，删除它向下的 `ContentBlockRelation`。
- 对每个子 ContentBlock：
  - 如果子 ContentBlock 没有独立保护引用，则继续删除子资产。
  - 如果子 ContentBlock 有独立保护引用，则保留子资产，只删除与被删父节点之间的关系。
- 递归删除必须避免重复访问和循环关系。
- 删除结果需要记录实际删除了多少个 ContentBlock、多少个关系。

### 5.6 文件资产删除

删除 ContentBlockVersion 时，需要尽量清理对应文件资产。

第一版只清理当前 V2 内容块版本可定位到的文件：

- docx 文件。
- html 预览文件。
- 如现有路径服务能够稳定定位 plain text 文件，则一并删除。

边界：

- 文件不存在时不报错。
- 文件删除失败时应记录失败并返回错误，不应留下“数据库已删、文件未删且用户不知道”的状态。
- 不清理模板文件。
- 不清理导出 Word 生成文件。

### 5.7 标签和教学评注

删除 ContentBlock 资产时，需要清理直接指向被删 ContentBlock 的关联数据。

第一版处理：

- 删除指向被删 ContentBlock 的标签绑定。
- 删除指向被删 ContentBlock 的教学评注绑定。
- 如果某条教学评注在清理后没有任何绑定，则删除该教学评注。

不处理：

- 不批量重写其他对象上的标签。
- 不迁移教学评注到其他对象。
- 不做标签词表清理。

### 5.8 编辑会话保护

如果待删除的 ContentBlock 或其递归子 ContentBlock 存在活跃编辑会话：

- 不删除资产。
- 当前引用也不删除。
- 返回可读错误，提示该内容块正在编辑中。

这样避免 Word 编辑中资产被删除。

## 6. API 设计

第一版新增 occurrence-aware 删除接口，不改变已有引用删除接口的语义。

### 6.1 删除 Section 直属内容块资产

```http
DELETE /api/cms-v2/sections/{sectionId}/items/{itemId}/content-asset
```

要求：

- `itemId` 必须属于 `sectionId`。
- `SectionItem.TargetType` 必须是 `ContentBlock`。
- 如果 `SectionItem` 指向 AtomicSection，返回 400，并提示第一版暂不支持删除 AtomicSection。

### 6.2 删除 AtomicSection 内部内容块资产

```http
DELETE /api/cms-v2/atomic-sections/{atomicSectionId}/items/{itemId}/content-asset
```

要求：

- `itemId` 必须属于 `atomicSectionId`。
- `AtomicSectionItem` 必须指向 ContentBlock。

### 6.3 响应 DTO

建议新增 `ContentAssetDeleteResult`。

```csharp
public sealed record ContentAssetDeleteResult(
    long RootContentBlockId,
    bool RemovedCurrentReference,
    bool DeletedRootAsset,
    int RemovedSectionItemCount,
    int RemovedSectionVariantItemCount,
    int RemovedAtomicSectionItemCount,
    int RemovedContentBlockRelationCount,
    int DeletedContentBlockCount,
    int DeletedContentBlockVersionCount,
    int DeletedFileCount,
    IReadOnlyList<ContentAssetRetainReasonDto> RetainReasons);
```

建议新增 `ContentAssetRetainReasonDto`。

```csharp
public sealed record ContentAssetRetainReasonDto(
    long ContentBlockId,
    string ReasonCode,
    string Message);
```

第一版 `ReasonCode` 至少覆盖：

- `ReferencedBySection`
- `ReferencedByAtomicSection`
- `ReferencedByHandout`
- `ReferencedByRelation`
- `ActiveEditSession`

### 6.4 错误处理

以下情况返回 400：

- item 不属于当前父对象。
- item 不是 ContentBlock。
- 尝试删除 AtomicSection 资产。
- ContentBlock 或其递归子节点存在活跃编辑会话。
- 文件删除失败。

以下情况返回 404：

- section / atomicSection / item 不存在。

## 7. 后端实现边界

### 7.1 用例位置

建议新增 Application 用例类：

```text
src-v2/WordSolution.CmsV2.Application/ContentBlocks/ContentAssetDeletionUseCases.cs
```

不要把 SectionPage 专用删除语义塞进现有全局删除方法里。

### 7.2 命令对象

建议新增两个命令：

```csharp
public sealed record DeleteSectionItemContentAssetCommand(
    string BankRootDirectory,
    long SectionId,
    long SectionItemId);

public sealed record DeleteAtomicSectionItemContentAssetCommand(
    string BankRootDirectory,
    long AtomicSectionId,
    long AtomicSectionItemId);
```

### 7.3 事务边界

每次删除必须在一个数据库事务内完成。

推荐顺序：

1. 读取当前 item 和根 ContentBlock。
2. 构建递归删除图。
3. 检查活跃编辑会话。
4. 删除当前引用。
5. 删除派生引用。
6. 判断哪些 ContentBlock 可删除、哪些需要保留。
7. 删除可删 ContentBlock 的关系、绑定、版本、资产文件、ContentBlock。
8. 保存并返回结果。

如果文件删除失败：

- 事务回滚。
- 返回错误。

### 7.4 不改变现有语义

不得修改以下既有行为：

- 现有引用删除接口仍然只做引用删除。
- HandoutPage 的删除逻辑保持不变。
- SectionVariant 的刷新、同步、进入页面自动修复逻辑保持不变。
- Word 导出容错逻辑保持不变。

## 8. 前端实现边界

### 8.1 SectionPage 行为

在 SectionPage 内容配置中：

- 对 ContentBlock item 的删除按钮改为调用新增 content-asset 删除接口。
- 删除成功后刷新当前 SectionPage 数据。
- 删除成功反馈必须区分：
  - 已删除当前引用并删除资产。
  - 已删除当前引用，但资产因其他引用保留。

### 8.2 AtomicSection 行为

第一版不允许删除 AtomicSection。

如果当前 UI 在 Section 树或 Section 配置里有 AtomicSection 删除按钮：

- 不调用资产删除接口。
- 可暂时禁用该按钮，或点击后显示“第一版暂不支持删除原子小节，请后续单独处理”。
- 不新增 AtomicSection 资产删除能力。

### 8.3 确认提示

因为这是破坏性操作，前端需要复用现有确认机制。

确认文案必须表达：

- 会从当前位置移除该内容。
- 如果没有其他独立引用，会同时删除内容块资产和版本文件。
- 如果仍被其他位置引用，资产会保留。

不得新增复杂问题抽屉、引用图弹窗或批量清理 UI。

### 8.4 前端 API 封装

在 `cmsV2Client` 中新增方法：

```ts
deleteSectionItemContentAsset(sectionId: number, itemId: number): Promise<ContentAssetDeleteResult>
deleteAtomicSectionItemContentAsset(atomicSectionId: number, itemId: number): Promise<ContentAssetDeleteResult>
```

前端 DTO 需要与后端响应保持一致。

## 9. 测试要求

### 9.1 Application 测试

至少覆盖：

1. 删除 Section 直属 ContentBlock，且无其他引用时，删除 SectionItem、SectionVariantItem、ContentBlock、Version、文件。
2. 删除 Section 直属 ContentBlock，但该 ContentBlock 被 HandoutVersionItem 直接引用时，只删除当前 SectionItem 和 SectionVariantItem，保留 ContentBlock。
3. 删除 AtomicSectionItem ContentBlock，且无其他引用时，删除 AtomicSectionItem 和 ContentBlock。
4. 删除 AtomicSectionItem ContentBlock，但该 ContentBlock 被其他 AtomicSectionItem 引用时，只删除当前 AtomicSectionItem，保留 ContentBlock。
5. 删除带 children 的 ContentBlock 时，递归删除无保护引用的 children。
6. children 被其他位置引用时，删除父子关系但保留 child ContentBlock。
7. 活跃编辑会话存在时，阻止删除，当前引用不变。
8. 尝试通过 SectionItem 删除 AtomicSection 时返回不支持。

### 9.2 API 测试

至少覆盖：

1. `DELETE /sections/{sectionId}/items/{itemId}/content-asset` 成功返回结果。
2. `DELETE /atomic-sections/{atomicSectionId}/items/{itemId}/content-asset` 成功返回结果。
3. item 不属于父对象返回 400 或 404。
4. AtomicSection 删除资产返回 400。

### 9.3 前端验证

至少执行：

```powershell
npm run typecheck
npm run build
```

如本地前后端可运行，需要浏览器 smoke：

- 打开 SectionPage。
- 删除一个普通题目 ContentBlock。
- 确认页面刷新后该题目消失。
- 如果该题无其他引用，ContentBlock 列表中也不再出现。
- 删除被其他位置引用的题目时，页面当前引用消失，但反馈提示资产保留。

## 10. 开发计划

### Phase 1：文档确认与后端用例骨架

范围：

- 阅读 AGENTS.md、CONTRIBUTING.md、`.codex/内容管理系统详细架构.md`、`.codex/内容管理系统升级路线.md`。
- 阅读后端文档：
  - `docs/cms-v2/backend/后端重建阶段计划.md`
  - `docs/cms-v2/backend/后端数据模型开发文档.md`
  - `docs/cms-v2/backend/领域模型结构说明.md`
  - 本文档
- 新增 Application 用例、命令、结果 DTO。
- 先写 Application 测试，覆盖 SectionItem / AtomicSectionItem 的基础成功路径和 AtomicSection 不支持路径。
- 实现最小可通过的删除当前引用 + 删除无保护 ContentBlock。

不得做：

- 不新增 API endpoint。
- 不做前端。
- 不做复杂递归 children。
- 不处理文件删除失败细节之外的 UI 文案。

验收：

- 新增 Application 测试通过。
- 既有相关 Application 测试不回归。

### Phase 2：递归删除、保护引用与文件资产清理

范围：

- 完成 ContentBlockRelation 递归删除图。
- 实现独立保护引用判断：
  - 其他 SectionItem
  - 其他 AtomicSectionItem
  - HandoutVersionItem
  - 外部 ContentBlockRelation
  - 活跃编辑会话
- 清理标签绑定、教学评注绑定。
- 删除可定位的 docx/html/plain text 文件。
- 补充对应 Application 测试。

不得做：

- 不新增 API endpoint。
- 不做前端。
- 不改变现有全局 ContentBlock 删除语义。

验收：

- 递归删除和保护引用测试通过。
- 文件删除失败能够回滚并返回错误。

### Phase 3：API 接入

范围：

- 新增两个 API：
  - `DELETE /api/cms-v2/sections/{sectionId}/items/{itemId}/content-asset`
  - `DELETE /api/cms-v2/atomic-sections/{atomicSectionId}/items/{itemId}/content-asset`
- 使用当前题库 RootDirectory。
- 添加 API 测试。

不得做：

- 不做前端。
- 不修改旧引用删除接口。
- 不新增 AtomicSection 资产删除接口。

验收：

- API 测试通过。
- 后端相关测试通过。

### Phase 4：SectionPage 前端入口与反馈

范围：

- 阅读 UI 文档：
  - `docs/ui/ui-architecture.md`
  - `docs/ui/component-rules.md`
  - `docs/ui/section-page.md`
  - `docs/ui/focus-tree.md`
  - `docs/ui/i18n.md`
  - `docs/ui/codex-workflow.md`
- `cmsV2Client` 增加两个删除方法和 DTO。
- SectionPage 中 ContentBlock 删除按钮调用新增接口。
- AtomicSection 删除资产入口禁用或提示不支持。
- 增加确认提示和成功反馈：
  - 已删除资产。
  - 已删除当前引用但资产保留。
- 更新 zh-CN/en 文案。

不得做：

- 不新增批量删除。
- 不新增引用图 UI。
- 不新增复杂问题抽屉。
- 不修改 HandoutPage 删除逻辑。

验收：

- `npm run typecheck` 通过。
- `npm run build` 通过。
- 浏览器 smoke 验证 SectionPage 删除题目后页面刷新正确。

### Phase 5：最终验证与收口

范围：

- 后端相关测试。
- 前端 typecheck/build。
- `git diff --check`。
- 对照本文档检查实现边界。
- 汇总最终修改文件、测试结果、剩余风险。

不得做：

- 不 stage。
- 不 commit。
- 不 push。
- 不改动 V1/VSTO/Word 本地文件操作核心库/题库本地服务/wwwroot。

最终人工验收清单：

- SectionPage 删除普通题目时，当前页面引用消失。
- 该题无其他独立引用时，ContentBlock 资产被删除。
- 该题被其他 Section/AS/Handout 直接引用时，只删除当前引用，资产保留，并有前端反馈。
- SectionVariantItem 不阻止资产删除。
- 删除 SectionItem 时，关联 SectionVariantItem 被清理。
- 递归 children 无独立引用时被删除。
- 递归 children 有独立引用时被保留。
- 活跃 Word 编辑会话中的内容块不能删除。
- AtomicSection 暂不允许删除资产。
- HandoutPage 删除逻辑不变。
- Word 导出容错逻辑不变。
