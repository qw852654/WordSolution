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
      i18n: 'Vue I18n resources are active.',
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
    sectionVariantCreate: {
      description:
        'Fill SectionVariant metadata first, then review and select SectionItem candidates. This uses Mock Data and does not call API.',
      previewLoading: 'Loading SectionItem candidates from the selection preview API...',
      previewError: 'Failed to load SectionItem candidates.',
      stepMetadata: 'Step 1 / Metadata',
      stepSelection: 'Step 2 / Select SectionItem',
      selectionTitle: 'SectionItem candidates',
      selectionDescription:
        'Default selections come from the backend selection preview rule. Users can add or remove selections. Empty selection is allowed.',
      selectedSummary: '{selected} / {total} selected',
      defaultSelected: 'Default selected',
      unavailableReason: 'Unavailable reason',
      emptyVariantAllowed:
        'Creating an empty SectionVariant is allowed; selectedSectionItemIds may be an empty array.',
      fields: {
        title: 'Name',
        type: 'Type',
        difficulty: 'Difficulty',
        description: 'Note',
      },
      placeholders: {
        title: 'Enter SectionVariant name',
        description: 'Optional note',
      },
      validation: {
        titleRequired: 'Enter a name before continuing.',
      },
      actions: {
        cancel: 'Cancel',
        previous: 'Previous',
        next: 'Next',
        submit: 'Generate Mock payload',
      },
      types: {
        Lecture: 'Lecture',
        Exercise: 'Exercise',
        Homework: 'Homework',
        Review: 'Review',
        ExamTraining: 'Exam training',
        Custom: 'Custom',
      },
      difficulties: {
        Unset: 'Unset',
        Basic: 'Basic',
        Medium: 'Medium',
        Advanced: 'Advanced',
        Top: 'Top',
      },
    },
    sectionItemView: {
      containerLabel: 'SectionItemView container',
      actionRailLabel: 'SectionItemView action rail placeholder',
      targetType: 'Target type',
      position: 'Structure position',
      sortOrder: 'Order',
      level: 'Level',
      referenceMode: 'Reference mode',
      lockedVersion: 'Locked version',
      atomicSectionReference: 'AtomicSection reference',
      noLockedVersion: 'No locked version',
      insertBefore: 'Insert before',
      insertAfter: 'Insert after',
      insertChildContentBlock: 'Create child ContentBlock',
      moveUp: 'Move up',
      moveDown: 'Move down',
      rename: 'Rename',
      indent: 'Indent',
      outdent: 'Outdent',
      remove: 'Remove',
      openWord: 'Word edit',
      preview: 'Preview',
    },
    contentBlockDisplay: {
      difficulty: 'Difficulty',
      openWord: 'Word edit',
      refreshPreview: 'Refresh preview',
      more: 'More',
      previewState: {
        ready: 'Preview ready',
        loading: 'Preview loading',
        empty: 'No HTML preview',
        error: 'Preview failed',
      },
    },
    structuredBlock: {
      atomicSection: 'AtomicSection',
      compositeBlock: 'CompositeBlock',
      collapse: 'Collapse',
      expand: 'Expand',
      more: 'More',
      emptyTitle: 'No child content',
      atomicEmptyDescription: 'This AtomicSection does not contain ContentBlock items yet.',
      compositeEmptyDescription: 'This CompositeBlock does not contain ContentBlock items yet.',
    },
    insertPoint: {
      insert: 'Insert',
      createContentBlock: 'Create ContentBlock',
      createAtomicSection: 'Create AtomicSection',
      searchExistingBlock: 'Insert existing block',
      contentBlock: 'ContentBlock',
      atomicSection: 'AtomicSection',
      compositeBlock: 'CompositeBlock',
    },
    insertCreateOverlay: {
      dialogLabel: 'InsertCreateOverlay',
      contentBlockTitle: 'Create ContentBlock',
      atomicSectionTitle: 'Create AtomicSection',
      wrapAtomicSectionTitle: 'Upgrade to AtomicSection',
      description:
        'The item belongs to the current Section by default. SectionPage submits it to the CMS V2 API.',
      wrapDescription:
        'The selected top-level blocks will be wrapped into a new AtomicSection in one CMS V2 API transaction.',
      insertPosition: 'Insert position',
      sectionLabel: 'Section',
      titleLabel: 'Name',
      titlePlaceholder: 'Enter a name',
      contentBlockTypeLabel: 'Type',
      difficultyLabel: 'Difficulty',
      noteLabel: 'Note',
      notePlaceholder: 'Optional note',
      cancel: 'Cancel',
      submitContentBlock: 'Create ContentBlock',
      submitAtomicSection: 'Create AtomicSection',
      submitWrapAtomicSection: 'Upgrade to AtomicSection',
      titleRequired: 'Enter a name before confirming.',
    },
    sectionInspector: {
      emptyTitle: 'Select a Section node',
      emptyDescription:
        'The inspector shows details after a SectionItem or AtomicSection is selected.',
      currentSelection: 'Current selection',
      kind: 'Node kind',
      type: 'Type',
      difficulty: 'Difficulty',
      status: 'Status',
      itemCount: 'Child items',
      questionCount: 'Questions',
      disabled: 'Disabled',
      yes: 'Yes',
      no: 'No',
      notSet: 'Not set',
      preview: 'Preview',
      openWord: 'Word edit',
    },
    basicTree: {
      expand: 'Expand node',
      collapse: 'Collapse node',
      emptyTitle: 'Empty tree state',
      emptyDescription:
        'The BasicTree can render an empty state before real structure data is available.',
    },
    sectionTree: {
      title: 'SectionTree',
      description: 'Section structure tree for the left structure area.',
      emptyTitle: 'No Section structure',
      emptyDescription: 'There are no SectionItem nodes to show in this mock state.',
      nodeCount: '{count} root nodes',
      itemCount: '{count} items',
      questionCount: '{count} questions',
      kind: {
        Section: 'Section',
        AtomicSection: 'AtomicSection',
        CompositeBlock: 'CompositeBlock',
        ContentBlock: 'ContentBlock',
      },
    },
    sectionTreeContextMenu: {
      label: 'SectionTree context menu',
      target: 'Context target',
      rootRemoveDisabled: 'The Section root cannot be removed.',
      actions: {
        CreateSectionVariant: 'Create SectionVariant',
        CreateContentBlock: 'Create ContentBlock',
        CreateAtomicSection: 'Create AtomicSection',
        SearchExistingBlock: 'Insert existing block',
        Remove: 'Remove',
      },
    },
    teachingTopicTree: {
      title: 'teachingStructureTree',
      description: 'Structure overview for TeachingTopic, Section, and SectionVariant.',
      emptyTitle: 'No TeachingTopic',
      emptyDescription: 'There are no teaching topics to show in this Mock Data.',
      nodeCount: '{count} root nodes',
      status: 'Status',
      sectionCountLabel: 'Section count',
      handoutCountLabel: 'Handout count',
      archivedLabel: 'Archived',
      sectionCount: '{count} Sections',
      handoutCount: '{count} Handouts',
      archived: 'Archived',
      readOnly: 'Read-only',
      emptyTopic: 'Empty topic',
    },
    teachingTopicTreeContextMenu: {
      label: 'teachingStructureTree context menu',
      target: 'Context target',
      actions: {
        AddChild: 'Add child node',
        AddAfter: 'Add following node',
        CreateSection: 'Create Section',
        Rename: 'Rename',
        Delete: 'Delete',
      },
    },
  },
  lab: {
    eyebrow: 'Component validation',
    title: 'Component Lab',
    description:
      'Only the mock scenarios for the current development round are shown here.',
    backHome: 'Back to Home',
    scenarioCount: 'Scenario count:',
    sections: {
      sectionVariantCreate: {
        description:
          'Validate the two-step SectionVariant create UI: fill metadata, then select SectionItem candidates. The result is only a Mock payload.',
        mockSectionTitle: 'Mechanical energy conservation',
        payloadTitle: 'Mock Data payload',
        payloadDescription:
          'After submit, inspect the final structure here and confirm selectedSectionItemIds may be an empty array.',
        emptyPayload: 'No Mock Data has been submitted yet.',
        reset: 'Reset',
      },
      contentBlockDisplay: {
        title: 'ContentBlockDisplay',
        description:
          'Validate the document-flow ContentBlock body preview without title or version, plus difficulty dot, long content, empty preview, and disabled state.',
      },
      structuredBlocks: {
        title: 'AtomicSectionBlock / CompositeBlock',
        description:
          'Validate shared weak container styling, inline border title, and action placement.',
      },
      insertPoint: {
        title: 'InsertPoint',
        description:
          'Validate the weak default state and hover or keyboard focus insert affordance.',
      },
      sectionItemComposition: {
        title: 'SectionItemView composition',
        description:
          'Validate SectionItemView as the outer container for ContentBlockDisplay, AtomicSectionBlock, and CompositeBlock.',
      },
      sectionTree: {
        title: 'SectionTree',
        description:
          'Validate the left Section structure tree with hierarchy, collapse controls, selected state, disabled node, long title, and empty state.',
        selectedTitle: 'Selected node',
      },
      insertCreateOverlay: {
        title: 'InsertCreateOverlay',
        description:
          'Validate the top-level create panel for ContentBlock / AtomicSection, backdrop blur, and Mock Data submit events.',
        openContentBlock: 'Open ContentBlock panel',
        openAtomicSection: 'Open AtomicSection panel',
        openDisabled: 'Open disabled panel',
        mockSectionTitle: 'SectionPage background Mock',
        mockSectionDescription:
          'When the overlay is open, this whole background should be blurred.',
        mockInsertPointLabel: 'InsertPoint',
        feedbackTitle: 'Mock feedback',
        emptyFeedback: 'No Mock Data has been submitted yet.',
        submitted: 'Received {targetType} Mock submit: {title}',
        cancelled: 'Cancelled the {targetType} create panel.',
      },
      sectionTreeContextMenu: {
        title: 'SectionTree context menu',
        description:
          'Validate that right-click overrides the browser menu, highlights only the context target, and keeps the selected node unchanged.',
        selectedTitle: 'Selected node',
        contextTargetTitle: 'Context target',
        emptySelected: 'No selected node',
        emptyContextTarget: 'No context target',
        contextRule: 'The context target is menu-only and does not sync the Inspector selection.',
        feedbackTitle: 'Mock feedback',
        emptyFeedback: 'No menu action has been triggered yet.',
        feedback: 'Triggered {action} for node: {node}',
      },
      teachingTopicTree: {
        title: 'teachingStructureTree',
        description:
          'Validate teachingStructureTree hierarchy, expand/collapse, selected state, long titles, archived disabled nodes, and shared BasicTree behavior.',
        selectedTitle: 'Selected TeachingTopic',
        nameLabel: 'Topic name',
        emptySelected: 'No TeachingTopic selected.',
        contextTargetTitle: 'Context target',
        emptyContextTarget: 'No TeachingTopic context target.',
        contextRule: 'The context target is menu-only and does not change the selected TeachingTopic.',
        dtoRootCount: 'DTO roots: {count}',
        feedbackTitle: 'Mock feedback',
        emptyFeedback: 'No teachingStructureTree menu action has been triggered yet.',
        contextFeedback: 'Triggered {action} for TeachingTopic: {node}',
      },
    },
  },
  sectionPage: {
    eyebrow: 'SectionPage minimal shell',
    description:
      'This round validates layout only. It does not connect APIs or render real SectionItemView document flow.',
    toolbar: {
      areaLabel: 'Section page control area',
      backToTopic: 'Back to topic workspace',
      refresh: 'Refresh',
      save: 'Save structure',
      questionBankSelectLabel: 'Select question bank',
    },
    teachingTopicDrawer: {
      triggerLabel: 'Hover to open teachingStructureTree',
      dialogLabel: 'teachingStructureTree floating drawer',
      closeLabel: 'Close teachingStructureTree',
      displayRootLabel: 'Current display scope',
      allRoots: 'All root structure',
      backToParentRoot: 'Back to parent',
      backToAllRoots: 'Back to all roots',
      setDisplayRoot: 'Set display root',
      promptAddChild: 'Enter the new child node name',
      promptAddAfter: 'Enter the new following node name',
      promptRename: 'Enter the new TeachingTopic name',
      defaultChildName: 'New child node',
      defaultSiblingName: 'New following node',
      confirmDelete: 'Delete empty TeachingTopic: {title}?',
      deleteDisabledMessage:
        'Only an empty TeachingTopic with no children and no bound Section can be deleted.',
      actionFailed: 'teachingStructureTree action failed. Please try again later.',
    },
    api: {
      loadingTitle: 'Loading Section',
      loadingStatus: 'Loading',
      emptyTitle: 'No Section data',
      emptyStatus: 'Not loaded',
      loadingMessage: 'Loading Section data from the CMS V2 API.',
      errorTitle: 'Failed to load Section data',
      loadError: 'Unable to load Section data.',
    },
    meta: {
      section: 'Section',
      sectionId: 'Section ID',
      teachingTopic: 'TeachingTopic',
      status: 'Status',
    },
    sectionVariantCreate: {
      dialogLabel: 'Create SectionVariant',
      closeLabel: 'Close SectionVariant create panel',
      previewFailed: 'Selection preview failed. Please try again.',
      pendingPayloadFeedback:
        'Generated pending SectionVariant payload with {count} selected SectionItems.',
      pendingPayloadTitle: 'Pending SectionVariant payload',
      pendingPayloadDescription:
        'This payload is for review only in this round. It has not been submitted to the create API.',
    },
    structure: {
      title: 'SectionStructurePanel',
      description: 'Left shell area reserved for the current Section structure tree.',
      emptyTitle: 'Structure tree pending',
      emptyDescription:
        'SectionItem structure, selection, and location behavior are intentionally not implemented in this round.',
    },
    workspace: {
      title: 'SectionWorkspace',
      description: 'Center shell area reserved for the SectionItemView document flow.',
      mainColumnLabel: 'Section document flow main column',
      teachingNoteColumnLabel: 'TeachingNoteColumn reserved area',
      teachingNoteColumnDescription:
        'Teaching Note Mode will show contextual notes beside content later. This is not the right-side Inspector.',
      emptyTitle: 'Insert the first block',
      emptyDescription:
        'This Section has no content yet. Create a ContentBlock, create an AtomicSection, or insert an existing block.',
      wrap: {
        enterAction: 'Upgrade to as',
        confirmAction: 'Confirm upgrade to as',
        clearAction: 'Clear selection',
        exitAction: 'Exit upgrade',
        invalidSelection: 'Select at least two top-level ContentBlock or CompositeBlock items.',
        atomicSectionNotAllowed: 'Existing as items cannot be upgraded.',
        notWrappableLabel: 'This block cannot be upgraded to as',
        selectedCount: '{count} blocks selected',
        action: 'Upgrade to AtomicSection',
        invalidRange:
          'Select at least two continuous top-level ContentBlock or CompositeBlock items.',
        selectedPositionLabel: '{count} selected blocks',
        blockingTitle: 'Upgrading to AtomicSection',
        blockingDescription:
          'SectionPage is temporarily locked while the CMS V2 API completes the transaction.',
        feedbackSuccess: 'Upgraded selected blocks to AtomicSection: {title}.',
      },
      insertPanel: {
        insertPositionLabel: 'Current selected insert position',
        firstInsertPositionLabel: 'First block',
        feedbackCreateContentBlock: 'Selected this position for creating ContentBlock.',
        feedbackCreateAtomicSection: 'Selected this position for creating AtomicSection.',
        feedbackSubmitting: 'Submitting to the CMS V2 API...',
        feedbackCreateSubmitted: 'Created {targetType} and added it to the current Section: {title}.',
        feedbackCreateAtomicChildSubmitted:
          'Created child ContentBlock inside the current AtomicSection: {title}.',
        feedbackCreateFailed: 'Create failed. Please try again.',
        feedbackMissingSection: 'No valid Section is available, so a block cannot be created.',
        feedbackSearchExistingBlock:
          'Selected this position for inserting an existing block. BlockSearchPicker will be connected later.',
      },
      atomicSectionActions: {
        insertChildPosition: 'Inside AtomicSection: {title}',
        renamePrompt: 'Rename AtomicSection',
        removeConfirm:
          'Remove this AtomicSection reference from the current Section? The AtomicSection and its child ContentBlocks will not be deleted. AtomicSection: {title}',
        operationFailed: 'AtomicSection operation failed. Please try again.',
        untitledContentBlock: 'Untitled ContentBlock',
      },
      atomicSectionItemActions: {
        removeConfirm:
          'Remove this ContentBlock reference from the current AtomicSection? The source ContentBlock will not be deleted. ContentBlock: {title}',
        operationFailed: 'AtomicSection child ContentBlock operation failed. Please try again.',
      },
      contentBlockActions: {
        wordEditPending: 'ContentBlock Word editing API is not connected yet',
        wordEditStarted:
          'ContentBlock Word edit session started. The page will sync automatically after Word is saved.',
        wordEditSyncing: 'Syncing ContentBlock Word edits...',
        wordEditSynced: 'ContentBlock Word edits synced automatically.',
        wordEditNoChanges: 'ContentBlock Word edits have no changes.',
        wordEditCancelled: 'ContentBlock Word edit session cancelled.',
        wordEditFailed: 'ContentBlock Word edit session sync failed.',
        removeConfirm:
          'Remove this ContentBlock reference from the current Section? The source ContentBlock will not be deleted. ContentBlock: {title}',
        operationFailed: 'ContentBlock operation failed. Please try again.',
      },
      contentBlockRelationActions: {
        removeConfirm:
          'Remove this child ContentBlock reference from the current CompositeBlock? The source ContentBlock will not be deleted. ContentBlock: {title}',
        operationFailed: 'CompositeBlock child ContentBlock operation failed. Please try again.',
      },
      mock: {
        contentBlockPlaceholderTitle: 'ContentBlockDisplay placeholder',
        contentBlockPlaceholderDescription:
          'This content is passed through the slot by SectionWorkspace. SectionItemView itself does not display titles, status, or versions.',
        atomicSectionPlaceholderTitle: 'AtomicSectionBlock placeholder',
        atomicSectionPlaceholderDescription:
          'This validates a parent SectionItemView carrying child SectionItemView containers.',
        childKnowledgePlaceholderTitle: 'Child ContentBlockDisplay placeholder',
        childKnowledgePlaceholderDescription:
          'A child SectionItemView inside the parent container, used to validate hierarchy and hover actions.',
        childExamplePlaceholderTitle: 'Child example display placeholder',
        childExamplePlaceholderDescription:
          'The child still stays as a pure container; ContentBlockDisplay will provide the real content later.',
        disabledPlaceholderTitle: 'Disabled container placeholder',
        disabledPlaceholderDescription:
          'Used to validate muted state and disabled right-side action buttons.',
      },
    },
  },
} as const

export default en
