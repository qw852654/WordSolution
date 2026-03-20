import * as React from "react";
import { makeStyles } from "@fluentui/react-components";

interface QuickAddTagFormState {
  名称: string;
  描述: string;
  数值文本: string;
}

interface QuickAddTagFormProps {
  标题: string;
  父标签名称?: string | null;
  表单: QuickAddTagFormState;
  是否显示数值输入: boolean;
  错误信息: string;
  正在保存: boolean;
  onChange: (字段: keyof QuickAddTagFormState, 值: string) => void;
  onSubmit: () => void;
  onCancel: () => void;
}

const useStyles = makeStyles({
  container: {
    marginTop: "10px",
    padding: "12px",
    borderRadius: "10px",
    border: "1px solid #e8dcc7",
    backgroundColor: "#fffaf0",
    display: "grid",
    gap: "10px",
  },
  title: {
    fontSize: "13px",
    fontWeight: "700",
    color: "#3b2a00",
    margin: 0,
  },
  note: {
    fontSize: "12px",
    lineHeight: "18px",
    color: "#6f675b",
    margin: 0,
  },
  label: {
    fontSize: "12px",
    fontWeight: "600",
    color: "#4a4339",
  },
  input: {
    width: "100%",
    padding: "8px 10px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    fontSize: "12px",
    boxSizing: "border-box",
  },
  textArea: {
    width: "100%",
    minHeight: "64px",
    padding: "8px 10px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    fontSize: "12px",
    boxSizing: "border-box",
    resize: "vertical",
  },
  row: {
    display: "flex",
    gap: "8px",
    flexWrap: "wrap",
    alignItems: "center",
  },
  primaryButton: {
    padding: "8px 12px",
    borderRadius: "8px",
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
    cursor: "pointer",
    fontSize: "12px",
  },
  secondaryButton: {
    padding: "8px 12px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    backgroundColor: "#ffffff",
    color: "#2f2a25",
    cursor: "pointer",
    fontSize: "12px",
  },
  errorText: {
    fontSize: "12px",
    lineHeight: "18px",
    margin: 0,
    color: "#b42318",
  },
});

export default function QuickAddTagForm(props: QuickAddTagFormProps) {
  const styles = useStyles();
  const 阻止冒泡 = (事件: React.SyntheticEvent) => {
    事件.stopPropagation();
  };

  return (
    <div
      className={styles.container}
      onClick={阻止冒泡}
      onMouseDown={阻止冒泡}
      onKeyDown={阻止冒泡}
    >
      <p className={styles.title}>{props.标题}</p>
      {props.父标签名称 && <p className={styles.note}>父标签：{props.父标签名称}</p>}
      <label className={styles.label} htmlFor={`${props.标题}-name`}>
        标签名称
      </label>
      <input
        id={`${props.标题}-name`}
        className={styles.input}
        value={props.表单.名称}
        onChange={(事件) => props.onChange("名称", 事件.target.value)}
      />
      <label className={styles.label} htmlFor={`${props.标题}-description`}>
        标签描述
      </label>
      <textarea
        id={`${props.标题}-description`}
        className={styles.textArea}
        value={props.表单.描述}
        onChange={(事件) => props.onChange("描述", 事件.target.value)}
      />
      {props.是否显示数值输入 && (
        <>
          <label className={styles.label} htmlFor={`${props.标题}-value`}>
            难度数值
          </label>
          <input
            id={`${props.标题}-value`}
            className={styles.input}
            value={props.表单.数值文本}
            onChange={(事件) => props.onChange("数值文本", 事件.target.value)}
          />
        </>
      )}
      {props.错误信息 !== "" && <p className={styles.errorText}>{props.错误信息}</p>}
      <div className={styles.row}>
        <button type="button" className={styles.primaryButton} onClick={props.onSubmit} disabled={props.正在保存}>
          {props.正在保存 ? "正在保存..." : "保存新增"}
        </button>
        <button type="button" className={styles.secondaryButton} onClick={props.onCancel} disabled={props.正在保存}>
          取消
        </button>
      </div>
    </div>
  );
}
