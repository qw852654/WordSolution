import * as React from "react";
import { makeStyles } from "@fluentui/react-components";

interface 选项项 {
  id: string | number;
  名称: string;
}

interface SingleSelectChipGroupProps {
  选项列表: 选项项[];
  当前选中ID: string | number | null;
  选择选项: (id: string | number) => void;
  空提示文本?: string;
}

const useStyles = makeStyles({
  row: {
    display: "flex",
    gap: "8px",
    flexWrap: "wrap",
  },
  chip: {
    padding: "8px 12px",
    borderRadius: "999px",
    border: "1px solid #dfd3bc",
    backgroundColor: "#ffffff",
    color: "#524c43",
    cursor: "pointer",
    fontSize: "12px",
    transition: "background-color 0.15s, border-color 0.15s, color 0.15s, box-shadow 0.15s",
  },
  selectedChip: {
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
    boxShadow: "inset 0 0 0 1px rgba(184, 134, 11, 0.18)",
  },
  noteText: {
    margin: 0,
    fontSize: "12px",
    lineHeight: "18px",
    color: "#756d60",
  },
});

export default function SingleSelectChipGroup(props: SingleSelectChipGroupProps) {
  const styles = useStyles();

  if (props.选项列表.length === 0) {
    return <p className={styles.noteText}>{props.空提示文本 ?? "当前没有可选项。"}</p>;
  }

  return (
    <div className={styles.row}>
      {props.选项列表.map((选项) => {
        const 已选中 = props.当前选中ID === 选项.id;
        return (
          <button
            key={选项.id}
            type="button"
            className={`${styles.chip} ${已选中 ? styles.selectedChip : ""}`}
            aria-pressed={已选中}
            onClick={() => props.选择选项(选项.id)}
          >
            {选项.名称}
          </button>
        );
      })}
    </div>
  );
}
