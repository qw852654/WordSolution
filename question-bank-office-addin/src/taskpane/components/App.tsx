import * as React from "react";
import { makeStyles } from "@fluentui/react-components";
import { 获取当前选区Ooxml, 插入题目到当前文档 } from "../taskpane";

interface AppProps {
  title: string;
}

interface 标签项 {
  id: number;
  名称: string;
  子标签列表?: 标签项[];
}

type 页面名称 = "首页" | "筛题页" | "录题页";
type 组合方式 = "交集" | "并集";

interface 筛选步骤项 {
  步骤编号: number;
  组合方式: 组合方式;
  已选标签列表: 标签项[];
}

interface 题目项 {
  id: number;
  description?: string | null;
  标签ID列表: number[];
}

interface 题目卡片项 {
  id: number;
  description?: string | null;
  标签名称列表: string[];
  预览Html: string;
}

const useStyles = makeStyles({
  root: {
    minHeight: "100vh",
    backgroundColor: "#f7f4ee",
  },
  container: {
    padding: "24px 20px",
  },
  title: {
    fontSize: "28px",
    fontWeight: "700",
    margin: "0 0 8px 0",
    color: "#1f1f1f",
  },
  subtitle: {
    fontSize: "14px",
    lineHeight: "22px",
    margin: "0 0 24px 0",
    color: "#555555",
  },
  sectionTitle: {
    fontSize: "16px",
    fontWeight: "600",
    margin: "0 0 12px 0",
    color: "#2f2f2f",
  },
  actionList: {
    display: "grid",
    gap: "12px",
  },
  actionCard: {
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#ffffff",
    border: "1px solid #e4ddd2",
    boxShadow: "0 2px 8px rgba(0, 0, 0, 0.04)",
  },
  actionButton: {
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#fff7eb",
    border: "1px solid #d8b36d",
    boxShadow: "0 2px 8px rgba(0, 0, 0, 0.04)",
    textAlign: "left",
    cursor: "pointer",
    width: "100%",
  },
  actionName: {
    fontSize: "15px",
    fontWeight: "600",
    margin: "0 0 6px 0",
    color: "#1f1f1f",
  },
  actionDescription: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: "0",
    color: "#666666",
  },
  footer: {
    marginTop: "24px",
    padding: "12px 14px",
    borderRadius: "10px",
    backgroundColor: "#fff9ec",
    color: "#7a5a00",
    fontSize: "13px",
    lineHeight: "20px",
  },
  tagSection: {
    marginTop: "24px",
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#ffffff",
    border: "1px solid #e4ddd2",
    boxShadow: "0 2px 8px rgba(0, 0, 0, 0.04)",
  },
  formField: {
    display: "grid",
    gap: "6px",
    marginBottom: "12px",
  },
  label: {
    fontSize: "13px",
    fontWeight: "600",
    color: "#2f2f2f",
  },
  input: {
    width: "100%",
    padding: "10px 12px",
    borderRadius: "8px",
    border: "1px solid #d7d0c5",
    fontSize: "13px",
    boxSizing: "border-box",
    backgroundColor: "#ffffff",
  },
  textArea: {
    width: "100%",
    minHeight: "72px",
    padding: "10px 12px",
    borderRadius: "8px",
    border: "1px solid #d7d0c5",
    fontSize: "13px",
    boxSizing: "border-box",
    backgroundColor: "#ffffff",
    resize: "vertical",
  },
  submitButton: {
    padding: "10px 14px",
    borderRadius: "8px",
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
    cursor: "pointer",
  },
  tipText: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: "0",
    color: "#666666",
  },
  errorText: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: "0",
    color: "#b42318",
  },
  tagList: {
    margin: "12px 0 0 0",
    paddingLeft: "20px",
    color: "#1f1f1f",
  },
  tagItem: {
    marginBottom: "6px",
    fontSize: "13px",
    lineHeight: "20px",
  },
  clickableTagItem: {
    marginBottom: "6px",
    fontSize: "13px",
    lineHeight: "20px",
    cursor: "pointer",
  },
  backButton: {
    padding: "8px 12px",
    borderRadius: "8px",
    backgroundColor: "#ffffff",
    border: "1px solid #d7d0c5",
    cursor: "pointer",
    color: "#2f2f2f",
    marginBottom: "20px",
  },
  selectedTagList: {
    margin: "0",
    paddingLeft: "20px",
    color: "#1f1f1f",
  },
  modeRow: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "12px",
    marginBottom: "12px",
  },
  modeInfo: {
    display: "flex",
    alignItems: "center",
    justifyContent: "flex-end",
  },
  switchField: {
    display: "flex",
    alignItems: "center",
    gap: "8px",
  },
  switchHint: {
    fontSize: "12px",
    color: "#6a6258",
  },
  switchInput: {
    appearance: "none",
    width: "46px",
    height: "26px",
    borderRadius: "999px",
    border: "1px solid #cdbfa7",
    backgroundColor: "#e8e0d2",
    cursor: "pointer",
    position: "relative",
    margin: "0",
    outlineStyle: "none",
    transitionProperty: "background-color, border-color",
    transitionDuration: "0.2s",
    transitionTimingFunction: "ease",
    "::before": {
      content: "\"\"",
      position: "absolute",
      top: "2px",
      left: "2px",
      width: "20px",
      height: "20px",
      borderRadius: "999px",
      backgroundColor: "#ffffff",
      boxShadow: "0 1px 4px rgba(0, 0, 0, 0.18)",
      transitionProperty: "transform",
      transitionDuration: "0.2s",
      transitionTimingFunction: "ease",
    },
    ":focus-visible": {
      boxShadow: "0 0 0 3px rgba(216, 179, 109, 0.35)",
    },
  },
  switchInputOn: {
    backgroundColor: "#f3c86a",
    border: "1px solid #b8860b",
    "::before": {
      transform: "translateX(20px)",
    },
  },
  stepCardList: {
    display: "grid",
    gap: "16px",
  },
  stepCard: {
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#ffffff",
    border: "1px solid #e4ddd2",
    boxShadow: "0 2px 8px rgba(0, 0, 0, 0.04)",
  },
  selectedTagRow: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "8px",
    marginBottom: "6px",
  },
  removeButton: {
    padding: "4px 8px",
    borderRadius: "6px",
    border: "1px solid #d7d0c5",
    backgroundColor: "#ffffff",
    color: "#6b4f00",
    cursor: "pointer",
    fontSize: "12px",
  },
  nextStepButton: {
    marginTop: "16px",
    padding: "10px 14px",
    borderRadius: "8px",
    border: "1px solid #d7d0c5",
    backgroundColor: "#ffffff",
    color: "#2f2f2f",
    cursor: "pointer",
  },
  stepActionRow: {
    display: "flex",
    gap: "10px",
    marginTop: "16px",
    flexWrap: "wrap",
  },
  runFilterButton: {
    padding: "10px 14px",
    borderRadius: "8px",
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
    cursor: "pointer",
  },
  resultSection: {
    marginTop: "24px",
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#ffffff",
    border: "1px solid #e4ddd2",
    boxShadow: "0 2px 8px rgba(0, 0, 0, 0.04)",
  },
  resultMetaText: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: "0 0 12px 0",
    color: "#666666",
  },
  resultToolbar: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "12px",
    flexWrap: "wrap",
    marginBottom: "12px",
  },
  resultCardList: {
    display: "grid",
    gap: "16px",
  },
  resultCard: {
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#fffdf9",
    border: "1px solid #eadfcb",
  },
  resultTagList: {
    display: "flex",
    gap: "8px",
    flexWrap: "wrap",
    marginBottom: "12px",
  },
  resultTag: {
    padding: "4px 10px",
    borderRadius: "999px",
    backgroundColor: "#fff3d8",
    border: "1px solid #e1c27e",
    color: "#6b4f00",
    fontSize: "12px",
    lineHeight: "18px",
  },
  resultDescription: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: "0 0 12px 0",
    color: "#555555",
  },
  resultPreview: {
    paddingTop: "12px",
    borderTop: "1px solid #efe6d8",
    color: "#1f1f1f",
    overflowX: "auto",
  },
  resultActionRow: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    gap: "12px",
    marginTop: "12px",
  },
  resultIdText: {
    fontSize: "12px",
    color: "#7a756d",
  },
  selectButton: {
    padding: "8px 12px",
    borderRadius: "8px",
    border: "1px solid #d7d0c5",
    backgroundColor: "#ffffff",
    color: "#2f2f2f",
    cursor: "pointer",
    fontSize: "12px",
  },
  selectedButton: {
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
  },
  successText: {
    fontSize: "13px",
    lineHeight: "20px",
    margin: "0",
    color: "#0f7b0f",
  },
});

const App: React.FC<AppProps> = (props: AppProps) => {
  const styles = useStyles();
  const [标签列表, 设置标签列表] = React.useState<标签项[]>([]);
  const [正在加载标签, 设置正在加载标签] = React.useState(true);
  const [标签加载错误, 设置标签加载错误] = React.useState("");
  const [当前页面, 设置当前页面] = React.useState<页面名称>("首页");
  const [筛选步骤列表, 设置筛选步骤列表] = React.useState<筛选步骤项[]>([
    {
      步骤编号: 1,
      组合方式: "交集",
      已选标签列表: [],
    },
  ]);
  const [新标签名称, 设置新标签名称] = React.useState("");
  const [新标签描述, 设置新标签描述] = React.useState("");
  const [正在新增标签, 设置正在新增标签] = React.useState(false);
  const [新增标签错误, 设置新增标签错误] = React.useState("");
  const [录题描述, 设置录题描述] = React.useState("");
  const [录题已选标签列表, 设置录题已选标签列表] = React.useState<标签项[]>([]);
  const [正在录题, 设置正在录题] = React.useState(false);
  const [录题错误, 设置录题错误] = React.useState("");
  const [录题成功提示, 设置录题成功提示] = React.useState("");
  const [正在筛题, 设置正在筛题] = React.useState(false);
  const [筛题错误, 设置筛题错误] = React.useState("");
  const [已执行筛题, 设置已执行筛题] = React.useState(false);
  const [筛题结果卡片列表, 设置筛题结果卡片列表] = React.useState<题目卡片项[]>([]);
  const [已选题目ID列表, 设置已选题目ID列表] = React.useState<number[]>([]);
  const [正在插题, 设置正在插题] = React.useState(false);
  const [插题错误, 设置插题错误] = React.useState("");
  const [插题成功提示, 设置插题成功提示] = React.useState("");

  const 读取标签 = async () => {
    try {
      设置标签加载错误("");
      设置正在加载标签(true);

      const 响应 = await fetch("http://localhost:5282/api/标签");

      if (!响应.ok) {
        throw new Error("标签接口调用失败。");
      }

      const 数据 = (await 响应.json()) as 标签项[];
      设置标签列表(数据);
    } catch (error) {
      console.error(error);
      设置标签加载错误("暂时无法连接本地题库服务。请先启动题库本地服务。");
    } finally {
      设置正在加载标签(false);
    }
  };

  React.useEffect(() => {
    读取标签();
  }, []);

  const 获取标签名称列表 = (标签ID列表: number[]) => {
    const 标签名称映射 = new Map<number, string>();

    const 收集标签 = (当前标签列表: 标签项[]) => {
      当前标签列表.forEach((标签) => {
        标签名称映射.set(标签.id, 标签.名称);

        if (标签.子标签列表 && 标签.子标签列表.length > 0) {
          收集标签(标签.子标签列表);
        }
      });
    };

    收集标签(标签列表);

    return 标签ID列表
      .map((标签ID) => 标签名称映射.get(标签ID))
      .filter((标签名称): 标签名称 is string => typeof 标签名称 === "string" && 标签名称.trim() !== "");
  };

  const 当前步骤 = 筛选步骤列表[筛选步骤列表.length - 1];

  const 选择标签 = (标签: 标签项) => {
    const 已存在 = 当前步骤.已选标签列表.some((已选标签) => 已选标签.id === 标签.id);

    if (已存在) {
      return;
    }

    const 新步骤列表 = 筛选步骤列表.map((步骤) => {
      if (步骤.步骤编号 !== 当前步骤.步骤编号) {
        return 步骤;
      }

      return {
        ...步骤,
        已选标签列表: [...步骤.已选标签列表, 标签],
      };
    });

    设置筛选步骤列表(新步骤列表);
  };

  const 取消标签 = (标签ID: number) => {
    const 新步骤列表 = 筛选步骤列表.map((步骤) => {
      if (步骤.步骤编号 !== 当前步骤.步骤编号) {
        return 步骤;
      }

      return {
        ...步骤,
        已选标签列表: 步骤.已选标签列表.filter((标签) => 标签.id !== 标签ID),
      };
    });

    设置筛选步骤列表(新步骤列表);
  };

  const 切换当前步骤组合方式 = () => {
    const 新步骤列表 = 筛选步骤列表.map((步骤) => {
      if (步骤.步骤编号 !== 当前步骤.步骤编号) {
        return 步骤;
      }

      const 新组合方式: 组合方式 = 步骤.组合方式 === "交集" ? "并集" : "交集";

      return {
        ...步骤,
        组合方式: 新组合方式,
      };
    });

    设置筛选步骤列表(新步骤列表);
  };

  const 开始下一步筛选 = () => {
    const 新步骤: 筛选步骤项 = {
      步骤编号: 筛选步骤列表.length + 1,
      组合方式: "交集",
      已选标签列表: [],
    };

    设置筛选步骤列表([...筛选步骤列表, 新步骤]);
  };

  const 执行筛题 = async () => {
    const 有效步骤列表 = 筛选步骤列表.filter((步骤) => 步骤.已选标签列表.length > 0);

    if (有效步骤列表.length === 0) {
      设置筛题错误("请至少在一步筛选中选择一个标签。");
      设置筛题结果卡片列表([]);
      设置已执行筛题(false);
      return;
    }

    try {
      设置正在筛题(true);
      设置筛题错误("");
      设置已执行筛题(true);
      设置插题错误("");
      设置插题成功提示("");

      const 响应 = await fetch("http://localhost:5282/api/题目/筛选", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(
          有效步骤列表.map((步骤, 索引) => ({
            标签ID列表: 步骤.已选标签列表.map((标签) => 标签.id),
            本步标签组合方式: 步骤.组合方式,
            与前一步结果组合方式: 索引 === 0 ? "交集" : "交集",
          })),
        ),
      });

      if (!响应.ok) {
        throw new Error("筛题失败。");
      }

      const 题目列表 = (await 响应.json()) as 题目项[];
      const 题目卡片列表 = await Promise.all(
        题目列表.map(async (题目) => {
          let 预览Html = "<p>暂时无法加载题目预览。</p>";

          try {
            const 预览响应 = await fetch(`http://localhost:5282/api/题目/${题目.id}/预览html`);
            if (预览响应.ok) {
              预览Html = await 预览响应.text();
            }
          } catch (error) {
            console.error(error);
          }

          return {
            id: 题目.id,
            description: 题目.description,
            标签名称列表: 获取标签名称列表(题目.标签ID列表),
            预览Html,
          };
        }),
      );

      设置筛题结果卡片列表(题目卡片列表);
    } catch (error) {
      console.error(error);
      设置筛题错误("筛题失败，请确认本地服务正在运行。");
      设置筛题结果卡片列表([]);
    } finally {
      设置正在筛题(false);
    }
  };

  const 切换题目选择状态 = (题目ID: number) => {
    const 当前已选 = 已选题目ID列表.includes(题目ID);

    if (当前已选) {
      设置已选题目ID列表(已选题目ID列表.filter((当前题目ID) => 当前题目ID !== 题目ID));
      return;
    }

    设置已选题目ID列表([...已选题目ID列表, 题目ID]);
  };

  const 插入已选题目 = async () => {
    if (已选题目ID列表.length === 0) {
      设置插题错误("请先选择至少一道题目。");
      设置插题成功提示("");
      return;
    }

    try {
      设置正在插题(true);
      设置插题错误("");
      设置插题成功提示("");

      const 待插入题目列表 = await Promise.all(
        已选题目ID列表.map(async (题目ID) => {
          const 响应 = await fetch(`http://localhost:5282/api/题目/${题目ID}/文件base64`);
          if (!响应.ok) {
            throw new Error(`题目 ${题目ID} 的原文件读取失败。`);
          }

          const 文件Base64 = await 响应.text();
          return {
            题目ID,
            文件Base64,
          };
        }),
      );

      await 插入题目到当前文档(待插入题目列表);
      设置已选题目ID列表([]);
      设置插题成功提示(`已成功插入 ${待插入题目列表.length} 道题目。`);
    } catch (error) {
      console.error(error);
      设置插题错误("插入已选题目失败，请确认本地服务和 Word 文档都处于正常状态。");
      设置插题成功提示("");
    } finally {
      设置正在插题(false);
    }
  };

  const 新增标签 = async () => {
    if (新标签名称.trim() === "") {
      设置新增标签错误("标签名称不能为空。");
      return;
    }

    try {
      设置新增标签错误("");
      设置正在新增标签(true);

      const 响应 = await fetch("http://localhost:5282/api/标签", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          名称: 新标签名称.trim(),
          Description: 新标签描述.trim() === "" ? null : 新标签描述.trim(),
        }),
      });

      if (!响应.ok) {
        throw new Error("新增标签失败。");
      }

      设置新标签名称("");
      设置新标签描述("");
      await 读取标签();
    } catch (error) {
      console.error(error);
      设置新增标签错误("新增标签失败，请检查本地服务是否正常运行。");
    } finally {
      设置正在新增标签(false);
    }
  };

  const 选择录题标签 = (标签: 标签项) => {
    const 已存在 = 录题已选标签列表.some((已选标签) => 已选标签.id === 标签.id);
    if (已存在) {
      return;
    }

    设置录题已选标签列表([...录题已选标签列表, 标签]);
  };

  const 取消录题标签 = (标签ID: number) => {
    设置录题已选标签列表(录题已选标签列表.filter((标签) => 标签.id !== 标签ID));
  };

  const 从当前选区录入题目 = async () => {
    if (录题已选标签列表.length === 0) {
      设置录题错误("请至少选择一个标签。");
      设置录题成功提示("");
      return;
    }

    try {
      设置正在录题(true);
      设置录题错误("");
      设置录题成功提示("");

      const Ooxml内容 = await 获取当前选区Ooxml();
      const 响应 = await fetch("http://localhost:5282/api/题目/ooxml", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          Description: 录题描述.trim() === "" ? null : 录题描述.trim(),
          标签ID列表: 录题已选标签列表.map((标签) => 标签.id),
          Ooxml内容,
        }),
      });

      if (!响应.ok) {
        throw new Error("录题失败。");
      }

      const 新题目 = (await 响应.json()) as { id: number };
      设置录题描述("");
      设置录题已选标签列表([]);
      设置录题成功提示(`录题成功，题目ID：${新题目.id}`);
    } catch (error) {
      console.error(error);
      设置录题错误("从当前选区录题失败，请确认已经在 Word 中选中内容，并且本地服务正在运行。");
      设置录题成功提示("");
    } finally {
      设置正在录题(false);
    }
  };

  if (当前页面 === "录题页") {
    return (
      <div className={styles.root}>
        <div className={styles.container}>
          <button type="button" className={styles.backButton} onClick={() => 设置当前页面("首页")}>
            返回首页
          </button>

          <h1 className={styles.title}>录入题目</h1>
          <p className={styles.subtitle}>先在 Word 中选中题目内容，再在这里填写描述、选择标签，最后从当前选区录入。</p>

          <div className={styles.tagSection}>
            <h2 className={styles.sectionTitle}>题目描述</h2>
            <textarea
              className={styles.textArea}
              value={录题描述}
              onChange={(事件) => 设置录题描述(事件.target.value)}
            />
          </div>

          <div className={styles.tagSection}>
            <h2 className={styles.sectionTitle}>已选标签</h2>

            {录题已选标签列表.length === 0 && <p className={styles.tipText}>当前还没有选择标签。</p>}

            {录题已选标签列表.length > 0 && (
              <ul className={styles.selectedTagList}>
                {录题已选标签列表.map((标签) => (
                  <li key={标签.id} className={styles.selectedTagRow}>
                    <span className={styles.tagItem}>{标签.名称}</span>
                    <button type="button" className={styles.removeButton} onClick={() => 取消录题标签(标签.id)}>
                      取消
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className={styles.tagSection}>
            <h2 className={styles.sectionTitle}>当前标签</h2>

            {正在加载标签 && <p className={styles.tipText}>正在读取本地题库标签...</p>}

            {!正在加载标签 && 标签加载错误 !== "" && <p className={styles.errorText}>{标签加载错误}</p>}

            {!正在加载标签 && 标签加载错误 === "" && 标签列表.length === 0 && (
              <p className={styles.tipText}>当前还没有标签数据。</p>
            )}

            {!正在加载标签 && 标签加载错误 === "" && 标签列表.length > 0 && (
              <ul className={styles.tagList}>
                {标签列表.map((标签) => (
                  <li
                    key={标签.id}
                    className={styles.clickableTagItem}
                    onClick={() => 选择录题标签(标签)}
                    onKeyDown={(事件) => {
                      if (事件.key === "Enter" || 事件.key === " ") {
                        选择录题标签(标签);
                      }
                    }}
                    role="button"
                    tabIndex={0}
                  >
                    {标签.名称}
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className={styles.tagSection}>
            <h2 className={styles.sectionTitle}>从当前选区录入</h2>

            {录题错误 !== "" && <p className={styles.errorText}>{录题错误}</p>}
            {录题成功提示 !== "" && <p className={styles.successText}>{录题成功提示}</p>}

            <button type="button" className={styles.submitButton} onClick={从当前选区录入题目} disabled={正在录题}>
              {正在录题 ? "正在录题..." : "从当前选区录入"}
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (当前页面 === "筛题页") {
    return (
      <div className={styles.root}>
        <div className={styles.container}>
          <button type="button" className={styles.backButton} onClick={() => 设置当前页面("首页")}>
            返回首页
          </button>

          <h1 className={styles.title}>筛选题目</h1>
          <p className={styles.subtitle}>这是最小筛题页骨架。后续会在这里接入多步筛选、题目预览和多选插题。</p>

          <div className={styles.stepCardList}>
            {筛选步骤列表.map((步骤) => (
              <div key={步骤.步骤编号} className={styles.stepCard}>
                <h2 className={styles.sectionTitle}>第{步骤.步骤编号}步筛选条件</h2>

                <div className={styles.modeRow}>
                  <div />
                  {步骤.步骤编号 === 当前步骤.步骤编号 && (
                    <div className={styles.modeInfo}>
                      <label className={styles.switchField}>
                        <span className={styles.switchHint}>并集</span>
                        <input
                          type="checkbox"
                          className={`${styles.switchInput} ${步骤.组合方式 === "并集" ? styles.switchInputOn : ""}`}
                          checked={步骤.组合方式 === "并集"}
                          onChange={切换当前步骤组合方式}
                        />
                      </label>
                    </div>
                  )}
                </div>

                {步骤.已选标签列表.length === 0 && <p className={styles.tipText}>当前还没有选择标签。</p>}

                {步骤.已选标签列表.length > 0 && (
                  <ul className={styles.selectedTagList}>
                    {步骤.已选标签列表.map((标签) => (
                      <li key={标签.id} className={styles.selectedTagRow}>
                        <span className={styles.tagItem}>{标签.名称}</span>
                        {步骤.步骤编号 === 当前步骤.步骤编号 && (
                          <button type="button" className={styles.removeButton} onClick={() => 取消标签(标签.id)}>
                            取消
                          </button>
                        )}
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            ))}
          </div>

          <div className={styles.tagSection}>
            <h2 className={styles.sectionTitle}>当前标签</h2>

            {正在加载标签 && <p className={styles.tipText}>正在读取本地题库标签...</p>}

            {!正在加载标签 && 标签加载错误 !== "" && <p className={styles.errorText}>{标签加载错误}</p>}

            {!正在加载标签 && 标签加载错误 === "" && 标签列表.length === 0 && (
              <p className={styles.tipText}>当前还没有标签数据。</p>
            )}

            {!正在加载标签 && 标签加载错误 === "" && 标签列表.length > 0 && (
              <ul className={styles.tagList}>
                {标签列表.map((标签) => (
                  <li
                    key={标签.id}
                    className={styles.clickableTagItem}
                    onClick={() => 选择标签(标签)}
                    onKeyDown={(事件) => {
                      if (事件.key === "Enter" || 事件.key === " ") {
                        选择标签(标签);
                      }
                    }}
                    role="button"
                    tabIndex={0}
                  >
                    {标签.名称}
                  </li>
                ))}
              </ul>
            )}
          </div>

          <button type="button" className={styles.nextStepButton} onClick={开始下一步筛选}>
            开始下一步筛选
          </button>

          <div className={styles.stepActionRow}>
            <button type="button" className={styles.runFilterButton} onClick={执行筛题} disabled={正在筛题}>
              {正在筛题 ? "正在筛题..." : "执行筛题"}
            </button>
          </div>

          <div className={styles.resultSection}>
            <h2 className={styles.sectionTitle}>筛题结果</h2>
            <div className={styles.resultToolbar}>
              <p className={styles.resultMetaText}>已选题目：{已选题目ID列表.length} 道</p>
              <button
                type="button"
                className={styles.runFilterButton}
                onClick={插入已选题目}
                disabled={正在插题 || 已选题目ID列表.length === 0}
              >
                {正在插题 ? "正在插入..." : "插入已选题目"}
              </button>
            </div>

            {筛题错误 !== "" && <p className={styles.errorText}>{筛题错误}</p>}
            {插题错误 !== "" && <p className={styles.errorText}>{插题错误}</p>}
            {插题成功提示 !== "" && <p className={styles.successText}>{插题成功提示}</p>}

            {筛题错误 === "" && 正在筛题 && <p className={styles.tipText}>正在加载筛题结果...</p>}

            {筛题错误 === "" && !正在筛题 && !已执行筛题 && (
              <p className={styles.tipText}>先完成当前筛选步骤，再执行筛题，结果会直接显示为题目卡片。</p>
            )}

            {筛题错误 === "" && !正在筛题 && 已执行筛题 && 筛题结果卡片列表.length === 0 && (
              <p className={styles.tipText}>当前筛选条件下还没有匹配到题目。</p>
            )}

            {筛题错误 === "" && 筛题结果卡片列表.length > 0 && (
              <div className={styles.resultCardList}>
                {筛题结果卡片列表.map((题目卡片) => {
                  const 已选中 = 已选题目ID列表.includes(题目卡片.id);

                  return (
                    <div key={题目卡片.id} className={styles.resultCard}>
                      {题目卡片.标签名称列表.length > 0 && (
                        <div className={styles.resultTagList}>
                          {题目卡片.标签名称列表.map((标签名称) => (
                            <span key={`${题目卡片.id}-${标签名称}`} className={styles.resultTag}>
                              {标签名称}
                            </span>
                          ))}
                        </div>
                      )}

                      {题目卡片.description && <p className={styles.resultDescription}>{题目卡片.description}</p>}

                      <div
                        className={styles.resultPreview}
                        dangerouslySetInnerHTML={{ __html: 题目卡片.预览Html }}
                      />

                      <div className={styles.resultActionRow}>
                        <span className={styles.resultIdText}>题目ID：{题目卡片.id}</span>
                        <button
                          type="button"
                          className={`${styles.selectButton} ${已选中 ? styles.selectedButton : ""}`}
                          onClick={() => 切换题目选择状态(题目卡片.id)}
                        >
                          {已选中 ? "取消选择" : "选择题目"}
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.root}>
      <div className={styles.container}>
        <h1 className={styles.title}>{props.title}</h1>
        <p className={styles.subtitle}>本地题库任务窗格。先完成首页骨架，后续再接入录题、筛题和插题功能。</p>

        <h2 className={styles.sectionTitle}>主入口</h2>

        <div className={styles.actionList}>
          <button type="button" className={styles.actionButton} onClick={() => 设置当前页面("录题页")}>
            <p className={styles.actionName}>录入题目</p>
            <p className={styles.actionDescription}>后续这里会接入从 Word 选区录入题目，并填写标签的流程。</p>
          </button>

          <button type="button" className={styles.actionButton} onClick={() => 设置当前页面("筛题页")}>
            <p className={styles.actionName}>筛选题目</p>
            <p className={styles.actionDescription}>点击后进入最小筛题页，先验证页面切换、标签展示和已选标签状态。</p>
          </button>
        </div>

        <div className={styles.tagSection}>
          <h2 className={styles.sectionTitle}>新增标签</h2>

          <div className={styles.formField}>
            <label className={styles.label} htmlFor="new-tag-name">
              标签名称
            </label>
            <input
              id="new-tag-name"
              className={styles.input}
              value={新标签名称}
              onChange={(事件) => 设置新标签名称(事件.target.value)}
            />
          </div>

          <div className={styles.formField}>
            <label className={styles.label} htmlFor="new-tag-description">
              标签描述
            </label>
            <textarea
              id="new-tag-description"
              className={styles.textArea}
              value={新标签描述}
              onChange={(事件) => 设置新标签描述(事件.target.value)}
            />
          </div>

          {新增标签错误 !== "" && <p className={styles.errorText}>{新增标签错误}</p>}

          <button type="button" className={styles.submitButton} onClick={新增标签} disabled={正在新增标签}>
            {正在新增标签 ? "正在新增..." : "新增标签"}
          </button>
        </div>

        <div className={styles.tagSection}>
          <h2 className={styles.sectionTitle}>标签接口测试</h2>

          {正在加载标签 && <p className={styles.tipText}>正在读取本地题库标签...</p>}

          {!正在加载标签 && 标签加载错误 !== "" && <p className={styles.errorText}>{标签加载错误}</p>}

          {!正在加载标签 && 标签加载错误 === "" && 标签列表.length === 0 && (
            <p className={styles.tipText}>当前还没有标签数据。</p>
          )}

          {!正在加载标签 && 标签加载错误 === "" && 标签列表.length > 0 && (
            <ul className={styles.tagList}>
              {标签列表.map((标签) => (
                <li key={标签.id} className={styles.tagItem}>
                  {标签.名称}
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className={styles.footer}>当前阶段：先把任务窗格首页搭起来，并确认前端可以成功读取本地服务接口。</div>
      </div>
    </div>
  );
};

export default App;
