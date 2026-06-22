# CMS V2 鍓嶇鏋舵瀯鏂囨。

鏈枃妗ｅ畾涔? CMS V2 鏂板墠绔殑鎬讳綋鏋舵瀯銆?

褰撳墠鍏辫瘑锛?

- 绗竴鐗堝墠绔凡缁忓簾寮冦??
- 绗竴鐗堝悗绔篃宸茬粡搴熷純銆?
- 鏂板墠绔彧鑳藉鎺? CMS V2 鍚庣銆?
- 鏃? V1 UI 鏂囨。鍜屾棫闈欐?侀〉闈㈠彧浣滀负鍘嗗彶鍙傝?冿紝涓嶅啀浣滀负褰撳墠瀹炵幇鍩虹銆?

褰撳墠闃舵 1 鍙仛鏂囨。娓呯悊锛屼笉鍒涘缓鍓嶇宸ョ▼锛屼笉寮?濮嬮〉闈㈠疄鐜般??

## 1. 浜у搧瀹氫綅

CMS V2 鍓嶇涓嶆槸鏅?氶搴? CRUD 鍚庡彴锛岃?屾槸闈㈠悜澶囪鍜岃涔夌敓浜х殑鏁欏缁撴瀯璁捐宸ヤ綔鍙般??

鏍稿績鐩爣锛?

```text
Teaching Structure Design
```

涓嶆槸锛?

```text
Question CRUD
```

鐢ㄦ埛涓昏缁勭粐鐨勬槸锛?

```text
TeachingTopic 鏁欏涓婚
ContentBlock 鍐呭璧勪骇
AtomicSection 鍘熷瓙灏忚妭
Section 灏忚妭缁撴瀯
SectionVariant 灏忚妭鍙樹綋
HandoutVersion 璁蹭箟鐗堟湰
OutputForm 杈撳嚭褰㈠紡
```

## 2. 鍓嶇鎶?鏈爤

V2 鍓嶇鍥哄畾閲囩敤锛?

```text
Vue 3
Vite
Tailwind CSS
shadcn-vue
Pinia
Vue Router
Vue I18n
```

绾︽潫锛?

- 涓嶅紩鍏ラ澶栧ぇ鍨嬫鏋躲??
- 涓嶅鐢? V1 闈欐?侀〉闈㈢粨鏋勪綔涓烘柊鏋舵瀯鍩虹銆?
- 涓嶅吋瀹规棫鍓嶇 API 褰㈢姸銆?
- 鎵?鏈変笟鍔¤姹傚鎺? `/api/cms-v2`銆?
- 涓嶅啀瀵规帴 `/api/棰樺簱瀹炰緥/...`銆?
- 涓嶅啀鎶? `棰樺簱鏈湴鏈嶅姟/wwwroot` 涓嬬殑鏃ч〉闈綔涓虹户缁紨杩涚殑鍓嶇宸ョ▼銆?
- 浼樺厛浣跨敤 shadcn-vue 缁勪欢琛ㄨ揪鎸夐挳銆佽緭鍏ユ銆佸脊绐椼?佷晶鏍忋?佽〃鍗曘?佽彍鍗曘?乀abs銆乀ooltip銆?
- 鍥炬爣浼樺厛浣跨敤 lucide-vue-next銆?

## 3. 鍓嶇鐩綍寤鸿

寤鸿鏂板墠绔綔涓虹嫭绔? V2 宸ョ▼锛屼粠椤圭洰缁撴瀯寮?濮嬪缓璁撅細

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

璇存槑锛?

- `frontend-v2/` 鏄綋鍓嶅缓璁洰褰曞悕銆?
- 鑻ュ悗缁‘瀹氫娇鐢ㄥ叾浠栫洰褰曞悕鎴栭」鐩悕锛屼篃蹇呴』淇濇寔鈥滅嫭绔? V2 宸ョ▼ + 鍙鎺? `/api/cms-v2`鈥濊繖涓?鍘熷垯銆?

鐩綍鑱岃矗锛?

```text
pages
  璺敱椤甸潰锛岃礋璐ｉ〉闈㈢骇鏁版嵁鍔犺浇銆侀〉闈㈢骇鐘舵?併?侀〉闈㈠竷灞?鍜屼笟鍔℃祦绋嬪叆鍙ｃ??

components/presentation
  绾睍绀虹粍浠讹紝涓嶇悊瑙? CMS 涓氬姟锛屼笉璋冪敤 API銆?

components/business
  涓氬姟灞曠ず缁勪欢锛岀悊瑙ｄ笟鍔″璞★紝鍙娇鐢? composable锛屼絾榛樿涓嶇洿鎺ヨ皟鐢? API銆?

components/containers
  鍙鐢ㄤ笟鍔″鍣ㄧ粍浠讹紝鍙姞杞芥暟鎹紝鍙皟鐢? API 鎴? composable銆?

stores
  璺ㄩ〉闈㈠叡浜姸鎬併?傚彧鏈夊涓〉闈㈢湡瀹炲叡浜椂鎵嶅厑璁歌繘鍏? store銆?

apis
  灏佽 HTTP 璇锋眰銆傚彧琛ㄨ揪 V2 API 绔偣鍜? DTO锛屼笉鎵胯浇 UI 鐘舵?併??

composables
  澶嶇敤椤甸潰閫昏緫銆佹煡璇㈤?昏緫銆佺劍鐐规爲閫昏緫銆侀?夋嫨閫昏緫鍜屽眬閮ㄤ笟鍔′氦浜掋??

types
  鍓嶇 DTO銆佽鍥炬ā鍨嬪拰鏋氫妇绫诲瀷銆?

mocks
  缁勪欢寮?鍙戝拰 ComponentLab 浣跨敤鐨勪唬琛ㄦ?? mock 鏁版嵁銆?

labs
  缁勪欢瀹為獙鍜? UI 楠岃瘉椤甸潰銆?
```

## 4. 鐘舵?佸綊灞炶鍒?

鐘舵?佸簲鎸変娇鐢ㄨ寖鍥存斁缃細

```text
Component State
  鍙鍗曚釜缁勪欢浣跨敤銆?

Page State
  琚竴涓〉闈㈠唴澶氫釜缁勪欢浣跨敤銆?

Store State
  琚涓〉闈㈠叡浜??
```

濡傛灉鍑嗗鎶婄姸鎬佹斁鍏? Pinia store锛屽繀椤诲厛璇存槑锛?

```text
鍝簺椤甸潰浼氫娇鐢ㄥ畠锛?
涓轰粈涔堝畠蹇呴』璺ㄩ〉闈㈠叡浜紵
```

娌℃湁鏄庣‘璺ㄩ〉闈㈠叡浜渶姹傛椂锛屼笉瑕侀粯璁よ繘 store銆?

## 5. API 璋冪敤杈圭晫

API 璋冪敤瑙勫垯锛?

```text
Presentation Components
  绂佹璋冪敤 API銆?

Business Components
  浼樺厛閫氳繃 composables 鑾峰彇鏁版嵁鎴栬Е鍙戣涓恒??

Business Container Components
  鍏佽璋冪敤 API銆?

Pages
  鍏佽璋冪敤 API銆?

Composables
  鍏佽璋冪敤 API銆?

Stores
  鍙湁璺ㄩ〉闈㈠叡浜姸鎬侀渶瑕佹椂鎵嶅厑璁歌皟鐢? API銆?
```

API 灏佽缁熶竴鏀惧湪 `src/apis/`銆傞〉闈㈠拰 composable 涓嶇洿鎺ユ暎钀? `fetch` URL 瀛楃涓层??

## 6. 璺敱寤鸿

绗竴杞矾鐢卞缓璁細

```text
/topics
  鏁欏涓婚宸ヤ綔鍙般??

/sections/:sectionId
  灏忚妭缁撴瀯缂栬緫椤点??

/handouts/:handoutVersionId
  璁蹭箟鐗堟湰缂栨帓椤点??

/content-blocks
  鍐呭璧勪骇搴撱??

/content-blocks/:contentBlockId
  鍐呭鍧楄鎯呫??

/outputs/:outputFormId
  杈撳嚭褰㈠紡鍜岀敓鎴愯褰曘??

/lab
  ComponentLabPage锛屼綔涓虹嫭绔嬮獙鏀堕〉闈紝鐢ㄤ簬褰撳墠寮?鍙戣疆娆＄殑缁勪欢銆侀〉闈㈠拰 mock 楠岃瘉銆?
```

璇存槑锛?

- `/lab` 涓嶅睘浜庝笟鍔″伐浣滃彴涓诲鑸紝涓嶅簲鍖呭湪涓诲簲鐢? AppShell 鎴栧乏渚у鑸腑銆?
- `/lab` 鐢ㄤ簬璁╃敤鎴风洿鎺ラ獙鏀舵湰杞紑鍙戝唴瀹癸紱姣忚疆鍙繚鐣欐湰杞渶瑕佺‘璁ょ殑缁勪欢銆侀〉闈㈡垨瀹屾暣椤甸潰 mock銆?
- 鐪熷疄涓氬姟椤甸潰浠嶄娇鐢ㄦ甯稿簲鐢ㄥ澹筹紱ComponentLabPage 鍙礋璐ｅ紑鍙戦獙鏀讹紝涓嶆壙鎷呬笟鍔″鑸亴璐ｃ??

## 7. 瑙嗚鏂瑰悜

CMS V2 鏄珮棰戝伐浣滃彴锛屼笉鍋氳惀閿?椤靛拰灞曠ず绔欍??

璁捐鍘熷垯锛?

- 淇℃伅瀵嗗害閫備腑锛屼紭鍏堟敮鎸佹壂鎻忋?佹瘮杈冦?佺紪鎺掑拰蹇?熷畾浣嶃??
- 涓ぎ宸ヤ綔鍖哄缁堟槸瑙嗚閲嶅績銆?
- 宸﹀彸渚ф爮鐢ㄤ簬瀵艰埅銆佺粨鏋勩?佹鏌ュ櫒锛屼笉鎶富宸ヤ綔鍖烘敞鎰忓姏銆?
- 浣跨敤绋冲畾灏哄鍜屾槑纭竷灞?锛屽噺灏? hover 鎴栧姩鎬佸唴瀹归?犳垚鐨勫竷灞?璺冲姩銆?
- 鎸夐挳浣跨敤娓呮櫚鍥炬爣鍜岀煭鏂囨湰锛屽鏉傛搷浣滆繘鍏ヨ彍鍗曘?丏rawer 鎴? Dialog銆?
- 涓嶄娇鐢ㄨ楗版?уぇ娓愬彉銆佽惀閿?寮? hero銆佸じ寮犲崱鐗囧爢鍙犮??
- 鍦ㄧ敤鎴锋槑纭姹傝瑙夌簿淇墠锛岄粯璁や娇鐢ㄦ渶绠?鏍峰紡锛屼笉鑷琛ュ厖瑁呴グ鎬ц瑙夈??
- 浼樺厛浣跨敤 shadcn-vue 涓? Tailwind token锛屼笉鍦ㄧ粍浠朵腑鏁ｈ惤涓?娆℃?ч鑹插?笺??

鎺ㄨ崘鍩鸿皟锛?

```text
涓撲笟銆佹竻鏅般?佸畨闈欍?佹湁缁勭粐鎰熴??
涓昏壊鍙娇鐢ㄨ摑鑹茬郴琛ㄨ揪缁撴瀯鍜屽彲闈犳?э紝杈呰壊鐢ㄤ簬鐘舵?佷笌鎿嶄綔鍖哄垎銆?
閬垮厤鏁寸珯鍙湁鍗曚竴钃濈传鑹茶皟銆?
```

## 8. 鍙闂?ц姹?

- 浣跨敤璇箟鍏冪礌锛歚button`銆乣nav`銆乣main`銆乣aside`銆乣section`銆?
- 鎵?鏈夊彲鐐瑰嚮鍏冪礌蹇呴』鍙敭鐩樿闂??
- 鍔ㄦ?佸睍寮?鐘舵?佸繀椤荤粦瀹? `aria-expanded`銆?
- Dialog銆丏rawer銆丏ropdown 蹇呴』鍏峰鐒︾偣绠＄悊銆?
- 鎵?鏈? hover 琛屼负闇?瑕佹湁閿洏绛変环鎿嶄綔銆?
- 閬靛畧 `prefers-reduced-motion`銆?


## 5.1 Server-confirmed Update 边界

当前 V2 前端采用 server-confirmed update 作为持久化交互原则。

- 前端负责表达用户意图和维护 UI 状态。
- 后端负责确认所有需要持久化的业务变化。
- 业务数据视图必须以后端确认后的返回结果或重新读取结果为准。
- 不采用 optimistic update。
- 不采用前端本地积累修改后手动保存的结构编辑模式。

## 5.2 SectionVariant 创建模式

`SectionVariant` 创建属于 `SectionPage` 内的临时工作模式，不是独立业务页面，也不是 TeachingStructureTree 的默认职责。

当前定稿：

- 唯一入口是 `SectionTree` 的 `Section` 根节点右键菜单。
- 入口文本为“新建 SectionVariant”或等价中文动作。
- 创建流程由 `SectionPage` 持有状态。
- 创建第一步填写元数据。
- 创建第二步进入 `VariantSelectionMode`，选择顶层 `SectionItem`。
- 成功后刷新当前 `Section` 数据和 Variant 展示，但不自动打开新 Variant。

页面职责：

```text
SectionTree
  触发 createSectionVariant 事件。

SectionPage
  持有创建流程状态。
  调用 selection-preview。
  调用 create SectionVariant。
  刷新 server-confirmed 数据。

SectionWorkspace
  在 VariantSelectionMode 中展示候选和勾选态。
```

不允许：

- 从 Toolbar 创建。
- 从 Workspace 普通插入点创建。
- 从 Inspector 创建。
- 从 TeachingStructureTree 创建。
- 在前端循环调用 AddItem API 拼装 Variant。
