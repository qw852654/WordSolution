(function () {
  const topicTree = {
    id: "topic-function-relation",
    title: "功能关系",
    summary: "高中物理功能关系主线。",
    children: [
      {
        id: "topic-mechanical-energy",
        title: "机械能守恒",
        summary: "围绕守恒条件、典型模型和输出讲义组织内容。",
        children: [
          { id: "topic-vertical-circle", title: "竖直圆轨道", summary: "圆周运动中的能量守恒与临界条件。" },
          { id: "topic-rod-model", title: "杆模型", summary: "轻杆连接体中的约束、速度关系与能量表达。" },
          { id: "topic-ball-model", title: "球模型", summary: "小球、轨道、弹簧等模型中的能量转化。" },
        ],
      },
    ],
  };

  const topicWorkspace = {
    topicId: "topic-mechanical-energy",
    secondary: {
      references: [
        "教学主题树当前仍是独立的 UI 原型导航数据。",
        "小节卡片和讲义卡片已经切到真实 API。",
        "当前阶段尚未接入真实教学主题模型。"
      ],
      recent: [
        "点击主题进入主题工作台。",
        "点击真实小节卡片进入小节编辑器。",
        "展开教学主题树不会重置当前编辑器。"
      ],
    },
  };

  const els = {
    iaLayout: document.getElementById("iaLayout"),
    pageEyebrow: document.getElementById("pageEyebrow"),
    pageTitle: document.getElementById("pageTitle"),
    breadcrumb: document.getElementById("breadcrumb"),
    modeStatus: document.getElementById("modeStatus"),
    backToTopicButton: document.getElementById("backToTopicButton"),
    topicNavPanel: document.getElementById("topicNavPanel"),
    topicNavToggleButton: document.getElementById("topicNavToggleButton"),
    topicTree: document.getElementById("topicTree"),
    objectOutlinePanel: document.getElementById("objectOutlinePanel"),
    objectOutlineEyebrow: document.getElementById("objectOutlineEyebrow"),
    objectOutlineTitle: document.getElementById("objectOutlineTitle"),
    objectOutlineTree: document.getElementById("objectOutlineTree"),
    mainStage: document.getElementById("mainStage"),
    detailPanel: document.getElementById("detailPanel"),
    selectedDetail: document.getElementById("selectedDetail"),
    detailPreviewCard: document.getElementById("detailPreviewCard"),
    actionMessage: document.getElementById("actionMessage"),
    resourceDrawer: document.getElementById("resourceDrawer"),
    closeResourceDrawerButton: document.getElementById("closeResourceDrawerButton"),
    resourceSearchInput: document.getElementById("resourceSearchInput"),
    resourceResults: document.getElementById("resourceResults"),
    dataStatusChip: document.querySelector(".topbar-actions .status-chip:not(.muted)"),
    detailActionCard: document.querySelectorAll(".detail-panel .detail-card")[2] || null,
  };

  const state = {
    mode: "topic",
    selectedTopicId: topicWorkspace.topicId,
    selectedSectionPlanId: null,
    selectedSectionNodeId: null,
    topicNavExpanded: true,
    collapsedCardIds: new Set(),
    insertContext: null,
    exportingSectionWord: false,
    loadingWorkspace: false,
    loadingSection: false,
    sections: [],
    handouts: [],
    sectionDetail: null,
    sectionItems: [],
    sectionEditor: null,
    selectedItem: null,
    candidates: [],
    activeSession: null,
    pollTimer: null,
  };

  const finalSessionStates = new Set(["已同步", "无变化", "失败", "已取消"]);

  function apiBase() {
    return window.QuestionBankContext.apiBase();
  }

  function sectionApiRoot() {
    return `${apiBase()}/小节`;
  }

  function handoutApiRoot() {
    return `${apiBase()}/讲义`;
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
    updateShell();
    renderTopicTree();

    if (state.mode === "topic") {
      renderTopicWorkspace();
      return;
    }

    renderEditor();
  }

  function updateShell() {
    const editor = currentEditor();
    const isEditorMode = state.mode === "section";
    els.iaLayout.classList.toggle("is-topic-mode", state.mode === "topic");
    els.iaLayout.classList.toggle("is-editor-mode", isEditorMode);
    els.iaLayout.classList.toggle("is-topic-nav-expanded", isEditorMode && state.topicNavExpanded);
    els.iaLayout.classList.toggle("is-topic-nav-compact", !state.topicNavExpanded);
    els.topicNavPanel.classList.toggle("is-compact", !state.topicNavExpanded);
    els.objectOutlinePanel.hidden = !isEditorMode;
    els.detailPanel.hidden = !isEditorMode;
    els.backToTopicButton.hidden = !isEditorMode;
    els.topicNavToggleButton.setAttribute("aria-expanded", String(state.topicNavExpanded));
    els.topicNavToggleButton.setAttribute("aria-label", state.topicNavExpanded ? "收起教学主题导航" : "展开教学主题导航");
    els.topicNavToggleButton.title = state.topicNavExpanded ? "收起教学主题导航" : "展开教学主题导航";
    els.topicNavToggleButton.innerHTML = `
      <svg viewBox="0 0 24 24" aria-hidden="true"><path d="${state.topicNavExpanded ? "m15 18-6-6 6-6" : "m9 18 6-6-6-6"}"></path></svg>
    `;

    if (els.dataStatusChip) {
      els.dataStatusChip.textContent = "真实 API";
    }

    if (state.mode === "topic") {
      const topic = findTopic(state.selectedTopicId) || findTopic(topicWorkspace.topicId);
      els.pageEyebrow.textContent = "教学主题工作台 · 原型 UI";
      els.pageTitle.textContent = topic.title;
      els.breadcrumb.textContent = topicPath(topic.id).join(" / ");
      els.modeStatus.textContent = "主题工作台无右侧详情面板";
      return;
    }

    els.pageEyebrow.textContent = "小节编辑器 · 真实 API";
    els.pageTitle.textContent = editor?.title || "小节编辑器";
    els.breadcrumb.textContent = `功能关系 / ${(editor?.topicTitle || "机械能守恒")} / ${(editor?.title || "未选择小节")}`;
    els.modeStatus.textContent = state.topicNavExpanded
      ? "教学主题树已展开，点击主题才导航"
      : "真实编辑 Section / SectionItem";
    els.objectOutlineEyebrow.textContent = "小节结构树";
    els.objectOutlineTitle.textContent = "当前小节结构";
  }

  function renderTopicTree() {
    const nodes = [mapTopicNode(topicTree)];
    els.topicTree.innerHTML = window.ContentTree.render(nodes, {
      selectedId: state.selectedTopicId,
      emptyHtml: "<div class=\"empty-state\">暂无教学主题</div>",
    });

    window.ContentTree.bind(els.topicTree, {
      onSelect(dataset) {
        selectTopic(dataset.nodeId || dataset.treeNodeId);
      },
    });
  }

  function mapTopicNode(node) {
    return {
      id: node.id,
      title: node.title,
      badge: node.id === state.selectedTopicId ? "当前" : "",
      meta: node.summary ? [node.summary] : [],
      data: { nodeId: node.id, treeKind: "teaching-topic-nav" },
      children: (node.children || []).map(mapTopicNode),
    };
  }

  function renderTopicWorkspace() {
    const topic = findTopic(state.selectedTopicId) || findTopic(topicWorkspace.topicId);
    const isMainTopic = topic.id === topicWorkspace.topicId;

    els.mainStage.innerHTML = `
      <section class="topic-workspace">
        <div class="topic-workspace-header">
          <div>
            <p class="eyebrow">主题工作台</p>
            <h2>主题：${escapeHtml(topic.title)}</h2>
            <p>${escapeHtml(topic.summary || "选择教学主题后，从这里进入小节编排或讲义编排。")}</p>
          </div>
          <span class="status-chip muted">无右侧常驻上下文面板</span>
        </div>

        ${isMainTopic ? renderMainTopicWorkspace() : renderTopicPlaceholder(topic)}
      </section>
    `;

    bindTopicWorkspace();
  }

  function renderMainTopicWorkspace() {
    return `
      <div class="workspace-lanes">
        <section class="workspace-lane section-workspace-card">
          <div class="workspace-lane-heading">
            <div>
              <p class="eyebrow">回答：这个主题应该怎么讲</p>
              <h3>小节编排</h3>
            </div>
            <button class="secondary-button" type="button" data-create-section>
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
              新建小节
            </button>
          </div>
          <div class="workspace-plan-list">
            ${renderSectionPlanCards()}
          </div>
        </section>

        <section class="workspace-lane handout-workspace-card">
          <div class="workspace-lane-heading">
            <div>
              <p class="eyebrow">回答：这份材料应该怎么输出</p>
              <h3>讲义编排</h3>
            </div>
            <button class="secondary-button" type="button" data-open-handouts-page>
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
              前往讲义页
            </button>
          </div>
          <div class="workspace-plan-list">
            ${renderHandoutCards()}
          </div>
        </section>
      </div>

      <div class="secondary-workspace-grid">
        <section class="workspace-secondary-card">
          <div class="workspace-lane-heading">
            <div>
              <p class="eyebrow">次级信息</p>
              <h3>接入说明</h3>
            </div>
          </div>
          <div class="summary-list">
            <span>教学主题树当前仍是原型导航数据，与真实小节数据隔离。</span>
            <span>当前主题下的小节卡片已接入真实小节 API。</span>
            <span>当前主题下的讲义卡片已接入真实讲义 API。</span>
          </div>
        </section>

        <section class="workspace-secondary-card">
          <div class="workspace-lane-heading">
            <div>
              <p class="eyebrow">次级信息</p>
              <h3>引用关系摘要</h3>
            </div>
          </div>
          <div class="summary-list">
            ${topicWorkspace.secondary.references.map((item) => `<span>${escapeHtml(item)}</span>`).join("")}
          </div>
        </section>

        <section class="workspace-secondary-card">
          <div class="workspace-lane-heading">
            <div>
              <p class="eyebrow">次级信息</p>
              <h3>当前题库状态</h3>
            </div>
          </div>
          <div class="summary-list">
            <span>${state.loadingWorkspace ? "正在加载小节和讲义..." : `小节 ${state.sections.length} 个`}</span>
            <span>${state.loadingWorkspace ? "正在加载..." : `讲义 ${state.handouts.length} 个`}</span>
            <span>当前题库：${escapeHtml(window.QuestionBankContext.getCurrentQuestionBankKey())}</span>
          </div>
        </section>
      </div>
    `;
  }

  function renderSectionPlanCards() {
    if (state.loadingWorkspace) {
      return "<div class=\"empty-state\">正在读取小节列表...</div>";
    }

    if (state.sections.length === 0) {
      return "<div class=\"empty-state\">当前题库还没有小节。</div>";
    }

    return state.sections.map((section) => renderWorkspacePlanCard({
      id: String(idOf(section)),
      title: textOf(section.标题),
      status: textOf(section.状态),
      summary: textOf(section.摘要, "暂无摘要"),
      metrics: [
        `项目 ${Number(section.项目数量 || 0)}`,
        `知识点 ${Number(section.知识点数量 || 0)}`,
        `例题 ${Number(section.例题数量 || 0)}`,
        formatDate(section.更新时间),
      ],
    }, "section")).join("");
  }

  function renderHandoutCards() {
    if (state.loadingWorkspace) {
      return "<div class=\"empty-state\">正在读取讲义列表...</div>";
    }

    if (state.handouts.length === 0) {
      return "<div class=\"empty-state\">当前题库还没有讲义。</div>";
    }

    return state.handouts.map((handout) => renderWorkspacePlanCard({
      id: String(idOf(handout)),
      title: textOf(handout.标题),
      status: textOf(handout.状态),
      summary: textOf(handout.摘要, "暂无摘要"),
      metrics: [
        `项目 ${Number(handout.项目数量 || 0)}`,
        handout.最新生成时间 ? "已有生成记录" : "未生成",
        formatDate(handout.更新时间),
      ],
    }, "handout")).join("");
  }

  function renderTopicPlaceholder(topic) {
    return `
      <div class="topic-empty-state">
        <h3>${escapeHtml(topic.title)} 仍是下级教学主题入口</h3>
        <p>当前阶段只把“机械能守恒”接成完整工作台。教学主题树 UI 仍保留，用于后续接入真实教学主题 API。</p>
        <button class="secondary-button" type="button" data-select-main-topic>查看机械能守恒工作台</button>
      </div>
    `;
  }

  function renderWorkspacePlanCard(plan, kind) {
    const action = kind === "section" ? "进入小节编辑器" : "进入讲义编辑器";
    const dataAttr = kind === "section" ? "data-open-section-plan" : "data-open-handout";
    return `
      <button class="workspace-plan-card" type="button" ${dataAttr}="${escapeHtml(plan.id)}">
        <span class="workspace-plan-card__top">
          <strong>${escapeHtml(plan.title)}</strong>
          <span>${escapeHtml(plan.status)}</span>
        </span>
        <span class="workspace-plan-card__summary">${escapeHtml(plan.summary)}</span>
        <span class="workspace-plan-card__meta">
          ${plan.metrics.map((metric) => `<span>${escapeHtml(metric)}</span>`).join("")}
        </span>
        <span class="workspace-plan-card__action">${action}</span>
      </button>
    `;
  }

  function bindTopicWorkspace() {
    els.mainStage.querySelectorAll("[data-open-section-plan]").forEach((button) => {
      button.addEventListener("click", () => openSectionEditor(Number(button.dataset.openSectionPlan)));
    });

    els.mainStage.querySelectorAll("[data-open-handout]").forEach((button) => {
      button.addEventListener("click", () => openHandoutEditor(Number(button.dataset.openHandout)));
    });

    els.mainStage.querySelector("[data-create-section]")?.addEventListener("click", createSectionFromWorkspace);
    els.mainStage.querySelector("[data-open-handouts-page]")?.addEventListener("click", () => {
      window.location.href = "./handouts.html";
    });

    const mainTopicButton = els.mainStage.querySelector("[data-select-main-topic]");
    if (mainTopicButton) {
      mainTopicButton.addEventListener("click", async () => {
        state.selectedTopicId = topicWorkspace.topicId;
        await loadTopicWorkspaceData();
        render();
      });
    }
  }

  function renderEditor() {
    const editor = currentEditor();
    const selectedId = selectedNodeId();
    const selectedItem = findItem(editor.items, selectedId) || editor.items[0] || null;
    const rootNode = buildRootOutlineNode(editor);

    els.objectOutlineTree.innerHTML = window.ContentTree.render([rootNode], {
      selectedId,
      emptyHtml: "<div class=\"empty-state\">暂无结构</div>",
    });
    window.ContentTree.bind(els.objectOutlineTree, {
      onSelect(dataset) {
        selectEditorNode(dataset.nodeId || dataset.treeNodeId, { scroll: true });
      },
    });

    els.mainStage.innerHTML = `
      <section class="editor-workspace is-section-editor">
        <div class="editor-header">
          <div>
            <p class="eyebrow">小节展开内容区</p>
            <h2>${escapeHtml(editor.title)}</h2>
            <p>${escapeHtml(editor.summary)}</p>
          </div>
          <div class="editor-header-actions">
            <button class="primary-button" type="button" data-export-section-word ${state.exportingSectionWord ? "disabled" : ""}>
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 3h8l4 4v14H6z"></path><path d="M14 3v5h5M9 15h6M9 18h6"></path></svg>
              ${state.exportingSectionWord ? "正在导出..." : "导出当前小节 Word"}
            </button>
            <button class="secondary-button" type="button" data-open-picker-root>
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M21 21l-4.3-4.3"></path><circle cx="11" cy="11" r="7"></circle></svg>
              从资源库插入
            </button>
          </div>
        </div>
        <div class="expanded-content-list">
          ${editor.items.length
            ? editor.items.map((item, index) => `
              ${renderInsertZone(`before-${item.id}`, index === 0 ? "添加到最前" : "插入到此处")}
              ${renderComposerItem(item, 0)}
            `).join("")
            : "<div class=\"empty-state\">当前小节还没有内容块。先从资源库插入已有内容块。</div>"}
          ${renderInsertZone("end", "添加到末尾")}
        </div>
      </section>
    `;

    bindEditorWorkspace();
    renderDetail(selectedItem);
  }

  function buildRootOutlineNode(editor) {
    return {
      id: "section-root",
      title: `小节：${editor.title}`,
      badge: editor.status,
      meta: [editor.items.length > 0 ? `${editor.items.length} 项` : "空小节"],
      data: { nodeId: "section-root", treeKind: "object-outline-tree" },
      children: editor.items.map(mapOutlineNode),
    };
  }

  function mapOutlineNode(item) {
    return {
      id: item.id,
      title: item.title,
      badge: item.type,
      meta: [item.structureType, item.versionStatus].filter(Boolean),
      data: { nodeId: item.id, treeKind: "object-outline-tree" },
      children: (item.children || []).map(mapOutlineNode),
    };
  }

  function renderComposerItem(item, depth) {
    const hasChildren = Boolean(item.children?.length);
    if (!hasChildren) {
      return renderComposerCard(item, depth, false);
    }

    const collapsed = state.collapsedCardIds.has(item.id);
    return `
      <section class="section-card-group${item.isReferenceView ? " is-reference-view" : ""}" data-card-group-id="${escapeHtml(item.id)}">
        ${renderComposerCard(item, depth, true)}
        <div class="section-card-group__children${collapsed ? " is-collapsed" : ""}">
          ${item.children.map((child, index) => `
            ${renderInsertZone(`inside-${item.id}-${index}`, "添加子卡片")}
            ${renderComposerItem(child, depth + 1)}
          `).join("")}
          ${renderInsertZone(`inside-${item.id}-end`, "添加子卡片")}
        </div>
      </section>
    `;
  }

  function renderComposerCard(item, depth, hasChildren) {
    const selected = item.id === selectedNodeId();
    const collapsed = state.collapsedCardIds.has(item.id);
    return `
      <article class="section-content-card editor-content-card${selected ? " is-selected" : ""}${item.isReferenceView ? " is-reference-view" : ""}" id="content-${escapeHtml(item.id)}" data-content-id="${escapeHtml(item.id)}" data-depth="${depth}">
        <div class="section-content-card__fold">
          ${hasChildren ? `
            <button class="fold-button" type="button" data-collapse-id="${escapeHtml(item.id)}" aria-label="${collapsed ? "展开" : "收起"}${escapeHtml(item.title)}" aria-expanded="${String(!collapsed)}">
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="${collapsed ? "m9 18 6-6-6-6" : "m6 9 6 6 6-6"}"></path></svg>
            </button>
          ` : ""}
        </div>
        <div class="section-content-card__body" role="button" tabindex="0" data-select-node="${escapeHtml(item.id)}">
          <div class="section-content-card__header">
            <strong class="section-content-card__title">${escapeHtml(item.title)}</strong>
            <span class="section-content-card__badges">
              <span class="section-content-card__type section-content-card__type--${typeClass(item.type)}">${escapeHtml(item.type)}</span>
              <span class="section-content-card__structure section-content-card__structure--${structureClass(item.structureType)}">${escapeHtml(item.structureType)}</span>
            </span>
          </div>
          ${item.referenceNote ? `<div class="reference-view-banner">${escapeHtml(item.referenceNote)}</div>` : ""}
          <div class="section-content-card__html">${item.html}</div>
        </div>
        ${renderCardActions(item)}
      </article>
    `;
  }

  function renderCardActions(item) {
    const index = state.sectionEditor?.items.findIndex((candidate) => candidate.id === item.id) ?? -1;
    const canMoveUp = index > 0;
    const canMoveDown = index >= 0 && index < state.sectionEditor.items.length - 1;
    return `
      <div class="section-content-card__actions" aria-label="${escapeHtml(item.title)} 操作">
        <button class="section-action-button" type="button" data-action="word" data-item-id="${escapeHtml(item.id)}" title="Word 编辑" aria-label="Word 编辑">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 4h16v16H4z"></path><path d="M8 8h8M8 12h8M8 16h5"></path></svg>
        </button>
        <button class="section-action-button" type="button" data-action="insert" data-item-id="${escapeHtml(item.id)}" title="从资源库插入" aria-label="从资源库插入">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M21 21l-4.3-4.3"></path><circle cx="11" cy="11" r="7"></circle></svg>
        </button>
        <button class="section-action-button" type="button" data-action="move-up" data-item-id="${escapeHtml(item.id)}" title="上移" aria-label="上移" ${canMoveUp ? "" : "disabled"}>
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m18 15-6-6-6 6"></path></svg>
        </button>
        <button class="section-action-button" type="button" data-action="move-down" data-item-id="${escapeHtml(item.id)}" title="下移" aria-label="下移" ${canMoveDown ? "" : "disabled"}>
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m6 9 6 6 6-6"></path></svg>
        </button>
        <button class="section-action-button is-danger" type="button" data-action="remove" data-item-id="${escapeHtml(item.id)}" title="移除" aria-label="移除">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M18 6 6 18"></path><path d="m6 6 12 12"></path></svg>
        </button>
      </div>
    `;
  }

  function renderInsertZone(position, label) {
    return `
      <div class="insert-zone" data-insert-position="${escapeHtml(position)}" aria-label="${escapeHtml(label)}">
        <span class="insert-zone__line">${escapeHtml(label)}</span>
        <div class="insert-zone__menu" role="group" aria-label="${escapeHtml(label)}">
          <button class="insert-zone__button" type="button" data-insert-action="insert" data-insert-position="${escapeHtml(position)}">插入卡片</button>
          <button class="insert-zone__button" type="button" data-insert-action="create" data-insert-position="${escapeHtml(position)}">新建卡片</button>
        </div>
      </div>
    `;
  }

  function bindEditorWorkspace() {
    els.mainStage.querySelectorAll("[data-select-node]").forEach((node) => {
      node.addEventListener("click", (event) => {
        event.stopPropagation();
        selectEditorNode(node.dataset.selectNode, { scroll: false });
      });
      node.addEventListener("keydown", (event) => {
        if (event.key === "Enter" || event.key === " ") {
          event.preventDefault();
          selectEditorNode(node.dataset.selectNode, { scroll: false });
        }
      });
    });

    els.mainStage.querySelectorAll("[data-collapse-id]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        toggleCollapsed(button.dataset.collapseId);
      });
    });

    els.mainStage.querySelectorAll("[data-insert-action]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        handleInsertAction(button.dataset.insertAction, button.dataset.insertPosition);
      });
    });

    els.mainStage.querySelector("[data-export-section-word]")?.addEventListener("click", exportCurrentSectionWord);
    els.mainStage.querySelector("[data-open-picker-root]")?.addEventListener("click", () => openResourceSearch({ position: "end" }));

    els.mainStage.querySelectorAll("[data-action]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        handleCardAction(button.dataset.action, button.dataset.itemId);
      });
    });
  }

  function renderDetail(item) {
    if (!item) {
      els.selectedDetail.innerHTML = "<div class=\"empty-state\">选择一个内容块</div>";
      els.detailPreviewCard.innerHTML = "";
      bindDetailActionButtons(null);
      return;
    }

    els.selectedDetail.innerHTML = `
      <h3 class="selected-title">${escapeHtml(item.title)}</h3>
      <div class="node-meta">
        <span><strong>类型</strong>${escapeHtml(item.type)}</span>
        <span><strong>块类型</strong>${escapeHtml(item.structureType)}</span>
        <span><strong>来源</strong>${escapeHtml(item.source)}</span>
        <span><strong>引用方式</strong>${escapeHtml(item.referenceMode)}</span>
        <span><strong>版本状态</strong>${escapeHtml(item.versionStatus)}</span>
        <span><strong>内容角色</strong>${escapeHtml(item.Role || "未设置")}</span>
        <span><strong>难度</strong>${escapeHtml(item.Difficulty || "未设置")}</span>
        <span><strong>用途</strong>${escapeHtml(item.Usage || "未设置")}</span>
        <span><strong>题型</strong>${escapeHtml(item.QuestionType || "未设置")}</span>
        <span><strong>默认选入</strong>${item.DefaultIncluded ? "是" : "否"}</span>
      </div>
      <p class="selected-summary">${escapeHtml(item.detail || "暂无摘要")}</p>
      ${item.Note ? `<p class="reference-detail-note">${escapeHtml(item.Note)}</p>` : ""}
    `;

    els.detailPreviewCard.innerHTML = `
      <div class="detail-card-heading">
        <h3>内容预览</h3>
        <span class="status-chip muted">${escapeHtml(item.versionStatus || "预览")}</span>
      </div>
      <div class="preview-surface">${item.html}</div>
    `;

    bindDetailActionButtons(item);
    setActionMessage(sessionMessage() || `已选中：${item.title}`);
  }

  function bindDetailActionButtons(item) {
    const detailRoot = els.detailActionCard || els.detailPanel;
    if (!detailRoot) return;

    detailRoot.querySelectorAll("[data-placeholder]").forEach((button) => {
      if (button.dataset.boundRealAction === "true") {
        button.replaceWith(button.cloneNode(true));
      }
    });

    const wordButton = detailRoot.querySelector("[data-placeholder=\"Word 编辑\"]");
    const insertButton = detailRoot.querySelector("[data-placeholder=\"从资源库插入\"]");
    const childButton = detailRoot.querySelector("[data-placeholder=\"添加下级内容\"]");

    if (wordButton) {
      wordButton.disabled = !item;
      wordButton.addEventListener("click", () => {
        if (!item) return;
        editContentInWord(item);
      });
      wordButton.dataset.boundRealAction = "true";
    }

    if (insertButton) {
      insertButton.disabled = state.mode !== "section";
      insertButton.addEventListener("click", () => openResourceSearch({ position: "end" }));
      insertButton.dataset.boundRealAction = "true";
    }

    if (childButton) {
      childButton.addEventListener("click", () => {
        setActionMessage("当前阶段仍只支持向小节插入已有内容块，不在本页新建下级内容。");
      });
      childButton.dataset.boundRealAction = "true";
    }
  }

  function renderContentBlockCard(block) {
    return `
      <article class="content-block-card">
        <div class="content-block-card__top">
          <strong class="content-block-card__title" title="${escapeHtml(block.title)}">${escapeHtml(block.title)}</strong>
          <span class="content-block-card__right">
            <span class="content-block-card__type content-block-card__type--${typeClass(block.type)}">${escapeHtml(block.type)}</span>
            <span class="content-block-card__structure content-block-card__structure--${structureClass(block.structureType)}">${escapeHtml(block.structureType)}</span>
          </span>
        </div>
        ${block.remark ? `<div class="content-block-card__remark">${escapeHtml(block.remark)}</div>` : ""}
        <div class="content-block-card__meta">
          ${(block.meta || []).map((meta) => `<span class="content-block-card__property">${escapeHtml(meta)}</span>`).join("")}
        </div>
      </article>
    `;
  }

  function renderResourceResults() {
    const referenceMode = state.insertContext?.referenceMode || "跟随最新";
    if (!Array.isArray(state.candidates) || state.candidates.length === 0) {
      els.resourceResults.innerHTML = `
        <div class="resource-picker-toolbar">
          <label>
            <span>引用模式</span>
            <select data-reference-mode>
              <option value="跟随最新"${referenceMode === "跟随最新" ? " selected" : ""}>跟随最新</option>
              <option value="锁定版本"${referenceMode === "锁定版本" ? " selected" : ""}>锁定当前版本</option>
            </select>
          </label>
        </div>
        <p class="resource-empty">没有匹配的内容块。</p>
      `;
      bindResourceToolbar();
      return;
    }

    els.resourceResults.innerHTML = `
      <div class="resource-picker-toolbar">
        <label>
          <span>引用模式</span>
          <select data-reference-mode>
            <option value="跟随最新"${referenceMode === "跟随最新" ? " selected" : ""}>跟随最新</option>
            <option value="锁定版本"${referenceMode === "锁定版本" ? " selected" : ""}>锁定当前版本</option>
          </select>
        </label>
      </div>
      <div class="resource-candidate-list">
        ${state.candidates.map((item) => `
          <button class="content-block-card resource-picker-card${item.状态 === "已废弃" ? " is-disabled" : ""}" type="button" data-resource-id="${escapeHtml(String(idOf(item)))}" ${item.状态 === "已废弃" ? "disabled" : ""}>
            <span class="content-block-card__top">
              <strong class="content-block-card__title" title="${escapeHtml(item.标题)}">${escapeHtml(item.标题)}</strong>
              <span class="content-block-card__right">
                <span class="content-block-card__type content-block-card__type--${typeClass(item.类型)}">${escapeHtml(item.类型)}</span>
                <span class="content-block-card__structure content-block-card__structure--${structureClass(item.结构类型)}">${escapeHtml(item.结构类型)}</span>
              </span>
            </span>
            ${item.Note ? `<span class="content-block-card__remark">${escapeHtml(item.Note)}</span>` : ""}
            <span class="content-block-card__meta">
              ${buildCandidateMeta(item).map((meta) => `<span class="content-block-card__property">${escapeHtml(meta)}</span>`).join("")}
            </span>
          </button>
        `).join("")}
      </div>
    `;

    bindResourceToolbar();
    els.resourceResults.querySelectorAll("[data-resource-id]").forEach((button) => {
      button.addEventListener("click", () => {
        const item = state.candidates.find((candidate) => String(idOf(candidate)) === button.dataset.resourceId);
        if (item) {
          addBlockToSection(item);
        }
      });
    });
  }

  function bindResourceToolbar() {
    els.resourceResults.querySelector("[data-reference-mode]")?.addEventListener("change", (event) => {
      if (!state.insertContext) return;
      state.insertContext.referenceMode = event.currentTarget.value;
    });
  }

  function buildCandidateMeta(item) {
    return [
      textOf(item.状态),
      item.当前版本号 ? `当前版本 v${item.当前版本号}` : "无当前版本",
      item.RoleOptionName || "未设角色",
      item.DifficultyOptionName || "未设难度",
      item.UsageOptionName || "未设用途",
    ];
  }

  async function selectTopic(topicId) {
    const topic = findTopic(topicId);
    if (!topic) return;

    state.selectedTopicId = topic.id;
    state.mode = "topic";
    state.topicNavExpanded = true;
    clearPoll();

    if (topic.id === topicWorkspace.topicId) {
      await loadTopicWorkspaceData();
    }

    render();
  }

  async function loadTopicWorkspaceData() {
    state.loadingWorkspace = true;
    render();

    try {
      const [sections, handouts] = await Promise.all([
        requestJson(sectionApiRoot()),
        requestJson(handoutApiRoot()),
      ]);
      state.sections = Array.isArray(sections) ? sections : [];
      state.handouts = Array.isArray(handouts) ? handouts : [];
    } catch (error) {
      state.sections = [];
      state.handouts = [];
      setActionMessage(error instanceof Error ? error.message : "读取主题工作台数据失败。");
    } finally {
      state.loadingWorkspace = false;
      render();
    }
  }

  async function openSectionEditor(planId) {
    const sectionId = Number(planId);
    if (!Number.isInteger(sectionId) || sectionId <= 0) {
      return;
    }

    state.mode = "section";
    state.selectedSectionPlanId = sectionId;
    state.topicNavExpanded = false;
    setActionMessage("正在加载真实小节...");
    await loadSectionEditorData(sectionId);
    render();
  }

  function openHandoutEditor(handoutId) {
    const target = Number(handoutId);
    window.location.href = `./handouts.html${Number.isInteger(target) && target > 0 ? `?id=${encodeURIComponent(target)}` : ""}`;
  }

  async function loadSectionEditorData(sectionId, preferredNodeId = null) {
    state.loadingSection = true;
    render();

    try {
      const [sectionDetail, sectionItems] = await Promise.all([
        requestJson(`${sectionApiRoot()}/${encodeURIComponent(sectionId)}`),
        requestJson(`${sectionApiRoot()}/${encodeURIComponent(sectionId)}/项目`),
      ]);

      state.sectionDetail = sectionDetail;
      state.sectionItems = Array.isArray(sectionItems) ? sectionItems : [];
      state.sectionEditor = buildSectionEditorModel(sectionDetail, state.sectionItems);
      state.selectedSectionNodeId = preferredNodeId || state.sectionEditor.items[0]?.id || null;
      state.selectedItem = findItem(state.sectionEditor.items, state.selectedSectionNodeId) || state.sectionEditor.items[0] || null;
      state.collapsedCardIds.clear();
      state.activeSession = null;
    } catch (error) {
      state.sectionDetail = null;
      state.sectionItems = [];
      state.sectionEditor = buildSectionEditorModel(null, []);
      state.selectedSectionNodeId = null;
      state.selectedItem = null;
      setActionMessage(error instanceof Error ? error.message : "读取小节详情失败。");
    } finally {
      state.loadingSection = false;
    }
  }

  function buildSectionEditorModel(sectionDetail, sectionItems) {
    const items = sectionItems
      .slice()
      .sort((left, right) => Number(left.排序 || 0) - Number(right.排序 || 0))
      .map(mapSectionItemToEditorItem);

    return {
      id: sectionDetail ? `section-${idOf(sectionDetail)}` : "section-empty",
      title: sectionDetail ? textOf(sectionDetail.标题) : "未选择小节",
      topicTitle: "机械能守恒",
      status: sectionDetail ? textOf(sectionDetail.状态) : "空状态",
      summary: sectionDetail
        ? textOf(sectionDetail.摘要, "这个小节回答：当前教学主题应该怎么讲。")
        : "当前小节尚未加载。",
      sectionId: sectionDetail ? idOf(sectionDetail) : null,
      items,
    };
  }

  function mapSectionItemToEditorItem(item) {
    const contentBlockId = Number(item.内容块ID || 0);
    const currentVersion = item.引用版本号 || item.内容块当前版本号 || item.当前版本号;
    const currentVersionId = item.引用版本ID || item.内容块版本ID || item.内容块当前版本ID;
    const note = textOf(item.Note, "");
    const summary = textOf(item.内容块摘要, "");
    const structureType = textOf(item.内容块结构类型, "原子块");
    const previewHtml = currentVersionId
      ? `<iframe class="detail-preview-frame" src="${contentPreviewUrl(contentBlockId)}" title="${escapeHtml(textOf(item.内容块标题))} 预览"></iframe>`
      : "<div class=\"empty-state\">这个内容块没有当前版本，暂无预览。</div>";

    return {
      id: `section-item-${item.Id}`,
      itemId: Number(item.Id),
      contentBlockId,
      title: textOf(item.内容块标题),
      type: textOf(item.RoleOptionName || item.角色 || item.内容块类型),
      rawType: textOf(item.内容块类型),
      structureType,
      source: `内容块 #${contentBlockId}`,
      referenceMode: textOf(item.引用版本模式),
      versionStatus: currentVersion ? `v${currentVersion}` : "无当前版本",
      detail: summary || note || "暂无摘要",
      html: previewHtml,
      previewUrl: currentVersionId ? contentPreviewUrl(contentBlockId) : "",
      Role: textOf(item.RoleOptionName, "未设置"),
      Difficulty: textOf(item.DifficultyOptionName, "未设置"),
      Usage: textOf(item.UsageOptionName, "未设置"),
      QuestionType: textOf(item.QuestionTypeOptionName, "未设置"),
      DefaultIncluded: item.DefaultIncluded !== false,
      Note: note,
      isReferenceView: false,
      children: [],
    };
  }

  function contentPreviewUrl(contentBlockId) {
    return `${contentApiRoot()}/${encodeURIComponent(contentBlockId)}/预览html?t=${Date.now()}`;
  }

  function createSectionFromWorkspace() {
    const title = window.prompt("请输入小节标题");
    if (!title || !title.trim()) {
      return;
    }

    createSection(title.trim());
  }

  async function createSection(title) {
    try {
      const created = await requestJson(sectionApiRoot(), {
        method: "POST",
        body: JSON.stringify({
          标题: title,
          摘要: null,
          状态: "草稿",
        }),
      });
      await loadTopicWorkspaceData();
      await openSectionEditor(idOf(created));
    } catch (error) {
      setActionMessage(error instanceof Error ? error.message : "新建小节失败。");
    }
  }

  function selectEditorNode(nodeId, options = {}) {
    if (!nodeId) return;

    if (nodeId === "section-root") {
      const first = currentEditor().items[0];
      nodeId = first?.id || nodeId;
    }

    state.selectedSectionNodeId = nodeId;
    state.selectedItem = findItem(currentEditor().items, nodeId) || null;
    renderEditor();
    if (state.selectedItem) {
      setActionMessage(`已选中：${state.selectedItem.title}。右侧详情面板已更新。`);
    }

    if (options.scroll) {
      window.setTimeout(() => scrollToContent(nodeId), 0);
    }
  }

  function toggleCollapsed(cardId) {
    if (state.collapsedCardIds.has(cardId)) {
      state.collapsedCardIds.delete(cardId);
    } else {
      state.collapsedCardIds.add(cardId);
    }
    renderEditor();
  }

  function handleInsertAction(action, position) {
    if (action === "create") {
      setActionMessage("本轮只把小节接入真实 API，不在这里新建内容块。");
      return;
    }

    openResourceSearch({ position });
  }

  function openResourceSearch(context = {}) {
    state.insertContext = {
      position: context.position || "end",
      referenceMode: "跟随最新",
    };
    els.resourceDrawer.classList.remove("is-hidden");
    renderResourceResults();
    loadContentCandidates();
    window.setTimeout(() => els.resourceSearchInput.focus(), 0);
  }

  function closeResourceSearch() {
    els.resourceDrawer.classList.add("is-hidden");
  }

  async function loadContentCandidates() {
    const keyword = els.resourceSearchInput.value.trim();
    const params = new URLSearchParams();
    if (keyword) {
      params.set("关键词", keyword);
    }

    try {
      const candidates = await requestJson(`${contentApiRoot()}${params.toString() ? `?${params}` : ""}`);
      state.candidates = Array.isArray(candidates) ? candidates : [];
    } catch (error) {
      state.candidates = [];
      setActionMessage(error instanceof Error ? error.message : "读取内容块候选失败。");
    }

    renderResourceResults();
  }

  async function addBlockToSection(candidate) {
    if (!state.sectionDetail) {
      return;
    }

    const referenceMode = state.insertContext?.referenceMode || "跟随最新";
    const body = {
      内容块ID: idOf(candidate),
      引用版本模式: referenceMode,
    };

    if (referenceMode === "锁定版本") {
      const currentVersionId = Number(candidate.当前版本ID || 0);
      if (!currentVersionId) {
        setActionMessage("这个内容块没有当前版本，不能锁定当前版本。");
        return;
      }
      body.内容块版本ID = currentVersionId;
    }

    try {
      const addedItem = await requestJson(`${sectionApiRoot()}/${encodeURIComponent(idOf(state.sectionDetail))}/项目`, {
        method: "POST",
        body: JSON.stringify(body),
      });

      const newItemNodeId = `section-item-${addedItem.Id}`;
      const orderedIds = reorderItemIdsAfterInsert(addedItem.Id, state.insertContext?.position || "end");
      if (orderedIds) {
        await saveItemOrder(orderedIds);
      }

      closeResourceSearch();
      await loadSectionEditorData(idOf(state.sectionDetail), newItemNodeId);
      await loadTopicWorkspaceData();
      render();
      window.setTimeout(() => scrollToContent(newItemNodeId), 0);
      setActionMessage(`已加入小节：${textOf(candidate.标题)}`);
    } catch (error) {
      setActionMessage(error instanceof Error ? error.message : "添加内容块失败。");
    }
  }

  function reorderItemIdsAfterInsert(newItemId, position) {
    const currentIds = state.sectionItems
      .slice()
      .sort((left, right) => Number(left.排序 || 0) - Number(right.排序 || 0))
      .map((item) => Number(item.Id));

    if (position === "end") {
      return null;
    }

    const targetIndex = resolveInsertIndex(position, currentIds.length);
    if (targetIndex === null) {
      return null;
    }

    currentIds.push(Number(newItemId));
    const withoutNew = currentIds.filter((id) => id !== Number(newItemId));
    withoutNew.splice(targetIndex, 0, Number(newItemId));
    return withoutNew;
  }

  function resolveInsertIndex(position, length) {
    if (!position || position === "end") {
      return null;
    }

    if (position.startsWith("before-")) {
      const nodeId = position.slice("before-".length);
      const index = currentEditor().items.findIndex((item) => item.id === nodeId);
      return index >= 0 ? index : null;
    }

    return length;
  }

  async function saveItemOrder(itemIds) {
    if (!state.sectionDetail) {
      return;
    }

    await requestJson(`${sectionApiRoot()}/${encodeURIComponent(idOf(state.sectionDetail))}/项目排序`, {
      method: "PUT",
      body: JSON.stringify({
        项目排序列表: itemIds.map((itemId, index) => ({
          小节项ID: Number(itemId),
          排序: index,
        })),
      }),
    });
  }

  async function handleCardAction(action, nodeId) {
    const item = findItem(currentEditor().items, nodeId);
    if (!item) {
      return;
    }

    if (action === "word") {
      await editContentInWord(item);
      return;
    }

    if (action === "insert") {
      openResourceSearch({ position: `before-${item.id}` });
      return;
    }

    if (action === "move-up" || action === "move-down") {
      await moveSectionItem(item, action === "move-up" ? -1 : 1);
      return;
    }

    if (action === "remove") {
      await removeSectionItem(item);
    }
  }

  async function moveSectionItem(item, direction) {
    const items = currentEditor().items.slice();
    const index = items.findIndex((candidate) => candidate.id === item.id);
    const targetIndex = index + direction;
    if (index < 0 || targetIndex < 0 || targetIndex >= items.length || !state.sectionDetail) {
      return;
    }

    const ordered = items.map((candidate) => candidate.itemId);
    const [moved] = ordered.splice(index, 1);
    ordered.splice(targetIndex, 0, moved);

    try {
      await saveItemOrder(ordered);
      await loadSectionEditorData(idOf(state.sectionDetail), item.id);
      render();
      window.setTimeout(() => scrollToContent(item.id), 0);
      setActionMessage(`已调整顺序：${item.title}`);
    } catch (error) {
      setActionMessage(error instanceof Error ? error.message : "调整排序失败。");
    }
  }

  async function removeSectionItem(item) {
    if (!state.sectionDetail) {
      return;
    }

    const confirmed = window.confirm(`只移除当前小节中的引用，不删除源内容块：${item.title}。确认继续？`);
    if (!confirmed) {
      return;
    }

    try {
      await requestJson(`${sectionApiRoot()}/${encodeURIComponent(idOf(state.sectionDetail))}/项目/${encodeURIComponent(item.itemId)}`, {
        method: "DELETE",
      });
      await loadSectionEditorData(idOf(state.sectionDetail));
      await loadTopicWorkspaceData();
      render();
      setActionMessage(`已从小节移除：${item.title}`);
    } catch (error) {
      setActionMessage(error instanceof Error ? error.message : "移除小节项失败。");
    }
  }

  async function editContentInWord(item) {
    try {
      const session = await requestJson(`${contentApiRoot()}/${encodeURIComponent(item.contentBlockId)}/编辑会话`, {
        method: "POST",
        body: JSON.stringify({ 是否打开Word: true }),
      });
      setSession(session);
      startPoll(session.会话ID);
      setActionMessage(session.消息 || "已创建 Word 编辑会话。");
    } catch (error) {
      setActionMessage(error instanceof Error ? error.message : "创建编辑会话失败。");
    }
  }

  function setSession(session) {
    state.activeSession = session;
    if (session && finalSessionStates.has(session.状态)) {
      clearPoll();
    }
  }

  function startPoll(sessionId) {
    clearPoll();
    state.pollTimer = window.setInterval(() => pollSession(sessionId), 2000);
  }

  function clearPoll() {
    if (state.pollTimer) {
      window.clearInterval(state.pollTimer);
      state.pollTimer = null;
    }
  }

  async function pollSession(sessionId) {
    try {
      const session = await requestJson(`${contentApiRoot()}/编辑会话/${encodeURIComponent(sessionId)}`);
      setSession(session);
      setActionMessage(sessionMessage());

      if ((session.状态 === "已同步" || session.状态 === "无变化") && state.sectionDetail) {
        await loadSectionEditorData(idOf(state.sectionDetail), selectedNodeId());
        render();
      }
    } catch (error) {
      clearPoll();
      setActionMessage(error instanceof Error ? error.message : "同步编辑会话失败。");
    }
  }

  function sessionMessage() {
    if (!state.activeSession) {
      return "";
    }

    return state.activeSession.消息 || state.activeSession.错误信息 || `编辑会话状态：${state.activeSession.状态 || "-"}`;
  }

  async function exportCurrentSectionWord() {
    if (state.mode !== "section" || state.exportingSectionWord || !state.sectionDetail) {
      return;
    }

    state.exportingSectionWord = true;
    renderEditor();
    setActionMessage("正在导出当前小节整体 Word，不会改变当前选中块。");

    try {
      const result = await requestBlob(`${sectionApiRoot()}/${encodeURIComponent(idOf(state.sectionDetail))}/导出Word`, {
        method: "POST",
      });
      const fileName = result.fileName || `${textOf(state.sectionDetail.标题)}-${formatTimestamp(new Date())}.docx`;
      downloadBlob(result.blob, fileName);
      setActionMessage(`已导出当前小节整体 Word：${fileName}`);
    } catch (error) {
      setActionMessage(error instanceof Error ? error.message : "导出当前小节 Word 失败。");
    } finally {
      state.exportingSectionWord = false;
      renderEditor();
    }
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

  function currentEditor() {
    return state.sectionEditor || buildSectionEditorModel(null, []);
  }

  function selectedNodeId() {
    return state.selectedSectionNodeId;
  }

  function findTopic(topicId, node = topicTree, path = []) {
    const nextPath = [...path, node.title];
    if (node.id === topicId) {
      return { ...node, path: nextPath };
    }
    for (const child of node.children || []) {
      const found = findTopic(topicId, child, nextPath);
      if (found) return found;
    }
    return null;
  }

  function topicPath(topicId) {
    return findTopic(topicId)?.path || ["功能关系", "机械能守恒"];
  }

  function findItem(items, itemId) {
    for (const item of items || []) {
      if (item.id === itemId) return item;
      const found = findItem(item.children, itemId);
      if (found) return found;
    }
    return null;
  }

  function scrollToContent(nodeId) {
    const node = document.getElementById(`content-${nodeId}`);
    if (node) {
      node.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  }

  function typeClass(type) {
    return {
      知识点: "knowledge",
      例题: "example",
      练习: "exercise",
      方法总结: "method",
      易错点: "mistake",
      普通说明: "note",
      题目: "question",
      题组: "group",
      例题组: "group",
      练习组: "group",
      小节: "group",
      模型: "method",
    }[type] || "default";
  }

  function structureClass(structureType) {
    return {
      原子块: "atomic",
      组合块: "composite",
      入口卡片: "entry",
      主题入口: "entry",
      引用展开: "composite",
      独立内容: "atomic",
    }[structureType] || "default";
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll("\"", "&quot;")
      .replaceAll("'", "&#039;");
  }

  function idOf(value) {
    const raw = value?.Id ?? value?.id ?? value?.ID;
    const number = Number(raw);
    return Number.isInteger(number) && number > 0 ? number : null;
  }

  function textOf(value, fallback = "-") {
    return value === null || value === undefined || value === "" ? fallback : String(value);
  }

  function formatDate(value) {
    if (!value) {
      return "未知时间";
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return "未知时间";
    }

    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
  }

  function formatTimestamp(date) {
    const pad = (value) => String(value).padStart(2, "0");
    return `${date.getFullYear()}${pad(date.getMonth() + 1)}${pad(date.getDate())}${pad(date.getHours())}${pad(date.getMinutes())}${pad(date.getSeconds())}`;
  }

  function setActionMessage(message) {
    if (els.actionMessage) {
      els.actionMessage.textContent = message;
    }
  }

  function debounce(fn, delay = 220) {
    let timer = null;
    return (...args) => {
      window.clearTimeout(timer);
      timer = window.setTimeout(() => fn(...args), delay);
    };
  }

  function bindEvents() {
    els.backToTopicButton.addEventListener("click", async () => {
      state.mode = "topic";
      state.topicNavExpanded = true;
      if (state.selectedTopicId === topicWorkspace.topicId) {
        await loadTopicWorkspaceData();
      }
      render();
    });

    els.topicNavToggleButton.addEventListener("click", () => {
      state.topicNavExpanded = !state.topicNavExpanded;
      render();
    });

    els.resourceSearchInput.addEventListener("input", debounce(loadContentCandidates));
    els.closeResourceDrawerButton.addEventListener("click", closeResourceSearch);
    els.resourceDrawer.addEventListener("click", (event) => {
      if (event.target === els.resourceDrawer) {
        closeResourceSearch();
      }
    });
  }

  async function init() {
    bindEvents();
    render();
    await window.QuestionBankContext.initSwitcher({ onChange: handleQuestionBankChanged });
  }

  async function handleQuestionBankChanged() {
    state.sections = [];
    state.handouts = [];
    state.sectionDetail = null;
    state.sectionItems = [];
    state.sectionEditor = buildSectionEditorModel(null, []);
    state.selectedSectionNodeId = null;
    state.activeSession = null;
    clearPoll();

    if (state.selectedTopicId === topicWorkspace.topicId) {
      await loadTopicWorkspaceData();
    }

    if (state.mode === "section" && state.selectedSectionPlanId) {
      await loadSectionEditorData(state.selectedSectionPlanId);
    }

    render();
  }

  init();
})();
