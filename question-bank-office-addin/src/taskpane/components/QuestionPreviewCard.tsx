import * as React from "react";
import { makeStyles } from "@fluentui/react-components";
import TagBadge from "./TagBadge";

type 删除按钮阶段 = "默认" | "确认" | "最终确认";

interface QuestionPreviewCardProps {
  题目ID: number;
  描述?: string | null;
  标签文本列表: string[];
  预览Html: string;
  已选中: boolean;
  切换选择: () => void;
  删除按钮阶段: 删除按钮阶段;
  正在删除: boolean;
  点击删除按钮: () => void;
}

const useStyles = makeStyles({
  card: {
    position: "relative",
    width: "100%",
    minWidth: 0,
    boxSizing: "border-box",
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#fffdf9",
    border: "1px solid #b98a41",
    overflow: "hidden",
    boxShadow: "0 1px 3px rgba(90, 65, 20, 0.08)",
    transition: "border-color 160ms ease, box-shadow 160ms ease, background-color 160ms ease, transform 160ms ease",
    cursor: "pointer",
    outline: "none",
    ":hover": {
      border: "1px solid #b27316",
      boxShadow: "0 6px 14px rgba(90, 65, 20, 0.12)",
    },
    ":focus-visible": {
      boxShadow: "0 0 0 3px rgba(201, 120, 0, 0.22), 0 10px 18px rgba(90, 65, 20, 0.12)",
    },
  },
  selectedCard: {
    backgroundColor: "#fff6db",
    border: "2px solid #c97800",
    boxShadow: "0 0 0 2px rgba(201, 120, 0, 0.15), 0 10px 18px rgba(90, 65, 20, 0.12)",
    transform: "translateY(-1px)",
  },
  headerRow: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "8px",
    flexWrap: "wrap",
    marginBottom: "10px",
  },
  headerActions: {
    display: "flex",
    alignItems: "center",
    justifyContent: "flex-end",
    gap: "8px",
    flexWrap: "wrap",
    marginLeft: "auto",
  },
  selectedBadge: {
    padding: "4px 8px",
    borderRadius: "999px",
    backgroundColor: "#c97800",
    color: "#fff8e8",
    fontSize: "11px",
    fontWeight: "700",
  },
  deleteButton: {
    padding: "6px 10px",
    borderRadius: "999px",
    border: "1px solid #d0c5b2",
    backgroundColor: "#ffffff",
    color: "#5b5144",
    fontSize: "12px",
    fontWeight: 600,
    cursor: "pointer",
    transition: "background-color 160ms ease, border-color 160ms ease, color 160ms ease, box-shadow 160ms ease",
  },
  deleteButtonConfirm: {
    border: "1px solid #d92d20",
    backgroundColor: "#fef3f2",
    color: "#b42318",
    boxShadow: "0 0 0 1px rgba(217, 45, 32, 0.08)",
  },
  deleteButtonFinal: {
    border: "1px solid #b42318",
    backgroundColor: "#d92d20",
    color: "#fff4f2",
    boxShadow: "0 6px 14px rgba(180, 35, 24, 0.2)",
  },
  tagRow: {
    display: "flex",
    gap: "6px",
    flexWrap: "wrap",
    marginBottom: "10px",
  },
  description: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: "0 0 10px 0",
    color: "#6f675b",
    wordBreak: "break-word",
  },
  preview: {
    minWidth: 0,
    maxWidth: "100%",
    paddingTop: "12px",
    borderTop: "1px solid #e5d2b5",
    color: "#1f1f1f",
    overflowX: "auto",
    wordBreak: "break-word",
    "& img": {
      maxWidth: "100%",
      height: "auto",
    },
    "& svg": {
      maxWidth: "100%",
      height: "auto",
    },
    "& canvas": {
      maxWidth: "100%",
      height: "auto",
    },
    "& table": {
      maxWidth: "100%",
      width: "100%",
      tableLayout: "fixed",
    },
  },
});

function 获取删除按钮文本(阶段: 删除按钮阶段, 正在删除: boolean) {
  if (正在删除) {
    return "正在删除...";
  }

  if (阶段 === "确认") {
    return "确认删除";
  }

  if (阶段 === "最终确认") {
    return "最终确认";
  }

  return "删除题目";
}

export default function QuestionPreviewCard(props: QuestionPreviewCardProps) {
  const styles = useStyles();

  return (
    <div
      className={`${styles.card} ${props.已选中 ? styles.selectedCard : ""}`}
      role="button"
      tabIndex={0}
      aria-pressed={props.已选中}
      onClick={props.切换选择}
      onKeyDown={(事件) => {
        if (事件.key === "Enter" || 事件.key === " ") {
          事件.preventDefault();
          props.切换选择();
        }
      }}
    >
      <div className={styles.headerRow}>
        <div className={styles.headerActions}>
          {props.已选中 && <span className={styles.selectedBadge}>已选中</span>}
          <button
            type="button"
            className={`${styles.deleteButton} ${props.删除按钮阶段 === "确认" ? styles.deleteButtonConfirm : ""} ${
              props.删除按钮阶段 === "最终确认" ? styles.deleteButtonFinal : ""
            }`}
            onClick={(事件) => {
              事件.preventDefault();
              事件.stopPropagation();
              props.点击删除按钮();
            }}
            disabled={props.正在删除}
          >
            {获取删除按钮文本(props.删除按钮阶段, props.正在删除)}
          </button>
        </div>
      </div>
      {props.标签文本列表.length > 0 && (
        <div className={styles.tagRow}>
          {props.标签文本列表.map((标签文本) => (
            <TagBadge key={`${props.题目ID}-${标签文本}`} 文本={标签文本} />
          ))}
        </div>
      )}
      {props.描述 && <p className={styles.description}>{props.描述}</p>}
      <div className={styles.preview} dangerouslySetInnerHTML={{ __html: props.预览Html }} />
    </div>
  );
}
