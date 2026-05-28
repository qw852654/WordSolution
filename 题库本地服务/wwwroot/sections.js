(function () {
  const sectionTree = {
    id: "function-relation",
    title: "功能关系",
    children: [
      {
        id: "mechanical-energy",
        title: "机械能守恒",
        summary: "围绕机械能守恒条件、能量转化和典型模型建立一节课的内容结构。",
        status: "静态原型",
        children: [
          { id: "vertical-circle", title: "竖直圆轨道", summary: "圆周运动中的能量守恒与临界条件。" },
          { id: "rod-model", title: "杆模型", summary: "轻杆连接体中的约束、速度关系与能量表达。" },
          { id: "ball-model", title: "球模型", summary: "小球、轨道、弹簧等模型中的能量转化。" },
        ],
      },
    ],
  };

  const workspaceCards = [
    {
      id: "knowledge-energy",
      kind: "content",
      title: "机械能守恒的条件与表达",
      type: "知识点",
      structureType: "原子块",
      status: "可复用",
      version: "v3",
      versionStatus: "当前版本",
      tags: ["功能关系", "机械能", "一轮复习"],
      summary: "明确只有重力或弹力做功时机械能守恒，并区分系统内力与外力做功。",
      preview: {
        title: "机械能守恒的条件",
        paragraphs: [
          "当系统内只有重力或弹力做功，其他力不做功或做功代数和为零时，系统机械能保持不变。",
          "解题时先选系统，再判断非保守力做功，最后列出初末状态的能量方程。",
        ],
        equation: "E_k1 + E_p1 = E_k2 + E_p2",
      },
    },
    {
      id: "example-group",
      kind: "content",
      title: "机械能守恒例题组",
      type: "例题组",
      structureType: "组合块",
      status: "可复用",
      version: "v2",
      versionStatus: "组合渲染",
      tags: ["例题组", "守恒条件", "模型迁移"],
      summary: "由两道例题组成，先判断守恒条件，再处理速度与高度关系。",
      preview: {
        title: "例题组预览",
        paragraphs: [
          "例题 1：光滑斜面上滑块从高度 h 处释放，求底端速度。",
          "例题 2：小球沿光滑轨道滑下后压缩弹簧，求最大压缩量。",
        ],
        callout: "组合块预览应由后端递归渲染。本轮展示静态组合渲染占位。",
      },
      children: [
        {
          id: "example-1",
          kind: "content",
          title: "例题 1：光滑斜面下滑",
          type: "例题",
          structureType: "原子块",
          status: "可复用",
          version: "v1",
          versionStatus: "当前版本",
          tags: ["基础例题", "速度"],
          summary: "从高度差出发建立机械能守恒方程，得到末速度。",
          preview: {
            title: "例题 1",
            paragraphs: ["质量为 m 的滑块从光滑斜面高 h 处由静止释放，求到达底端时速度大小。"],
            equation: "mgh = 1/2 mv^2",
          },
        },
        {
          id: "example-2",
          kind: "content",
          title: "例题 2：弹簧最大压缩量",
          type: "例题",
          structureType: "原子块",
          status: "待审查",
          version: "v1",
          versionStatus: "待审查",
          tags: ["弹簧", "最大压缩"],
          summary: "把重力势能转化为弹性势能，强调最低点与最大压缩位置的差异。",
          preview: {
            title: "例题 2",
            paragraphs: ["小球从高度 h 处释放后压缩水平弹簧，忽略摩擦，求弹簧最大压缩量。"],
            equation: "mgh = 1/2 kx^2",
          },
        },
      ],
    },
    {
      id: "practice-energy",
      kind: "content",
      title: "机械能守恒三题练习",
      type: "练习",
      structureType: "原子块",
      status: "可复用",
      version: "v4",
      versionStatus: "当前版本",
      tags: ["课后练习", "基础巩固"],
      summary: "三道递进练习，覆盖高度差、弹簧压缩、动能变化。",
      preview: {
        title: "练习",
        paragraphs: [
          "1. 判断下列过程机械能是否守恒。",
          "2. 已知高度差求末速度。",
          "3. 含弹簧模型中求最大形变量。",
        ],
      },
    },
    {
      id: "model-entry",
      kind: "model",
      title: "下级模型入口",
      type: "模型",
      structureType: "入口卡片",
      status: "入口",
      version: "-",
      versionStatus: "静态入口",
      tags: ["竖直圆轨道", "杆模型", "球模型"],
      summary: "这些入口不是内容块正文，而是继续进入更细的小节模型。",
      models: [
        { id: "vertical-circle", title: "竖直圆轨道", summary: "最低点、最高点、临界速度" },
        { id: "rod-model", title: "杆模型", summary: "轻杆约束、端点速度关系" },
        { id: "ball-model", title: "球模型", summary: "轨道、小球、弹簧组合" },
      ],
      preview: {
        title: "下级模型入口",
        paragraphs: ["点击模型入口后，后续可以切换到对应小节。本轮只展示入口，不加载真实数据。"],
      },
    },
  ];

  const resourceCandidates = [
    {
      id: "candidate-circle",
      title: "竖直圆轨道临界条件",
      type: "知识点",
      structureType: "原子块",
      status: "可复用",
      version: "v2",
      tags: ["圆轨道", "临界速度"],
      summary: "整理最高点压力为零和杆模型临界条件的差异。",
    },
    {
      id: "candidate-spring",
      title: "弹簧能量转化例题",
      type: "例题",
      structureType: "原子块",
      status: "可复用",
      version: "v1",
      tags: ["弹簧", "守恒"],
      summary: "从释放点到最大压缩位置列能量方程。",
    },
    {
      id: "candidate-practice",
      title: "机械能守恒分层练习",
      type: "练习",
      structureType: "组合块",
      status: "待审查",
      version: "v1",
      tags: ["练习", "分层"],
      summary: "基础判断、公式代入、模型迁移三组题。",
    },
  ];

  const els = {
    sectionComposer: document.getElementById("sectionComposer"),
    teachingTopicNav: document.getElementById("teachingTopicNav"),
    toggleTopicNavButton: document.getElementById("toggleTopicNavButton"),
    sectionTree: document.getElementById("sectionTree"),
    sectionSummary: document.getElementById("sectionSummary"),
    cardWorkspace: document.getElementById("cardWorkspace"),
    selectedDetail: document.getElementById("selectedDetail"),
    previewSurface: document.getElementById("previewSurface"),
    actionMessage: document.getElementById("actionMessage"),
    resourceDrawer: document.getElementById("resourceDrawer"),
    closeResourceDrawerButton: document.getElementById("closeResourceDrawerButton"),
    resourceSearchInput: document.getElementById("resourceSearchInput"),
    resourceResults: document.getElementById("resourceResults"),
  };

  const state = {
    selectedSectionId: "mechanical-energy",
    selectedCardId: "knowledge-energy",
    collapsedCardIds: new Set(),
    collapsedTreeIds: new Set(),
    isTopicNavCollapsed: false,
    insertPosition: "end",
  };

  function renderTree() {
    els.sectionTree.innerHTML = renderTreeNode(sectionTree, 0);

    els.sectionTree.querySelectorAll("[data-section-id]").forEach((button) => {
      button.addEventListener("click", () => selectSection(button.dataset.sectionId));
    });
    els.sectionTree.querySelectorAll("[data-tree-toggle]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        toggleTreeNode(button.dataset.treeToggle);
      });
    });
  }

  function renderTreeNode(node, level) {
    const hasChildren = Boolean(node.children?.length);
    const collapsed = state.collapsedTreeIds.has(node.id);
    const isActive = state.selectedSectionId === node.id;

    return `
      <div class="teaching-tree-item" data-level="${level}">
        <div class="teaching-tree-row" style="--tree-level: ${level}">
          ${hasChildren ? `
            <button class="tree-fold-button" type="button" data-tree-toggle="${escapeHtml(node.id)}" aria-label="${collapsed ? "展开" : "收起"}${escapeHtml(node.title)}" aria-expanded="${String(!collapsed)}">
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="${collapsed ? "m9 18 6-6-6-6" : "m6 9 6 6 6-6"}"></path></svg>
            </button>
          ` : `<span class="tree-fold-spacer" aria-hidden="true"></span>`}
          <button class="tree-node${isActive ? " is-active" : ""}" type="button" data-section-id="${escapeHtml(node.id)}" title="${escapeHtml(node.title)}">
            <span class="tree-dot" aria-hidden="true"></span>
            <span class="tree-label">${escapeHtml(node.title)}</span>
          </button>
        </div>
        ${hasChildren && !collapsed ? `<div class="teaching-tree-children">${node.children.map((child) => renderTreeNode(child, level + 1)).join("")}</div>` : ""}
      </div>
    `;
  }

  function renderSectionSummary() {
    const section = findSection(state.selectedSectionId);
    els.sectionSummary.innerHTML = `
      <h3>${escapeHtml(section.title)}</h3>
      <p>${escapeHtml(section.summary || "本轮仅展示静态结构，不读取后端。")}</p>
      <div class="summary-metrics">
        <span>知识点 1</span>
        <span>例题组 1</span>
        <span>例题 2</span>
        <span>练习 1</span>
        <span>模型入口 3</span>
      </div>
    `;
  }

  function renderWorkspace() {
    const html = [];
    workspaceCards.forEach((card, index) => {
      html.push(renderInsertZone(`before-${card.id}`, index === 0 ? "添加到最前" : "插入到此处"));
      html.push(renderWorkspaceCard(card, 0));
    });
    html.push(renderInsertZone("end", "添加到末尾"));

    els.cardWorkspace.innerHTML = html.join("");
    els.cardWorkspace.querySelectorAll("[data-card-id]").forEach((node) => {
      node.addEventListener("click", (event) => {
        event.stopPropagation();
        selectCard(node.dataset.cardId);
      });
      node.addEventListener("keydown", (event) => {
        if (event.key === "Enter" || event.key === " ") {
          event.preventDefault();
          selectCard(node.dataset.cardId);
        }
      });
    });
    els.cardWorkspace.querySelectorAll("[data-collapse-id]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        toggleCollapsed(button.dataset.collapseId);
      });
    });
    els.cardWorkspace.querySelectorAll("[data-insert-action]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        openResourceSearch(button.dataset.insertPosition, button.dataset.insertAction);
      });
    });
    bindPlaceholderButtons(els.cardWorkspace);
  }

  function renderWorkspaceCard(card, depth) {
    const hasChildren = Boolean(card.children?.length);
    if (hasChildren) {
      const collapsed = state.collapsedCardIds.has(card.id);
      return `
        <section class="section-card-group" data-card-group-id="${escapeHtml(card.id)}">
          ${renderSectionContentCard(card, depth, hasChildren)}
          <div class="section-card-group__children${collapsed ? " is-collapsed" : ""}">
            ${card.children.map((child, index) => `
              ${renderInsertZone(`inside-${card.id}-${index}`, "添加子卡片")}
              ${renderWorkspaceCard(child, depth + 1)}
            `).join("")}
            ${renderInsertZone(`inside-${card.id}-end`, "添加子卡片")}
          </div>
        </section>
      `;
    }

    return renderSectionContentCard(card, depth, false);
  }

  function renderSectionContentCard(card, depth, hasChildren) {
    const selected = card.id === state.selectedCardId;
    const collapsed = state.collapsedCardIds.has(card.id);
    const typeClass = getTypeClass(card.type);
    const structureClass = getStructureClass(card.structureType);

    return `
      <article class="section-content-card${selected ? " is-selected" : ""}" data-depth="${depth}">
        <div class="section-content-card__fold">
          ${hasChildren ? `
            <button class="fold-button" type="button" data-collapse-id="${escapeHtml(card.id)}" aria-label="${collapsed ? "展开" : "收起"}${escapeHtml(card.title)}" aria-expanded="${String(!collapsed)}">
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="${collapsed ? "m9 18 6-6-6-6" : "m6 9 6 6 6-6"}"></path></svg>
            </button>
          ` : ""}
        </div>
        <div class="section-content-card__body" role="button" tabindex="0" data-card-id="${escapeHtml(card.id)}">
          <div class="section-content-card__header">
            <strong class="section-content-card__title">${escapeHtml(card.title)}</strong>
            <span class="section-content-card__badges">
              <span class="section-content-card__type section-content-card__type--${typeClass}">${escapeHtml(card.type)}</span>
              <span class="section-content-card__structure section-content-card__structure--${structureClass}">${escapeHtml(card.structureType)}</span>
            </span>
          </div>
          <div class="section-content-card__html">
            ${renderCardHtml(card)}
          </div>
        </div>
        ${renderCardActions(card)}
      </article>
    `;
  }

  function renderCardHtml(card) {
    const preview = card.preview || {};
    const paragraphs = preview.paragraphs || [card.summary || "静态正文占位。"];
    const modelHtml = card.models?.length ? `
      <div class="section-content-card__models">
        ${card.models.map((model) => `
          <span>
            <strong>${escapeHtml(model.title)}</strong>
            <small>${escapeHtml(model.summary)}</small>
          </span>
        `).join("")}
      </div>
    ` : "";

    return `
      ${preview.title ? `<h4>${escapeHtml(preview.title)}</h4>` : ""}
      ${paragraphs.map((paragraph) => `<p>${escapeHtml(paragraph)}</p>`).join("")}
      ${preview.equation ? `<div class="section-content-card__equation">${escapeHtml(preview.equation)}</div>` : ""}
      ${preview.callout ? `<div class="section-content-card__callout">${escapeHtml(preview.callout)}</div>` : ""}
      ${modelHtml}
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

  function renderCardActions(card) {
    const isGroup = card.structureType === "组合块" || card.kind === "model";
    return `
      <div class="section-content-card__actions" aria-label="${escapeHtml(card.title)} 操作">
        <button class="section-action-button" type="button" data-placeholder="Word 编辑" title="Word 编辑" aria-label="Word 编辑">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 4h16v16H4z"></path><path d="M8 8h8M8 12h8M8 16h5"></path></svg>
        </button>
        <button class="section-action-button" type="button" data-placeholder="添加下级内容" title="添加下级内容" aria-label="添加下级内容" ${isGroup ? "" : "disabled"}>
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
        </button>
        <button class="section-action-button" type="button" data-placeholder="从资源库插入" title="从资源库插入" aria-label="从资源库插入">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M21 21l-4.3-4.3"></path><circle cx="11" cy="11" r="7"></circle></svg>
        </button>
        <button class="section-action-button" type="button" data-placeholder="上移" title="上移" aria-label="上移">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m18 15-6-6-6 6"></path></svg>
        </button>
        <button class="section-action-button" type="button" data-placeholder="下移" title="下移" aria-label="下移">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m6 9 6 6 6-6"></path></svg>
        </button>
        <button class="section-action-button is-danger" type="button" data-placeholder="移除" title="移除" aria-label="移除">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M18 6 6 18"></path><path d="m6 6 12 12"></path></svg>
        </button>
      </div>
    `;
  }

  function renderDetail() {
    const card = findCard(state.selectedCardId) || workspaceCards[0];
    els.selectedDetail.innerHTML = `
      <h3 class="selected-title">${escapeHtml(card.title)}</h3>
    `;

    const preview = card.preview || {};
    els.previewSurface.innerHTML = `
      <div class="preview-document">
        <h4>${escapeHtml(preview.title || card.title)}</h4>
        ${(preview.paragraphs || [card.summary || "静态预览占位"]).map((paragraph) => `<p>${escapeHtml(paragraph)}</p>`).join("")}
        ${preview.equation ? `<div class="preview-equation">${escapeHtml(preview.equation)}</div>` : ""}
        ${preview.callout ? `<div class="preview-callout">${escapeHtml(preview.callout)}</div>` : ""}
      </div>
    `;
  }

  function renderResourceResults() {
    const keyword = els.resourceSearchInput.value.trim().toLowerCase();
    const results = resourceCandidates.filter((item) => {
      const text = `${item.title} ${item.type} ${item.structureType} ${item.tags.join(" ")} ${item.summary}`.toLowerCase();
      return !keyword || text.includes(keyword);
    });

    els.resourceResults.innerHTML = results.length ? results.map((item) => `
      <button class="resource-result-card" type="button" data-resource-id="${escapeHtml(item.id)}">
        <span class="resource-result-card__title">${escapeHtml(item.title)}</span>
        <span class="resource-result-card__meta">${escapeHtml(item.type)} · ${escapeHtml(item.structureType)} · ${escapeHtml(item.status)} · ${escapeHtml(item.version)}</span>
        <span class="resource-result-card__summary">${escapeHtml(item.summary)}</span>
      </button>
    `).join("") : `<p class="resource-empty">没有匹配的静态候选。</p>`;

    els.resourceResults.querySelectorAll("[data-resource-id]").forEach((button) => {
      button.addEventListener("click", () => {
        const item = resourceCandidates.find((candidate) => candidate.id === button.dataset.resourceId);
        els.actionMessage.textContent = `已选择静态候选：${item?.title || "未知内容"}。插入位置：${state.insertPosition}。本轮不写入小节。`;
        els.resourceDrawer.classList.add("is-hidden");
      });
    });
  }

  function openResourceSearch(position = "end", action = "insert") {
    state.insertPosition = position;
    els.resourceDrawer.classList.remove("is-hidden");
    renderResourceResults();
    const actionText = action === "create" ? "新建卡片" : "插入卡片";
    els.actionMessage.textContent = `已打开静态搜索框：${actionText}，目标位置：${position}。`;
    window.setTimeout(() => els.resourceSearchInput.focus(), 0);
  }

  function selectSection(sectionId) {
    state.selectedSectionId = sectionId;
    if (sectionId !== "mechanical-energy") {
      state.selectedCardId = "model-entry";
    }
    renderTree();
    renderSectionSummary();
    renderWorkspace();
    renderDetail();
    els.actionMessage.textContent = sectionId === "mechanical-energy"
      ? "已选中机械能守恒。中间展示该小节的静态层级卡片。"
      : "已切换到下级模型入口。本轮不加载真实小节，只保留线稿关系。";
  }

  function selectCard(cardId) {
    state.selectedCardId = cardId;
    renderWorkspace();
    renderDetail();
    const card = findCard(cardId);
    els.actionMessage.textContent = `已选中：${card?.title || "未知卡片"}。右侧详情和预览已切换。`;
  }

  function toggleCollapsed(cardId) {
    if (state.collapsedCardIds.has(cardId)) {
      state.collapsedCardIds.delete(cardId);
    } else {
      state.collapsedCardIds.add(cardId);
    }
    renderWorkspace();
  }

  function toggleTreeNode(nodeId) {
    if (state.collapsedTreeIds.has(nodeId)) {
      state.collapsedTreeIds.delete(nodeId);
    } else {
      state.collapsedTreeIds.add(nodeId);
    }
    renderTree();
  }

  function toggleTopicNav() {
    state.isTopicNavCollapsed = !state.isTopicNavCollapsed;
    els.sectionComposer.classList.toggle("is-nav-collapsed", state.isTopicNavCollapsed);
    els.teachingTopicNav.classList.toggle("is-collapsed", state.isTopicNavCollapsed);
    els.toggleTopicNavButton.setAttribute("aria-expanded", String(!state.isTopicNavCollapsed));
    els.toggleTopicNavButton.setAttribute("aria-label", state.isTopicNavCollapsed ? "展开教学主题导航" : "收起教学主题导航");
    els.toggleTopicNavButton.innerHTML = `
      <svg viewBox="0 0 24 24" aria-hidden="true"><path d="${state.isTopicNavCollapsed ? "m9 18 6-6-6-6" : "m15 18-6-6 6-6"}"></path></svg>
    `;
  }

  function findSection(sectionId) {
    if (sectionId === sectionTree.id) return sectionTree;
    const mechanical = sectionTree.children[0];
    if (sectionId === mechanical.id) return mechanical;
    return mechanical.children.find((child) => child.id === sectionId) || mechanical;
  }

  function findCard(cardId, cards = workspaceCards) {
    for (const card of cards) {
      if (card.id === cardId) return card;
      const child = findCard(cardId, card.children || []);
      if (child) return child;
    }
    return null;
  }

  function bindPlaceholderButtons(root = document) {
    if (!root) return;
    root.querySelectorAll("[data-placeholder]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        const action = button.dataset.placeholder;
        if (action === "从资源库插入" || action === "添加下级内容") {
          openResourceSearch(action);
          return;
        }
        els.actionMessage.textContent = `${action} 是静态占位，本轮不调用真实逻辑。`;
      });
    });
  }

  function getTypeClass(type) {
    return {
      知识点: "knowledge",
      例题: "example",
      练习: "exercise",
      例题组: "group",
      模型: "method",
    }[type] || "default";
  }

  function getStructureClass(structureType) {
    return structureType === "组合块" ? "composite" : structureType === "入口卡片" ? "entry" : "atomic";
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;");
  }

  function init() {
    renderTree();
    renderSectionSummary();
    renderWorkspace();
    renderDetail();
    renderResourceResults();
    bindPlaceholderButtons(document.querySelector(".composition-panel > .panel-heading"));
    bindPlaceholderButtons(document.querySelector(".detail-panel"));
    els.toggleTopicNavButton.addEventListener("click", toggleTopicNav);
    els.resourceSearchInput.addEventListener("input", renderResourceResults);
    els.closeResourceDrawerButton.addEventListener("click", () => {
      els.resourceDrawer.classList.add("is-hidden");
    });
    els.resourceDrawer.addEventListener("click", (event) => {
      if (event.target === els.resourceDrawer) {
        els.resourceDrawer.classList.add("is-hidden");
      }
    });
  }

  init();
})();
