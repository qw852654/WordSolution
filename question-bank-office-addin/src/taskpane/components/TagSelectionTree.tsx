import * as React from "react";
import { Tree, TreeItem, TreeItemLayout, makeStyles } from "@fluentui/react-components";

interface TagTreeNode {
  id: number;
  名称: string;
  description?: string | null;
  子标签列表: TagTreeNode[];
}

interface TagSelectionTreeProps {
  树名称: string;
  标签列表: TagTreeNode[];
  已选标签ID列表: number[];
  获取标签显示文本: (标签: TagTreeNode) => string;
  切换标签: (标签ID: number) => void;
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
  itemContent: {
    display: "grid",
    gap: "4px",
    minWidth: 0,
  },
  tagName: {
    fontSize: "13px",
    fontWeight: "600",
    color: "#2f2b26",
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
  },
  meta: {
    fontSize: "12px",
    color: "#756d60",
    lineHeight: "18px",
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
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
  selectedActionButton: {
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
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

export default function TagSelectionTree(props: TagSelectionTreeProps) {
  const styles = useStyles();
  const 已选标签ID集合 = React.useMemo(() => new Set(props.已选标签ID列表), [props.已选标签ID列表]);
  const 默认展开项 = React.useMemo<number[]>(() => [], []);
  const 选中路径展开项 = React.useMemo(
    () => 收集选中路径展开节点ID(props.标签列表, 已选标签ID集合),
    [props.标签列表, 已选标签ID集合]
  );
  const [展开项集合, 设置展开项集合] = React.useState<Set<number>>(new Set(默认展开项));

  React.useEffect(() => {
    设置展开项集合(new Set(默认展开项));
  }, [默认展开项]);

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

  const 处理展开变化 = React.useCallback((_: unknown, 数据: { openItems: Set<string | number> }) => {
    设置展开项集合(new Set(Array.from(数据.openItems, (标签ID) => Number(标签ID))));
  }, []);

  const 渲染节点 = React.useCallback(
    (标签: TagTreeNode): React.ReactNode => {
      const 有子标签 = 标签.子标签列表.length > 0;
      const 已选中 = 已选标签ID集合.has(标签.id);
      return (
        <TreeItem key={标签.id} itemType={有子标签 ? "branch" : "leaf"} value={标签.id}>
          <TreeItemLayout
            actions={{
              visible: true,
              children: (
                <button
                  type="button"
                  className={`${styles.actionButton} ${已选中 ? styles.selectedActionButton : ""}`}
                  onClick={(事件) => {
                    事件.preventDefault();
                    事件.stopPropagation();
                    props.切换标签(标签.id);
                  }}
                >
                  {已选中 ? "已选中" : "选择"}
                </button>
              ),
            }}
          >
            <div className={styles.itemContent}>
              <span className={styles.tagName}>{props.获取标签显示文本(标签)}</span>
              {标签.description && <span className={styles.meta}>{标签.description}</span>}
            </div>
          </TreeItemLayout>
          {有子标签 ? <Tree>{标签.子标签列表.map((子标签) => 渲染节点(子标签))}</Tree> : null}
        </TreeItem>
      );
    },
    [props, styles.actionButton, styles.itemContent, styles.meta, styles.selectedActionButton, styles.tagName, 已选标签ID集合]
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
      className={styles.tree}
    >
      {props.标签列表.map((标签) => 渲染节点(标签))}
    </Tree>
  );
}


