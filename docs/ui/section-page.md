# SectionPage 灏忚妭缁撴瀯缂栬緫椤?

SectionPage 鏄? CMS V2 鍓嶇鐨勬牳蹇冮〉闈箣涓?銆傚畠鐨勭洰鏍囨槸鏁欏缁撴瀯缂栬緫锛屼笉鏄潗鏂欐敹闆嗛〉锛屼篃涓嶆槸棰樼洰鍒楄〃椤点??

## 1. 椤甸潰鐩殑

SectionPage 鍥炵瓟鐨勯棶棰橈細

```text
杩欎釜鏁欏涓婚搴旇鎬庝箞璁诧紵
```

椤甸潰鏍稿績浠诲姟锛?

- 缂栨帓 Section 鍐呴儴缁撴瀯銆?
- 缁勭粐 ContentBlock 鍜? AtomicSection銆?
- 閫夋嫨鎴栧垱寤? SectionVariant銆?
- 绠＄悊 SectionItem 鐨勯『搴忋?佸眰绾у拰寮曠敤妯″紡銆?
- 鍦ㄧ粨鏋勭紪杈戣繃绋嬩腑蹇?熸煡鐪嬪唴瀹瑰潡棰勮鍜屽厓鏁版嵁銆?

## 2. 鍐呭妯″瀷鐞嗚В

### ContentBlock

鍙紪杈戝唴瀹瑰崟鍏冦??

绀轰緥锛?

```text
Question
Knowledge Point
Summary
Note
Method Explanation
```

鐗圭偣锛?

- 闀挎湡淇濆瓨涓? `.docx` 鍐呭璧勪骇銆?
- 鍙互鎵撳紑 Word 绮剧粏缂栬緫銆?
- 鍙互鐢熸垚 HTML 棰勮銆?
- 鍙互琚? Section銆丄tomicSection銆丠andout 寮曠敤銆?

### AtomicSection

鏈?灏忔暀瀛︾粨鏋勫崟鍏冦??

鐗圭偣锛?

- 缁勭粐澶氫釜 ContentBlock銆?
- 鑷韩涓嶇洿鎺ュ寘鍚彲缂栬緫鏂囨。姝ｆ枃銆?
- 涓嶅祵濂? AtomicSection銆?
- 閫傚悎娌夋穩鍙鐢ㄧ殑璁茶В鐗囨銆?

### Section

鏁欏缁勭粐缁撴瀯銆?

鐗圭偣锛?

- 灞炰簬 TeachingTopic銆?
- 鐢? SectionItem 缁勬垚銆?
- SectionItem 鍙互寮曠敤 ContentBlock 鎴? AtomicSection銆?
- Section 涓嶇洿鎺ョ紪杈戞枃妗ｅ唴瀹癸紝瀹冪紪杈戞暀瀛︾粨鏋勩??

## 3. 椤甸潰甯冨眬

SectionPage 浣跨敤涓夋寮忓伐浣滃彴锛?

```text
Toolbar

Left:
SectionStructurePanel

Center:
SectionWorkspace

Right:
SectionInspector
```

宸︿晶 `SectionStructurePanel`锛?

```text
鍥哄畾瀹藉害
鍙姌鍙?
鍙仠闈?
鏄剧ず褰撳墠 Section 鐨勭粨鏋勬爲
鐢ㄤ簬蹇?熷畾浣嶅拰璋冩暣缁撴瀯
```

涓棿 `SectionWorkspace`锛?

```text
寮规?у搴?
涓诲伐浣滃尯
鍗犵敤鏈?澶х┖闂?
鏄剧ず灞曞紑鍚庣殑鏁欏鍐呭缁撴瀯
鎵胯浇涓昏缂栬緫鎿嶄綔
```

鍙充晶 `SectionInspector`锛?

```text
鍥哄畾瀹藉害
鍙姌鍙?
鍙仠闈?
鏄剧ず褰撳墠閫変腑鑺傜偣璇︽儏
鎻愪緵鐗堟湰銆侀瑙堛?佸娉ㄣ?佸紩鐢ㄦā寮忕瓑涓婁笅鏂囨搷浣?
```

鍘熷垯锛?

- 涓棿宸ヤ綔鍖哄繀椤昏幏寰楁渶澶氭敞鎰忓姏銆?
- 宸﹀彸渚ф爮鏈嶅姟浜庣粨鏋勫畾浣嶅拰缁嗚妭妫?鏌ワ紝涓嶅簲鎴愪负涓绘搷浣滃尯鍩熴??
- 椤甸潰涓嶅簲鍑虹幇鍥涙爮甯搁┗甯冨眬銆?

## 4. Toolbar 鑱岃矗

Toolbar 鎻愪緵椤甸潰绾у懡浠わ細

```text
杩斿洖涓婚宸ヤ綔鍙?
鍒囨崲 SectionVariant
鏂板缓 AtomicSection
鎻掑叆 ContentBlock
鎵撳紑鍐呭閫夋嫨鍣?
鎵撳紑浜岀骇宸ヤ綔娴? Drawer
淇濆瓨缁撴瀯
鍒锋柊棰勮
```

Toolbar 涓嶆壙杞藉ぇ閲忕瓫閫夊櫒銆傚鏉傜瓫閫夎繘鍏? Drawer 鎴? Dialog銆?

## 5. Secondary Workflows

浠ヤ笅鍖哄煙涓嶅簲鎴愪负 SectionPage 鐨勫父椹讳富鍖哄煙锛?

```text
Question Staging Area
Pending Atomic Sections
Temporary Collections
Content Import Queue
```

瀹冧滑灞炰簬浜岀骇宸ヤ綔娴侊紝搴旈?氳繃浠ヤ笅鏂瑰紡杩涘叆锛?

```text
Toolbar Entry
Drawer
Dialog
Secondary Panel
```

鍘熷洜锛?

- SectionPage 鐨勪富浠诲姟鏄紪杈戞暀瀛︾粨鏋勩??
- 鏆傚瓨鍖哄拰鏉愭枡鏀堕泦浼氬垎鏁ｇ粨鏋勭紪杈戞敞鎰忓姏銆?
- 鐢ㄦ埛搴旇兘鍦ㄩ渶瑕佹椂鎵撳紑杈呭姪娴佺▼锛岀敤瀹屽悗鍏抽棴銆?

## 6. 浜や簰瑙勫垯

### 6.1 閫夋嫨鑺傜偣

鐢ㄦ埛鐐瑰嚮缁撴瀯鏍戞垨宸ヤ綔鍖轰腑鐨勮妭鐐规椂锛?

```text
鏇存柊褰撳墠 selectedNode
涓棿宸ヤ綔鍖烘粴鍔ㄥ埌瀵瑰簲鑺傜偣
鍙充晶 Inspector 鏄剧ず鑺傜偣璇︽儏
```

### 6.2 鎻掑叆鍐呭

鎻掑叆鍐呭浣跨敤鏄庣‘鍏ュ彛锛?

```text
鍦ㄩ?変腑鑺傜偣鍓嶆彃鍏?
鍦ㄩ?変腑鑺傜偣鍚庢彃鍏?
浣滀负瀛愰」鎻掑叆
杩藉姞鍒版湯灏?
```

濡傛灉鐩爣鏄? ContentBlock锛屾墦寮? `ContentBlockPicker`銆?  
濡傛灉鐩爣鏄? AtomicSection锛屾墦寮? `AtomicSectionPicker` 鎴栨柊寤? Dialog銆?

褰撳墠宸茬‘璁ょ殑 InsertPoint 浜や簰瑙勫垯锛?

```text
InsertPoint 涓嶅啀浣跨敤鍗曚釜鈥滄彃鍏モ?濇寜閽??

InsertPoint 鐩存帴灞曠ず涓変釜鍏蜂綋鎿嶄綔锛?

1. 鏂板缓 ContentBlock
2. 鏂板缓 AtomicSection
3. 鎻掑叆宸叉湁鍧?
```

瑙勫垯锛?

- 鐐瑰嚮鈥滄柊寤? ContentBlock鈥濊繘鍏ユ柊寤? ContentBlock 娴佺▼銆?
- 鐐瑰嚮鈥滄柊寤? AtomicSection鈥濊繘鍏ユ柊寤? AtomicSection 娴佺▼銆?
- 鐐瑰嚮鈥滄彃鍏ュ凡鏈夊潡鈥濇墦寮?鍚庣画鍗曠嫭寮?鍙戠殑鍧楁悳绱㈢粍浠躲??
- 鍧楁悳绱㈢粍浠剁殑鎼滅储鑼冨洿蹇呴』鍚屾椂鍖呭惈 ContentBlock 鍜? AtomicSection銆?
- 鍧楁悳绱㈢粍浠舵湰杞笉瀹炵幇锛涘悗缁繀椤诲厛鍦? ComponentLab 涓敤 Mock Data 楠屾敹锛屽啀鎺ュ叆 SectionPage銆?

鏂板缓 ContentBlock / AtomicSection 鐨勬彃鍏ユ祦绋嬩娇鐢? `InsertCreateOverlay`銆?

灞曠ず瑙勫垯锛?

- 鐐瑰嚮鈥滄柊寤? ContentBlock鈥濇垨鈥滄柊寤? AtomicSection鈥濆悗锛屾墦寮?鏈?涓婂眰鎻掑叆闈㈡澘銆?
- 闈㈡澘鎵撳紑鏃讹紝鑳屽悗鐨勬暣涓? SectionPage 妯＄硦銆?
- 闈㈡澘涓嶆斁鍦ㄥ彸渚? Inspector銆?
- 闈㈡澘涓嶄綔涓烘櫘閫氭枃妗ｆ祦鍐呰仈鍧椼??
- ComponentLab 涓娇鐢? Mock Data 楠屾敹闈㈡澘锛涙帴鍏? SectionPage 鍚庯紝鎻愪氦蹇呴』璋冪敤 CMS V2 API 鍒涘缓鐪熷疄瀵硅薄骞堕噸鏂拌鍙? Section 鏁版嵁銆?

瀛楁瑙勫垯锛?

褰? targetType = ContentBlock锛?

- 鎵?灞? Section锛氶粯璁ゆ樉绀哄綋鍓? Section 鍚嶇О锛屽綋鍓嶉樁娈典笉鍙慨鏀癸紱鎻愪氦鏃跺悗绔褰曞綋鍓? SectionId銆?
- 鍚嶇О锛屽彲閫夛紱娌℃湁鍚嶇О鏃朵笉闃绘鏂板缓
- 绫诲瀷锛氱煡璇嗙偣 / 渚嬮 / 鍙樺紡棰? / 缁冧範棰? / 鍙樺紡棰樼粍 / 缁冧範棰樼粍
- 闅惧害锛氬熀纭? / 涓。 / 鎻愰珮 / 鍘嬭酱

褰? targetType = AtomicSection锛?

- 鎵?灞? Section锛氶粯璁ゆ樉绀哄綋鍓? Section 鍚嶇О锛屽綋鍓嶉樁娈典笉鍙慨鏀癸紱鎻愪氦鏃跺悗绔褰曞綋鍓? SectionId銆?
- 鍚嶇О
- 闅惧害锛氬熀纭? / 涓。 / 鎻愰珮 / 鍘嬭酱
- 澶囨敞锛屽彲閫?

瀛楁璇存槑锛?

- 鎵?灞? Section 鍦? UI 涓樉绀哄悕绉帮紝浣嗘寔涔呭寲浣跨敤 SectionId锛岄伩鍏? Section 鏀瑰悕鍚庡綊灞炴柇瑁傘??
- ContentBlock 鍚嶇О鍙负绌猴紱灞曠ず鏃剁敱绫诲瀷銆侀瑙堟憳瑕佹垨涓婁笅鏂囧厹搴曘??
- AtomicSection 鍚嶇О浠嶅繀濉紝闅惧害鏄嫭绔嬪瓧娈碉紝涓嶆槧灏勫埌 Description銆?
- 杩欎簺瀛楁鏄綋鍓嶆彃鍏ラ潰鏉跨殑鏈?灏忓垱寤哄瓧娈碉紝涓嶄唬琛ㄥ悗缁畬鏁寸紪杈? DTO 宸茬粡鍥哄畾銆?

### 6.3 璋冩暣椤哄簭

绗竴鐗堜紭鍏堟敮鎸佺ǔ瀹氭搷浣滐細

```text
涓婄Щ
涓嬬Щ
缂╄繘
鍙栨秷缂╄繘
绉婚櫎寮曠敤
```

鎷栨嫿鎺掑簭鍙互鍚庣画鍐嶅仛锛屼笉浣滀负绗竴鐗堝繀瑕佹潯浠躲??

### 6.4 寮曠敤妯″紡

ContentBlock 寮曠敤蹇呴』鏄剧ず锛?

```text
FollowLatest
LockedVersion
```

褰撻攣瀹氱増鏈椂锛屽繀椤绘樉绀虹増鏈彿鎴栫増鏈? ID銆?  
AtomicSection 寮曠敤涓嶅簲鐢? ContentBlock 鐗堟湰閿佸畾銆?

## 7. 椤甸潰鐘舵?佸缓璁?

Page State锛?

```text
currentSection
sectionItems
sectionVariants
activeVariantId
selectedNodeId
selectedNodeType
expandedNodeIds
dirtyState
loadingState
errorState
```

Store State 鍙湁鍦ㄥ涓〉闈㈠叡浜椂鎵嶈?冭檻锛屼緥濡傦細

```text
currentTeachingTopicId
globalBankStatus
userUiPreferences
```

濡傞渶寮曞叆 store锛屽繀椤诲厛璇存槑璺ㄩ〉闈娇鐢ㄥ満鏅??


### 6.5 SectionTree 右键菜单

SectionTree 支持节点右键上下文菜单，并且必须覆盖浏览器原生右键菜单。

交互规则：

- 右键节点时，不默认选中该节点。
- 右键节点时，只把该节点作为 context target 临时高亮。
- 当前 selectedNodeId 保持不变。
- 右侧 Inspector 保持当前选中节点详情不变。
- Workspace 当前选中态保持不变。
- 点击菜单项后，由页面或父级容器决定后续动作。
- 点击左键选择节点时，才更新 selectedNodeId、Inspector 和 Workspace 选中态。

第一版菜单项：

1. 新建 ContentBlock
2. 新建 AtomicSection
3. 插入已有块
4. 移除

当前不进入右键菜单的操作：

- 上移
- 下移
- 缩进
- 反缩进

本能力必须先在 ComponentLab 中使用 Mock Data 验收，再接入 SectionPage。

## 当前补充约定：SectionPage 持久化更新模式

SectionPage 后续接入真实 API 时，采用 server-confirmed update 模式。

也就是说：

```text
用户触发需要持久化的动作
↓
前端调用 /api/cms-v2
↓
后端完成修改并返回最新数据
↓
前端用后端返回的数据更新页面
```

规则：

- 不采用前端手动保存结构模式。
- 不采用 optimistic update。
- 不允许前端先把结构、顺序、引用关系或字段值改成本地成功态，再等待后端确认。
- API 失败时，前端必须保持原有已确认数据不变，并显示错误提示。
- API 成功时，前端必须以后端返回的最新 Section / SectionItem / ContentBlock / AtomicSection 数据为准重新渲染。
- 如果后端接口暂时只返回操作结果而不返回聚合数据，前端必须在成功后重新读取对应聚合数据，再更新页面。
- `保存结构` 不再作为 SectionPage 主流程命令；新增、插入、移除、排序、字段编辑等动作各自触发对应 API。
- `dirtyState` 不作为“待保存结构”的核心状态；只允许用于非持久化 UI 提示，或未来明确设计的草稿能力。

需要持久化的典型动作：

- 新建 ContentBlock。
- 新建 AtomicSection。
- 插入已有块。
- 移除 SectionItem。
- 调整 SectionItem 顺序或层级。
- 修改 ContentBlock / AtomicSection / SectionItem 字段。

不需要持久化的 UI 状态：

- 当前选中节点。
- 展开 / 折叠状态。
- hover 状态。
- 右键菜单临时目标。
- 插入面板打开状态。
- 表单提交前的临时输入。

## 褰撳墠琛ュ厖绾﹀畾锛欳ontentBlock Word 缂栬緫鍏ュ彛

`SectionPage` 涓殑 `ContentBlock` 鎿嶄綔鍖洪渶瑕佹彁渚? Word 缂栬緫鍏ュ彛锛屼絾鍓嶇涓嶇洿鎺ユ墦寮?鏈湴 DOCX銆乣ms-word:` URI 鎴栦换浣曟搷浣滅郴缁熻矾寰勩??

鍥哄畾杈圭晫锛?

- `ContentBlockDisplay` / `SectionItemView` 鍙礋璐ｆ樉绀烘寜閽苟 emit Word 缂栬緫鎰忓浘銆?
- `SectionPage` 鎴栭〉闈㈢骇 composable 璐熻矗璋冪敤 CMS V2 鍚庣 API銆?
- 鍚庣蹇呴』鎻愪緵绋冲畾鐨? `ContentBlock` 缂栬緫浼氳瘽鎺ュ彛銆?
- 鏈湴鎵撳紑 Word銆佹湭鏉ヤ簯绔紪杈戙?佸閮? URI 璺宠浆绛夊疄鐜板樊寮傦紝蹇呴』灏佽鍦ㄥ悗绔瓥鐣ヤ腑銆?
- 鍓嶇鍙叧蹇冧細璇濆垱寤恒?佺姸鎬併?佸悓姝ャ?佸彇娑堝拰閿欒鎻愮ず銆?
- 涓嶅厑璁稿湪 V2 鍓嶇涓皟鐢? V1 `缂栬緫浼氳瘽` 鎺ュ彛銆?
- 涓嶅厑璁稿湪 V2 鍓嶇涓嫾鎺? `/api/棰樺簱瀹炰緥/...`銆?

鎺ㄨ崘鍚庣 API 璇箟锛?

```text
POST /api/cms-v2/content-blocks/{contentBlockId}/edit-session
GET  /api/cms-v2/content-block-edit-sessions/{sessionId}
POST /api/cms-v2/content-block-edit-sessions/{sessionId}/sync
POST /api/cms-v2/content-block-edit-sessions/{sessionId}/cancel
```

绗竴鐗? `ContentBlock` 鎿嶄綔鍖烘帴鍏ョ湡瀹炲姩浣滄椂锛學ord 缂栬緫鎸夐挳搴旂瓑寰呬笂杩? V2 API 瀹屾垚鍚庡啀鎺ュ叆鐪熷疄琛屼负銆?

## 褰撳墠琛ュ厖绾﹀畾锛歋ectionPage 鍔ㄤ綔缂栨帓灞?

`SectionPage` 闇?瑕佸尯鍒嗕笁灞傝亴璐ｏ細

```text
灞曠ず缁勪欢
  鍙? emit 浜嬩欢

SectionPage 椤甸潰缂栨帓
  鎺ユ敹浜嬩欢锛屾彁渚涘綋鍓? Section / 閫変腑鑺傜偣 / 鎻掑叆涓婁笅鏂?

Action composables
  缁熶竴鎵ц鐪熷疄鍔ㄤ綔锛岃皟鐢? CMS V2 API锛屽埛鏂版暟鎹苟璁剧疆鍙嶉
```

鍥犳锛?

- Workspace 涓殑鍒犻櫎銆佺Щ鍔ㄣ?侀噸鍛藉悕銆佹柊寤哄瓙鍧楋紝涓嶅簲鐩存帴鍐欏湪 `SectionItemView` 鎴? `AtomicSectionBlock` 閲屻??
- SectionTree 鍙抽敭鑿滃崟浠ュ悗涔熶細瑙﹀彂鍒犻櫎銆佹柊澧炪?侀噸鍛藉悕绛夊姩浣滐紝蹇呴』澶嶇敤鍚屼竴濂? action composable銆?
- Inspector 鍚庣画濡傛灉鎻愪緵鎿嶄綔鎸夐挳锛屼篃蹇呴』澶嶇敤鍚屼竴濂? action composable銆?
- 蹇嵎閿悗缁鏋滆Е鍙戞搷浣滐紝涔熷繀椤诲鐢ㄥ悓涓?濂? action composable銆?

绗竴鎵瑰缓璁娊鍙栵細

```text
useSectionItemActions
useAtomicSectionActions
useContentBlockActions
```

鍒犻櫎璇箟蹇呴』绮剧‘锛?

- 浠? Workspace 绉婚櫎 AtomicSection 褰撳墠椤? = `removeSectionItemReference`銆?
- 鍒犻櫎 AtomicSection 鏈綋 = `deleteAtomicSectionEntity`锛屽綋鍓嶄笉浣滀负 Workspace 榛樿鍔ㄤ綔銆?
- 浠? AtomicSection 鍐呴儴绉婚櫎 ContentBlock = `removeAtomicSectionChildItem`銆?

璇ヨ鍒欑敤浜庨伩鍏? Workspace銆丼ectionTree銆両nspector 鍚勮嚜澶嶅埗涓?濂楃湡瀹炲姩浣滈?昏緫銆?

## 鍚庣画寰呭疄鐜帮細绉婚櫎寮曠敤鍚庣殑绌哄３瀵硅薄娓呯悊

鐘舵?侊細Planned / Deferred锛屽綋鍓嶄笉瀹炵幇銆?

鑳屾櫙锛?

- `SectionPage` 褰撳墠鍒犻櫎鍔ㄤ綔璇箟浠嶆槸鈥滅Щ闄ゅ紩鐢ㄢ?濄??
- 鍚庣画甯屾湜鍦ㄧЩ闄ゅ紩鐢ㄥ悗锛岀敱鍚庣鍒ゆ柇琚Щ闄ゅ璞℃槸鍚︿负绌哄３銆?
- 濡傛灉鐩爣瀵硅薄涓虹┖澹筹紝骞朵笖涓嶅啀琚叾浠栧璞″紩鐢紝鍚庣鍙互鍚屾椂鍒犻櫎璇ュ璞℃湰浣撱??
- 璇ラ?昏緫娑夊強寮曠敤缁熻銆佹枃浠惰祫浜с?佺増鏈?佺粍鍚堝叧绯诲拰鍒犻櫎椤哄簭锛屽鏄撳紩鍏ヨ鍒狅紝鍥犳鍏堣褰曚负鍚庣画涓撻」鑳藉姏銆?

鐩爣璇箟锛?

1. 鍓嶇浠嶅彧鎻愪氦鈥滅Щ闄ゅ紩鐢ㄢ?濊姹傦紝涓嶅湪鍓嶇鍒ゆ柇鐩爣瀵硅薄鏄惁涓虹┖銆?
2. 鍚庣搴旂敤灞傚湪鍚屼竴涓氬姟鐢ㄤ緥涓墽琛岋細
   - 鍒犻櫎褰撳墠寮曠敤銆?
   - 鍒ゆ柇琚Щ闄ょ洰鏍囨槸鍚︿负绌哄３銆?
   - 鍒ゆ柇琚Щ闄ょ洰鏍囨槸鍚︿粛琚叾浠栧璞″紩鐢ㄣ??
   - 鍙湁鈥滅┖澹? + 鏃犲叾浠栧紩鐢ㄢ?濆悓鏃舵垚绔嬫椂锛屾墠鍒犻櫎鐩爣瀵硅薄鏈綋銆?
3. 鍚庣画 API 杩斿洖缁撴灉搴旇兘琛ㄨ揪锛?
   - `removedReference`
   - `deletedEmptyTarget`
   - `deletedTargetType`
   - `deletedTargetId`

鍊欓?夋竻鐞嗚鍒欙細

- `ContentBlock`锛氬彧鏈夊湪鏃? `ContentBlockVersion` / 鏃? DOCX 鍐呭璧勪骇銆佹棤 `ContentBlockRelation` children锛屽苟涓旀病鏈? `SectionItem`銆乣AtomicSectionItem`銆乣ContentBlockRelation`銆乣HandoutVersionItem` 绛夊叾浠栧紩鐢ㄦ椂锛屾墠鍏佽娓呯悊銆?
- `AtomicSection`锛氬彧鏈夊湪鏃? `AtomicSectionItem`锛屽苟涓旀病鏈夊叾浠? `SectionItem` 寮曠敤鏃讹紝鎵嶅厑璁告竻鐞嗐??
- `CompositeBlock` 鏈川浠嶆槸 `ContentBlock`锛涘鏋滀綔涓? child relation 琚Щ闄わ紝娓呯悊瑙勫垯鎸? `ContentBlock` 鎵ц銆?
- 鍒犻櫎 parent `CompositeBlock` 鑷韩涓嶅簲鐢卞垹闄ゅ叾鍐呴儴 child relation 鑷姩瑙﹀彂銆?

杈圭晫锛?

- 涓嶆敼鍙樺睍绀虹粍浠惰亴璐ｃ??
- 涓嶅厑璁稿墠绔洿鎺ュ垹闄ゅ疄浣撱??
- 涓嶅厑璁镐粎鍑爣棰樹负绌哄垽鏂? `ContentBlock` 涓虹┖銆?
- 涓嶅厑璁稿垹闄ゅ凡鏈夌増鏈?丏OCX銆丠TML銆丳lainText 鎴栬鍏朵粬缁撴瀯寮曠敤鐨勫唴瀹硅祫浜с??
- 鍚庣画瀹炵幇鍓嶅繀椤诲厛琛ュ悗绔敤渚嬫祴璇曪紝鍐嶅疄鐜颁笟鍔￠?昏緫銆?
## 当前补充约定：多个顶层块升级为 AtomicSection

SectionPage 支持在 Workspace 中把多个连续的顶层块升级为一个新的 AtomicSection。

交互规则：

- 入口只出现在中间 Workspace，不放在 SectionTree、Inspector 或右键菜单中。
- 用户通过 Shift + click 连续选择顶层 ContentBlock / CompositeBlock。
- CompositeBlock 本质仍是 ContentBlock，因此允许参与升级。
- 已有 AtomicSection 不允许参与升级。
- 非连续选择、不足两个块、包含 AtomicSection 的选择都不显示有效升级入口，或显示轻量错误提示。
- 点击“升级为 AtomicSection”后打开 InsertCreateOverlay，填写 AtomicSection 名称、难度和备注。
- AtomicSection 名称必填。
- 提交期间整个 SectionPage 顶层阻塞并显示“正在升级为 AtomicSection”，期间禁止 Workspace、SectionTree、右键菜单、插入、删除、移动和 Word 编辑等操作。
- 成功后重新读取 Section 数据，不做 optimistic update。
- 失败时保持原数据不变，由后端事务回滚保证不留下半成品。
- 成功后的撤销能力本轮不实现。

数据规则：

- 前端只提交一次 CMS V2 API 调用，不在前端串联“新建 AtomicSection、移动 item、删除旧引用”等多个 API。
- 后端 API 必须在一个事务中完成创建 AtomicSection、创建 AtomicSectionItem、删除旧 SectionItem 引用、插入新 SectionItem 和规整 SortOrder。
- 删除旧 SectionItem 只删除引用，不删除源 ContentBlock、DOCX、版本或组合关系。
## 当前修订：Workspace 升级为 as 交互

本节覆盖旧的 `Shift + click` 连续选择规则。后续用户口头说 `as` 时，均表示 `AtomicSection` / 原子小节。

交互入口：

- `Workspace` 顶部右侧放置 `升级为 as` 按钮。
- 默认状态下，点击该按钮进入升级状态。
- 进入升级状态后，按钮文案切换为确认动作，例如 `确认升级为 as`。
- 升级状态下同时显示已选数量，并提供 `清空选择`、`退出升级` 这类轻量操作。

块选择规则：

- 升级状态下，点击可升级块会切换选择状态。
- 点击未选中的可升级块：加入选择并高亮。
- 再次点击已选中的块：取消选择。
- 允许不连续选择。
- 已有 `as` / `AtomicSection` 块不可选，不参与升级。
- `ContentBlock` 与组类型 `CompositeBlock` 可以参与升级；`CompositeBlock` 本质仍是 `ContentBlock`。
- 点击空白区域不清空选择，避免误操作。
- 按 `Esc` 可以退出升级状态并清空选择。

确认与面板：

- 只有点击顶部 `确认升级为 as` 按钮，才打开升级确认面板。
- 块本身的点击行为只负责选择 / 取消选择，不负责打开面板。
- 少于两个块时，顶部确认按钮禁用或给出轻量提示。
- 升级确认面板填写：as 名称、难度、备注。
- as 名称必填。

提交行为：

- 提交时整个 `SectionPage` 顶层阻塞，并显示 `正在升级为 as`。
- 阻塞期间禁止 `Workspace` 选择、插入、删除、移动、Word 编辑、右键菜单和 `SectionTree` 操作。
- 成功后重新读取 `Section` 数据，不做 optimistic update。
- 失败时保持原页面数据不变，并显示错误提示。
