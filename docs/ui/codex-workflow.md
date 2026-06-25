# CMS V2 鍓嶇 Codex 寮?鍙戞祦绋?

鏈枃妗ｇ害鏉熷悗缁娇鐢? Codex 寮?鍙? V2 鍓嶇鏃剁殑宸ヤ綔鏂瑰紡銆?

褰撳墠鍓嶆彁锛?

- V1 鍓嶇宸茬粡搴熷純銆?
- V1 鍚庣涔熷凡缁忓簾寮冦??
- 鏂板墠绔彧鑳藉鎺? CMS V2 鍚庣銆?
- 鏃ч潤鎬侀〉闈㈠拰鏃ф帴鍙ｅ彧鑳戒綔涓哄巻鍙插弬鑰冿紝涓嶈兘缁х画浣滀负瀹炵幇搴曞骇銆?

褰撳墠闃舵 1 鍙厑璁稿仛鏂囨。娓呯悊锛屼笉鍒涘缓鍓嶇宸ョ▼锛屼笉寮?濮嬪疄鐜伴〉闈㈡垨缁勪欢銆?

## 1. 寮?鍙戝墠蹇呰

鎵?鏈変换鍔″繀椤昏鍙栵細

```text
AGENTS.md
CONTRIBUTING.md
.codex/鍐呭绠＄悊绯荤粺璇︾粏鏋舵瀯.md
.codex/鍐呭绠＄悊绯荤粺鍗囩骇璺嚎.md
```

娑夊強 UI / 鍓嶇 / 椤甸潰 / 浜や簰 / 甯冨眬 / 鏍峰紡鏃堕澶栬鍙栵細

```text
docs/ui/ui-architecture.md
docs/ui/component-rules.md
docs/ui/section-page.md
docs/ui/focus-tree.md
docs/ui/i18n.md
docs/ui/codex-workflow.md
```

娑夊強鍚庣鏁版嵁妯″瀷鏃惰鍙栵細

```text
docs/cms-v2/backend/鍚庣鏁版嵁妯″瀷寮?鍙戞枃妗?.md
docs/cms-v2/backend/棰嗗煙妯″瀷缁撴瀯璇存槑.md
docs/cms-v2/backend/鍚庣鏁版嵁妯″瀷杩涘害.md
docs/cms-v2/backend/鍚庣閲嶅缓闃舵璁″垝.md
```

## 2. 鏂囨。浼樺厛

褰撳墠 V2 鍓嶇閲嶅啓閲囩敤鏂囨。浼樺厛锛?

```text
鍏堢‘璁ら〉闈㈢洰鏍?
鍏堢‘璁ょ粍浠惰竟鐣?
鍏堢‘璁? DTO 鍜? mock 鏁版嵁
鍐嶅疄鐜扮粍浠?
鏈?鍚庢帴 API
```

涓嶈涓?涓婃潵鐩存帴鍐欏鏉傞〉闈€??

## 2.1 姣忚疆鏈?灏忓紑鍙戣鍒?

鍚庣画 UI 寮?鍙戞寜鏈?灏忚疆娆℃帹杩涳細

- 姣忎竴杞彧瀹屾垚鐢ㄦ埛宸茬‘璁よ鍒掍腑鐨勬渶灏忓紑鍙戠洰鏍囥??
- 涓嶉『鎵嬪紑鍙戝悗缁粍浠躲?佸悗缁〉闈€?佸悗缁氦浜掓垨鐪熷疄 API 鎺ュ叆銆?
- 涓嶅洜涓哄彂鐜扮浉閭婚棶棰樺氨鐩存帴鎵╁睍淇敼鑼冨洿锛涚浉閭婚棶棰樺厛璁板綍鍒版眹鎶ユ垨涓嬩竴杞鍒掋??
- 涓嶅仛璁″垝澶栬瑙夌簿淇?佺粨鏋勯噸鎺掋?佹娊璞″崌绾ф垨閲嶆瀯銆?
- 濡傛灉鏈疆闇?瑕佹柊澧炵粍浠讹紝Codex 蹇呴』鍏堢畝瑕佽鏄庣粍浠惰亴璐ｃ?侀潪鑱岃矗銆佽緭鍏ユ暟鎹?佷簨浠惰竟鐣屽拰 ComponentLab 楠屾敹鍦烘櫙锛屽緟鐢ㄦ埛纭鍚庡啀寮?濮嬪疄鐜般??
- 姣忚疆瀹屾垚鍚庯紝蹇呴』璇存槑鏈疆寮?鍙戜簡浠?涔堛?佽闂摢涓湴鍧?楠屾敹銆侀噸鐐规鏌ュ摢浜涘尯鍩熴?佸摢浜涗粛鏄崰浣嶃??

## 3. Mock Data First

缁勪欢寮?鍙戞祦绋嬶細

```text
Define DTO
鈫?
Create Mock Data
鈫?
Build Component
鈫?
Verify in ComponentLabPage
鈫?
Connect API
```

浠讳綍鍙鐢ㄤ笟鍔＄粍浠堕兘搴斿厛鏈? mock 鍦烘櫙銆?

杩欓噷鐨? mock 浠呯敤浜? V2 鍓嶇寮?鍙戜笌缁勪欢楠岃瘉锛屼笉寰楀洖閫?鍒? V1 椤甸潰閲屽仛鎷兼帴寮忓紑鍙戙??

## 3.1 ComponentLab 鐙珛楠屾敹瑙勫垯

ComponentLab 鏄綋鍓嶅紑鍙戣疆娆＄殑鐙珛楠屾敹椤甸潰銆?

瑙勫垯锛?

- `/lab` 蹇呴』浣滀负鐙珛楠屾敹璺敱浣跨敤锛屼笉搴斿寘鍦ㄥ甫涓诲鑸垨宸︿晶瀵艰埅鐨? AppShell 涓??
- 鐢ㄦ埛楠屾敹缁勪欢鎴栭〉闈㈠師鍨嬫椂锛屽簲鐩存帴璁块棶 `/lab`锛岃?屼笉鏄?氳繃涓诲簲鐢ㄥ鑸獥鍙ｆ煡鐪嬨??
- 姣忚疆鍙妸鏈疆闇?瑕侀獙鏀剁殑缁勪欢銆侀〉闈㈡垨瀹屾暣椤甸潰鍘熷瀷鏀惧叆 ComponentLab銆?
- 椤甸潰绾у紑鍙戜篃鍙互鍦? ComponentLab 涓斁鍏ュ畬鏁撮〉闈㈣繘琛? mock 楠屾敹銆?
- 涓婁竴杞棤鍏崇殑缁勪欢鍜屽満鏅簲浠庡綋鍓? ComponentLab 瑙嗗浘涓Щ闄ゃ??
- ComponentLab 涓嶆壙鎷呬笟鍔″鑸?佺湡瀹炴暟鎹伐浣滃彴鎴栨案涔呯粍浠跺睍瑙堥鑱岃矗銆?

## 3.2 瑙嗚瀹炵幇绾︽潫

鍦ㄧ敤鎴疯缁嗘弿杩拌瑙夋牱寮忎慨鏀逛箣鍓嶏紝鍏佽 UI 瑙嗚涓嶇簿淇紝浣嗙姝㈤殢鎰忓彂鎸ユ牱寮忋??

蹇呴』閬靛畧锛?

- 涓嶅湪缁勪欢涓殢鎰忓啓涓?娆℃?ч鑹插?笺??
- 涓嶄娇鐢ㄥぇ闈㈢Н闃村奖銆佹笎鍙樸?佽楗版?ц儗鏅??
- 涓嶄负鐩镐技缁勪欢閲嶅鍐欏濂楁牱寮忋??
- 浼樺厛浣跨敤 shadcn-vue銆乀ailwind spacing銆乥order銆乼ext銆乥ackground token銆?
- 甯冨眬缁撴瀯銆佺粍浠跺眰绾с?佺姸鎬佺被蹇呴』绋冲畾銆?
- 濡傛灉瑙嗚缁嗚妭涓嶇‘瀹氾紝鍏堜娇鐢ㄦ渶绠?鏍峰紡锛屼笉瑕佽嚜琛屽彂鎸ャ??
- 鎵?鏈夊彲澶嶇敤缁勪欢蹇呴』鍏堝湪 ComponentLab 涓敤 mock 鏁版嵁楠屾敹銆?

## 4. 鐘舵?佸喅绛栨祦绋?

鏂板鐘舵?佸墠鍏堝垽鏂細

```text
鍙粰涓?涓粍浠剁敤锛?
  鏀剧粍浠跺唴閮ㄣ??

涓?涓〉闈㈠唴澶氫釜缁勪欢鐢紵
  鏀鹃〉闈㈢姸鎬佹垨椤甸潰 composable銆?

澶氫釜椤甸潰鍏变韩锛?
  璇存槑鍏变韩椤甸潰鍜屽師鍥狅紝鍐嶆斁 Pinia store銆?
```

涓嶈榛樿鎶婄姸鎬佹斁杩? Pinia銆?

## 5. API 鎺ュ叆娴佺▼

API 鎺ュ叆椤哄簭锛?

```text
闃呰 docs/cms-v2/backend/鍚庣鏁版嵁妯″瀷寮?鍙戞枃妗?.md 涓? API 绔偣
瀹氫箟鍓嶇 DTO
鍦? src/apis 涓皝瑁呰姹?
鍦? composable 鎴? page 涓皟鐢?
鐢? loading / empty / error 鐘舵?佽鐩? UI
```

绂佹锛?

- 鍦ㄧ粍浠堕噷鏁ｈ惤纭紪鐮? URL銆?
- 涓烘棫鍓嶇鎺ュ彛鍋氬吋瀹瑰眰銆?
- 浠庡墠绔鍙栨棫 `question-bank.db` 鐩稿叧姒傚康銆?
- 瀵规帴 `/api/棰樺簱瀹炰緥/...`銆?
- 鍦? `棰樺簱鏈湴鏈嶅姟/wwwroot` 涓婄户缁爢鍙? V2 椤甸潰閫昏緫銆?

## 6. UI 楠岃瘉

姣忎釜椤甸潰瀹屾垚鍓嶈嚦灏戞鏌ワ細

- 1440px 瀹藉睆甯冨眬銆?
- 1024px 涓瓑瀹藉害甯冨眬銆?
- 768px 绐勫睆甯冨眬銆?
- 375px 绉诲姩瀹藉害閫?鍖栥??
- 鏂囨湰涓嶆孩鍑烘寜閽拰闈㈡澘銆?
- 宸﹀彸渚ф爮鎶樺彔鍚庝富鍖哄煙鍙敤銆?
- 閿洏鍙搷浣滀富瑕佷氦浜掋??
- 绌虹姸鎬佸拰閿欒鐘舵?佸彲瑙併??

## 7. 涓嶅仛浜嬮」

V2 鍓嶇绗竴闃舵涓嶅仛锛?

- 鍏煎 V1 闈欐?侀〉闈€??
- 澶嶇敤 V1 CSS 缁勪欢绾﹀畾銆?
- 缁х画鎵╁啓 V1 鍓嶇鏂囨。浣滀负褰撳墠瑕佹眰銆?
- Word 娣卞害鍔犺浇椤硅仈鍔ㄣ??
- 澶氫汉鍗忎綔鍜屾潈闄愩??
- PDF 杈撳嚭 UI銆?
- 鍙橀噺鏇挎崲鍜屾潯浠跺唴瀹瑰鏉傜紪杈戝櫒銆?

杩欎簺鑳藉姏鍚庣画鎸夐樁娈佃ˉ鍏呫??


## 5.1 持久化交互规则

后续接入真实 API 时，CMS V2 前端不采用手动保存结构模式，也不采用 optimistic update。

标准流程：

```text
用户动作
↓
页面 / composable 调用 API
↓
后端确认并返回最新数据
↓
前端更新 UI
```

因此：

- 每个需要持久化的动作都必须对应明确 API 调用。
- 前端不能先改业务数据，再等待后端确认。
- 后端失败时，前端保持原业务数据不变。
- 后端成功后，前端以返回数据或重新读取的数据为准。
- “保存结构”不是 SectionPage 的主流程；结构变化应由具体动作即时提交。

## 8. SectionVariant 任务工作流

涉及 `SectionVariant` 的开发任务开始前，必须先确认以下文档：

```text
docs/ui/section-page.md
docs/ui/section-page-development-plan.md
docs/ui/section-page-architecture-plan.md
docs/cms-v2/backend/后端数据模型开发文档.md
docs/cms-v2/backend/领域模型结构说明.md
```

当前定稿规则：

- `SectionVariant` 创建入口只在 `SectionTree` 的 `Section` 根节点右键菜单中。
- 创建流程分两步：元数据 -> 内容选择。
- 默认选择由后端 `selection-preview` 返回。
- 前端提交 `selectedSectionItemIds`。
- 前端不循环调用 AddItem API。
- 创建成功后刷新树和列表，但不自动打开新 Variant。
- 失败时保留草稿和选择，不做 optimistic update。

如果任务只是校正文档：

- 不修改前端代码。
- 不修改后端代码。
- 不新增 API。
- 不改数据库迁移。

如果任务进入组件开发：

- 新增 `CreateSectionVariantPanel`、`VariantSelectionMode` 或候选项组件时，必须先放入 `ComponentLab` 使用 Mock Data 验收。

## 9. Handout 任务工作流

涉及 `HandoutPage`、`HandoutVersion`、`HandoutVersionItem`、`OutputTemplate`、`OutputForm`、`GeneratedFile` 或讲义 Word 生成的开发任务开始前，必须先确认以下文档：

```text
docs/ui/handout-page.md
docs/ui/component-rules.md
docs/ui/ui-architecture.md
docs/cms-v2/backend/后端数据模型开发文档.md
docs/cms-v2/backend/后端重建阶段计划.md
docs/cms-v2/backend/领域模型结构说明.md
```

当前定稿规则：

- `HandoutPage` 以 `HandoutVersion` 为核心对象。
- `HandoutVersionItem` 第一版允许 `SectionVariant`、`AtomicSection`、`ContentBlock`。
- 不允许直接添加 `Section`、`SectionItem`、`AtomicSectionItem`、`TeachingTopic` 或 `ContentBlockVersion`。
- 添加入口只在左侧 `HandoutStructurePanel`。
- 根节点提供“添加到末尾”。
- 顶层 `HandoutVersionItem` 提供“在此后添加”。
- 内部派生节点只读。
- 后端负责插入位置和 `SortOrder` 规整。
- 第一版允许重复添加，但前端必须提示。
- 第一版支持上移、下移、删除引用、`TitleOverride`、`Note`。
- 第一版 `OutputForm` 只允许 Word。
- `OutputTemplate` 是 Word 模板权威，必须保留页眉页脚和样式。
- 讲义生成必须处理结构标题和跨 DOCX 例题编号连续。

Handout 开发阶段必须按以下顺序推进：

```text
H0 冲突审计
H1 正式文档完善
H2 技术调研与编号 Spike
H3 Domain 与 Persistence
H4 Application 编排用例
H5 Workspace Aggregate 与 API
H6 Render Plan 与 Word 生成重构
H7 前端基础和 HandoutIndexPage
H8 HandoutPage 三栏与树
H9 真实编排接入
H10 Output 与 GeneratedFile
H11 端到端上线验收
```

如果任务只是校正文档：

- 不修改前端代码。
- 不修改后端代码。
- 不新增 API。
- 不改数据库迁移。

如果任务进入组件开发：

- 新增 Handout 可复用组件前，先向用户说明组件职责、输入、事件和 ComponentLab 验收范围。
- 组件必须先在 ComponentLab 使用 Mock Data 验收。
- 真实页面接入必须另起一轮或获得用户明确确认。

## 10. 题目结构化与多题导入任务工作流

涉及题目结构化预览、输出 Word 样式重绑定或多题导入时，必须先读取：

```text
docs/cms-v2/backend/题目结构化预览-输出样式重绑定-多题导入开发文档.md
docs/ui/component-rules.md
docs/ui/section-page.md
```

当前已收口口径：

- 结构化解析阶段 `Question` 缺 Stem 是失败，不是 warning；讲义 Word 输出阶段缺 Stem 的题目块会作为 `MissingQuestionStem` 可跳过问题返回，生成时跳过该块。
- Part 输出顺序固定为 `Stem / Answer / Analysis / Hint / Other`。
- 非 `Question` 内容块默认 `NotApplicable`，不生成题目 Parts。
- 输出 Word 样式重绑定严格 Stem-only，不使用 `Other` 兜底。
- 输出预检使用 `POST /api/cms-v2/output-forms/{id}/validate-word-generation`。
- 多题导入主流程是临时 Word session、状态轮询、candidates 查询和批量 confirm。
- Workspace 顶部支持 Section 顶层多题导入，`AtomicSectionPanelBlock` 内部支持 `导入题目`，两者复用 `QuestionImportDialog`。
- `AtomicSectionPanelId` 导入上下文由后端校验 panel / AtomicSection / Section 归属，并在确认后创建归属该 panel 的 `AtomicSectionItem`。

禁止回归：

- 不把多题导入做回本地 `.docx` multipart 上传主流程。
- 不恢复 `/question-import-sessions/{sessionId}/candidates/{candidateId}/confirm` 逐候选确认。
- 不在 `QuestionImportDialog` 中直接调用 API 或创建正式 `ContentBlock`。
- 不在非 panel 导入上下文中伪造 `atomicSectionPanelId`；panel 导入必须来自 `AtomicSectionPanelBlock` 的 `导入题目` 入口，并交由后端校验。

## 11. Phase 5 前端收口验收

当前 `frontend-v2` 只有 `typecheck` 和 `build` 脚本，尚未建立前端单元测试或组件测试脚本。Phase 5 不临时引入新的大测试体系，多题导入前端收口采用以下验收方式：

- 运行 `npm run typecheck`，确认类型层面不再引用上传式 question import 请求或单候选确认 API。
- 运行 `npm run build`，确认 `QuestionImportDialog`、`SectionPage`、i18n 和 API DTO 能一起通过构建。
- 人工验收 `QuestionImportDialog`：启动 session、显示状态、重新打开 Word、取消 session、`ReadyForReview` 后展示候选列表。
- 人工验收候选确认：候选默认可勾选、允许取消勾选、标题允许为空、批量确认只发出 `confirmCandidates`，没有单候选入库按钮。
- 人工验收 `SectionPage`：创建 session 后轮询，读取 candidates，批量确认后刷新当前 Section，并定位首个新增节点。
- 人工验收边界：界面不提供本地 `.docx` 文件上传作为多题导入主入口，不在组件内创建正式 `ContentBlock`、`SectionItem` 或 `AtomicSectionItem`。

## 12. Phase 5 收口执行边界

Phase 5 只做测试、文档和一致性检查，不新增业务能力。

- 不修改 V1、VSTO 或 `Word本地文件操作核心库`。
- 不 stage、commit、reset、checkout。
- 多题导入文档不得再把 `AtomicSectionPanel` 的 non-null `atomicSectionPanelId` 写成待确认或未接入事项。
