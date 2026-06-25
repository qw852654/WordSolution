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
