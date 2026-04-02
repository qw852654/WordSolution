import * as React from "react";
import { makeStyles } from "@fluentui/react-components";
import QuickAddTagForm from "./QuickAddTagForm";
import TagBadge from "./TagBadge";
import TagSearchPanel from "./TagSearchPanel";
import TagSelectionTree from "./TagSelectionTree";
import type { 标签搜索项 } from "../search/tagSearch";

interface 标签种类项 {
  id: number;
  名称: string;
  是否树形: boolean;
  是否允许多选: boolean;
}

interface 标签项 {
  id: number;
  名称: string;
  标签种类ID?: number;
  父标签ID?: number | null;
  description?: string | null;
  描述?: string | null;
  numericValue?: number | null;
  子标签列表?: 标签项[];
}

interface 树形标签项 extends 标签项 {
  子标签列表: 树形标签项[];
}

interface 快速新增标签表单 {
  名称: string;
  描述: string;
  数值文本: string;
}

interface 新增标签参数 {
  标签种类: 标签种类项;
  父标签ID: number | null;
  名称: string;
  描述: string;
  数值文本: string;
}

interface 编辑标签参数 {
  标签: 标签项;
  标签种类: 标签种类项;
  名称: string;
  描述: string;
  数值文本: string;
}

interface 标签工作台Props {
  模式: "录题" | "导入标签检查" | "导入标签辅助";
  标签种类列表: 标签种类项[];
  已选标签ID映射: Record<number, number[]>;
  标签搜索项列表: 标签搜索项[];
  获取指定种类标签列表: (标签种类ID: number) => 标签项[];
  获取标签显示文本: (标签: 标签项) => string;
  切换标签: (标签种类: 标签种类项, 标签ID: number) => void;
  通过搜索选择标签: (标签ID: number, 标签种类ID: number) => void;
  新增标签: (参数: 新增标签参数) => Promise<{ id: number; 名称: string }>;
  编辑标签: (参数: 编辑标签参数) => Promise<void>;
  移动标签?: (
    标签种类: 标签种类项,
    拖动标签ID: number,
    目标标签ID: number,
    放置方式: "before" | "after" | "inside"
  ) => Promise<void>;
}

interface 新增目标 {
  标签种类ID: number;
  父标签ID: number | null;
  父标签名称: string | null;
}

interface 编辑目标 {
  标签种类ID: number;
  标签: 标签项;
}

const useStyles = makeStyles({
  root: {
    display: "grid",
    gap: "16px",
  },
  compactRoot: {
    gap: "12px",
  },
  combinedTopSection: {
    display: "grid",
    gap: "16px",
    padding: "14px",
    borderRadius: "12px",
    border: "1px solid #ece4d7",
    backgroundColor: "#fffdf9",
    gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
  },
  compactCombinedTopSection: {
    gap: "12px",
    padding: "12px",
    gridTemplateColumns: "1fr",
  },
  topBlock: {
    display: "grid",
    gap: "10px",
    alignContent: "start",
  },
  blockTitle: {
    margin: 0,
    fontSize: "15px",
    fontWeight: "600",
    color: "#2d2a26",
  },
  blockNote: {
    margin: 0,
    fontSize: "12px",
    lineHeight: "18px",
    color: "#756d60",
  },
  compactSummaryText: {
    margin: 0,
    fontSize: "12px",
    lineHeight: "18px",
    color: "#5f584d",
  },
  summaryGroup: {
    display: "grid",
    gap: "10px",
  },
  summaryRow: {
    display: "grid",
    gap: "8px",
  },
  summaryLabel: {
    fontSize: "12px",
    color: "#756d60",
    fontWeight: "600",
  },
  chipGroup: {
    display: "flex",
    flexWrap: "wrap",
    gap: "8px",
  },
  section: {
    display: "grid",
    gap: "12px",
    padding: "14px",
    borderRadius: "12px",
    border: "1px solid #ece4d7",
    backgroundColor: "#fffdf9",
  },
  compactSection: {
    gap: "10px",
    padding: "12px",
  },
  sectionHeader: {
    display: "flex",
    flexWrap: "wrap",
    justifyContent: "space-between",
    alignItems: "center",
    gap: "10px",
  },
  sectionTitle: {
    margin: 0,
    fontSize: "16px",
    fontWeight: "600",
    color: "#2d2a26",
  },
  noteText: {
    margin: 0,
    fontSize: "12px",
    lineHeight: "18px",
    color: "#756d60",
  },
  kindSection: {
    display: "grid",
    gap: "10px",
    padding: "12px",
    borderRadius: "12px",
    border: "1px solid #ece4d7",
    backgroundColor: "#fffdf9",
  },
  compactKindSection: {
    gap: "8px",
    padding: "10px",
  },
  kindHeader: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "10px",
    flexWrap: "wrap",
  },
  kindTitle: {
    margin: 0,
    fontSize: "14px",
    fontWeight: "600",
    color: "#2d2a26",
  },
  actionRow: {
    display: "flex",
    flexWrap: "wrap",
    gap: "8px",
  },
  secondaryButton: {
    padding: "8px 12px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    backgroundColor: "#fff",
    color: "#2f2a25",
    cursor: "pointer",
    fontSize: "12px",
  },
  compactSecondaryButton: {
    padding: "4px 6px",
    fontSize: "11px",
    lineHeight: "14px",
    minWidth: "auto",
  },
  flatWall: {
    display: "flex",
    flexWrap: "wrap",
    gap: "10px",
  },
  flatChipItem: {
    display: "flex",
    alignItems: "center",
    gap: "6px",
    flexWrap: "nowrap",
  },
  selectableChip: {
    padding: "8px 12px",
    borderRadius: "999px",
    border: "1px solid #dfd3bc",
    backgroundColor: "#fff",
    color: "#524c43",
    cursor: "pointer",
    fontSize: "12px",
    lineHeight: "16px",
    whiteSpace: "nowrap",
  },
  selectedChip: {
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
    boxShadow: "0 3px 8px rgba(160, 112, 9, 0.14)",
  },
  highlightedFlatChip: {
    border: "1px solid #d79b27",
    backgroundColor: "#fff6dd",
    boxShadow:
      "0 0 0 2px rgba(215, 155, 39, 0.14), 0 8px 16px rgba(160, 112, 9, 0.10)",
  },
  flatEditButton: {
    padding: "6px 10px",
    borderRadius: "999px",
    border: "1px solid #d8cfc0",
    backgroundColor: "#fff",
    color: "#5b5348",
    cursor: "pointer",
    fontSize: "11px",
    lineHeight: "14px",
    whiteSpace: "nowrap",
  },
  inlineFormHost: {
    marginTop: "8px",
  },
  fullWidthForm: {
    width: "100%",
    flexBasis: "100%",
  },
  errorText: {
    margin: 0,
    fontSize: "12px",
    lineHeight: "18px",
    color: "#b42318",
  },
});

const 空表单 = (): 快速新增标签表单 => ({
  名称: "",
  描述: "",
  数值文本: "",
});

function 获取标签描述(标签: 标签项): string | null {
  const 描述 = 标签.description ?? 标签.描述 ?? null;
  if (描述 === null) {
    return null;
  }
  const 修整后 = 描述.trim();
  return 修整后 === "" ? null : 修整后;
}

function 归一化树节点(标签列表: 标签项[]): 树形标签项[] {
  return 标签列表.map((标签) => ({
    ...标签,
    description: 标签.description ?? 标签.描述 ?? null,
    子标签列表: 归一化树节点(标签.子标签列表 ?? []),
  }));
}

function 收集标签映射(标签列表: 标签项[], 映射: Map<number, 标签项>) {
  标签列表.forEach((标签) => {
    映射.set(标签.id, 标签);
    收集标签映射(标签.子标签列表 ?? [], 映射);
  });
}

export default function 标签工作台(props: 标签工作台Props) {
  const styles = useStyles();
  const 是导入辅助模式 = props.模式 === "导入标签辅助";

  const [新增目标, 设置新增目标] = React.useState<新增目标 | null>(null);
  const [新增表单, 设置新增表单] = React.useState<快速新增标签表单>(空表单);
  const [新增错误, 设置新增错误] = React.useState("");
  const [正在新增, 设置正在新增] = React.useState(false);

  const [编辑目标, 设置编辑目标] = React.useState<编辑目标 | null>(null);
  const [编辑表单, 设置编辑表单] = React.useState<快速新增标签表单>(空表单);
  const [编辑错误, 设置编辑错误] = React.useState("");
  const [正在编辑, 设置正在编辑] = React.useState(false);
  const [工作台错误, 设置工作台错误] = React.useState("");
  const [最近成功标签ID, 设置最近成功标签ID] = React.useState<number | null>(null);
  const [树展开状态映射, 设置树展开状态映射] = React.useState<Record<number, boolean>>({});
  const [树展开指令映射, 设置树展开指令映射] = React.useState<
    Record<number, { 展开: boolean; 令牌: number } | undefined>
  >({});

  const 全部已选标签ID列表 = React.useMemo(
    () => Array.from(new Set(Object.values(props.已选标签ID映射).flat())),
    [props.已选标签ID映射]
  );

  const 按种类标签映射 = React.useMemo(() => {
    const 映射 = new Map<number, 标签项[]>();
    props.标签种类列表.forEach((标签种类) => {
      const 原始列表 = props.获取指定种类标签列表(标签种类.id) ?? [];
      映射.set(标签种类.id, 标签种类.是否树形 ? 归一化树节点(原始列表) : 原始列表);
    });
    return 映射;
  }, [props]);

  const 全部标签字典 = React.useMemo(() => {
    const 映射 = new Map<number, 标签项>();
    props.标签种类列表.forEach((标签种类) => {
      收集标签映射(按种类标签映射.get(标签种类.id) ?? [], 映射);
    });
    return 映射;
  }, [按种类标签映射, props.标签种类列表]);

  const 获取种类已选标签 = React.useCallback(
    (标签种类ID: number) =>
      (props.已选标签ID映射[标签种类ID] ?? [])
        .map((标签ID) => 全部标签字典.get(标签ID))
        .filter((标签): 标签 is 标签项 => Boolean(标签)),
    [全部标签字典, props.已选标签ID映射]
  );

  const 紧凑已选标签预览 = React.useMemo(
    () =>
      全部已选标签ID列表
        .map((标签ID) => 全部标签字典.get(标签ID))
        .filter((标签): 标签 is 标签项 => Boolean(标签))
        .slice(0, 4),
    [全部已选标签ID列表, 全部标签字典]
  );

  const 关闭新增表单 = React.useCallback(() => {
    设置新增目标(null);
    设置新增表单(空表单());
    设置新增错误("");
    设置工作台错误("");
    设置正在新增(false);
  }, []);

  const 关闭编辑表单 = React.useCallback(() => {
    设置编辑目标(null);
    设置编辑表单(空表单());
    设置编辑错误("");
    设置工作台错误("");
    设置正在编辑(false);
  }, []);

  React.useEffect(() => {
    if (最近成功标签ID === null) {
      return undefined;
    }
    const 定时器 = window.setTimeout(() => {
      设置最近成功标签ID(null);
    }, 2200);
    return () => window.clearTimeout(定时器);
  }, [最近成功标签ID]);

  const 切换树展开状态 = React.useCallback((标签种类ID: number) => {
    设置树展开状态映射((当前) => {
      const 下一状态 = !当前[标签种类ID];
      设置树展开指令映射((当前指令) => ({
        ...当前指令,
        [标签种类ID]: {
          展开: 下一状态,
          令牌: Date.now() + 标签种类ID,
        },
      }));
      return {
        ...当前,
        [标签种类ID]: 下一状态,
      };
    });
  }, []);

  const 处理移动标签 = React.useCallback(
    async (
      标签种类: 标签种类项,
      拖动标签ID: number,
      目标标签ID: number,
      放置方式: "before" | "after" | "inside"
    ) => {
      if (!props.移动标签) {
        return;
      }

      try {
        设置工作台错误("");
        await props.移动标签(标签种类, 拖动标签ID, 目标标签ID, 放置方式);
        设置最近成功标签ID(拖动标签ID);
      } catch (error) {
        console.error(error);
        设置工作台错误(
          error instanceof Error && error.message.trim() !== ""
            ? error.message
            : "移动标签失败。"
        );
        throw error;
      }
    },
    [props]
  );

  const 打开新增根标签表单 = React.useCallback(
    (标签种类: 标签种类项) => {
      关闭编辑表单();
      设置新增目标({
        标签种类ID: 标签种类.id,
        父标签ID: null,
        父标签名称: null,
      });
      设置新增表单(空表单());
      设置新增错误("");
    },
    [关闭编辑表单]
  );

  const 打开新增子标签表单 = React.useCallback(
    (标签种类: 标签种类项, 标签: 标签项) => {
      关闭编辑表单();
      设置新增目标({
        标签种类ID: 标签种类.id,
        父标签ID: 标签.id,
        父标签名称: 标签.名称,
      });
      设置新增表单(空表单());
      设置新增错误("");
    },
    [关闭编辑表单]
  );

  const 打开编辑表单 = React.useCallback(
    (标签种类: 标签种类项, 标签: 标签项) => {
      关闭新增表单();
      设置编辑目标({ 标签种类ID: 标签种类.id, 标签 });
      设置编辑表单({
        名称: 标签.名称,
        描述: 获取标签描述(标签) ?? "",
        数值文本: typeof 标签.numericValue === "number" ? String(标签.numericValue) : "",
      });
      设置编辑错误("");
    },
    [关闭新增表单]
  );

  const 提交新增标签 = React.useCallback(async () => {
    if (!新增目标) {
      return;
    }

    const 标签种类 = props.标签种类列表.find((项) => 项.id === 新增目标.标签种类ID);
    if (!标签种类) {
      设置新增错误("当前标签种类不存在。");
      return;
    }

    try {
      设置正在新增(true);
      设置新增错误("");
      const 新标签 = await props.新增标签({
        标签种类,
        父标签ID: 标签种类.是否树形 ? 新增目标.父标签ID : null,
        名称: 新增表单.名称.trim(),
        描述: 新增表单.描述.trim(),
        数值文本: 新增表单.数值文本.trim(),
      });
      设置最近成功标签ID(新标签.id);
      props.通过搜索选择标签(新标签.id, 标签种类.id);
      关闭新增表单();
    } catch (error) {
      console.error(error);
      设置新增错误(
        error instanceof Error && error.message.trim() !== ""
          ? error.message
          : "新增标签失败。"
      );
    } finally {
      设置正在新增(false);
    }
  }, [关闭新增表单, props, 新增目标, 新增表单]);

  const 提交编辑标签 = React.useCallback(async () => {
    if (!编辑目标) {
      return;
    }

    const 标签种类 = props.标签种类列表.find((项) => 项.id === 编辑目标.标签种类ID);
    if (!标签种类) {
      设置编辑错误("当前标签种类不存在。");
      return;
    }

    try {
      设置正在编辑(true);
      设置编辑错误("");
      await props.编辑标签({
        标签: 编辑目标.标签,
        标签种类,
        名称: 编辑表单.名称.trim(),
        描述: 编辑表单.描述.trim(),
        数值文本: 编辑表单.数值文本.trim(),
      });
      设置最近成功标签ID(编辑目标.标签.id);
      关闭编辑表单();
    } catch (error) {
      console.error(error);
      设置编辑错误(
        error instanceof Error && error.message.trim() !== ""
          ? error.message
          : "编辑标签失败。"
      );
    } finally {
      设置正在编辑(false);
    }
  }, [关闭编辑表单, props, 编辑目标, 编辑表单]);

  const 渲染新增表单 = React.useCallback(
    (标题: string, 父标签名称?: string | null) => (
      <QuickAddTagForm
        标题={标题}
        父标签名称={父标签名称 ?? null}
        表单={新增表单}
        是否显示数值输入={新增目标?.标签种类ID === 3}
        错误信息={新增错误}
        正在保存={正在新增}
        onChange={(字段, 值) => 设置新增表单((当前) => ({ ...当前, [字段]: 值 }))}
        onSubmit={() => void 提交新增标签()}
        onCancel={关闭新增表单}
      />
    ),
    [关闭新增表单, 新增表单, 新增目标?.标签种类ID, 新增错误, 正在新增, 提交新增标签]
  );

  const 渲染编辑表单 = React.useCallback(
    () => (
      <QuickAddTagForm
        标题="编辑标签"
        表单={编辑表单}
        是否显示数值输入={编辑目标?.标签种类ID === 3}
        错误信息={编辑错误}
        正在保存={正在编辑}
        onChange={(字段, 值) => 设置编辑表单((当前) => ({ ...当前, [字段]: 值 }))}
        onSubmit={() => void 提交编辑标签()}
        onCancel={关闭编辑表单}
      />
    ),
    [关闭编辑表单, 编辑表单, 编辑目标?.标签种类ID, 编辑错误, 正在编辑, 提交编辑标签]
  );

  return (
    <div className={`${styles.root} ${是导入辅助模式 ? styles.compactRoot : ""}`}>
      <div
        className={`${styles.combinedTopSection} ${
          是导入辅助模式 ? styles.compactCombinedTopSection : ""
        }`}
      >
        <div className={styles.topBlock}>
          <h3 className={styles.blockTitle}>{是导入辅助模式 ? "标签树辅助" : "搜索标签"}</h3>
          <p className={styles.blockNote}>
            {是导入辅助模式
              ? "这里专门用来快速查看、补充和维护标签树，改动会立刻同步到下方正式检查区。"
              : props.模式 === "录题"
                ? "输入关键字，快速把需要的标签挂到当前题目上。"
                : "输入关键字，快速调整当前导入题目的最终标签。"}
          </p>
          <TagSearchPanel
            标题="搜索标签"
            提示文本="输入关键字，快速定位标签"
            标签搜索项列表={props.标签搜索项列表}
            已选标签ID列表={全部已选标签ID列表}
            选择标签={props.通过搜索选择标签}
            无外框
            隐藏标题
          />
        </div>

        <div className={styles.topBlock}>
          <h3 className={styles.blockTitle}>当前已选标签</h3>
          {是导入辅助模式 ? (
            <>
              <p className={styles.compactSummaryText}>
                当前共选中 {全部已选标签ID列表.length} 个标签。
                {全部已选标签ID列表.length > 0 ? " 右侧补充或下方正式检查后会实时同步。" : ""}
              </p>
              {紧凑已选标签预览.length > 0 && (
                <div className={styles.chipGroup}>
                  {紧凑已选标签预览.map((标签) => {
                    const 标签种类 = props.标签种类列表.find((项) => 项.id === 标签.标签种类ID);
                    return (
                      <TagBadge
                        key={`compact-selected-${标签.id}`}
                        文本={props.获取标签显示文本(标签)}
                        强调
                        onClick={
                          标签种类 ? () => props.切换标签(标签种类, 标签.id) : undefined
                        }
                      />
                    );
                  })}
                  {全部已选标签ID列表.length > 紧凑已选标签预览.length && (
                    <TagBadge 文本={`还有 ${全部已选标签ID列表.length - 紧凑已选标签预览.length} 个`} />
                  )}
                </div>
              )}
            </>
          ) : (
            <>
              <p className={styles.blockNote}>点击标签可直接取消。</p>
              <div className={styles.summaryGroup}>
                {props.标签种类列表.map((标签种类) => {
                  const 已选标签列表 = 获取种类已选标签(标签种类.id);
                  if (已选标签列表.length === 0) {
                    return null;
                  }
                  return (
                    <div key={`summary-${标签种类.id}`} className={styles.summaryRow}>
                      <div className={styles.summaryLabel}>{标签种类.名称}</div>
                      <div className={styles.chipGroup}>
                        {已选标签列表.map((标签) => (
                          <TagBadge
                            key={`selected-${标签.id}`}
                            文本={props.获取标签显示文本(标签)}
                            强调
                            onClick={() => props.切换标签(标签种类, 标签.id)}
                          />
                        ))}
                      </div>
                    </div>
                  );
                })}
                {全部已选标签ID列表.length === 0 && (
                  <p className={styles.noteText}>当前还没有选中标签。</p>
                )}
              </div>
            </>
          )}
        </div>
      </div>

      <div className={`${styles.section} ${是导入辅助模式 ? styles.compactSection : ""}`}>
        <div className={styles.sectionHeader}>
          <h3 className={styles.sectionTitle}>{是导入辅助模式 ? "标签树" : "标签选择"}</h3>
          <p className={styles.noteText}>
            {是导入辅助模式
              ? "这里保留完整标签能力，用来快速补标签，不替代下方正式确认。"
              : "这里统一完成选择、新增和编辑标签。"}
          </p>
        </div>

        {工作台错误 !== "" && <p className={styles.errorText}>{工作台错误}</p>}

        {props.标签种类列表.map((标签种类) => {
          const 标签列表 = 按种类标签映射.get(标签种类.id) ?? [];
          const 已选标签ID列表 = props.已选标签ID映射[标签种类.id] ?? [];
          const 正在新增根标签 =
            新增目标?.标签种类ID === 标签种类.id && 新增目标.父标签ID === null;

          return (
            <div
              key={`kind-${标签种类.id}`}
              className={`${styles.kindSection} ${是导入辅助模式 ? styles.compactKindSection : ""}`}
            >
              <div className={styles.kindHeader}>
                <h4 className={styles.kindTitle}>{标签种类.名称}</h4>
                <div className={styles.actionRow}>
                  {标签种类.是否树形 && (
                    <button
                      type="button"
                      className={`${styles.secondaryButton} ${
                        是导入辅助模式 ? styles.compactSecondaryButton : ""
                      }`}
                      onClick={() => 切换树展开状态(标签种类.id)}
                    >
                      {树展开状态映射[标签种类.id] ? "收起全部" : "展开全部"}
                    </button>
                  )}
                  <button
                    type="button"
                    className={`${styles.secondaryButton} ${
                      是导入辅助模式 ? styles.compactSecondaryButton : ""
                    }`}
                    onClick={() => 打开新增根标签表单(标签种类)}
                  >
                    {标签种类.是否树形 ? "新增根标签" : "新增标签"}
                  </button>
                </div>
              </div>

              {正在新增根标签 && (
                <div className={styles.inlineFormHost}>
                  {渲染新增表单(
                    标签种类.是否树形 ? `新增${标签种类.名称}根标签` : `新增${标签种类.名称}`
                  )}
                </div>
              )}

              {标签种类.是否树形 ? (
                <TagSelectionTree
                  树名称={标签种类.名称}
                  标签列表={标签列表 as 树形标签项[]}
                  已选标签ID列表={已选标签ID列表}
                  高亮标签ID={最近成功标签ID}
                  紧凑模式={是导入辅助模式}
                  显示选择按钮={!是导入辅助模式}
                  展开指令={树展开指令映射[标签种类.id]}
                  获取标签显示文本={props.获取标签显示文本}
                  切换标签={(标签ID) => props.切换标签(标签种类, 标签ID)}
                  移动标签={
                    props.移动标签
                      ? (拖动标签ID, 目标标签ID, 放置方式) =>
                          处理移动标签(标签种类, 拖动标签ID, 目标标签ID, 放置方式)
                      : undefined
                  }
                  空提示文本="当前种类下还没有标签。"
                  渲染节点附加操作={(标签) => (
                    <>
                      {!是导入辅助模式 && (
                        <button
                          type="button"
                          className={`${styles.secondaryButton} ${
                            是导入辅助模式 ? styles.compactSecondaryButton : ""
                          }`}
                          onClick={(事件) => {
                            事件.preventDefault();
                            事件.stopPropagation();
                            打开编辑表单(标签种类, 标签);
                          }}
                        >
                          编辑标签
                        </button>
                      )}
                      <button
                        type="button"
                        className={`${styles.secondaryButton} ${
                          是导入辅助模式 ? styles.compactSecondaryButton : ""
                        }`}
                        onClick={(事件) => {
                          事件.preventDefault();
                          事件.stopPropagation();
                          打开新增子标签表单(标签种类, 标签);
                        }}
                      >
                        {是导入辅助模式 ? "子" : "新增子标签"}
                      </button>
                    </>
                  )}
                  渲染节点下方内容={(标签) => {
                    const 显示新增子标签表单 =
                      新增目标?.标签种类ID === 标签种类.id && 新增目标.父标签ID === 标签.id;
                    const 显示编辑表单 = 编辑目标?.标签.id === 标签.id;

                    if (显示新增子标签表单) {
                      return (
                        <div className={styles.inlineFormHost}>
                          {渲染新增表单(`新增${标签种类.名称}子标签`, 新增目标?.父标签名称 ?? 标签.名称)}
                        </div>
                      );
                    }

                    if (显示编辑表单) {
                      return <div className={styles.inlineFormHost}>{渲染编辑表单()}</div>;
                    }

                    return null;
                  }}
                />
              ) : 标签列表.length === 0 ? (
                <p className={styles.noteText}>当前种类下还没有标签。</p>
              ) : (
                <div className={styles.flatWall}>
                  {标签列表.map((标签) => {
                    const 已选中 = 已选标签ID列表.includes(标签.id);
                    const 正在编辑当前标签 = 编辑目标?.标签.id === 标签.id;

                    return (
                      <div key={`flat-${标签.id}`} className={styles.flatChipItem}>
                        <button
                          type="button"
                          className={`${styles.selectableChip} ${已选中 ? styles.selectedChip : ""} ${
                            最近成功标签ID === 标签.id ? styles.highlightedFlatChip : ""
                          }`}
                          onClick={() => props.切换标签(标签种类, 标签.id)}
                          title={获取标签描述(标签) ?? 标签.名称}
                        >
                          {props.获取标签显示文本(标签)}
                        </button>

                        {已选中 && (
                          <button
                            type="button"
                            className={styles.flatEditButton}
                            onClick={() => 打开编辑表单(标签种类, 标签)}
                          >
                            编辑
                          </button>
                        )}

                        {正在编辑当前标签 && (
                          <div className={`${styles.inlineFormHost} ${styles.fullWidthForm}`}>
                            {渲染编辑表单()}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
