import * as React from "react";
import { makeStyles, mergeClasses } from "@fluentui/react-components";
import { 搜索标签, type 标签搜索项 } from "../search/tagSearch";

interface TagSearchPanelProps {
  标题: string;
  提示文本?: string;
  标签搜索项列表: 标签搜索项[];
  已选标签ID列表: number[];
  选择标签: (标签ID: number, 标签种类ID: number) => void;
}

const useStyles = makeStyles({
  root: {
    display: "grid",
    gap: "10px",
    padding: "14px",
    borderRadius: "12px",
    backgroundColor: "#fffdf9",
    border: "1px solid #ece4d7",
  },
  title: {
    margin: 0,
    fontSize: "15px",
    fontWeight: "600",
    color: "#2d2a26",
  },
  input: {
    width: "100%",
    padding: "10px 12px",
    borderRadius: "10px",
    border: "1px solid #d8cfc0",
    fontSize: "13px",
    boxSizing: "border-box",
  },
  resultList: {
    display: "grid",
    gap: "8px",
  },
  resultButton: {
    width: "100%",
    textAlign: "left",
    padding: "10px 12px",
    borderRadius: "10px",
    border: "1px solid #e5dac7",
    backgroundColor: "#ffffff",
    cursor: "pointer",
    transition: "background-color 140ms ease, box-shadow 140ms ease, transform 140ms ease",
    ":hover": {
      backgroundColor: "#fff7eb",
      boxShadow: "0 6px 14px rgba(90, 65, 20, 0.08)",
      transform: "translateY(-1px)",
    },
  },
  selectedResultButton: {
    backgroundColor: "#fff3d0",
    boxShadow: "inset 0 0 0 1px #b8860b, 0 4px 12px rgba(160, 112, 9, 0.12)",
  },
  resultTopRow: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "10px",
  },
  resultName: {
    fontSize: "13px",
    fontWeight: "600",
    color: "#2f2b26",
  },
  resultSelected: {
    fontSize: "11px",
    fontWeight: "700",
    color: "#9a6700",
    backgroundColor: "#fde8b2",
    borderRadius: "999px",
    padding: "3px 8px",
  },
  resultPath: {
    marginTop: "4px",
    fontSize: "12px",
    lineHeight: "18px",
    color: "#756d60",
    wordBreak: "break-word",
  },
  emptyText: {
    margin: 0,
    fontSize: "12px",
    lineHeight: "18px",
    color: "#756d60",
  },
});

export default function TagSearchPanel(props: TagSearchPanelProps) {
  const styles = useStyles();
  const [关键字, 设置关键字] = React.useState("");

  const 搜索结果列表 = React.useMemo(
    () => 搜索标签(props.标签搜索项列表, 关键字),
    [props.标签搜索项列表, 关键字]
  );

  return (
    <div className={styles.root}>
      <h2 className={styles.title}>{props.标题}</h2>
      <input
        className={styles.input}
        placeholder={props.提示文本 ?? "输入关键字，快速定位标签"}
        value={关键字}
        onChange={(事件) => 设置关键字(事件.target.value)}
      />
      {关键字.trim() !== "" && 搜索结果列表.length === 0 && (
        <p className={styles.emptyText}>没有找到匹配的标签。</p>
      )}
      {关键字.trim() !== "" && 搜索结果列表.length > 0 && (
        <div className={styles.resultList}>
          {搜索结果列表.map((结果项) => {
            const 已选中 = props.已选标签ID列表.includes(结果项.标签ID);
            return (
              <button
                key={`${结果项.标签种类ID}-${结果项.标签ID}`}
                type="button"
                className={mergeClasses(
                  styles.resultButton,
                  已选中 ? styles.selectedResultButton : undefined
                )}
                onClick={() => {
                  props.选择标签(结果项.标签ID, 结果项.标签种类ID);
                  设置关键字("");
                }}
              >
                <div className={styles.resultTopRow}>
                  <span className={styles.resultName}>{结果项.标签名称}</span>
                  {已选中 && <span className={styles.resultSelected}>已选中</span>}
                </div>
                <div className={styles.resultPath}>
                  {结果项.标签种类名称}
                  {结果项.展示路径文本 !== "" ? ` · ${结果项.展示路径文本}` : ""}
                </div>
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
