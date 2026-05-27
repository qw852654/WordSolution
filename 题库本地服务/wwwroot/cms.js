(function () {
  const bankKey = "TEST";
  const apiBase = `/api/题库实例/${encodeURIComponent(bankKey)}`;
  const apiRoot = `/api/题库实例/${encodeURIComponent(bankKey)}/内容块`;
  const compositeTypes = new Set(["题组", "小节", "练习组", "专题片段"]);
  const finalSessionStates = new Set(["已同步", "无变化", "失败", "已取消"]);

  const state = {
    blocks: [],
    pickerBlocks: [],
    selectedId: null,
    selectedBlock: null,
    children: [],
    structureTree: null,
    versions: [],
    references: null,
    tagKinds: [],
    tagsByKind: {},
    selectedTagIds: new Set(),
    activeSession: null,
    pollTimer: null,
  };

  const els = {
    keywordInput: document.getElementById("keywordInput"),
    typeSelect: document.getElementById("typeSelect"),
    statusSelect: document.getElementById("statusSelect"),
    tagFilterKindSelect: document.getElementById("tagFilterKindSelect"),
    tagFilterSelect: document.getElementById("tagFilterSelect"),
    searchButton: document.getElementById("searchButton"),
    refreshButton: document.getElementById("refreshButton"),
    newTitleInput: document.getElementById("newTitleInput"),
    newTypeSelect: document.getElementById("newTypeSelect"),
    newStructureSelect: document.getElementById("newStructureSelect"),
    createAndEditButton: document.getElementById("createAndEditButton"),
    contentList: document.getElementById("contentList"),
    listCountText: document.getElementById("listCountText"),
    globalStatus: document.getElementById("globalStatus"),
    detailEyebrow: document.getElementById("detailEyebrow"),
    detailTitle: document.getElementById("detailTitle"),
    metaGrid: document.getElementById("metaGrid"),
    editTitleInput: document.getElementById("editTitleInput"),
    editSummaryInput: document.getElementById("editSummaryInput"),
    editTypeSelect: document.getElementById("editTypeSelect"),
    editStatusSelect: document.getElementById("editStatusSelect"),
    editStructureSelect: document.getElementById("editStructureSelect"),
    saveMetaButton: document.getElementById("saveMetaButton"),
    metadataMessageText: document.getElementById("metadataMessageText"),
    saveTagsButton: document.getElementById("saveTagsButton"),
    tagMessageText: document.getElementById("tagMessageText"),
    tagEditor: document.getElementById("tagEditor"),
    editInWordButton: document.getElementById("editInWordButton"),
    sessionStatusText: document.getElementById("sessionStatusText"),
    sessionVersionText: document.getElementById("sessionVersionText"),
    sessionIdText: document.getElementById("sessionIdText"),
    sessionMessageText: document.getElementById("sessionMessageText"),
    manualSyncButton: document.getElementById("manualSyncButton"),
    cancelSessionButton: document.getElementById("cancelSessionButton"),
    addChildButton: document.getElementById("addChildButton"),
    structureNotice: document.getElementById("structureNotice"),
    childList: document.getElementById("childList"),
    structureTree: document.getElementById("structureTree"),
    childPickerBackdrop: document.getElementById("childPickerBackdrop"),
    closeChildPickerButton: document.getElementById("closeChildPickerButton"),
    childSearchInput: document.getElementById("childSearchInput"),
    childRoleSelect: document.getElementById("childRoleSelect"),
    childReferenceModeSelect: document.getElementById("childReferenceModeSelect"),
    childCandidateList: document.getElementById("childCandidateList"),
    reloadVersionsButton: document.getElementById("reloadVersionsButton"),
    versionList: document.getElementById("versionList"),
    reloadReferencesButton: document.getElementById("reloadReferencesButton"),
    referenceNotice: document.getElementById("referenceNotice"),
    referenceStats: document.getElementById("referenceStats"),
    referenceList: document.getElementById("referenceList"),
    reloadPreviewButton: document.getElementById("reloadPreviewButton"),
    previewFrame: document.getElementById("previewFrame"),
    emptyPreview: document.getElementById("emptyPreview"),
  };

  function text(value, fallback = "-") {
    return value === null || value === undefined || value === "" ? fallback : String(value);
  }

  function idOf(value) {
    return value?.id ?? value?.Id ?? value?.ID;
  }

  function prop(value, ...names) {
    if (!value) return undefined;
    for (const name of names) {
      if (Object.prototype.hasOwnProperty.call(value, name)) {
        return value[name];
      }
    }

    return undefined;
  }

  function nameOf(value) {
    return prop(value, "名称", "name", "Name") || "";
  }

  async function requestJson(url, options = {}) {
    const response = await fetch(url, {
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

  function setGlobalStatus(message) {
    els.globalStatus.textContent = message;
  }

  function buildQuery() {
    const params = new URLSearchParams();
    const keyword = els.keywordInput.value.trim();
    if (keyword) params.set("关键词", keyword);
    if (els.typeSelect.value) params.set("类型", els.typeSelect.value);
    if (els.statusSelect.value) params.set("状态", els.statusSelect.value);
    if (els.tagFilterSelect.value) params.append("标签ID列表", els.tagFilterSelect.value);
    const query = params.toString();
    return query ? `?${query}` : "";
  }

  async function loadBlocks() {
    setGlobalStatus("加载中");
    try {
      state.blocks = await requestJson(`${apiRoot}${buildQuery()}`, { method: "GET" });
      renderList();
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("加载失败");
      alert(error.message);
    }
  }

  function renderList() {
    els.contentList.innerHTML = "";
    els.listCountText.textContent = `${state.blocks.length} 条`;

    if (state.blocks.length === 0) {
      const empty = document.createElement("div");
      empty.className = "content-item empty-item";
      empty.textContent = "没有匹配的内容块";
      els.contentList.appendChild(empty);
      return;
    }

    const fragment = document.createDocumentFragment();
    for (const block of state.blocks) {
      const blockId = idOf(block);
      const button = document.createElement("button");
      button.type = "button";
      button.className = `content-item${blockId === state.selectedId ? " is-active" : ""}`;
      button.setAttribute("role", "listitem");
      button.dataset.id = blockId;
      button.innerHTML = `
        <div class="content-item-title">
          <strong title="${escapeHtml(block.标题)}">${escapeHtml(block.标题)}</strong>
          <span class="badge">v${text(block.当前版本号, "0")}</span>
        </div>
        <div class="content-item-meta">
          <span>${escapeHtml(block.类型)}</span>
          <span class="status-text status-${statusClass(block.状态)}">${escapeHtml(block.状态)}</span>
          <span>${escapeHtml(block.结构类型)}</span>
          <span>${formatDate(block.更新时间)}</span>
        </div>
      `;
      button.addEventListener("click", () => selectBlock(blockId));
      fragment.appendChild(button);
    }

    els.contentList.appendChild(fragment);
  }

  async function selectBlock(id) {
    state.selectedId = id;
    state.activeSession = null;
    state.children = [];
    state.structureTree = null;
    state.versions = [];
    state.references = null;
    state.selectedTagIds = new Set();
    clearPoll();
    renderList();
    setSession(null);
    renderTags();

    setGlobalStatus("读取详情");
    try {
      const block = await requestJson(`${apiRoot}/${id}`, { method: "GET" });
      state.selectedBlock = block;
      renderDetail(block);
      loadPreview(block);
      await Promise.all([
        loadBlockTags(block),
        loadStructure(block),
        loadVersions(block),
        loadReferences(block),
      ]);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("读取失败");
      alert(error.message);
    }
  }

  function renderDetail(block) {
    els.detailEyebrow.textContent = `ID ${idOf(block)}`;
    els.detailTitle.textContent = block.标题;
    els.editInWordButton.disabled = false;
    els.reloadPreviewButton.disabled = !block.当前版本ID;
    els.addChildButton.disabled = !block.是否允许子块;
    els.reloadVersionsButton.disabled = false;
    els.reloadReferencesButton.disabled = false;
    renderMetadataForm(block);

    const values = [
      ["类型", block.类型],
      ["状态", block.状态],
      ["结构", block.结构类型],
      ["版本", block.当前版本号 ? `v${block.当前版本号}` : "无"],
    ];

    els.metaGrid.innerHTML = values.map(([label, value]) => `
      <div class="meta-card">
        <span>${escapeHtml(label)}</span>
        <strong>${escapeHtml(value)}</strong>
      </div>
    `).join("");
  }

  function renderMetadataForm(block) {
    const hasBlock = !!block;
    setMetadataDisabled(!hasBlock);
    els.editTitleInput.value = block?.标题 || "";
    els.editSummaryInput.value = block?.摘要 || "";
    els.editTypeSelect.value = block?.类型 || "知识点";
    els.editStatusSelect.value = block?.状态 || "草稿";
    els.editStructureSelect.value = block?.结构类型 || "原子块";
    els.metadataMessageText.textContent = hasBlock
      ? "元数据保存不会生成 Word 内容版本。"
      : "选择内容块后可编辑标题、摘要、类型和状态。";
  }

  function setMetadataDisabled(disabled) {
    els.editTitleInput.disabled = disabled;
    els.editSummaryInput.disabled = disabled;
    els.editTypeSelect.disabled = disabled;
    els.editStatusSelect.disabled = disabled;
    els.editStructureSelect.disabled = disabled;
    els.saveMetaButton.disabled = disabled;
  }

  async function loadTagCatalog() {
    try {
      const kinds = await requestJson(`${apiBase}/标签种类`, { method: "GET" });
      state.tagKinds = (kinds || []).filter((kind) => prop(kind, "是否在正式工作流中可见") !== false);
      const tagEntries = await Promise.all(state.tagKinds.map(async (kind) => {
        const kindId = idOf(kind);
        const tags = await requestJson(`${apiBase}/标签?标签种类ID=${encodeURIComponent(kindId)}`, { method: "GET" });
        return [kindId, flattenTagNodes(tags || [])];
      }));

      state.tagsByKind = Object.fromEntries(tagEntries);
      renderTagFilterKinds();
      renderTags();
    } catch (error) {
      els.tagMessageText.textContent = `标签目录读取失败：${error.message}`;
      els.tagEditor.innerHTML = "<div class=\"empty-state\">标签目录读取失败</div>";
    }
  }

  function flattenTagNodes(nodes, prefix = []) {
    const result = [];
    for (const tag of nodes) {
      const tagName = nameOf(tag);
      const path = [...prefix, tagName].filter(Boolean);
      result.push({
        tag,
        id: idOf(tag),
        kindId: prop(tag, "标签种类ID"),
        label: path.join(" / "),
        isEnabled: prop(tag, "isEnabled", "IsEnabled") !== false,
      });

      const children = prop(tag, "子标签列表") || [];
      result.push(...flattenTagNodes(children, path));
    }

    return result;
  }

  function renderTagFilterKinds() {
    els.tagFilterKindSelect.innerHTML = `<option value="">全部标签种类</option>${state.tagKinds.map((kind) => `
      <option value="${idOf(kind)}">${escapeHtml(nameOf(kind))}</option>
    `).join("")}`;
    renderTagFilterOptions();
  }

  function renderTagFilterOptions() {
    const kindId = Number(els.tagFilterKindSelect.value || 0);
    const tags = (state.tagsByKind[kindId] || []).filter((item) => item.isEnabled);
    els.tagFilterSelect.disabled = !kindId || tags.length === 0;
    els.tagFilterSelect.innerHTML = `<option value="">全部标签</option>${tags.map((item) => `
      <option value="${item.id}">${escapeHtml(item.label)}</option>
    `).join("")}`;
  }

  async function loadBlockTags(block) {
    if (!block) {
      state.selectedTagIds = new Set();
      renderTags();
      return;
    }

    try {
      const tags = await requestJson(`${apiRoot}/${idOf(block)}/标签`, { method: "GET" });
      state.selectedTagIds = new Set((tags || []).map((tag) => idOf(tag)));
      renderTags();
    } catch (error) {
      state.selectedTagIds = new Set();
      els.tagMessageText.textContent = error.message;
      renderTags();
    }
  }

  function renderTags() {
    const hasBlock = !!state.selectedBlock;
    els.saveTagsButton.disabled = !hasBlock;

    if (state.tagKinds.length === 0) {
      els.tagEditor.innerHTML = "<div class=\"empty-state\">暂无标签目录</div>";
      return;
    }

    if (!hasBlock) {
      els.tagMessageText.textContent = "选择内容块后可挂接章节、难度、来源等标签。";
      els.tagEditor.innerHTML = "<div class=\"empty-state\">选择内容块后编辑分类标签</div>";
      return;
    }

    els.tagMessageText.textContent = "分类只改变内容块归属，不会生成 Word 内容版本。";
    els.tagEditor.innerHTML = state.tagKinds.map((kind) => {
      const kindId = idOf(kind);
      const options = state.tagsByKind[kindId] || [];
      const enabledOptions = options.filter((item) => item.isEnabled);
      const allowsMulti = prop(kind, "是否允许多选") !== false;
      const checkedCount = enabledOptions.filter((item) => state.selectedTagIds.has(item.id)).length;
      const name = `tag-kind-${kindId}`;
      const clearOption = allowsMulti ? "" : `
        <label class="tag-option">
          <input type="radio" name="${name}" value="" ${checkedCount === 0 ? "checked" : ""}>
          <span>不选择</span>
        </label>
      `;

      const optionMarkup = enabledOptions.map((item) => `
        <label class="tag-option">
          <input type="${allowsMulti ? "checkbox" : "radio"}" name="${name}" value="${item.id}" data-tag-id="${item.id}" ${state.selectedTagIds.has(item.id) ? "checked" : ""}>
          <span title="${escapeHtml(item.label)}">${escapeHtml(item.label)}</span>
        </label>
      `).join("");

      return `
        <div class="tag-group">
          <div class="tag-group-header">
            <strong>${escapeHtml(nameOf(kind))}</strong>
            <span>${allowsMulti ? "可多选" : "单选"}</span>
          </div>
          <div class="tag-options">
            ${enabledOptions.length === 0 ? "<span class=\"empty-state\">暂无可用标签</span>" : `${clearOption}${optionMarkup}`}
          </div>
        </div>
      `;
    }).join("");
  }

  async function saveTags() {
    if (!state.selectedBlock) return;

    const selectedIds = Array
      .from(els.tagEditor.querySelectorAll("input[data-tag-id]:checked"))
      .map((input) => Number(input.dataset.tagId))
      .filter((tagId) => tagId > 0);

    setGlobalStatus("保存标签");
    els.tagMessageText.textContent = "保存中";
    try {
      const tags = await requestJson(`${apiRoot}/${state.selectedId}/标签`, {
        method: "PUT",
        body: JSON.stringify({ 标签ID列表: selectedIds }),
      });
      state.selectedTagIds = new Set((tags || []).map((tag) => idOf(tag)));
      renderTags();
      await loadBlocks();
      els.tagMessageText.textContent = `已保存 ${formatDate(new Date().toISOString())}`;
      setGlobalStatus("就绪");
    } catch (error) {
      els.tagMessageText.textContent = error.message;
      setGlobalStatus("保存失败");
      alert(error.message);
    }
  }

  function loadPreview(block) {
    if (!block || !block.当前版本ID) {
      els.previewFrame.removeAttribute("src");
      els.emptyPreview.classList.remove("is-hidden");
      return;
    }

    els.emptyPreview.classList.add("is-hidden");
    els.previewFrame.src = `${apiRoot}/${idOf(block)}/预览html?t=${Date.now()}`;
  }

  async function loadStructure(block) {
    if (!block) {
      renderNoStructure("选择一个内容块后查看组合结构。");
      return;
    }

    if (!block.是否允许子块) {
      state.children = [];
      state.structureTree = {
        内容块: block,
        深度: 1,
        已达到最大深度: false,
        子块列表: [],
      };
      els.structureNotice.textContent = "当前内容块是原子块，不能嵌套子块。";
      renderChildren();
      renderTree();
      return;
    }

    els.structureNotice.textContent = "组合块可以引用其他内容块，并在后续讲义中复用这一组结构。";
    try {
      const blockId = idOf(block);
      const [children, tree] = await Promise.all([
        requestJson(`${apiRoot}/${blockId}/子块`, { method: "GET" }),
        requestJson(`${apiRoot}/${blockId}/结构树`, { method: "GET" }),
      ]);
      state.children = children || [];
      state.structureTree = tree;
      renderChildren();
      renderTree();
    } catch (error) {
      els.structureNotice.textContent = error.message;
      renderNoStructure("结构读取失败");
    }
  }

  function renderNoStructure(message) {
    els.childList.innerHTML = `<div class="empty-state">${escapeHtml(message)}</div>`;
    els.structureTree.innerHTML = `<div class="empty-state">${escapeHtml(message)}</div>`;
  }

  async function loadVersions(block) {
    if (!block) {
      state.versions = [];
      els.reloadVersionsButton.disabled = true;
      renderVersions();
      return;
    }

    els.reloadVersionsButton.disabled = false;
    try {
      state.versions = await requestJson(`${apiRoot}/${idOf(block)}/版本`, { method: "GET" });
      renderVersions();
    } catch (error) {
      state.versions = [];
      els.versionList.innerHTML = `<div class="empty-state">${escapeHtml(error.message)}</div>`;
    }
  }

  function renderVersions() {
    if (!state.selectedBlock) {
      els.versionList.innerHTML = "<div class=\"empty-state\">选择内容块后查看版本</div>";
      return;
    }

    if (state.versions.length === 0) {
      els.versionList.innerHTML = "<div class=\"empty-state\">暂无版本</div>";
      return;
    }

    els.versionList.innerHTML = state.versions.map((version) => `
      <div class="version-item">
        <div class="version-number">v${escapeHtml(version.版本号)}</div>
        <div class="version-date">${formatDate(version.创建时间)}</div>
        <span class="badge${version.是否当前版本 ? " current-version-badge" : ""}">
          ${version.是否当前版本 ? "当前" : `#${escapeHtml(idOf(version))}`}
        </span>
      </div>
    `).join("");
  }

  async function loadReferences(block) {
    if (!block) {
      state.references = null;
      els.reloadReferencesButton.disabled = true;
      renderReferences();
      return;
    }

    els.reloadReferencesButton.disabled = false;
    try {
      state.references = await requestJson(`${apiRoot}/${idOf(block)}/引用`, { method: "GET" });
      renderReferences();
    } catch (error) {
      state.references = null;
      els.referenceNotice.textContent = error.message;
      els.referenceStats.innerHTML = "";
      els.referenceList.innerHTML = `<div class="empty-state">${escapeHtml(error.message)}</div>`;
    }
  }

  function renderReferences() {
    if (!state.selectedBlock) {
      els.referenceNotice.textContent = "选择内容块后查看它被哪些小节和讲义引用。";
      els.referenceStats.innerHTML = "";
      els.referenceList.innerHTML = "<div class=\"empty-state\">选择内容块后查看影响范围</div>";
      return;
    }

    if (!state.references) {
      els.referenceNotice.textContent = "引用关系尚未读取。";
      els.referenceStats.innerHTML = "";
      els.referenceList.innerHTML = "<div class=\"empty-state\">暂无引用数据</div>";
      return;
    }

    const positions = state.references.引用位置列表 || [];
    const staleCount = Number(state.references.锁定旧版本数量 || 0);
    els.referenceNotice.textContent = staleCount > 0
      ? `有 ${staleCount} 个引用锁定在旧版本，更新内容块后这些位置不会自动跟随。`
      : "没有发现锁定旧版本引用。";
    els.referenceStats.innerHTML = [
      ["组合块", state.references.组合块引用数量],
      ["小节", state.references.小节引用数量],
      ["讲义", state.references.讲义引用数量],
      ["旧版本", staleCount],
    ].map(([label, value]) => `
      <div class="reference-stat">
        <span>${escapeHtml(label)}</span>
        <strong>${Number(value || 0)}</strong>
      </div>
    `).join("");

    if (positions.length === 0) {
      els.referenceList.innerHTML = "<div class=\"empty-state\">当前内容块还没有被复用</div>";
      return;
    }

    els.referenceList.innerHTML = positions.map((position) => {
      const stale = !!position.是否锁定旧版本;
      const directText = position.是否直接引用 ? "直接引用" : "间接引用";
      const versionText = position.锁定内容块版本ID
        ? `锁定 v${text(position.锁定版本号, "0")}`
        : text(position.引用版本模式, "跟随最新");
      return `
        <article class="reference-item${stale ? " is-stale" : ""}">
          <div class="reference-title-row">
            <strong title="${escapeHtml(position.引用对象标题)}">${escapeHtml(position.引用对象标题)}</strong>
            <span class="badge">${escapeHtml(position.引用类型)}</span>
          </div>
          <div class="reference-meta">
            <span>${escapeHtml(directText)}</span>
            <span>${escapeHtml(versionText)}</span>
            <span>${stale ? "旧版本" : "会跟随"}</span>
          </div>
          <div class="reference-chain" title="${escapeHtml(position.引用链)}">${escapeHtml(position.引用链)}</div>
        </article>
      `;
    }).join("");
  }

  function renderChildren() {
    els.childList.innerHTML = "";
    const children = sortedChildren();
    if (children.length === 0) {
      els.childList.innerHTML = "<div class=\"empty-state\">还没有子块</div>";
      return;
    }

    els.childList.innerHTML = children.map((child, index) => `
      <div class="child-item">
        <div class="child-main">
          <div class="child-title-row">
            <strong title="${escapeHtml(child.子内容块标题)}">${escapeHtml(child.子内容块标题)}</strong>
            <span class="badge">v${text(child.引用版本号, "0")}</span>
          </div>
          <div class="child-meta">
            <span>${escapeHtml(child.角色 || "未指定角色")}</span>
            <span>${escapeHtml(child.子内容块类型)}</span>
            <span>${escapeHtml(child.引用版本模式)}</span>
            <span>${escapeHtml(child.子内容块结构类型)}</span>
          </div>
        </div>
        <div class="child-actions">
          <button class="icon-button compact" type="button" data-action="up" data-child-id="${idOf(child)}" title="上移" aria-label="上移" ${index === 0 ? "disabled" : ""}>
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m18 15-6-6-6 6"></path></svg>
          </button>
          <button class="icon-button compact" type="button" data-action="down" data-child-id="${idOf(child)}" title="下移" aria-label="下移" ${index === children.length - 1 ? "disabled" : ""}>
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m6 9 6 6 6-6"></path></svg>
          </button>
          <button class="icon-button compact danger" type="button" data-action="remove" data-child-id="${idOf(child)}" title="移除引用" aria-label="移除引用">
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M18 6 6 18"></path><path d="m6 6 12 12"></path></svg>
          </button>
        </div>
      </div>
    `).join("");

    els.childList.querySelectorAll("[data-action]").forEach((button) => {
      const childId = Number(button.dataset.childId);
      const action = button.dataset.action;
      if (action === "up") button.addEventListener("click", () => moveChild(childId, -1));
      if (action === "down") button.addEventListener("click", () => moveChild(childId, 1));
      if (action === "remove") button.addEventListener("click", () => removeChild(childId));
    });
  }

  function renderTree() {
    if (!state.structureTree) {
      els.structureTree.innerHTML = "<div class=\"empty-state\">暂无结构树</div>";
      return;
    }

    els.structureTree.innerHTML = window.ContentTree.render(mapStructureTreeNode(state.structureTree), {
      emptyHtml: "<div class=\"empty-state\">暂无结构树</div>",
    });
    window.ContentTree.bind(els.structureTree);
  }

  function mapStructureTreeNode(node) {
    const block = node.内容块 || {};
    const source = node.来源子项;
    const children = node.子块列表 || [];
    const depth = Number(node.深度 || 1);
    const sourceText = source
      ? `${source.角色 || "子块"} · ${source.引用版本模式} · v${text(source.引用版本号, "0")}`
      : "根内容块";

    return {
      id: `content-block-${idOf(block)}-${depth}`,
      title: block.标题 || "未命名内容块",
      badge: block.类型 || "内容块",
      depth,
      selectable: false,
      meta: [
        sourceText,
        block.结构类型,
        block.当前版本号 ? `v${block.当前版本号}` : "无版本",
      ],
      data: {
        contentBlockId: idOf(block),
      },
      children: children.map(mapStructureTreeNode),
    };
  }

  function sortedChildren() {
    return [...state.children].sort((a, b) => {
      const sortA = Number(a.排序 ?? 0);
      const sortB = Number(b.排序 ?? 0);
      return sortA - sortB || idOf(a) - idOf(b);
    });
  }

  async function openChildPicker() {
    if (!state.selectedBlock || !state.selectedBlock.是否允许子块) return;

    els.childPickerBackdrop.classList.remove("is-hidden");
    els.childSearchInput.value = "";
    els.childSearchInput.focus();
    setGlobalStatus("读取候选");
    try {
      state.pickerBlocks = await requestJson(apiRoot, { method: "GET" });
      renderChildCandidates();
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("读取失败");
      els.childCandidateList.innerHTML = `<div class="empty-state">${escapeHtml(error.message)}</div>`;
    }
  }

  function closeChildPicker() {
    els.childPickerBackdrop.classList.add("is-hidden");
  }

  function renderChildCandidates() {
    const keyword = els.childSearchInput.value.trim().toLowerCase();
    const selectedId = state.selectedId;
    const existingChildIds = new Set(state.children.map((child) => child.子内容块ID));
    const mode = els.childReferenceModeSelect.value;

    const candidates = state.pickerBlocks.filter((block) => {
      const blockId = idOf(block);
      if (blockId === selectedId) return false;
      const haystack = [block.标题, block.摘要, block.类型, block.状态, block.结构类型]
        .map((value) => text(value, "").toLowerCase())
        .join(" ");
      return !keyword || haystack.includes(keyword);
    });

    if (candidates.length === 0) {
      els.childCandidateList.innerHTML = "<div class=\"empty-state\">没有可选内容块</div>";
      return;
    }

    els.childCandidateList.innerHTML = candidates.map((block) => {
      const blockId = idOf(block);
      const alreadyAdded = existingChildIds.has(blockId);
      const lockedWithoutVersion = mode === "锁定版本" && !block.当前版本ID;
      const disabled = alreadyAdded || lockedWithoutVersion;
      const reason = alreadyAdded ? "已在子块列表中" : (lockedWithoutVersion ? "没有可锁定版本" : "添加");
      return `
        <button class="candidate-item${disabled ? " is-disabled" : ""}" type="button" data-block-id="${blockId}" ${disabled ? "disabled" : ""}>
          <div>
            <strong title="${escapeHtml(block.标题)}">${escapeHtml(block.标题)}</strong>
            <span>${escapeHtml(block.类型)} · ${escapeHtml(block.状态)} · ${escapeHtml(block.结构类型)} · v${text(block.当前版本号, "0")}</span>
          </div>
          <span class="candidate-action">${escapeHtml(reason)}</span>
        </button>
      `;
    }).join("");

    els.childCandidateList.querySelectorAll("[data-block-id]").forEach((button) => {
      button.addEventListener("click", () => addChild(Number(button.dataset.blockId)));
    });
  }

  async function addChild(childBlockId) {
    if (!state.selectedBlock) return;

    const child = state.pickerBlocks.find((block) => idOf(block) === childBlockId);
    const mode = els.childReferenceModeSelect.value;
    const body = {
      子内容块ID: childBlockId,
      引用版本模式: mode,
      角色: els.childRoleSelect.value || null,
    };

    if (mode === "锁定版本") {
      if (!child?.当前版本ID) {
        alert("锁定版本模式需要子块已有当前版本。");
        return;
      }
      body.子内容块版本ID = child.当前版本ID;
    }

    setGlobalStatus("添加子块");
    try {
      await requestJson(`${apiRoot}/${state.selectedId}/子块`, {
        method: "POST",
        body: JSON.stringify(body),
      });
      closeChildPicker();
      await loadStructure(state.selectedBlock);
      await loadReferences(state.selectedBlock);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("添加失败");
      alert(error.message);
    }
  }

  async function moveChild(childId, direction) {
    const children = sortedChildren();
    const index = children.findIndex((child) => idOf(child) === childId);
    const nextIndex = index + direction;
    if (index < 0 || nextIndex < 0 || nextIndex >= children.length) return;

    const [moving] = children.splice(index, 1);
    children.splice(nextIndex, 0, moving);
    const body = {
      子项排序列表: children.map((child, order) => ({
        子项ID: idOf(child),
        排序: order,
      })),
    };

    setGlobalStatus("调整排序");
    try {
      state.children = await requestJson(`${apiRoot}/${state.selectedId}/子块排序`, {
        method: "PUT",
        body: JSON.stringify(body),
      });
      await loadStructure(state.selectedBlock);
      await loadReferences(state.selectedBlock);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("排序失败");
      alert(error.message);
    }
  }

  async function removeChild(childId) {
    const child = state.children.find((item) => idOf(item) === childId);
    const title = child?.子内容块标题 || "这个子块";
    if (!window.confirm(`移除“${title}”的引用？原内容块不会被删除。`)) return;

    setGlobalStatus("移除子块");
    try {
      await requestJson(`${apiRoot}/${state.selectedId}/子块/${childId}`, { method: "DELETE" });
      await loadStructure(state.selectedBlock);
      await loadReferences(state.selectedBlock);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("移除失败");
      alert(error.message);
    }
  }

  async function saveMetadata() {
    if (!state.selectedBlock) return;

    const title = els.editTitleInput.value.trim();
    if (!title) {
      els.editTitleInput.focus();
      return;
    }

    const structure = els.editStructureSelect.value;
    const body = {
      标题: title,
      摘要: els.editSummaryInput.value.trim() || null,
      内容块类型: els.editTypeSelect.value,
      内容块状态: els.editStatusSelect.value,
      内容块结构类型: structure,
      是否允许子块: structure === "组合块",
    };

    setGlobalStatus("保存信息");
    els.metadataMessageText.textContent = "保存中";
    try {
      const block = await requestJson(`${apiRoot}/${state.selectedId}`, {
        method: "PUT",
        body: JSON.stringify(body),
      });
      state.selectedBlock = block;
      renderDetail(block);
      loadPreview(block);
      await loadBlocks();
      await Promise.all([
        loadStructure(block),
        loadVersions(block),
        loadReferences(block),
      ]);
      els.metadataMessageText.textContent = `已保存 ${formatDate(new Date().toISOString())}`;
      setGlobalStatus("就绪");
    } catch (error) {
      els.metadataMessageText.textContent = error.message;
      setGlobalStatus("保存失败");
      alert(error.message);
    }
  }

  function updateMetadataStructureDefault() {
    if (els.editTypeSelect.value === "题目") {
      els.editStructureSelect.value = "原子块";
      return;
    }

    if (compositeTypes.has(els.editTypeSelect.value)) {
      els.editStructureSelect.value = "组合块";
    }
  }

  async function createAndEdit() {
    const title = els.newTitleInput.value.trim();
    if (!title) {
      els.newTitleInput.focus();
      return;
    }

    const structure = els.newStructureSelect.value;
    const body = {
      标题: title,
      内容块类型: els.newTypeSelect.value,
      内容块状态: "草稿",
      内容块结构类型: structure,
      是否允许子块: structure === "组合块",
      是否打开Word: true,
    };

    setGlobalStatus("创建会话");
    try {
      const session = await requestJson(`${apiRoot}/编辑会话`, {
        method: "POST",
        body: JSON.stringify(body),
      });
      els.newTitleInput.value = "";
      setSession(session);
      await loadBlocks();
      await selectBlock(session.内容块ID);
      setSession(session);
      startPoll(session.会话ID);
      setGlobalStatus("等待编辑");
    } catch (error) {
      setGlobalStatus("创建失败");
      alert(error.message);
    }
  }

  async function editSelectedInWord() {
    if (!state.selectedBlock) return;

    setGlobalStatus("创建会话");
    try {
      const session = await requestJson(`${apiRoot}/${idOf(state.selectedBlock)}/编辑会话`, {
        method: "POST",
        body: JSON.stringify({ 是否打开Word: true }),
      });
      setSession(session);
      startPoll(session.会话ID);
      setGlobalStatus("等待编辑");
    } catch (error) {
      setGlobalStatus("创建失败");
      alert(error.message);
    }
  }

  function setSession(session) {
    state.activeSession = session;
    if (!session) {
      els.sessionStatusText.textContent = "无会话";
      els.sessionVersionText.textContent = "-";
      els.sessionIdText.textContent = "-";
      els.sessionMessageText.textContent = "";
      els.manualSyncButton.disabled = true;
      els.cancelSessionButton.disabled = true;
      return;
    }

    els.sessionStatusText.textContent = session.状态;
    els.sessionVersionText.textContent = session.最新版本号 ? `v${session.最新版本号}` : "-";
    els.sessionIdText.textContent = session.会话ID ? session.会话ID.slice(0, 10) : "-";
    els.sessionMessageText.textContent = session.消息 || session.错误信息 || "";

    const isFinal = finalSessionStates.has(session.状态);
    els.manualSyncButton.disabled = isFinal;
    els.cancelSessionButton.disabled = isFinal;

    if (isFinal) {
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
      const session = await requestJson(`${apiRoot}/编辑会话/${encodeURIComponent(sessionId)}`, { method: "GET" });
      setSession(session);
      if (session.状态 === "已同步" || session.状态 === "无变化") {
        await loadBlocks();
        if (state.selectedId) {
          await selectBlock(state.selectedId);
          setSession(session);
        }
      }
    } catch (error) {
      clearPoll();
      els.sessionMessageText.textContent = error.message;
    }
  }

  async function manualSync() {
    if (!state.activeSession) return;
    setGlobalStatus("同步中");
    try {
      const session = await requestJson(`${apiRoot}/编辑会话/${encodeURIComponent(state.activeSession.会话ID)}/同步`, {
        method: "POST",
        body: "{}",
      });
      setSession(session);
      await loadBlocks();
      if (state.selectedId) {
        await selectBlock(state.selectedId);
        setSession(session);
      }
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("同步失败");
      alert(error.message);
    }
  }

  async function cancelSession() {
    if (!state.activeSession) return;
    setGlobalStatus("取消中");
    try {
      const session = await requestJson(`${apiRoot}/编辑会话/${encodeURIComponent(state.activeSession.会话ID)}/取消`, {
        method: "POST",
        body: "{}",
      });
      setSession(session);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("取消失败");
      alert(error.message);
    }
  }

  function updateCreateDefaults() {
    if (compositeTypes.has(els.newTypeSelect.value)) {
      els.newStructureSelect.value = "组合块";
    } else {
      els.newStructureSelect.value = "原子块";
    }
  }

  function escapeHtml(value) {
    return text(value, "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;");
  }

  function formatDate(value) {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return `${date.getMonth() + 1}/${date.getDate()} ${String(date.getHours()).padStart(2, "0")}:${String(date.getMinutes()).padStart(2, "0")}`;
  }

  function statusClass(value) {
    switch (value) {
      case "可复用":
        return "ready";
      case "待审查":
        return "review";
      case "需修订":
        return "revise";
      case "已废弃":
        return "archived";
      default:
        return "draft";
    }
  }

  function bindEvents() {
    els.searchButton.addEventListener("click", loadBlocks);
    els.refreshButton.addEventListener("click", loadBlocks);
    els.keywordInput.addEventListener("keydown", (event) => {
      if (event.key === "Enter") loadBlocks();
    });
    els.typeSelect.addEventListener("change", loadBlocks);
    els.statusSelect.addEventListener("change", loadBlocks);
    els.tagFilterKindSelect.addEventListener("change", () => {
      renderTagFilterOptions();
      loadBlocks();
    });
    els.tagFilterSelect.addEventListener("change", loadBlocks);
    els.newTypeSelect.addEventListener("change", updateCreateDefaults);
    els.editTypeSelect.addEventListener("change", updateMetadataStructureDefault);
    els.saveMetaButton.addEventListener("click", saveMetadata);
    els.saveTagsButton.addEventListener("click", saveTags);
    els.createAndEditButton.addEventListener("click", createAndEdit);
    els.editInWordButton.addEventListener("click", editSelectedInWord);
    els.manualSyncButton.addEventListener("click", manualSync);
    els.cancelSessionButton.addEventListener("click", cancelSession);
    els.addChildButton.addEventListener("click", openChildPicker);
    els.closeChildPickerButton.addEventListener("click", closeChildPicker);
    els.childPickerBackdrop.addEventListener("click", (event) => {
      if (event.target === els.childPickerBackdrop) closeChildPicker();
    });
    els.childSearchInput.addEventListener("input", renderChildCandidates);
    els.childReferenceModeSelect.addEventListener("change", renderChildCandidates);
    els.reloadVersionsButton.addEventListener("click", () => loadVersions(state.selectedBlock));
    els.reloadReferencesButton.addEventListener("click", () => loadReferences(state.selectedBlock));
    els.reloadPreviewButton.addEventListener("click", () => loadPreview(state.selectedBlock));
    window.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && !els.childPickerBackdrop.classList.contains("is-hidden")) {
        closeChildPicker();
      }
    });
  }

  async function init() {
    bindEvents();
    updateCreateDefaults();
    renderMetadataForm(null);
    renderTags();
    renderNoStructure("选择一个内容块后查看组合结构。");
    renderVersions();
    renderReferences();
    await loadTagCatalog();
    await loadBlocks();
  }

  init();
})();
