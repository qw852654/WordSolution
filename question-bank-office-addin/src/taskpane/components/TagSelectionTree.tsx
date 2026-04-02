import * as React from "react";
import { Tree, TreeItem, TreeItemLayout, makeStyles } from "@fluentui/react-components";

interface TagTreeNode {
  id: number;
  名称: string;
  description?: string | null;
  子标签列表: TagTreeNode[];
}

type 放置方式 = "before" | "after" | "inside";

interface 拖拽落点 {
  目标标签ID: number;
  放置方式: 放置方式;
}

interface TagSelectionTreeProps {
  树名称: string;
  标签列表: TagTreeNode[];
  已选标签ID列表: number[];
  高亮标签ID?: number | null;
  紧凑模式?: boolean;
  显示选择按钮?: boolean;
  展开指令?: {
    展开: boolean;
    令牌: number;
  };
  获取标签显示文本: (标签: TagTreeNode) => string;
  切换标签: (标签ID: number) => void;
  移动标签?: (拖动标签ID: number, 目标标签ID: number, 放置方式: 放置方式) => Promise<void>;
  渲染节点附加操作?: (标签: TagTreeNode) => React.ReactNode;
  渲染节点下方内容?: (标签: TagTreeNode) => React.ReactNode;
  选择按钮文案?: {
    未选中: string;
    已选中: string;
  };
  空提示文本?: string;
}

const useStyles = makeStyles({
  emptyText: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: 0,
    color: "#6f675b",
  },
  tree: {
    rowGap: "4px",
  },
  compactTree: {
    rowGap: "2px",
  },
  childTree: {
    marginLeft: "14px",
    paddingLeft: "12px",
    borderLeft: "1px solid #ead8b7",
  },
  compactChildTree: {
    marginLeft: "2px",
    paddingLeft: "4px",
  },
  itemContent: {
    display: "grid",
    gap: "4px",
    minWidth: 0,
  },
  itemContentButton: {
    display: "grid",
    gap: "4px",
    minWidth: 0,
    padding: "6px 8px",
    width: "100%",
    textAlign: "left",
    borderRadius: "10px",
    cursor: "pointer",
    transition:
      "background-color 120ms ease, box-shadow 120ms ease, border-color 120ms ease, transform 120ms ease",
    border: "1px solid transparent",
    appearance: "none",
    backgroundColor: "transparent",
    ":hover": {
      backgroundColor: "#fff6e4",
      boxShadow: "0 2px 8px rgba(90, 65, 20, 0.08)",
      transform: "translateY(-1px)",
    },
    ":focus-visible": {
      outline: "2px solid #d79b27",
      outlineOffset: "2px",
    },
  },
  compactItemContentButton: {
    gap: "2px",
    padding: "4px 6px",
    borderRadius: "8px",
  },
  compactTreeItemLayout: {
    paddingLeft: "0 !important",
  },
  selectedItemContentButton: {
    backgroundColor: "#f3c86a",
    border: "1px solid #b8860b",
    boxShadow: "0 4px 10px rgba(160, 112, 9, 0.18)",
    transform: "translateY(-1px)",
    color: "#3b2a00",
    "& span": {
      color: "#3b2a00",
    },
  },
  highlightedItemContentButton: {
    border: "1px solid #d79b27",
    backgroundColor: "#fff6dd",
    boxShadow:
      "0 0 0 2px rgba(215, 155, 39, 0.14), 0 8px 16px rgba(160, 112, 9, 0.10)",
  },
  dropBeforeItemContentButton: {
    boxShadow: "inset 0 3px 0 #d79b27",
  },
  dropAfterItemContentButton: {
    boxShadow: "inset 0 -3px 0 #d79b27",
  },
  dropInsideItemContentButton: {
    border: "1px solid #d79b27",
    backgroundColor: "#fff3d6",
    boxShadow: "0 0 0 2px rgba(215, 155, 39, 0.16)",
  },
  draggingItemContentButton: {
    opacity: 0.7,
  },
  tagName: {
    fontSize: "13px",
    fontWeight: "600",
    color: "#2f2b26",
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
  },
  compactTagName: {
    fontSize: "12px",
    lineHeight: "16px",
  },
  meta: {
    fontSize: "12px",
    color: "#756d60",
    lineHeight: "18px",
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
  },
  compactMeta: {
    fontSize: "11px",
    lineHeight: "16px",
  },
  actionButton: {
    padding: "6px 10px",
    borderRadius: "999px",
    border: "1px solid #dfd3bc",
    backgroundColor: "#ffffff",
    color: "#524c43",
    cursor: "pointer",
    fontSize: "12px",
  },
  compactActionButton: {
    padding: "4px 6px",
    fontSize: "11px",
    lineHeight: "12px",
  },
  selectedActionButton: {
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
  },
  actionGroup: {
    display: "flex",
    gap: "6px",
    flexWrap: "wrap",
    alignItems: "center",
  },
  compactActionGroup: {
    gap: "4px",
  },
  dragHandle: {
    padding: "6px 8px",
    borderRadius: "999px",
    border: "1px dashed #d8cfc0",
    backgroundColor: "#ffffff",
    color: "#6b655b",
    cursor: "move",
    fontSize: "12px",
    lineHeight: "14px",
    userSelect: "none",
  },
  compactDragHandle: {
    padding: "4px 6px",
    fontSize: "11px",
    lineHeight: "12px",
  },
  draggingHandle: {
    cursor: "move",
    border: "1px dashed #d79b27",
    backgroundColor: "#fff6dd",
    color: "#6a5600",
  },
});

function 收集选中路径展开节点ID(标签列表: TagTreeNode[], 已选标签ID集合: Set<number>) {
  const 展开节点ID集合 = new Set<number>();

  const 遍历 = (当前标签列表: TagTreeNode[], 祖先路径: number[]) => {
    当前标签列表.forEach((标签) => {
      const 当前路径 = [...祖先路径, 标签.id];
      if (已选标签ID集合.has(标签.id)) {
        当前路径.forEach((标签ID) => 展开节点ID集合.add(标签ID));
      }
      if (标签.子标签列表.length > 0) {
        遍历(标签.子标签列表, 当前路径);
      }
    });
  };

  遍历(标签列表, []);
  return Array.from(展开节点ID集合);
}

function 收集全部可展开节点ID(标签列表: TagTreeNode[]) {
  const 展开节点ID集合 = new Set<number>();

  const 遍历 = (当前标签列表: TagTreeNode[]) => {
    当前标签列表.forEach((标签) => {
      if (标签.子标签列表.length > 0) {
        展开节点ID集合.add(标签.id);
        遍历(标签.子标签列表);
      }
    });
  };

  遍历(标签列表);
  return Array.from(展开节点ID集合);
}

function 收集后代映射(标签列表: TagTreeNode[]) {
  const 映射 = new Map<number, Set<number>>();

  const 遍历 = (标签: TagTreeNode): Set<number> => {
    const 当前后代集合 = new Set<number>();
    标签.子标签列表.forEach((子标签) => {
      当前后代集合.add(子标签.id);
      const 子后代集合 = 遍历(子标签);
      子后代集合.forEach((标签ID) => 当前后代集合.add(标签ID));
    });
    映射.set(标签.id, 当前后代集合);
    return 当前后代集合;
  };

  标签列表.forEach((标签) => {
    遍历(标签);
  });

  return 映射;
}

function 查找标签路径(标签列表: TagTreeNode[], 目标标签ID: number, 当前路径: number[] = []): number[] | null {
  for (const 标签 of 标签列表) {
    const 新路径 = [...当前路径, 标签.id];
    if (标签.id === 目标标签ID) {
      return 新路径;
    }
    if (标签.子标签列表.length > 0) {
      const 子路径 = 查找标签路径(标签.子标签列表, 目标标签ID, 新路径);
      if (子路径) {
        return 子路径;
      }
    }
  }
  return null;
}

function 计算放置方式(元素: HTMLElement, clientY: number): 放置方式 {
  const rect = 元素.getBoundingClientRect();
  const ratio = rect.height <= 0 ? 0.5 : (clientY - rect.top) / rect.height;

  if (ratio <= 0.25) {
    return "before";
  }

  if (ratio >= 0.75) {
    return "after";
  }

  return "inside";
}

export default function TagSelectionTree(props: TagSelectionTreeProps) {
  const styles = useStyles();
  const 是紧凑模式 = props.紧凑模式 === true;
  const 显示选择按钮 = props.显示选择按钮 !== false;
  const 已选标签ID集合 = React.useMemo(() => new Set(props.已选标签ID列表), [props.已选标签ID列表]);
  const 默认展开项 = React.useMemo<number[]>(() => [], []);
  const 全部可展开项 = React.useMemo(
    () => 收集全部可展开节点ID(props.标签列表),
    [props.标签列表]
  );
  const 选中路径展开项 = React.useMemo(
    () => 收集选中路径展开节点ID(props.标签列表, 已选标签ID集合),
    [props.标签列表, 已选标签ID集合]
  );
  const 后代映射 = React.useMemo(() => 收集后代映射(props.标签列表), [props.标签列表]);
  const [展开项集合, 设置展开项集合] = React.useState<Set<number>>(new Set(默认展开项));
  const [拖拽中标签ID, 设置拖拽中标签ID] = React.useState<number | null>(null);
  const [当前落点, 设置当前落点] = React.useState<拖拽落点 | null>(null);
  const [正在提交移动, 设置正在提交移动] = React.useState(false);
  const [待展开移动标签ID, 设置待展开移动标签ID] = React.useState<number | null>(null);

  React.useEffect(() => {
    设置展开项集合(new Set(默认展开项));
  }, [默认展开项]);

  React.useEffect(() => {
    if (!props.展开指令) {
      return;
    }
    设置展开项集合(new Set(props.展开指令.展开 ? 全部可展开项 : []));
  }, [props.展开指令, 全部可展开项]);

  React.useEffect(() => {
    设置展开项集合((当前展开项集合) => {
      const 新展开项集合 = new Set(当前展开项集合);
      let 已变化 = false;
      选中路径展开项.forEach((标签ID) => {
        if (!新展开项集合.has(标签ID)) {
          新展开项集合.add(标签ID);
          已变化 = true;
        }
      });
      return 已变化 ? 新展开项集合 : 当前展开项集合;
    });
  }, [选中路径展开项]);

  React.useEffect(() => {
    if (待展开移动标签ID === null) {
      return;
    }

    const 移动后路径 = 查找标签路径(props.标签列表, 待展开移动标签ID);
    if (!移动后路径) {
      return;
    }

    设置展开项集合((当前展开项集合) => {
      const 新展开项集合 = new Set(当前展开项集合);
      let 已变化 = false;
      移动后路径.forEach((标签ID) => {
        if (!新展开项集合.has(标签ID)) {
          新展开项集合.add(标签ID);
          已变化 = true;
        }
      });
      return 已变化 ? 新展开项集合 : 当前展开项集合;
    });
    设置待展开移动标签ID(null);
  }, [props.标签列表, 待展开移动标签ID]);

  const 处理展开变化 = React.useCallback((_: unknown, 数据: { openItems: Set<string | number> }) => {
    设置展开项集合(new Set(Array.from(数据.openItems, (标签ID) => Number(标签ID))));
  }, []);

  const 选择按钮文案 = props.选择按钮文案 ?? {
    未选中: "选择",
    已选中: "已选中",
  };

  const 可以放置到目标 = React.useCallback(
    (源标签ID: number, 目标标签ID: number) => {
      if (源标签ID === 目标标签ID) {
        return false;
      }

      return !后代映射.get(源标签ID)?.has(目标标签ID);
    },
    [后代映射]
  );

  const 重置拖拽状态 = React.useCallback(() => {
    设置拖拽中标签ID(null);
    设置当前落点(null);
    设置正在提交移动(false);
  }, []);

  const 处理拖拽开始 = React.useCallback(
    (事件: React.DragEvent<HTMLButtonElement>, 标签ID: number) => {
      if (!props.移动标签 || 正在提交移动) {
        事件.preventDefault();
        return;
      }

      事件.stopPropagation();
      事件.dataTransfer.effectAllowed = "move";
      事件.dataTransfer.setData("text/plain", String(标签ID));
      设置拖拽中标签ID(标签ID);
      设置当前落点(null);
    },
    [props.移动标签, 正在提交移动]
  );

  const 处理拖拽结束 = React.useCallback(() => {
    重置拖拽状态();
  }, [重置拖拽状态]);

  const 处理拖拽悬停 = React.useCallback(
    (事件: React.DragEvent<HTMLButtonElement>, 目标标签ID: number) => {
      if (!props.移动标签 || 拖拽中标签ID === null || 正在提交移动) {
        return;
      }

      const 放置方式 = 计算放置方式(事件.currentTarget, 事件.clientY);
      if (!可以放置到目标(拖拽中标签ID, 目标标签ID)) {
        if (当前落点?.目标标签ID === 目标标签ID) {
          设置当前落点(null);
        }
        return;
      }

      事件.preventDefault();
      事件.dataTransfer.dropEffect = "move";
      const 新落点 = { 目标标签ID, 放置方式 };
      if (
        当前落点?.目标标签ID !== 新落点.目标标签ID ||
        当前落点.放置方式 !== 新落点.放置方式
      ) {
        设置当前落点(新落点);
      }
    },
    [props.移动标签, 拖拽中标签ID, 正在提交移动, 可以放置到目标, 当前落点]
  );

  const 处理放下 = React.useCallback(
    async (事件: React.DragEvent<HTMLButtonElement>, 目标标签ID: number) => {
      if (!props.移动标签 || 拖拽中标签ID === null || 正在提交移动) {
        return;
      }

      const 放置方式 = 计算放置方式(事件.currentTarget, 事件.clientY);
      if (!可以放置到目标(拖拽中标签ID, 目标标签ID)) {
        重置拖拽状态();
        return;
      }

      事件.preventDefault();
      事件.stopPropagation();
      设置正在提交移动(true);
      设置当前落点({ 目标标签ID, 放置方式 });
      设置待展开移动标签ID(拖拽中标签ID);

      try {
        await props.移动标签(拖拽中标签ID, 目标标签ID, 放置方式);
      } finally {
        重置拖拽状态();
      }
    },
    [props.移动标签, 拖拽中标签ID, 正在提交移动, 可以放置到目标, 重置拖拽状态]
  );

  const 渲染节点 = React.useCallback(
    (标签: TagTreeNode, 层级深度 = 0): React.ReactNode => {
      const 有子标签 = 标签.子标签列表.length > 0;
      const 已选中 = 已选标签ID集合.has(标签.id);
      const 当前高亮 = props.高亮标签ID === 标签.id;
      const 附加操作 = props.渲染节点附加操作?.(标签);
      const 节点下方内容 = props.渲染节点下方内容?.(标签);
      const 正在拖拽当前节点 = 拖拽中标签ID === 标签.id;
      const 当前节点落点 = 当前落点?.目标标签ID === 标签.id ? 当前落点.放置方式 : null;

      return (
        <TreeItem key={标签.id} itemType={有子标签 ? "branch" : "leaf"} value={标签.id}>
          <TreeItemLayout
            className={是紧凑模式 ? styles.compactTreeItemLayout : undefined}
            actions={{
              visible: true,
              children: (
                <div className={`${styles.actionGroup} ${是紧凑模式 ? styles.compactActionGroup : ""}`}>
                  {props.移动标签 && (
                    <button
                      type="button"
                      draggable={!正在提交移动}
                      className={`${styles.dragHandle} ${是紧凑模式 ? styles.compactDragHandle : ""} ${正在拖拽当前节点 ? styles.draggingHandle : ""}`}
                      title="拖拽移动标签"
                      aria-label="拖拽移动标签"
                      onPointerDownCapture={(事件) => {
                        事件.stopPropagation();
                      }}
                      onMouseDownCapture={(事件) => {
                        事件.stopPropagation();
                      }}
                      onClick={(事件) => {
                        事件.preventDefault();
                        事件.stopPropagation();
                      }}
                      onDragStart={(事件) => 处理拖拽开始(事件, 标签.id)}
                      onDragEnd={处理拖拽结束}
                    >
                      拖
                    </button>
                  )}
                  {显示选择按钮 && (
                    <button
                      type="button"
                      className={`${styles.actionButton} ${是紧凑模式 ? styles.compactActionButton : ""} ${已选中 ? styles.selectedActionButton : ""}`}
                      onClick={(事件) => {
                        事件.preventDefault();
                        事件.stopPropagation();
                        props.切换标签(标签.id);
                      }}
                    >
                      {已选中 ? 选择按钮文案.已选中 : 选择按钮文案.未选中}
                    </button>
                  )}
                  {附加操作}
                </div>
              ),
            }}
          >
            <button
              type="button"
              className={`${styles.itemContentButton} ${是紧凑模式 ? styles.compactItemContentButton : ""} ${已选中 ? styles.selectedItemContentButton : ""} ${当前高亮 ? styles.highlightedItemContentButton : ""} ${当前节点落点 === "before" ? styles.dropBeforeItemContentButton : ""} ${当前节点落点 === "after" ? styles.dropAfterItemContentButton : ""} ${当前节点落点 === "inside" ? styles.dropInsideItemContentButton : ""} ${正在拖拽当前节点 ? styles.draggingItemContentButton : ""}`}
              onPointerDownCapture={(事件) => {
                事件.stopPropagation();
              }}
              onMouseDownCapture={(事件) => {
                事件.stopPropagation();
              }}
              onDragOver={(事件) => 处理拖拽悬停(事件, 标签.id)}
              onDrop={(事件) => void 处理放下(事件, 标签.id)}
              onDragLeave={(事件) => {
                const relatedTarget = 事件.relatedTarget as Node | null;
                if (!relatedTarget || !事件.currentTarget.contains(relatedTarget)) {
                  设置当前落点((当前值) =>
                    当前值?.目标标签ID === 标签.id ? null : 当前值
                  );
                }
              }}
              onClick={(事件) => {
                事件.preventDefault();
                事件.stopPropagation();
                props.切换标签(标签.id);
              }}
              onKeyDown={(事件) => {
                if (事件.key === "Enter" || 事件.key === " ") {
                  事件.preventDefault();
                  事件.stopPropagation();
                  props.切换标签(标签.id);
                }
              }}
            >
              <div className={styles.itemContent}>
                <span className={`${styles.tagName} ${是紧凑模式 ? styles.compactTagName : ""}`}>
                  {props.获取标签显示文本(标签)}
                </span>
                {标签.description && (
                  <span className={`${styles.meta} ${是紧凑模式 ? styles.compactMeta : ""}`}>
                    {标签.description}
                  </span>
                )}
              </div>
            </button>
          </TreeItemLayout>
          {节点下方内容}
          {有子标签 ? (
            <Tree
              className={
                层级深度 >= 0
                  ? `${styles.childTree} ${是紧凑模式 ? styles.compactChildTree : ""}`
                  : undefined
              }
            >
              {标签.子标签列表.map((子标签) => 渲染节点(子标签, 层级深度 + 1))}
            </Tree>
          ) : null}
        </TreeItem>
      );
    },
    [
      props,
      styles.actionButton,
      styles.actionGroup,
      styles.childTree,
      styles.dragHandle,
      styles.draggingHandle,
      styles.draggingItemContentButton,
      styles.dropAfterItemContentButton,
      styles.dropBeforeItemContentButton,
      styles.dropInsideItemContentButton,
      styles.highlightedItemContentButton,
      styles.itemContent,
      styles.itemContentButton,
      styles.meta,
      styles.selectedItemContentButton,
      styles.selectedActionButton,
      styles.tagName,
      已选标签ID集合,
      props.高亮标签ID,
      选择按钮文案.已选中,
      选择按钮文案.未选中,
      拖拽中标签ID,
      当前落点,
      正在提交移动,
      处理拖拽开始,
      处理拖拽结束,
      处理拖拽悬停,
      处理放下,
    ]
  );

  if (props.标签列表.length === 0) {
    return <p className={styles.emptyText}>{props.空提示文本 ?? "当前没有可选标签。"}</p>;
  }

  return (
    <Tree
      aria-label={props.树名称}
      appearance="transparent"
      size="small"
      openItems={展开项集合}
      onOpenChange={处理展开变化}
      className={`${styles.tree} ${是紧凑模式 ? styles.compactTree : ""}`}
    >
      {props.标签列表.map((标签) => 渲染节点(标签))}
    </Tree>
  );
}
