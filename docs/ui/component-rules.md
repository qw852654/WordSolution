# CMS V2 鍓嶇缁勪欢瑙勫垯

鏈枃妗ｅ畾涔?V2 鍓嶇缁勪欢鍒嗗眰銆佽亴璐ｈ竟鐣屻?丄PI 璋冪敤瑙勫垯銆乵ock 浼樺厛娴佺▼鍜岀粍浠堕獙璇佽姹傘??

## 1. 缁勪欢绫诲瀷

### 1.1 Presentation Components

绾?UI 灞曠ず缁勪欢銆?

鑱岃矗锛?

- 鎺ユ敹 props銆?
- 娓叉煋瑙嗚缁撴瀯銆?
- 閫氳繃 emits 鏆撮湶浜嬩欢銆?
- 涓嶇悊瑙ｅ叿浣?CMS 涓氬姟鍚箟銆?
- 涓嶈皟鐢?API銆?
- 涓嶈鍙?Pinia store銆?

绀轰緥锛?

```text
Badge
Breadcrumb
ToolbarButton
IconButton
EmptyState
LoadingState
FieldLabel
```

### 1.2 Business Components

涓氬姟灞曠ず缁勪欢銆?

鑱岃矗锛?

- 鐞嗚В鏌愪釜涓氬姟瀵硅薄鐨勫睍绀鸿涔夈??
- 鍙互缁勫悎 presentation components銆?
- 鍙互浣跨敤 composables銆?
- 榛樿涓嶇洿鎺ヨ皟鐢?API銆?

绀轰緥锛?

```text
ContentBlockCard
AtomicSectionCard
SectionItemView
SectionVariantCard
HandoutItemCard
OutputFormCard
GeneratedFileRow
TeachingNotePanel
```

### 1.3 Business Container Components

鍙鐢ㄤ笟鍔″鍣ㄣ??

鑱岃矗锛?

- 灏佽涓?娈靛彲澶嶇敤涓氬姟娴佺▼銆?
- 鍙互鍔犺浇鏁版嵁銆?
- 鍙互璋冪敤 API 鎴?composables銆?
- 鍙互绠＄悊灞?閮ㄥ鍣ㄧ姸鎬併??

绀轰緥锛?

```text
ContentBlockPicker
AtomicSectionPicker
SectionItemPicker
OutputTemplatePicker
TeachingNoteDrawer
GeneratedFilePanel
```

## 2. API 璋冪敤瑙勫垯

```text
Presentation Components
  No API calls

Business Components
  Prefer composables

Business Container Components
  API calls allowed

Pages
  API calls allowed

Composables
  API calls allowed
```

绂佹锛?

```text
鍦ㄦ櫘閫氬睍绀虹粍浠朵腑鐩存帴 fetch銆?
鍦ㄥ涓粍浠朵腑澶嶅埗鍚屼竴涓?API URL銆?
鎶婇〉闈㈢姸鎬佸伔鍋锋斁杩涘叏灞? store銆?
```

鍏佽锛?

```text
椤甸潰鍔犺浇涓绘暟鎹??
涓氬姟瀹瑰櫒鍔犺浇閫夋嫨鍣ㄦ暟鎹??
composable 灏佽鏌ヨ銆侀?夋嫨銆佷繚瀛樸?佸睍寮?绛夊鐢ㄩ?昏緫銆?
```

## 3. Mock Data First 宸ヤ綔娴?

鎵?鏈夊彲澶嶇敤缁勪欢寮?鍙戦伒寰細

```text
Define DTO
鈫?
Create Mock Data
鈫?
Build Component
鈫?
Connect API
```

瑕佹眰锛?

- 娌℃湁浠ｈ〃鎬?mock 鏁版嵁锛屼笉寮?濮嬪仛鍙鐢ㄧ粍浠躲??
- mock 鏁版嵁蹇呴』瑕嗙洊绌虹姸鎬併?侀暱鏂囨湰銆佸灞傜骇銆佺鐢ㄧ姸鎬併?侀敊璇姸鎬併??
- 缁勪欢鍏堝湪 ComponentLabPage 楠岃瘉锛屽啀鎺ュ叆鐪熷疄椤甸潰銆?

## 3.1 鏍峰紡绾︽潫

鍦ㄧ敤鎴锋病鏈夋槑纭粰鍑鸿缁嗚瑙変慨鏀硅姹備箣鍓嶏細

- 鍏佽缁勪欢鍏堜娇鐢ㄦ渶绠?鏍峰紡钀藉湴銆?
- 绂佹鑷鍙戞尌鎴愯楗版?ф柟妗堛??

蹇呴』閬靛畧锛?

- 涓嶅湪缁勪欢涓殢鎰忓啓涓?娆℃?ч鑹插?笺??
- 涓嶄娇鐢ㄥぇ闈㈢Н闃村奖銆佹笎鍙樸?佽楗版?ц儗鏅??
- 涓嶄负鐩镐技缁勪欢閲嶅鍐欏濂楁牱寮忋??
- 浼樺厛澶嶇敤 shadcn-vue 鍩虹缁勪欢涓?Tailwind spacing銆乥order銆乼ext銆乥ackground token銆?
- 甯冨眬缁撴瀯銆佺粍浠跺眰绾с?佺姸鎬佺被蹇呴』绋冲畾锛屼笉鍥?hover銆侀?変腑銆佸姞杞借?屼复鏃舵敼缁撴瀯銆?
- 濡傛灉瑙嗚缁嗚妭涓嶇‘瀹氾紝鍏堜繚鎸佺畝娲佷腑鎬э紝涓嶈嚜琛岃ˉ鍏呰楗般??
- 鎵?鏈夊彲澶嶇敤缁勪欢蹇呴』鍏堝湪 ComponentLabPage 涓互 mock 鏁版嵁楠屾敹锛屽啀杩涘叆鐪熷疄椤甸潰銆?

### 3.1.1 Theme Token Rule

鍚庣画鎵?鏈変笟鍔＄粍浠讹紝鍖呮嫭锛?

- `ContentBlockDisplay`
- `AtomicSectionBlock`
- `CompositeBlock`
- `SectionTree`
- `SectionInspector`
- `Toolbar`
- `StatusTag`

绂佹鐩存帴鍐欐棰滆壊銆?

缁熶竴閫氳繃 Theme Token 寮曠敤棰滆壊銆?

濡傛灉褰撳墠缂哄皯 Token锛?

- 鍏堟彁鍑洪渶瑕佹柊澧炰粈涔?Token銆?
- 涓嶈鐩存帴鍐欓鑹插?笺??

## 3.2 鏂扮粍浠跺紑鍙戝墠纭

姣忔鏂板鎴栨娊璞?UI 缁勪欢鍓嶏紝蹇呴』鍏堝悜鐢ㄦ埛鍋氱畝瑕佸榻愶紝鍐呭鍖呮嫭锛?

- 缁勪欢鍚嶇О銆?
- 缁勪欢鑱岃矗銆?
- 缁勪欢涓嶈礋璐ｄ粈涔堛??
- 杈撳叆鏁版嵁鎴?mock 鏁版嵁鑼冨洿銆?
- 瀵瑰 emits / 浜嬩欢杈圭晫銆?
- 闇?瑕佹斁鍏?ComponentLabPage 鐨勯獙鏀跺満鏅??

鐢ㄦ埛纭鍚庢墠鑳藉紑濮嬪疄鐜拌缁勪欢銆?

## 4. ComponentLabPage

寤鸿璺敱锛?

```text
/lab
```

鑱岃矗锛?

```text
Component Development
Mock Data Testing
UI Verification
```

姣忎釜鍙鐢ㄧ粍浠惰嚦灏戞彁渚涳細

- 榛樿鐘舵?併??
- 閫変腑鐘舵?併??
- 绂佺敤鐘舵?併??
- 闀挎爣棰?/ 闀挎鏂囥??
- 绌烘暟鎹姸鎬併??
- 鍔犺浇鐘舵?併??
- 閿欒鐘舵?併??

浼樺厛楠岃瘉缁勪欢锛?

```text
褰撳墠寮?鍙戣疆娆＄浉鍏崇粍浠?
```

璇存槑锛?

- ComponentLabPage 鏄綋鍓嶅紑鍙戣疆娆＄殑楠屾敹鍏ュ彛锛屼笉鏄案涔呯粍浠跺睍瑙堥銆?
- 姣忎竴杞彧淇濈暀鏈疆闇?瑕侀獙鏀剁殑缁勪欢鍜?mock 鍦烘櫙銆?
- 涓婁竴杞棤鍏崇粍浠跺簲浠庡綋鍓?ComponentLabPage 瑙嗗浘涓Щ闄ゃ??
- ComponentLabPage 蹇呴』浣滀负鐙珛椤甸潰娓叉煋锛屼笉搴斿寘鍦ㄤ富搴旂敤 AppShell銆佷富瀵艰埅鎴栧乏渚у鑸腑銆?
- 椤甸潰绾у姛鑳介獙鏀舵椂锛屽彲浠ュ湪 ComponentLabPage 涓斁鍏ュ畬鏁撮〉闈?mock锛岃?屼笉鍙睍绀哄绔嬬粍浠躲??
- 姣忚疆浜や粯鏃跺繀椤昏鏄?ComponentLabPage 涓叿浣撴斁鍏ヤ簡浠?涔堛?佺敤鎴烽渶瑕侀獙鏀跺摢浜涘尯鍩熴?佸摢浜涙寜閽垨鏁版嵁浠嶆槸鍗犱綅銆?

## 5. 鏍稿績涓氬姟缁勪欢鑱岃矗

### ContentBlockCard

琛ㄧず涓?涓彲澶嶇敤鍐呭璧勪骇銆?

浣跨敤鑼冨洿锛?

- 璧勬簮搴撱??
- 鍐呭閫夋嫨鍣ㄣ??
- 鍏朵粬闇?瑕佲?滈?夋嫨鍐呭璧勪骇鈥濈殑鍒楄〃鎴栫綉鏍笺??

涓嶇敤浜庯細

- SectionWorkspace 鏂囨。娴佹鏂囧睍绀恒??
- SectionItemView 鍐呴儴姝ｆ枃鍐呭銆?

蹇呴』灞曠ず锛?

- 鏍囬銆?
- 鍐呭绫诲瀷銆?
- 闅惧害銆?
- 鐘舵?併??
- 褰撳墠鐗堟湰淇℃伅銆?
- 鎽樿鎴栫函鏂囨湰棰勮鍏ュ彛銆?

鍏佽鎿嶄綔锛?

- 閫夋嫨銆?
- 鎵撳紑璇︽儏銆?
- 鎵撳紑 Word 缂栬緫鍏ュ彛銆?
- 鏌ョ湅 HTML 棰勮銆?

### ContentBlockDisplay

琛ㄧず ContentBlock 鍦?SectionWorkspace 鏂囨。娴佷腑鐨勬鏂囧睍绀恒??

鑱岃矗锛?

- 灞曠ず ContentBlock 鐨勬鏂?HTML 棰勮銆?
- 涓嶆樉绀?ContentBlock 鏍囬銆?
- 涓嶆樉绀虹増鏈俊鎭??
- 姝ｆ枃棰勮鍖哄煙涓嶆樉绀鸿竟妗嗭紝灏介噺璐磋繎鏂囨。娴併??
- 涓嶆樉绀?ContentBlock 绫诲瀷銆佸彲鐢ㄧ姸鎬併?佸紩鐢ㄦā寮忕瓑鏂囧瓧鍏冧俊鎭??
- 鍙樉绀洪毦搴︼紝闅惧害鍙敤宸︿晶椤堕儴鐨勫皬棰滆壊鐐硅〃绀猴紱鍏蜂綋棰滆壊鍊煎悗缁敱鐢ㄦ埛纭鍚庡啀鍥哄畾銆?
- 鑷韩涓婁笅宸﹀彸 padding 涓?0锛岄棿璺濈敱澶栧眰 SectionItemView 鎺у埗銆?
- 榧犳爣 hover 鍒?ContentBlockDisplay 鏃朵笉鏄剧ず杈规銆?
- 鎻愪緵杞婚噺鍔ㄤ綔鍏ュ彛锛歐ord 缂栬緫銆佸埛鏂伴瑙堛?佹洿澶氥??
- 鍙互浣滀负 SectionItemView 鐨?slot 鍐呭銆?
- 鍙互浣滀负 AtomicSectionBlock / CompositeBlock 鐨勫瓙鍐呭銆?

杈圭晫锛?

- 涓嶄綔涓鸿祫婧愬簱鍗＄墖浣跨敤銆?
- 涓嶄娇鐢?`StructuredContainer`銆?
- 涓嶇洿鎺ヨ皟鐢?API銆?
- 涓嶇洿鎺ュ疄鐜?Word 缂栬緫浼氳瘽杞銆?
- 涓嶆寔鏈?SectionPage 椤甸潰鐘舵?併??

ComponentLabPage 楠屾敹锛?

- 榛樿鐘舵?併??
- 閫変腑鐘舵?併??
- LockedVersion銆?
- 鏃?HTML 棰勮銆?
- 闀挎鏂囥??
- 绂佺敤鐘舵?併??
- 涓嶆樉绀烘爣棰樺拰鐗堟湰銆?
- 涓嶆樉绀?ContentBlock 绫诲瀷鍜岀姸鎬併??

### InsertPoint

琛ㄧず鏂囨。娴佷腑鈥滆繖閲屽彲浠ユ彃鍏モ?濈殑浜や簰浣嶇疆銆?

鑱岃矗锛?

- 鍑虹幇鍦ㄤ袱涓?flow item 涔嬮棿銆?
- 榛樿寮卞寲鏄剧ず銆?
- 楂樺害淇濇寔绱у噾銆?
- 榧犳爣鍋滅暀绾?0.5 绉掑悗鏄剧ず鎻掑叆鍏ュ彛銆?
- 閿洏 focus 鍚庡簲鏄剧ず鎻掑叆鍏ュ彛銆?
- 涓棿鎻愪緵 slot锛岀敤浜庡睍绀哄綋鍓嶄綅缃厑璁告彃鍏ョ殑鍏ㄩ儴鍐呭绫诲瀷銆?
- 閫氳繃 `insert` 浜嬩欢鎶婃彃鍏ョ偣 id 浜ょ粰鐖剁粍浠躲??

杈圭晫锛?

- 涓嶅喅瀹氬彲浠ユ彃鍏ュ摢浜涗笟鍔″璞°??
- 涓嶈皟鐢?API銆?
- 涓嶅啓姝绘彃鍏ヨ彍鍗曢?夐」銆?
- 涓嶄慨鏀?Section 鏁版嵁銆?

### StructuredContainer / InlineBorderHeader

琛ㄧず AtomicSectionBlock 鍜?CompositeBlock 鍏变韩鐨勫急杈规缁撴瀯瀹瑰櫒銆?

鑱岃矗锛?

- `StructuredContainer` 璐熻矗寮辫竟妗嗗鍣ㄥ拰 body slot銆?
- `InlineBorderHeader` 璐熻矗杈规绾夸笂鐨勬爣棰樺拰 actions slot銆?
- 鏀寔闀挎爣棰樺拰澶氫釜鎿嶄綔鍏ュ彛銆?
- AtomicSectionBlock / CompositeBlock 鍐呴儴瀛愬潡涓嶄娇鐢ㄥ乏渚х珫绾裤??
- AtomicSectionBlock / CompositeBlock 鍐呴儴鐨勬瘡涓?ContentBlockDisplay 蹇呴』鍏堢敱瀛愮骇 SectionItemView 鍖呰９锛屽啀鎵胯浇姝ｆ枃灞曠ず銆?

杈圭晫锛?

- 涓嶇悊瑙?CMS 涓氬姟璇箟銆?
- 涓嶈皟鐢?API銆?
- 涓嶇敤浜?ContentBlockDisplay銆?
- 涓嶆寔鏈夊睍寮?銆侀?変腑鎴栧啓鍏ョ姸鎬併??

### AtomicSectionCard

琛ㄧず涓?涓師瀛愭暀瀛︾墖娈点??

蹇呴』灞曠ず锛?

- 鏍囬銆?
- 绫诲瀷銆?
- 鐘舵?併??
- 鍐呴儴鍐呭鍧楁暟閲忋??
- 绠?瑕佽鏄庛??

璇箟锛?

- AtomicSection 缁勭粐 ContentBlock銆?
- AtomicSection 鑷韩涓嶆壙杞藉彲缂栬緫姝ｆ枃銆?

### SectionItemView

琛ㄧず SectionItem 鍦?SectionWorkspace 涓殑鍙鍖栬〃鐜般??

褰撳墠宸茬‘璁ゅ彛寰勶細

- SectionItemView 鏄笂灞傚鍣紝涓嶆槸璧勬簮鍗＄墖銆?
- SectionItemView 涓嶅睍绀烘爣棰樸?佺被鍨嬨?佺姸鎬併?佺増鏈?佸娉ㄣ?佸紩鐢ㄦā寮忔垨鎽樿銆?
- SectionItemView 鍙礋璐ｆ壙杞芥湭鏉ョ殑 ContentBlockDisplay / AtomicSectionBlock / CompositeBlock 绛夊叿浣撳唴瀹圭粍浠躲??
- SectionItemView 鐨勫搴﹀簲寮规?у～婊℃í鍚戝尯鍩熴??
- SectionItemView 鐨勯珮搴︾敱鍐呴儴瀹為檯娓叉煋鍐呭鑷姩鎾戝紑銆?
- SectionItemView 鍏佽瀛愮骇 SectionItemView锛岀敤浜庤〃杈?SectionItem 鐨勭埗瀛愬眰绾с??
- SectionItemView 榛樿涓嶆樉绀鸿竟妗嗐??
- SectionItemView 鐨勫彸渚х旱鍚戞搷浣滃尯榛樿闅愯棌銆?
- SectionItemView 鐨勫彸渚х旱鍚戞搷浣滃尯蹇呴』鑴辩姝ｅ父甯冨眬娴侊紝涓嶅厑璁告拺楂?SectionItemView銆?
- 澶氫釜 SectionItemView 杩炵画鍑虹幇鏃堕粯璁ょ珫鐩磋创鍚堬紝涓嶅湪澶栧眰棰濆娣诲姞 gap / margin銆?
- 榧犳爣 hover 鍒?SectionItemView 鐨勬鏂囧尯鍩熸椂锛屼笉鏄剧ず杈规锛屼篃涓嶆樉绀烘搷浣滃浘鏍囥??
- 鍙湁榧犳爣杩涘叆鍙充晶绾靛悜鎿嶄綔鐑尯锛屾垨閿洏 focus 杩涘叆鍙充晶鎿嶄綔鍖烘椂锛屽彸渚ф搷浣滃浘鏍囧拰 SectionItemView 瀹瑰櫒杈规鎵嶄竴璧锋樉鐜般??

璇箟锛?

- 淇敼瀹冨彧淇敼灏忚妭缁撴瀯寮曠敤銆?
- 涓嶇洿鎺ヤ慨鏀规簮 ContentBlock 鎴?AtomicSection銆?
- SectionItemView 鏄笂灞傛蹇碉紝鍏蜂綋鍐呭鐢?ContentBlockDisplay / AtomicSectionBlock / CompositeBlock 鎵胯浇銆?- 瀹炵幇蹇呴』鍏堝湪 ComponentLabPage 涓敤 Mock Data 楠屾敹榛樿銆侀?変腑銆佺鐢ㄣ?佹í鍚戝～婊°?佸唴瀹硅嚜閫傚簲楂樺害銆佸瓙绾х粨鏋勫拰 hover 鎿嶄綔鍖烘樉闅愩??- 缁勪欢鍙?氳繃 emits 鏆撮湶閫夋嫨銆佸墠鎻掋?佸悗鎻掋?佷笂绉汇?佷笅绉汇?佺缉杩涖?佸弽缂╄繘銆佺Щ闄ゅ拰 Word 缂栬緫鍏ュ彛銆?- 缁勪欢涓嶈皟鐢?API锛屼笉璇诲彇 Pinia锛屼笉鎸佹湁 SectionPage 椤甸潰鐘舵?併??
### SectionTree

琛ㄧず SectionStructurePanel 涓殑褰撳墠 Section 缁撴瀯鏍戙??
鑱岃矗锛?
- 灞曠ず褰撳墠 Section 鍐呴儴鐨?SectionItem 缁撴瀯銆?- 鑺傜偣鍙互琛ㄨ揪 Section銆丄tomicSection銆丆ompositeBlock銆丆ontentBlock 绛?Section 鍐呴儴缁撴瀯瀵硅薄銆?- 灞曠ず灞傜骇銆佸睍寮? / 鎶樺彔銆侀?変腑鎬併?佺鐢ㄦ?佸拰鑺傜偣绫诲瀷鎽樿銆?- 鐐瑰嚮鑺傜偣鍙?氳繃浜嬩欢鎶婅妭鐐?id 浜ょ粰鐖剁骇锛岀敱鐖剁骇鍐冲畾鏄惁婊氬姩宸ヤ綔鍖恒?佹洿鏂?Inspector 鎴栬Е鍙戝叾浠栭〉闈㈢姸鎬併??- 澶嶇敤 BasicTree 鐨勯?氱敤灞曞紑 / 鎶樺彔鍜屾爲璇箟鑳藉姏銆?
杈圭晫锛?
- 涓嶈皟鐢?API銆?- 涓嶈鍙?Pinia銆?- 涓嶆寔鏈?SectionPage 椤甸潰绾?selectedNodeId銆?- 涓嶇洿鎺ユ粴鍔?SectionWorkspace銆?- 涓嶇洿鎺ヤ慨鏀?SectionItem 椤哄簭銆佸眰绾ф垨寮曠敤鍏崇郴銆?- 涓嶆贩鍏?TeachingTopic銆丠andout銆丟eneratedFile 鎴?ContentBlockVersion銆?- 涓嶆妸 BasicTree 鏈哄埗鍐欐垚 Section 涓撶敤瑙勫垯銆?
ComponentLabPage 楠屾敹锛?
- 榛樿灞傜骇鏍戙??- 鎶樺彔 / 灞曞紑鎸夐挳銆?- 閫変腑鎬併??- 绂佺敤鑺傜偣銆?- 闀挎爣棰樸??- 绌虹姸鎬併??
### SectionTreeNode

琛ㄧず SectionTree 涓殑涓?琛岃妭鐐瑰唴瀹广??
鑱岃矗锛?
- 灞曠ず鑺傜偣鏍囬锛涢缁勫拰棰樼洰绫昏妭鐐圭殑涓绘樉绀哄悕浣跨敤绫诲瀷鍚嶏紝鑰屼笉鏄嫭绔嬫爣棰樸??- 灞曠ず闅惧害锛屼娇鐢ㄧ揣璐磋妭鐐规爣棰樺乏渚х殑鐭珫绾胯〃绀猴紝绔栫嚎浣跨敤缁熶竴涓婚鑹诧紝涓嶅湪缁勪欢涓啓姝诲叿浣撻鑹插?笺??- 灞曠ず涓氬姟绫诲瀷锛屼緥濡傜煡璇嗙偣銆佷緥棰樸?佷緥棰樼粍銆佸彉寮忛缁勩??- 褰撹妭鐐规槸棰樼粍绫诲璞℃椂锛屽睍绀洪鐩暟閲忋??- 浣滀负 SectionTree 鐨勮妭鐐瑰唴瀹规彃妲戒娇鐢紝鏂逛究鍚庣画鎵╁睍鏇村瀛楁銆?
杈圭晫锛?
- 涓嶈礋璐ｅ睍寮? / 鎶樺彔銆?- 涓嶈礋璐ｈ妭鐐归?変腑鐘舵?佺鐞嗐??- 涓嶈皟鐢?API銆?- 涓嶈鍙?Pinia銆?- 涓嶇洿鎺ユ粴鍔?SectionWorkspace銆?
### SectionPage Skeleton Components

褰撳墠鏈?灏忛鏋跺寘鍚細

- `SectionTopToolbar`
- `SectionStructurePanel`
- `SectionWorkspace`
- `SectionInspector`

鑱岃矗锛?

- `SectionTopToolbar` 鍙綔涓哄彸渚ч《閮ㄧ殑绱у噾宸ュ叿鎺т欢鍖猴紝涓嶆樉绀洪〉闈㈡爣棰樸??
- `SectionStructurePanel` 鍙繚鐣欏乏渚х粨鏋勬爲鍖哄煙鍜岀┖鐘舵?併??
- `SectionWorkspace` 淇濈暀浣庨珮搴?Section 淇℃伅鏉°?丼ectionItemView 鏂囨。娴佷富鍒椼?佹湭鏉?TeachingNoteColumn 鍒嗘爮棰勭暀鍜岀┖鐘舵?侊紝骞跺湪绔栫洿鏂瑰悜鍗犳弧椤甸潰涓诲伐浣滃尯楂樺害銆?
- `SectionWorkspace` 鏂囨。娴佷富婊氬姩鍖轰娇鐢?`WeakScrollArea`锛岄伩鍏嶉粯璁ょ矖婊氬姩鏉℃姠鍗犲唴瀹规敞鎰忓姏銆?
- `SectionInspector` 鍙繚鐣欏彸渚ч?変腑瀵硅薄妫?鏌ュ尯鍩熷拰绌虹姸鎬併??

杈圭晫锛?

- 涓嶆帴 API銆?
- 涓嶅啓鍏ユ暟鎹??
- 涓嶅疄鐜?SectionTree銆丅asicTree 鑱斿姩銆佺湡瀹?SectionItemView 鍒楄〃銆丆ontentBlockDisplay銆丄tomicSectionBlock 鎴栫湡瀹?InsertPoint 浜や簰銆?
- 鏈疆 ComponentLabPage 鍙睍绀鸿繖浜涢鏋剁粍浠躲??

### WeakScrollArea

琛ㄧず寮辫瑙夋粴鍔ㄥ鍣ㄣ??

鑱岃矗锛?

- 缁熶竴鎵胯浇椤甸潰涓渶瑕佺珫鍚戞粴鍔ㄧ殑灞?閮ㄥ尯鍩熴??
- 浣跨敤杞婚噺杞ㄩ亾鍜屽急瑙嗚婊戝潡锛岄檷浣庨粯璁ゆ粴鍔ㄦ潯瀵瑰唴瀹瑰尯鐨勮瑙夊共鎵般??
- 浼樺厛鐢ㄤ簬 SectionWorkspace 鏂囨。娴併?乀eachingNoteColumn銆丼ectionStructurePanel銆丼ectionInspector锛屼互鍙婂悗缁?HandoutPage 鐨勭被浼兼粴鍔ㄥ尯鍩熴??

杈圭晫锛?

- 涓嶇悊瑙?CMS 涓氬姟璇箟銆?
- 涓嶈皟鐢?API銆?
- 涓嶈鍙?Pinia銆?
- 涓嶇鐞嗘粴鍔ㄥ尯鍩熷唴閮ㄥ唴瀹圭姸鎬併??
- 涓嶆浛浠ｉ〉闈㈠竷灞?瀹瑰櫒锛屽彧璐熻矗婊氬姩澶栧３銆?

### SectionInspector

琛ㄧず SectionPage 鍙充晶褰撳墠閫変腑鑺傜偣妫?鏌ラ潰鏉裤??

蹇呴』灞曠ず锛?

- 褰撳墠閫変腑鏍囬銆?
- 鐩爣绫诲瀷銆?
- 鐘舵?併??
- 鎺掑簭鍜屽眰绾с??
- 寮曠敤妯″紡銆?
- 閿佸畾鐗堟湰銆?
- 鎽樿銆?
- 澶囨敞銆?

璇箟锛?

- 鍙樉绀哄綋鍓嶉?変腑鐨?SectionItem / AtomicSection / ContentBlock 寮曠敤淇℃伅銆?
- 涓嶇洿鎺ヤ慨鏀?Section 缁撴瀯銆?
- 涓嶇洿鎺ヤ慨鏀规簮 ContentBlock 鎴?AtomicSection銆?
- 绗竴杞彧鎻愪緵棰勮鍜?Word 缂栬緫鍏ュ彛浜嬩欢锛屼笉璋冪敤 API銆?
- 蹇呴』鍦?ComponentLabPage 涓悓鏃跺睍绀虹┖鐘舵?佸拰閫変腑鐘舵?併??

### SectionVariantCard

琛ㄧず鍚屼竴 Section 涓嬬殑涓?涓暀瀛︾敤閫斿彉浣撱??

蹇呴』灞曠ず锛?

- 鏍囬銆?
- 绫诲瀷銆?
- 闅惧害銆?
- 鐘舵?併??
- 宸查??SectionItem 鏁伴噺銆?

### HandoutItemCard

琛ㄧず璁蹭箟鐗堟湰涓殑涓?涓緭鍑虹紪鎺掗」銆?

蹇呴』灞曠ず锛?

- 鐩爣绫诲瀷銆?
- 鐩爣鏍囬銆?
- 鎺掑簭銆?
- 鏍囬瑕嗙洊銆?
- 澶囨敞銆?

璇箟锛?

- 寮曠敤 SectionVariant 鏃讹紝灞曠ず涓哄睍寮?棰勮銆?
- 璋冩暣璁蹭箟椤逛笉鑳藉弽鍚戜慨鏀规簮 Section 缁撴瀯銆?

## 6. shadcn-vue 浣跨敤瑙勫垯

浼樺厛浣跨敤 shadcn-vue 鎻愪緵鐨勫熀纭?缁勪欢锛?

```text
Button
Input
Textarea
Select
Dialog
Drawer
Sheet
DropdownMenu
Tabs
Tooltip
Badge
Card
Table
Sidebar
```

绾︽潫锛?

- 涓诲鑸紭鍏堝熀浜?Sidebar 妯″紡锛屼笉鎵嬪啓涓?濂楁棤璇箟 sidebar銆?
- 琛ㄥ崟浼樺厛澶嶇敤缁熶竴琛ㄥ崟缁勪欢涓庢牎楠屾ā寮忋??
- 涓嶄负姣忎釜椤甸潰涓存椂鍒涢?犵浉浼兼寜閽?丅adge 鍜岄潰鏉挎牱寮忋??
- 鍗＄墖鍙敤浜庨噸澶嶄笟鍔″璞★紝涓嶆妸鏁撮〉 section 鍋氭垚鍗＄墖濂楀崱鐗囥??

## 7. 鍥炬爣瑙勫垯

- 鍥炬爣浼樺厛浣跨敤 `lucide-vue-next`銆?
- 涓嶄娇鐢?emoji 浣滀负 UI 鍥炬爣銆?
- 鍥炬爣鎸夐挳蹇呴』鏈?Tooltip 鎴?`aria-label`銆?
- 鍥炬爣灏哄淇濇寔绋冲畾锛屽父鐢?16px銆?8px銆?0px銆?4px 鍥涙。銆?

## 8. 鏂囨湰鍜?i18n

鎵?鏈夊彲瑙佹枃鏈繀椤婚?氳繃 i18n key锛?

```vue
<Button>{{ t("common.save") }}</Button>
```

绂佹锛?

```vue
<Button>Save</Button>
```

缁勪欢 props 涓鏋滀紶鍏ユ樉绀烘枃妗堬紝璋冪敤鏂逛篃搴斾粠 i18n 鑾峰彇銆?


## 褰撳墠琛ュ厖绾﹀畾锛欼nsertPoint 涓? BlockSearchPicker

### InsertPoint 褰撳墠浣跨敤瑙勫垯

鍦? SectionWorkspace 涓紝InsertPoint 鐩存帴鍛堢幇褰撳墠浣嶇疆鍏佽鐨勫叿浣撴搷浣滄寜閽??

褰撳墠鎸夐挳涓猴細

1. 鏂板缓 ContentBlock
2. 鏂板缓 AtomicSection
3. 鎻掑叆宸叉湁鍧?

瑙勫垯锛?

- 涓嶅啀浣跨敤鍗曚釜鈥滄彃鍏モ?濇寜閽綔涓轰富鍏ュ彛銆?
- 涓嶅啀閫氳繃鐐瑰嚮鈥滄彃鍏モ?濆悗灞曞紑浜岀骇闈㈡澘銆?
- 鐐瑰嚮鈥滄柊寤? ContentBlock鈥濊繘鍏ユ柊寤? ContentBlock 娴佺▼銆?
- 鐐瑰嚮鈥滄柊寤? AtomicSection鈥濊繘鍏ユ柊寤? AtomicSection 娴佺▼銆?
- 鐐瑰嚮鈥滄彃鍏ュ凡鏈夊潡鈥濆悗缁墦寮? BlockSearchPicker銆?
- InsertPoint 涓嶇洿鎺ヤ慨鏀? Section 鏁版嵁锛屼笉璋冪敤 API銆?

### BlockSearchPicker

BlockSearchPicker 琛ㄧず浠庡凡鏈夊潡涓悳绱㈠苟閫夋嫨鎻掑叆鐩爣鐨勪笟鍔″鍣ㄧ粍浠躲??

鑱岃矗锛?

- 鍚屾椂鎼滅储 ContentBlock 鍜? AtomicSection銆?
- 鍦ㄥ悓涓?涓粨鏋滃垪琛ㄤ腑灞曠ず瀵硅薄绫诲瀷銆佹爣棰樺拰蹇呰鎽樿銆?
- 鍏佽鐢ㄦ埛閫夋嫨涓?涓凡鏈夊潡锛屽苟閫氳繃浜嬩欢鎶婇?変腑瀵硅薄浜ょ粰鐖剁骇銆?
- 鏀寔绌虹粨鏋溿?侀暱鏍囬銆佺鐢ㄩ」鍜屽姞杞界姸鎬佺殑 Mock Data 楠屾敹銆?

杈圭晫锛?

- 涓嶇洿鎺ヤ慨鏀? Section 鏁版嵁銆?
- 涓嶇洿鎺ュ垱寤? ContentBlock 鎴? AtomicSection銆?
- 涓嶆妸 ContentBlock 鍜? AtomicSection 鎷嗘垚涓や釜浜掓枼鎼滅储鍏ュ彛銆?
- 涓嶅湪褰撳墠 InsertPoint 灏忛棴鐜腑瀹炵幇锛屽悗缁崟鐙紑鍙戙??
- 鍚庣画蹇呴』鍏堣繘鍏? ComponentLab 浣跨敤 Mock Data 楠屾敹锛屽啀鎺ュ叆 SectionPage銆?
## 褰撳墠琛ュ厖绾﹀畾锛欼nsertCreateOverlay

InsertCreateOverlay 琛ㄧず浠? InsertPoint 鏂板缓鍧楁椂寮瑰嚭鐨勬彃鍏ラ潰鏉裤??

瑙﹀彂鍏ュ彛锛?

1. 鏂板缓 ContentBlock
2. 鏂板缓 AtomicSection

鑱岃矗锛?

- 浣滀负 SectionPage 涓婃柟鐨勬渶涓婂眰 overlay 鏄剧ず銆?
- 鎵撳紑鏃惰鑳屽悗鐨勬暣涓? SectionPage 妯＄硦銆?
- 鏍规嵁 targetType 鏄剧ず鏂板缓 ContentBlock 鎴栨柊寤? AtomicSection 鐨勫垱寤鸿〃鍗曪紱ComponentLab 涓娇鐢? Mock Data 楠屾敹锛孲ectionPage 涓敱鐖剁骇璋冪敤 CMS V2 API銆?
- 鏄剧ず褰撳墠鎻掑叆浣嶇疆涓婁笅鏂囥??
- 閫氳繃浜嬩欢鎶婄敤鎴峰～鍐欑殑鍒涘缓鏁版嵁浜ょ粰鐖剁骇銆?
- 鎻愪緵鍙栨秷鍜岀‘璁ゆ柊寤哄叆鍙ｃ??

瀛楁锛?

褰? targetType = ContentBlock锛?

- 鎵?灞? Section锛氶粯璁ゆ樉绀哄綋鍓? Section 鍚嶇О锛屽綋鍓嶉樁娈典笉鍙慨鏀癸紱鎻愪氦鏃朵紶閫? SectionId銆?
- 鍚嶇О锛屽彲閫夛紱娌℃湁鍚嶇О鏃朵笉闃绘鏂板缓
- 绫诲瀷锛氱煡璇嗙偣 / 渚嬮 / 鍙樺紡棰? / 缁冧範棰? / 鍙樺紡棰樼粍 / 缁冧範棰樼粍
- 闅惧害锛氬熀纭? / 涓。 / 鎻愰珮 / 鍘嬭酱

褰? targetType = AtomicSection锛?

- 鎵?灞? Section锛氶粯璁ゆ樉绀哄綋鍓? Section 鍚嶇О锛屽綋鍓嶉樁娈典笉鍙慨鏀癸紱鎻愪氦鏃朵紶閫? SectionId銆?
- 鍚嶇О
- 闅惧害锛氬熀纭? / 涓。 / 鎻愰珮 / 鍘嬭酱
- 澶囨敞锛屽彲閫?

瀛楁璇存槑锛?

- 鎵?灞? Section 鍦? UI 涓樉绀哄悕绉帮紝浣嗘寔涔呭寲浣跨敤 SectionId锛岄伩鍏? Section 鏀瑰悕鍚庡綊灞炴柇瑁傘??
- ContentBlock 鍚嶇О鍙负绌猴紱灞曠ず鏃剁敱绫诲瀷銆侀瑙堟憳瑕佹垨涓婁笅鏂囧厹搴曘??
- AtomicSection 鍚嶇О浠嶅繀濉紝闅惧害鏄嫭绔嬪瓧娈碉紝涓嶆槧灏勫埌 Description銆?

杈圭晫锛?

- ComponentLab 涓彧鎻愪氦 Mock 鍙嶉锛屼笉璋冪敤 API銆?
- SectionPage 涓敱椤甸潰鐖剁骇璋冪敤 CMS V2 API 鍒涘缓 ContentBlock / AtomicSection锛屽苟閲嶆柊璇诲彇 Section 鏁版嵁銆?
- 缁勪欢鏈韩涓嶇洿鎺ヨ皟鐢? API锛屼笉鐩存帴淇敼 Section 鏁版嵁銆?
- 涓嶆墦寮? Word銆?
- 涓嶆悳绱㈠凡鏈夊潡銆?
- 涓嶅鐞? BlockSearchPicker銆?

ComponentLabPage 楠屾敹锛?

- ContentBlock 鏂板缓闈㈡澘銆?
- AtomicSection 鏂板缓闈㈡澘銆?
- 绌哄悕绉扮姸鎬併??
- 闀垮悕绉扮姸鎬併??
- 绂佺敤鐘舵?併??
- 鎻愪氦鍚? Mock 鍙嶉銆?
- 鍙栨秷鍏抽棴鐘舵?併??
- 鑳屽悗 SectionPage 妯＄硦鏁堟灉銆?
## 当前补充约定：SectionTreeContextMenu

SectionTreeContextMenu 表示 SectionTree 节点上的右键上下文菜单。

职责：

- 覆盖浏览器默认右键菜单。
- 显示当前右键目标节点的菜单操作。
- 通过事件把菜单动作交给父级处理。
- 使用 SectionTree 的临时 context target 高亮，不修改 selectedNodeId。
- 支持 Escape 和点击外部关闭。

菜单项：

1. 新建 ContentBlock
2. 新建 AtomicSection
3. 插入已有块
4. 移除

边界：

- 右键节点时只高亮该节点，不默认选中该节点。
- 右键不应同步右侧 Inspector。
- 右键不应同步 Workspace 选中态。
- 上移、下移、缩进、反缩进不出现在该菜单中。
- 组件不调用 API。
- 组件不修改 Section 数据。
- 组件不持有 SectionPage 页面状态。

ComponentLabPage 验收：

- 放置一个 SectionTree 进行联动测试。
- 点击节点时更新 selectedNodeId。
- 右键节点时只更新 context target。
- 右键菜单出现时浏览器原生菜单不出现。
- 选择菜单项后只显示 Mock 反馈，不改数据。

## 当前补充约定：Server-confirmed Update

CMS V2 前端涉及持久化的交互统一采用 server-confirmed update 模式。

职责边界：

- 展示组件仍然不调用 API。
- 业务组件仍然只通过 emits 暴露用户意图。
- 页面、业务容器或 composable 负责调用 `/api/cms-v2`。
- 前端页面只有在后端成功返回确认数据后，才允许更新对应业务数据视图。

禁止：

- 不允许 optimistic update。
- 不允许先本地修改 Section 结构，再失败回滚。
- 不允许把多个结构修改先堆在前端，最后通过“保存结构”统一提交。
- 不允许组件内部私自维护一份会与后端状态分叉的业务数据副本。

允许：

- 维护纯 UI 状态，例如 selectedNodeId、expandedNodeIds、context target、overlay open、loading、error。
- 表单提交前维护临时输入。
- API 调用期间显示 loading 状态。
- API 失败时显示错误提示，并保留原有后端确认过的数据。
- API 成功后使用返回数据替换本地业务数据；如果响应不含最新聚合数据，则成功后重新读取。
补充：SectionTree 右键目标高亮必须使用具备业务含义的 theme token。

当前 token 命名：

- section-tree-context-target
- section-tree-context-target-foreground
- section-tree-context-target-ring

规则：

- 不使用 primary、accent 这类抽象命名直接表达业务状态。
- 不写死具体颜色值。
- 若后续需要调整视觉颜色，只修改 token 映射，不在组件中改一次性颜色。

## 当前补充约定：BasicTreeNodeView 与 TeachingStructureTree

### BasicTreeNodeView

BasicTreeNodeView 表示树节点的一行通用视觉结构。

职责：

- 显示节点主标题。
- 显示可选的左侧短竖线标记。
- 显示右侧轻量 meta 信息。
- 处理长标题截断和基础布局稳定性。

边界：

- 不理解 Section、TeachingTopic、ContentBlock 等业务语义。
- 不负责展开 / 折叠。
- 不负责选中态、右键目标态或 hover 背景。
- 不调用 API。
- 不读取 Pinia。

使用规则：

- SectionTreeNode 必须通过 BasicTreeNodeView 渲染节点内容。
- TeachingTopicTreeNode 必须通过 BasicTreeNodeView 渲染节点内容。
- 不允许为 SectionTree 和 TeachingTopicTree 分别复制两套节点行样式。

### TeachingStructureTree / TeachingTopicTree

产品语义上，这棵树称为 TeachingStructureTree【教学结构树】。

当前代码中已有的 TeachingTopicTree 是它的实现基础。后续是否把组件文件从 TeachingTopicTree 重命名为 TeachingStructureTree，需要单独规划，不在普通功能修改中顺手完成。

TeachingStructureTree 表示整个内容库的教学结构总览树，不再只是纯 TeachingTopic 导航树。

核心模型：

```text
TeachingStructureNode
  = TeachingTopic 信息
  + 可选绑定的 Section 信息
  + 只读 SectionVariant 列表
```

典型结构：

```text
功能关系
  机械能守恒                 TeachingTopic + Section
    基础讲解版               SectionVariant，只读
    提高版                   SectionVariant，只读
    一轮复习版               SectionVariant，只读
  竖直圆轨道                 TeachingTopic + Section
  杆模型                     TeachingTopic，未绑定 Section
```

职责：

- 展示全库 TeachingTopic 层级。
- 表达每个 TeachingTopic 是否已经绑定 Section。
- 展开绑定了 Section 的 TeachingTopic 后，只读显示该 Section 下的 SectionVariant 列表。
- 作为全库教学结构总览入口，帮助用户快速理解章节、主题、Section 和 Variant 的分布。
- 允许选择一个 TeachingTopic / Section / SectionVariant 节点，并通过事件交给父级处理。
- 显示轻量字段，例如 Section 状态、Variant 数量、归档状态。
- 复用 BasicTree 的树行为和 BasicTreeNodeView 的节点视觉结构。

边界：

- 不展示 Section 内部结构。
- 不展示 SectionItem、ContentBlock、版本或生成记录。
- 不展示 Handout 内部结构。
- 不把 ContentBlock、ContentBlockVersion、GeneratedFile 混进这棵树。
- SectionVariant 第一版只读，不在这棵树内新增、重命名、删除或复制。
- 第一版不提供 Section 解绑能力，避免出现无法从 UI 找回的孤儿 Section。
- 组件本身不调用 API，真实读写由页面或 composable 处理。

基础交互：

- 单击 TeachingTopic 节点：只选中节点，用于查看或右键管理。
- 双击已绑定 Section 的 TeachingTopic 节点：打开该 Section 本身。
- 展开已绑定 Section 的 TeachingTopic 节点：显示它下面的 SectionVariant 列表。
- 单击 SectionVariant 节点：只选中，显示 Variant 信息。
- 双击 SectionVariant 节点：后续可打开对应 Variant 视图；第一版如果页面未完成，可以保留为占位。
- 点击树外区域或按 Escape：关闭悬浮抽屉。

Display Root【显示根节点】：

- TeachingStructureTree 必须支持把某个节点临时设为显示根节点。
- Display Root 只是前端注意力管理状态，不修改真实 TeachingTopic 层级，不写数据库，不调用持久化 API。
- `displayRootNodeId = null` 表示显示全库根结构。
- `displayRootNodeId = 某节点 id` 表示只显示该节点及其下级分支。
- 前端必须维护 `displayRootNodeId` 和 `displayRootPath`，用于返回上一级和返回全库根。
- 允许通过“返回上一级”把显示根切回父节点。
- 允许通过“返回全库根”把 `displayRootNodeId` 置回 `null`。

可设为显示根的节点：

- 有子 TeachingTopic 的 TeachingTopic。
- 已绑定 Section 的 TeachingTopic，即使它没有子 TeachingTopic、没有 SectionVariant。

不可设为显示根的节点：

- 空 TeachingTopic。空 = 没有子 TeachingTopic + 没有绑定 Section。
- SectionVariant。
- SectionTree 内部节点，例如 SectionItem / AtomicSection / ContentBlock。

SectionVariant 规则：

- SectionVariant 在 TeachingStructureTree 中不新增独立节点类型。
- 它复用现有 SectionVariant 节点语义，作为已绑定 Section 的 TeachingTopic 下方只读子项。
- SectionVariant 不参与 Display Root。

ComponentLabPage 验收：

- 本轮只放 TeachingStructureTree / TeachingTopicTree 相关验收内容。
- 必须覆盖未绑定 Section、已绑定 Section、带 SectionVariant、空主题、长标题等 Mock Data 场景。
- 点击节点后，右侧显示当前选中的 TeachingStructureNode Mock 信息。
- 展开 / 折叠、选中态、禁用态、长标题必须沿用 BasicTree 行为。
- 必须覆盖 Display Root 场景：可提升节点、不可提升空节点、不可提升 SectionVariant、返回上一级、返回全库根。

### TeachingStructureTreeContextMenu

TeachingStructureTreeContextMenu 表示 TeachingStructureTree 节点上的右键上下文菜单。当前代码可继续使用 TeachingTopicTreeContextMenu 作为实现名，但文档语义按 TeachingStructureTree 理解。

职责：

- 覆盖浏览器默认右键菜单。
- 显示当前右键目标 TeachingStructureNode 的菜单操作。
- 通过事件把菜单动作交给父级处理。
- 使用 BasicTree 的 context target 高亮能力。
- 支持 Escape 和点击外部关闭。

第一版菜单项：

未绑定 Section 且允许管理的 TeachingTopic 节点：

1. 新增子节点
2. 新增后续节点
3. 重命名主题
4. 创建 Section
5. 删除空主题

已绑定 Section 的 TeachingTopic 节点：

1. 新增子节点
2. 新增后续节点
3. 重命名主题
4. 打开 Section

SectionVariant 节点：

- 第一版只读，不提供管理动作。

删除规则：

- 第一版只允许删除空 TeachingTopic。
- 空 TeachingTopic = 没有子主题 + 没有绑定 Section。
- 已绑定 Section 的节点不能删除。
- 有子主题的节点不能删除。

边界：

- 右键节点时只高亮该节点，不默认选中该节点。
- 右键不改变当前选中的 TeachingStructureNode。
- 本轮只在 ComponentLabPage 中使用 Mock Data 验收。
- 真实新增、重命名、创建 Section、删除空主题必须走页面或 composable 的 CMS V2 API。
- 不做 Section 解绑。
- 不做 SectionVariant 管理。
- 本轮不调用 API。

ComponentLabPage 验收：

- 放置一个 TeachingStructureTree / TeachingTopicTree 进行联动测试。
- 点击节点时更新 selectedStructureNodeId。
- 右键节点时只更新 context target。
- 右键菜单出现时浏览器原生菜单不出现。
- 选择菜单项后只显示 Mock 反馈，不改树数据。

## 褰撳墠琛ュ厖绾﹀畾锛欴ifficulty Theme Tokens

Difficulty Theme Tokens 鐢ㄤ簬缁熶竴鏄剧ず ContentBlock銆丄tomicSection銆丆ompositeBlock銆丼ectionTree 鑺傜偣涓殑闅惧害瑙嗚鏍囪銆?

褰撳墠璇箟锛?

- difficulty-unset锛氭湭璁剧疆銆?
- difficulty-basic锛氬熀纭?锛屼綆闅惧害锛屽亸缁胯壊銆?
- difficulty-medium锛氫腑妗ｏ紝涓瓑闅惧害锛屽亸钃濊壊銆?
- difficulty-advanced锛氭彁楂橈紝杈冮珮闅惧害锛屽亸姗欒壊銆?
- difficulty-top锛氬帇杞达紝鏈?楂橀毦搴︼紝鍋忕孩鑹层??

浣跨敤瑙勫垯锛?

- `ContentBlockDisplay` 鐨勯毦搴﹀皬鐐瑰繀椤讳娇鐢ㄤ笂杩? token銆?
- `SectionTreeNode` 鐨勯毦搴︾煭绔栫嚎蹇呴』浣跨敤涓婅堪 token銆?
- 涓氬姟缁勪欢涓笉寰楃洿鎺ュ啓鍏蜂綋棰滆壊鍊笺??
- 濡傛灉鍚庣画瑕佽皟鏁撮毦搴﹂鑹诧紝鍙慨鏀? theme token锛屼笉鍦ㄧ粍浠朵腑鏀逛竴娆℃?ч鑹层??
## 褰撳墠琛ュ厖绾﹀畾锛欰tomicSection 鐨? SectionItemView 鎿嶄綔鍖?

褰? `SectionItemView` 鎵胯浇鐨勬槸 `AtomicSectionBlock` 鏃讹紝鍙充晶鎿嶄綔鍖哄彧鏄剧ず锛?

- 鏂板缓瀛愮骇 `ContentBlock`銆?
- 涓婄Щ銆?
- 涓嬬Щ銆?
- 閲嶅懡鍚? `AtomicSection`銆?
- 绉婚櫎褰撳墠 `SectionItem` 寮曠敤銆?

杈圭晫锛?

- 鏂板缓瀛愮骇 `ContentBlock` 琛ㄧず鍦ㄥ綋鍓? `AtomicSection` 鍐呴儴鍒涘缓骞跺姞鍏? `AtomicSectionItem`锛屼笉鏄湪褰撳墠 `Section` 椤跺眰鏂板鍏勫紵 `SectionItem`銆?
- 閲嶅懡鍚嶄慨鏀圭殑鏄? `AtomicSection` 鏈綋鍚嶇О锛屼笉鏄? `SectionItem` 鐨勬爣棰樿鐩栥??
- 绉婚櫎鍙垹闄ゅ綋鍓? `SectionItem` 寮曠敤锛屼笉鍒犻櫎 `AtomicSection` 鏈綋锛屼篃涓嶅垹闄ゅ畠鍐呴儴宸叉湁鐨? `ContentBlock`銆?
- 涓婄Щ / 涓嬬Щ鍙皟鏁村綋鍓? `Section` 鍐呰 `SectionItem` 鐨勯『搴忥紝涓嶅仛缂╄繘銆佸弽缂╄繘鎴栨嫋鎷姐??
- `SectionItemView` 浠嶇劧涓嶇洿鎺ヨ皟鐢? API锛涚湡瀹炲姩浣滅敱 `SectionPage` 缂栨帓锛岀粍浠跺彧閫氳繃 emits 鏆撮湶浜嬩欢銆?

## 褰撳墠琛ュ厖绾﹀畾锛欳ontentBlock 鐨? Word 缂栬緫鎿嶄綔鍖?

褰? `SectionItemView` 鎵胯浇鐨勬槸 `ContentBlockDisplay` 鏃讹紝鍙充晶鎿嶄綔鍖哄彲浠ユ彁渚? Word 缂栬緫鍏ュ彛銆?

缁勪欢杈圭晫锛?

- `ContentBlockDisplay` 涓嶇洿鎺ヨ皟鐢? API銆?
- `SectionItemView` 涓嶇洿鎺ヨ皟鐢? API銆?
- 缁勪欢鍙? emit `openWord` 鎴栫瓑浠蜂簨浠躲??
- `SectionPage` 鎴栭〉闈㈢骇 composable 璐熻矗璋冪敤 CMS V2 鍚庣銆?
- 缁勪欢涓嶅緱鏋勯?犳湰鍦? DOCX 璺緞銆?
- 缁勪欢涓嶅緱鏋勯?? `ms-word:`銆乣file://` 鎴栧叾浠栨湰鍦版墦寮? URI銆?
- 缁勪欢涓嶅緱璋冪敤 V1 `缂栬緫浼氳瘽` 鎺ュ彛銆?

鍚庣杈圭晫锛?

- Word 缂栬緫鍏ュ彛蹇呴』閫氳繃 CMS V2 鍚庣缂栬緫浼氳瘽 API銆?
- 鏈湴 Word 鍚姩鏂瑰紡鐢卞悗绔瓥鐣ュ皝瑁呫??
- 鏈潵杩佺Щ鍒颁簯绔椂锛屽簲鏇挎崲鍚庣 `ContentBlock` 缂栬緫浼氳瘽鍚姩绛栫暐锛岃?屼笉鏄慨鏀逛笟鍔＄粍浠躲??

ComponentLabPage 楠屾敹锛?

- 鍚庣画寮?鍙? `ContentBlock` 鎿嶄綔鍖烘椂锛屽簲鍏堝湪 ComponentLabPage 涓獙璇? Word 缂栬緫鎸夐挳鐨勫睍绀恒?乴oading銆侀敊璇拰鎴愬姛鍙嶉銆?
- ComponentLabPage 涓粛鍙娇鐢? Mock Data锛涚湡瀹? API 鎺ュ叆蹇呴』鍦? `SectionPage` 椤甸潰绾у畬鎴愩??

## 褰撳墠琛ュ厖绾﹀畾锛歋ection 鍔ㄤ綔闆嗗悎 / Composables

`SectionPage` 涓殑鐪熷疄鍔ㄤ綔涓嶅緱鍐欏叆涓氬姟灞曠ず缁勪欢銆?

閫傜敤鑼冨洿锛?

- `SectionItemView`
- `AtomicSectionBlock`
- `ContentBlockDisplay`
- `SectionTree`
- `SectionInspector`

鍥哄畾瑙勫垯锛?

- 缁勪欢鍙礋璐ｅ睍绀? UI 鍜? emit 浜嬩欢銆?
- 缁勪欢涓嶇洿鎺ヨ皟鐢? `cmsV2Client`銆?
- 缁勪欢涓嶇洿鎺ヤ慨鏀? Section / AtomicSection / ContentBlock 鏁版嵁銆?
- 鐪熷疄鍔ㄤ綔缁熶竴杩涘叆椤甸潰绾? action composable銆?
- 鍚屼竴鍔ㄤ綔蹇呴』鑳借 Workspace銆丼ectionTree 鍙抽敭鑿滃崟銆両nspector銆佸揩鎹烽敭绛夊叆鍙ｅ鐢ㄣ??

鎺ㄨ崘 composable锛?

```text
frontend-v2/src/composables/useSectionItemActions.ts
frontend-v2/src/composables/useAtomicSectionActions.ts
frontend-v2/src/composables/useContentBlockActions.ts
```

鍛藉悕璇箟锛?

- `removeSectionItemReference`锛氫粠褰撳墠 Section 涓Щ闄や竴涓? SectionItem 寮曠敤銆?
- `deleteAtomicSectionEntity`锛氱湡姝ｅ垹闄? AtomicSection 鏈綋锛涜繖鏄洿楂橀闄╁姩浣滐紝涓嶈兘鍜岀Щ闄ゅ紩鐢ㄦ贩鐢ㄣ??
- `removeAtomicSectionChildItem`锛氫粠 AtomicSection 鍐呴儴绉婚櫎涓?涓? ContentBlock 寮曠敤銆?
- `renameAtomicSection`锛氶噸鍛藉悕 AtomicSection 鏈綋銆?
- `createContentBlockInsideAtomicSection`锛氬湪 AtomicSection 鍐呴儴鏂板缓 ContentBlock 骞跺姞鍏? AtomicSectionItem銆?

褰撳墠 Workspace 涓? AtomicSection 鐨勨?滃垹闄も?濇寜閽涔夋槸锛?

```text
removeSectionItemReference
```

涓嶆槸锛?

```text
deleteAtomicSectionEntity
```

寮?鍙戣姹傦細

- 鍚庣画鏂板浠讳綍鍒犻櫎銆佺Щ鍔ㄣ?侀噸鍛藉悕銆乄ord 缂栬緫銆佹柊寤哄瓙鍧楃瓑鐪熷疄鍔ㄤ綔锛屽繀椤诲厛鍒ゆ柇瀹冩槸鍚﹀簲璇ヨ繘鍏? action composable銆?
- 涓嶅厑璁稿洜涓烘煇涓姩浣滄渶鍏堝嚭鐜板湪 Workspace锛屽氨鎶婄湡瀹炴柟娉曞啓姝诲湪 Workspace 鎴栧叿浣撳睍绀虹粍浠朵腑銆?
- action composable 鍙互璋冪敤 API銆佽Е鍙? server-confirmed refresh銆佽缃弽棣堟秷鎭紱灞曠ず缁勪欢涓嶅仛杩欎簺浜嬫儏銆?

璇︾粏瀹炴柦璁″垝瑙侊細

```text
docs/superpowers/plans/2026-06-17-section-action-composables.md
```
## 当前补充约定：Workspace Wrap As AtomicSection

本能力涉及的组件边界如下：

- SectionItemView 仍然只是 SectionItem 的视觉容器，只负责选中事件和操作事件 emit。
- SectionItemView 不判断自己是否可以被升级为 AtomicSection。
- ContentBlockDisplay、AtomicSectionBlock、CompositeBlock 不调用 API，不持有升级状态。
- SectionWorkspace 可以展示连续多选状态和“升级为 AtomicSection”入口，但不直接调用 API。
- SectionPage 持有 Workspace 多选状态、InsertCreateOverlay 打开状态和页面级 blocking 状态。
- useSectionItemActions 负责调用 `/api/cms-v2/sections/{sectionId}/items/wrap-as-atomic-section` 并在成功后触发 server-confirmed refresh。
- 后端应用层负责事务、连续性校验、写入和失败回滚。

ComponentLab 规则：

- 本能力不是新增独立展示组件，本轮不要求放入 ComponentLab。
- 如果后续抽出可复用 MultiSelectToolbar、BlockingOverlay 或 WrapAsAtomicSectionPanel，必须先放入 ComponentLab 用 Mock Data 验收。

禁止事项：

- 不允许在展示组件中串联多个 API 模拟升级。
- 不允许在前端 optimistic update 中先本地改结构。
- 不允许把已有 AtomicSection 包进新的 AtomicSection。
- 不允许把非连续选择交给后端“尽量处理”。
## 当前修订：Workspace 升级为 as 的组件边界

`as` 是 `AtomicSection` / 原子小节的口头简称。

组件职责：

- `SectionItemView` 仍然只负责视觉容器、选中态展示和事件 emit。
- `ContentBlockDisplay`、`CompositeBlock`、`AtomicSectionBlock` 不持有升级状态，不调用升级 API。
- `SectionWorkspace` 可以展示升级模式、已选高亮、已选数量和顶部操作入口，但不直接调用 API。
- `SectionPage` 持有升级模式状态、已选块集合、升级面板状态和页面级阻塞状态。
- `useSectionItemActions` 负责调用 `/api/cms-v2/sections/{sectionId}/items/wrap-as-atomic-section`，并在成功后触发 server-confirmed refresh。

交互边界：

- 升级状态只能由 `Workspace` 顶部右侧按钮进入。
- 升级状态下，点击可升级块只做选择 / 取消选择。
- 再次点击已选块是取消选择，不打开面板。
- 升级确认面板只能由顶部 `确认升级为 as` 按钮打开。
- 已有 `AtomicSection` 不允许参与升级。
- 允许不连续选择。
- 少于两个块时，不允许提交升级。

ComponentLab 规则：

- 如果后续抽出独立 `WrapAsAtomicSectionToolbar`、`WrapAsAtomicSectionPanel` 或升级模式高亮组件，必须先放入 `ComponentLab` 使用 Mock Data 验收。
- 当前若只在 `SectionPage` / `SectionWorkspace` 内调整页面级联动，不强制新增 ComponentLab 展示。
## 当前补充约定：SectionWorkspace 内部插入点与 CompositeBlock 渲染

本节记录 2026-06-19 已确认的组件职责边界。

### InsertPoint 在嵌套结构中的使用规则

`InsertPoint` 不只服务顶层 SectionItem。

允许出现的位置：

- SectionWorkspace 顶层 flow item 之间。
- AtomicSectionBlock 内部 child item 之间。
- CompositeBlock 内部 child relation 之间。
- 空 Section、空 AtomicSectionBlock、空 CompositeBlock 的首个插入入口。

组件职责保持不变：

- `InsertPoint` 只展示当前插入位置允许的操作按钮。
- `InsertPoint` 只通过 emit 抛出用户意图。
- `InsertPoint` 不直接创建数据。
- `InsertPoint` 不调用 API。
- `InsertPoint` 不持有 SectionPage 页面状态。

父级容器必须传入明确上下文，至少区分：

```text
Section
AtomicSection
CompositeBlock
```

因为三类插入的真实写入目标不同：

```text
Section -> SectionItem
AtomicSection -> AtomicSectionItem
CompositeBlock -> ContentBlockRelation
```

### CompositeBlock 的自身正文与子块列表

`CompositeBlock` 是组类型 ContentBlock 的工作区展示组件。

它必须同时承载两部分内容：

```text
1. CompositeBlock 自己的 docx/html 预览
2. CompositeBlock 通过 ContentBlockRelation 展开的子块列表
```

实现规则：

- CompositeBlock 不能只渲染 children。
- CompositeBlock 自己的正文预览必须复用 `ContentBlockDisplay` 或同等正文展示能力。
- CompositeBlock 的 children 继续使用 `SectionItemView` 包裹后展示。
- CompositeBlock 内部的 children 之间也必须有 `InsertPoint`。
- CompositeBlock 自身正文和 children 之间应保持文档流连续，不使用重卡片、阴影或装饰背景。

建议 view model：

```text
StructuredBlockModel
  selfContent?: ContentBlockDisplayModel
  children: StructuredBlockChildModel[]
```

其中：

- `selfContent` 表示 CompositeBlock 自身 docx/html 正文。
- `children` 表示 CompositeBlock 的组合关系展开内容。

`AtomicSectionBlock` 不等同于 ContentBlock，因此不需要 `selfContent`；它只展示自身结构信息和内部 AtomicSectionItem。

## 当前补充：SectionVariant 创建相关组件边界

本节记录 SectionPage 后续开发 `SectionVariant` 创建流程时的组件职责。当前只是文档约定，不表示组件已经实现。

### SectionTree 右键入口

唯一入口：

```text
SectionTree -> Section 根节点右键菜单 -> 新建 SectionVariant
```

组件边界：

- `SectionTree` 只负责展示右键菜单项并 emit `createSectionVariant`。
- `SectionTree` 不持有创建表单状态。
- `SectionTree` 不调用 API。
- `TeachingStructureTree` 不提供 `SectionVariant` 创建入口。

### CreateSectionVariantPanel

职责：

- 展示 `SectionVariant` 元数据表单。
- 字段包含 `Title`、`Type`、`Difficulty`、`Description`。
- `Difficulty` 只允许 `Basic / Medium / Advanced / Top`。
- 点击“下一步 / 进入选择”时 emit 元数据。

边界：

- 不调用 API。
- 不推断默认选择。
- 不持有 Workspace 选择结果。
- 不提交 `Status` 或 `SortOrder`。

### VariantSelectionMode

职责：

- 在 `SectionPage` / `SectionWorkspace` 内展示候选顶层 `SectionItem`。
- 根据后端 selection preview 结果显示默认勾选、不可选项和错误原因。
- 支持用户增减勾选。
- 允许空选择。

边界：

- 只选择顶层 `SectionItem`。
- 不选择 `AtomicSectionItem`。
- 不选择 `CompositeBlock` 子 relation。
- 不直接创建 `SectionVariant`。

### SectionVariantSelectionCandidate

建议作为可复用的行 / 块级展示组件。

展示字段：

- SectionItem 标题或推导名称。
- TargetType：`ContentBlock` / `AtomicSection`。
- 类型：知识点、例题组、练习等。
- 难度。
- 是否默认勾选。
- 不可选原因。

边界：

- 不调用 API。
- 不理解后端默认选择算法。
- 不自行读取 ContentBlock / AtomicSection 详情。

### ComponentLab 要求

如果后续新增 `CreateSectionVariantPanel`、`VariantSelectionMode` 或 `SectionVariantSelectionCandidate`，必须先放入 `ComponentLab` 使用 Mock Data 验收。

验收完成后再接入真实 `SectionPage`。
