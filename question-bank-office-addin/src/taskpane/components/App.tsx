import * as React from "react";
import { makeStyles } from "@fluentui/react-components";

interface AppProps {
  title: string;
}

interface 标签项 {
  id: number;
  name: string;
  children?: 标签项[];
}

type 页面名称 = "首页" | "筛题页";

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
});

const App: React.FC<AppProps> = (props: AppProps) => {
  const styles = useStyles();
  const [标签列表, 设置标签列表] = React.useState<标签项[]>([]);
  const [正在加载标签, 设置正在加载标签] = React.useState(true);
  const [标签加载错误, 设置标签加载错误] = React.useState("");
  const [当前页面, 设置当前页面] = React.useState<页面名称>("首页");
  const [已选标签列表, 设置已选标签列表] = React.useState<标签项[]>([]);

  React.useEffect(() => {
    const 读取标签 = async () => {
      try {
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

    读取标签();
  }, []);

  const 选择标签 = (标签: 标签项) => {
    const 已存在 = 已选标签列表.some((已选标签) => 已选标签.id === 标签.id);

    if (已存在) {
      return;
    }

    设置已选标签列表([...已选标签列表, 标签]);
  };

  if (当前页面 === "筛题页") {
    return (
      <div className={styles.root}>
        <div className={styles.container}>
          <button type="button" className={styles.backButton} onClick={() => 设置当前页面("首页")}>
            返回首页
          </button>

          <h1 className={styles.title}>筛选题目</h1>
          <p className={styles.subtitle}>这是最小筛题页骨架。后续会在这里接入多步筛选、题目预览和多选插题。</p>

          <div className={styles.tagSection}>
            <h2 className={styles.sectionTitle}>已选标签</h2>

            {已选标签列表.length === 0 && <p className={styles.tipText}>当前还没有选择标签。</p>}

            {已选标签列表.length > 0 && (
              <ul className={styles.selectedTagList}>
                {已选标签列表.map((标签) => (
                  <li key={标签.id} className={styles.tagItem}>
                    {标签.name}
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
                    onClick={() => 选择标签(标签)}
                    onKeyDown={(事件) => {
                      if (事件.key === "Enter" || 事件.key === " ") {
                        选择标签(标签);
                      }
                    }}
                    role="button"
                    tabIndex={0}
                  >
                    {标签.name}
                  </li>
                ))}
              </ul>
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
          <div className={styles.actionCard}>
            <p className={styles.actionName}>录入题目</p>
            <p className={styles.actionDescription}>后续这里会接入从 Word 选区录入题目，并填写标签的流程。</p>
          </div>

          <button type="button" className={styles.actionButton} onClick={() => 设置当前页面("筛题页")}>
            <p className={styles.actionName}>筛选题目</p>
            <p className={styles.actionDescription}>点击后进入最小筛题页，先验证页面切换、标签展示和已选标签状态。</p>
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
                  {标签.name}
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
