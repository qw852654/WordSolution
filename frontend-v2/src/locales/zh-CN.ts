const zhCN = {
  common: {
    appName: 'CMS V2',
  },
  shell: {
    subtitle: '本地教学内容管理工作台',
    stage: '阶段 3',
    primaryNavigation: '主导航',
  },
  navigation: {
    topics: '教学主题',
    sections: 'Section',
    handouts: '讲义',
    contentBlocks: 'ContentBlock',
    outputs: '输出',
    lab: 'ComponentLab',
    descriptions: {
      topics: '进入教学主题工作台',
      sections: '进入 Section 编辑入口',
      handouts: '进入讲义编辑入口',
      contentBlocks: '管理可复用内容资产',
      outputs: '查看输出形式和生成文件',
      lab: '用 Mock Data 验收组件',
    },
  },
  home: {
    stageLabel: '阶段 3',
    title: 'CMS V2 前端基础骨架',
    description:
      '当前 V2 前端已经具备稳定的应用外壳、占位路由、Vue I18n、Mock Data 和组件验收入口。',
    statusTitle: '基础状态',
    statusDescription:
      '本阶段为后续 SectionPage 和 HandoutPage 实现准备稳定结构。',
    apiBaseLabel: 'API 基准路径',
    boundaryTitle: '阶段边界',
    boundaryDescription:
      '当前阶段不接真实业务 API，也不实现编辑器工作流。',
    boundaryBody:
      '可以通过教学主题入口和 ComponentLab 检查路由、布局和可复用组件基础。',
    openLab: '打开 ComponentLab',
    openTopics: '打开教学主题',
    checklist: {
      router: '占位路由已接入。',
      i18n: '默认界面语言为 zh-CN，并通过 Vue I18n 提供文案。',
      mock: 'Mock Data 已接入组件验收入口。',
      api: 'CMS V2 API 基准路径已为后续集成保留。',
    },
  },
  routes: {
    placeholder: {
      contextTitle: '路由上下文',
      contextDescription: '这里显示当前占位路由和参数。',
      pathLabel: '当前路径',
      paramLabel: '路由参数',
    },
    topics: {
      eyebrow: 'TeachingTopic 工作台',
      title: '教学主题',
      description:
        '这里将承载教学主题工作台，然后再进入 Section 或讲义编辑器。',
      emptyTitle: '教学主题工作台占位',
      emptyDescription:
        '真实教学主题数据和工作台操作会在后续阶段接入。',
    },
    sections: {
      eyebrow: 'Section 编辑器',
      title: 'Section 占位页',
      description:
        '这里预留给后续 SectionPage，不在当前阶段实现工作流。',
      emptyTitle: 'Section 编辑器基础入口已就绪',
      emptyDescription:
        '该路由接收 section id，等待阶段 4 SectionPage 实现。',
    },
    handouts: {
      eyebrow: '讲义编辑器',
      title: '讲义占位页',
      description:
        '这里预留给后续 HandoutPage，不在当前阶段实现讲义工作流。',
      emptyTitle: '讲义编辑器基础入口已就绪',
      emptyDescription:
        '该路由接收 handout version id，等待阶段 5 HandoutPage 实现。',
    },
    contentBlocks: {
      eyebrow: '内容资产库',
      title: 'ContentBlock',
      description:
        '这里预留给后续可复用内容资产工作流。',
      emptyTitle: '内容库占位页',
      emptyDescription:
        'ContentBlock 列表、筛选和详情工作流会在编辑器基础阶段之后加入。',
    },
    contentBlockDetail: {
      eyebrow: 'ContentBlock 详情',
      title: 'ContentBlock 占位页',
      description:
        '这里预留给 ContentBlock 详情视图和编辑入口。',
      emptyTitle: 'ContentBlock 详情入口已就绪',
      emptyDescription:
        '该路由接收 content block id，等待内容工作流阶段接入。',
    },
    outputs: {
      eyebrow: '输出形式',
      title: '输出占位页',
      description:
        '这里预留给输出形式和生成文件。',
      emptyTitle: '输出工作流占位页',
      emptyDescription:
        'Word 生成、输出形式和生成文件当前阶段不实现。',
    },
  },
  emptyState: {
    lab: {
      title: 'EmptyState 组件',
      description:
        '这里验证中性空状态、可选图标和操作入口。',
      action: 'Mock 操作',
    },
  },
  components: {
    contentBlockCard: {
      role: '角色',
      blockType: '块类型',
      difficulty: '难度',
      version: '版本',
      open: '打开',
    },
    sectionVariantCard: {
      purpose: '用途',
      difficulty: '难度',
      itemCount: '项目数',
      open: '打开',
    },
    sectionItemView: {
      containerLabel: 'SectionItemView 容器',
      actionRailLabel: 'SectionItemView 右侧操作占位',
      targetType: '目标类型',
      position: '结构位置',
      sortOrder: '顺序',
      level: '层级',
      referenceMode: '引用模式',
      lockedVersion: '锁定版本',
      atomicSectionReference: 'AtomicSection 引用',
      noLockedVersion: '不锁定版本',
      insertBefore: '前插',
      insertAfter: '后插',
      moveUp: '上移',
      moveDown: '下移',
      indent: '缩进',
      outdent: '反缩进',
      remove: '移除',
      openWord: 'Word 编辑',
      preview: '预览',
    },
    sectionInspector: {
      emptyTitle: '请选择一个 Section 节点',
      emptyDescription:
        '选中 SectionItem 或 AtomicSection 后，这里会显示右侧检查信息。',
      currentSelection: '当前选中',
      status: '状态',
      position: '结构位置',
      referenceMode: '引用模式',
      lockedVersion: '锁定版本',
      note: '备注',
      preview: '预览',
      openWord: 'Word 编辑',
    },
    focusTree: {
      expand: '展开节点',
      collapse: '折叠节点',
      emptyTitle: '空树状态',
      emptyDescription:
        'FocusTree 可以在真实结构数据接入前显示空状态。',
    },
  },
  lab: {
    eyebrow: '组件验收',
    title: 'ComponentLab',
    description:
      '这里仅保留当前开发轮次需要验收的 Mock Data 场景。',
    backHome: '返回首页',
    scenarioCount: '场景数：',
    sections: {
      sectionWorkspace: {
        title: 'SectionWorkspace skeleton',
        description:
          '本轮验收 SectionWorkspace 的短信息区、文档流区域，以及未来 TeachingNoteColumn 的结构预留。',
      },
      sectionItemView: {
        title: 'SectionItemView',
        description:
          '本轮验收 SectionItemView 作为纯容器的表现：允许子级、默认无边框、hover 后显示右侧操作区。',
      },
    },
  },
  sectionPage: {
    eyebrow: 'SectionPage 最小骨架',
    description:
      '本轮只验证页面结构，不接 API，不展示真实 SectionItemView 文档流。',
    toolbar: {
      areaLabel: 'Section 页面工具控件区',
      backToTopic: '返回主题工作台',
      refresh: '刷新',
      save: '保存结构',
    },
    meta: {
      section: 'Section',
      sectionId: 'Section ID',
      teachingTopic: 'TeachingTopic',
      status: '状态',
    },
    structure: {
      title: 'SectionStructurePanel',
      description: '左侧结构区空壳，后续用于承载当前 Section 的结构树。',
      emptyTitle: '结构树待接入',
      emptyDescription:
        '后续会在这里显示 SectionItem 结构。本轮不实现树节点、选中或定位。',
    },
    workspace: {
      title: 'SectionWorkspace',
      description: '中间工作区骨架，后续用于承载 SectionItemView 文档流。',
      mainColumnLabel: 'Section 文档流主列',
      teachingNoteColumnLabel: 'TeachingNoteColumn 预留区',
      teachingNoteColumnDescription:
        '后续 Teaching Note Mode 会在这里显示与内容块并排的教学备注；它不是右侧 Inspector。',
      emptyTitle: 'SectionItemView 文档流待接入',
      emptyDescription:
        '本轮不展示假正文、不实现 ContentBlockDisplay、AtomicSectionBlock 或 InsertPoint。',
      mock: {
        contentBlockPlaceholderTitle: 'ContentBlockDisplay 占位',
        contentBlockPlaceholderDescription:
          '这段内容由 SectionWorkspace 通过 slot 放入；SectionItemView 本身不展示标题、状态或版本。',
        atomicSectionPlaceholderTitle: 'AtomicSectionBlock 占位',
        atomicSectionPlaceholderDescription:
          '这里用于验收父级 SectionItemView 承载子级 SectionItemView 的结构。',
        childKnowledgePlaceholderTitle: '子级 ContentBlockDisplay 占位',
        childKnowledgePlaceholderDescription:
          '这是父级容器中的子级 SectionItemView，用来验收层级显示和 hover 操作区。',
        childExamplePlaceholderTitle: '子级例题显示占位',
        childExamplePlaceholderDescription:
          '子级仍然保持纯容器语义，具体内容后续由 ContentBlockDisplay 承载。',
        disabledPlaceholderTitle: '禁用态容器占位',
        disabledPlaceholderDescription:
          '用于验收弱化状态和右侧操作按钮禁用状态。',
      },
    },
  },
} as const

export default zhCN
