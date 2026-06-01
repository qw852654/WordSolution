(function () {
  const MAX_RENDER_DEPTH = 10;

  const temporaryTopicTree = {
    id: "topic-function-relation",
    title: "功能关系",
    children: [
      {
        id: "topic-mechanical-energy",
        title: "机械能守恒",
        children: [
          { id: "topic-vertical-circle", title: "竖直圆轨道" },
          { id: "topic-rod-model", title: "杆模型" },
          { id: "topic-ball-model", title: "球模型" },
        ],
      },
    ],
  };

  const els = {
    pageEyebrow: document.getElementById("pageEyebrow"),
    pageTitle: document.getElementById("pageTitle"),
    pageBreadcrumb: document.getElementById("pageBreadcrumb"),
    sectionSelect: document.getElementById("sectionSelect"),
    refreshButton: document.getElementById("refreshButton"),
    newSectionButton: document.getElementById("newSectionButton"),
    exportSectionButton: document.getElementById("exportSectionButton"),
    insertAtEndButton: document.getElementById("insertAtEndButton"),
    newAtEndButton: document.getElementById("newAtEndButton"),
    topicDrawerTab: document.getElementById("topicDrawerTab"),
    topicDrawerBackdrop: document.getElementById("topicDrawerBackdrop"),
    topicDrawer: document.getElementById("topicDrawer"),
    closeTopicDrawerButton: document.getElementById("closeTopicDrawerButton"),
    topicTree: document.getElementById("topicTree"),
    objectOutlineTree: document.getElementById("objectOutlineTree"),
    outlineCount: document.getElementById("outlineCount"),
    documentTitle: document.getElementById("documentTitle"),
    documentFlow: document.getElementById("documentFlow"),
    inspectorKind: document.getElementById("inspectorKind"),
    inspectorPanel: document.getElementById("inspectorPanel"),
    statusBar: document.getElementById("statusBar"),
    resourceModal: document.getElementById("resourceModal"),
    closeResourceModalButton: document.getElementById("closeResourceModalButton"),
    resourceSearchInput: document.getElementById("resourceSearchInput"),
    referenceModeSelect: document.getElementById("referenceModeSelect"),
    resourceResults: document.getElementById("resourceResults"),
    createModal: document.getElementById("createModal"),
    createBlockForm: document.getElementById("createBlockForm"),
    closeCreateModalButton: document.getElementById("closeCreateModalButton"),
    cancelCreateBlockButton: document.getElementById("cancelCreateBlockButton"),
    createBlockTitleInput: document.getElementById("createBlockTitleInput"),
    createBlockTypeSelect: document.getElementById("createBlockTypeSelect"),
    createBlockStructureSelect: document.getElementById("createBlockStructureSelect"),
    createBlockNoteInput: document.getElementById("createBlockNoteInput"),
    blockMoreMenu: document.getElementById("blockMoreMenu"),
  };

  const state = {
    sections: [],
    selectedSectionId: null,
    sectionDetail: null,
    sectionItems: [],
    nodes: [],
    nodeMap: new Map(),
    selectedNodeId: null,
    collapsedBlockIds: new Set(),
    topicCollapsedIds: new Set(),
    selectedTopicId: "topic-mechanical-energy",
    topicDrawerOpen: false,
    loadingSections: false,
    loadingSection: false,
    exportingSection: false,
    contentCandidates: [],
    loadingCandidates: false,
    insertContext: null,
    draggingNodeId: null,
    activeSession: null,
    pollTimer: null,
    statusMessage: "正在初始化...",
  };

  const finalSessionStates = new Set(["已同步", "无变化", "失败", "已取消"]);

  function apiBase() {
    return window.QuestionBankContext.apiBase();
  }

  function sectionApiRoot() {
    return `${apiBase()}/小节`;
  }

  function contentApiRoot() {
    return `${apiBase()}/内容块`;
  }

  async function requestJson(url, options = {}) {
    const response = await fetch(url, {
      cache: "no-store",
      headers: {
        "Content-Type": "application/json; charset=utf-8",
        ...(options.headers || {}),
      },
      ...options,
    });

    if (!response.ok) {
      const message = await response.text();
      throw new Error(message || `请求失败：${response.status}`);
    }

    if (response.status === 204) {
      return null;
    }

    return response.json();
  }

  async function requestBlob(url, options = {}) {
    const response = await fetch(url, {
      cache: "no-store",
      ...options,
    });

    if (!response.ok) {
      const message = await response.text();
      throw new Error(message || `请求失败：${response.status}`);
    }

    return {
      blob: await response.blob(),
      fileName: getDownloadFileName(response.headers.get("content-disposition")),
    };
  }

  function render() {
    renderTopbar();
    renderTopicDrawer();
    renderOutlineTree();
    renderDocumentFlow();
    renderInspector();
    renderStatus();
  }

  function renderTopbar() {
    const section = state.sectionDetail;
    els.pageEyebrow.textContent = "小节编辑器 · 真实 API";
    els.pageTitle.textContent = section ? displaySectionName(section) : "小节编排";
    els.pageBreadcrumb.textContent = section
      ? `当前题库 / ${displaySectionName(section)}`
      : "当前题库 / 未选择小节";
    els.documentTitle.textContent = section ? displaySectionName(section) : "小节展开内容";

    els.sectionSelect.innerHTML = [
      `<option value="">${state.loadingSections ? "正在读取小节..." : "选择小节"}</option>`,
      ...state.sections.map((item) => {
        const id = idOf(item);
        const selected = id === state.selectedSectionId ? " selected" : "";
        return `<option value="${escapeHtml(id)}"${selected}>${escapeHtml(displaySectionName(item))}</option>`;
      }),
    ].join("");

    els.exportSectionButton.disabled = !state.selectedSectionId || state.exportingSection || state.loadingSection;
    els.exportSectionButton.textContent = state.exportingSection ? "正在导出..." : "导出当前小节 Word";
  }

  function renderTopicDrawer() {
    els.topicDrawer.classList.toggle("is-hidden", !state.topicDrawerOpen);
    els.topicDrawerBackdrop.classList.toggle("is-hidden", !state.topicDrawerOpen);
    els.topicDrawerTab.setAttribute("aria-expanded", String(state.topicDrawerOpen));
    els.topicTree.innerHTML = renderTopicTree([temporaryTopicTree]);
  }

  function renderTopicTree(nodes) {
    return `<div class="topic-tree-list">${nodes.map((node) => renderTopicNode(node, 0)).join("")}</div>`;
  }

  function renderTopicNode(node, level) {
    const children = Array.isArray(node.children) ? node.children : [];
    const hasChildren = children.length > 0;
    const collapsed = state.topicCollapsedIds.has(node.id);
    const active = node.id === state.selectedTopicId;
    return `
      <div class="topic-tree-item">
        <div class="topic-tree-row" style="--topic-level:${Number(level)}">
          ${hasChildren
            ? `<button class="topic-toggle" type="button" data-topic-toggle="${escapeHtml(node.id)}" aria-label="展开或收起">${collapsed ? "›" : "⌄"}</button>`
            : "<span></span>"}
          <button class="topic-node${active ? " is-active" : ""}" type="button" data-topic-id="${escapeHtml(node.id)}">${escapeHtml(node.title)}</button>
        </div>
        ${hasChildren
          ? `<div class="topic-tree-children${collapsed ? " is-collapsed" : ""}">${children.map((child) => renderTopicNode(child, level + 1)).join("")}</div>`
          : ""}
      </div>
    `;
  }

  function renderOutlineTree() {
    const count = flattenNodes(state.nodes).length;
    els.outlineCount.textContent = `${count} 项`;

    if (state.loadingSection) {
      els.objectOutlineTree.innerHTML = "<div class=\"loading-state\">正在读取当前小节结构...</div>";
      return;
    }

    if (!state.selectedSectionId) {
      els.objectOutlineTree.innerHTML = "<div class=\"empty-state\">请选择或新建一个小节。</div>";
      return;
    }

    if (state.nodes.length === 0) {
      els.objectOutlineTree.innerHTML = "<div class=\"empty-state\">当前小节还没有内容块。</div>";
      return;
    }

    const rootCollapsed = state.collapsedBlockIds.has("section-root");
    els.objectOutlineTree.innerHTML = `
      <div class="outline-tree">
        <div class="outline-branch">
          <div class="outline-row" data-outline-root="section-root">
            <button class="outline-toggle" type="button" data-outline-toggle="section-root" aria-label="展开或收起">${rootCollapsed ? "›" : "⌄"}</button>
            <span class="outline-title" title="${escapeHtml(displaySectionName(state.sectionDetail))}">小节：${escapeHtml(displaySectionName(state.sectionDetail))}</span>
            <span class="outline-chip">${escapeHtml(textOf(field(state.sectionDetail, "状态"), "草稿"))}</span>
          </div>
          <div class="outline-children${rootCollapsed ? " is-collapsed" : ""}">
            ${state.nodes.map((node) => renderOutlineNode(node)).join("")}
          </div>
        </div>
      </div>
    `;
  }

  function renderOutlineNode(node) {
    const hasChildren = node.children.length > 0;
    const collapsed = state.collapsedBlockIds.has(node.id);
    const selected = node.id === state.selectedNodeId;
    return `
      <div class="outline-branch" data-outline-branch="${escapeHtml(node.id)}">
        <div class="outline-row${selected ? " is-selected" : ""}" data-outline-node="${escapeHtml(node.id)}">
          ${hasChildren
            ? `<button class="outline-toggle" type="button" data-outline-toggle="${escapeHtml(node.id)}" aria-label="展开或收起">${collapsed ? "›" : "⌄"}</button>`
            : "<span class=\"outline-toggle-placeholder\"></span>"}
          <span class="outline-title" title="${escapeHtml(node.displayName)}">${escapeHtml(node.displayName)}</span>
          <span class="outline-meta">
            <span class="outline-chip">${escapeHtml(node.role || node.type)}</span>
          </span>
        </div>
        ${hasChildren
          ? `<div class="outline-children${collapsed ? " is-collapsed" : ""}">${node.children.map(renderOutlineNode).join("")}</div>`
          : ""}
      </div>
    `;
  }

  function renderDocumentFlow() {
    if (state.loadingSection) {
      els.documentFlow.innerHTML = "<div class=\"loading-state\">正在加载小节文档流...</div>";
      return;
    }

    if (!state.selectedSectionId) {
      els.documentFlow.innerHTML = `
        <div class="empty-state">
          <strong>当前题库没有选中的小节。</strong>
          <p>可以从顶部选择真实小节，或点击“新建小节”创建一个空小节。</p>
        </div>
      `;
      return;
    }

    if (state.nodes.length === 0) {
      els.documentFlow.innerHTML = `
        <div class="document-flow">
          ${renderInsertHandle(rootInsertContext(null), "插入到小节开头")}
          <div class="empty-state">当前小节还没有内容块。使用插入条添加已有内容块，或新建卡片。</div>
          ${renderInsertHandle(rootInsertContext(null), "添加到小节末尾")}
        </div>
      `;
      return;
    }

    els.documentFlow.innerHTML = `
      <div class="document-flow">
        ${state.nodes.map((node) => `
          ${renderInsertHandle(rootInsertContext(node.id), "插入到此处")}
          ${renderContentBlockNode(node)}
        `).join("")}
        ${renderInsertHandle(rootInsertContext(null), "添加到小节末尾")}
      </div>
    `;
  }

  function renderContentBlockNode(node) {
    const hasChildren = node.children.length > 0;
    if (!hasChildren) {
      return renderContentNodeCard(node, false);
    }

    const collapsed = state.collapsedBlockIds.has(node.id);
    return `
      <section class="content-node-group" data-depth="${Number(node.depth)}" data-group-node="${escapeHtml(node.id)}">
        ${renderContentNodeCard(node, true)}
        <div class="content-node-children${collapsed ? " is-collapsed" : ""}">
          ${node.children.map((child) => `
            ${renderInsertHandle(childInsertContext(node, child.id), "添加子卡片")}
            ${renderContentBlockNode(child)}
          `).join("")}
          ${renderInsertHandle(childInsertContext(node, null), "添加子卡片")}
        </div>
      </section>
    `;
  }

  function renderContentNodeCard(node, hasChildren) {
    const selected = node.id === state.selectedNodeId;
    const collapsed = state.collapsedBlockIds.has(node.id);
    const role = node.role || node.type || "内容块";
    const structureLabel = node.structureType || (node.canHaveChildren ? "组合块" : "原子块");
    return `
      <article
        class="content-node${selected ? " is-selected" : ""}"
        id="content-${escapeAttr(node.id)}"
        draggable="true"
        data-node-id="${escapeAttr(node.id)}"
        data-parent-key="${escapeAttr(node.parentKey)}"
      >
        <div class="node-fold-cell">
          ${hasChildren
            ? `<button class="fold-button" type="button" data-fold="${escapeAttr(node.id)}" aria-expanded="${String(!collapsed)}">${collapsed ? "›" : "⌄"}</button>`
            : ""}
        </div>
        <div class="node-body" data-select-node="${escapeAttr(node.id)}" tabindex="0">
          <div class="node-header" data-node-title="${escapeAttr(node.id)}">
            <strong class="node-title" title="${escapeAttr(node.displayName)}">${escapeHtml(node.displayName)}</strong>
            <span class="node-badges">
              <span class="node-type node-type--${typeClass(role)}">${escapeHtml(role)}</span>
              <span class="node-structure node-structure--${structureClass(structureLabel)}">${escapeHtml(structureLabel)}</span>
            </span>
          </div>
          ${node.note ? `<div class="node-note">${escapeHtml(node.note)}</div>` : ""}
          <div class="node-preview">${renderNodePreview(node)}</div>
          <div class="node-quick-meta">
            ${node.difficulty ? `<span class="node-chip">难度 ${escapeHtml(node.difficulty)}</span>` : ""}
            ${node.usage ? `<span class="node-chip">用途 ${escapeHtml(node.usage)}</span>` : ""}
            ${node.questionType ? `<span class="node-chip">题型 ${escapeHtml(node.questionType)}</span>` : ""}
            ${node.currentVersionNo ? `<span class="node-chip">v${escapeHtml(node.currentVersionNo)}</span>` : ""}
          </div>
        </div>
        <div class="node-actions">
          <button class="node-action-button" type="button" data-action="word" data-node-id="${escapeAttr(node.id)}" title="Word 编辑">▣</button>
          ${node.canHaveChildren ? `<button class="node-action-button" type="button" data-action="add-child" data-node-id="${escapeAttr(node.id)}" title="添加子块">＋</button>` : ""}
          <button class="node-action-button" type="button" data-action="move-up" data-node-id="${escapeAttr(node.id)}" title="上移">⌃</button>
          <button class="node-action-button" type="button" data-action="move-down" data-node-id="${escapeAttr(node.id)}" title="下移">⌄</button>
          <button class="node-action-button" type="button" data-action="more" data-node-id="${escapeAttr(node.id)}" title="更多">⋯</button>
        </div>
      </article>
    `;
  }

  function renderInsertHandle(context, label) {
    return `
      <div
        class="insert-handle"
        data-insert-parent-kind="${escapeAttr(context.parentKind)}"
        data-insert-parent-node-id="${escapeAttr(context.parentNodeId || "")}"
        data-insert-parent-content-block-id="${escapeAttr(context.parentContentBlockId || "")}"
        data-insert-target-node-id="${escapeAttr(context.targetNodeId || "")}"
      >
        <span class="insert-hint">${escapeHtml(label)}</span>
        <span class="insert-actions">
          <button class="insert-action" type="button" data-insert-existing>插入卡片</button>
          <button class="insert-action" type="button" data-insert-new>新建卡片</button>
        </span>
      </div>
    `;
  }

  function renderNodePreview(node) {
    if (!node.currentVersionId) {
      return "<div class=\"node-preview-empty\">这个内容块还没有当前版本，暂无 docx 预览。</div>";
    }

    return `<iframe src="${escapeAttr(contentPreviewUrl(node.contentBlockId))}" title="${escapeAttr(node.displayName)} 预览"></iframe>`;
  }

  function renderInspector() {
    const node = selectedNode();
    els.inspectorKind.textContent = node ? (node.structureType || "内容块") : "未选择";

    if (state.loadingSection) {
      els.inspectorPanel.innerHTML = "<div class=\"loading-state\">正在读取详情...</div>";
      return;
    }

    if (!node) {
      els.inspectorPanel.innerHTML = "<div class=\"empty-state\">请选择中间文档流或左侧结构树中的内容块。</div>";
      return;
    }

    els.inspectorPanel.innerHTML = `
      <section class="inspector-card">
        <h3 class="inspector-title">${escapeHtml(node.displayName)}</h3>
        <p class="inspector-summary">${escapeHtml(node.summary || node.note || "暂无摘要或备注。")}</p>
        <div class="inspector-meta">
          <span><strong>内容块</strong>#${escapeHtml(node.contentBlockId)}</span>
          <span><strong>来源</strong>${escapeHtml(node.sourceKind === "section-item" ? "小节引用" : "组合块子项")}</span>
          <span><strong>类型</strong>${escapeHtml(node.type || "-")}</span>
          <span><strong>结构</strong>${escapeHtml(node.structureType || "-")}</span>
          <span><strong>角色</strong>${escapeHtml(node.role || "-")}</span>
          <span><strong>难度</strong>${escapeHtml(node.difficulty || "-")}</span>
          <span><strong>用途</strong>${escapeHtml(node.usage || "-")}</span>
          <span><strong>题型</strong>${escapeHtml(node.questionType || "-")}</span>
          <span><strong>默认选入</strong>${node.defaultIncluded ? "是" : "否"}</span>
          <span><strong>当前版本</strong>${node.currentVersionNo ? `v${escapeHtml(node.currentVersionNo)}` : "无"}</span>
        </div>
        <div class="inspector-ref">
          ${node.sourceKind === "section-item"
            ? "当前节点是 SectionItem 对 ContentBlock 的引用；移除只删除小节引用，不删除源内容块。"
            : `当前节点是组合块 #${escapeHtml(node.parentContentBlockId)} 的子块引用；排序和移除会修改该组合块结构。`}
        </div>
        <div class="inspector-actions">
          <button class="primary-button" type="button" data-inspector-action="word">Word 编辑</button>
          ${node.canHaveChildren ? "<button class=\"secondary-button\" type=\"button\" data-inspector-action=\"add-child-existing\">插入子块</button>" : ""}
          ${node.canHaveChildren ? "<button class=\"secondary-button\" type=\"button\" data-inspector-action=\"add-child-new\">新建子块</button>" : ""}
          <button class="ghost-button" type="button" data-inspector-action="open-library">打开内容库</button>
          <button class="ghost-button" type="button" data-inspector-action="remove">移除引用</button>
        </div>
        ${state.activeSession ? `<div class="session-message">${escapeHtml(sessionMessage())}</div>` : ""}
        <div class="inspector-preview">${renderInspectorPreview(node)}</div>
      </section>
    `;
  }

  function renderInspectorPreview(node) {
    if (!node.currentVersionId) {
      return "<div class=\"node-preview-empty\">没有当前版本，右侧暂无预览。</div>";
    }

    return `<iframe src="${escapeAttr(contentPreviewUrl(node.contentBlockId))}" title="${escapeAttr(node.displayName)} 详情预览"></iframe>`;
  }

  function renderStatus() {
    els.statusBar.textContent = state.statusMessage || "就绪";
  }

  async function loadSections(options = {}) {
    state.loadingSections = true;
    state.statusMessage = "正在读取真实小节列表...";
    render();

    try {
      const sections = await requestJson(sectionApiRoot());
      state.sections = Array.isArray(sections) ? sections : [];
      const preferredId = options.selectId || state.selectedSectionId;
      const exists = state.sections.some((section) => idOf(section) === preferredId);
      const nextId = exists ? preferredId : idOf(state.sections[0]);
      state.selectedSectionId = nextId;
      state.statusMessage = state.sections.length > 0
        ? `已读取 ${state.sections.length} 个真实小节。`
        : "当前题库没有小节，可以新建一个。";

      if (nextId) {
        await loadSection(nextId, options.preferredNodeId);
      } else {
        clearCurrentSection();
      }
    } catch (error) {
      state.sections = [];
      clearCurrentSection();
      state.statusMessage = errorMessage(error, "读取小节列表失败。");
    } finally {
      state.loadingSections = false;
      render();
    }
  }

  async function loadSection(sectionId, preferredNodeId = null) {
    if (!sectionId) {
      clearCurrentSection();
      render();
      return;
    }

    state.selectedSectionId = Number(sectionId);
    state.loadingSection = true;
    state.statusMessage = "正在读取小节详情和小节项...";
    render();

    try {
      const [detail, items] = await Promise.all([
        requestJson(`${sectionApiRoot()}/${encodeURIComponent(sectionId)}`),
        requestJson(`${sectionApiRoot()}/${encodeURIComponent(sectionId)}/项目`),
      ]);

      state.sectionDetail = detail;
      state.sectionItems = Array.isArray(items) ? items : [];
      const sortedItems = state.sectionItems
        .slice()
        .sort((left, right) => Number(field(left, "排序", "sort") || 0) - Number(field(right, "排序", "sort") || 0));

      const nodes = await Promise.all(sortedItems.map(loadSectionItemNode));
      state.nodes = nodes.filter(Boolean);
      rebuildNodeIndex();

      if (preferredNodeId && state.nodeMap.has(preferredNodeId)) {
        state.selectedNodeId = preferredNodeId;
      } else if (!state.selectedNodeId || !state.nodeMap.has(state.selectedNodeId)) {
        state.selectedNodeId = state.nodes[0]?.id || null;
      }

      state.statusMessage = `已加载小节：${displaySectionName(detail)}。`;
    } catch (error) {
      clearCurrentSection();
      state.selectedSectionId = Number(sectionId);
      state.statusMessage = errorMessage(error, "读取小节详情失败。");
    } finally {
      state.loadingSection = false;
      render();
    }
  }

  async function loadSectionItemNode(item) {
    const contentBlockId = Number(field(item, "内容块ID", "contentBlockId") || 0);
    if (!contentBlockId) {
      return null;
    }

    try {
      const tree = await requestJson(`${contentApiRoot()}/${encodeURIComponent(contentBlockId)}/结构树`);
      if (tree && field(tree, "内容块", "contentBlock")) {
        return mapStructureTree(tree, {
          sourceKind: "section-item",
          sectionItem: item,
          depth: 0,
          parentKey: "section-root",
        });
      }
    } catch (error) {
      state.statusMessage = errorMessage(error, `内容块 #${contentBlockId} 结构树读取失败，已降级显示小节项。`);
    }

    return nodeFromSectionItem(item);
  }

  function mapStructureTree(tree, context) {
    const block = normalizeBlock(field(tree, "内容块", "contentBlock"));
    const sourceChild = field(tree, "来源子项", "sourceChild");
    const isSectionRoot = context.sourceKind === "section-item";
    const sourceItem = isSectionRoot ? context.sectionItem : sourceChild;
    const nodeId = isSectionRoot
      ? `section-item-${idOf(context.sectionItem)}`
      : `content-child-${idOf(sourceChild) || `${context.parentContentBlockId}-${block.id}`}`;
    const depth = Number(context.depth || 0);
    const nextDepth = depth + 1;
    const childTrees = Array.isArray(field(tree, "子块列表", "children", "childBlocks"))
      ? field(tree, "子块列表", "children", "childBlocks")
      : [];
    const node = {
      id: nodeId,
      sourceKind: context.sourceKind,
      sectionItemId: isSectionRoot ? idOf(context.sectionItem) : null,
      childItemId: isSectionRoot ? null : idOf(sourceChild),
      parentContentBlockId: context.parentContentBlockId || null,
      parentKey: context.parentKey,
      depth,
      sort: Number(field(sourceItem, "排序", "sort") || 0),
      referenceMode: textOf(field(sourceItem, "引用版本模式", "referenceMode"), "跟随最新"),
      referenceVersionNo: field(sourceItem, "引用版本号", "referenceVersionNo"),
      ...block,
      children: [],
      maxDepthReached: Boolean(field(tree, "已达到最大深度", "maxDepthReached")),
    };

    if (nextDepth <= MAX_RENDER_DEPTH) {
      node.children = childTrees
        .map((childTree) => mapStructureTree(childTree, {
          sourceKind: "content-child",
          parentContentBlockId: node.contentBlockId,
          parentKey: node.id,
          depth: nextDepth,
        }))
        .filter(Boolean);
    }

    return node;
  }

  function nodeFromSectionItem(item) {
    const block = normalizeBlock({
      Id: field(item, "内容块ID", "contentBlockId"),
      标题: field(item, "内容块标题", "contentBlockTitle"),
      摘要: field(item, "内容块摘要", "contentBlockSummary"),
      类型: field(item, "内容块类型", "contentBlockType"),
      状态: field(item, "内容块状态", "contentBlockStatus"),
      结构类型: field(item, "内容块结构类型", "contentBlockStructureType"),
      RoleOptionName: field(item, "RoleOptionName", "roleOptionName"),
      DifficultyOptionName: field(item, "DifficultyOptionName", "difficultyOptionName"),
      UsageOptionName: field(item, "UsageOptionName", "usageOptionName"),
      QuestionTypeOptionName: field(item, "QuestionTypeOptionName", "questionTypeOptionName"),
      DefaultIncluded: field(item, "DefaultIncluded", "defaultIncluded"),
      Note: field(item, "Note", "note"),
      当前版本ID: field(item, "内容块当前版本ID", "contentBlockCurrentVersionId"),
      当前版本号: field(item, "引用版本号", "referenceVersionNo"),
    });

    return {
      id: `section-item-${idOf(item)}`,
      sourceKind: "section-item",
      sectionItemId: idOf(item),
      childItemId: null,
      parentContentBlockId: null,
      parentKey: "section-root",
      depth: 0,
      sort: Number(field(item, "排序", "sort") || 0),
      referenceMode: textOf(field(item, "引用版本模式", "referenceMode"), "跟随最新"),
      referenceVersionNo: field(item, "引用版本号", "referenceVersionNo"),
      ...block,
      children: [],
    };
  }

  function normalizeBlock(block) {
    const contentBlockId = Number(idOf(block) || field(block, "内容块ID", "contentBlockId", "子内容块ID", "childContentBlockId") || 0);
    const type = textOf(field(block, "类型", "type", "内容块类型", "contentBlockType", "子内容块类型", "childContentBlockType"), "普通说明");
    const structureType = textOf(field(block, "结构类型", "structureType", "内容块结构类型", "contentBlockStructureType", "子内容块结构类型", "childContentBlockStructureType"), "原子块");
    const role = textOf(field(block, "RoleOptionName", "roleOptionName", "角色", "role"), "");
    const difficulty = textOf(field(block, "DifficultyOptionName", "difficultyOptionName"), "");
    const usage = textOf(field(block, "UsageOptionName", "usageOptionName"), "");
    const questionType = textOf(field(block, "QuestionTypeOptionName", "questionTypeOptionName"), "");
    const note = textOf(field(block, "Note", "note"), "");
    const summary = textOf(field(block, "摘要", "summary", "内容块摘要", "contentBlockSummary"), "");
    const title = textOf(field(block, "标题", "title", "内容块标题", "contentBlockTitle", "子内容块标题", "childContentBlockTitle"), "");
    const canHaveChildren = Boolean(field(block, "是否允许子块", "allowChildren", "子内容块是否允许子块", "childContentBlockAllowChildren"))
      || structureType === "组合块";

    return {
      contentBlockId,
      title,
      displayName: displayBlockName({
        id: contentBlockId,
        title,
        summary,
        note,
        role,
        difficulty,
        questionType,
        type,
      }),
      summary,
      type,
      status: textOf(field(block, "状态", "status", "内容块状态", "contentBlockStatus", "子内容块状态", "childContentBlockStatus"), ""),
      structureType,
      canHaveChildren,
      role,
      difficulty,
      usage,
      questionType,
      defaultIncluded: field(block, "DefaultIncluded", "defaultIncluded") !== false,
      note,
      currentVersionId: Number(field(block, "当前版本ID", "currentVersionId", "内容块当前版本ID", "contentBlockCurrentVersionId", "子内容块当前版本ID", "childContentBlockCurrentVersionId") || 0),
      currentVersionNo: field(block, "当前版本号", "currentVersionNo", "引用版本号", "referenceVersionNo"),
    };
  }

  function rebuildNodeIndex() {
    state.nodeMap = new Map();
    flattenNodes(state.nodes).forEach((node) => state.nodeMap.set(node.id, node));
  }

  function flattenNodes(nodes) {
    const list = [];
    (nodes || []).forEach((node) => {
      list.push(node);
      list.push(...flattenNodes(node.children));
    });
    return list;
  }

  function selectedNode() {
    return state.selectedNodeId ? state.nodeMap.get(state.selectedNodeId) || null : null;
  }

  function selectNode(nodeId, options = {}) {
    if (!nodeId || !state.nodeMap.has(nodeId)) {
      return;
    }

    state.selectedNodeId = nodeId;
    render();
    if (options.scroll) {
      window.setTimeout(() => scrollToNode(nodeId), 0);
    }
  }

  function toggleCollapse(nodeId) {
    if (!nodeId) return;

    if (state.collapsedBlockIds.has(nodeId)) {
      state.collapsedBlockIds.delete(nodeId);
    } else {
      state.collapsedBlockIds.add(nodeId);
    }

    render();
  }

  function rootInsertContext(targetNodeId) {
    return {
      parentKind: "section",
      parentNodeId: "section-root",
      parentContentBlockId: "",
      targetNodeId,
    };
  }

  function childInsertContext(parentNode, targetNodeId) {
    return {
      parentKind: "content",
      parentNodeId: parentNode.id,
      parentContentBlockId: parentNode.contentBlockId,
      targetNodeId,
    };
  }

  function readInsertContext(element) {
    const host = element.closest(".insert-handle");
    return {
      parentKind: host?.dataset.insertParentKind || "section",
      parentNodeId: host?.dataset.insertParentNodeId || "section-root",
      parentContentBlockId: Number(host?.dataset.insertParentContentBlockId || 0),
      targetNodeId: host?.dataset.insertTargetNodeId || null,
    };
  }

  function openResourceModal(context) {
    if (!state.selectedSectionId) {
      setStatus("请先选择一个小节。");
      return;
    }

    state.insertContext = context || rootInsertContext(null);
    state.resourceMode = "insert-existing";
    els.resourceModal.classList.remove("is-hidden");
    els.resourceSearchInput.value = "";
    state.contentCandidates = [];
    renderResourceResults();
    loadContentCandidates();
    window.setTimeout(() => els.resourceSearchInput.focus(), 0);
  }

  function closeResourceModal() {
    els.resourceModal.classList.add("is-hidden");
  }

  async function loadContentCandidates() {
    state.loadingCandidates = true;
    renderResourceResults();
    const keyword = els.resourceSearchInput.value.trim();
    const params = new URLSearchParams();
    if (keyword) {
      params.set("关键词", keyword);
    }

    try {
      const list = await requestJson(`${contentApiRoot()}${params.toString() ? `?${params}` : ""}`);
      state.contentCandidates = Array.isArray(list) ? list : [];
    } catch (error) {
      state.contentCandidates = [];
      setStatus(errorMessage(error, "读取内容块候选失败。"));
    } finally {
      state.loadingCandidates = false;
      renderResourceResults();
    }
  }

  function renderResourceResults() {
    if (state.loadingCandidates) {
      els.resourceResults.innerHTML = "<div class=\"loading-state\">正在读取内容块...</div>";
      return;
    }

    if (state.contentCandidates.length === 0) {
      els.resourceResults.innerHTML = "<div class=\"empty-state\">暂无候选内容块。</div>";
      return;
    }

    els.resourceResults.innerHTML = state.contentCandidates.map((candidate) => {
      const block = normalizeBlock(candidate);
      const disabled = block.status === "已废弃" || (els.referenceModeSelect.value === "锁定版本" && !block.currentVersionId);
      const reason = block.status === "已废弃"
        ? "已废弃"
        : !block.currentVersionId && els.referenceModeSelect.value === "锁定版本"
          ? "无当前版本"
          : "";
      return renderContentBlockCard(block, { disabled, reason });
    }).join("");
  }

  function renderContentBlockCard(block, options = {}) {
    const type = block.role || block.type || "内容块";
    const disabledClass = options.disabled ? " is-disabled" : "";
    const action = options.disabled ? (options.reason || "不可选") : "选择";
    return `
      <button class="content-block-card${disabledClass}" type="button" data-candidate-id="${escapeAttr(block.contentBlockId)}" ${options.disabled ? "disabled" : ""}>
        <div class="content-block-card__top">
          <strong class="content-block-card__title">${escapeHtml(block.displayName)}</strong>
          <span class="content-block-card__right">
            <span class="content-block-card__type content-block-card__type--${typeClass(type)}">${escapeHtml(type)}</span>
            <span class="content-block-card__structure content-block-card__structure--${structureClass(block.structureType)}">${escapeHtml(block.structureType)}</span>
            <span class="content-block-card__action">${escapeHtml(action)}</span>
          </span>
        </div>
        ${block.note || block.summary ? `<div class="content-block-card__remark">${escapeHtml(block.note || block.summary)}</div>` : ""}
        <div class="content-block-card__meta">
          <span class="content-block-card__property"><span class="content-block-card__property-label">状态</span>${escapeHtml(block.status || "-")}</span>
          <span class="content-block-card__property"><span class="content-block-card__property-label">版本</span>${block.currentVersionNo ? `v${escapeHtml(block.currentVersionNo)}` : "无"}</span>
        </div>
      </button>
    `;
  }

  function openCreateModal(context) {
    if (!state.selectedSectionId) {
      setStatus("请先选择一个小节。");
      return;
    }

    state.insertContext = context || rootInsertContext(null);
    els.createModal.classList.remove("is-hidden");
    els.createBlockForm.reset();
    window.setTimeout(() => els.createBlockTitleInput.focus(), 0);
  }

  function closeCreateModal() {
    els.createModal.classList.add("is-hidden");
  }

  async function createAndInsertBlock(event) {
    event.preventDefault();
    const title = els.createBlockTitleInput.value.trim();
    if (!title) {
      setStatus("新建内容块需要标题。");
      return;
    }

    const structureType = els.createBlockStructureSelect.value;
    const note = els.createBlockNoteInput.value.trim();
    try {
      const created = await requestJson(contentApiRoot(), {
        method: "POST",
        body: JSON.stringify({
          标题: title,
          摘要: note || null,
          内容块类型: els.createBlockTypeSelect.value,
          内容块状态: "草稿",
          内容块结构类型: structureType,
          是否允许子块: structureType === "组合块",
          Note: note || null,
        }),
      });
      closeCreateModal();
      await insertContentBlock(created, state.insertContext, { created: true });
    } catch (error) {
      setStatus(errorMessage(error, "新建内容块失败。"));
    }
  }

  async function insertSelectedCandidate(candidateId) {
    const candidate = state.contentCandidates.find((item) => Number(idOf(item)) === Number(candidateId));
    if (!candidate) {
      return;
    }

    await insertContentBlock(candidate, state.insertContext);
  }

  async function insertContentBlock(blockLike, context, options = {}) {
    const block = normalizeBlock(blockLike);
    if (!block.contentBlockId) {
      setStatus("候选内容块缺少 ID，无法插入。");
      return;
    }

    const insertContext = context || rootInsertContext(null);
    const referenceMode = options.created ? "跟随最新" : (els.referenceModeSelect?.value || "跟随最新");
    if (referenceMode === "锁定版本" && !block.currentVersionId) {
      setStatus("这个内容块没有当前版本，不能锁定当前版本。");
      return;
    }

    try {
      if (insertContext.parentKind === "content") {
        await insertBlockIntoComposite(block, insertContext, referenceMode);
      } else {
        await insertBlockIntoSection(block, insertContext, referenceMode);
      }

      closeResourceModal();
      const messagePrefix = options.created ? "已新建并插入" : "已插入";
      setStatus(`${messagePrefix}：${block.displayName}`);
    } catch (error) {
      setStatus(errorMessage(error, "插入内容块失败。"));
    }
  }

  async function insertBlockIntoSection(block, context, referenceMode) {
    const body = {
      内容块ID: block.contentBlockId,
      引用版本模式: referenceMode,
    };
    if (referenceMode === "锁定版本") {
      body.内容块版本ID = block.currentVersionId;
    }

    const added = await requestJson(`${sectionApiRoot()}/${encodeURIComponent(state.selectedSectionId)}/项目`, {
      method: "POST",
      body: JSON.stringify(body),
    });
    const addedId = idOf(added);
    const preferredNodeId = `section-item-${addedId}`;
    const order = computeOrderAfterInsertedId(state.nodes, addedId, context.targetNodeId, "sectionItemId");
    if (order) {
      await saveSectionOrder(order);
    }

    await reloadCurrentSection(preferredNodeId);
    window.setTimeout(() => scrollToNode(preferredNodeId), 0);
  }

  async function insertBlockIntoComposite(block, context, referenceMode) {
    const parentNode = state.nodeMap.get(context.parentNodeId);
    if (!parentNode || !parentNode.canHaveChildren) {
      throw new Error("当前父级不是组合块，不能添加子卡片。");
    }

    if (wouldCreateCycle(parentNode, block.contentBlockId)) {
      throw new Error("不能把当前组合块自身或其祖先加入为子块。");
    }

    const body = {
      子内容块ID: block.contentBlockId,
      引用版本模式: referenceMode,
    };
    if (referenceMode === "锁定版本") {
      body.子内容块版本ID = block.currentVersionId;
    }

    const added = await requestJson(`${contentApiRoot()}/${encodeURIComponent(parentNode.contentBlockId)}/子块`, {
      method: "POST",
      body: JSON.stringify(body),
    });
    const addedId = idOf(added);
    const preferredNodeId = `content-child-${addedId}`;
    const order = computeOrderAfterInsertedId(parentNode.children, addedId, context.targetNodeId, "childItemId");
    if (order) {
      await saveContentChildOrder(parentNode.contentBlockId, order);
    }

    state.collapsedBlockIds.delete(parentNode.id);
    await reloadCurrentSection(preferredNodeId);
    window.setTimeout(() => scrollToNode(preferredNodeId), 0);
  }

  function computeOrderAfterInsertedId(siblings, addedId, targetNodeId, idProperty) {
    if (!targetNodeId) {
      return null;
    }

    const targetIndex = siblings.findIndex((node) => node.id === targetNodeId);
    if (targetIndex < 0) {
      return null;
    }

    const ordered = siblings.map((node) => Number(node[idProperty])).filter(Boolean);
    ordered.push(Number(addedId));
    const withoutNew = ordered.filter((id) => id !== Number(addedId));
    withoutNew.splice(targetIndex, 0, Number(addedId));
    return withoutNew;
  }

  function wouldCreateCycle(parentNode, childContentBlockId) {
    if (Number(parentNode.contentBlockId) === Number(childContentBlockId)) {
      return true;
    }

    const ancestors = getAncestorNodes(parentNode);
    if (ancestors.some((node) => Number(node.contentBlockId) === Number(childContentBlockId))) {
      return true;
    }

    const existingChildNode = flattenNodes(state.nodes)
      .find((node) => Number(node.contentBlockId) === Number(childContentBlockId));
    return existingChildNode
      ? hasDescendantContentBlock(existingChildNode, parentNode.contentBlockId)
      : false;
  }

  function hasDescendantContentBlock(node, contentBlockId) {
    return (node.children || []).some((child) => (
      Number(child.contentBlockId) === Number(contentBlockId)
      || hasDescendantContentBlock(child, contentBlockId)
    ));
  }

  function getAncestorNodes(node) {
    const ancestors = [];
    let current = node;
    while (current && current.parentKey && current.parentKey !== "section-root") {
      const parent = state.nodeMap.get(current.parentKey);
      if (!parent) break;
      ancestors.push(parent);
      current = parent;
    }
    return ancestors;
  }

  async function saveSectionOrder(sectionItemIds) {
    await requestJson(`${sectionApiRoot()}/${encodeURIComponent(state.selectedSectionId)}/项目排序`, {
      method: "PUT",
      body: JSON.stringify({
        项目排序列表: sectionItemIds.map((itemId, index) => ({
          小节项ID: Number(itemId),
          排序: index,
        })),
      }),
    });
  }

  async function saveContentChildOrder(parentContentBlockId, childItemIds) {
    await requestJson(`${contentApiRoot()}/${encodeURIComponent(parentContentBlockId)}/子块排序`, {
      method: "PUT",
      body: JSON.stringify({
        子项排序列表: childItemIds.map((itemId, index) => ({
          子项ID: Number(itemId),
          排序: index,
        })),
      }),
    });
  }

  async function moveNode(nodeId, direction) {
    const node = state.nodeMap.get(nodeId);
    if (!node) return;
    const siblings = getSiblings(node.parentKey);
    const index = siblings.findIndex((item) => item.id === node.id);
    const target = index + direction;
    if (index < 0 || target < 0 || target >= siblings.length) {
      return;
    }

    const reordered = siblings.slice();
    const [moved] = reordered.splice(index, 1);
    reordered.splice(target, 0, moved);
    await persistSiblingOrder(node, reordered);
    await reloadCurrentSection(node.id);
    window.setTimeout(() => scrollToNode(node.id), 0);
  }

  async function dropNodeBefore(draggedId, targetId) {
    const dragged = state.nodeMap.get(draggedId);
    const target = state.nodeMap.get(targetId);
    if (!dragged || !target || dragged.id === target.id) {
      return;
    }

    if (dragged.parentKey !== target.parentKey || dragged.sourceKind !== target.sourceKind) {
      setStatus("第一版只支持同级拖拽排序，不能跨父级移动。");
      return;
    }

    const siblings = getSiblings(dragged.parentKey);
    const withoutDragged = siblings.filter((node) => node.id !== dragged.id);
    const targetIndex = withoutDragged.findIndex((node) => node.id === target.id);
    if (targetIndex < 0) return;
    withoutDragged.splice(targetIndex, 0, dragged);
    await persistSiblingOrder(dragged, withoutDragged);
    await reloadCurrentSection(dragged.id);
    window.setTimeout(() => scrollToNode(dragged.id), 0);
  }

  async function persistSiblingOrder(node, orderedNodes) {
    if (node.parentKey === "section-root") {
      await saveSectionOrder(orderedNodes.map((item) => item.sectionItemId));
      setStatus("已保存小节项同级排序。");
      return;
    }

    const parentNode = state.nodeMap.get(node.parentKey);
    if (!parentNode) {
      throw new Error("找不到父级组合块。");
    }

    await saveContentChildOrder(parentNode.contentBlockId, orderedNodes.map((item) => item.childItemId));
    setStatus("已保存组合块子项同级排序。");
  }

  function getSiblings(parentKey) {
    if (parentKey === "section-root") {
      return state.nodes;
    }

    const parentNode = state.nodeMap.get(parentKey);
    return parentNode?.children || [];
  }

  async function removeNode(nodeId) {
    const node = state.nodeMap.get(nodeId);
    if (!node) return;

    const prompt = node.sourceKind === "section-item"
      ? `只移除当前小节中的引用，不删除源内容块：${node.displayName}。确认继续？`
      : `只从组合块中移除子块引用，不删除源内容块：${node.displayName}。确认继续？`;
    if (!window.confirm(prompt)) {
      return;
    }

    try {
      if (node.sourceKind === "section-item") {
        await requestJson(`${sectionApiRoot()}/${encodeURIComponent(state.selectedSectionId)}/项目/${encodeURIComponent(node.sectionItemId)}`, {
          method: "DELETE",
        });
      } else {
        await requestJson(`${contentApiRoot()}/${encodeURIComponent(node.parentContentBlockId)}/子块/${encodeURIComponent(node.childItemId)}`, {
          method: "DELETE",
        });
      }

      const nextNodeId = pickNeighborNodeId(node);
      await reloadCurrentSection(nextNodeId);
      setStatus(`已移除引用：${node.displayName}`);
    } catch (error) {
      setStatus(errorMessage(error, "移除引用失败。"));
    }
  }

  function pickNeighborNodeId(node) {
    const siblings = getSiblings(node.parentKey);
    const index = siblings.findIndex((item) => item.id === node.id);
    return siblings[index + 1]?.id || siblings[index - 1]?.id || null;
  }

  async function editContentInWord(nodeId) {
    const node = state.nodeMap.get(nodeId);
    if (!node) return;

    try {
      const session = await requestJson(`${contentApiRoot()}/${encodeURIComponent(node.contentBlockId)}/编辑会话`, {
        method: "POST",
        body: JSON.stringify({ 是否打开Word: true }),
      });
      setSession(session);
      startPoll(field(session, "会话ID", "sessionId"));
      setStatus(field(session, "消息", "message") || "已创建 Word 编辑会话。");
      renderInspector();
    } catch (error) {
      setStatus(errorMessage(error, "创建 Word 编辑会话失败。"));
    }
  }

  function setSession(session) {
    state.activeSession = session;
    if (session && finalSessionStates.has(textOf(field(session, "状态", "status"), ""))) {
      clearPoll();
    }
  }

  function startPoll(sessionId) {
    if (!sessionId) return;
    clearPoll();
    state.pollTimer = window.setInterval(() => pollSession(sessionId), 2000);
  }

  async function pollSession(sessionId) {
    try {
      const session = await requestJson(`${contentApiRoot()}/编辑会话/${encodeURIComponent(sessionId)}`);
      setSession(session);
      setStatus(sessionMessage());
      const status = textOf(field(session, "状态", "status"), "");
      if ((status === "已同步" || status === "无变化") && state.selectedSectionId) {
        await reloadCurrentSection(state.selectedNodeId);
      }
    } catch (error) {
      clearPoll();
      setStatus(errorMessage(error, "读取编辑会话失败。"));
    }
  }

  function clearPoll() {
    if (state.pollTimer) {
      window.clearInterval(state.pollTimer);
      state.pollTimer = null;
    }
  }

  function sessionMessage() {
    if (!state.activeSession) {
      return "";
    }

    return textOf(field(state.activeSession, "消息", "message"), "")
      || textOf(field(state.activeSession, "错误信息", "errorMessage"), "")
      || `编辑会话状态：${textOf(field(state.activeSession, "状态", "status"), "-")}`;
  }

  async function exportCurrentSectionWord() {
    if (!state.selectedSectionId || state.exportingSection) {
      return;
    }

    state.exportingSection = true;
    setStatus("正在导出当前小节整体 Word...");
    renderTopbar();

    try {
      const result = await requestBlob(`${sectionApiRoot()}/${encodeURIComponent(state.selectedSectionId)}/导出Word`, {
        method: "POST",
      });
      const fileName = result.fileName || `${displaySectionName(state.sectionDetail)}-${formatTimestamp(new Date())}.docx`;
      downloadBlob(result.blob, fileName);
      setStatus(`已导出当前小节 Word：${fileName}`);
    } catch (error) {
      setStatus(errorMessage(error, "导出当前小节 Word 失败。"));
    } finally {
      state.exportingSection = false;
      renderTopbar();
    }
  }

  async function createSection() {
    const title = window.prompt("请输入小节标题");
    if (!title || !title.trim()) {
      return;
    }

    try {
      const created = await requestJson(sectionApiRoot(), {
        method: "POST",
        body: JSON.stringify({
          标题: title.trim(),
          摘要: null,
          状态: "草稿",
        }),
      });
      await loadSections({ selectId: idOf(created) });
      setStatus(`已新建小节：${title.trim()}`);
    } catch (error) {
      setStatus(errorMessage(error, "新建小节失败。"));
    }
  }

  async function reloadCurrentSection(preferredNodeId = null) {
    if (!state.selectedSectionId) return;
    await loadSection(state.selectedSectionId, preferredNodeId);
  }

  function clearCurrentSection() {
    state.sectionDetail = null;
    state.sectionItems = [];
    state.nodes = [];
    state.nodeMap = new Map();
    state.selectedNodeId = null;
    state.collapsedBlockIds.clear();
  }

  function openTopicDrawer() {
    state.topicDrawerOpen = true;
    renderTopicDrawer();
  }

  function closeTopicDrawer() {
    state.topicDrawerOpen = false;
    renderTopicDrawer();
  }

  function selectTopic(topicId) {
    state.selectedTopicId = topicId;
    closeTopicDrawer();
    setStatus("已选择教学主题。当前阶段只展开导航 UI，不用教学主题替代真实小节数据。");
  }

  function toggleTopic(topicId) {
    if (state.topicCollapsedIds.has(topicId)) {
      state.topicCollapsedIds.delete(topicId);
    } else {
      state.topicCollapsedIds.add(topicId);
    }
    renderTopicDrawer();
  }

  function openMoreMenu(nodeId, anchor) {
    const node = state.nodeMap.get(nodeId);
    if (!node) return;
    const rect = anchor.getBoundingClientRect();
    els.blockMoreMenu.style.left = `${Math.min(rect.left, window.innerWidth - 240)}px`;
    els.blockMoreMenu.style.top = `${Math.min(rect.bottom + 6, window.innerHeight - 260)}px`;
    els.blockMoreMenu.innerHTML = `
      <button class="menu-item" type="button" data-menu-action="word" data-node-id="${escapeAttr(node.id)}">Word 编辑</button>
      <button class="menu-item" type="button" data-menu-action="detail" data-node-id="${escapeAttr(node.id)}">查看详情</button>
      <button class="menu-item" type="button" data-menu-action="copy" data-node-id="${escapeAttr(node.id)}">复制引用</button>
      <button class="menu-item" type="button" data-menu-action="library" data-node-id="${escapeAttr(node.id)}">打开内容库</button>
      ${node.canHaveChildren ? `<button class="menu-item" type="button" data-menu-action="add-existing" data-node-id="${escapeAttr(node.id)}">插入子块</button>` : ""}
      ${node.canHaveChildren ? `<button class="menu-item" type="button" data-menu-action="add-new" data-node-id="${escapeAttr(node.id)}">新建子块</button>` : ""}
      <button class="menu-item is-danger" type="button" data-menu-action="remove" data-node-id="${escapeAttr(node.id)}">移除引用</button>
    `;
    els.blockMoreMenu.classList.remove("is-hidden");
  }

  function closeMoreMenu() {
    els.blockMoreMenu.classList.add("is-hidden");
  }

  async function handleMoreMenuAction(action, nodeId) {
    closeMoreMenu();
    const node = state.nodeMap.get(nodeId);
    if (!node) return;

    if (action === "word") {
      await editContentInWord(nodeId);
    } else if (action === "detail") {
      selectNode(nodeId, { scroll: true });
    } else if (action === "copy") {
      await copyReference(node);
    } else if (action === "library") {
      window.location.href = `./cms.html?contentBlockId=${encodeURIComponent(node.contentBlockId)}`;
    } else if (action === "add-existing") {
      openResourceModal(childInsertContext(node, null));
    } else if (action === "add-new") {
      openCreateModal(childInsertContext(node, null));
    } else if (action === "remove") {
      await removeNode(nodeId);
    }
  }

  async function copyReference(node) {
    const text = `ContentBlock#${node.contentBlockId} ${node.displayName}`;
    try {
      await navigator.clipboard.writeText(text);
      setStatus(`已复制引用：${text}`);
    } catch {
      setStatus(`引用：${text}`);
    }
  }

  function bindEvents() {
    els.sectionSelect.addEventListener("change", async () => {
      const id = Number(els.sectionSelect.value || 0);
      state.selectedNodeId = null;
      if (id) {
        await loadSection(id);
      } else {
        clearCurrentSection();
        render();
      }
    });

    els.refreshButton.addEventListener("click", () => loadSections({ selectId: state.selectedSectionId, preferredNodeId: state.selectedNodeId }));
    els.newSectionButton.addEventListener("click", createSection);
    els.exportSectionButton.addEventListener("click", exportCurrentSectionWord);
    els.insertAtEndButton.addEventListener("click", () => openResourceModal(rootInsertContext(null)));
    els.newAtEndButton.addEventListener("click", () => openCreateModal(rootInsertContext(null)));

    els.topicDrawerTab.addEventListener("click", openTopicDrawer);
    els.closeTopicDrawerButton.addEventListener("click", closeTopicDrawer);
    els.topicDrawerBackdrop.addEventListener("click", closeTopicDrawer);

    els.topicTree.addEventListener("click", (event) => {
      const toggle = event.target.closest("[data-topic-toggle]");
      if (toggle) {
        toggleTopic(toggle.dataset.topicToggle);
        return;
      }

      const node = event.target.closest("[data-topic-id]");
      if (node) {
        selectTopic(node.dataset.topicId);
      }
    });

    els.objectOutlineTree.addEventListener("click", (event) => {
      const toggle = event.target.closest("[data-outline-toggle]");
      if (toggle) {
        toggleCollapse(toggle.dataset.outlineToggle);
        return;
      }

      const row = event.target.closest("[data-outline-node]");
      if (row) {
        selectNode(row.dataset.outlineNode, { scroll: true });
      }
    });

    els.objectOutlineTree.addEventListener("dblclick", (event) => {
      const row = event.target.closest("[data-outline-node]");
      if (row && state.nodeMap.get(row.dataset.outlineNode)?.children.length) {
        toggleCollapse(row.dataset.outlineNode);
      }
    });

    els.documentFlow.addEventListener("click", async (event) => {
      const insertExisting = event.target.closest("[data-insert-existing]");
      if (insertExisting) {
        openResourceModal(readInsertContext(insertExisting));
        return;
      }

      const insertNew = event.target.closest("[data-insert-new]");
      if (insertNew) {
        openCreateModal(readInsertContext(insertNew));
        return;
      }

      const fold = event.target.closest("[data-fold]");
      if (fold) {
        toggleCollapse(fold.dataset.fold);
        return;
      }

      const action = event.target.closest("[data-action]");
      if (action) {
        const nodeId = action.dataset.nodeId;
        const actionName = action.dataset.action;
        if (actionName === "word") {
          await editContentInWord(nodeId);
        } else if (actionName === "add-child") {
          const node = state.nodeMap.get(nodeId);
          if (node) openResourceModal(childInsertContext(node, null));
        } else if (actionName === "move-up") {
          await moveNode(nodeId, -1);
        } else if (actionName === "move-down") {
          await moveNode(nodeId, 1);
        } else if (actionName === "more") {
          openMoreMenu(nodeId, action);
        }
        return;
      }

      const body = event.target.closest("[data-select-node]");
      if (body) {
        selectNode(body.dataset.selectNode);
      }
    });

    els.documentFlow.addEventListener("dblclick", (event) => {
      const title = event.target.closest("[data-node-title]");
      if (title) {
        const nodeId = title.dataset.nodeTitle;
        if (state.nodeMap.get(nodeId)?.children.length) {
          toggleCollapse(nodeId);
        }
      }
    });

    els.documentFlow.addEventListener("dragstart", (event) => {
      const node = event.target.closest("[data-node-id]");
      if (!node) return;
      state.draggingNodeId = node.dataset.nodeId;
      node.classList.add("is-dragging");
      event.dataTransfer.effectAllowed = "move";
      event.dataTransfer.setData("text/plain", state.draggingNodeId);
    });

    els.documentFlow.addEventListener("dragend", () => {
      state.draggingNodeId = null;
      els.documentFlow.querySelectorAll(".is-dragging,.is-drop-target").forEach((item) => {
        item.classList.remove("is-dragging", "is-drop-target");
      });
    });

    els.documentFlow.addEventListener("dragover", (event) => {
      const target = event.target.closest("[data-node-id]");
      if (!target || !state.draggingNodeId || target.dataset.nodeId === state.draggingNodeId) {
        return;
      }

      const dragged = state.nodeMap.get(state.draggingNodeId);
      const targetNode = state.nodeMap.get(target.dataset.nodeId);
      if (dragged && targetNode && dragged.parentKey === targetNode.parentKey && dragged.sourceKind === targetNode.sourceKind) {
        event.preventDefault();
        target.classList.add("is-drop-target");
      }
    });

    els.documentFlow.addEventListener("dragleave", (event) => {
      const target = event.target.closest("[data-node-id]");
      if (target) {
        target.classList.remove("is-drop-target");
      }
    });

    els.documentFlow.addEventListener("drop", async (event) => {
      const target = event.target.closest("[data-node-id]");
      if (!target || !state.draggingNodeId) return;
      event.preventDefault();
      await dropNodeBefore(state.draggingNodeId, target.dataset.nodeId);
    });

    els.inspectorPanel.addEventListener("click", async (event) => {
      const button = event.target.closest("[data-inspector-action]");
      if (!button) return;
      const node = selectedNode();
      if (!node) return;
      const action = button.dataset.inspectorAction;
      if (action === "word") {
        await editContentInWord(node.id);
      } else if (action === "add-child-existing") {
        openResourceModal(childInsertContext(node, null));
      } else if (action === "add-child-new") {
        openCreateModal(childInsertContext(node, null));
      } else if (action === "open-library") {
        window.location.href = `./cms.html?contentBlockId=${encodeURIComponent(node.contentBlockId)}`;
      } else if (action === "remove") {
        await removeNode(node.id);
      }
    });

    els.closeResourceModalButton.addEventListener("click", closeResourceModal);
    els.resourceModal.addEventListener("click", (event) => {
      if (event.target === els.resourceModal) closeResourceModal();
    });
    els.resourceSearchInput.addEventListener("input", debounce(loadContentCandidates, 240));
    els.referenceModeSelect.addEventListener("change", renderResourceResults);
    els.resourceResults.addEventListener("click", async (event) => {
      const card = event.target.closest("[data-candidate-id]");
      if (card && !card.disabled) {
        await insertSelectedCandidate(card.dataset.candidateId);
      }
    });

    els.createBlockForm.addEventListener("submit", createAndInsertBlock);
    els.closeCreateModalButton.addEventListener("click", closeCreateModal);
    els.cancelCreateBlockButton.addEventListener("click", closeCreateModal);
    els.createModal.addEventListener("click", (event) => {
      if (event.target === els.createModal) closeCreateModal();
    });

    els.blockMoreMenu.addEventListener("click", async (event) => {
      const button = event.target.closest("[data-menu-action]");
      if (button) {
        await handleMoreMenuAction(button.dataset.menuAction, button.dataset.nodeId);
      }
    });

    document.addEventListener("click", (event) => {
      if (!event.target.closest("#blockMoreMenu,[data-action='more']")) {
        closeMoreMenu();
      }
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        closeTopicDrawer();
        closeResourceModal();
        closeCreateModal();
        closeMoreMenu();
      }
    });
  }

  async function handleQuestionBankChanged() {
    clearPoll();
    state.selectedSectionId = null;
    clearCurrentSection();
    state.sections = [];
    await loadSections();
  }

  async function init() {
    bindEvents();
    render();
    await window.QuestionBankContext.initSwitcher({ onChange: handleQuestionBankChanged });
    await loadSections();
  }

  function contentPreviewUrl(contentBlockId) {
    return `${contentApiRoot()}/${encodeURIComponent(contentBlockId)}/预览html?t=${Date.now()}`;
  }

  function scrollToNode(nodeId) {
    const element = document.getElementById(`content-${nodeId}`);
    if (element) {
      element.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  }

  function displaySectionName(section) {
    return textOf(field(section, "标题", "title"), "未命名小节");
  }

  function displayBlockName(block) {
    const title = textOf(block.title, "");
    if (title) return title;
    if (block.note) return block.note;
    if (block.summary) return block.summary;
    const parts = [block.role, block.difficulty, block.questionType].filter(Boolean);
    if (parts.length) return parts.join(" · ");
    return `${block.type || "内容块"} #${block.id || "-"}`;
  }

  function typeClass(type) {
    return {
      知识点: "knowledge",
      例题: "example",
      练习: "exercise",
      练习组: "group",
      题组: "group",
      知识点组: "group",
      例题组: "group",
      方法总结: "method",
      易错点: "note",
      普通说明: "note",
      题目: "example",
      小节: "group",
      专题片段: "group",
    }[type] || "default";
  }

  function structureClass(structureType) {
    return structureType === "组合块" ? "composite" : structureType === "原子块" ? "atomic" : "default";
  }

  function setStatus(message) {
    state.statusMessage = message;
    renderStatus();
  }

  function field(obj, ...names) {
    if (!obj) return undefined;
    for (const name of names) {
      if (Object.prototype.hasOwnProperty.call(obj, name)) {
        return obj[name];
      }
    }
    return undefined;
  }

  function idOf(value) {
    const raw = field(value, "Id", "id", "ID");
    const number = Number(raw);
    return Number.isInteger(number) && number > 0 ? number : null;
  }

  function textOf(value, fallback = "-") {
    return value === null || value === undefined || value === "" ? fallback : String(value);
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll("\"", "&quot;")
      .replaceAll("'", "&#039;");
  }

  function escapeAttr(value) {
    return escapeHtml(value);
  }

  function errorMessage(error, fallback) {
    return error instanceof Error && error.message ? error.message : fallback;
  }

  function debounce(fn, delay) {
    let timer = null;
    return (...args) => {
      window.clearTimeout(timer);
      timer = window.setTimeout(() => fn(...args), delay);
    };
  }

  function downloadBlob(blob, fileName) {
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.setTimeout(() => URL.revokeObjectURL(url), 0);
  }

  function getDownloadFileName(contentDisposition) {
    if (!contentDisposition) return "";
    const utf8Match = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i);
    if (utf8Match) {
      return decodeURIComponent(utf8Match[1].replaceAll("\"", ""));
    }

    const fallbackMatch = contentDisposition.match(/filename=\"?([^\";]+)\"?/i);
    return fallbackMatch ? fallbackMatch[1] : "";
  }

  function formatTimestamp(date) {
    const pad = (value) => String(value).padStart(2, "0");
    return `${date.getFullYear()}${pad(date.getMonth() + 1)}${pad(date.getDate())}${pad(date.getHours())}${pad(date.getMinutes())}${pad(date.getSeconds())}`;
  }

  init();
})();
