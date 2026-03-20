import * as React from "react";
import { makeStyles } from "@fluentui/react-components";
import TagBadge from "./TagBadge";

interface QuestionPreviewCardProps {
  题目ID: number;
  描述?: string | null;
  标签文本列表: string[];
  预览Html: string;
  已选中: boolean;
  切换选择: () => void;
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
  selectedBadge: {
    position: "absolute",
    top: "12px",
    right: "12px",
    padding: "4px 8px",
    borderRadius: "999px",
    backgroundColor: "#c97800",
    color: "#fff8e8",
    fontSize: "11px",
    fontWeight: "700",
  },
  tagRow: {
    display: "flex",
    gap: "6px",
    flexWrap: "wrap",
    marginBottom: "10px",
    paddingRight: "64px",
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
      {props.已选中 && <span className={styles.selectedBadge}>已选中</span>}
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
