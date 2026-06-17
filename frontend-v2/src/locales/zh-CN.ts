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
    statusDescription: '本阶段为后续 SectionPage 和 HandoutPage 实现准备稳定结构。',
    apiBaseLabel: 'API 基准路径',
    boundaryTitle: '阶段边界',
    boundaryDescription: '当前阶段不接真实业务 API，也不实现编辑器工作流。',
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
      emptyDescription: '真实教学主题数据和工作台操作会在后续阶段接入。',
    },
    sections: {
      eyebrow: 'Section 编辑器',
      title: 'Section 占位页',
      description: '这里预留给后续 SectionPage，不在当前阶段实现工作流。',
      emptyTitle: 'Section 编辑器基础入口已就绪',
      emptyDescription: '该路由接收 section id，等待阶段 4 SectionPage 实现。',
    },
    handouts: {
      eyebrow: '讲义编辑器',
      title: '讲义占位页',
      description: '这里预留给后续 HandoutPage，不在当前阶段实现讲义工作流。',
      emptyTitle: '讲义编辑器基础入口已就绪',
      emptyDescription: '该路由接收 handout version id，等待阶段 5 HandoutPage 实现。',
    },
    contentBlocks: {
      eyebrow: '内容资产库',
      title: 'ContentBlock',
      description: '这里预留给后续可复用内容资产工作流。',
      emptyTitle: '内容库占位页',
      emptyDescription:
        'ContentBlock 列表、筛选和详情工作流会在编辑器基础阶段之后加入。',
    },
    contentBlockDetail: {
      eyebrow: 'ContentBlock 详情',
      title: 'ContentBlock 占位页',
      description: '这里预留给 ContentBlock 详情视图和编辑入口。',
      emptyTitle: 'ContentBlock 详情入口已就绪',
      emptyDescription: '该路由接收 content block id，等待内容工作流阶段接入。',
    },
    outputs: {
      eyebrow: '输出形式',
      title: '输出占位页',
      description: '这里预留给输出形式和生成文件。',
      emptyTitle: '输出工作流占位页',
      emptyDescription: 'Word 生成、输出形式和生成文件当前阶段不实现。',
    },
  },
  emptyState: {
    lab: {
      title: 'EmptyState 组件',
      description: '这里验收中性空状态、可选图标和操作入口。',
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
    contentBlockDisplay: {
      difficulty: '难度',
      openWord: 'Word 编辑',
      refreshPreview: '刷新预览',
      more: '更多',
      previewState: {
        ready: '预览已就绪',
        loading: '预览加载中',
        empty: '暂无 HTML 预览',
        error: '预览加载失败',
      },
    },
    structuredBlock: {
      atomicSection: 'AtomicSection',
      compositeBlock: 'CompositeBlock',
      collapse: '折叠',
      more: '更多',
      emptyTitle: '暂无子内容',
      atomicEmptyDescription: '这个 AtomicSection 还没有放入 ContentBlock。',
      compositeEmptyDescription: '这个 CompositeBlock 还没有放入 ContentBlock。',
    },
    insertPoint: {
      insert: '插入',
      createContentBlock: '新建 ContentBlock',
      createAtomicSection: '新建 AtomicSection',
      searchExistingBlock: '插入已有块',
      contentBlock: 'ContentBlock',
      atomicSection: 'AtomicSection',
      compositeBlock: 'CompositeBlock',
    },
    insertCreateOverlay: {
      dialogLabel: 'InsertCreateOverlay',
      contentBlockTitle: '新建 ContentBlock',
      atomicSectionTitle: '新建 AtomicSection',
      description: '填写 Mock Data 字段后，只向父级发送提交事件。',
      insertPosition: '插入位置',
      titleLabel: '名称',
      titlePlaceholder: '输入名称',
      contentBlockTypeLabel: '类型',
      difficultyLabel: '难度',
      noteLabel: '备注',
      notePlaceholder: '可选备注',
      cancel: '取消',
      submitContentBlock: '新建 ContentBlock',
      submitAtomicSection: '新建 AtomicSection',
      titleRequired: '请输入名称后再确认。',
    },
    sectionInspector: {
      kind: '节点类型',
      type: '类型',
      difficulty: '难度',
      itemCount: '子项数量',
      questionCount: '题目数量',
      disabled: '是否禁用',
      yes: '是',
      no: '否',
      notSet: '未设置',
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
    basicTree: {
      expand: '展开节点',
      collapse: '折叠节点',
      emptyTitle: '空树状态',
      emptyDescription: 'BasicTree 可以在真实结构数据接入前显示空状态。',
    },
    sectionTree: {
      title: 'SectionTree',
      description: '左侧 structure 区域使用的 Section 结构树。',
      emptyTitle: '暂无 Section 结构',
      emptyDescription: '当前 Mock Data 中没有可展示的 SectionItem 节点。',
      nodeCount: '{count} 个根节点',
      itemCount: '{count} 项',
      questionCount: '{count} 题',
      kind: {
        Section: 'Section',
        AtomicSection: 'AtomicSection',
        CompositeBlock: 'CompositeBlock',
        ContentBlock: 'ContentBlock',
      },
    },
    sectionTreeContextMenu: {
      label: 'SectionTree 右键菜单',
      target: '右键目标',
      rootRemoveDisabled: 'Section 根节点不能移除',
      actions: {
        CreateContentBlock: '新建 ContentBlock',
        CreateAtomicSection: '新建 AtomicSection',
        SearchExistingBlock: '插入已有块',
        Remove: '移除',
      },
    },
  },
  lab: {
    eyebrow: '组件验收',
    title: 'ComponentLab',
    description: '这里仅保留当前开发轮次需要验收的 Mock Data 场景。',
    backHome: '返回首页',
    scenarioCount: '场景数：',
    sections: {
      contentBlockDisplay: {
        title: 'ContentBlockDisplay',
        description:
          '验收文档流里的 ContentBlock 正文展示：无标题、无版本、难度小点、正文预览、长正文、无预览和禁用态。',
      },
      structuredBlocks: {
        title: 'AtomicSectionBlock / CompositeBlock',
        description:
          '验收两个结构块是否共用弱边框容器，标题和操作区是否贴在线框上。',
      },
      insertPoint: {
        title: 'InsertPoint',
        description:
          '验收插入点默认弱化，鼠标悬停或键盘聚焦后显示插入入口。',
      },
      sectionItemComposition: {
        title: 'SectionItemView composition',
        description:
          '验收 SectionItemView 作为外层容器承载 ContentBlockDisplay、AtomicSectionBlock 和 CompositeBlock。',
      },
      sectionTree: {
        title: 'SectionTree',
        description:
          '验收左侧 Section 结构树的层级、折叠按钮、选中态、禁用节点、长标题和空状态。',
        selectedTitle: '当前选中节点',
      },
      insertCreateOverlay: {
        title: 'InsertCreateOverlay',
        description:
          '验收新建 ContentBlock / AtomicSection 的最上层插入面板、背景模糊和 Mock Data 提交事件。',
        openContentBlock: '打开 ContentBlock 面板',
        openAtomicSection: '打开 AtomicSection 面板',
        openDisabled: '打开禁用态面板',
        mockSectionTitle: 'SectionPage 背景 Mock',
        mockSectionDescription: '打开面板后，这块背景区域应该整体模糊。',
        mockInsertPointLabel: 'InsertPoint',
        feedbackTitle: 'Mock 反馈',
        emptyFeedback: '尚未提交 Mock Data。',
        submitted: '已收到 {targetType} Mock 提交：{title}',
        cancelled: '已取消 {targetType} 新建面板。',
      },
      sectionTreeContextMenu: {
        title: 'SectionTree 右键菜单',
        description:
          '右键树节点时覆盖浏览器默认菜单，只高亮右键目标，不改变当前选中节点。',
        selectedTitle: '当前选中节点',
        contextTargetTitle: '当前右键目标',
        emptySelected: '尚未选中节点',
        emptyContextTarget: '尚未右键节点',
        contextRule: '右键目标只用于菜单上下文，不同步 Inspector 选中态。',
        feedbackTitle: 'Mock 反馈',
        emptyFeedback: '尚未触发菜单动作。',
        feedback: '已触发 {action}，目标节点：{node}',
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
      questionBankSelectLabel: '选择题库',
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
      insertPanel: {
        insertPositionLabel: '当前选中的插入位置',
        feedbackCreateContentBlock: '已选择在此处新建 ContentBlock',
        feedbackCreateAtomicSection: '已选择在此处新建 AtomicSection',
        feedbackCreateSubmitted: '已收到 Mock 提交：新建 {targetType}，名称为 {title}。',
        feedbackSearchExistingBlock:
          '已选择在此处插入已有块；BlockSearchPicker 后续接入。',
      },
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
