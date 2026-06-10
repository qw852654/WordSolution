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
      '可复用 UI 组件必须先在这里用 Mock Data 验收，再进入真实页面。',
    summaryTitle: 'Lab 已就绪',
    summaryDescription:
      '这里集中展示基础组件和业务组件的 Mock Data 场景。',
    backHome: '返回首页',
    previewAction: '预览',
    scenarioCount: '场景数：',
    selectedNodeLabel: '选中节点：',
    statusPillTitle: 'StatusPill 状态',
    status: {
      ready: '就绪',
      neutral: '普通',
      muted: '弱化',
      danger: '阻塞',
    },
    checks: {
      router: 'Vue Router 已接入。',
      i18n: '可见文案通过 Vue I18n 提供。',
      mock: 'Mock Data 入口已接入。',
      api: 'CMS V2 API client 占位已接入。',
    },
    sections: {
      presentation: {
        title: 'Presentation components',
        description: '验证中性的可复用 UI 组件，再放入页面使用。',
      },
      contentBlockCard: {
        title: 'ContentBlockCard',
        description:
          '验证默认、选中、禁用和长标题等 ContentBlockCard 状态。',
      },
      sectionVariantCard: {
        title: 'SectionVariantCard',
        description:
          '验证 SectionVariantCard 的项目数、状态和禁用状态。',
      },
      focusTree: {
        title: 'FocusTree',
        description:
          '验证紧凑树结构、选中态、禁用节点和多层级展开。',
      },
    },
  },
} as const

export default zhCN
