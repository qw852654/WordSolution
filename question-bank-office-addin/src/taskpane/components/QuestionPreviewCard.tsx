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
    width: "100%",
    minWidth: 0,
    boxSizing: "border-box",
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#fffdf9",
    border: "1px solid #b98a41",
    overflow: "hidden",
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
  footer: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "12px",
    flexWrap: "wrap",
    marginTop: "12px",
  },
  noteText: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: "0",
    color: "#6f675b",
  },
  actionButton: {
    padding: "8px 12px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    backgroundColor: "#ffffff",
    color: "#2f2a25",
    cursor: "pointer",
    fontSize: "12px",
  },
});

export default function QuestionPreviewCard(props: QuestionPreviewCardProps) {
  const styles = useStyles();

  return (
    <div className={styles.card}>
      {props.标签文本列表.length > 0 && (
        <div className={styles.tagRow}>
          {props.标签文本列表.map((标签文本) => (
            <TagBadge key={`${props.题目ID}-${标签文本}`} 文本={标签文本} />
          ))}
        </div>
      )}
      {props.描述 && <p className={styles.description}>{props.描述}</p>}
      <div className={styles.preview} dangerouslySetInnerHTML={{ __html: props.预览Html }} />
      <div className={styles.footer}>
        <span className={styles.noteText}>题目ID：{props.题目ID}</span>
        <button type="button" className={styles.actionButton} onClick={props.切换选择}>
          {props.已选中 ? "取消选择" : "选择题目"}
        </button>
      </div>
    </div>
  );
}
