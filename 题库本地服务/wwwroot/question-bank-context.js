(function () {
  const storageKey = "wordSolution.currentQuestionBankKey";
  const defaultQuestionBankKey = "TEST";
  const listeners = new Set();

  let currentQuestionBankKey = readStoredQuestionBankKey() || defaultQuestionBankKey;
  let questionBanks = [];
  let switcherElement = null;
  let switcherMessage = "";
  let isLoading = false;
  let isCreating = false;

  function getCurrentQuestionBankKey() {
    return currentQuestionBankKey || defaultQuestionBankKey;
  }

  function apiBase() {
    return `/api/题库实例/${encodeURIComponent(getCurrentQuestionBankKey())}`;
  }

  async function initSwitcher(options = {}) {
    if (typeof options.onChange === "function") {
      listeners.add(options.onChange);
    }

    switcherElement = document.querySelector("[data-question-bank-switcher]");
    if (!switcherElement) {
      return notifyQuestionBankChanged({ reason: "init" });
    }

    renderSwitcher();
    await loadQuestionBanks();
    await notifyQuestionBankChanged({ reason: "init" });
  }

  async function loadQuestionBanks() {
    isLoading = true;
    switcherMessage = "正在读取题库列表...";
    renderSwitcher();

    try {
      const response = await fetch("/api/题库实例", { cache: "no-store" });
      if (!response.ok) {
        throw new Error(await response.text() || `读取题库失败：${response.status}`);
      }

      const data = await response.json();
      questionBanks = Array.isArray(data) ? data : [];
      const fallbackMessage = ensureCurrentQuestionBankExists();
      switcherMessage = fallbackMessage || buildCurrentQuestionBankMessage();
    } catch (error) {
      questionBanks = [];
      switcherMessage = error.message || "读取题库失败。";
    } finally {
      isLoading = false;
      renderSwitcher();
    }
  }

  async function createQuestionBank(key, displayName) {
    const trimmedKey = String(key || "").trim();
    if (!trimmedKey) {
      switcherMessage = "请输入题库键。";
      renderSwitcher();
      return;
    }

    isCreating = true;
    switcherMessage = "正在创建题库...";
    renderSwitcher();

    try {
      const response = await fetch("/api/题库实例", {
        method: "POST",
        cache: "no-store",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify({
          题库键: trimmedKey,
          显示名称: String(displayName || trimmedKey).trim() || trimmedKey,
        }),
      });

      if (!response.ok) {
        throw new Error(await response.text() || `创建题库失败：${response.status}`);
      }

      const created = await response.json();
      const createdKey = field(created, "题库键", "key", "Key") || trimmedKey;
      setCurrentQuestionBankKey(createdKey, { notify: false });
      await loadQuestionBanks();
      switcherMessage = `已创建并切换到 ${createdKey}。`;
      renderSwitcher();
      await notifyQuestionBankChanged({ reason: "create" });
    } catch (error) {
      switcherMessage = error.message || "创建题库失败。";
      renderSwitcher();
    } finally {
      isCreating = false;
      renderSwitcher();
    }
  }

  function setCurrentQuestionBankKey(key, options = {}) {
    const trimmedKey = String(key || "").trim();
    if (!trimmedKey || trimmedKey === currentQuestionBankKey) {
      return;
    }

    currentQuestionBankKey = trimmedKey;
    writeStoredQuestionBankKey(currentQuestionBankKey);
    switcherMessage = buildCurrentQuestionBankMessage();
    renderSwitcher();

    if (options.notify !== false) {
      notifyQuestionBankChanged({ reason: "select" });
    }
  }

  function ensureCurrentQuestionBankExists() {
    if (questionBanks.some((item) => field(item, "题库键", "key", "Key") === currentQuestionBankKey)) {
      writeStoredQuestionBankKey(currentQuestionBankKey);
      return "";
    }

    const fallback = questionBanks.find((item) => field(item, "题库键", "key", "Key") === defaultQuestionBankKey)
      || questionBanks[0];
    if (fallback) {
      const fallbackKey = field(fallback, "题库键", "key", "Key");
      if (fallbackKey) {
        currentQuestionBankKey = fallbackKey;
        writeStoredQuestionBankKey(currentQuestionBankKey);
        return `上次选择的题库不存在，已切换到 ${currentQuestionBankKey}。`;
      }
    }

    return "";
  }

  function renderSwitcher() {
    updateCurrentQuestionBankLabels();
    if (!switcherElement) {
      return;
    }

    const current = getCurrentQuestionBank();
    const disabled = isLoading || isCreating || questionBanks.length === 0;
    const options = questionBanks.length > 0
      ? questionBanks.map((item) => {
        const key = field(item, "题库键", "key", "Key") || "";
        const name = field(item, "显示名称", "displayName", "DisplayName") || key;
        const initialized = field(item, "是否已初始化", "isInitialized", "IsInitialized") !== false;
        const label = `${name} (${key})${initialized ? "" : " - 未初始化"}`;
        return `<option value="${escapeHtml(key)}"${key === currentQuestionBankKey ? " selected" : ""}>${escapeHtml(label)}</option>`;
      }).join("")
      : `<option value="${escapeHtml(currentQuestionBankKey)}">${escapeHtml(currentQuestionBankKey)}</option>`;

    switcherElement.innerHTML = `
      <div class="question-bank-switcher__row">
        <label class="question-bank-switcher__field">
          <span>当前题库</span>
          <select data-question-bank-select ${disabled ? "disabled" : ""}>${options}</select>
        </label>
        <button class="question-bank-switcher__icon" type="button" data-question-bank-refresh title="刷新题库列表" aria-label="刷新题库列表" ${isLoading ? "disabled" : ""}>
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M21 12a9 9 0 1 1-2.64-6.36"></path><path d="M21 3v6h-6"></path></svg>
        </button>
      </div>
      <details class="question-bank-switcher__create">
        <summary>新建题库</summary>
        <div class="question-bank-switcher__create-grid">
          <input data-question-bank-new-key type="text" autocomplete="off" placeholder="题库键：WORK 或 正式题库">
          <input data-question-bank-new-name type="text" autocomplete="off" placeholder="显示名称">
          <button type="button" data-question-bank-create ${isCreating ? "disabled" : ""}>${isCreating ? "创建中" : "创建"}</button>
        </div>
      </details>
      <p class="question-bank-switcher__message${current && field(current, "是否已初始化", "isInitialized", "IsInitialized") === false ? " is-warning" : ""}">${escapeHtml(switcherMessage || buildCurrentQuestionBankMessage())}</p>
    `;

    bindSwitcherEvents();
  }

  function bindSwitcherEvents() {
    switcherElement.querySelector("[data-question-bank-select]")?.addEventListener("change", (event) => {
      setCurrentQuestionBankKey(event.currentTarget.value);
    });

    switcherElement.querySelector("[data-question-bank-refresh]")?.addEventListener("click", async () => {
      await loadQuestionBanks();
    });

    switcherElement.querySelector("[data-question-bank-create]")?.addEventListener("click", async () => {
      const keyInput = switcherElement.querySelector("[data-question-bank-new-key]");
      const nameInput = switcherElement.querySelector("[data-question-bank-new-name]");
      await createQuestionBank(keyInput?.value, nameInput?.value);
    });
  }

  function getCurrentQuestionBank() {
    return questionBanks.find((item) => field(item, "题库键", "key", "Key") === currentQuestionBankKey) || null;
  }

  function buildCurrentQuestionBankMessage() {
    const current = getCurrentQuestionBank();
    if (!current) {
      return `当前使用 ${currentQuestionBankKey}。`;
    }

    if (field(current, "是否已初始化", "isInitialized", "IsInitialized") === false) {
      return "当前题库未初始化，页面数据可能无法读取。";
    }

    return `正在使用 ${field(current, "显示名称", "displayName", "DisplayName") || currentQuestionBankKey}。`;
  }

  async function notifyQuestionBankChanged(detail) {
    updateCurrentQuestionBankLabels();
    for (const listener of listeners) {
      await listener({
        key: getCurrentQuestionBankKey(),
        instance: getCurrentQuestionBank(),
        ...detail,
      });
    }
  }

  function updateCurrentQuestionBankLabels() {
    document.querySelectorAll("[data-current-question-bank-label]").forEach((element) => {
      const current = getCurrentQuestionBank();
      const name = current ? field(current, "显示名称", "displayName", "DisplayName") || currentQuestionBankKey : currentQuestionBankKey;
      element.textContent = `当前题库：${name}`;
      element.title = `题库键：${currentQuestionBankKey}`;
    });
  }

  function readStoredQuestionBankKey() {
    try {
      return window.localStorage.getItem(storageKey);
    } catch {
      return null;
    }
  }

  function writeStoredQuestionBankKey(key) {
    try {
      window.localStorage.setItem(storageKey, key);
    } catch {
      // localStorage 不可用时，当前页面仍然可以使用内存中的题库键。
    }
  }

  function field(value, ...names) {
    if (!value) return undefined;
    for (const name of names) {
      if (Object.prototype.hasOwnProperty.call(value, name)) {
        return value[name];
      }
    }

    return undefined;
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  window.QuestionBankContext = {
    apiBase,
    getCurrentQuestionBankKey,
    initSwitcher,
    loadQuestionBanks,
    setCurrentQuestionBankKey,
  };
})();
