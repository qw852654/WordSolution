const en = {
  common: {
    appName: 'CMS V2',
  },
  shell: {
    subtitle: 'Local teaching content management workspace',
    stage: 'Stage 3',
    primaryNavigation: 'Primary navigation',
  },
  navigation: {
    topics: 'Topics',
    sections: 'Sections',
    handouts: 'Handouts',
    contentBlocks: 'Content blocks',
    outputs: 'Outputs',
    lab: 'Component Lab',
    descriptions: {
      topics: 'Teaching topic workspace',
      sections: 'Section editor entry',
      handouts: 'Handout editor entry',
      contentBlocks: 'Reusable content assets',
      outputs: 'Output forms and generated files',
      lab: 'Mock component validation',
    },
  },
  home: {
    stageLabel: 'Stage 3',
    title: 'CMS V2 Frontend Foundation',
    description:
      'The V2 frontend now has a stable application shell, placeholder routes, i18n, mock data, and component validation entry points.',
    statusTitle: 'Foundation status',
    statusDescription:
      'This stage prepares the structure for future SectionPage and HandoutPage implementation.',
    apiBaseLabel: 'API base',
    boundaryTitle: 'Stage boundary',
    boundaryDescription:
      'This stage does not connect real business APIs or implement editor workflows.',
    boundaryBody:
      'Use the topic workspace and Component Lab entries to verify routing, layout, and reusable component foundations.',
    openLab: 'Open Component Lab',
    openTopics: 'Open Topics',
    checklist: {
      router: 'Placeholder routes are wired.',
      i18n: 'English i18n resources are active.',
      mock: 'Mock component data is available for validation.',
      api: 'The CMS V2 API base remains reserved for later integrations.',
    },
  },
  routes: {
    placeholder: {
      contextTitle: 'Route context',
      contextDescription: 'This panel shows the active placeholder route and parameters.',
      pathLabel: 'Current path',
      paramLabel: 'Route parameter',
    },
    topics: {
      eyebrow: 'Teaching topic workspace',
      title: 'Topics',
      description:
        'This route will host the teaching topic workspace before entering section or handout editors.',
      emptyTitle: 'Topic workspace placeholder',
      emptyDescription:
        'Real teaching topic data and workspace actions will be connected in a later stage.',
    },
    sections: {
      eyebrow: 'Section editor',
      title: 'Section placeholder',
      description:
        'This route is reserved for the future SectionPage editor without implementing the workflow yet.',
      emptyTitle: 'Section editor foundation ready',
      emptyDescription:
        'The route accepts a section id and waits for the Stage 4 SectionPage implementation.',
    },
    handouts: {
      eyebrow: 'Handout editor',
      title: 'Handout placeholder',
      description:
        'This route is reserved for the future HandoutPage editor without implementing the workflow yet.',
      emptyTitle: 'Handout editor foundation ready',
      emptyDescription:
        'The route accepts a handout version id and waits for the Stage 5 HandoutPage implementation.',
    },
    contentBlocks: {
      eyebrow: 'Content asset library',
      title: 'Content blocks',
      description:
        'This placeholder keeps the content library route available for future reusable asset workflows.',
      emptyTitle: 'Content library placeholder',
      emptyDescription:
        'Content block listing, filtering, and detail workflows will be added after the editor foundations.',
    },
    contentBlockDetail: {
      eyebrow: 'Content block detail',
      title: 'Content block placeholder',
      description:
        'This route is reserved for content block detail views and editing entry points.',
      emptyTitle: 'Content block detail foundation ready',
      emptyDescription:
        'The route accepts a content block id and waits for the content workflow stage.',
    },
    outputs: {
      eyebrow: 'Output form',
      title: 'Output placeholder',
      description:
        'This placeholder keeps output form and generated file routes available for later stages.',
      emptyTitle: 'Output workflow placeholder',
      emptyDescription:
        'Word generation, output forms, and generated files are intentionally not implemented in this stage.',
    },
  },
  emptyState: {
    lab: {
      title: 'Empty state component',
      description:
        'This verifies a neutral empty state with an optional icon and action slot.',
      action: 'Mock action',
    },
  },
  components: {
    contentBlockCard: {
      role: 'Role',
      blockType: 'Block type',
      difficulty: 'Difficulty',
      version: 'Version',
      open: 'Open',
    },
    sectionVariantCard: {
      purpose: 'Purpose',
      difficulty: 'Difficulty',
      itemCount: 'Items',
      open: 'Open',
    },
    focusTree: {
      expand: 'Expand node',
      collapse: 'Collapse node',
      emptyTitle: 'Empty tree state',
      emptyDescription:
        'The FocusTree can render an empty state before real structure data is available.',
    },
  },
  lab: {
    eyebrow: 'Component validation',
    title: 'Component Lab',
    description:
      'Reusable UI pieces must be verified here with mock data before they enter real pages.',
    summaryTitle: 'Lab ready',
    summaryDescription:
      'The lab route hosts foundation components and mock business components for review.',
    backHome: 'Back to Home',
    previewAction: 'Preview',
    scenarioCount: 'Scenario count:',
    selectedNodeLabel: 'Selected node:',
    statusPillTitle: 'Status pill states',
    status: {
      ready: 'Ready',
      neutral: 'Neutral',
      muted: 'Muted',
      danger: 'Blocked',
    },
    checks: {
      router: 'Router integration is active.',
      i18n: 'Visible text is served through Vue I18n.',
      mock: 'Mock data entry is connected.',
      api: 'CMS V2 client placeholder is connected.',
    },
    sections: {
      presentation: {
        title: 'Presentation components',
        description: 'Validate neutral, reusable UI pieces before using them in pages.',
      },
      contentBlockCard: {
        title: 'ContentBlockCard',
        description:
          'Validate default, selected, disabled, and long-title content block card states.',
      },
      sectionVariantCard: {
        title: 'SectionVariantCard',
        description:
          'Validate section variant cards with item counts, status, and disabled state.',
      },
      focusTree: {
        title: 'FocusTree',
        description:
          'Validate a compact tree with selection, disabled nodes, and expandable levels.',
      },
    },
  },
} as const

export default en
