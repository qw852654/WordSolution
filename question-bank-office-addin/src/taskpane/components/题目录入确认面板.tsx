import * as React from "react";
import { makeStyles } from "@fluentui/react-components";
import SingleSelectChipGroup from "./SingleSelectChipGroup";

interface 题型选项项 {
  id: number;
  名称: string;
}

interface 题目录入确认面板Props {
  描述: string;
  更新描述: (值: string) => void;
  预览Html: string;
  推荐题型名称?: string | null;
  识别说明: string;
  置信度: number;
  当前题型ID: number | null;
  题型列表: 题型选项项[];
  选择题型: (题型ID: number) => void;
  当前难度ID: number | null;
  难度选项列表: 题型选项项[];
  选择难度: (标签ID: number | null) => void;
  标签检查区: React.ReactNode;
  正在确认: boolean;
  确认录入: () => void;
  返回编辑: () => void;
}

const useStyles = makeStyles({
  panel: {
    display: "grid",
    gap: "14px",
  },
  section: {
    padding: "14px",
    borderRadius: "14px",
    backgroundColor: "rgba(255, 251, 244, 0.96)",
    border: "1px solid #e8dcc8",
    boxShadow: "0 12px 28px rgba(110, 82, 35, 0.08)",
    display: "grid",
    gap: "8px",
  },
  title: {
    margin: 0,
    fontSize: "16px",
    fontWeight: 600,
    color: "#2d2a26",
  },
  note: {
    margin: 0,
    fontSize: "12px",
    lineHeight: "18px",
    color: "#6f675b",
  },
  descriptionBox: {
    width: "100%",
    padding: "12px",
    borderRadius: "10px",
    backgroundColor: "#fffaf0",
    border: "1px solid #ead8b9",
    fontSize: "13px",
    lineHeight: "20px",
    color: "#3f3a33",
    whiteSpace: "pre-wrap",
    wordBreak: "break-word",
  },
  textArea: {
    width: "100%",
    minHeight: "96px",
    padding: "12px",
    borderRadius: "10px",
    border: "1px solid #d8cfc0",
    boxSizing: "border-box",
    fontSize: "13px",
    lineHeight: "20px",
    color: "#3f3a33",
    resize: "vertical",
    backgroundColor: "#fffdf8",
  },
  coreGrid: {
    display: "grid",
    gap: "12px",
    gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
  },
  actionRow: {
    display: "flex",
    gap: "10px",
    flexWrap: "wrap",
  },
  button: {
    padding: "10px 14px",
    borderRadius: "8px",
    border: "1px solid #c58b2a",
    backgroundImage: "linear-gradient(180deg, #f7ce77 0%, #efbd57 100%)",
    color: "#3b2a00",
    cursor: "pointer",
    fontSize: "13px",
    boxShadow: "0 8px 16px rgba(160, 112, 9, 0.18)",
  },
  secondaryButton: {
    padding: "8px 12px",
    borderRadius: "8px",
    border: "1px solid #ddcfbb",
    backgroundColor: "rgba(255, 253, 248, 0.98)",
    color: "#3a342d",
    cursor: "pointer",
  },
  preview: {
    minWidth: 0,
    maxWidth: "100%",
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

export default function 题目录入确认面板(props: 题目录入确认面板Props) {
  const styles = useStyles();

  return (
    <div className={styles.panel}>
      <div className={styles.section}>
        <h2 className={styles.title}>题目预览</h2>
        <div className={styles.preview} dangerouslySetInnerHTML={{ __html: props.预览Html }} />
      </div>

      <div className={styles.section}>
        <h2 className={styles.title}>题目描述</h2>
        <textarea
          className={styles.textArea}
          value={props.描述}
          onChange={(事件) => props.更新描述(事件.target.value)}
        />
      </div>

      <div className={styles.section}>
        <div className={styles.coreGrid}>
          <div className={styles.descriptionBox}>
            <h2 className={styles.title}>题型</h2>
            <p className={styles.note}>
              推荐题型：{props.推荐题型名称 ?? "暂未给出推荐"}　置信度：{props.置信度.toFixed(2)}
            </p>
            <p className={styles.note}>识别说明：{props.识别说明 || "暂无说明"}</p>
            <SingleSelectChipGroup
              选项列表={props.题型列表}
              当前选中ID={props.当前题型ID}
              选择选项={(id) => props.选择题型(Number(id))}
            />
          </div>
          <div className={styles.descriptionBox}>
            <h2 className={styles.title}>难度</h2>
            <SingleSelectChipGroup
              选项列表={props.难度选项列表}
              当前选中ID={props.当前难度ID}
              选择选项={(id) => props.选择难度(Number(id))}
              空提示文本="当前没有可选难度。"
            />
            {props.当前难度ID !== null && (
              <div className={styles.actionRow}>
                <button type="button" className={styles.secondaryButton} onClick={() => props.选择难度(null)}>
                  清空难度
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      <div className={styles.section}>
        {props.标签检查区}
      </div>

      <div className={styles.section}>
        <div className={styles.actionRow}>
          <button type="button" className={styles.button} onClick={props.确认录入} disabled={props.正在确认}>
            {props.正在确认 ? "正在录入并插入..." : "确认录入并插入"}
          </button>
          <button type="button" className={styles.secondaryButton} onClick={props.返回编辑} disabled={props.正在确认}>
            返回编辑
          </button>
        </div>
      </div>
    </div>
  );
}
