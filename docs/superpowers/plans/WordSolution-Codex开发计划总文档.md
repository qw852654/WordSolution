# WordSolution Codex 开发计划总文档

> 本文档用于沉淀后续开发计划。后续不再为每个需求新建一次性计划文件；除非用户明确要求，新的实施计划应追加到本文档中，并在规格归属的模块文档中同步记录产品与技术规格。

## 使用规则

- 每个计划必须先有明确的规格落点；如果现有模块文档可以承接规格，优先补充到现有模块文档。
- 每个计划按阶段推进，每个阶段必须小而闭环，能够独立验证。
- 执行代码修改前必须确认当前分支，不得在 `master` 上直接开发。
- 默认不 stage、commit、push、reset、checkout；只有用户明确要求时才执行 Git 写操作。
- 不修改 V1、VSTO、`Word本地文件操作核心库`、`题库本地服务/wwwroot`。
- 总控线程只负责需求对齐、计划、调度、读取结果和必要的后端重启；实现阶段优先使用唯一执行线程串行推进，避免重复拉起并发线程。

---

## 2026-06-25 CMS V2 三题库上线配置

### 目标

让 CMS V2 支持三个固定题库：

- `TEST`：测试题库，用于开发验证与回归测试。
- `GZ`：高中题库，正式题库。
- `CZ`：初中题库，正式题库。

第一版采用“单后端实例绑定一个当前题库”的运行方式：启动后端时通过配置选择当前题库，所有业务 API 都落在当前题库目录与当前题库数据库上。前端只显示当前题库身份，不提供运行时切换入口。

### 规格来源

- 后端数据与运行时规格：`docs/cms-v2/backend/后端数据模型开发文档.md` 中的“三题库上线配置与运行时题库上下文”。
- 领域模型边界：`docs/cms-v2/backend/领域模型结构说明.md` 中的“题库上下文不是领域模型”。
- 协作与文档沉淀规则：`docs/superpowers/Codex协作与文档沉淀规则.md`。

### 已确认规格

题库目录固定为：

```text
E:\Desktop\题库中心\TEST\cms-v2
E:\Desktop\题库中心\GZ\cms-v2
E:\Desktop\题库中心\CZ\cms-v2
```

后端配置形态为：

```json
{
  "CmsV2": {
    "ActiveBankKey": "TEST",
    "Banks": [
      {
        "key": "TEST",
        "displayName": "测试题库",
        "kind": "Test",
        "rootDirectory": "E:\\Desktop\\题库中心\\TEST\\cms-v2"
      },
      {
        "key": "GZ",
        "displayName": "高中题库",
        "kind": "Production",
        "rootDirectory": "E:\\Desktop\\题库中心\\GZ\\cms-v2"
      },
      {
        "key": "CZ",
        "displayName": "初中题库",
        "kind": "Production",
        "rootDirectory": "E:\\Desktop\\题库中心\\CZ\\cms-v2"
      }
    ]
  }
}
```

配置规则：

- `ActiveBankKey` 必须匹配 `Banks[].key`，大小写不敏感，但运行时展示使用规范化后的原始 key。
- `Banks[].key` 不允许重复。
- `Banks[].displayName`、`Banks[].kind`、`Banks[].rootDirectory` 均不能为空。
- `kind` 第一版只允许 `Test` 与 `Production`。
- 如果配置了 `ActiveBankKey + Banks`，必须使用新配置。
- 如果没有配置 `Banks`，但存在旧的 `CmsV2:BankRootDirectory`，允许兼容启动，运行时视为 `LEGACY / 当前题库 / Test`。
- 正式上线配置不得依赖兼容模式。

不做范围：

- 不做运行时题库切换。
- 不在业务 API 路径或请求体中加入 `{bankKey}`。
- 不复用 V1 `/api/题库实例/{题库键}`。
- 不把题库注册表写入 CMS V2 数据库。
- 不新增 `QuestionBank`、`Bank`、`BankInstance`、`Tenant`、`Workspace` 等领域实体。
- 不给业务实体增加 `BankKey` 字段。
- 不做正式库危险操作保护。
- 不做跨题库复制、迁移、同步、对比。

### 文件范围

后端配置与当前题库上下文：

- 修改：`src-v2/WordSolution.CmsV2.Api/CmsV2ApiOptions.cs`
- 新增：`src-v2/WordSolution.CmsV2.Api/CmsV2BankOptions.cs`
- 新增：`src-v2/WordSolution.CmsV2.Api/CmsV2CurrentBank.cs`
- 新增：`src-v2/WordSolution.CmsV2.Api/CmsV2CurrentBankResolver.cs`
- 修改：`src-v2/WordSolution.CmsV2.Api/CmsV2ApiServiceCollectionExtensions.cs`
- 修改：`src-v2/WordSolution.CmsV2.Api/CmsV2ApiEndpointExtensions.cs`
- 修改：`src-v2/WordSolution.CmsV2.Api/ContentBlockEditSessionBackgroundService.cs`
- 修改：`src-v2/WordSolution.CmsV2.Api/appsettings.json`

后端测试：

- 修改：`src-v2/WordSolution.CmsV2.Tests/Api/CmsV2ApiIntegrationTests.cs`
- 可新增：`src-v2/WordSolution.CmsV2.Tests/Api/CmsV2CurrentBankResolverTests.cs`

前端显示：

- 修改：`frontend-v2/src/apis/cmsV2Client.ts`
- 修改：`frontend-v2/src/components/containers/AppShell.vue`
- 修改：`frontend-v2/src/locales/zh-CN.ts`
- 修改：`frontend-v2/src/locales/en.ts`

文档：

- 已补充：`docs/cms-v2/backend/后端数据模型开发文档.md`
- 已补充：`docs/cms-v2/backend/领域模型结构说明.md`
- 修改：`docs/ui/ui-architecture.md` 或 `docs/ui/component-rules.md`，仅在前端新增共享展示组件时更新。
- 修改：本文档。

### Phase 1：后端配置模型与解析器

目标：把三题库配置从旧的单 `BankRootDirectory` 扩展为 `ActiveBankKey + Banks`，并提供稳定的当前题库解析结果。

实施步骤：

- 在 `CmsV2ApiOptions` 中保留 `BankRootDirectory` 作为兼容字段，新增 `ActiveBankKey` 与 `Banks`。
- 新增 `CmsV2BankOptions`：

```csharp
public sealed class CmsV2BankOptions
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string RootDirectory { get; set; } = string.Empty;
}
```

- 新增 `CmsV2CurrentBank`：

```csharp
public sealed record CmsV2CurrentBank(
    string BankKey,
    string DisplayName,
    string Kind,
    string RootDirectory);
```

- 新增 `CmsV2CurrentBankResolver`，提供 `Resolve(CmsV2ApiOptions options)`。
- 解析器必须校验：
  - `Banks` 配置存在时，`ActiveBankKey` 必填。
  - `Banks[].Key` 去空白后不为空，且大小写不敏感唯一。
  - `Banks[].Kind` 只允许 `Test`、`Production`。
  - `Banks[].RootDirectory` 去空白后不为空。
  - `ActiveBankKey` 必须匹配某个题库 key。
  - 解析出的 `RootDirectory` 使用 `Path.GetFullPath` 规范化。
- 兼容模式规则：
  - 当 `Banks` 为空且 `BankRootDirectory` 有值时，返回 `LEGACY / 当前题库 / Test / BankRootDirectory`。
  - 当 `Banks` 为空且 `BankRootDirectory` 也为空时，使用旧默认值启动，但最终仍返回 `LEGACY`。

测试要求：

- 新增解析器测试，覆盖：
  - `ActiveBankKey = TEST` 能解析到测试题库。
  - `ActiveBankKey = gz` 能匹配 `GZ`。
  - 重复 key 抛出清晰异常。
  - 未知 `ActiveBankKey` 抛出清晰异常。
  - 非法 `kind` 抛出清晰异常。
  - 旧 `BankRootDirectory` 兼容模式可用。

推荐命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter CmsV2CurrentBankResolverTests
```

实施状态（2026-06-25）：已完成。

- 关键文件：`CmsV2ApiOptions.cs`、`CmsV2BankOptions.cs`、`CmsV2CurrentBank.cs`、`CmsV2CurrentBankResolver.cs`、`CmsV2CurrentBankResolverTests.cs`。
- 实现结果：保留旧 `BankRootDirectory` 兼容字段，新增 `DefaultBankRootDirectory`、`ActiveBankKey` 与 `Banks`；解析器支持大小写不敏感 key 匹配、规范 key 返回、kind 校验、根目录 full path 规范化和 `LEGACY` 兼容模式。
- 验证结果：`CmsV2CurrentBankResolverTests` 7/7 通过。

### Phase 2：后端当前题库上下文接入

目标：API 层所有需要题库根目录的位置统一使用当前题库上下文，`health` 返回当前题库身份。

实施步骤：

- 在 DI 中注册 `CmsV2CurrentBank` 或等价 provider。
- `AddDbContext<CmsV2DbContext>` 使用 `currentBank.RootDirectory` 创建目录与 SQLite 路径。
- `InitializeCmsV2DatabaseAsync` 使用 `currentBank.RootDirectory`。
- `ContentBlockEditSessionBackgroundService` 使用当前题库根目录，不再直接读取 `options.Value.BankRootDirectory`。
- `CmsV2ApiEndpointExtensions` 中所有构造 Application command 的位置改用当前题库根目录。
- `GET /api/cms-v2/health` 返回：

```json
{
  "status": "ok",
  "bankKey": "TEST",
  "bankDisplayName": "测试题库",
  "bankKind": "Test",
  "bankRootDirectory": "E:\\Desktop\\题库中心\\TEST\\cms-v2"
}
```

- 更新 `appsettings.json` 为三题库配置，默认 `ActiveBankKey` 为 `TEST`。

测试要求：

- 更新 API 集成测试中的配置注入，从旧 `BankRootDirectory` 改为新 `ActiveBankKey + Banks`。
- 更新 health 测试断言 `bankKey`、`bankDisplayName`、`bankKind`、`bankRootDirectory`。
- 增加一个集成测试确认数据库文件创建在 active bank root 下。
- 保留或新增一个兼容模式测试，确保旧 `BankRootDirectory` 配置仍能跑通 health。

推荐命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter CmsV2ApiIntegrationTests
```

实施状态（2026-06-25）：已完成。

- 关键文件：`CmsV2ApiServiceCollectionExtensions.cs`、`CmsV2ApiEndpointExtensions.cs`、`ContentBlockEditSessionBackgroundService.cs`、`appsettings.json`、`CmsV2ApiIntegrationTests.cs`。
- 实现结果：DI 注册 `CmsV2CurrentBank`；DbContext、数据库初始化、后台编辑会话和 API endpoint command 构造统一使用 `currentBank.RootDirectory`；默认配置为 `TEST/GZ/CZ` 三题库且 `ActiveBankKey = TEST`。
- Health 契约：`GET /api/cms-v2/health` 返回 `status`、`bankKey`、`bankDisplayName`、`bankKind`、`bankRootDirectory`。
- 验证结果：`CmsV2ApiIntegrationTests` 29/29 通过。

### Phase 3：前端当前题库身份展示

目标：前端启动后在全局顶部栏展示当前题库身份，让用户明确当前连接的是 `TEST`、`GZ` 还是 `CZ`。

实施步骤：

- 在 `cmsV2Client.ts` 新增 `CmsV2HealthDto`：

```ts
export interface CmsV2HealthDto {
  status: string
  bankKey: string
  bankDisplayName: string
  bankKind: 'Test' | 'Production' | string
  bankRootDirectory: string
}
```

- 在 `cmsV2Api` 中新增：

```ts
getHealth: () => cmsV2FetchJson<CmsV2HealthDto>('/health')
```

- 在 `AppShell.vue` 中加载 health：
  - 初始状态显示“题库读取中”。
  - 成功后显示 `{bankDisplayName} · {bankKey}`。
  - `bankKind = Production` 时显示“正式”，`Test` 时显示“测试”。
  - 失败时显示“题库状态不可用”，不阻塞页面使用。
- 视觉位置：放在顶部栏右侧，与阶段状态和 API base 同组。
- 不增加危险操作拦截，不改变页面业务流程。
- 补充中英文 i18n 文案。

验证要求：

- 前端 typecheck 通过。
- 前端 build 通过。
- 浏览器 smoke：启动后端与前端后，顶部栏可看到当前题库身份。

推荐命令：

```powershell
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
```

实施状态（2026-06-25）：已完成。

- 关键文件：`frontend-v2/src/apis/cmsV2Client.ts`、`frontend-v2/src/components/containers/AppShell.vue`、`frontend-v2/src/locales/zh-CN.ts`、`frontend-v2/src/locales/en.ts`。
- 实现结果：前端 health DTO 与 `cmsV2Api.getHealth()` 已接入；`AppShell` 顶部栏右侧显示当前题库名称、key 与测试/正式属性；失败时显示不可用状态，不阻塞页面使用。
- 验证结果：前端 typecheck、build 与浏览器 smoke 通过；默认 `TEST` 显示为“测试题库 · TEST”和“测试”。

### Phase 4：文档同步、上线配置与最终验证

目标：补齐文档状态，确认三题库配置能作为上线前基础运行。

实施步骤：

- 检查 `docs/cms-v2/backend/后端数据模型开发文档.md` 与实际实现字段一致。
- 检查 `docs/cms-v2/backend/领域模型结构说明.md` 与实际边界一致。
- 如新增前端共享展示组件，更新 `docs/ui/component-rules.md`；如果只改 `AppShell.vue` 局部展示，则记录在现有 UI 文档中。本次仅补充 `docs/ui/i18n.md` 的 AppShell 当前题库文案约束。
- 检查 `appsettings.json` 默认 `ActiveBankKey = TEST`。
- 手动或脚本方式分别切换 `ActiveBankKey` 为 `TEST`、`GZ`、`CZ` 启动后端，确认 health 返回对应题库。
- 确认 `GZ` 与 `CZ` 不触发额外危险操作保护，因为第一版明确不做。

最终验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
git diff --check
```

浏览器 smoke：

- 后端默认 `TEST` 启动后，访问 `http://localhost:5166/api/cms-v2/health`，确认返回 `bankKey = TEST`。
- 前端打开后，顶部栏显示“测试题库 · TEST”。
- 将 `ActiveBankKey` 分别改为 `GZ`、`CZ` 并重启后端，health 能返回对应正式题库。
- 前端刷新后显示“高中题库 · GZ”或“初中题库 · CZ”。
- SectionPage、ContentBlocksPage、HandoutPage 能正常加载。

实施状态（2026-06-25）：已完成。

- 关键文件：`docs/cms-v2/backend/后端数据模型开发文档.md`、`docs/cms-v2/backend/领域模型结构说明.md`、`docs/ui/i18n.md`、`docs/superpowers/plans/WordSolution-Codex开发计划总文档.md`。
- 文档结果：规格已对齐 `TEST/GZ/CZ` 的 key、displayName、kind、rootDirectory；明确当前题库上下文属于 API/基础设施运行时配置，不进入 Domain/Application；业务 API 不新增 `{bankKey}`；不提供运行时切换入口；不新增正式库危险操作保护。
- Health 验证：默认 `TEST` 返回 `TEST / 测试题库 / Test`；临时配置覆盖 `GZ` 返回 `GZ / 高中题库 / Production`；临时配置覆盖 `CZ` 返回 `CZ / 初中题库 / Production`；三个题库均确认存在各自目录下的 `cms-v2.db`。
- 最终验证：`CmsV2CurrentBankResolverTests` 7/7 通过，`CmsV2ApiIntegrationTests` 29/29 通过，前端 typecheck 通过，前端 build 通过，浏览器 smoke 通过，相关文件 `git diff --check` 通过。

### 人工验收清单

- `TEST` 后端启动后，health 返回 `TEST / 测试题库 / Test`。
- `GZ` 后端启动后，health 返回 `GZ / 高中题库 / Production`。
- `CZ` 后端启动后，health 返回 `CZ / 初中题库 / Production`。
- 三个题库各自使用独立目录下的 `cms-v2.db`。
- 前端顶部栏能明确显示当前题库名称、key 和测试/正式属性。
- 业务 API 路径没有新增 `{bankKey}`。
- 前端没有出现运行时切换题库入口。
- 正式题库没有新增危险操作二次确认。
- ContentBlock、SectionPage、HandoutPage 基础流程在 `TEST` 下仍可用。
- 切换到 `GZ` 或 `CZ` 后，不会读取 `TEST` 的数据库或文件资产。

---

## 2026-06-26 CZ 内容清理与课前复习测验题面板

> **执行要求**：实现阶段必须使用唯一执行线程串行推进；每次只执行一个 Phase；不创建新的计划文档；不 stage、commit、push、reset、checkout。

### 目标

清理 `CZ` 题库中当前已有的全部 ContentBlock 内容数据，保留教学结构树、Section、AtomicSection 与已有 panel；同时在每个 AtomicSection 末尾提供一个空的“课前复习测验题”面板，用于后续向下堂课前测验练习导入题目。

### 架构口径

- “课前复习测验题”是新的 `AtomicSectionTeachingRole.PreClassQuiz`，不是新的 `ContentBlockType`，也不是新的题目类型。
- 向“课前复习测验题”面板导入题目时，仍创建普通 `ContentBlockType.Question`，只是在 `AtomicSectionItem.TeachingRole` 上标记为 `PreClassQuiz`。
- `Knowledge` 面板不容纳题目，前端隐藏导入题目入口，后端在题目导入上下文校验中防御性拒绝。
- Handout Word 与 SectionPage 直接导出 Word 第一版跳过 `PreClassQuiz` 内容，不映射为 `练习题` 样式。
- 现有 AS 通过数据回填补齐 `PreClassQuiz` panel；新建 AS 与 wrap-as-AS 通过默认 panel 定义自动创建。

### 规格来源

- `docs/cms-v2/backend/领域模型结构说明.md`
- `docs/cms-v2/backend/后端数据模型开发文档.md`
- `docs/cms-v2/backend/题目结构化预览-输出样式重绑定-多题导入开发文档.md`
- `docs/ui/section-page.md`
- `docs/ui/component-rules.md`

### 文件范围

后端领域与应用：

- 修改：`src-v2/WordSolution.CmsV2.Domain/Enums/DomainEnums.cs`
- 修改：`src-v2/WordSolution.CmsV2.Domain/Documents/QuestionOutputStyleOptions.cs`
- 修改：`src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionUseCases.cs`
- 修改：`src-v2/WordSolution.CmsV2.Application/Sections/SectionUseCases.cs`
- 修改：`src-v2/WordSolution.CmsV2.Application/ContentBlocks/QuestionImportUseCases.cs`
- 修改：`src-v2/WordSolution.CmsV2.Application/Handouts/HandoutGenerationUseCases.cs`

后端持久化与测试：

- 新增：`src-v2/WordSolution.CmsV2.Infrastructure/Persistence/Migrations/<timestamp>_AddPreClassQuizPanels.cs`
- 修改：`src-v2/WordSolution.CmsV2.Infrastructure/Persistence/Migrations/CmsV2DbContextModelSnapshot.cs`
- 修改或新增测试：`src-v2/WordSolution.CmsV2.Tests/Domain/DomainModelRuleTests.cs`
- 修改或新增测试：`src-v2/WordSolution.CmsV2.Tests/Application/AtomicSectionUseCasesTests.cs`
- 修改或新增测试：`src-v2/WordSolution.CmsV2.Tests/Application/SectionUseCasesTests.cs`
- 修改或新增测试：`src-v2/WordSolution.CmsV2.Tests/Application/QuestionImportUseCasesTests.cs`
- 修改或新增测试：`src-v2/WordSolution.CmsV2.Tests/Application/HandoutGenerationUseCasesTests.cs`
- 修改：`src-v2/WordSolution.CmsV2.Tests/Infrastructure/CmsV2PersistenceSchemaTests.cs`

前端：

- 修改：`frontend-v2/src/apis/cmsV2Client.ts`
- 修改：`frontend-v2/src/types/index.ts`
- 修改：`frontend-v2/src/components/business/AtomicSectionPanelCreateOverlay.vue`
- 修改：`frontend-v2/src/components/business/AtomicSectionPanelBlock.vue`
- 修改：`frontend-v2/src/components/business/SectionInspector.vue`
- 修改：`frontend-v2/src/composables/useSectionPageData.ts`
- 修改：`frontend-v2/src/locales/zh-CN.ts`
- 修改：`frontend-v2/src/locales/en.ts`

CZ 数据维护：

- 目标数据库：`E:\Desktop\题库中心\CZ\cms-v2\cms-v2.db`
- 目标资产目录：`E:\Desktop\题库中心\CZ\cms-v2\content-blocks`
- 目标导入会话目录：`E:\Desktop\题库中心\CZ\cms-v2\question-import-sessions`

### Phase 1：后端角色、默认面板与输出跳过

目标：后端理解 `PreClassQuiz`，新建 AS / wrap-as-AS 默认创建四个 panel，Word 输出不会把课前复习测验题误输出成练习题。

实施步骤：

- [ ] 在 `DomainEnums.cs` 中给 `AtomicSectionTeachingRole` 增加：

```csharp
PreClassQuiz = 6
```

- [ ] 在 `AtomicSectionUseCases.cs` 的 `DefaultPanelDefinitions` 末尾追加：

```csharp
(AtomicSectionTeachingRole.PreClassQuiz, 40),
```

- [ ] 确认 `CreateDefaultPanelsForAtomicSectionAsync` 仍只给 `Knowledge` panel 创建默认知识点 ContentBlock；`PreClassQuiz` panel 必须保持空状态。
- [ ] 在 `SectionUseCases.cs` 的 wrap-as-AS 默认 panel 定义中追加 `PreClassQuiz`，sort order 为 `40`。
- [ ] 在 `QuestionOutputStyleOptions.ResolveForTeachingRole` 中显式让 `PreClassQuiz` 返回 `null`：

```csharp
AtomicSectionTeachingRole.PreClassQuiz => null,
```

- [ ] 在 `HandoutGenerationUseCases.cs` 中确认输出收集逻辑遇到 `outputStemStyleName is null` 时跳过对应 ContentBlock，不加入 `HandoutDocumentElement`。
- [ ] 写测试确认新建 AS 默认 panel 顺序为 `Knowledge / Example / Variant / PreClassQuiz`，并且默认只存在知识点 ContentBlock，不存在课前复习测验题 ContentBlock。
- [ ] 写测试确认 wrap-as-AS 后默认 panel 同样包含 `PreClassQuiz`，已有 section item 仍不被错误归到该 panel。
- [ ] 写测试确认 `PreClassQuiz` 题目不会进入 Handout Word 与 SectionPage Word 的元素列表。

推荐验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter "AtomicSection|SectionUseCases|HandoutGeneration"
```

阶段完成标准：

- 新建 AS 与 wrap-as-AS 均创建四个默认 panel。
- 课前复习测验题 panel 为空，不默认创建 ContentBlock。
- Word 输出跳过 `PreClassQuiz`，不退化到 `Practice` / `练习题`。
- 不需要重启后端，除非执行线程后续启动了本地服务做验证。

### Phase 2：题目导入与前端入口适配

目标：用户可以向课前复习测验题 panel 导入题目；知识点 panel 不再显示导入题目入口，并由后端兜底拒绝题目导入。

实施步骤：

- [ ] 在 `QuestionImportUseCases.ValidateContextAsync` 中补充规则：
  - `AtomicSectionPanel` context 指向 `Knowledge` panel 时抛出业务校验异常。
  - 指向 `PreClassQuiz` panel 时允许创建 session，并将 `DefaultTeachingRole` 解析为 `PreClassQuiz`。
- [ ] 保持 `ConfirmAsync` 创建的内容块类型为 `ContentBlockType.Question`，不要新增 ContentBlock 类型或题目类型。
- [ ] 在 `cmsV2Client.ts` 与 `frontend-v2/src/types/index.ts` 的 `AtomicSectionTeachingRole` 联合类型中加入 `'PreClassQuiz'`。
- [ ] 在 `zh-CN.ts` / `en.ts` 中补充 `PreClassQuiz` 显示文案：
  - 中文：`课前复习测验题`
  - 英文：`Pre-class Quiz`
- [ ] 在 `AtomicSectionPanelCreateOverlay.vue` 的 role 选项中加入 `PreClassQuiz`，用于用户后续手动补 panel。
- [ ] 检查 `AtomicSectionPanelBlock.vue` 的导入按钮逻辑，保持 `Knowledge` 隐藏导入按钮；`PreClassQuiz` 应正常显示导入按钮。
- [ ] 在 `SectionInspector.vue` 的教学角色选项中加入 `PreClassQuiz`。
- [ ] 如 `useSectionPageData.ts` 有角色展示映射，补齐 `PreClassQuiz` 的显示 label。
- [ ] 写后端测试：Knowledge panel 创建题目导入 session 返回校验失败。
- [ ] 写后端测试：PreClassQuiz panel 创建并确认题目导入后，生成 `Question` ContentBlock 与 `AtomicSectionItem.TeachingRole = PreClassQuiz`。
- [ ] 运行前端类型检查与构建。

推荐验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter QuestionImport
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
```

阶段完成标准：

- Knowledge panel 前端没有“导入题目”按钮。
- Knowledge panel 题目导入请求被后端拒绝。
- PreClassQuiz panel 可以打开题目导入，并导入普通 Question ContentBlock。
- 前端 typecheck 与 build 通过。

### Phase 3：现有 AS 回填与 CZ 内容清理

目标：所有既有 AS 补齐空的课前复习测验题 panel；CZ 题库清掉已有 ContentBlock 内容和引用，只保留教学结构。

实施步骤：

- [ ] 新增 EF migration `AddPreClassQuizPanels`，用数据迁移为缺少 `PreClassQuiz` panel 的既有 AS 插入一条 panel。
- [ ] 回填 SQL 的核心规则：
  - `TeachingRole = 6`
  - `Title = AtomicSections.Title`
  - `Difficulty = AtomicSections.Difficulty`
  - `SortOrder = MAX(existing SortOrder) + 10`，若没有 panel 则用 `40`
  - 对已存在 `PreClassQuiz` panel 的 AS 不重复插入
- [ ] 更新 `CmsV2DbContextModelSnapshot.cs`。
- [ ] 写持久化测试确认迁移后的 schema 仍允许 `TeachingRole = 6`，且 `AtomicSectionPanels` 的唯一索引不会被破坏。
- [ ] 在执行 CZ 清理前，复制数据库备份到同目录，例如：

```powershell
Copy-Item -LiteralPath 'E:\Desktop\题库中心\CZ\cms-v2\cms-v2.db' -Destination 'E:\Desktop\题库中心\CZ\cms-v2\cms-v2.before-content-cleanup-20260626.db'
```

- [ ] 停止或确认没有正在运行的 CZ 后端写入进程。
- [ ] 对 `E:\Desktop\题库中心\CZ\cms-v2\cms-v2.db` 在单个事务中清理内容数据：
  - 删除指向 ContentBlock 的 `HandoutVersionItems`
  - 删除引用被删 `SectionItem` 的 `SectionVariantItems`
  - 删除 `SectionItems`
  - 删除 `AtomicSectionItems`
  - 删除 `ContentBlockRelations`
  - 删除 `ContentBlockVersionParts`
  - 删除 `ContentBlockVersions`
  - 删除指向 ContentBlock 的 `TagBindings`
  - 删除指向 ContentBlock / SectionItem / AtomicSectionItem 的 `TeachingNoteBindings`
  - 删除无绑定的孤立 `TeachingNotes`
  - 删除 `ContentBlocks`
- [ ] 删除 CZ 内容资产目录中的内容块资产：

```powershell
Remove-Item -LiteralPath 'E:\Desktop\题库中心\CZ\cms-v2\content-blocks' -Recurse -Force
```

- [ ] 删除 CZ 导入会话临时目录：

```powershell
Remove-Item -LiteralPath 'E:\Desktop\题库中心\CZ\cms-v2\question-import-sessions' -Recurse -Force
```

- [ ] 重新启动 CZ 后端，让迁移执行并确认 health 正常。
- [ ] 查询 CZ 数据库确认：
  - `ContentBlocks` 数量为 `0`
  - `SectionItems` 数量为 `0`
  - `AtomicSectionItems` 数量为 `0`
  - `TeachingTopics` / `Sections` / `AtomicSections` 数量仍保留
  - 每个 AS 至多一条 `PreClassQuiz` panel

推荐验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter Persistence
```

阶段完成标准：

- CZ 的内容数据清空，教学结构保留。
- 所有既有 AS 都有空的课前复习测验题 panel。
- 没有创建任何空的课前复习测验题 ContentBlock。
- 备份数据库存在，可在人工需要时回滚。

### Phase 4：文档同步、全量验证与人工验收准备

目标：确认实现与规格一致，完成后端/前端/浏览器/数据层验证，并给出人工验收清单。

实施步骤：

- [ ] 对照以下文档检查实现是否一致：
  - `docs/cms-v2/backend/领域模型结构说明.md`
  - `docs/cms-v2/backend/后端数据模型开发文档.md`
  - `docs/cms-v2/backend/题目结构化预览-输出样式重绑定-多题导入开发文档.md`
  - `docs/ui/section-page.md`
  - `docs/ui/component-rules.md`
- [ ] 如实现细节与文档存在差异，只更新对应既有文档，不新建规格文档。
- [ ] 运行后端全量测试。
- [ ] 运行前端 typecheck 与 build。
- [ ] 运行 `git diff --check`。
- [ ] 启动 CZ 后端与前端，浏览器 smoke 验证 SectionPage：
  - 教学结构树仍能加载。
  - AS 下能看到 `Knowledge / Example / Variant / PreClassQuiz`。
  - Knowledge panel 没有导入题目入口。
  - PreClassQuiz panel 有导入题目入口。
  - PreClassQuiz panel 初始为空。
- [ ] 导入一小题到 PreClassQuiz panel，确认：
  - 页面中显示为普通题目内容块。
  - Inspector / 数据中 occurrence role 是 `PreClassQuiz`。
  - Handout Word 输出与 SectionPage Word 输出不包含该题。
- [ ] 回归 HandoutPage 原有 Word 输出，确认 Example / Variant / Practice / Homework 未受影响。

最终验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
git diff --check
```

阶段完成标准：

- 后端测试、前端 typecheck、前端 build、`git diff --check` 通过。
- CZ 数据清理结果可解释，结构数据保留。
- 导入题目功能在 PreClassQuiz panel 中可用，在 Knowledge panel 中不可用。
- Word 输出跳过 PreClassQuiz，其他题型输出不回退。
- 最终报告说明是否需要重启后端，以及当前后端 health 结果。

### 人工验收清单

- CZ 题库中 ContentBlock 列表为空，教学结构树仍存在。
- 每个 AtomicSection 末尾都有“课前复习测验题”panel。
- “课前复习测验题”panel 初始为空，没有默认空题目。
- 新建 AtomicSection 后自动生成 `Knowledge / Example / Variant / PreClassQuiz` 四个默认 panel。
- 新建 AtomicSection 的 Knowledge panel 仍默认有一个同名、同难度的知识点 ContentBlock。
- wrap-as-AS 后已有内容仍不被错误归入 PreClassQuiz。
- Knowledge panel 不显示“导入题目”按钮。
- PreClassQuiz panel 显示“导入题目”按钮。
- 向 PreClassQuiz panel 导入题目后，ContentBlock 类型仍是普通 Question。
- 导入到 PreClassQuiz 的题目不出现在 Handout Word 与 SectionPage Word 导出结果中。
- Example / Variant / Practice / Homework 的导入题目和 Word 输出保持原行为。
- 标签、教学评注、SectionPage、HandoutPage 基础流程仍可用。

---

## 2026-06-26 Word 导出容错与跳过反馈

> **执行要求**：本计划涉及 `HandoutGenerationUseCases`、SectionPage 和 Word 输出测试，和当前“CZ 内容清理与课前复习测验题面板”计划存在共享文件。实现前必须确认 CZ 计划已完成、暂停，或由用户明确授权接管；实现阶段仍使用唯一执行线程串行推进，不要并发拉起多个执行线程。

### 目标

让 Handout Word 与 SectionPage 直接导出 Word 在遇到局部内容质量问题时不轻易失败：空 panel / PreClassQuiz 等应静默跳过；缺 Stem、源 docx 缺失或损坏等应提示跳过但继续生成；模板缺样式、结构关系循环、目标 Section 不存在等仍必须阻断生成。

### 架构口径

- Word 输出问题统一分为 `SilentSkip / WarningSkip / Blocking` 三类。
- `SilentSkip` 不进入用户提示；`WarningSkip` 进入结构化 issue 列表并跳过对应内容；`Blocking` 阻断生成。
- 结构化 issue 由后端产生，前端只展示，不在前端推断 Word 可导出性。
- SectionPage 直接导出增加独立预检接口；点击导出时先预检，只有无 Blocking issue 才继续下载。
- `generate-word` 仍必须复用同一套校验，不能只依赖前端预检。
- 第一版不做内容自动修复、不做模板选择、不做 PDF、不做导出问题集中处理中心。

### 规格来源

- `docs/cms-v2/backend/后端数据模型开发文档.md` 中“SectionPage 直接导出 Word / 导出容错与跳过策略”。
- `docs/cms-v2/backend/题目结构化预览-输出样式重绑定-多题导入开发文档.md` 中“输出 Word 样式重绑定 / 导出容错分级”。
- `docs/ui/section-page.md` 中“SectionPage 直接导出 Word / 导出预检与跳过提示”。
- `docs/ui/component-rules.md` 中“SectionPage Word 导出反馈”。

### 预计修改范围

后端领域与生成接口：

- 修改：`src-v2/WordSolution.CmsV2.Domain/Documents/IHandoutDocumentGenerator.cs`
- 修改：`src-v2/WordSolution.CmsV2.Application/Handouts/HandoutGenerationCommands.cs`
- 修改：`src-v2/WordSolution.CmsV2.Application/Handouts/HandoutGenerationUseCases.cs`
- 修改：`src-v2/WordSolution.CmsV2.Infrastructure/Documents/AsposeHandoutDocumentGenerator.cs`
- 修改：`src-v2/WordSolution.CmsV2.Api/CmsV2ApiEndpointExtensions.cs`

后端测试：

- 修改：`src-v2/WordSolution.CmsV2.Tests/Application/CmsV2HandoutGenerationUseCaseTests.cs`
- 修改：`src-v2/WordSolution.CmsV2.Tests/Api/CmsV2ApiIntegrationTests.cs`

前端：

- 修改：`frontend-v2/src/apis/cmsV2Client.ts`
- 修改：`frontend-v2/src/pages/SectionPage.vue`
- 修改：`frontend-v2/src/components/containers/SectionWorkspace.vue`（只在按钮 loading / disabled 透传不足时修改）
- 修改：`frontend-v2/src/locales/zh-CN.ts`
- 修改：`frontend-v2/src/locales/en.ts`

文档：

- 必要时同步修改上述规格来源文档。
- 不新建独立规格或计划文档。

### Phase 1：后端 issue 分级与 Section 预检用例

目标：让应用层能表达 `SilentSkip / WarningSkip / Blocking`，并为 SectionPage 提供独立预检用例。

实施步骤：

- [ ] 在 `IHandoutDocumentGenerator.cs` 中新增导出问题分级枚举，建议命名：

```csharp
public enum HandoutDocumentGenerationIssueSeverity
{
    SilentSkip = 1,
    WarningSkip = 2,
    Blocking = 3
}
```

- [ ] 给 `HandoutDocumentGenerationIssue` 增加 `Severity` 字段，默认值为 `Blocking`，避免现有缺样式问题被误判为可跳过。
- [ ] 将现有 `MissingQuestionStem` issue 标记为 `WarningSkip`。
- [ ] 在 `HandoutGenerationCommands.cs` 增加 Section 预检命令和复用结果：

```csharp
public sealed record ValidateSectionWordGenerationCommand(
    string BankRootDirectory,
    int SectionId);
```

- [ ] 在 `HandoutGenerationUseCases.cs` 增加 `ValidateSectionWordGenerationAsync`，复用 Section Word 内容解析和文档生成器校验。
- [ ] 将 `GetBlockingIssues` 从“非 MissingQuestionStem”改为按 `Severity == Blocking` 判断。
- [ ] 将 `RemoveSkippedContentBlocks` 从只识别 `MissingQuestionStem` 改为识别 `Severity == WarningSkip` 且带 `ContentBlockId / ContentBlockVersionId` 的 issue。
- [ ] 写应用层测试：Section 预检遇到缺 Stem 时返回 `WarningSkip` issue，`IsValid = true` 或等价语义表示“可生成但有跳过”。
- [ ] 写应用层测试：模板缺少必需样式时返回 `Blocking` issue，生成入口仍失败。

推荐验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter "GenerateSectionWord|ValidateSectionWord|HandoutGeneration"
```

阶段完成标准：

- `MissingQuestionStem` 不再依赖硬编码方法单独判断，而是通过 issue severity 进入跳过链路。
- Section 预检应用层能力存在。
- 既有 Handout Word 缺样式阻断行为不变。

### Phase 2：后端容错生成与 Section API

目标：补齐缺源文件、坏源文件、无当前版本、空 AS heading 等导出失败点，并暴露 Section 级预检 API。

实施步骤：

- [ ] 在 `HandoutGenerationUseCases.cs` 的内容解析阶段，把以下问题转换为 `WarningSkip` issue，而不是直接抛出导致整份导出失败：
  - ContentBlock 没有当前版本。
  - ContentBlock 当前版本没有可读取 docx。
  - ContentBlock 源 docx 文件不存在。
- [ ] 调整内部解析结果结构，让解析阶段可以携带 `Issues`，并与 `AsposeHandoutDocumentGenerator.ValidateWordGenerationAsync` 返回的 issue 合并。
- [ ] 在 `AsposeHandoutDocumentGenerator.cs` 中捕获单个 ContentBlock 源 docx 无法打开 / 损坏的问题，返回 `WarningSkip` issue；模板文件无法打开仍为 `Blocking`。
- [ ] 在生成前移除 `WarningSkip` 对应的 ContentBlock 元素。
- [ ] 在移除跳过内容后清理空 AtomicSection heading：如果某个 Heading 3 后面直到下一个同级或更高层级 heading 前没有任何 ContentBlock，则移除该 Heading 3。
- [ ] 保持 Section Heading 2：只要 Section 存在，导出的文档仍有 Section 标题。
- [ ] 在 `CmsV2ApiEndpointExtensions.cs` 增加：

```text
POST /api/cms-v2/sections/{sectionId}/validate-word-generation
```

- [ ] API 返回结构化 JSON，至少包含 `isValid`、`issues`；其中 `isValid = false` 表示存在 `Blocking` issue。
- [ ] `POST /sections/{sectionId}/generate-word` 继续返回文件；内部必须复用 Phase 1/2 的同一校验和跳过逻辑。
- [ ] 写 API 测试：Section 预检缺 Stem 返回 200 与 `WarningSkip` issue。
- [ ] 写 API 测试：Section 预检缺模板样式返回 `isValid = false` 与 `Blocking` issue。
- [ ] 写应用层测试：只有空 panel 的 AS 不输出占位；所有内容被跳过的 AS 不留下孤立 Heading 3。

推荐验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter "GenerateSectionWord|ValidateSectionWord|CmsV2ApiIntegrationTests"
```

阶段完成标准：

- Section 预检 API 可用。
- 缺 Stem、缺源 docx、坏源 docx、无当前版本不会直接阻断整份 Section Word 导出。
- 缺模板样式仍阻断。
- 空 AS / 空 panel 不产生占位或孤立 AS 标题。
- 不创建 Handout、OutputForm、GeneratedFile。

### Phase 3：SectionPage 前端预检与跳过提示

目标：用户点击 SectionPage “导出 Word”时，能先看到阻断错误；只有可生成时才下载；下载成功后能知道跳过了哪些内容。

实施步骤：

- [ ] 在 `cmsV2Client.ts` 增加类型：

```ts
export type WordGenerationIssueSeverity = 'SilentSkip' | 'WarningSkip' | 'Blocking'

export interface WordGenerationIssue {
  code: string
  message: string
  severity: WordGenerationIssueSeverity
  contentBlockId?: number | null
  contentBlockVersionId?: number | null
  outputTemplateId?: number | null
  requiredStyleName?: string | null
  occurrenceRole?: string | null
}

export interface WordGenerationValidationResult {
  isValid: boolean
  issues: WordGenerationIssue[]
}
```

- [ ] 在 `cmsV2Client.ts` 增加 `validateSectionWordGeneration(sectionId)`，调用 Section 级预检 API。
- [ ] 修改 `SectionPage.vue` 的 `requestSectionWordExport`：
  - 导出按钮进入 loading。
  - 先调用 `validateSectionWordGeneration`。
  - 若存在 `Blocking` issue，不调用下载接口，显示页面级错误。
  - 若只有 `WarningSkip` issue，继续调用 `downloadSectionWord`。
  - 下载成功后显示“已导出，跳过 N 个内容项”。
  - `SilentSkip` 不提示。
- [ ] 错误与提示文案放入 `zh-CN.ts` / `en.ts`，不要在组件里散落长中文。
- [ ] 如 `SectionWorkspace.vue` 现有按钮只能 disabled 不能表达导出 loading，则只补最小 loading/disabled prop；不要改布局。
- [ ] 不新增复杂问题抽屉；第一版使用页面级 alert / toast / compact list。
- [ ] 不把 issue 判断下沉到 `ContentBlockDisplay`、`AtomicSectionBlock`、`AtomicSectionPanelBlock`。

推荐验证命令：

```powershell
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
```

阶段完成标准：

- SectionPage 导出前会调用预检。
- Blocking issue 阻止下载。
- WarningSkip issue 允许下载并提示跳过数量。
- 前端 typecheck / build 通过。

### Phase 4：文档同步、回归验证与人工验收准备

目标：确认实现与文档一致，完整验证 Handout Word 与 SectionPage Word 的主流程和容错流程。

实施步骤：

- [ ] 对照以下文档检查实现是否一致：
  - `docs/cms-v2/backend/后端数据模型开发文档.md`
  - `docs/cms-v2/backend/题目结构化预览-输出样式重绑定-多题导入开发文档.md`
  - `docs/ui/section-page.md`
  - `docs/ui/component-rules.md`
- [ ] 如实现细节与文档存在差异，只更新对应既有文档。
- [ ] 运行后端全量测试。
- [ ] 运行前端 typecheck 与 build。
- [ ] 运行 `git diff --check`。
- [ ] 浏览器 smoke 验证 SectionPage：
  - 正常 Section 能直接导出 Word。
  - 存在缺 Stem 题目时，导出继续，页面提示跳过。
  - 模板缺样式时，页面不下载并显示错误。
  - 空 panel / 空 AS 不提示，不输出占位。
- [ ] 回归 HandoutPage：
  - 原有 `output-forms/{id}/validate-word-generation` 仍可用。
  - 原有 Word 生成仍创建 GeneratedFile。
  - 缺 Stem 仍跳过并不写入 manifest sources。
  - 缺模板样式仍阻断。

最终验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
git diff --check
```

阶段完成标准：

- SectionPage 与 HandoutPage Word 导出问题分级一致。
- 可跳过内容不会阻断整份导出。
- 真正结构/模板级问题仍会阻断。
- 实现报告说明是否需要重启后端，以及 health 结果。

### 人工验收清单

- SectionPage 正常内容点击“导出 Word”可下载 `.docx`。
- SectionPage 中空 panel 不提示、不输出占位。
- AtomicSection 内没有可输出内容时，不留下孤立 AS 标题。
- 题目缺 Stem 时，SectionPage 仍可导出，并提示跳过该题。
- 源 docx 缺失或损坏时，SectionPage 仍可导出，并提示跳过对应内容。
- 默认模板缺少 `例题 / 变式 / 练习题` 等必需样式时，SectionPage 不下载文件并显示错误。
- `PreClassQuiz` 仍静默跳过，不提示“跳过题目”。
- HandoutPage 预检和 Word 生成保持原行为。
- HandoutPage 缺 Stem 题目仍跳过且不写入 manifest sources。
- HandoutPage 缺模板样式仍阻断生成。
- 不创建额外 Handout、OutputForm 或 GeneratedFile 来服务 SectionPage 直接导出。

---

## 2026-06-26 Section 新增 AS 后 Variant 同步提示

> **执行要求**：本计划应等待当前“Word 导出容错与跳过反馈”计划完成后再启动。实现阶段必须复用唯一执行线程串行推进；不新建计划文档；不 stage、commit、push、reset、checkout；不修改 V1、VSTO、Word 本地文件操作核心库、题库本地服务/wwwroot。

**Goal:** 在 SectionPage 新增顶层 AtomicSection 后，提示用户可将该 AS 同步到符合条件的已有 SectionVariant，并由用户确认后批量写入。

**Architecture:** 后端负责候选计算、难度覆盖判断、插入位置推导和事务性批量写入；前端只在新增 AS 成功后展示后端候选、收集用户选择并调用同步 API。`SectionVariant` 继续保持显式选择模型，不改成自动实时镜像。

**Tech Stack:** CMS V2 Application / API / EF Core 仓储、Vue 3、TypeScript、vue-i18n、现有 SectionPage 组件体系。

### 规格来源

- `docs/ui/section-page.md` 中“当前补充：新增 AS 后 Variant 同步提示”。
- `docs/ui/component-rules.md` 中“Section 新增 AS 后 Variant 同步提示”。
- `docs/cms-v2/backend/领域模型结构说明.md` 中“SectionVariant 显式选择与新增 AS 同步语义”。
- `docs/cms-v2/backend/后端数据模型开发文档.md` 中“新增 AS 后 SectionVariant 同步候选与批量写入”。

### 预计修改范围

后端应用层：

- 修改：`src-v2/WordSolution.CmsV2.Application/SectionVariants/SectionVariantCommands.cs`
- 修改：`src-v2/WordSolution.CmsV2.Application/SectionVariants/SectionVariantUseCases.cs`
- 如仓储缺少所需查询，修改：`src-v2/WordSolution.CmsV2.Domain/Repositories/ICmsV2Repositories.cs`
- 如仓储缺少所需查询，修改：`src-v2/WordSolution.CmsV2.Infrastructure/Repositories/EfEntityRepositories.cs`

后端 API：

- 修改：`src-v2/WordSolution.CmsV2.Api/CmsV2ApiEndpointExtensions.cs`

后端测试：

- 修改：`src-v2/WordSolution.CmsV2.Tests/Application/CmsV2ApplicationUseCaseTests.cs` 或新增同目录下聚焦 `SectionVariantSync` 的测试文件。
- 修改：`src-v2/WordSolution.CmsV2.Tests/Api/CmsV2ApiIntegrationTests.cs`

前端：

- 修改：`frontend-v2/src/apis/cmsV2Client.ts`
- 修改：`frontend-v2/src/pages/SectionPage.vue`
- 修改：`frontend-v2/src/composables/useSectionItemActions.ts`
- 可新增：`frontend-v2/src/components/business/SectionVariantSyncDialog.vue`
- 修改：`frontend-v2/src/locales/zh-CN.ts`
- 修改：`frontend-v2/src/locales/en.ts`

文档：

- 必要时收口更新上述规格来源文档。

### 执行线程派发边界

总控线程后续拉起本计划每个阶段时，必须把当前阶段完整边界复制给唯一执行线程，不允许只发送阶段标题或只引用本计划名称。

通用派发边界：

- 必须先读 `AGENTS.md`、`CONTRIBUTING.md`、`.codex/内容管理系统详细架构.md`、`.codex/内容管理系统升级路线.md`、本总计划文档、上述规格来源文档，以及该阶段涉及的 UI / backend 文档。
- 每次只执行当前阶段；完成后停止，等待总控继续派发下一阶段。
- 不修改 V1、VSTO、Word 本地文件操作核心库、题库本地服务/wwwroot。
- 不 stage、commit、push、reset、checkout。
- 保护工作区已有未提交改动，不回滚用户、总控或其他线程改动。
- 不把本能力实现成自动实时镜像；`SectionVariant` 继续是显式选择模型。
- 不新增 `SectionVariantSyncRule`、`SectionVariantAutoSyncPolicy`、`SectionVariantDiff`、`SectionVariantPendingChange` 等持久化模型。
- 不复制 `AtomicSection`、`ContentBlock`、DOCX、HTML 或文件资产；同步只创建 `SectionVariantItem`。
- 不新增 `{bankKey}` API 路径。
- 如发现超出当前阶段范围的问题，只在最终报告中说明，不顺手修改。

Phase 1 派发边界：

- 只做 Application / Domain / Infrastructure 仓储所需的候选计算与事务性批量同步用例。
- 可以新增命令、DTO、用例方法和必要仓储查询。
- 必须覆盖候选规则、顶层 AS 校验、扫描式插入顺序、空目标列表拒绝、POST 阶段重新校验和事务回滚。
- 不暴露 API，不改前端，不改文档收口，不新增 EF migration。

Phase 2 派发边界：

- 只做 Section 级 API endpoint 和 API 集成测试。
- `GET` 只返回后端计算出的候选；`POST` 必须调用 Phase 1 用例并重新校验，不得只信任前端传入 ID。
- 错误响应沿用 CMS V2 现有异常处理。
- 不做前端调用，不改 UI，不改 Variant 详情页，不做文档最终收口。

Phase 3 派发边界：

- 只做 SectionPage 前端入口、client 类型/API、轻提示、紧凑同步对话框、loading/disabled 与 i18n。
- 仅在新增顶层 AS SectionItem 成功后触发候选查询；AS 内部新增块、panel/item 变化、顶层 ContentBlock、非顶层 SectionItem 均不触发。
- 候选数量为 0 不提示。
- 候选对话框默认全选；当选中数为 0 时禁用确认按钮，不发送空同步请求。
- 同步成功后只显示成功反馈、关闭对话框、清除 pending 状态；不刷新 Variant 列表，不切换视图，不自动打开 Variant。
- 同步失败后保留对话框和勾选状态，提示“同步失败，未修改任何 Variant”。
- 不新增复杂同步中心、问题抽屉、全局通知中心，不改后端，不改 Variant 查看时的数据刷新逻辑。

Phase 4 派发边界：

- 只做规格一致性检查、必要文档收口、后端全量测试、前端 typecheck/build、`git diff --check`、浏览器 smoke 和最终报告。
- 不新增功能，不扩大同步触发范围，不修改业务语义。
- 如发现实现和规格不一致，优先要求执行线程修复；只有实现确实合理且经总控/用户确认后才更新规格。

### Phase 1：后端候选计算与批量同步用例

目标：在应用层提供候选查询和事务性批量同步能力，不暴露 API，不接前端。

实施步骤：

- [ ] 在 `SectionVariantCommands.cs` 增加：

```csharp
public sealed record GetSectionVariantSyncCandidatesCommand(
    int SectionId,
    int SectionItemId);

public sealed record SyncSectionItemToVariantsCommand(
    int SectionId,
    int SectionItemId,
    IReadOnlyList<int> SectionVariantIds);

public sealed record SectionVariantSyncCandidateDto(
    int SectionVariantId,
    string Title,
    SectionVariantType Type,
    Difficulty Difficulty,
    SectionVariantStatus Status);

public sealed record SectionVariantSyncResultDto(
    int SectionItemId,
    IReadOnlyList<int> SyncedSectionVariantIds);
```

- [ ] 在 `SectionVariantUseCases` 增加 `GetSectionVariantSyncCandidatesAsync`。
- [ ] 候选规则严格限定为同 Section、`Draft / Active`、未包含该 `SectionItem`、难度覆盖、非归档。
- [ ] 校验待同步 `SectionItem` 必须是顶层 `AtomicSection` item。
- [ ] 增加 `SyncSectionItemToVariantsAsync`，在单个事务中为用户选择的 Variant 创建 `SectionVariantItem`。
- [ ] 插入顺序使用扫描式规则：从新增 AS 在 Section 顶层顺序中的位置向前找最近已选前序项，向后找最近已选后序项，再决定插入位置。
- [ ] 如果相邻 `SortOrder` 没有可用间隙，在同一事务内重排该 Variant 的 `SectionVariantItem.SortOrder`。
- [ ] `SectionVariantIds` 为空时拒绝请求，不返回“成功但无写入”。
- [ ] 同步写入时重新校验全部候选规则，防止 GET 候选结果过期。
- [ ] 任意目标 Variant 不合法、已包含、已归档、跨 Section 或难度不覆盖时，整体失败并回滚。

测试要求：

- [ ] 候选查询包含 `Draft / Active` 且难度覆盖的 Variant。
- [ ] `Archived`、已包含、难度不覆盖、跨 Section 的 Variant 不进入候选。
- [ ] 直接前一个 SectionItem 未在 Variant 中时，继续向前找到最近已选前序项。
- [ ] 只有后序项时插入后序项之前。
- [ ] 空目标列表被拒绝。
- [ ] GET 后目标 Variant 状态或内容变化时，POST 重新校验并整体失败。
- [ ] 任意目标 Variant 不合法时不产生部分写入。

推荐验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter "SectionVariant"
```

阶段完成标准：

- 应用层候选和同步测试通过。
- 不触碰 API 与前端。
- 不新增 EF migration。

### Phase 2：Section 级 API 接入

目标：暴露 SectionPage 所需的候选查询和批量同步 API，并用集成测试固定契约。

实施步骤：

- [ ] 在 `CmsV2ApiEndpointExtensions.cs` 新增：

```text
GET  /api/cms-v2/sections/{sectionId}/variant-sync-candidates?sectionItemId={sectionItemId}
POST /api/cms-v2/sections/{sectionId}/variant-sync
```

- [ ] `GET` 返回 `SectionVariantSyncCandidateDto[]`。
- [ ] `POST` 请求体为：

```json
{
  "sectionItemId": 123,
  "sectionVariantIds": [10, 11, 12]
}
```

- [ ] `POST` 成功返回：

```json
{
  "sectionItemId": 123,
  "syncedSectionVariantIds": [10, 11, 12]
}
```

- [ ] API 不新增 `{bankKey}`，继续使用当前题库上下文。
- [ ] 错误响应沿用 CMS V2 现有应用层异常处理。

测试要求：

- [ ] API 候选查询返回符合规则的候选列表。
- [ ] API 批量同步成功后能查询到新增 `SectionVariantItem`。
- [ ] 已包含、跨 Section 或归档 Variant 请求返回错误，并确认没有部分写入。
- [ ] 空 `sectionVariantIds` 请求返回错误。

推荐验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj --filter "CmsV2ApiIntegrationTests"
```

阶段完成标准：

- API 集成测试通过。
- 仍不接前端。

### Phase 3：SectionPage 前端提示与同步对话框

目标：新增 AS 成功后，SectionPage 展示候选同步提示，并支持用户确认批量同步。

实施步骤：

- [ ] 在 `cmsV2Client.ts` 增加类型：

```ts
export interface SectionVariantSyncCandidateDto {
  sectionVariantId: number
  title: string
  type: string
  difficulty: string
  status: string
}

export interface SectionVariantSyncResultDto {
  sectionItemId: number
  syncedSectionVariantIds: number[]
}
```

- [ ] 在 `cmsV2Client.ts` 增加 `getSectionVariantSyncCandidates(sectionId, sectionItemId)` 与 `syncSectionItemToVariants(sectionId, sectionItemId, sectionVariantIds)`。
- [ ] 在 `useSectionItemActions.ts` 或 `SectionPage.vue` 中识别“新增成功的顶层 AtomicSection SectionItem”。
- [ ] 新增 AS 后先刷新当前 Section 数据，再调用候选 API。
- [ ] 候选数量为 0 时不提示。
- [ ] 候选数量大于 0 时显示页面级轻提示：`已新增 AS，可同步到 X 个 Variant`。
- [ ] 新增或复用紧凑对话框，候选 Variant 默认全选，用户可取消勾选。
- [ ] 当已选候选数量为 0 时禁用确认按钮，不调用同步 API。
- [ ] 对话框只 emit `submit(sectionVariantIds)` 与 `close`，不直接调用 API。
- [ ] 同步成功后显示 `已同步到 X 个 Variant`，关闭对话框，清除当前 pending sync 状态。
- [ ] 同步失败后显示 `同步失败，未修改任何 Variant`，保留对话框与勾选状态。
- [ ] 不刷新 Variant 列表，不切换页面，不自动打开 Variant。
- [ ] 中英文文案写入 `zh-CN.ts` / `en.ts`。

推荐验证命令：

```powershell
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
```

阶段完成标准：

- 新增 AS 后能看到候选同步提示。
- 用户可选择 Variant 并提交同步。
- 前端 typecheck / build 通过。

### Phase 4：文档同步、全量验证与人工验收准备

目标：确认实现与规格一致，完成后端、前端、浏览器 smoke 与回归验证。

实施步骤：

- [ ] 对照本计划的规格来源文档检查实现一致性。
- [ ] 如实现细节与文档存在差异，只更新对应既有文档，不新建规格或计划文档。
- [ ] 运行后端全量测试。
- [ ] 运行前端 typecheck 与 build。
- [ ] 运行 `git diff --check`。
- [ ] 浏览器 smoke 验证 SectionPage：
  - 新增顶层 AS 后出现同步提示。
  - 点击 `查看并同步` 可看到候选 Variant。
  - 默认全选，可取消部分选择。
  - 同步成功后提示成功且不切换当前视图。
  - 同步失败时保留选择并提示未修改任何 Variant。
- [ ] 回归 Variant 创建、Variant 查看、HandoutPage 引用 SectionVariant 的基础流程。

最终验证命令：

```powershell
dotnet test .\src-v2\WordSolution.CmsV2.Tests\WordSolution.CmsV2.Tests.csproj
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
git diff --check
```

阶段完成标准：

- 后端全量测试、前端 typecheck/build、`git diff --check` 通过。
- SectionPage 浏览器 smoke 通过。
- 最终报告说明是否需要重启后端及 health 结果。

### 人工验收清单

- 新增顶层 AS 后，如果没有符合条件 Variant，不显示同步提示。
- 新增顶层 AS 后，如果存在符合条件 Variant，显示“已新增 AS，可同步到 X 个 Variant”。
- 候选只包含同 Section、`Draft / Active`、未包含该 AS、难度覆盖的 Variant。
- `Archived` Variant 不进入候选。
- 点击“查看并同步”后候选默认全选。
- 取消部分候选后只同步剩余候选。
- 取消全部候选后确认按钮不可用，不发送空同步请求。
- 同步后的 AS 在目标 Variant 中按 Section 顶层顺序插入；直接邻居缺失时能继续扫描更远的已选前后项。
- 同步成功后不刷新 Variant 列表、不切换视图、不打开 Variant。
- 同步失败时没有部分写入，前端提示“同步失败，未修改任何 Variant”。
- AS 内部新增块不会触发 Variant 同步提示；已包含该 AS 的 Variant 后续能自然展示 AS 内部最新内容。

---

## 2026-06-26 AS 级导入题目到未归组

> **执行要求**：本计划是一个窄范围前端接线任务。实现阶段必须使用单独执行线程推进；不 stage、commit、push、reset、checkout；不修改 V1、VSTO、Word 本地文件操作核心库、题库本地服务/wwwroot；不顺手修复 Word 导出、SectionVariant 同步、AS 删除、标签或教学评注问题。

### 目标

在 `SectionPage` 的 `AtomicSectionBlock` 标题操作区增加一个 AS 级“导入题目”入口。该入口复用现有 `QuestionImportDialog` 与临时 Word session 流程，但导入结果必须进入当前 AS 的未归组区域，也就是创建 `AtomicSectionPanelId = null` 的 `AtomicSectionItem`，并追加到未归组末尾。

### 规格来源

- `docs/ui/section-page.md` 中“AS 级导入题目到未归组”。
- `docs/ui/component-rules.md` 中 `AtomicSectionBlock` 与 `QuestionImportDialog 可复用上下文`。
- `docs/cms-v2/backend/题目结构化预览-输出样式重绑定-多题导入开发文档.md` 中现有临时 Word 多题导入、`InsertQuestionContext` 与 panel 导入约束。

### 当前已确认实现基础

- 后端 `InsertQuestionContext` 已支持 `AtomicSectionId`。
- 当 `AtomicSectionId` 有值且 `AtomicSectionPanelId = null` 时，确认导入会创建 `AtomicSectionItem`。
- `InsertAtomicSectionItemsAsync` 会按 `AtomicSectionPanelId` 作用域插入；`null` 作用域即未归组区域。
- 当前缺口是前端只暴露了 Section 顶层入口和 AtomicSectionPanel 入口，尚未在 `AtomicSectionBlock` 暴露 `AtomicSection` 入口。

### 预计修改范围

前端类型：

- 修改：`frontend-v2/src/types/index.ts`
  - 将 `QuestionImportTarget` 扩展为 `SectionTopLevel | AtomicSection | AtomicSectionPanel`。
  - 为 `QuestionImportContext` 增加 `AtomicSection` 分支，字段包括 `sectionId`、`sectionTitle`、`atomicSectionId`、`atomicSectionTitle`、可选 `afterAtomicSectionItemId`。

前端组件接线：

- 修改：`frontend-v2/src/components/business/AtomicSectionBlock.vue`
  - 在 AS 标题操作区增加 AS 级导入按钮。
  - 只 emit AS 级导入事件，不调用 API。
  - 不向该事件传 `atomicSectionPanelId`。
- 修改：`frontend-v2/src/components/containers/SectionWorkspace.vue`
  - 透传 `AtomicSectionBlock` 的 AS 级导入事件给页面。
- 修改：`frontend-v2/src/pages/SectionPage.vue`
  - 增加 AS 级导入事件处理函数。
  - 打开 `QuestionImportDialog` 时使用 `QuestionImportContext.target = AtomicSection`。
  - `buildQuestionImportApiContext` 识别 `AtomicSection` 分支，并构造：
    - `sectionId = 当前 Section`
    - `atomicSectionId = 当前 AS`
    - `atomicSectionPanelId = null`
    - `afterAtomicSectionItemId = null`
    - `afterSectionItemId = null`
    - `defaultTeachingRole = Unclassified`
    - `defaultDifficulty = 当前 AS 难度`
  - 批量确认后沿用现有刷新和定位逻辑。

前端文案：

- 修改：`frontend-v2/src/locales/zh-CN.ts`
- 修改：`frontend-v2/src/locales/en.ts`
- 文案必须使用 i18n，不在 Vue 模板中硬编码中文或英文。

文档：

- 必要时只收口上述规格来源文档，不新增新文档文件。

### 明确不做

- 不修改后端 Application / API / Domain / Infrastructure，除非实现中发现现有 AtomicSection import context 已损坏；如果损坏，停止并报告，不自行扩展需求。
- 不新增 API endpoint。
- 不新增新的导入弹窗。
- 不恢复 `.docx` 文件上传式导入。
- 不新增逐候选确认 API。
- 不在 `QuestionImportDialog` 里直接调用 API。
- 不在 `AtomicSectionBlock`、`SectionWorkspace` 中调用 API。
- 不把 AS 级导入写成 panel 导入。
- 不传递、伪造或推断 `atomicSectionPanelId`。
- 不自动归入 Example / Variant / Practice / Homework / PreClassQuiz panel。
- 不修改 `Knowledge` panel 隐藏导入题目入口的现有规则。
- 不新增 ContentBlockType、QuestionType、TeachingRole 或 Difficulty。
- 不修改 Word 导出跳过逻辑。
- 不修改 SectionVariant 自动同步计划。
- 不修改标签、教学评注、AS 删除或内容清理逻辑。

### 单阶段执行计划

本任务只设置一个实现阶段，避免把小功能拆成多个执行线程。

#### Phase 1：前端 AS 级导入入口接入

目标：在 `AtomicSectionBlock` 上增加 AS 级导入按钮，并复用现有 `QuestionImportDialog` 把题目导入当前 AS 未归组末尾。

实施步骤：

- [ ] 读取必读文档与本计划，确认当前分支不是 `master`。
- [ ] 检查 `QuestionImportContext`、`SectionPage`、`SectionWorkspace`、`AtomicSectionBlock`、`AtomicSectionPanelBlock` 现有导入链路。
- [ ] 扩展前端 `QuestionImportTarget` / `QuestionImportContext`，增加 `AtomicSection` 分支。
- [ ] 在 `SectionPage.getQuestionImportContextKey` 中加入 `AtomicSection` 上下文 key，避免 AS 间切换时复用旧 session。
- [ ] 在 `SectionPage.buildQuestionImportApiContext` 中加入 `AtomicSection` 分支，确保 `atomicSectionPanelId = null` 且 `defaultTeachingRole = Unclassified`、`defaultDifficulty = 当前 AS 难度`。
- [ ] 在 `AtomicSectionBlock` 增加按钮和 emit，按钮只在非只读状态可用。
- [ ] 在 `SectionWorkspace` 透传 AS 级导入事件。
- [ ] 在 `SectionPage` 增加 AS 级导入处理函数，打开 `QuestionImportDialog`。
- [ ] 补充 `zh-CN.ts` 与 `en.ts` 文案。
- [ ] 运行前端 typecheck 与 build。
- [ ] 运行 `git diff --check`。

推荐验证命令：

```powershell
npm --prefix .\frontend-v2 run typecheck
npm --prefix .\frontend-v2 run build
git diff --check
```

阶段完成标准：

- `AtomicSectionBlock` 标题操作区出现 AS 级导入题目入口。
- 点击后打开现有 `QuestionImportDialog`，目标语义为当前 AS。
- API 请求中的 `atomicSectionId` 为当前 AS，`atomicSectionPanelId = null`。
- 导入确认后题目进入未归组区域末尾。
- 现有 Section 顶层导入保持原行为。
- 现有 panel 导入保持原行为。
- `Knowledge` panel 仍无导入题目入口。
- 前端 typecheck、build、`git diff --check` 通过。

### 人工验收清单

- 在 SectionPage 中找到某个 AS，AS 标题操作区可见“导入题目”入口。
- 点击 AS 级入口后打开现有多题导入弹窗，而不是新弹窗。
- 完成临时 Word 导入并确认候选题后，新题目出现在该 AS 的未归组区域末尾。
- 新题目不进入 Example / Variant / Practice / Homework / PreClassQuiz 任一 panel。
- panel 内原“导入题目”入口仍把题目导入对应 panel。
- `Knowledge` panel 仍不显示“导入题目”。
- Section 顶部“导入题目”仍导入为 Section 顶层内容。
- 只读模式、Variant 选择模式、wrap 选择模式下不能触发 AS 级导入。
