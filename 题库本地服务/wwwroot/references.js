(function () {
  const state = {
    references: [],
  };

  const els = {
    refreshButton: document.getElementById("refreshButton"),
    keywordInput: document.getElementById("keywordInput"),
    globalStatus: document.getElementById("globalStatus"),
    statsGrid: document.getElementById("statsGrid"),
    countText: document.getElementById("countText"),
    referenceList: document.getElementById("referenceList"),
  };

  function apiBase() {
    return window.QuestionBankContext.apiBase();
  }

  async function requestJson(url) {
    const response = await fetch(url, {
      headers: { "Content-Type": "application/json; charset=utf-8" },
    });
    if (!response.ok) {
      throw new Error(await response.text() || `请求失败：${response.status}`);
    }

    return response.json();
  }

  async function loadReferences() {
    els.globalStatus.textContent = "加载中";
    try {
      state.references = await requestJson(`${apiBase()}/引用关系/旧版本引用`);
      render();
      els.globalStatus.textContent = "就绪";
    } catch (error) {
      els.globalStatus.textContent = "加载失败";
      els.referenceList.innerHTML = `<div class="empty-state">${escapeHtml(error.message)}</div>`;
    }
  }

  function render() {
    const keyword = els.keywordInput.value.trim().toLowerCase();
    const filtered = state.references.filter((item) => {
      const haystack = [
        item.内容块标题,
        item.引用对象标题,
        item.引用类型,
        item.引用链,
      ].map((value) => text(value, "").toLowerCase()).join(" ");
      return !keyword || haystack.includes(keyword);
    });

    const typeGroups = groupBy(filtered, (item) => item.引用类型);
    els.statsGrid.innerHTML = [
      ["全部旧版本", filtered.length],
      ["组合块", typeGroups["组合块"]?.length || 0],
      ["小节", typeGroups["小节"]?.length || 0],
      ["讲义", typeGroups["讲义"]?.length || 0],
    ].map(([label, value]) => `
      <div class="stat-card">
        <span>${escapeHtml(label)}</span>
        <strong>${Number(value || 0)}</strong>
      </div>
    `).join("");

    els.countText.textContent = `${filtered.length} 条`;
    if (filtered.length === 0) {
      els.referenceList.innerHTML = "<div class=\"empty-state\">没有旧版本锁定引用</div>";
      return;
    }

    els.referenceList.innerHTML = filtered.map((item) => `
      <article class="reference-item">
        <div class="reference-title-row">
          <strong>${escapeHtml(item.内容块标题)}</strong>
          <span class="badge">${escapeHtml(item.引用类型)}</span>
        </div>
        <div class="reference-meta">
          <span>当前 v${text(item.当前版本号, "0")}</span>
          <span>锁定 v${text(item.锁定版本号, "0")}</span>
          <span>${escapeHtml(item.引用对象标题)}</span>
        </div>
        <div class="reference-chain" title="${escapeHtml(item.引用链)}">${escapeHtml(item.引用链)}</div>
      </article>
    `).join("");
  }

  function groupBy(items, keySelector) {
    return items.reduce((map, item) => {
      const key = keySelector(item);
      map[key] = map[key] || [];
      map[key].push(item);
      return map;
    }, {});
  }

  function text(value, fallback = "-") {
    return value === null || value === undefined || value === "" ? fallback : String(value);
  }

  function escapeHtml(value) {
    return text(value, "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  async function reloadForCurrentQuestionBank() {
    state.references = [];
    render();
    await loadReferences();
  }

  async function init() {
    els.refreshButton.addEventListener("click", loadReferences);
    els.keywordInput.addEventListener("input", render);
    await window.QuestionBankContext.initSwitcher({ onChange: reloadForCurrentQuestionBank });
  }

  init();
})();
