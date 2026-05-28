(function () {
  const bankKey = "TEST";
  const apiBase = `/api/题库实例/${encodeURIComponent(bankKey)}`;
  const sectionApiRoot = `${apiBase}/小节`;

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
    sectionPlans: [
      {
        id: "section-basic",
        title: "基础讲解版",
        status: "静态原型",
        summary: "先建立机械能守恒条件，再用例题组和练习巩固。",
        metrics: ["知识点 1", "例题组 1", "练习 1", "下级模型 3"],
      },
      {
        id: "section-advanced",
        title: "提高班版",
        status: "待设计",
        summary: "强化约束关系、速度关联和复杂能量方程。",
        metrics: ["模型迁移", "综合例题", "拓展练习"],
      },
      {
        id: "section-review",
        title: "一轮复习版",
        status: "待设计",
        summary: "面向复习课，突出判断流程和易错点纠偏。",
        metrics: ["方法总结", "易错点", "分层训练"],
      },
    ],
    handouts: [
      {
        id: "handout-special",
        title: "机械能守恒专题讲义",
        status: "静态原型",
        summary: "围绕机械能守恒条件、例题组、圆轨道模型形成专题材料。",
        metrics: ["引用小节 1", "内容块 3", "可生成入口占位"],
      },
      {
        id: "handout-unit-review",
        title: "功能关系单元复习讲义",
        status: "待设计",
        summary: "面向单元复习，把机械能守恒与功、能量变化串联。",
        metrics: ["跨主题", "复习讲义", "引用展开"],
      },
    ],
    resources: [
      {
        id: "resource-knowledge",
        title: "机械能守恒知识点",
        type: "知识点",
        structureType: "原子块",
        remark: "判断守恒条件，区分系统内外力做功。",
        meta: ["当前版本 v3", "标签 功能关系 / 机械能"],
      },
      {
        id: "resource-circle-group",
        title: "圆轨道例题组",
        type: "例题组",
        structureType: "组合块",
        remark: "由临界速度和最高点受力两道例题组成。",
        meta: ["组合渲染", "标签 圆轨道 / 模型迁移"],
      },
      {
        id: "resource-class-practice",
        title: "课堂练习",
        type: "练习",
        structureType: "原子块",
        remark: "三道递进题，用于课堂即时巩固。",
        meta: ["当前版本 v2", "标签 基础巩固"],
      },
    ],
    secondary: {
      references: ["2 份讲义引用机械能守恒基础小节", "1 个组合块引用圆轨道例题组", "暂无旧版本锁定提醒"],
      recent: ["今天更新：机械能守恒知识点 v3", "昨天编排：机械能守恒专题讲义草稿", "本轮页面为静态 IA 原型"],
    },
  };

  const sectionEditor = {
    id: "section-basic",
    title: "机械能守恒 - 基础讲解版",
    topicTitle: "机械能守恒",
    status: "静态原型",
    summary: "这个小节回答：机械能守恒这个教学主题应该怎么讲。",
    apiSectionId: null,
    apiKeyword: "机械能守恒",
    items: [
      {
        id: "sec-knowledge",
        title: "机械能守恒的条件与表达",
        type: "知识点",
        structureType: "原子块",
        source: "内容块：机械能守恒知识点",
        referenceMode: "跟随最新",
        versionStatus: "当前版本 v3",
        detail: "用于小节开头建立判断标准。",
        html: `
          <h4>机械能守恒的条件</h4>
          <p>当系统内只有重力或弹力做功，其他力不做功或做功代数和为零时，系统机械能保持不变。</p>
          <p>解题时先选择研究系统，再判断非保守力做功，最后列初末状态能量方程。</p>
          <div class="doc-equation">E_k1 + E_p1 = E_k2 + E_p2</div>
        `,
      },
      {
        id: "sec-example-group",
        title: "机械能守恒例题组",
        type: "例题组",
        structureType: "组合块",
        source: "组合内容块：机械能守恒例题组",
        referenceMode: "锁定当前版本",
        versionStatus: "组合渲染 v2",
        detail: "用两道例题串起守恒条件和速度、高度关系。",
        html: `
          <h4>例题组说明</h4>
          <p>由两道例题组成，先判断守恒条件，再处理速度与高度关系。</p>
        `,
        children: [
          {
            id: "sec-example-1",
            title: "例题 1：光滑斜面下滑",
            type: "例题",
            structureType: "原子块",
            source: "内容块：光滑斜面下滑",
            referenceMode: "跟随最新",
            versionStatus: "当前版本 v1",
            detail: "从高度差出发建立机械能守恒方程。",
            html: `
              <h4>例题 1</h4>
              <p>质量为 m 的滑块从光滑斜面高 h 处由静止释放，求到达底端时速度大小。</p>
              <div class="doc-equation">mgh = 1/2 mv^2</div>
            `,
          },
          {
            id: "sec-example-2",
            title: "例题 2：弹簧最大压缩量",
            type: "例题",
            structureType: "原子块",
            source: "内容块：弹簧最大压缩量",
            referenceMode: "跟随最新",
            versionStatus: "待审查 v1",
            detail: "把重力势能转化为弹性势能，强调最大压缩位置。",
            html: `
              <h4>例题 2</h4>
              <p>小球从高度 h 处释放后压缩水平弹簧，忽略摩擦，求弹簧最大压缩量。</p>
              <div class="doc-equation">mgh = 1/2 kx^2</div>
            `,
          },
        ],
      },
      {
        id: "sec-practice",
        title: "机械能守恒三题练习",
        type: "练习",
        structureType: "原子块",
        source: "内容块：课堂练习",
        referenceMode: "跟随最新",
        versionStatus: "当前版本 v4",
        detail: "用于讲解后即时练习和反馈。",
        html: `
          <h4>课堂练习</h4>
          <p>1. 判断下列过程机械能是否守恒。</p>
          <p>2. 已知高度差求末速度。</p>
          <p>3. 含弹簧模型中求最大形变量。</p>
        `,
      },
      {
        id: "sec-model-entry",
        title: "下级模型入口",
        type: "模型",
        structureType: "入口卡片",
        source: "教学主题入口",
        referenceMode: "不适用",
        versionStatus: "静态入口",
        detail: "用于继续进入更细的模型主题，不是内容块版本。",
        html: `
          <h4>下级模型</h4>
          <p>这些入口帮助从机械能守恒进入更具体的教学主题。</p>
        `,
        children: [
          {
            id: "sec-model-circle",
            title: "竖直圆轨道",
            type: "模型",
            structureType: "主题入口",
            source: "教学主题：竖直圆轨道",
            referenceMode: "不适用",
            versionStatus: "无版本",
            detail: "最低点、最高点、临界速度。",
            html: "<p>进入竖直圆轨道模型，继续处理临界条件与受力关系。</p>",
          },
          {
            id: "sec-model-rod",
            title: "杆模型",
            type: "模型",
            structureType: "主题入口",
            source: "教学主题：杆模型",
            referenceMode: "不适用",
            versionStatus: "无版本",
            detail: "轻杆约束、端点速度关系。",
            html: "<p>进入杆模型，处理约束关系与能量方程。</p>",
          },
          {
            id: "sec-model-ball",
            title: "球模型",
            type: "模型",
            structureType: "主题入口",
            source: "教学主题：球模型",
            referenceMode: "不适用",
            versionStatus: "无版本",
            detail: "轨道、小球、弹簧组合。",
            html: "<p>进入球模型，处理小球、轨道和弹簧组合问题。</p>",
          },
        ],
      },
    ],
  };

  const handoutEditor = {
    id: "handout-special",
    title: "机械能守恒专题讲义",
    topicTitle: "机械能守恒",
    status: "静态原型",
    summary: "这个讲义回答：这份材料应该怎么输出。",
    items: [
      {
        id: "handout-intro",
        title: "导入",
        type: "讲义项",
        structureType: "独立内容",
        source: "讲义直接拥有",
        referenceMode: "讲义内顺序",
        versionStatus: "静态草稿",
        detail: "从功能关系切入机械能守恒。",
        html: `
          <h4>导入</h4>
          <p>从“力做功改变能量”切入，提出什么时候可以只看初末状态。</p>
        `,
      },
      {
        id: "handout-section-basic",
        title: "机械能守恒基础小节",
        type: "小节引用",
        structureType: "引用展开",
        source: "引用小节：机械能守恒 - 基础讲解版",
        referenceMode: "跟随小节结构",
        versionStatus: "引用展开视图",
        detail: "讲义直接拥有的是这个小节引用，下面展开的是源小节预览。",
        isReferenceView: true,
        referenceNote: "引用展开视图：展开内容来自源小节，不是讲义自己真实拥有的子节点。",
        html: `
          <h4>小节引用</h4>
          <p>讲义中移除该小节时，只删除讲义引用，不删除源小节。</p>
        `,
        children: [
          {
            id: "handout-ref-knowledge",
            title: "知识点",
            type: "知识点",
            structureType: "原子块",
            source: "来自源小节：机械能守恒 - 基础讲解版",
            referenceMode: "引用展开预览",
            versionStatus: "当前版本 v3",
            detail: "源内容块预览；编辑时编辑源内容块。",
            isReferenceView: true,
            referenceNote: "引用展开视图：这是源小节中的内容块预览。",
            html: sectionEditor.items[0].html,
          },
          {
            id: "handout-ref-example-group",
            title: "例题组",
            type: "例题组",
            structureType: "组合块",
            source: "来自源小节：机械能守恒 - 基础讲解版",
            referenceMode: "引用展开预览",
            versionStatus: "组合渲染 v2",
            detail: "源小节内的组合块展开预览。",
            isReferenceView: true,
            referenceNote: "引用展开视图：调整讲义结构不会反向修改源小节结构。",
            html: sectionEditor.items[1].html,
          },
          {
            id: "handout-ref-practice",
            title: "练习",
            type: "练习",
            structureType: "原子块",
            source: "来自源小节：机械能守恒 - 基础讲解版",
            referenceMode: "引用展开预览",
            versionStatus: "当前版本 v4",
            detail: "源小节中的练习内容预览。",
            isReferenceView: true,
            referenceNote: "引用展开视图：讲义当前阶段不生成局部副本。",
            html: sectionEditor.items[2].html,
          },
        ],
      },
      {
        id: "handout-circle-model",
        title: "竖直圆轨道模型",
        type: "内容块",
        structureType: "组合块",
        source: "讲义直接引用内容块",
        referenceMode: "锁定当前版本",
        versionStatus: "组合渲染 v1",
        detail: "作为讲义中的模型专题补充。",
        html: `
          <h4>竖直圆轨道模型</h4>
          <p>最高点临界条件：轻绳模型要求速度不小于临界速度，轻杆模型可以提供支持力。</p>
          <div class="doc-equation">v_min = sqrt(gR)</div>
        `,
      },
      {
        id: "handout-class-practice",
        title: "课堂练习",
        type: "内容块",
        structureType: "原子块",
        source: "讲义直接引用内容块",
        referenceMode: "跟随最新",
        versionStatus: "当前版本 v2",
        detail: "讲义尾部练习，用于课堂检测。",
        html: topicWorkspace.resources[2].remark ? `
          <h4>课堂练习</h4>
          <p>围绕守恒条件、速度表达和模型迁移设置三道题。</p>
        ` : "<p>课堂练习占位。</p>",
      },
    ],
  };

  const resourceCandidates = [
    {
      id: "candidate-circle",
      title: "竖直圆轨道临界条件",
      type: "知识点",
      structureType: "原子块",
      remark: "整理最高点压力为零和杆模型临界条件的差异。",
      meta: ["圆轨道", "临界速度", "当前版本 v2"],
    },
    {
      id: "candidate-spring",
      title: "弹簧能量转化例题",
      type: "例题",
      structureType: "原子块",
      remark: "从释放点到最大压缩位置列能量方程。",
      meta: ["弹簧", "守恒", "当前版本 v1"],
    },
    {
      id: "candidate-practice",
      title: "机械能守恒分层练习",
      type: "练习",
      structureType: "组合块",
      remark: "基础判断、公式代入、模型迁移三组题。",
      meta: ["练习", "分层", "待审查"],
    },
  ];

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
  };

  const state = {
    mode: "topic",
    selectedTopicId: "topic-mechanical-energy",
    selectedSectionPlanId: "section-basic",
    selectedHandoutId: "handout-special",
    selectedSectionNodeId: "sec-knowledge",
    selectedHandoutNodeId: "handout-intro",
    topicNavExpanded: true,
    collapsedCardIds: new Set(),
    insertContext: null,
    exportingSectionWord: false,
  };

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
    const isEditorMode = state.mode !== "topic";
    els.iaLayout.classList.toggle("is-topic-mode", state.mode === "topic");
    els.iaLayout.classList.toggle("is-editor-mode", isEditorMode);
    els.iaLayout.classList.toggle("is-topic-nav-expanded", state.topicNavExpanded);
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

    if (state.mode === "topic") {
      const topic = findTopic(state.selectedTopicId) || findTopic(topicWorkspace.topicId);
      els.pageEyebrow.textContent = "教学主题工作台 · 静态原型";
      els.pageTitle.textContent = topic.title;
      els.breadcrumb.textContent = topicPath(topic.id).join(" / ");
      els.modeStatus.textContent = "主题工作台无右侧详情面板";
      return;
    }

    els.pageEyebrow.textContent = state.mode === "section" ? "小节编辑器 · 静态原型" : "讲义编辑器 · 静态原型";
    els.pageTitle.textContent = editor.title;
    els.breadcrumb.textContent = `功能关系 / ${editor.topicTitle} / ${editor.title}`;
    els.modeStatus.textContent = state.topicNavExpanded
      ? "教学主题树已展开，点击主题才导航"
      : (state.mode === "section" ? "编辑 Section / SectionItem 占位" : "编辑 Handout / HandoutItem 占位");
    els.objectOutlineEyebrow.textContent = state.mode === "section" ? "小节结构树" : "讲义结构树";
    els.objectOutlineTitle.textContent = state.mode === "section" ? "当前小节结构" : "当前讲义结构";
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
            <button class="secondary-button" type="button" data-placeholder="新建小节方案">
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
              新建小节方案
            </button>
          </div>
          <div class="workspace-plan-list">
            ${topicWorkspace.sectionPlans.map((plan) => renderWorkspacePlanCard(plan, "section")).join("")}
          </div>
        </section>

        <section class="workspace-lane handout-workspace-card">
          <div class="workspace-lane-heading">
            <div>
              <p class="eyebrow">回答：这份材料应该怎么输出</p>
              <h3>讲义编排</h3>
            </div>
            <button class="secondary-button" type="button" data-placeholder="新建讲义">
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
              新建讲义
            </button>
          </div>
          <div class="workspace-plan-list">
            ${topicWorkspace.handouts.map((plan) => renderWorkspacePlanCard(plan, "handout")).join("")}
          </div>
        </section>
      </div>

      <div class="secondary-workspace-grid">
        <section class="workspace-secondary-card">
          <div class="workspace-lane-heading">
            <div>
              <p class="eyebrow">次级信息</p>
              <h3>内容资源摘要</h3>
            </div>
          </div>
          <div class="resource-summary-grid">
            ${topicWorkspace.resources.map((resource) => renderContentBlockCard(resource)).join("")}
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
              <h3>最近编辑</h3>
            </div>
          </div>
          <div class="summary-list">
            ${topicWorkspace.secondary.recent.map((item) => `<span>${escapeHtml(item)}</span>`).join("")}
          </div>
        </section>
      </div>
    `;
  }

  function renderTopicPlaceholder(topic) {
    return `
      <div class="topic-empty-state">
        <h3>${escapeHtml(topic.title)} 仍是下级教学主题入口</h3>
        <p>本轮假数据只展开“机械能守恒”的主题工作台，不把小节方案、讲义方案或版本记录混入教学主题树。</p>
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
      button.addEventListener("click", () => openSectionEditor(button.dataset.openSectionPlan));
    });

    els.mainStage.querySelectorAll("[data-open-handout]").forEach((button) => {
      button.addEventListener("click", () => openHandoutEditor(button.dataset.openHandout));
    });

    const mainTopicButton = els.mainStage.querySelector("[data-select-main-topic]");
    if (mainTopicButton) {
      mainTopicButton.addEventListener("click", () => {
        state.selectedTopicId = topicWorkspace.topicId;
        render();
      });
    }

    bindPlaceholderButtons(els.mainStage);
  }

  function renderEditor() {
    const editor = currentEditor();
    const selectedId = selectedNodeId();
    const selectedItem = findItem(editor.items, selectedId) || editor.items[0];
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
      <section class="editor-workspace ${state.mode === "handout" ? "is-handout-editor" : "is-section-editor"}">
        <div class="editor-header">
          <div>
            <p class="eyebrow">${state.mode === "section" ? "小节展开内容区" : "讲义展开内容区"}</p>
            <h2>${escapeHtml(editor.title)}</h2>
            <p>${escapeHtml(editor.summary)}</p>
          </div>
          <div class="editor-header-actions">
            ${state.mode === "section" ? `
              <button class="primary-button" type="button" data-export-section-word ${state.exportingSectionWord ? "disabled" : ""}>
                <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 3h8l4 4v14H6z"></path><path d="M14 3v5h5M9 15h6M9 18h6"></path></svg>
                ${state.exportingSectionWord ? "正在导出..." : "导出当前小节 Word"}
              </button>
            ` : ""}
            ${state.mode === "handout" ? `
              <button class="primary-button" type="button" data-placeholder="生成 Word">
                <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 3h8l4 4v14H6z"></path><path d="M14 3v5h5M9 15h6M9 18h6"></path></svg>
                生成 Word
              </button>
            ` : ""}
            <button class="secondary-button" type="button" data-placeholder="从资源库插入">
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M21 21l-4.3-4.3"></path><circle cx="11" cy="11" r="7"></circle></svg>
              从资源库插入
            </button>
          </div>
        </div>
        <div class="expanded-content-list">
          ${editor.items.map((item, index) => `
            ${renderInsertZone(`before-${item.id}`, index === 0 ? "添加到最前" : "插入到此处")}
            ${renderComposerItem(item, 0)}
          `).join("")}
          ${renderInsertZone("end", "添加到末尾")}
        </div>
      </section>
    `;

    bindEditorWorkspace();
    renderDetail(selectedItem);
  }

  function buildRootOutlineNode(editor) {
    const rootTitle = state.mode === "section"
      ? `小节：${editor.title}`
      : `讲义：${editor.title}`;
    return {
      id: `${state.mode}-root`,
      title: rootTitle,
      badge: state.mode === "section" ? "小节方案" : "讲义",
      meta: [editor.status],
      data: { nodeId: `${state.mode}-root`, treeKind: "object-outline-tree" },
      children: editor.items.map(mapOutlineNode),
    };
  }

  function mapOutlineNode(item) {
    return {
      id: item.id,
      title: item.title,
      badge: item.type,
      meta: [
        item.structureType,
        item.isReferenceView ? "引用展开视图" : "",
      ].filter(Boolean),
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
    const allowChildren = item.structureType === "组合块" || item.structureType === "引用展开" || item.type === "模型" || item.type === "小节引用";
    return `
      <div class="section-content-card__actions" aria-label="${escapeHtml(item.title)} 操作">
        <button class="section-action-button" type="button" data-placeholder="Word 编辑" title="Word 编辑" aria-label="Word 编辑">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 4h16v16H4z"></path><path d="M8 8h8M8 12h8M8 16h5"></path></svg>
        </button>
        <button class="section-action-button" type="button" data-placeholder="添加下级内容" title="添加下级内容" aria-label="添加下级内容" ${allowChildren ? "" : "disabled"}>
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
        openResourceSearch({
          action: button.dataset.insertAction,
          position: button.dataset.insertPosition,
        });
      });
    });

    const exportButton = els.mainStage.querySelector("[data-export-section-word]");
    if (exportButton) {
      exportButton.addEventListener("click", exportCurrentSectionWord);
    }

    bindPlaceholderButtons(els.mainStage);
  }

  function renderDetail(item) {
    if (!item) {
      els.selectedDetail.innerHTML = "<div class=\"empty-state\">选择一个内容块</div>";
      els.detailPreviewCard.innerHTML = "";
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
      </div>
      <p class="selected-summary">${escapeHtml(item.detail)}</p>
      ${item.isReferenceView ? `<p class="reference-detail-note">讲义直接拥有的是 HandoutItem；这里展开的小节内容只是引用预览，调整讲义不会反向修改源小节结构。</p>` : ""}
    `;

    els.detailPreviewCard.innerHTML = `
      <div class="detail-card-heading">
        <h3>${state.mode === "handout" ? "预览" : "内容预览"}</h3>
        <span class="status-chip muted">${item.isReferenceView ? "引用展开" : "静态 HTML"}</span>
      </div>
      <div class="preview-surface">
        <div class="preview-document">${item.html}</div>
      </div>
    `;

    bindPlaceholderButtons(els.detailPanel);
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
          ${(block.meta || []).map((item) => `<span class="content-block-card__property">${escapeHtml(item)}</span>`).join("")}
        </div>
      </article>
    `;
  }

  function renderResourceResults() {
    const keyword = els.resourceSearchInput.value.trim().toLowerCase();
    const results = resourceCandidates.filter((item) => {
      const haystack = `${item.title} ${item.type} ${item.structureType} ${item.remark} ${(item.meta || []).join(" ")}`.toLowerCase();
      return !keyword || haystack.includes(keyword);
    });

    els.resourceResults.innerHTML = results.length
      ? results.map((item) => `
        <button class="content-block-card resource-picker-card" type="button" data-resource-id="${escapeHtml(item.id)}">
          <span class="content-block-card__top">
            <strong class="content-block-card__title" title="${escapeHtml(item.title)}">${escapeHtml(item.title)}</strong>
            <span class="content-block-card__right">
              <span class="content-block-card__type content-block-card__type--${typeClass(item.type)}">${escapeHtml(item.type)}</span>
              <span class="content-block-card__structure content-block-card__structure--${structureClass(item.structureType)}">${escapeHtml(item.structureType)}</span>
            </span>
          </span>
          ${item.remark ? `<span class="content-block-card__remark">${escapeHtml(item.remark)}</span>` : ""}
          <span class="content-block-card__meta">
            ${(item.meta || []).map((meta) => `<span class="content-block-card__property">${escapeHtml(meta)}</span>`).join("")}
          </span>
        </button>
      `).join("")
      : "<p class=\"resource-empty\">没有匹配的静态候选。</p>";

    els.resourceResults.querySelectorAll("[data-resource-id]").forEach((button) => {
      button.addEventListener("click", () => {
        const item = resourceCandidates.find((candidate) => candidate.id === button.dataset.resourceId);
        closeResourceSearch();
        setActionMessage(`已选择静态候选：${item?.title || "未知内容"}。本轮只更新提示，不写入任何对象。`);
      });
    });
  }

  function selectTopic(topicId) {
    const topic = findTopic(topicId);
    if (!topic) return;
    state.selectedTopicId = topic.id;
    state.mode = "topic";
    state.topicNavExpanded = true;
    render();
  }

  function openSectionEditor(planId) {
    state.selectedSectionPlanId = planId || sectionEditor.id;
    state.mode = "section";
    state.topicNavExpanded = false;
    state.selectedSectionNodeId = state.selectedSectionNodeId || sectionEditor.items[0].id;
    setActionMessage("已进入小节编辑器静态原型。");
    render();
  }

  function openHandoutEditor(handoutId) {
    state.selectedHandoutId = handoutId || handoutEditor.id;
    state.mode = "handout";
    state.topicNavExpanded = false;
    state.selectedHandoutNodeId = state.selectedHandoutNodeId || handoutEditor.items[0].id;
    setActionMessage("已进入讲义编辑器静态原型。");
    render();
  }

  function selectEditorNode(nodeId, options = {}) {
    if (!nodeId) return;

    if (nodeId.endsWith("-root")) {
      const first = currentEditor().items[0];
      nodeId = first?.id || nodeId;
    }

    if (state.mode === "section") {
      state.selectedSectionNodeId = nodeId;
    } else {
      state.selectedHandoutNodeId = nodeId;
    }

    renderEditor();
    const item = findItem(currentEditor().items, nodeId);
    setActionMessage(`已选中：${item?.title || "结构节点"}。右侧详情面板已更新。`);

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

  function openResourceSearch(context = {}) {
    state.insertContext = context;
    els.resourceDrawer.classList.remove("is-hidden");
    renderResourceResults();
    const action = context.action === "create" ? "新建卡片" : "插入卡片";
    setActionMessage(`已打开静态资源选择器：${action}，位置：${context.position || "当前选中块"}。`);
    window.setTimeout(() => els.resourceSearchInput.focus(), 0);
  }

  function closeResourceSearch() {
    els.resourceDrawer.classList.add("is-hidden");
  }

  function scrollToContent(nodeId) {
    const node = document.getElementById(`content-${nodeId}`);
    if (node) {
      node.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  }

  function bindPlaceholderButtons(root = document) {
    if (!root) return;
    root.querySelectorAll("[data-placeholder]").forEach((button) => {
      if (button.dataset.boundPlaceholder === "true") return;
      button.dataset.boundPlaceholder = "true";
      button.addEventListener("click", (event) => {
        event.stopPropagation();
        const action = button.dataset.placeholder;
        if (action === "从资源库插入" || action === "添加下级内容") {
          openResourceSearch({ action: "insert", position: selectedNodeId() || "当前对象" });
          return;
        }
        setActionMessage(`${action} 是静态占位，本轮不调用真实逻辑。`);
      });
    });
  }

  function setActionMessage(message) {
    if (els.actionMessage) {
      els.actionMessage.textContent = message;
    }
  }

  async function exportCurrentSectionWord() {
    if (state.mode !== "section" || state.exportingSectionWord) {
      return;
    }

    state.exportingSectionWord = true;
    renderEditor();
    setActionMessage("正在导出当前小节整体 Word，不会改变当前选中块。");

    try {
      const sectionId = await resolveExportSectionId();
      if (!sectionId) {
        throw new Error("未找到可导出的真实小节。请先在真实小节列表中创建或选择对应小节。");
      }

      const response = await fetch(`${sectionApiRoot}/${encodeURIComponent(sectionId)}/导出Word`, {
        method: "POST",
      });

      if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `导出失败：HTTP ${response.status}`);
      }

      const blob = await response.blob();
      const fileName = getDownloadFileName(response.headers.get("content-disposition"))
        || `${sectionEditor.title}-${formatTimestamp(new Date())}.docx`;
      downloadBlob(blob, fileName);
      setActionMessage(`已导出当前小节整体 Word：${fileName}`);
    } catch (error) {
      setActionMessage(error instanceof Error ? error.message : "导出当前小节 Word 失败。");
    } finally {
      state.exportingSectionWord = false;
      renderEditor();
    }
  }

  async function resolveExportSectionId() {
    if (Number.isInteger(sectionEditor.apiSectionId) && sectionEditor.apiSectionId > 0) {
      return sectionEditor.apiSectionId;
    }

    const response = await fetch(`${sectionApiRoot}?关键词=${encodeURIComponent(sectionEditor.apiKeyword || sectionEditor.topicTitle)}`);
    if (!response.ok) {
      const message = await response.text();
      throw new Error(message || "读取小节列表失败，无法确认要导出的小节。");
    }

    const sections = await response.json();
    const matched = Array.isArray(sections)
      ? sections.find((section) => {
        const title = textOf(section.标题 ?? section.title ?? section.Title);
        return title.includes(sectionEditor.topicTitle) || title.includes("机械能守恒");
      }) || sections[0]
      : null;

    return idOf(matched);
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

    const fallbackMatch = contentDisposition.match(/filename="?([^";]+)"?/i);
    return fallbackMatch ? fallbackMatch[1] : "";
  }

  function idOf(value) {
    const raw = value?.Id ?? value?.id;
    const id = Number(raw);
    return Number.isInteger(id) && id > 0 ? id : null;
  }

  function textOf(value) {
    return String(value ?? "");
  }

  function formatTimestamp(date) {
    const pad = (value) => String(value).padStart(2, "0");
    return `${date.getFullYear()}${pad(date.getMonth() + 1)}${pad(date.getDate())}${pad(date.getHours())}${pad(date.getMinutes())}${pad(date.getSeconds())}`;
  }

  function currentEditor() {
    return state.mode === "handout" ? handoutEditor : sectionEditor;
  }

  function selectedNodeId() {
    return state.mode === "handout" ? state.selectedHandoutNodeId : state.selectedSectionNodeId;
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

  function typeClass(type) {
    return {
      知识点: "knowledge",
      例题: "example",
      练习: "exercise",
      方法总结: "method",
      易错点: "mistake",
      普通说明: "note",
      题目: "question",
      例题组: "group",
      模型: "method",
      小节引用: "group",
      讲义项: "note",
      内容块: "default",
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
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  function bindEvents() {
    els.backToTopicButton.addEventListener("click", () => {
      state.mode = "topic";
      state.selectedTopicId = topicWorkspace.topicId;
      state.topicNavExpanded = true;
      render();
    });

    els.topicNavToggleButton.addEventListener("click", () => {
      state.topicNavExpanded = !state.topicNavExpanded;
      render();
    });

    els.resourceSearchInput.addEventListener("input", renderResourceResults);
    els.closeResourceDrawerButton.addEventListener("click", closeResourceSearch);
    els.resourceDrawer.addEventListener("click", (event) => {
      if (event.target === els.resourceDrawer) {
        closeResourceSearch();
      }
    });
  }

  function init() {
    bindEvents();
    render();
    renderResourceResults();
  }

  init();
})();
