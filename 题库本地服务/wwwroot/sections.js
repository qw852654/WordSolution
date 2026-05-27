(function () {
  const bankKey = "TEST";
  const apiBase = `/api/题库实例/${encodeURIComponent(bankKey)}`;
  const sectionRoot = `${apiBase}/小节`;
  const contentRoot = `${apiBase}/内容块`;

  const state = {
    sections: [],
    selectedId: null,
    selectedSection: null,
    items: [],
    contentBlocks: [],
    chapterTags: [],
  };

  const els = {
    keywordInput: document.getElementById("keywordInput"),
    statusSelect: document.getElementById("statusSelect"),
    chapterFilterSelect: document.getElementById("chapterFilterSelect"),
    searchButton: document.getElementById("searchButton"),
    refreshButton: document.getElementById("refreshButton"),
    newTitleInput: document.getElementById("newTitleInput"),
    newChapterSelect: document.getElementById("newChapterSelect"),
    createSectionButton: document.getElementById("createSectionButton"),
    sectionList: document.getElementById("sectionList"),
    sectionCountText: document.getElementById("sectionCountText"),
    globalStatus: document.getElementById("globalStatus"),
    detailEyebrow: document.getElementById("detailEyebrow"),
    detailTitle: document.getElementById("detailTitle"),
    statsGrid: document.getElementById("statsGrid"),
    openContentPickerButton: document.getElementById("openContentPickerButton"),
    editTitleInput: document.getElementById("editTitleInput"),
    editChapterSelect: document.getElementById("editChapterSelect"),
    editStatusSelect: document.getElementById("editStatusSelect"),
    editSummaryInput: document.getElementById("editSummaryInput"),
    saveSectionButton: document.getElementById("saveSectionButton"),
    metadataMessageText: document.getElementById("metadataMessageText"),
    itemsMessageText: document.getElementById("itemsMessageText"),
    itemList: document.getElementById("itemList"),
    reloadPreviewButton: document.getElementById("reloadPreviewButton"),
    previewFrame: document.getElementById("previewFrame"),
    emptyPreview: document.getElementById("emptyPreview"),
    contentPickerBackdrop: document.getElementById("contentPickerBackdrop"),
    closeContentPickerButton: document.getElementById("closeContentPickerButton"),
    contentSearchInput: document.getElementById("contentSearchInput"),
    itemRoleSelect: document.getElementById("itemRoleSelect"),
    referenceModeSelect: document.getElementById("referenceModeSelect"),
    candidateList: document.getElementById("candidateList"),
  };

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

  function text(value, fallback = "-") {
    return value === null || value === undefined || value === "" ? fallback : String(value);
  }

  function setGlobalStatus(message) {
    els.globalStatus.textContent = message;
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

  async function loadChapters() {
    const tags = await requestJson(`${apiBase}/标签?标签种类ID=1`, { method: "GET" });
    state.chapterTags = flattenTags(tags || []);
    renderChapterOptions();
  }

  function flattenTags(nodes, prefix = []) {
    const result = [];
    for (const tag of nodes) {
      const name = prop(tag, "名称", "name", "Name") || "";
      const path = [...prefix, name].filter(Boolean);
      result.push({
        id: idOf(tag),
        label: path.join(" / "),
        isEnabled: prop(tag, "isEnabled", "IsEnabled") !== false,
      });

      result.push(...flattenTags(prop(tag, "子标签列表") || [], path));
    }

    return result;
  }

  function renderChapterOptions() {
    const options = state.chapterTags
      .filter((tag) => tag.isEnabled)
      .map((tag) => `<option value="${tag.id}">${escapeHtml(tag.label)}</option>`)
      .join("");
    els.chapterFilterSelect.innerHTML = `<option value="">全部章节</option>${options}`;
    els.newChapterSelect.innerHTML = `<option value="">未归属</option>${options}`;
    els.editChapterSelect.innerHTML = `<option value="">未归属</option>${options}`;
  }

  function buildSectionQuery() {
    const params = new URLSearchParams();
    const keyword = els.keywordInput.value.trim();
    if (keyword) params.set("关键词", keyword);
    if (els.statusSelect.value) params.set("状态", els.statusSelect.value);
    if (els.chapterFilterSelect.value) params.set("章节标签ID", els.chapterFilterSelect.value);
    const query = params.toString();
    return query ? `?${query}` : "";
  }

  async function loadSections() {
    setGlobalStatus("加载中");
    try {
      state.sections = await requestJson(`${sectionRoot}${buildSectionQuery()}`, { method: "GET" });
      renderSectionList();
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("加载失败");
      alert(error.message);
    }
  }

  function renderSectionList() {
    els.sectionList.innerHTML = "";
    els.sectionCountText.textContent = `${state.sections.length} 条`;

    if (state.sections.length === 0) {
      els.sectionList.innerHTML = "<div class=\"empty-state\">没有匹配的小节</div>";
      return;
    }

    els.sectionList.innerHTML = state.sections.map((section) => {
      const sectionId = idOf(section);
      return `
        <button class="section-item${sectionId === state.selectedId ? " is-active" : ""}" type="button" data-section-id="${sectionId}">
          <div class="section-title-row">
            <strong title="${escapeHtml(section.标题)}">${escapeHtml(section.标题)}</strong>
            <span class="badge">${escapeHtml(section.状态)}</span>
          </div>
          <div class="section-meta">
            <span>${escapeHtml(section.章节名称 || "未归属")}</span>
            <span>${Number(section.项目数量 || 0)} 项</span>
            <span>${formatDate(section.更新时间)}</span>
          </div>
        </button>
      `;
    }).join("");

    els.sectionList.querySelectorAll("[data-section-id]").forEach((button) => {
      button.addEventListener("click", () => selectSection(Number(button.dataset.sectionId)));
    });
  }

  async function selectSection(id) {
    state.selectedId = id;
    state.selectedSection = null;
    state.items = [];
    renderSectionList();
    setDetailDisabled(true);
    setGlobalStatus("读取小节");

    try {
      const section = await requestJson(`${sectionRoot}/${id}`, { method: "GET" });
      state.selectedSection = section;
      renderSectionDetail(section);
      await loadItems(section);
      loadPreview(section);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("读取失败");
      alert(error.message);
    }
  }

  function renderSectionDetail(section) {
    els.detailEyebrow.textContent = `ID ${idOf(section)}`;
    els.detailTitle.textContent = section.标题;
    els.editTitleInput.value = section.标题 || "";
    els.editSummaryInput.value = section.摘要 || "";
    els.editChapterSelect.value = section.章节标签ID || "";
    els.editStatusSelect.value = section.状态 || "草稿";
    els.metadataMessageText.textContent = "元数据保存不会改变内容块版本。";
    setDetailDisabled(false);
    renderStats(section);
  }

  function renderStats(section = state.selectedSection) {
    const values = [
      ["项目", section?.项目数量],
      ["知识点", section?.知识点数量],
      ["例题", section?.例题数量],
      ["练习", section?.练习数量],
    ];
    els.statsGrid.innerHTML = values.map(([label, value]) => `
      <div class="stat-card">
        <span>${escapeHtml(label)}</span>
        <strong>${escapeHtml(text(value, "-"))}</strong>
      </div>
    `).join("");
  }

  function setDetailDisabled(disabled) {
    els.editTitleInput.disabled = disabled;
    els.editSummaryInput.disabled = disabled;
    els.editChapterSelect.disabled = disabled;
    els.editStatusSelect.disabled = disabled;
    els.saveSectionButton.disabled = disabled;
    els.openContentPickerButton.disabled = disabled;
    els.reloadPreviewButton.disabled = disabled;
  }

  async function loadItems(section) {
    if (!section) {
      state.items = [];
      renderItems();
      return;
    }

    try {
      state.items = await requestJson(`${sectionRoot}/${idOf(section)}/项目`, { method: "GET" });
      renderItems();
    } catch (error) {
      state.items = [];
      els.itemList.innerHTML = `<div class="empty-state">${escapeHtml(error.message)}</div>`;
    }
  }

  function renderItems() {
    if (!state.selectedSection) {
      els.itemList.innerHTML = "<div class=\"empty-state\">选择小节后查看项目</div>";
      return;
    }

    const items = sortedItems();
    if (items.length === 0) {
      els.itemList.innerHTML = "<div class=\"empty-state\">还没有内容块</div>";
      return;
    }

    els.itemList.innerHTML = items.map((item, index) => `
      <div class="item-row">
        <div class="item-index">${index + 1}</div>
        <div>
          <div class="item-title-row">
            <strong title="${escapeHtml(item.内容块标题)}">${escapeHtml(item.内容块标题)}</strong>
            <span class="badge">v${text(item.引用版本号, "0")}</span>
          </div>
          <div class="item-meta">
            <span>${escapeHtml(item.角色 || item.内容块类型)}</span>
            <span>${escapeHtml(item.内容块类型)}</span>
            <span>${escapeHtml(item.引用版本模式)}</span>
            <span>${escapeHtml(item.内容块状态)}</span>
          </div>
        </div>
        <div class="item-actions">
          <button class="icon-button compact" type="button" data-action="up" data-item-id="${idOf(item)}" title="上移" aria-label="上移" ${index === 0 ? "disabled" : ""}>
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m18 15-6-6-6 6"></path></svg>
          </button>
          <button class="icon-button compact" type="button" data-action="down" data-item-id="${idOf(item)}" title="下移" aria-label="下移" ${index === items.length - 1 ? "disabled" : ""}>
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m6 9 6 6 6-6"></path></svg>
          </button>
          <button class="icon-button compact danger" type="button" data-action="remove" data-item-id="${idOf(item)}" title="移除引用" aria-label="移除引用">
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M18 6 6 18"></path><path d="m6 6 12 12"></path></svg>
          </button>
        </div>
      </div>
    `).join("");

    els.itemList.querySelectorAll("[data-action]").forEach((button) => {
      const itemId = Number(button.dataset.itemId);
      const action = button.dataset.action;
      if (action === "up") button.addEventListener("click", () => moveItem(itemId, -1));
      if (action === "down") button.addEventListener("click", () => moveItem(itemId, 1));
      if (action === "remove") button.addEventListener("click", () => removeItem(itemId));
    });
  }

  function sortedItems() {
    return [...state.items].sort((a, b) => Number(a.排序 || 0) - Number(b.排序 || 0) || idOf(a) - idOf(b));
  }

  async function createSection() {
    const title = els.newTitleInput.value.trim();
    if (!title) {
      els.newTitleInput.focus();
      return;
    }

    setGlobalStatus("新建小节");
    try {
      const section = await requestJson(sectionRoot, {
        method: "POST",
        body: JSON.stringify({
          标题: title,
          摘要: null,
          章节标签ID: els.newChapterSelect.value ? Number(els.newChapterSelect.value) : null,
          状态: "草稿",
        }),
      });
      els.newTitleInput.value = "";
      await loadSections();
      await selectSection(idOf(section));
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("新建失败");
      alert(error.message);
    }
  }

  async function saveSection() {
    if (!state.selectedSection) return;
    const title = els.editTitleInput.value.trim();
    if (!title) {
      els.editTitleInput.focus();
      return;
    }

    setGlobalStatus("保存小节");
    try {
      const section = await requestJson(`${sectionRoot}/${state.selectedId}`, {
        method: "PUT",
        body: JSON.stringify({
          标题: title,
          摘要: els.editSummaryInput.value.trim() || null,
          章节标签ID: els.editChapterSelect.value ? Number(els.editChapterSelect.value) : null,
          状态: els.editStatusSelect.value,
        }),
      });
      state.selectedSection = section;
      renderSectionDetail(section);
      await loadSections();
      loadPreview(section);
      els.metadataMessageText.textContent = `已保存 ${formatDate(new Date().toISOString())}`;
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("保存失败");
      els.metadataMessageText.textContent = error.message;
      alert(error.message);
    }
  }

  async function openContentPicker() {
    if (!state.selectedSection) return;
    els.contentPickerBackdrop.classList.remove("is-hidden");
    els.contentSearchInput.value = "";
    els.contentSearchInput.focus();
    setGlobalStatus("读取内容块");
    try {
      state.contentBlocks = await requestJson(contentRoot, { method: "GET" });
      renderCandidates();
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("读取失败");
      els.candidateList.innerHTML = `<div class="empty-state">${escapeHtml(error.message)}</div>`;
    }
  }

  function closeContentPicker() {
    els.contentPickerBackdrop.classList.add("is-hidden");
  }

  function renderCandidates() {
    const keyword = els.contentSearchInput.value.trim().toLowerCase();
    const existingIds = new Set(state.items.map((item) => item.内容块ID));
    const mode = els.referenceModeSelect.value;
    const candidates = state.contentBlocks.filter((block) => {
      const haystack = [block.标题, block.摘要, block.类型, block.状态, block.结构类型]
        .map((value) => text(value, "").toLowerCase())
        .join(" ");
      return !keyword || haystack.includes(keyword);
    });

    if (candidates.length === 0) {
      els.candidateList.innerHTML = "<div class=\"empty-state\">没有可选内容块</div>";
      return;
    }

    els.candidateList.innerHTML = candidates.map((block) => {
      const blockId = idOf(block);
      const alreadyAdded = existingIds.has(blockId);
      const lockedWithoutVersion = mode === "锁定版本" && !block.当前版本ID;
      const disabled = alreadyAdded || lockedWithoutVersion;
      const reason = alreadyAdded ? "已添加" : (lockedWithoutVersion ? "没有可锁定版本" : "添加");
      return `
        <button class="candidate-item${disabled ? " is-disabled" : ""}" type="button" data-block-id="${blockId}" ${disabled ? "disabled" : ""}>
          <div class="candidate-title-row">
            <strong title="${escapeHtml(block.标题)}">${escapeHtml(block.标题)}</strong>
            <span class="candidate-action">${escapeHtml(reason)}</span>
          </div>
          <div class="candidate-meta">
            <span>${escapeHtml(block.类型)}</span>
            <span>${escapeHtml(block.状态)}</span>
            <span>${escapeHtml(block.结构类型)}</span>
            <span>v${text(block.当前版本号, "0")}</span>
          </div>
        </button>
      `;
    }).join("");

    els.candidateList.querySelectorAll("[data-block-id]").forEach((button) => {
      button.addEventListener("click", () => addItem(Number(button.dataset.blockId)));
    });
  }

  async function addItem(contentBlockId) {
    const block = state.contentBlocks.find((item) => idOf(item) === contentBlockId);
    if (!block || !state.selectedSection) return;

    const mode = els.referenceModeSelect.value;
    const body = {
      内容块ID: contentBlockId,
      引用版本模式: mode,
      角色: els.itemRoleSelect.value || null,
    };

    if (mode === "锁定版本") {
      if (!block.当前版本ID) {
        alert("锁定版本模式需要内容块已有当前版本。");
        return;
      }

      body.内容块版本ID = block.当前版本ID;
    }

    setGlobalStatus("添加项目");
    try {
      await requestJson(`${sectionRoot}/${state.selectedId}/项目`, {
        method: "POST",
        body: JSON.stringify(body),
      });
      closeContentPicker();
      await refreshSelectedSection();
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("添加失败");
      alert(error.message);
    }
  }

  async function moveItem(itemId, direction) {
    const items = sortedItems();
    const index = items.findIndex((item) => idOf(item) === itemId);
    const nextIndex = index + direction;
    if (index < 0 || nextIndex < 0 || nextIndex >= items.length) return;

    const [moving] = items.splice(index, 1);
    items.splice(nextIndex, 0, moving);
    const body = {
      项目排序列表: items.map((item, order) => ({
        小节项ID: idOf(item),
        排序: order,
      })),
    };

    setGlobalStatus("调整排序");
    try {
      state.items = await requestJson(`${sectionRoot}/${state.selectedId}/项目排序`, {
        method: "PUT",
        body: JSON.stringify(body),
      });
      renderItems();
      await refreshSelectedSection(false);
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("排序失败");
      alert(error.message);
    }
  }

  async function removeItem(itemId) {
    const item = state.items.find((entry) => idOf(entry) === itemId);
    if (!item) return;
    if (!window.confirm(`移除“${item.内容块标题}”的引用？原内容块不会被删除。`)) return;

    setGlobalStatus("移除项目");
    try {
      await requestJson(`${sectionRoot}/${state.selectedId}/项目/${itemId}`, { method: "DELETE" });
      await refreshSelectedSection();
      setGlobalStatus("就绪");
    } catch (error) {
      setGlobalStatus("移除失败");
      alert(error.message);
    }
  }

  async function refreshSelectedSection(reloadItems = true) {
    if (!state.selectedId) return;
    const section = await requestJson(`${sectionRoot}/${state.selectedId}`, { method: "GET" });
    state.selectedSection = section;
    renderSectionDetail(section);
    if (reloadItems) {
      await loadItems(section);
    }
    await loadSections();
    loadPreview(section);
  }

  function loadPreview(section) {
    if (!section) {
      els.previewFrame.removeAttribute("src");
      els.emptyPreview.classList.remove("is-hidden");
      return;
    }

    els.emptyPreview.classList.add("is-hidden");
    els.previewFrame.src = `${sectionRoot}/${idOf(section)}/预览html?t=${Date.now()}`;
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

  function bindEvents() {
    els.searchButton.addEventListener("click", loadSections);
    els.refreshButton.addEventListener("click", loadSections);
    els.keywordInput.addEventListener("keydown", (event) => {
      if (event.key === "Enter") loadSections();
    });
    els.statusSelect.addEventListener("change", loadSections);
    els.chapterFilterSelect.addEventListener("change", loadSections);
    els.createSectionButton.addEventListener("click", createSection);
    els.saveSectionButton.addEventListener("click", saveSection);
    els.openContentPickerButton.addEventListener("click", openContentPicker);
    els.closeContentPickerButton.addEventListener("click", closeContentPicker);
    els.contentPickerBackdrop.addEventListener("click", (event) => {
      if (event.target === els.contentPickerBackdrop) closeContentPicker();
    });
    els.contentSearchInput.addEventListener("input", renderCandidates);
    els.referenceModeSelect.addEventListener("change", renderCandidates);
    els.reloadPreviewButton.addEventListener("click", () => loadPreview(state.selectedSection));
    window.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && !els.contentPickerBackdrop.classList.contains("is-hidden")) {
        closeContentPicker();
      }
    });
  }

  async function init() {
    bindEvents();
    setDetailDisabled(true);
    renderStats(null);
    renderItems();
    loadPreview(null);
    await loadChapters();
    await loadSections();
  }

  init();
})();
