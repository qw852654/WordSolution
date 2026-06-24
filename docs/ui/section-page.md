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

## 3.1 TeachingStructureTree 悬浮入口

TeachingStructureTree【教学结构树】是 SectionPage 中的全库教学结构总览入口。

它不是 SectionStructurePanel，也不是 SectionTree。

职责区分：

```text
TeachingStructureTree
  全库总览：章节 / 教学主题 / Section / SectionVariant。

SectionTree
  当前 Section 内部结构：SectionItem / AtomicSection / ContentBlock。
```

当前阶段只在 SectionPage 中以悬浮抽屉形式出现，不实现主题工作台或常驻教学结构管理页。

呼出规则：

- 用户把鼠标停留在整个页面最左侧热区 2 秒后，打开 TeachingStructureTree 悬浮抽屉。
- 抽屉宽度可以根据内容动态展开，优先完整展示树节点文本。
- 抽屉打开时，整个 SectionPage 背景模糊，避免用户误以为还能直接编辑当前 Section。
- 点击抽屉外区域或按 Escape 关闭抽屉。
- 抽屉不是第四个常驻侧栏，不挤占 SectionStructurePanel、Workspace 或 Inspector 的布局空间。

节点模型：

```text
TeachingStructureNode
  = TeachingTopic
  + 可选绑定的 Section
  + 只读 SectionVariant 列表
```

行为规则：

- 单击 TeachingTopic 节点：只选中节点，用于查看节点信息或准备右键操作。
- 双击已绑定 Section 的 TeachingTopic 节点：打开该 Section 本身。
- 展开已绑定 Section 的 TeachingTopic 节点：显示该 Section 下的 SectionVariant 列表。
- SectionVariant 在 TeachingStructureTree 中第一版只读，只允许查看或打开占位，不允许新增、重命名、删除、复制；删除 SectionVariant 只允许在 SectionPage 的 SectionTree 右键菜单中执行。
- 第一版不提供 Section 解绑能力，避免产生无法从 UI 找回的孤儿 Section。
- 删除只允许作用于空 TeachingTopic；空 TeachingTopic 指没有子主题且没有绑定 Section。

Display Root【显示根节点】：

- TeachingStructureTree 在前端必须维护当前显示根节点状态。
- `displayRootNodeId = null` 时显示全库根结构。
- `displayRootNodeId` 有值时，只显示该节点作为临时显示根的分支。
- `displayRootPath` 用于显示当前显示根路径，并支持返回上一级、返回全库根。
- 设为显示根不修改后端数据，不改变真实 TeachingTopic 层级。

允许设为显示根：

- 有子 TeachingTopic 的 TeachingTopic。
- 已绑定 Section 的 TeachingTopic，即使它没有子 TeachingTopic、没有 SectionVariant。

禁止设为显示根：

- 空 TeachingTopic。空 = 没有子 TeachingTopic + 没有绑定 Section。
- SectionVariant。
- SectionTree 内部节点。

SectionVariant 节点规则：

- SectionVariant 不新增 TeachingStructureTree 专属节点类型。
- SectionVariant 复用已有 SectionVariant 节点语义，作为已绑定 Section 的 TeachingTopic 的只读子项。
- SectionVariant 不允许设为显示根。

右键菜单第一版：

未绑定 Section 的 TeachingTopic：

```text
新建子主题
新建后续主题
重命名主题
创建 Section
删除空主题
```

已绑定 Section 的 TeachingTopic：

```text
新建子主题
新建后续主题
重命名主题
打开 Section
```

SectionVariant：

```text
第一版在 TeachingStructureTree 中只读，不提供管理动作；删除动作属于 SectionPage 的 SectionTree。
```

后续方向：

- 主题工作台 / 教学结构管理页完成后，TeachingStructureTree 可以在该页面常驻左侧。
- SectionPage 中仍保持悬浮抽屉形态，避免长期占用编辑空间。

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
## 当前补充约定：Workspace 内部插入点与 CompositeBlock 自身正文

本节记录 2026-06-19 已确认的 SectionPage 细节，用于约束后续实现。

### 1. InsertPoint 不只存在于顶层 SectionItem 之间

`InsertPoint` 是 SectionWorkspace 文档流中的通用插入入口，不应只出现在顶层 SectionItem 之间。

后续实现必须覆盖三类位置：

```text
Section 顶层 flow item 之间
AtomicSectionBlock 内部 child item 之间
CompositeBlock 内部 child relation 之间
```

空容器也必须保留插入入口：

```text
空 Section：显示插入第一个块的入口
空 AtomicSectionBlock：显示插入第一个子块的入口
空 CompositeBlock：显示插入第一个子块的入口
```

不同层级的插入语义不同，不能简单复制顶层插入逻辑：

```text
Section 顶层插入
  -> 创建或插入 SectionItem

AtomicSectionBlock 内部插入
  -> 创建或插入 AtomicSectionItem

CompositeBlock 内部插入
  -> 创建或插入 ContentBlockRelation
```

因此，插入点必须携带明确上下文：

```text
insertContext.parentType = Section | AtomicSection | CompositeBlock
insertContext.parentId
insertContext.beforeItemId / afterItemId
insertContext.allowedActions
```

`InsertPoint` 组件本身仍然只负责 UI 和 emit，不直接判断业务规则，不调用 API。

### 2. CompositeBlock 必须显示自身 docx/html 正文

`CompositeBlock` 本质仍然是 `ContentBlock`，不是一个只负责展示子块的空壳。

因此，CompositeBlock 在 SectionWorkspace 中的渲染顺序应为：

```text
CompositeBlock 容器
  CompositeBlock 自身 docx/html 预览
  CompositeBlock 子块列表
```

也就是说：

- CompositeBlock 自己有 `ContentBlockVersion`、docx、html preview。
- CompositeBlock 自身正文必须使用 `ContentBlockDisplay` 或等价正文展示能力渲染。
- CompositeBlock 的 children 只表示组合关系展开结果，不能替代它自己的正文。
- 如果 CompositeBlock 自身没有 docx/html，则显示清晰空状态，但仍然保留子块列表。

后续数据映射中，CompositeBlock 的 view model 必须同时包含：

```text
selfContent: ContentBlockDisplayModel
children: StructuredBlockChildModel[]
```

其中 `selfContent` 表示 CompositeBlock 自身正文，`children` 表示它包含的子块。

## 当前定稿：SectionVariant 创建流程

本节记录已确认的 SectionPage 第一版 `SectionVariant` 创建规则。它是后续开发依据，不表示当前已经实现。

### 1. 概念关系

```text
TeachingTopic
  Section
    SectionVariant
```

`Section` 是完整知识池和完整教学结构，即“上帝小节”。
`SectionVariant` 是从 `Section` 派生出的教学用途方案，例如基础讲解版、提高版、一轮复习版、冲刺版。

`SectionVariant` 不复制 `Section` 结构。它通过 `SectionVariantItem` 引用 `SectionItem`：

```text
SectionVariantItem -> SectionItemId
```

前端创建时提交的是 `selectedSectionItemIds`，不是 `contentBlockIds`、`atomicSectionIds` 或前端 `flowItems`。

### 2. 唯一创建入口

第一版唯一业务入口：

```text
SectionPage -> SectionTree -> Section 根节点右键菜单 -> 新建 SectionVariant
```

不得放在：

- 顶部 Toolbar。
- Workspace。
- Inspector。
- TeachingStructureTree。
- `AtomicSection` / `ContentBlock` / `CompositeBlock` 节点右键菜单。

### 3. 两步创建流程

第一步：填写元数据。

字段：

- `Title`：必填。
- `Type`：必填，使用 `SectionVariantType`。
- `Difficulty`：必填，只允许 `Basic / Medium / Advanced / Top`，UI 不提供 `Unset`。
- `Description`：可选。

第一版前端不提交：

```text
Status
SortOrder
```

后端默认：

```text
Status = Draft
SortOrder = 当前 Section 下最大值 + 1
```

第二步：进入 `VariantSelectionMode`。

- 点击“下一步 / 进入选择”时调用后端选择预览。
- 预览成功后，当前 SectionPage / Workspace 进入选择模式。
- 预览失败时停留在元数据步骤，保留用户输入，并显示错误。
- 不做实时预览、debounce、自动刷新或前端本地推断。

### 4. 默认勾选规则

后端根据目标顶层 `SectionItem` 的难度计算默认勾选。

顶层定义：

```text
SectionItem.ParentItemId == null
```

目标难度来源：

- 顶层 `ContentBlock`：`ContentBlock.Difficulty`。
- 顶层组类型 `CompositeBlock`：本质仍是 `ContentBlock`，使用 `ContentBlock.Difficulty`。
- 顶层 `AtomicSection`：`AtomicSection.Difficulty`。

难度包含：

```text
Basic    -> Basic
Medium   -> Basic + Medium
Advanced -> Basic + Medium + Advanced
Top      -> Basic + Medium + Advanced + Top
Unset    -> 永不默认选中
```

### 5. 第一版选择范围

第一版只选择顶层 `SectionItem`。

不选择：

- `AtomicSectionItem`。
- `CompositeBlock` 子 relation。
- `ContentBlockVersion`。
- `SectionItem` 的前端子 view。

允许创建空 `SectionVariant`。如果用户不勾选任何项，后端仍可创建空 Variant。

### 6. 提交与成功后行为

提交时前端发送：

```text
sectionId
title
description?
type
difficulty
selectedSectionItemIds[]
```

后端必须一次事务创建 `SectionVariant` 和 `SectionVariantItem`。

成功后：

- 重新读取 `Section` / `SectionTree` / Variant 列表。
- 新建 Variant 在树中可见。
- 不自动打开新 Variant。
- 不自动选中新 Variant。
- 显示中文成功提示。

失败后：

- 保留用户填写的元数据和勾选状态。
- 停留在当前创建流程。
- 显示整体错误。
- 不做 optimistic update。

### 7. 未来 AtomicSection 部分选择

第一版整个 `AtomicSection` 作为一个顶层项选择。

未来如果要选择同一个 `AtomicSection` 内部的部分知识点、例题或练习，必须在 `SectionVariantItem` 之下扩展部分选择模型，而不是直接绑定 `AtomicSectionId`：

```text
SectionVariantItem
  SelectionMode: Whole | Partial

SectionVariantAtomicItemSelection
  SectionVariantItemId
  AtomicSectionItemId
  SortOrder
  Note
  UpdatedTime
```

## Current Update: SectionVariant Workspace Selection Mode

This section records the current implementation rule for the first real SectionVariant selection interaction.

### Entry

The only entry remains:

```text
SectionPage -> SectionTree -> Section root node context menu -> Create SectionVariant
```

No toolbar, Workspace, Inspector, TeachingStructureTree, or child node menu may create SectionVariant.

### Flow

1. User opens `SectionVariantCreatePanel` from the Section root context menu.
2. The panel only collects metadata in the real page flow:
   - `title`
   - `type`
   - `difficulty`
   - optional `description`
3. User clicks next.
4. `SectionPage` calls:

```text
POST /api/cms-v2/section-variants/selection-preview
```

5. If preview fails, the panel stays on metadata and shows the error.
6. If preview succeeds:
   - close `SectionVariantCreatePanel`
   - enter `SectionWorkspace` `VariantSelectionMode`
   - initialize selection state from `defaultSelected`
   - use API `selectable / unavailableReason` as the source of truth

### Workspace Selection Rule

- Selection happens in the Workspace document flow, not inside a list in the create panel.
- First version only allows selecting top-level `SectionItem`.
- Nested `AtomicSectionItem` and `CompositeBlock` child relations are visible but not independently selectable.
- Click selectable top-level blocks to toggle selected / unselected.
- Unavailable blocks use weak state and cannot be toggled.
- The mode toolbar shows selected count, clear, cancel, and confirm.
- InsertPoint, action rail, move, delete, Word edit, and other normal editing actions are disabled while this mode is active.
- Clearing selection is allowed and may produce an empty `selectedSectionItemIds` array.

### Current Round Boundary

- Confirming selection calls `POST /api/cms-v2/section-variants` with the metadata and final `selectedSectionItemIds`.
- Empty `selectedSectionItemIds` is allowed and creates an empty `SectionVariant`.
- While create is submitting, `SectionPage` is blocked with a top-level loading overlay.
- On success, `SectionPage` refreshes current data but does not automatically open or select the new `SectionVariant`.
- On failure, `VariantSelectionMode` stays active and keeps the user's current selection for retry.

### SectionTree Variant Visibility

After a `SectionVariant` is created, `SectionTree` must show it as a read-only node at the same visual level as the current `Section` root node.

This is only a frontend UI projection. The backend model and API relation remain unchanged:

```text
Section
  -> SectionVariant[]
```

The `SectionTree` visual projection is:

```text
SectionTree
  Section
    SectionItem / AtomicSection / ContentBlock ...
  SectionVariant
  SectionVariant
```

- `SectionVariant` nodes are for visibility and Inspector selection only.
- `SectionVariant` nodes are visually sibling nodes of the `Section` root in `SectionTree`.
- `SectionVariant` nodes still belong to the current `Section` in the backend model.
- They do not map to `Workspace` document-flow content.
- They expose only a delete context-menu action in `SectionTree`; edit, rename, copy, and content reselection actions stay out of the first version.
- SectionItem / AtomicSection / ContentBlock operations remain separate from `SectionVariant` display.

## Current Update: Shared Workspace Selection Modes

This section records the confirmed SectionPage rule for temporary Workspace selection modes.

### Goal

`SectionPage` will have multiple workflows where the user selects items directly in the Workspace document flow.
These workflows must share the same interaction model and visual feedback.

The current known modes are:

```text
WrapAsAtomicSectionMode
SectionVariantSelectionMode
```

Future modes should reuse the same behavior instead of adding another one-off selection implementation.

### Shared mode model

Recommended page-level model:

```text
WorkspaceSelectionMode
  modeId
  label
  selectedItemIds
  candidates
  selectionStateByItemId
  disabledEditingActions
  primaryAction
  secondaryActions
```

`modeId` is a page/workspace concern. Child display components should not branch on it.

`selectionStateByItemId` maps each visible top-level Workspace item to:

```text
none
selectable
selected
unavailable
```

### Selection is separate from normal node selection

Workspace selection mode is not the same thing as normal selected node state.

Normal selection:

- updates `SectionTree`;
- updates `Workspace` active item;
- updates `SectionInspector`.

Workspace selection mode:

- selects items for a temporary operation;
- may not update Inspector;
- must not accidentally trigger Word edit, delete, move, InsertPoint, or ordinary context-menu actions;
- exits only through the mode toolbar actions such as confirm, cancel, or clear.

### Generic click behavior

When a Workspace selection mode is active:

- clicking a selectable top-level item toggles selected / unselected;
- clicking a selected item cancels its selected state;
- clicking an unavailable item does nothing and keeps the unavailable reason visible or available;
- clicking nested children does nothing unless the active mode explicitly supports nested selection;
- clicking blank Workspace space does not clear the selection.

Selected items must visibly change in the same strength class as the existing `as` upgrade selection feedback.

### Mode: WrapAsAtomicSectionMode

Entry:

```text
Workspace top-right action -> upgrade to as
```

Selectable items:

- top-level `ContentBlock`;
- top-level group-type `CompositeBlock`.

Unavailable items:

- existing `AtomicSection`;
- nested children.

Confirm behavior:

- requires at least two selected items;
- opens the upgrade panel;
- later calls `/api/cms-v2/sections/{sectionId}/items/wrap-as-atomic-section`;
- uses page-level blocking while the API is running.

### Mode: SectionVariantSelectionMode

Entry:

```text
SectionTree -> Section root node context menu -> Create SectionVariant
```

Flow:

1. `SectionVariantCreatePanel` collects metadata.
2. `SectionPage` calls `POST /api/cms-v2/section-variants/selection-preview`.
3. Preview success closes the panel and enters `SectionVariantSelectionMode`.
4. Workspace initializes selected state from `defaultSelected`.
5. User reviews and adjusts selection in the document flow.

Selectable items:

- first version only supports top-level `SectionItem`.

Unavailable items:

- any candidate where preview API returns `selectable = false`;
- nested `AtomicSectionItem`;
- `CompositeBlock` child relations.

Confirm behavior for the current round:

- calls `POST /api/cms-v2/section-variants`;
- refreshes `SectionTree` / Variant visibility after server-confirmed success;
- does not automatically open the new `SectionVariant`;
- allows empty selection.

### Implementation migration rule

Existing `as` upgrade selection and `SectionVariant` selection must be migrated to the shared Workspace selection behavior before introducing another selection mode.

Do not add more mode-specific props such as `upgradeSelected` or `variantSelectionState` to `SectionItemView`.
Use generic selection state and keep business-specific rules in `SectionPage` or mode-specific page logic.

## SectionVariant read-only view mode

### Entry

The first version of `SectionVariant` viewing is entered only from `SectionTree`:

```text
SectionTree -> click SectionVariant node
```

Clicking the `Section` root node exits `VariantViewMode` and returns to the full editable `Section` document flow.

### Tree display rule

In the UI, `SectionVariant` nodes are displayed at the same visual level as the `Section` root node.

This is only a navigation / readability rule for `SectionTree`.
The backend model remains:

```text
Section
  -> SectionVariant[]
```

Do not create a separate persisted tree-node type for `SectionVariant`.

### Data loading

When a `SectionVariant` node is selected, `SectionPage` reads:

```text
GET /api/cms-v2/section-variants/{id}/items
```

Then it filters the already loaded full `Section` Workspace flow by `SectionVariantItem.sectionItemId`.

The display order must follow:

```text
SectionVariantItem.SortOrder -> SectionVariantItem.Id
```

If a selected `SectionItem` cannot be resolved from the current full Section flow, it is skipped in the first version instead of inventing placeholder content.

### Workspace behavior

`VariantViewMode` is read-only.

The Workspace must:

- show only the top-level `SectionItem` entries selected by the `SectionVariant`;
- show a clear read-only status in the Workspace header;
- hide `InsertPoint`;
- hide the `upgrade to as` action;
- hide or disable move, delete, rename, Word edit, and other structure-editing entry points;
- keep the page in the same Section context;
- show an empty state when the `SectionVariant` contains no `SectionItem`.

The first version does not support editing, renaming, copying, or reselecting `SectionVariant` contents from this mode.
Deleting a `SectionVariant` is supported only from the `SectionTree` `SectionVariant` context menu; it does not happen inside the read-only Workspace view.

### Inspector behavior

When the selected tree node is a `SectionVariant`, `SectionInspector` displays the `SectionVariant` metadata:

- name;
- type;
- difficulty;
- status;
- selected `SectionItem` count.

Selecting a `SectionVariant` should not automatically open a new page or switch to a future `SectionVariant` editor.

## SectionPage v0.1 status checkpoint

The current `SectionPage v0.1` scope is considered complete when these capabilities are available together:

- `SectionVariant` creation from the `SectionTree` Section root context menu;
- `SectionVariant` read-only view mode from a `SectionVariant` tree node;
- shared Workspace selection behavior for `upgrade to as` and `SectionVariantSelectionMode`;
- nested `InsertPoint` support at the beginning, between items, and at the end of `Section`, `AtomicSection`, and `CompositeBlock` flows;
- `CompositeBlock` displays both its own `ContentBlock` document preview and its child block list;
- a `ContentBlock` without a Word document displays its type, a concise empty-document hint, and the Word edit entry;
- `SectionInspector` displays stable read-only properties for `Section`, `ContentBlock`, `AtomicSection`, `CompositeBlock`, and `SectionVariant`.

The following items are explicitly deferred after `SectionPage v0.1`:

- `BlockSearchPicker` and inserting existing blocks;
- field editing in `SectionInspector` and any required update API;
- `TeachingNoteColumn`;
- automatic cleanup for empty shell objects;
- `SectionVariant` management actions such as rename, copy, or content reselection; the first version only supports deleting a `SectionVariant` from the `SectionTree` context menu.

## Current Update: Permanent ContentBlock Delete

`SectionPage` now distinguishes two different delete meanings:

```text
Remove reference
  Remove the current SectionItem / AtomicSectionItem / ContentBlockRelation reference only.
  The source ContentBlock entity, versions, and files remain.

Permanent ContentBlock delete
  Delete the ContentBlock entity itself, its versions, file assets, and all references to it.
  This is a dangerous operation and has no undo in the first version.
```

The permanent delete entry is only exposed from `SectionInspector` when the selected node is a `ContentBlock` or group-type `CompositeBlock`.

It must not be exposed from:

- `Workspace` action rail;
- `SectionTree` context menu;
- `ContentBlockDisplay`;
- `CompositeBlock`;
- `SectionItemView`.

`SectionInspector` must show a dedicated danger area and a clear confirmation prompt:

- this is not reference removal;
- it deletes the `ContentBlock` body and versions;
- it cleans all references from `Section`, `AtomicSection`, `CompositeBlock`, `SectionVariant`, and `HandoutVersion` structures;
- it cannot be undone in the first version.

For `CompositeBlock`, the confirmation must additionally state:

```text
Deleting the CompositeBlock deletes the CompositeBlock entity and its child relations.
It does not recursively delete child ContentBlock entities.
```

After success, `SectionPage` must reload current `Section` data and clear the current selected content node. If the page is in `SectionVariant` read-only view, it must also refresh the currently loaded Variant items so removed references do not remain visible.

If the backend rejects deletion, for example because the ContentBlock has an active Word edit session, `SectionPage` must keep the current page state and show the backend error message. It must not render a fake success state.
## 当前补充：AtomicSection Panel 工作流

本节记录 `SectionPage` 中 `AtomicSection` 板块化题目演化工作流的目标 UI。当前补充为开发口径，不代表所有功能已经实现。

### 1. AtomicSection 在 Workspace 中的结构

`AtomicSectionBlock` 不再只显示一个简单子块列表。目标结构为：

```text
AtomicSectionBlock
  AtomicSectionPanelBlock[]
  AtomicSectionUnassignedArea
```

其中：

- `AtomicSectionPanelBlock` 表示知识点、例题、变式题、练习题、课后练习等教学板块。
- `AtomicSectionUnassignedArea` 表示尚未归入任何 panel 的内容。
- 空 panel 也必须显示，便于用户知道结构已经存在但内容还没补齐。

### 2. Panel 展示规则

`AtomicSectionPanelBlock` 顶部显示：

- 难度短标记。
- panel 标题。
- 教学职责：知识点 / 例题 / 变式题 / 练习题 / 课后练习。
- 难度。
- 操作入口。

panel 内显示 `AtomicSectionItem` 对应的内容块展示。每个 item 仍然由 `SectionItemView` 外层容器承载。

### 3. 未归组区域

`AtomicSectionUnassignedArea` 用于展示 `AtomicSectionPanelId = null` 的 item。

规则：

- 未归组区域始终在 panel 区之后。
- 未归组为空时显示弱提示，不占用大面积空间。
- 未归组区域允许插入新内容。

### 4. InsertPoint 上下文

`SectionPage` 中的插入点必须区分上下文：

```text
section top level
atomic section panel list
atomic section panel item list
atomic section unassigned item list
composite block child list
```

目标上下文：

- panel 前 / panel 间 / panel 后：用于新增 panel。
- panel 内首位 / 中间 / 末尾：用于新增或插入 `AtomicSectionItem`。
- 未归组首位 / 中间 / 末尾：用于新增未归组 `AtomicSectionItem`。

第一版暂不做 `BlockSearchPicker`，但插入点模型要保留 `panelId`、`afterAtomicSectionItemId` 等上下文。

### 5. SectionTree 展示规则

`SectionTree` 需要显示：

```text
AtomicSection
  AtomicSectionPanel
    ContentBlock
  AtomicSectionUnassigned
    ContentBlock
```

规则：

- panel 是结构节点。
- 未归组区域是结构节点。
- 点击 panel 节点时，Workspace 滚动到 panel 顶部。
- 点击未归组节点时，Workspace 滚动到未归组区域。
- 空 panel 也在树中可见。

### 6. Inspector 展示规则

右侧 `SectionInspector` 需要支持：

- `AtomicSectionPanel`：标题、教学职责、难度、子项数量、排序位置。
- `AtomicSectionUnassignedArea`：未归组 item 数量。
- `AtomicSectionItem`：教学职责、来源内容块难度、所在 panel。

`ContentBlock` Inspector 不显示“所属 AS / 所属 panel”这类全局归属字段，因为 `ContentBlock` 是可复用资产。

### 7. 操作边界

组件只 emit 事件，不直接调用 API。

页面级 action 负责：

- 新增 panel。
- 重命名 panel。
- 修改 panel 教学职责和难度。
- 移动 panel。
- 删除 panel。
- 修改 item 教学职责和难度。
- 创建 panel 内 item。
- 创建未归组 item。

### 8. Handout 预览和生成边界

`Handout` 展开 `AtomicSection` 时：

1. 先按 panel 的 `SortOrder` 输出 panel 内 item。
2. panel 内按 item 的 `SortOrder` 输出。
3. 最后输出未归组 item。

第一版不输出 panel 标题。

### 9. 当前明确不做

- 不自动创建知识点、例题组、练习题组。
- 不在 `AtomicSection` 创建时生成默认 `ContentBlock`。
- 不在前端伪造 panel 数据。
- 不做 panel 标题进入 Word 输出。
- 不做按学生层级自动选题。

## 当前补充：题目结构化预览接入边界

题目结构化预览、输出样式重绑定和多题导入的完整开发约定见：

```text
docs/cms-v2/backend/题目结构化预览-输出样式重绑定-多题导入开发文档.md
```

`SectionPage` 后续只负责展示和触发页面级动作：

- `Workspace` 中的 `ContentBlockDisplay` 根据当前版本结构解析状态展示普通预览或结构化预览。
- `SectionPage` 不解析 Word 样式。
- `SectionPage` 不在前端推断 `Stem / Answer / Analysis / Hint / Other`。
- `SectionPage` 不根据 Word 样式推断 `TeachingRole`。
- 非 `Question ContentBlock` 默认展示普通预览；`ExampleGroup / ExerciseGroup / VariantGroup` 不因本能力线被前端强制当作结构化题目 Part。
- 结构化预览组件必须先进入 `ComponentLab` 验收，再接入真实 `SectionPage`。

Phase 4 已完成 `SectionPage` 多题导入前端重写。早期实现曾经是“选择 `.docx` 文件上传并逐个候选确认”，该口径是历史偏差，已修正，不得作为当前验收依据。

当前实现已经校准的链路包括：

- `QuestionImportDialog` 的面板结构。
- `cmsV2Client.createQuestionImportSession` 的请求形态。
- `SectionPage` 的导入上下文传递。
- 候选题确认方式。
- 导入成功后插入到 Section 顶层、AtomicSection 或 AtomicSectionPanel 上下文的页面级刷新和定位。

因此，后续如搜索到“上传式导入 / 逐候选确认”等描述，应按历史偏差处理，不得恢复为主流程。

正式目标：

```text
SectionPage 在可插入题目的上下文触发多题导入
-> 后端创建临时 Word 并自动打开
-> 用户粘贴 / 整理题目后保存关闭
-> 后端检测关闭并自动切割
-> 前端轮询到 ReadyForReview
-> 前端展示候选题确认页
-> 用户勾选并确认
-> 后端批量正式入库
-> SectionPage 刷新并定位首个新增题目
```

确认导入后的正式 `ContentBlock` 是否插入 `SectionPage` 工作区，由页面级导入上下文和后续插入逻辑决定。

确认前 `SectionPage` 不得展示任何伪造的正式新增题成功态；后端也不得写正式数据库或正式 `content-blocks/` 资产目录。

## 当前补充：多题导入组件上下文

`SectionPage` 中的多题导入不再被视为“顶部工具栏专用功能”，而是一个可复用导入工作流。

当前已确认的导入目标包括：

```text
SectionTopLevel
  在上帝小节顶层导入 ContentBlock。
  后续自动插入时应创建顶层 SectionItem。

AtomicSection
  在 AtomicSection 内部导入 ContentBlock。
  确认候选题后，由后端创建 AtomicSectionItem。

AtomicSectionPanel
  在 AtomicSection 内部具体 panel 中导入 ContentBlock。
  确认候选题后，由后端创建归属该 panel 的 AtomicSectionItem。
  默认 TeachingRole / Difficulty 来自 panel 本身。
```

当前实现边界：

- 抽出 `QuestionImportContext`，让同一 `QuestionImportDialog` 能表达不同导入目标。
- Workspace 顶部入口使用 `SectionTopLevel` 上下文。
- `AtomicSection` 上下文已有后端确认路径。
- `AtomicSectionPanelBlock` 内部 `导入题目` 使用 `AtomicSectionPanel` 上下文。
- `SectionPage` 构造 API context 时传入 `sectionId`、`atomicSectionId`、`atomicSectionPanelId`、默认教学职责和默认难度。
- 后端校验 panel 属于指定 AtomicSection，AtomicSection 属于指定 Section。
- 组件只负责展示临时 Word 会话、候选题、预览和确认元数据。
- 组件不直接创建 `SectionItem` 或 `AtomicSectionItem`。
- 插入到哪里必须由 `SectionPage` 或页面级 action 根据上下文决定。
- 组件不得把正式流程写成“选择并上传 `.docx` 文件”。
- 组件不得逐个候选题单独入库。
- 候选确认页只允许勾选、标题输入、查看结构状态和预览；不做拆题、合题、正文编辑、逐题 TeachingRole / Difficulty / 标签编辑。

当前完成情况：

1. 已替换上传式实现：创建临时 Word 会话、自动打开、轮询关闭后切割。
2. 已支持 `SectionTopLevel`：确认候选题后创建 `ContentBlock`，并插入当前 Section 顶层。
3. 已支持 `AtomicSectionPanel` 上下文的前端入口和后端确认路径；确认后创建归属该 panel 的 `AtomicSectionItem`，默认使用 panel 的 `TeachingRole` / `Difficulty`。
4. 如果后续支持其他导入目标，必须继续复用 `QuestionImportContext`，不得复制新的多题导入弹窗。

### 题目结构化与输出样式校准结果

`SectionPage` 当前与原始规格的关系如下：

- 结构化预览：可以消费后端 `data-question-part` HTML，但组件验收仍要覆盖 Stem、Answer、Analysis、Hint、Other、warning、Failed、NotApplicable、无 Word 文档。
- 多题导入：当前已切换为临时 Word session、轮询、candidate 查询和批量确认；上传式面板是历史偏差，已修正。
- 输出样式重绑定：属于后端生成链路，`SectionPage` 只显示生成结果和错误，不在前端计算或写死输出样式。
- 输出预检：后端已提供 `validate-word-generation`，`SectionPage` 只负责展示预检或生成错误，不在前端计算样式问题。
- 插入上下文：Section 顶层、AtomicSection 和 AtomicSectionPanel 内部导入必须共用 `QuestionImportContext`，不能各写一套导入面板。
- 页面级刷新：正式批量导入成功后，`SectionPage` 负责刷新 Section 数据，并定位首个新增题目；组件本身不得保存成功状态或伪造插入结果。
