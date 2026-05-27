(function () {
  function text(value, fallback = "") {
    return value === null || value === undefined || value === "" ? fallback : String(value);
  }

  function escapeHtml(value) {
    return text(value)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  function render(nodes, options = {}) {
    const list = Array.isArray(nodes) ? nodes : [nodes].filter(Boolean);
    if (list.length === 0) {
      return options.emptyHtml || "<div class=\"empty-state\">暂无结构</div>";
    }

    return `<div class="content-tree">${list.map((node) => renderNode(node, 1, options)).join("")}</div>`;
  }

  function renderNode(node, depth, options) {
    const nodeId = text(node.id || `${node.kind || "node"}-${depth}`);
    const children = Array.isArray(node.children) ? node.children : [];
    const hasChildren = children.length > 0;
    const selectedId = text(options.selectedId || "");
    const selected = node.selected || (selectedId && selectedId === nodeId);
    const selectable = node.selectable !== false;
    const disabled = node.disabled === true;
    const actions = Array.isArray(node.actions) ? node.actions : [];
    const meta = Array.isArray(node.meta) ? node.meta.filter((item) => text(item, "")) : [];
    const data = node.data || {};
    const dataAttrs = Object.keys(data)
      .filter((key) => data[key] !== null && data[key] !== undefined && data[key] !== "")
      .map((key) => `data-${toKebab(key)}="${escapeHtml(data[key])}"`)
      .join(" ");
    const nodeTag = "div";
    const selectableClass = selectable ? " is-selectable" : "";
    const selectedClass = selected ? " is-selected" : "";
    const disabledClass = disabled ? " is-disabled" : "";
    const actionMarkup = actions.length > 0
      ? `<div class="content-tree-actions">${actions.map(renderAction).join("")}</div>`
      : "";
    const childMarkup = hasChildren
      ? `<div class="content-tree-children">${children.map((child) => renderNode(child, depth + 1, options)).join("")}</div>`
      : "";

    return `
      <div class="content-tree-branch" data-tree-branch-id="${escapeHtml(nodeId)}">
        <${nodeTag} class="content-tree-node${selectableClass}${selectedClass}${disabledClass}" data-tree-node-id="${escapeHtml(nodeId)}" ${dataAttrs}>
          ${renderToggle(hasChildren, nodeId)}
          <span class="content-tree-depth">${Number(node.depth || depth)}</span>
          <div class="content-tree-main">
            <div class="content-tree-title-row">
              <strong title="${escapeHtml(node.title || "未命名")}">${escapeHtml(node.title || "未命名")}</strong>
              ${node.badge ? `<span class="content-tree-badge">${escapeHtml(node.badge)}</span>` : ""}
            </div>
            ${meta.length > 0 ? `<div class="content-tree-meta">${meta.map((item) => `<span title="${escapeHtml(item)}">${escapeHtml(item)}</span>`).join("")}</div>` : ""}
          </div>
          ${actionMarkup}
        </${nodeTag}>
        ${childMarkup}
      </div>
    `;
  }

  function renderToggle(hasChildren, nodeId) {
    if (!hasChildren) {
      return "<span class=\"content-tree-toggle-placeholder\" aria-hidden=\"true\"></span>";
    }

    return `
      <button class="content-tree-toggle" type="button" data-tree-toggle-id="${escapeHtml(nodeId)}" title="展开或收起" aria-label="展开或收起">
        <svg class="content-tree-chevron" viewBox="0 0 24 24" aria-hidden="true">
          <path d="m6 9 6 6 6-6"></path>
        </svg>
      </button>
    `;
  }

  function renderAction(action) {
    const label = text(action.label || action.title || action.key || "操作");
    return `
      <button class="content-tree-action" type="button" data-tree-action="${escapeHtml(action.key || "")}" title="${escapeHtml(action.title || label)}" aria-label="${escapeHtml(action.title || label)}">
        ${action.icon || ""}
        <span>${escapeHtml(label)}</span>
      </button>
    `;
  }

  function bind(container, handlers = {}) {
    if (!container) return;

    container.querySelectorAll("[data-tree-toggle-id]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        const branch = button.closest(".content-tree-branch");
        if (branch) {
          branch.classList.toggle("is-collapsed");
        }
      });
    });

    container.querySelectorAll("[data-tree-node-id]").forEach((nodeElement) => {
      nodeElement.addEventListener("click", (event) => {
        if (event.target.closest("[data-tree-toggle-id], [data-tree-action]")) {
          return;
        }

        if (typeof handlers.onSelect === "function") {
          handlers.onSelect(readDataset(nodeElement), nodeElement);
        }
      });
    });

    container.querySelectorAll("[data-tree-action]").forEach((button) => {
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        const nodeElement = button.closest("[data-tree-node-id]");
        if (typeof handlers.onAction === "function") {
          handlers.onAction(button.dataset.treeAction, readDataset(nodeElement), button);
        }
      });
    });
  }

  function readDataset(element) {
    return element ? { ...element.dataset } : {};
  }

  function toKebab(value) {
    return value.replace(/[A-Z]/g, (match) => `-${match.toLowerCase()}`);
  }

  window.ContentTree = {
    render,
    bind,
  };
})();
