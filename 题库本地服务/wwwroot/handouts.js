(function () {
  const bankKey = "TEST";
  const apiBase = `/api/题库实例/${encodeURIComponent(bankKey)}`;
  const handoutRoot = `${apiBase}/讲义`;
  const sectionRoot = `${apiBase}/小节`;
  const contentRoot = `${apiBase}/内容块`;
  const finalSessionStates = new Set(["已同步", "无变化", "失败", "已取消"]);

  const state = {
    handouts: [],
    selectedId: null,
    selectedHandout: null,
    treeRoot: null,
    selectedNodeId: null,
    selectedNode: null,
    generations: [],
    candidates: [],
    pickerContext: null,
    activeSession: null,
    pollTimer: null,
  };

  const els = {
    keywordInput: document.getElementById("keywordInput"),
    statusSelect: document.getElementById("statusSelect"),
    refreshButton: document.getElementById("refreshButton"),
    searchButton: document.getElementById("searchButton"),
    newTitleInput: document.getElementById("newTitleInput"),
    createHandoutButton: document.getElementById("createHandoutButton"),
    handoutList: document.getElementById("handoutList"),
    handoutCountText: document.getElementById("handoutCountText"),
    globalStatus: document.getElementById("globalStatus"),
    detailEyebrow: document.getElementById("detailEyebrow"),
    detailTitle: document.getElementById("detailTitle"),
    openPickerButton: document.getElementById("openPickerButton"),
    generateButton: document.getElementById("generateButton"),
    saveHandoutButton: document.getElementById("saveHandoutButton"),
    editTitleInput: document.getElementById("editTitleInput"),
    editStatusSelect: document.getElementById("editStatusSelect"),
    editSummaryInput: document.getElementById("editSummaryInput"),
    metadataMessageText: document.getElementById("metadataMessageText"),
    generationCountText: document.getElementById("generationCountText"),
    generationList: document.getElementById("generationList"),
    refreshTreeButton: document.getElementById("refreshTreeButton"),
    arrangementTreeMessageText: document.getElementById("arrangementTreeMessageText"),
    arrangementTree: document.getElementById("arrangementTree"),
    contextMessageText: document.getElementById("contextMessageText"),
    contextSummary: document.getElementById("contextSummary"),
    editContextBlockButton: document.getElementById("editContextBlockButton"),
    reloadContextPreviewButton: document.getElementById("reloadContextPreviewButton"),
    contextSessionStatusText: document.getElementById("contextSessionStatusText"),
    contextSessionVersionText: document.getElementById("contextSessionVersionText"),
    contextSessionIdText: document.getElementById("contextSessionIdText"),
    contextPreviewFrame: document.getElementById("contextPreviewFrame"),
    contextEmptyPreview: document.getElementById("contextEmptyPreview"),
    pickerBackdrop: document.getElementById("pickerBackdrop"),
    pickerTitle: document.getElementById("pickerTitle"),
    closePickerButton: document.getElementById("closePickerButton"),
    pickerTargetTypeSelect: document.getElementById("pickerTargetTypeSelect"),
    pickerSearchInput: document.getElementById("pickerSearchInput"),
    pickerRoleField: document.getElementById("pickerRoleField"),
    pickerRoleInput: document.getElementById("pickerRoleInput"),
    referenceModeField: document.getElementById("referenceModeField"),
    referenceModeSelect: document.getElementById("referenceModeSelect"),
    candidateList: document.getElementById("candidateList"),
  };

  function idOf(value) {
    return value?.id ?? value?.Id ?? value?.ID;
  }

  function text(value, fallback = "-") {
    return value === null || value === undefined || value === "" ? fallback : String(value);
  }

  function setGlobalStatus(message) {
    els.globalStatus.textContent = message;
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

  function buildHandoutQuery() {
    const params = new URLSearchParams();
    const keyword = els.keywordInput.value.trim();
    if (keyword) params.set("关键词", keyword);
    if (els.statusSelect.value) params.set("状态", els.statusSelect.value);
    const query = params.toString();
    return query ? `?${query}` : "";
  }

  async function loadHandouts() {
    setGlobalStatus("加载中");
    try {
      const handouts = await requestJson(`${handoutRoot}${buildHandoutQuery()}`, { method: "GET" });
      state.handouts = Array.isArray(handouts) ? handouts : [];
      renderHandoutList();
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("加载失败");
      alert(error.message);
    }
  }

  function renderHandoutList() {
    els.handoutList.innerHTML = "";
    els.handoutCountText.textContent = `${state.handouts.length} 条`;

    if (state.handouts.length === 0) {
      els.handoutList.innerHTML = "<div class=\"empty-state\">没有匹配的讲义</div>";
      return;
    }

    els.handoutList.innerHTML = state.handouts.map((handout) => {
      const handoutId = idOf(handout);
      return `
        <button class="handout-item${handoutId === state.selectedId ? " is-active" : ""}" type="button" data-handout-id="${handoutId}">
          <div class="handout-title-row">
            <strong title="${escapeHtml(handout.标题)}">${escapeHtml(handout.标题)}</strong>
            <span class="badge">${escapeHtml(handout.状态)}</span>
          </div>
          <div class="handout-meta">
            <span>${Number(handout.项目数量 || 0)} 项</span>
            <span>${handout.最新生成时间 ? "已生成" : "未生成"}</span>
            <span>${formatDate(handout.更新时间)}</span>
          </div>
        </button>
      `;
    }).join("");

    els.handoutList.querySelectorAll("[data-handout-id]").forEach((button) => {
      button.addEventListener("click", () => selectHandout(Number(button.dataset.handoutId)));
    });
  }

  async function selectHandout(id) {
    state.selectedId = id;
    state.selectedHandout = null;
    state.treeRoot = null;
    state.selectedNodeId = null;
    state.selectedNode = null;
    state.generations = [];
    state.activeSession = null;
    clearPoll();
    renderHandoutList();
    renderArrangementTree();
    renderNodeDetail(null);
    setContextSession(null);
    setDetailDisabled(true);
    setGlobalStatus("读取讲义");

    try {
      const handout = await requestJson(`${handoutRoot}/${id}`, { method: "GET" });
      state.selectedHandout = handout;
      renderHandoutDetail(handout);
      await Promise.allSettled([loadGenerations(), loadArrangementTree()]);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("读取失败");
      alert(error.message);
    }
  }

  function renderHandoutDetail(handout) {
    els.detailEyebrow.textContent = `ID ${idOf(handout)}`;
    els.detailTitle.textContent = handout.标题;
    els.editTitleInput.value = handout.标题 || "";
    els.editSummaryInput.value = handout.摘要 || "";
    els.editStatusSelect.value = handout.状态 || "草稿";
    els.metadataMessageText.textContent = "讲义元数据不会复制正文，只保存编排关系。";
    setDetailDisabled(false);
  }

  function setDetailDisabled(disabled) {
    [
      els.openPickerButton,
      els.generateButton,
      els.saveHandoutButton,
      els.refreshTreeButton,
      els.editTitleInput,
      els.editStatusSelect,
      els.editSummaryInput,
    ].forEach((element) => {
      element.disabled = disabled;
    });

    if (disabled) {
      els.detailEyebrow.textContent = "未选择";
      els.detailTitle.textContent = "选择一个讲义";
      els.editTitleInput.value = "";
      els.editSummaryInput.value = "";
      els.editStatusSelect.value = "草稿";
      els.arrangementTree.innerHTML = "<div class=\"empty-state\">选择讲义后显示编排树</div>";
      els.arrangementTreeMessageText.textContent = "选择讲义后显示完整编排层级。";
      els.generationList.innerHTML = "<div class=\"empty-state\">暂无生成记录</div>";
      els.generationCountText.textContent = "0 条";
    }
  }

  async function createHandout() {
    const title = els.newTitleInput.value.trim();
    if (!title) {
      alert("请先输入讲义标题。");
      return;
    }

    try {
      const created = await requestJson(handoutRoot, {
        method: "POST",
        body: JSON.stringify({
          标题: title,
          状态: "草稿",
        }),
      });
      els.newTitleInput.value = "";
      await loadHandouts();
      await selectHandout(idOf(created));
    } catch (error) {
      alert(error.message);
    }
  }

  async function saveHandout() {
    if (!state.selectedId) return;
    try {
      const updated = await requestJson(`${handoutRoot}/${state.selectedId}`, {
        method: "PUT",
        body: JSON.stringify({
          标题: els.editTitleInput.value.trim(),
          摘要: els.editSummaryInput.value.trim() || null,
          状态: els.editStatusSelect.value,
        }),
      });
      state.selectedHandout = updated;
      renderHandoutDetail(updated);
      await loadHandouts();
      renderHandoutList();
      await loadArrangementTree(state.selectedNodeId);
      els.metadataMessageText.textContent = "已保存。";
    } catch (error) {
      alert(error.message);
    }
  }

  async function loadArrangementTree(preferredNodeId = state.selectedNodeId) {
    if (!state.selectedId) return;

    els.arrangementTreeMessageText.textContent = "正在读取编排树。";
    els.arrangementTree.innerHTML = "<div class=\"empty-state\">正在读取编排树</div>";
    try {
      state.treeRoot = await requestJson(`${handoutRoot}/${state.selectedId}/结构树`, { method: "GET" });
      const nextNode = findNodeById(state.treeRoot, preferredNodeId)
        || state.treeRoot
        || null;
      state.selectedNodeId = nextNode?.节点ID || null;
      state.selectedNode = nextNode;
      renderArrangementTree();
      renderNodeDetail(nextNode);
      els.arrangementTreeMessageText.textContent = `${countTreeNodes(state.treeRoot)} 个节点，树上可直接编排。`;
    } catch (error) {
      state.treeRoot = null;
      els.arrangementTreeMessageText.textContent = error.message;
      els.arrangementTree.innerHTML = `<div class="empty-state">${escapeHtml(error.message)}</div>`;
      if (state.selectedHandout) {
        renderHandoutOnlyNodeDetail();
      } else {
        renderNodeDetail(null);
      }
    }
  }

  function renderHandoutOnlyNodeDetail() {
    const handout = state.selectedHandout;
    if (!handout) return;
    renderNodeDetail({
      节点ID: `讲义-${idOf(handout)}`,
      节点类型: "讲义",
      目标ID: idOf(handout),
      标题: handout.标题,
      摘要: handout.摘要,
      状态: handout.状态,
      子节点数量: Number(handout.项目数量 || 0),
      子节点列表: [],
    });
  }

  function renderArrangementTree() {
    if (!state.selectedId) {
      els.arrangementTree.innerHTML = "<div class=\"empty-state\">选择讲义后显示编排树</div>";
      return;
    }

    if (!state.treeRoot) {
      els.arrangementTree.innerHTML = "<div class=\"empty-state\">暂无编排树</div>";
      return;
    }

    els.arrangementTree.innerHTML = window.ContentTree.render(mapTreeNode(state.treeRoot), {
      selectedId: state.selectedNodeId,
      emptyHtml: "<div class=\"empty-state\">暂无编排树</div>",
    });
    window.ContentTree.bind(els.arrangementTree, {
      onSelect: (dataset) => selectTreeNode(dataset.treeNodeId || dataset.nodeId),
      onAction: handleTreeAction,
    });
  }

  function mapTreeNode(node) {
    const nodeId = node.节点ID;
    return {
      id: nodeId,
      title: node.标题 || "未命名",
      badge: node.节点类型,
      depth: Number(node.深度 || 0) + 1,
      selected: state.selectedNodeId === nodeId,
      selectable: true,
      disabled: !!node.是否错误,
      meta: buildNodeMeta(node),
      actions: buildTreeActions(node),
      data: {
        nodeId,
        nodeType: node.节点类型,
        targetId: node.目标ID,
        sourceType: node.来源类型,
        sourceId: node.来源ID,
        parentTargetId: node.父目标ID,
      },
      children: (node.子节点列表 || []).map(mapTreeNode),
    };
  }

  function buildNodeMeta(node) {
    if (node.是否错误) {
      return [node.错误信息 || "引用异常"];
    }

    if (node.节点类型 === "讲义") {
      return [
        text(node.状态, "草稿"),
        `${Number(node.子节点数量 || 0)} 个顶层项目`,
      ];
    }

    if (node.节点类型 === "小节") {
      return [
        text(node.状态, "草稿"),
        node.章节名称 || (node.章节标签ID ? `章节标签 ${node.章节标签ID}` : "未挂章节"),
        `${Number(node.子节点数量 || 0)} 个内容块`,
        `排序 ${Number(node.排序 || 0) + 1}`,
      ];
    }

    return [
      text(node.角色, "内容块"),
      text(node.内容类型, "内容块"),
      text(node.结构类型, "原子块"),
      `${text(node.引用版本模式, "跟随最新")} · v${text(node.引用版本号, "0")}`,
      `排序 ${Number(node.排序 || 0) + 1}`,
    ];
  }

  function buildTreeActions(node) {
    if (node.节点类型 === "讲义") {
      return [
        { key: "add-section", label: "加小节" },
        { key: "add-content", label: "加内容块" },
      ];
    }

    if (node.是否错误) {
      return [{ key: "remove", label: "移除" }];
    }

    if (node.节点类型 === "小节") {
      return [
        { key: "up", label: "上移" },
        { key: "down", label: "下移" },
        { key: "add-content", label: "加内容块" },
        { key: "edit-section", label: "编辑" },
        { key: "remove", label: "移除" },
      ];
    }

    const actions = [
      { key: "up", label: "上移" },
      { key: "down", label: "下移" },
      { key: "edit-word", label: "Word" },
    ];
    if (node.是否允许子块) {
      actions.push({ key: "add-child-content", label: "加子块" });
    }
    actions.push({ key: "remove", label: "移除" });
    return actions;
  }

  function selectTreeNode(nodeId) {
    const node = findNodeById(state.treeRoot, nodeId);
    if (!node) return;

    state.selectedNodeId = node.节点ID;
    state.selectedNode = node;
    renderArrangementTree();
    renderNodeDetail(node);
  }

  async function handleTreeAction(action, dataset) {
    selectTreeNode(dataset.nodeId || dataset.treeNodeId);
    const node = state.selectedNode;
    if (!node) return;

    switch (action) {
      case "add-section":
        openPicker({ mode: "handout", fixedTargetType: null, parentNode: node });
        break;
      case "add-content":
        openPicker({
          mode: node.节点类型 === "小节" ? "section" : "handout",
          fixedTargetType: node.节点类型 === "小节" ? "内容块" : null,
          parentNode: node,
        });
        break;
      case "add-child-content":
        openPicker({ mode: "content-child", fixedTargetType: "内容块", parentNode: node });
        break;
      case "edit-section":
        focusSectionEditor();
        break;
      case "edit-word":
        await editSelectedContentInWord();
        break;
      case "up":
        await moveSelectedNode(-1);
        break;
      case "down":
        await moveSelectedNode(1);
        break;
      case "remove":
        await removeSelectedNode();
        break;
      default:
        break;
    }
  }

  function renderNodeDetail(node) {
    state.selectedNode = node;
    hidePreview("选择树节点后显示预览");
    els.editContextBlockButton.style.display = "none";
    els.reloadContextPreviewButton.style.display = "none";
    els.editContextBlockButton.disabled = true;
    els.reloadContextPreviewButton.disabled = true;

    if (!node) {
      els.contextMessageText.textContent = "选择树节点后显示可执行操作。";
      els.contextSummary.innerHTML = "<div class=\"empty-state\">选择一个讲义节点</div>";
      return;
    }

    if (node.节点类型 === "讲义") {
      renderRootDetail(node);
      return;
    }

    if (node.节点类型 === "小节") {
      renderSectionDetailPanel(node);
      loadNodePreview(node);
      return;
    }

    renderContentDetailPanel(node);
    loadNodePreview(node);
  }

  function renderRootDetail(node) {
    els.contextMessageText.textContent = "讲义根节点用于添加顶层小节或内容块。";
    els.contextSummary.innerHTML = `
      <strong title="${escapeHtml(node.标题)}">${escapeHtml(node.标题)}</strong>
      <p>${escapeHtml(node.摘要 || "暂无摘要")}</p>
      <div class="context-meta">
        <span>${escapeHtml(node.状态 || "草稿")}</span>
        <span>${Number(node.子节点数量 || 0)} 个顶层项目</span>
      </div>
      <div class="detail-action-row">
        <button class="secondary-button" type="button" data-detail-action="add-section">添加小节</button>
        <button class="secondary-button" type="button" data-detail-action="add-content">添加内容块</button>
      </div>
    `;
    bindDetailActions();
  }

  function renderSectionDetailPanel(node) {
    els.contextMessageText.textContent = "小节在网页中编辑；小节内部内容块从树上添加、移动或移除。";
    els.contextSummary.innerHTML = `
      <div class="node-editor">
        <label class="field">
          <span>小节标题</span>
          <input id="nodeSectionTitleInput" type="text" value="${escapeHtml(node.标题 || "")}">
        </label>
        <label class="field">
          <span>状态</span>
          <select id="nodeSectionStatusSelect">
            <option value="草稿" ${node.状态 === "草稿" ? "selected" : ""}>草稿</option>
            <option value="待审查" ${node.状态 === "待审查" ? "selected" : ""}>待审查</option>
            <option value="可复用" ${node.状态 === "可复用" ? "selected" : ""}>可复用</option>
            <option value="需修订" ${node.状态 === "需修订" ? "selected" : ""}>需修订</option>
            <option value="已废弃" ${node.状态 === "已废弃" ? "selected" : ""}>已废弃</option>
          </select>
        </label>
        <label class="field">
          <span>摘要</span>
          <textarea id="nodeSectionSummaryInput">${escapeHtml(node.摘要 || "")}</textarea>
        </label>
        <div class="context-meta">
          <span>ID ${escapeHtml(node.目标ID)}</span>
          <span>${escapeHtml(node.章节名称 || (node.章节标签ID ? `章节标签 ${node.章节标签ID}` : "未挂章节"))}</span>
          <span>${Number(node.子节点数量 || 0)} 个内容块</span>
        </div>
        <div class="detail-action-row">
          <button class="primary-button" type="button" data-detail-action="save-section">保存小节</button>
          <button class="secondary-button" type="button" data-detail-action="add-content">添加内容块</button>
        </div>
      </div>
    `;
    bindDetailActions();
  }

  function renderContentDetailPanel(node) {
    els.contextMessageText.textContent = "这里编辑的是源内容块，所有引用它的位置会按版本规则更新。";
    els.editContextBlockButton.style.display = "inline-flex";
    els.reloadContextPreviewButton.style.display = "inline-flex";
    els.editContextBlockButton.disabled = !!node.是否错误;
    els.reloadContextPreviewButton.disabled = !!node.是否错误 || !node.当前版本ID;

    els.contextSummary.innerHTML = `
      <strong title="${escapeHtml(node.标题)}">${escapeHtml(node.标题)}</strong>
      <p>${escapeHtml(node.摘要 || "暂无摘要")}</p>
      <div class="context-meta">
        <span>ID ${escapeHtml(node.目标ID)}</span>
        <span>${escapeHtml(node.内容类型 || "内容块")}</span>
        <span>${escapeHtml(node.状态 || "草稿")}</span>
        <span>${escapeHtml(node.结构类型 || "原子块")}</span>
        <span>${node.当前版本号 ? `当前 v${escapeHtml(node.当前版本号)}` : "无当前版本"}</span>
        <span>${escapeHtml(text(node.引用版本模式, "跟随最新"))} · v${escapeHtml(text(node.引用版本号, "0"))}</span>
      </div>
      ${node.是否允许子块 ? `
        <div class="detail-action-row">
          <button class="secondary-button" type="button" data-detail-action="add-child-content">添加子内容块</button>
        </div>
      ` : ""}
    `;
    bindDetailActions();
  }

  function bindDetailActions() {
    els.contextSummary.querySelectorAll("[data-detail-action]").forEach((button) => {
      button.addEventListener("click", async () => {
        const action = button.dataset.detailAction;
        if (action === "add-section") openPicker({ mode: "handout", fixedTargetType: "小节", parentNode: state.selectedNode });
        if (action === "add-content") {
          openPicker({
            mode: state.selectedNode?.节点类型 === "小节" ? "section" : "handout",
            fixedTargetType: state.selectedNode?.节点类型 === "小节" ? "内容块" : "内容块",
            parentNode: state.selectedNode,
          });
        }
        if (action === "add-child-content") openPicker({ mode: "content-child", fixedTargetType: "内容块", parentNode: state.selectedNode });
        if (action === "save-section") await saveSelectedSection();
      });
    });
  }

  function focusSectionEditor() {
    const input = document.getElementById("nodeSectionTitleInput");
    if (input) {
      input.focus();
      input.select();
    }
  }

  async function saveSelectedSection() {
    const node = state.selectedNode;
    if (!node || node.节点类型 !== "小节") return;

    const title = document.getElementById("nodeSectionTitleInput")?.value.trim();
    if (!title) return;

    setGlobalStatus("保存小节");
    try {
      await requestJson(`${sectionRoot}/${node.目标ID}`, {
        method: "PUT",
        body: JSON.stringify({
          标题: title,
          摘要: document.getElementById("nodeSectionSummaryInput")?.value.trim() || null,
          章节标签ID: node.章节标签ID || null,
          状态: document.getElementById("nodeSectionStatusSelect")?.value || "草稿",
        }),
      });
      await loadArrangementTree(node.节点ID);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("保存失败");
      alert(error.message);
    }
  }

  function loadNodePreview(node) {
    if (!node || node.是否错误) {
      hidePreview("当前节点无法预览");
      return;
    }

    const url = node.节点类型 === "小节"
      ? `${sectionRoot}/${node.目标ID}/预览html?t=${Date.now()}`
      : `${contentRoot}/${node.目标ID}/预览html?t=${Date.now()}`;

    els.reloadContextPreviewButton.style.display = node.节点类型 === "小节" ? "inline-flex" : els.reloadContextPreviewButton.style.display;
    els.reloadContextPreviewButton.disabled = false;
    els.contextEmptyPreview.classList.add("is-hidden");
    els.contextPreviewFrame.src = url;
  }

  function hidePreview(message) {
    els.contextPreviewFrame.removeAttribute("src");
    els.contextEmptyPreview.textContent = message;
    els.contextEmptyPreview.classList.remove("is-hidden");
  }

  async function moveSelectedNode(direction) {
    const node = state.selectedNode;
    if (!node || !node.来源类型 || !node.来源ID) return;

    const parent = findParentNode(state.treeRoot, node.节点ID);
    const siblings = parent?.子节点列表 || [];
    const index = siblings.findIndex((item) => item.节点ID === node.节点ID);
    const nextIndex = index + direction;
    if (index < 0 || nextIndex < 0 || nextIndex >= siblings.length) return;

    const nextSiblings = [...siblings];
    [nextSiblings[index], nextSiblings[nextIndex]] = [nextSiblings[nextIndex], nextSiblings[index]];

    setGlobalStatus("调整排序");
    try {
      if (node.来源类型 === "讲义项") {
        await requestJson(`${handoutRoot}/${state.selectedId}/项目排序`, {
          method: "PUT",
          body: JSON.stringify({ 讲义项ID列表: nextSiblings.map((item) => Number(item.来源ID)) }),
        });
      } else if (node.来源类型 === "小节项") {
        await requestJson(`${sectionRoot}/${node.父目标ID}/项目排序`, {
          method: "PUT",
          body: JSON.stringify({
            项目排序列表: nextSiblings.map((item, order) => ({
              小节项ID: Number(item.来源ID),
              排序: order,
            })),
          }),
        });
      } else if (node.来源类型 === "内容块子项") {
        await requestJson(`${contentRoot}/${node.父目标ID}/子块排序`, {
          method: "PUT",
          body: JSON.stringify({
            子项排序列表: nextSiblings.map((item, order) => ({
              子项ID: Number(item.来源ID),
              排序: order,
            })),
          }),
        });
      }

      await loadArrangementTree(node.节点ID);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("排序失败");
      alert(error.message);
    }
  }

  async function removeSelectedNode() {
    const node = state.selectedNode;
    if (!node || !node.来源类型 || !node.来源ID) return;
    if (!window.confirm(`从当前编排中移除“${node.标题}”？源内容不会被删除。`)) return;

    setGlobalStatus("移除引用");
    try {
      if (node.来源类型 === "讲义项") {
        await requestJson(`${handoutRoot}/${state.selectedId}/项目/${node.来源ID}`, { method: "DELETE" });
      } else if (node.来源类型 === "小节项") {
        await requestJson(`${sectionRoot}/${node.父目标ID}/项目/${node.来源ID}`, { method: "DELETE" });
      } else if (node.来源类型 === "内容块子项") {
        await requestJson(`${contentRoot}/${node.父目标ID}/子块/${node.来源ID}`, { method: "DELETE" });
      }

      const nextSelectedId = findParentNode(state.treeRoot, node.节点ID)?.节点ID || null;
      await loadArrangementTree(nextSelectedId);
      await loadHandouts();
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("移除失败");
      alert(error.message);
    }
  }

  async function loadGenerations() {
    if (!state.selectedId) return;
    state.generations = await requestJson(`${handoutRoot}/${state.selectedId}/生成记录`, { method: "GET" });
    renderGenerations();
  }

  function renderGenerations() {
    els.generationCountText.textContent = `${state.generations.length} 条`;
    if (state.generations.length === 0) {
      els.generationList.innerHTML = "<div class=\"empty-state\">暂无生成记录</div>";
      return;
    }

    els.generationList.innerHTML = state.generations.map((record) => {
      const recordId = idOf(record);
      const href = `${handoutRoot}/${state.selectedId}/生成记录/${recordId}/文件`;
      return `
        <div class="generation-card">
          <div class="generation-row">
            <a href="${href}">${escapeHtml(record.文件名)}</a>
            <span class="muted-text">${formatDate(record.生成时间)}</span>
          </div>
        </div>
      `;
    }).join("");
  }

  async function generateHandout() {
    if (!state.selectedId) return;
    setGlobalStatus("生成中");
    els.generateButton.disabled = true;
    try {
      const record = await requestJson(`${handoutRoot}/${state.selectedId}/生成`, { method: "POST" });
      await loadGenerations();
      await loadHandouts();
      setGlobalStatus("已生成");
      const href = `${handoutRoot}/${state.selectedId}/生成记录/${idOf(record)}/文件`;
      window.open(href, "_blank");
    } catch (error) {
      setGlobalStatus("生成失败");
      alert(error.message);
    } finally {
      els.generateButton.disabled = false;
    }
  }

  function openPicker(context) {
    if (!state.selectedId) return;
    state.pickerContext = context || { mode: "handout", fixedTargetType: null, parentNode: state.treeRoot };
    els.pickerBackdrop.hidden = false;
    els.pickerTitle.textContent = pickerTitle();
    els.pickerSearchInput.value = "";
    els.pickerRoleInput.value = "";
    if (state.pickerContext.fixedTargetType) {
      els.pickerTargetTypeSelect.value = state.pickerContext.fixedTargetType;
      els.pickerTargetTypeSelect.disabled = true;
    } else {
      els.pickerTargetTypeSelect.disabled = false;
    }
    updatePickerMode();
    loadCandidates();
    els.pickerSearchInput.focus();
  }

  function pickerTitle() {
    if (state.pickerContext?.mode === "section") return "添加内容块到小节";
    if (state.pickerContext?.mode === "content-child") return "添加子内容块";
    return "添加到讲义";
  }

  function closePicker() {
    els.pickerBackdrop.hidden = true;
    state.candidates = [];
    state.pickerContext = null;
    els.pickerTargetTypeSelect.disabled = false;
    els.candidateList.innerHTML = "";
  }

  function updatePickerMode() {
    const targetType = els.pickerTargetTypeSelect.value;
    const isContent = targetType === "内容块";
    els.referenceModeField.style.display = isContent ? "grid" : "none";
    els.pickerRoleField.style.display = isContent ? "grid" : "none";
  }

  async function loadCandidates() {
    const targetType = els.pickerTargetTypeSelect.value;
    const keyword = els.pickerSearchInput.value.trim();
    const params = new URLSearchParams();
    if (keyword) params.set("关键词", keyword);
    const query = params.toString() ? `?${params.toString()}` : "";
    const url = targetType === "小节" ? `${sectionRoot}${query}` : `${contentRoot}${query}`;

    try {
      state.candidates = await requestJson(url, { method: "GET" });
      renderCandidates();
    } catch (error) {
      els.candidateList.innerHTML = `<div class="empty-state">${escapeHtml(error.message)}</div>`;
    }
  }

  function renderCandidates() {
    const targetType = els.pickerTargetTypeSelect.value;
    if (state.candidates.length === 0) {
      els.candidateList.innerHTML = "<div class=\"empty-state\">没有匹配项目</div>";
      return;
    }

    els.candidateList.innerHTML = state.candidates.map((candidate) => {
      const candidateId = idOf(candidate);
      const summary = candidate.摘要 || "";
      const version = targetType === "内容块" ? `v${text(candidate.当前版本号, "0")}` : `${Number(candidate.项目数量 || 0)} 项`;
      return `
        <button class="candidate-item" type="button" data-candidate-id="${candidateId}" data-current-version-id="${candidate.当前版本ID || ""}">
          <strong>${escapeHtml(candidate.标题 || candidate.名称 || `ID ${candidateId}`)}</strong>
          <div class="candidate-meta">
            <span>${escapeHtml(targetType)}</span>
            <span>${escapeHtml(version)}</span>
            <span>${escapeHtml(summary || "暂无摘要")}</span>
          </div>
        </button>
      `;
    }).join("");

    els.candidateList.querySelectorAll("[data-candidate-id]").forEach((button) => {
      button.addEventListener("click", () => addCandidate(Number(button.dataset.candidateId), button.dataset.currentVersionId));
    });
  }

  async function addCandidate(candidateId, currentVersionId) {
    const context = state.pickerContext || { mode: "handout", parentNode: state.treeRoot };
    const targetType = els.pickerTargetTypeSelect.value;

    try {
      if (context.mode === "handout") {
        const body = {
          目标类型: targetType,
          目标ID: candidateId,
        };
        if (targetType === "内容块") applyContentReference(body, currentVersionId, "锁定内容块版本ID");
        await requestJson(`${handoutRoot}/${state.selectedId}/项目`, {
          method: "POST",
          body: JSON.stringify(body),
        });
      } else if (context.mode === "section") {
        const body = {
          内容块ID: candidateId,
          角色: roleValue(),
        };
        applyContentReference(body, currentVersionId, "内容块版本ID");
        await requestJson(`${sectionRoot}/${context.parentNode.目标ID}/项目`, {
          method: "POST",
          body: JSON.stringify(body),
        });
      } else if (context.mode === "content-child") {
        const body = {
          子内容块ID: candidateId,
          角色: roleValue(),
        };
        applyContentReference(body, currentVersionId, "子内容块版本ID");
        await requestJson(`${contentRoot}/${context.parentNode.目标ID}/子块`, {
          method: "POST",
          body: JSON.stringify(body),
        });
      }

      const preferredNodeId = context.parentNode?.节点ID || state.selectedNodeId;
      closePicker();
      await loadArrangementTree(preferredNodeId);
      await loadHandouts();
    } catch (error) {
      alert(error.message);
    }
  }

  function applyContentReference(body, currentVersionId, versionFieldName) {
    const mode = els.referenceModeSelect.value;
    body.引用版本模式 = mode;
    if (mode === "锁定版本") {
      const versionId = Number(currentVersionId);
      if (!versionId) {
        throw new Error("这个内容块还没有当前版本，不能锁定。");
      }
      body[versionFieldName] = versionId;
    }
  }

  function roleValue() {
    return els.pickerRoleInput.value.trim() || null;
  }

  async function editSelectedContentInWord() {
    const node = state.selectedNode;
    if (!node || node.节点类型 !== "内容块") return;

    setGlobalStatus("创建会话");
    try {
      const session = await requestJson(`${contentRoot}/${node.目标ID}/编辑会话`, {
        method: "POST",
        body: JSON.stringify({ 是否打开Word: true }),
      });
      setContextSession(session);
      startPoll(session.会话ID);
      setGlobalStatus("等待编辑");
    } catch (error) {
      setGlobalStatus("创建失败");
      alert(error.message);
    }
  }

  function setContextSession(session) {
    state.activeSession = session;
    if (!session) {
      els.contextSessionStatusText.textContent = "无会话";
      els.contextSessionVersionText.textContent = "-";
      els.contextSessionIdText.textContent = "-";
      return;
    }

    els.contextSessionStatusText.textContent = session.状态 || "-";
    els.contextSessionVersionText.textContent = session.最新版本号 ? `v${session.最新版本号}` : "-";
    els.contextSessionIdText.textContent = session.会话ID ? session.会话ID.slice(0, 10) : "-";
    els.contextMessageText.textContent = session.消息 || session.错误信息 || "等待 Word 关闭后自动同步。";

    if (finalSessionStates.has(session.状态)) {
      clearPoll();
    }
  }

  function startPoll(sessionId) {
    clearPoll();
    state.pollTimer = window.setInterval(() => pollContextSession(sessionId), 2000);
  }

  function clearPoll() {
    if (state.pollTimer) {
      window.clearInterval(state.pollTimer);
      state.pollTimer = null;
    }
  }

  async function pollContextSession(sessionId) {
    try {
      const session = await requestJson(`${contentRoot}/编辑会话/${encodeURIComponent(sessionId)}`, { method: "GET" });
      setContextSession(session);
      if (session.状态 === "已同步" || session.状态 === "无变化") {
        await loadArrangementTree(state.selectedNodeId);
        setContextSession(session);
      }
    } catch (error) {
      clearPoll();
      els.contextMessageText.textContent = error.message;
    }
  }

  function findNodeById(node, nodeId) {
    if (!node || !nodeId) return null;
    if (node.节点ID === nodeId) return node;
    for (const child of node.子节点列表 || []) {
      const found = findNodeById(child, nodeId);
      if (found) return found;
    }
    return null;
  }

  function findParentNode(root, nodeId) {
    if (!root || !nodeId) return null;
    for (const child of root.子节点列表 || []) {
      if (child.节点ID === nodeId) return root;
      const found = findParentNode(child, nodeId);
      if (found) return found;
    }
    return null;
  }

  function countTreeNodes(node) {
    if (!node) return 0;
    return 1 + (node.子节点列表 || []).reduce((count, child) => count + countTreeNodes(child), 0);
  }

  function formatDate(value) {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  function pad(value) {
    return String(value).padStart(2, "0");
  }

  function escapeHtml(value) {
    return text(value, "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  function debounce(fn, delay = 250) {
    let timer = null;
    return function (...args) {
      window.clearTimeout(timer);
      timer = window.setTimeout(() => fn.apply(this, args), delay);
    };
  }

  function bindEvents() {
    els.refreshButton.addEventListener("click", loadHandouts);
    els.searchButton.addEventListener("click", loadHandouts);
    els.keywordInput.addEventListener("keydown", (event) => {
      if (event.key === "Enter") loadHandouts();
    });
    els.statusSelect.addEventListener("change", loadHandouts);
    els.createHandoutButton.addEventListener("click", createHandout);
    els.newTitleInput.addEventListener("keydown", (event) => {
      if (event.key === "Enter") createHandout();
    });
    els.saveHandoutButton.addEventListener("click", saveHandout);
    els.openPickerButton.addEventListener("click", () => openPicker({ mode: "handout", fixedTargetType: null, parentNode: state.treeRoot }));
    els.generateButton.addEventListener("click", generateHandout);
    els.refreshTreeButton.addEventListener("click", () => loadArrangementTree(state.selectedNodeId));
    els.editContextBlockButton.addEventListener("click", editSelectedContentInWord);
    els.reloadContextPreviewButton.addEventListener("click", () => loadNodePreview(state.selectedNode));
    els.closePickerButton.addEventListener("click", closePicker);
    els.pickerBackdrop.addEventListener("click", (event) => {
      if (event.target === els.pickerBackdrop) closePicker();
    });
    els.pickerTargetTypeSelect.addEventListener("change", () => {
      updatePickerMode();
      loadCandidates();
    });
    els.pickerSearchInput.addEventListener("input", debounce(loadCandidates));
  }

  bindEvents();
  function init() {
    els.keywordInput.value = "";
    els.statusSelect.value = "";
    setDetailDisabled(true);
    renderNodeDetail(null);
    loadHandouts();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init, { once: true });
  } else {
    init();
  }
})();
