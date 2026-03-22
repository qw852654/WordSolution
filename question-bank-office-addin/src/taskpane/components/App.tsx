import * as React from "react";
import { makeStyles, ToggleButton } from "@fluentui/react-components";
import { 清除待处理导航请求 as clearPendingTaskpaneNavigation, 监听导航请求 as listenTaskpaneNavigation, 读取待处理导航请求 as readPendingTaskpaneNavigation, type Ribbon目标页面 as RibbonTargetPage } from "../../shared/taskpaneNavigation";
import { 获取当前选区Ooxml, 插入题目到当前文档 } from "../taskpane";
import QuestionPreviewCard from "./QuestionPreviewCard";
import QuickAddTagForm from "./QuickAddTagForm";
import TagBadge from "./TagBadge";
import TagSearchPanel from "./TagSearchPanel";
import TagSelectionTree from "./TagSelectionTree";
import type { 标签搜索项 } from "../search/tagSearch";
import { 获取当前题目内容控件上下文 } from "../word/题目内容控件上下文";

interface AppProps {
  title: string;
}

interface 题库实例项 {
  题库键: string;
  显示名称: string;
  是否已初始化: boolean;
  题目数量: number;
  标签数量: number;
}

interface 标签种类项 {
  id: number;
  名称: string;
  是否树形: boolean;
  是否允许多选: boolean;
  是否系统内置: boolean;
  是否在正式工作流中可见: boolean;
}

interface 标签项 {
  id: number;
  标签种类ID: number;
  名称: string;
  description?: string | null;
  parentId?: number | null;
  同级排序值: number;
  numericValue?: number | null;
  isEnabled: boolean;
  子标签列表: 标签项[];
}

interface 题目项 {
  id: number;
  description?: string | null;
  标签ID列表: number[];
}

interface 题目卡片项 {
  id: number;
  description?: string | null;
  标题: string;
  标签列表: 标签项[];
  预览Html: string;
}

interface 标签新增表单 {
  名称: string;
  描述: string;
  父标签ID文本: string;
  数值文本: string;
}

interface 标签编辑表单 {
  id: number;
  标签种类ID: number;
  名称: string;
  描述: string;
  父标签ID文本: string;
  数值文本: string;
  isEnabled: boolean;
}

interface 快速新增标签表单 {
  名称: string;
  描述: string;
  数值文本: string;
}

interface 快速新增目标 {
  标签种类ID: number;
  父标签ID: number | null;
  父标签名称: string | null;
}

interface 筛选步骤项 {
  步骤编号: number;
  组合方式: 组合方式;
  已选标签ID映射: 标签选择映射;
}

interface 删除确认状态 {
  题目ID: number;
  阶段: 1 | 2;
}

type 页面名称 = "首页" | "录题页" | "筛题页" | "标签整理页";
type 组合方式 = "交集" | "并集";
type 标签选择映射 = Record<number, number[]>;

const API_ROOT = (() => {
  const { protocol, hostname, port, origin } = window.location;

  if ((hostname === "localhost" || hostname === "127.0.0.1") && port === "3000") {
    return "http://localhost:5282/api";
  }

  if (hostname === "localhost" || hostname === "127.0.0.1") {
    return `${protocol}//${hostname}${port ? `:${port}` : ""}/api`;
  }

  return `${origin}/api`;
})();
const 当前题库键存储键 = "currentQuestionBankKey";
const 默认题库键 = "TEST";
const 系统标签种类 = {
  章节: 1,
  做题方法: 2,
  难度: 3,
  附加标签: 4,
  待整理: 5,
  试卷题型: 6,
} as const;

const 标签种类显示顺序 = [
  系统标签种类.章节,
  系统标签种类.做题方法,
  系统标签种类.难度,
  系统标签种类.附加标签,
  系统标签种类.试卷题型,
  系统标签种类.待整理,
];

function 获取标签种类排序值(标签种类ID: number) {
  const 索引 = 标签种类显示顺序.indexOf(标签种类ID as (typeof 标签种类显示顺序)[number]);
  return 索引 >= 0 ? 索引 : 标签种类显示顺序.length + 标签种类ID;
}

const useStyles = makeStyles({
  root: { minHeight: "100vh", backgroundColor: "#f6f1e7" },
  container: { padding: "24px 18px 28px" },
  title: { fontSize: "28px", fontWeight: "700", margin: "0 0 8px 0", color: "#1f1f1f" },
  subtitle: { fontSize: "14px", lineHeight: "22px", margin: "0 0 20px 0", color: "#5f5a50" },
  section: {
    marginTop: "18px",
    padding: "16px",
    borderRadius: "14px",
    backgroundColor: "#ffffff",
    border: "1px solid #e4ddd2",
  },
  sectionTitle: { fontSize: "16px", fontWeight: "600", margin: "0 0 12px 0", color: "#2d2a26" },
  input: {
    width: "100%",
    padding: "10px 12px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    fontSize: "13px",
    boxSizing: "border-box",
  },
  textArea: {
    width: "100%",
    minHeight: "80px",
    padding: "10px 12px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    fontSize: "13px",
    boxSizing: "border-box",
    resize: "vertical",
  },
  select: {
    width: "100%",
    padding: "10px 12px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    fontSize: "13px",
    boxSizing: "border-box",
  },
  button: {
    padding: "10px 14px",
    borderRadius: "8px",
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
    cursor: "pointer",
    fontSize: "13px",
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
  backButton: {
    padding: "8px 12px",
    borderRadius: "8px",
    border: "1px solid #d8cfc0",
    backgroundColor: "#ffffff",
    cursor: "pointer",
    marginBottom: "16px",
  },
  bankBanner: {
    marginBottom: "14px",
    padding: "10px 12px",
    borderRadius: "10px",
    backgroundColor: "#fff8ec",
    color: "#6a5800",
    fontSize: "12px",
  },
  label: { fontSize: "13px", fontWeight: "600", color: "#4a4339" },
  row: { display: "flex", gap: "10px", flexWrap: "wrap", alignItems: "center" },
  column: { display: "grid", gap: "10px" },
  sectionHeaderRow: { display: "flex", gap: "10px", flexWrap: "wrap", alignItems: "center", justifyContent: "space-between" },
  selectedTagsCardHeader: {
    display: "flex",
    gap: "12px",
    flexWrap: "wrap",
    alignItems: "center",
    justifyContent: "space-between",
    marginBottom: "12px",
  },
  selectedTagsCardActions: {
    display: "flex",
    gap: "10px",
    flexWrap: "wrap",
    alignItems: "center",
    justifyContent: "flex-end",
  },
  selectedTagsRecordButton: {
    minWidth: "140px",
  },
  continuousToggle: {
    borderRadius: "999px",
    border: "1px solid #d8cfc0",
    backgroundColor: "#ffffff",
    color: "#5a544a",
    boxShadow: "none",
  },
  continuousToggleChecked: {
    border: "1px solid #c9952e",
    backgroundColor: "#fde8b2",
    color: "#5c3b00",
    boxShadow: "0 4px 10px rgba(160, 112, 9, 0.16)",
  },
  actionGrid: { display: "grid", gap: "12px" },
  actionButton: {
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#fff7eb",
    border: "1px solid #d8b36d",
    cursor: "pointer",
    textAlign: "left",
  },
  actionName: { fontSize: "15px", fontWeight: "600", margin: "0 0 6px 0", color: "#1f1f1f" },
  actionDescription: { fontSize: "13px", lineHeight: "20px", margin: "0", color: "#655f55" },
  kindSection: {
    padding: "14px",
    borderRadius: "12px",
    backgroundColor: "#fffdf9",
    border: "1px solid #ece4d7",
  },
  chipRow: { display: "flex", gap: "8px", flexWrap: "wrap" },
  chip: {
    padding: "6px 10px",
    borderRadius: "999px",
    border: "1px solid #dfd3bc",
    backgroundColor: "#ffffff",
    color: "#524c43",
    cursor: "pointer",
    fontSize: "12px",
  },
  selectedChip: { border: "1px solid #b8860b", backgroundColor: "#f3c86a", color: "#3b2a00" },
  treeBlock: { display: "grid", gap: "8px" },
  inlineFormHost: { paddingLeft: "16px", borderLeft: "2px solid #efe2c8", marginTop: "4px" },
  treeRow: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "12px",
    flexWrap: "wrap",
  },
  treeInfo: { display: "grid", gap: "4px", minWidth: 0, flexGrow: 1 },
  tagName: { fontSize: "13px", fontWeight: "600", color: "#2f2b26" },
  meta: { fontSize: "12px", color: "#756d60", lineHeight: "18px" },
  resultCard: {
    padding: "16px",
    borderRadius: "12px",
    backgroundColor: "#fffdf9",
    border: "1px solid #caa56a",
  },
  resultTagRow: { display: "flex", gap: "6px", flexWrap: "wrap", marginBottom: "10px" },
  resultPreview: {
    paddingTop: "12px",
    borderTop: "1px solid #efe6d8",
    color: "#1f1f1f",
    overflowX: "auto",
  },
  errorText: { fontSize: "13px", lineHeight: "20px", margin: "0", color: "#b42318" },
  successText: { fontSize: "13px", lineHeight: "20px", margin: "0", color: "#0f7b0f" },
  noteText: { fontSize: "13px", lineHeight: "20px", margin: "0", color: "#6f675b" },
});

function 创建空新增表单(): 标签新增表单 {
  return { 名称: "", 描述: "", 父标签ID文本: "", 数值文本: "" };
}

function 创建空快速新增表单(): 快速新增标签表单 {
  return { 名称: "", 描述: "", 数值文本: "" };
}

function 创建空筛选步骤(步骤编号: number): 筛选步骤项 {
  return { 步骤编号, 组合方式: "交集", 已选标签ID映射: {} };
}

function 构建题库接口路径(题库键: string, 子路径: string) {
  return `${API_ROOT}/题库实例/${encodeURIComponent(题库键)}${子路径}`;
}

function mapRibbonPageToAppPage(targetPage: RibbonTargetPage): 页面名称 {
  if (targetPage === "录题页") {
    return "录题页";
  }
  if (targetPage === "筛题页") {
    return "筛题页";
  }
  return "首页";
}

function 读取本地题库键() {
  try {
    return localStorage.getItem(当前题库键存储键);
  } catch {
    return null;
  }
}

function 保存本地题库键(题库键: string) {
  try {
    localStorage.setItem(当前题库键存储键, 题库键);
  } catch {}
}

function 选择指定标签(原映射: 标签选择映射, 标签种类: 标签种类项, 标签ID: number) {
  const 当前已选标签ID列表 = 原映射[标签种类.id] ?? [];
  if (标签种类.是否允许多选) {
    return 当前已选标签ID列表.includes(标签ID)
      ? 原映射
      : { ...原映射, [标签种类.id]: [...当前已选标签ID列表, 标签ID] };
  }
  return { ...原映射, [标签种类.id]: [标签ID] };
}

function 解析整数文本(文本: string) {
  const 修整值 = 文本.trim();
  if (修整值 === "") {
    return null;
  }
  const 数值 = Number.parseInt(修整值, 10);
  return Number.isNaN(数值) ? null : 数值;
}

function 解析小数文本(文本: string) {
  const 修整值 = 文本.trim();
  if (修整值 === "") {
    return null;
  }
  const 数值 = Number.parseFloat(修整值);
  return Number.isNaN(数值) ? null : 数值;
}

function 截断文本(文本: string, 最大长度: number) {
  return 文本.length <= 最大长度 ? 文本 : `${文本.slice(0, 最大长度)}...`;
}

function 构建内容控件标题(难度名称?: string, 描述?: string | null) {
  const 修整后的描述 = 描述?.trim() ?? "";
  const 截断后的描述 = 修整后的描述 === "" ? "" : 截断文本(修整后的描述, 40);
  if (难度名称 && 截断后的描述 !== "") {
    return `${难度名称}｜${截断后的描述}`;
  }
  if (难度名称) {
    return 难度名称;
  }
  if (截断后的描述 !== "") {
    return 截断后的描述;
  }
  return "题目";
}

export default function App(props: AppProps) {
  const styles = useStyles();
  const [当前页面, 设置当前页面] = React.useState<页面名称>(() => {
    const pendingNavigation = readPendingTaskpaneNavigation();
    if (pendingNavigation) {
      return mapRibbonPageToAppPage(pendingNavigation.目标页面);
    }
    return "首页";
  });
  const [题库实例列表, 设置题库实例列表] = React.useState<题库实例项[]>([]);
  const [当前题库键, 设置当前题库键] = React.useState(默认题库键);
  const [正在加载题库实例, 设置正在加载题库实例] = React.useState(true);
  const [题库实例错误, 设置题库实例错误] = React.useState("");
  const [标签种类列表, 设置标签种类列表] = React.useState<标签种类项[]>([]);
  const [标签映射, 设置标签映射] = React.useState<Record<number, 标签项[]>>({});
  const [正在加载标签基础数据, 设置正在加载标签基础数据] = React.useState(false);
  const [标签基础数据错误, 设置标签基础数据错误] = React.useState("");
  const [录题描述, 设置录题描述] = React.useState("");
  const [录题标签选择, 设置录题标签选择] = React.useState<标签选择映射>({});
  const [连续录入已开启, 设置连续录入已开启] = React.useState(true);
  const [正在录题, 设置正在录题] = React.useState(false);
  const [录题错误, 设置录题错误] = React.useState("");
  const [录题成功提示, 设置录题成功提示] = React.useState("");
  const [最近录入题目ID, 设置最近录入题目ID] = React.useState<number | null>(null);
  const [正在更新题目, 设置正在更新题目] = React.useState(false);
  const [更新题目错误, 设置更新题目错误] = React.useState("");
  const [更新题目成功提示, 设置更新题目成功提示] = React.useState("");
  const [最近更新题目ID, 设置最近更新题目ID] = React.useState<number | null>(null);
  const [更新题目按钮失败提示, 设置更新题目按钮失败提示] = React.useState("");
  const [筛选步骤列表, 设置筛选步骤列表] = React.useState<筛选步骤项[]>([创建空筛选步骤(1)]);
  const [正在筛题, 设置正在筛题] = React.useState(false);
  const [筛题错误, 设置筛题错误] = React.useState("");
  const [已执行筛题, 设置已执行筛题] = React.useState(false);
  const [筛题结果卡片列表, 设置筛题结果卡片列表] = React.useState<题目卡片项[]>([]);
  const [已选题目ID列表, 设置已选题目ID列表] = React.useState<number[]>([]);
  const [删除确认状态, 设置删除确认状态] = React.useState<删除确认状态 | null>(null);
  const [正在删除题目ID, 设置正在删除题目ID] = React.useState<number | null>(null);
  const [删除题目错误, 设置删除题目错误] = React.useState("");
  const [正在插题, 设置正在插题] = React.useState(false);
  const [插题错误, 设置插题错误] = React.useState("");
  const [插题成功提示, 设置插题成功提示] = React.useState("");
  const [新增标签表单映射, 设置新增标签表单映射] = React.useState<Record<number, 标签新增表单>>({});
  const [正在保存标签, 设置正在保存标签] = React.useState(false);
  const [标签整理错误, 设置标签整理错误] = React.useState("");
  const [标签整理成功提示, 设置标签整理成功提示] = React.useState("");
  const [编辑标签表单, 设置编辑标签表单] = React.useState<标签编辑表单 | null>(null);
  const [快速新增目标, 设置快速新增目标] = React.useState<快速新增目标 | null>(null);
  const [快速新增表单, 设置快速新增表单] = React.useState<快速新增标签表单>(创建空快速新增表单);
  const [快速新增错误, 设置快速新增错误] = React.useState("");

  const 标签种类字典 = React.useMemo(
    () => new Map(标签种类列表.map((标签种类) => [标签种类.id, 标签种类])),
    [标签种类列表]
  );
  const 正式标签种类列表 = React.useMemo(
    () => 标签种类列表.filter((标签种类) => 标签种类.是否在正式工作流中可见),
    [标签种类列表]
  );
  const 当前题库实例 = React.useMemo(
    () => 题库实例列表.find((题库实例) => 题库实例.题库键 === 当前题库键) ?? null,
    [题库实例列表, 当前题库键]
  );
  const 当前筛选步骤 = 筛选步骤列表[筛选步骤列表.length - 1];
  const 当前结果题目ID列表 = React.useMemo(() => 筛题结果卡片列表.map((题目卡片) => 题目卡片.id), [筛题结果卡片列表]);
  const 当前结果题目ID集合 = React.useMemo(() => new Set(当前结果题目ID列表), [当前结果题目ID列表]);
  const 当前结果已选题目数量 = React.useMemo(
    () => 已选题目ID列表.filter((题目ID) => 当前结果题目ID集合.has(题目ID)).length,
    [已选题目ID列表, 当前结果题目ID集合]
  );
  const 当前结果题目总数 = 当前结果题目ID列表.length;
  const 当前结果已全选 = 当前结果题目总数 > 0 && 当前结果已选题目数量 === 当前结果题目总数;
  const handledNavigationTokenRef = React.useRef<string | null>(null);
  const 上一个页面Ref = React.useRef<页面名称>(当前页面);

  const 扁平标签字典 = React.useMemo(() => {
    const 字典 = new Map<number, 标签项>();
    const 收集 = (标签列表: 标签项[]) =>
      标签列表.forEach((标签) => {
        字典.set(标签.id, 标签);
        if (标签.子标签列表 && 标签.子标签列表.length > 0) {
          收集(标签.子标签列表);
        }
      });
    Object.values(标签映射).forEach((标签列表) => 收集(标签列表));
    return 字典;
  }, [标签映射]);

  const 按种类扁平标签映射 = React.useMemo(() => {
    const 结果: Record<number, 标签项[]> = {};
    const 收集 = (标签种类ID: number, 标签列表: 标签项[]) => {
      if (!结果[标签种类ID]) {
        结果[标签种类ID] = [];
      }
      标签列表.forEach((标签) => {
        结果[标签种类ID].push(标签);
        if (标签.子标签列表 && 标签.子标签列表.length > 0) {
          收集(标签种类ID, 标签.子标签列表);
        }
      });
    };
    Object.entries(标签映射).forEach(([标签种类ID文本, 标签列表]) =>
      收集(Number(标签种类ID文本), 标签列表)
    );
    return 结果;
  }, [标签映射]);

  const 读取题库实例列表 = React.useCallback(async () => {
    try {
      设置正在加载题库实例(true);
      设置题库实例错误("");
      const 响应 = await fetch(`${API_ROOT}/题库实例`);
      if (!响应.ok) {
        throw new Error("题库实例读取失败。");
      }
      const 列表 = (await 响应.json()) as 题库实例项[];
      设置题库实例列表(列表);
      const 本地题库键 = 读取本地题库键();
      const 有效题库键 =
        列表.find((题库实例) => 题库实例.题库键 === 本地题库键)?.题库键 ??
        列表.find((题库实例) => 题库实例.题库键 === 默认题库键)?.题库键 ??
        列表[0]?.题库键 ??
        默认题库键;
      设置当前题库键(有效题库键);
    } catch (error) {
      console.error(error);
      设置题库实例错误("题库实例读取失败，请确认本地服务正在运行。");
    } finally {
      设置正在加载题库实例(false);
    }
  }, []);

  const 读取标签基础数据 = React.useCallback(async (题库键: string) => {
    try {
      设置正在加载标签基础数据(true);
      设置标签基础数据错误("");
      const 标签种类响应 = await fetch(构建题库接口路径(题库键, "/标签种类"));
      if (!标签种类响应.ok) {
        throw new Error("标签种类读取失败。");
      }
      const 标签种类结果 = ((await 标签种类响应.json()) as 标签种类项[]).sort(
        (前一个, 后一个) => 获取标签种类排序值(前一个.id) - 获取标签种类排序值(后一个.id)
      );
      const 标签读取结果 = await Promise.all(
        标签种类结果.map(async (标签种类) => {
          const 标签响应 = await fetch(
            `${构建题库接口路径(题库键, "/标签")}?标签种类ID=${encodeURIComponent(标签种类.id)}`
          );
          if (!标签响应.ok) {
            throw new Error(`${标签种类.名称} 读取失败。`);
          }
          return [标签种类.id, (await 标签响应.json()) as 标签项[]] as const;
        })
      );
      设置标签种类列表(标签种类结果);
      设置标签映射(
        标签读取结果.reduce<Record<number, 标签项[]>>((结果, [标签种类ID, 标签列表]) => {
          结果[标签种类ID] = 标签列表;
          return 结果;
        }, {})
      );
    } catch (error) {
      console.error(error);
      设置标签基础数据错误("标签数据读取失败，请确认本地服务正在运行。");
      设置标签种类列表([]);
      设置标签映射({});
    } finally {
      设置正在加载标签基础数据(false);
    }
  }, []);

  React.useEffect(() => {
    读取题库实例列表();
  }, [读取题库实例列表]);

  React.useEffect(() => {
    if (当前题库键.trim() === "") {
      return;
    }
    保存本地题库键(当前题库键);
    设置录题标签选择({});
    设置录题描述("");
    设置连续录入已开启(true);
    设置录题错误("");
    设置录题成功提示("");
    设置最近录入题目ID(null);
    设置正在更新题目(false);
    设置更新题目错误("");
    设置更新题目成功提示("");
    设置最近更新题目ID(null);
    设置更新题目按钮失败提示("");
    设置筛选步骤列表([创建空筛选步骤(1)]);
    设置筛题结果卡片列表([]);
    设置已执行筛题(false);
    设置筛题错误("");
    设置已选题目ID列表([]);
    设置删除确认状态(null);
    设置正在删除题目ID(null);
    设置删除题目错误("");
    设置插题错误("");
    设置插题成功提示("");
    设置编辑标签表单(null);
    设置标签整理错误("");
    设置标签整理成功提示("");
    设置快速新增目标(null);
    设置快速新增表单(创建空快速新增表单());
    设置快速新增错误("");
    读取标签基础数据(当前题库键);
  }, [当前题库键, 读取标签基础数据]);

  React.useEffect(() => {
    if (当前页面 === "录题页" && 上一个页面Ref.current !== "录题页") {
      设置连续录入已开启(true);
      设置最近录入题目ID(null);
      设置最近更新题目ID(null);
      设置更新题目按钮失败提示("");
    }
    if (当前页面 !== "筛题页") {
      设置删除确认状态(null);
      设置正在删除题目ID(null);
      设置删除题目错误("");
    }
    上一个页面Ref.current = 当前页面;
  }, [当前页面]);

  React.useEffect(() => {
    if (更新题目按钮失败提示 === "") {
      return undefined;
    }

    const 定时器 = window.setTimeout(() => {
      设置更新题目按钮失败提示("");
    }, 5000);

    return () => {
      window.clearTimeout(定时器);
    };
  }, [更新题目按钮失败提示]);

  React.useEffect(() => {
    const applyPendingNavigation = (
      pendingNavigation: ReturnType<typeof readPendingTaskpaneNavigation> | null
    ) => {
      if (!pendingNavigation) {
        return;
      }
      if (pendingNavigation.导航令牌 === handledNavigationTokenRef.current) {
        return;
      }
      handledNavigationTokenRef.current = pendingNavigation.导航令牌;
      设置当前页面(mapRibbonPageToAppPage(pendingNavigation.目标页面));
      clearPendingTaskpaneNavigation(pendingNavigation.导航令牌);
    };

    applyPendingNavigation(readPendingTaskpaneNavigation());
    const stopListening = listenTaskpaneNavigation(applyPendingNavigation);
    const pollTimer = window.setInterval(() => {
      applyPendingNavigation(readPendingTaskpaneNavigation());
    }, 500);

    return () => {
      stopListening();
      window.clearInterval(pollTimer);
    };
  }, []);
  const 获取标签显示文本 = React.useCallback((标签: 标签项) => {
    if (
      标签.标签种类ID === 系统标签种类.难度 &&
      标签.numericValue !== null &&
      标签.numericValue !== undefined
    ) {
      return `${标签.名称}（${标签.numericValue}）`;
    }
    return 标签.名称;
  }, []);

  const 获取指定种类标签列表 = React.useCallback(
    (标签种类ID: number) => 标签映射[标签种类ID] ?? [],
    [标签映射]
  );
  const 获取指定种类扁平标签列表 = React.useCallback(
    (标签种类ID: number) => 按种类扁平标签映射[标签种类ID] ?? [],
    [按种类扁平标签映射]
  );

  const 获取已选标签分组 = React.useCallback(
    (已选标签ID映射: 标签选择映射) => {
      return 正式标签种类列表
        .map((标签种类) => ({
          标签种类,
          标签列表: (已选标签ID映射[标签种类.id] ?? [])
            .map((标签ID) => 扁平标签字典.get(标签ID))
            .filter((标签): 标签 is 标签项 => Boolean(标签)),
        }))
        .filter((项目) => 项目.标签列表.length > 0);
    },
    [正式标签种类列表, 扁平标签字典]
  );

  const 获取题目标签列表 = React.useCallback(
    (标签ID列表: number[]) => {
      return 标签ID列表
        .map((标签ID) => 扁平标签字典.get(标签ID))
        .filter((标签): 标签 is 标签项 => Boolean(标签))
        .filter((标签) => {
          const 标签种类 = 标签种类字典.get(标签.标签种类ID);
          return Boolean(标签种类?.是否在正式工作流中可见);
        })
        .sort((前一个, 后一个) => {
          if (前一个.标签种类ID !== 后一个.标签种类ID) {
            return 获取标签种类排序值(前一个.标签种类ID) - 获取标签种类排序值(后一个.标签种类ID);
          }
          if (前一个.同级排序值 !== 后一个.同级排序值) {
            return 前一个.同级排序值 - 后一个.同级排序值;
          }
          return 前一个.id - 后一个.id;
        });
    },
    [扁平标签字典, 标签种类字典]
  );

  const 正式标签搜索项列表 = React.useMemo<标签搜索项[]>(() => {
    const 结果列表: 标签搜索项[] = [];

    const 收集树形标签 = (标签种类: 标签种类项, 标签列表: 标签项[], 祖先路径: string[]) => {
      标签列表.forEach((标签) => {
        const 当前显示名称 = 获取标签显示文本(标签);
        const 当前路径 = [...祖先路径, 当前显示名称];
        const 路径文本 = 当前路径.join(" / ");
        const 展示路径文本 = [...祖先路径].reverse().join(" / ");
        结果列表.push({
          标签ID: 标签.id,
          标签种类ID: 标签种类.id,
          标签种类名称: 标签种类.名称,
          标签名称: 标签.名称,
          完整路径文本: `${标签种类.名称} · ${路径文本}`,
          路径文本,
          展示路径文本,
          是否树形标签: true,
        });

        if (标签.子标签列表.length > 0) {
          收集树形标签(标签种类, 标签.子标签列表, 当前路径);
        }
      });
    };

    正式标签种类列表.forEach((标签种类) => {
      const 标签列表 = 获取指定种类标签列表(标签种类.id);
      if (标签种类.是否树形) {
        收集树形标签(标签种类, 标签列表, []);
        return;
      }

      标签列表.forEach((标签) => {
        const 路径文本 = 获取标签显示文本(标签);
        结果列表.push({
          标签ID: 标签.id,
          标签种类ID: 标签种类.id,
          标签种类名称: 标签种类.名称,
          标签名称: 标签.名称,
          完整路径文本: `${标签种类.名称} · ${路径文本}`,
          路径文本,
          展示路径文本: 路径文本,
          是否树形标签: false,
        });
      });
    });

    return 结果列表;
  }, [正式标签种类列表, 获取指定种类标签列表, 获取标签显示文本]);

  const 录题按钮文本 = React.useMemo(() => {
    if (正在录题) {
      return "正在录题...";
    }
    if (最近录入题目ID !== null) {
      return `已录入，题目ID：${最近录入题目ID}`;
    }
    return "从当前选区录入";
  }, [正在录题, 最近录入题目ID]);

  const 更新题目按钮文本 = React.useMemo(() => {
    if (正在更新题目) {
      return "正在更新...";
    }
    if (更新题目按钮失败提示 !== "") {
      return 更新题目按钮失败提示;
    }
    if (最近更新题目ID !== null) {
      return `已更新，题目ID：${最近更新题目ID}`;
    }
    return "更新当前题目";
  }, [正在更新题目, 更新题目按钮失败提示, 最近更新题目ID]);

  const 切换标签选择状态 = React.useCallback(
    (原映射: 标签选择映射, 标签种类: 标签种类项, 标签ID: number) => {
      const 当前已选标签ID列表 = 原映射[标签种类.id] ?? [];
      const 已存在 = 当前已选标签ID列表.includes(标签ID);
      if (标签种类.是否允许多选) {
        return {
          ...原映射,
          [标签种类.id]: 已存在
            ? 当前已选标签ID列表.filter((当前标签ID) => 当前标签ID !== 标签ID)
            : [...当前已选标签ID列表, 标签ID],
        };
      }
      return {
        ...原映射,
        [标签种类.id]: 已存在 ? [] : [标签ID],
      };
    },
    []
  );

  const 切换录题标签 = (标签种类: 标签种类项, 标签ID: number) => {
    设置最近录入题目ID(null);
    设置录题标签选择((当前映射) => 切换标签选择状态(当前映射, 标签种类, 标签ID));
  };

  const 通过搜索选择录题标签 = React.useCallback(
    (标签ID: number, 标签种类ID: number) => {
      const 标签种类 = 标签种类字典.get(标签种类ID);
      if (!标签种类) {
        return;
      }
      设置最近录入题目ID(null);
      设置录题标签选择((当前映射) => 选择指定标签(当前映射, 标签种类, 标签ID));
    },
    [标签种类字典]
  );

  const 打开快速新增表单 = React.useCallback((标签种类: 标签种类项, 父标签: 标签项 | null = null) => {
    设置快速新增目标({
      标签种类ID: 标签种类.id,
      父标签ID: 父标签?.id ?? null,
      父标签名称: 父标签 ? 获取标签显示文本(父标签) : null,
    });
    设置快速新增表单(创建空快速新增表单());
    设置快速新增错误("");
  }, [获取标签显示文本]);

  const 关闭快速新增表单 = React.useCallback(() => {
    设置快速新增目标(null);
    设置快速新增表单(创建空快速新增表单());
    设置快速新增错误("");
  }, []);

  const 更新快速新增表单字段 = React.useCallback((字段: keyof 快速新增标签表单, 值: string) => {
    设置快速新增表单((当前表单) => ({ ...当前表单, [字段]: 值 }));
  }, []);

  const 提交快速新增标签 = React.useCallback(async () => {
    if (!快速新增目标) {
      return;
    }
    if (快速新增表单.名称.trim() === "") {
      设置快速新增错误("标签名称不能为空。");
      return;
    }
    const 标签种类 = 标签种类字典.get(快速新增目标.标签种类ID);
    if (!标签种类) {
      设置快速新增错误("目标标签种类不存在。");
      return;
    }

    try {
      设置正在保存标签(true);
      设置快速新增错误("");
      const 响应 = await fetch(构建题库接口路径(当前题库键, "/标签"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          标签种类ID: 标签种类.id,
          名称: 快速新增表单.名称.trim(),
          Description: 快速新增表单.描述.trim() === "" ? null : 快速新增表单.描述.trim(),
          ParentId: 标签种类.是否树形 ? 快速新增目标.父标签ID : null,
          NumericValue: 标签种类.id === 系统标签种类.难度 ? 解析小数文本(快速新增表单.数值文本) : null,
          IsEnabled: true,
        }),
      });
      if (!响应.ok) {
        const 错误文本 = await 响应.text();
        throw new Error(错误文本 || "新增标签失败。");
      }
      const 新标签 = (await 响应.json()) as 标签项;
      await 读取标签基础数据(当前题库键);
      设置最近录入题目ID(null);
      设置录题标签选择((当前映射) => 选择指定标签(当前映射, 标签种类, 新标签.id));
      关闭快速新增表单();
    } catch (error) {
      console.error(error);
      设置快速新增错误(
        error instanceof Error && error.message.trim() !== "" ? error.message : "新增标签失败。"
      );
    } finally {
      设置正在保存标签(false);
    }
  }, [关闭快速新增表单, 快速新增目标, 快速新增表单, 当前题库键, 标签种类字典, 读取标签基础数据]);

  const 切换筛选步骤标签 = (步骤编号: number, 标签种类: 标签种类项, 标签ID: number) => {
    设置筛选步骤列表((当前步骤列表) =>
      当前步骤列表.map((步骤) => {
        if (步骤.步骤编号 !== 步骤编号) {
          return 步骤;
        }
        return { ...步骤, 已选标签ID映射: 切换标签选择状态(步骤.已选标签ID映射, 标签种类, 标签ID) };
      })
    );
  };

  const 通过搜索选择筛选步骤标签 = React.useCallback(
    (步骤编号: number, 标签ID: number, 标签种类ID: number) => {
      const 标签种类 = 标签种类字典.get(标签种类ID);
      if (!标签种类) {
        return;
      }
      设置筛选步骤列表((当前步骤列表) =>
        当前步骤列表.map((步骤) => {
          if (步骤.步骤编号 !== 步骤编号) {
            return 步骤;
          }

          return {
            ...步骤,
            已选标签ID映射: 选择指定标签(步骤.已选标签ID映射, 标签种类, 标签ID),
          };
        })
      );
    },
    [标签种类字典]
  );

  const 移除筛选步骤标签 = (步骤编号: number, 标签种类ID: number, 标签ID: number) => {
    设置筛选步骤列表((当前步骤列表) =>
      当前步骤列表.map((步骤) => {
        if (步骤.步骤编号 !== 步骤编号) {
          return 步骤;
        }
        return {
          ...步骤,
          已选标签ID映射: {
            ...步骤.已选标签ID映射,
            [标签种类ID]: (步骤.已选标签ID映射[标签种类ID] ?? []).filter(
              (当前标签ID) => 当前标签ID !== 标签ID
            ),
          },
        };
      })
    );
  };

  const 录入当前选区题目 = async () => {
    const 已选标签ID列表 = Object.values(录题标签选择).flat();
    if (已选标签ID列表.length === 0) {
      设置录题错误("请至少选择一个标签。");
      设置录题成功提示("");
      设置最近录入题目ID(null);
      return;
    }
    try {
      设置正在录题(true);
      设置录题错误("");
      设置录题成功提示("");
      设置最近录入题目ID(null);
      设置更新题目错误("");
      设置更新题目成功提示("");
      设置最近更新题目ID(null);
      const Ooxml内容 = await 获取当前选区Ooxml();
      const 响应 = await fetch(构建题库接口路径(当前题库键, "/题目/ooxml"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          Description: 录题描述.trim() === "" ? null : 录题描述.trim(),
          标签ID列表: 已选标签ID列表,
          Ooxml内容,
        }),
      });
      if (!响应.ok) {
        const 错误文本 = await 响应.text();
        throw new Error(错误文本 || "录题失败。");
      }
      const 新题目 = (await 响应.json()) as { id: number };
      设置录题描述("");
      if (!连续录入已开启) {
        设置录题标签选择({});
      }
      设置最近录入题目ID(新题目.id);
      设置录题成功提示(`录题成功，题目ID：${新题目.id}`);
    } catch (error) {
      console.error(error);
      设置录题错误(
        error instanceof Error && error.message.trim() !== ""
          ? error.message
          : "从当前选区录题失败。"
      );
      设置录题成功提示("");
      设置最近录入题目ID(null);
    } finally {
      设置正在录题(false);
    }
  };

  const 更新当前题目 = async () => {
    try {
      设置正在更新题目(true);
      设置更新题目错误("");
      设置更新题目成功提示("");
      设置最近更新题目ID(null);
      设置更新题目按钮失败提示("");
      设置录题错误("");
      设置录题成功提示("");
      设置最近录入题目ID(null);

      const 当前题目上下文 = await 获取当前题目内容控件上下文();
      const 响应 = await fetch(
        构建题库接口路径(当前题库键, `/题目/${当前题目上下文.题目ID}/ooxml`),
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            Ooxml内容: 当前题目上下文.OOXML内容,
          }),
        }
      );

      if (!响应.ok) {
        if (响应.status === 404) {
          throw new Error("当前题目在题库中不存在。");
        }

        const 错误文本 = await 响应.text();
        throw new Error(错误文本 || "更新题目失败。");
      }

      设置最近更新题目ID(当前题目上下文.题目ID);
      设置更新题目成功提示(`更新成功，题目ID：${当前题目上下文.题目ID}`);
      设置更新题目按钮失败提示("");
    } catch (error) {
      console.error(error);
      const 错误消息 =
        error instanceof Error && error.message.trim() !== "" ? error.message : "更新题目失败。";
      设置更新题目错误(错误消息);
      设置更新题目成功提示("");
      设置最近更新题目ID(null);
      设置更新题目按钮失败提示(截断文本(错误消息, 18));
    } finally {
      设置正在更新题目(false);
    }
  };

  const 开始下一步筛选 = () => {
    设置筛选步骤列表((当前步骤列表) => [...当前步骤列表, 创建空筛选步骤(当前步骤列表.length + 1)]);
  };

  const 切换当前步骤组合方式 = () => {
    if (!当前筛选步骤) {
      return;
    }
    设置筛选步骤列表((当前步骤列表) =>
      当前步骤列表.map((步骤) => {
        if (步骤.步骤编号 !== 当前筛选步骤.步骤编号) {
          return 步骤;
        }
        return { ...步骤, 组合方式: 步骤.组合方式 === "交集" ? "并集" : "交集" };
      })
    );
  };

  const 执行筛题 = async () => {
    const 有效步骤列表 = 筛选步骤列表
      .map((步骤) => ({ ...步骤, 标签ID列表: Object.values(步骤.已选标签ID映射).flat() }))
      .filter((步骤) => 步骤.标签ID列表.length > 0);
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
      设置已选题目ID列表([]);
      设置删除确认状态(null);
      设置正在删除题目ID(null);
      设置删除题目错误("");
      设置插题错误("");
      设置插题成功提示("");
      const 响应 = await fetch(构建题库接口路径(当前题库键, "/题目/筛选"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(
          有效步骤列表.map((步骤, 索引) => ({
            标签ID列表: 步骤.标签ID列表,
            本步标签组合方式: 步骤.组合方式,
            与前一步结果组合方式: 索引 === 0 ? "交集" : "交集",
          }))
        ),
      });
      if (!响应.ok) {
        const 错误文本 = await 响应.text();
        throw new Error(错误文本 || "筛题失败。");
      }
      const 题目列表 = (await 响应.json()) as 题目项[];
      const 题目卡片列表 = await Promise.all(
        题目列表.map(async (题目) => {
          let 预览Html = "<p>暂时无法加载题目预览。</p>";
          try {
            const 预览响应 = await fetch(构建题库接口路径(当前题库键, `/题目/${题目.id}/预览html`));
            if (预览响应.ok) {
              预览Html = await 预览响应.text();
            }
          } catch (error) {
            console.error(error);
          }
          const 标签列表 = 获取题目标签列表(题目.标签ID列表);
          const 难度标签 = 标签列表.find((标签) => 标签.标签种类ID === 系统标签种类.难度);
          return {
            id: 题目.id,
            description: 题目.description,
            标题: 构建内容控件标题(
              难度标签 ? 获取标签显示文本(难度标签) : undefined,
              题目.description
            ),
            标签列表,
            预览Html,
          };
        })
      );
      设置筛题结果卡片列表(题目卡片列表);
    } catch (error) {
      console.error(error);
      设置筛题错误(
        error instanceof Error && error.message.trim() !== ""
          ? error.message
          : "筛题失败，请确认本地服务正在运行。"
      );
      设置筛题结果卡片列表([]);
      设置删除确认状态(null);
      设置正在删除题目ID(null);
    } finally {
      设置正在筛题(false);
    }
  };

  const 切换题目选择状态 = (题目ID: number) => {
    设置删除题目错误("");
    设置已选题目ID列表((当前题目ID列表) =>
      当前题目ID列表.includes(题目ID)
        ? 当前题目ID列表.filter((当前题目ID) => 当前题目ID !== 题目ID)
        : [...当前题目ID列表, 题目ID]
    );
  };

  const 切换全选当前结果 = () => {
    设置删除题目错误("");
    设置已选题目ID列表((当前题目ID列表) => {
      if (当前结果已全选) {
        return 当前题目ID列表.filter((题目ID) => !当前结果题目ID集合.has(题目ID));
      }

      const 结果 = [...当前题目ID列表];
      当前结果题目ID列表.forEach((题目ID) => {
        if (!结果.includes(题目ID)) {
          结果.push(题目ID);
        }
      });
      return 结果;
    });
  };

  const 删除当前题目 = async (题目ID: number) => {
    try {
      设置正在删除题目ID(题目ID);
      设置删除题目错误("");
      const 响应 = await fetch(构建题库接口路径(当前题库键, `/题目/${题目ID}`), {
        method: "DELETE",
      });
      if (响应.status === 404) {
        throw new Error("题目不存在，可能已被删除。");
      }
      if (!响应.ok) {
        const 错误文本 = await 响应.text();
        throw new Error(错误文本 || "删除题目失败。");
      }

      设置筛题结果卡片列表((当前列表) => 当前列表.filter((题目卡片) => 题目卡片.id !== 题目ID));
      设置已选题目ID列表((当前题目ID列表) => 当前题目ID列表.filter((当前题目ID) => 当前题目ID !== 题目ID));
      设置删除确认状态(null);
    } catch (error) {
      console.error(error);
      设置删除题目错误(
        error instanceof Error && error.message.trim() !== "" ? error.message : "删除题目失败。"
      );
    } finally {
      设置正在删除题目ID(null);
    }
  };

  const 点击删除题目按钮 = async (题目ID: number) => {
    if (正在删除题目ID !== null) {
      return;
    }

    设置删除题目错误("");

    if (!删除确认状态 || 删除确认状态.题目ID !== 题目ID) {
      设置删除确认状态({ 题目ID, 阶段: 1 });
      return;
    }

    if (删除确认状态.阶段 === 1) {
      设置删除确认状态({ 题目ID, 阶段: 2 });
      return;
    }

    await 删除当前题目(题目ID);
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
      设置删除题目错误("");
      const 题目卡片字典 = new Map(
        筛题结果卡片列表.map((题目卡片) => [题目卡片.id, 题目卡片] as const)
      );
      const 待插入题目列表 = await Promise.all(
        已选题目ID列表.map(async (题目ID) => {
          const 响应 = await fetch(构建题库接口路径(当前题库键, `/题目/${题目ID}/文件base64`));
          if (!响应.ok) {
            throw new Error(`题目 ${题目ID} 的原文件读取失败。`);
          }
          return {
            题目ID,
            文件Base64: await 响应.text(),
            标题: 题目卡片字典.get(题目ID)?.标题 ?? "题目",
          };
        })
      );
      await 插入题目到当前文档(待插入题目列表);
      设置已选题目ID列表([]);
      设置插题成功提示(`已成功插入 ${待插入题目列表.length} 道题目。`);
    } catch (error) {
      console.error(error);
      设置插题错误(
        error instanceof Error && error.message.trim() !== "" ? error.message : "插入已选题目失败。"
      );
      设置插题成功提示("");
    } finally {
      设置正在插题(false);
    }
  };

  const 更新新增标签表单字段 = (标签种类ID: number, 字段: keyof 标签新增表单, 值: string) => {
    设置新增标签表单映射((当前映射) => ({
      ...当前映射,
      [标签种类ID]: { ...(当前映射[标签种类ID] ?? 创建空新增表单()), [字段]: 值 },
    }));
  };

  const 新增标签 = async (标签种类: 标签种类项) => {
    const 表单 = 新增标签表单映射[标签种类.id] ?? 创建空新增表单();
    if (表单.名称.trim() === "") {
      设置标签整理错误("标签名称不能为空。");
      设置标签整理成功提示("");
      return;
    }
    try {
      设置正在保存标签(true);
      设置标签整理错误("");
      设置标签整理成功提示("");
      const 响应 = await fetch(构建题库接口路径(当前题库键, "/标签"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          标签种类ID: 标签种类.id,
          名称: 表单.名称.trim(),
          Description: 表单.描述.trim() === "" ? null : 表单.描述.trim(),
          ParentId: 标签种类.是否树形 ? 解析整数文本(表单.父标签ID文本) : null,
          NumericValue: 标签种类.id === 系统标签种类.难度 ? 解析小数文本(表单.数值文本) : null,
          IsEnabled: true,
        }),
      });
      if (!响应.ok) {
        const 错误文本 = await 响应.text();
        throw new Error(错误文本 || "新增标签失败。");
      }
      设置新增标签表单映射((当前映射) => ({ ...当前映射, [标签种类.id]: 创建空新增表单() }));
      设置标签整理成功提示(`已在 ${标签种类.名称} 中新增标签。`);
      await 读取标签基础数据(当前题库键);
    } catch (error) {
      console.error(error);
      设置标签整理错误(
        error instanceof Error && error.message.trim() !== "" ? error.message : "新增标签失败。"
      );
      设置标签整理成功提示("");
    } finally {
      设置正在保存标签(false);
    }
  };
  const 开始编辑标签 = (标签: 标签项) => {
    设置编辑标签表单({
      id: 标签.id,
      标签种类ID: 标签.标签种类ID,
      名称: 标签.名称,
      描述: 标签.description ?? "",
      父标签ID文本: 标签.parentId ? String(标签.parentId) : "",
      数值文本:
        标签.numericValue !== null && 标签.numericValue !== undefined
          ? String(标签.numericValue)
          : "",
      isEnabled: 标签.isEnabled,
    });
    设置标签整理错误("");
    设置标签整理成功提示("");
  };

  const 保存标签编辑 = async () => {
    if (!编辑标签表单) {
      return;
    }
    if (编辑标签表单.名称.trim() === "") {
      设置标签整理错误("标签名称不能为空。");
      设置标签整理成功提示("");
      return;
    }
    try {
      设置正在保存标签(true);
      设置标签整理错误("");
      设置标签整理成功提示("");
      const 目标标签种类 = 标签种类字典.get(编辑标签表单.标签种类ID);
      if (!目标标签种类) {
        throw new Error("目标标签种类不存在。");
      }
      const 更新响应 = await fetch(构建题库接口路径(当前题库键, `/标签/${编辑标签表单.id}`), {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          标签种类ID: 编辑标签表单.标签种类ID,
          名称: 编辑标签表单.名称.trim(),
          Description: 编辑标签表单.描述.trim() === "" ? null : 编辑标签表单.描述.trim(),
          NumericValue:
            目标标签种类.id === 系统标签种类.难度 ? 解析小数文本(编辑标签表单.数值文本) : null,
          IsEnabled: 编辑标签表单.isEnabled,
        }),
      });
      if (!更新响应.ok) {
        const 错误文本 = await 更新响应.text();
        throw new Error(错误文本 || "更新标签失败。");
      }
      if (目标标签种类.是否树形) {
        const 父级响应 = await fetch(
          构建题库接口路径(当前题库键, `/标签/${编辑标签表单.id}/调整父级`),
          {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ 新父标签ID: 解析整数文本(编辑标签表单.父标签ID文本) }),
          }
        );
        if (!父级响应.ok) {
          const 错误文本 = await 父级响应.text();
          throw new Error(错误文本 || "调整父级失败。");
        }
      }
      设置编辑标签表单(null);
      设置标签整理成功提示("标签已更新。");
      await 读取标签基础数据(当前题库键);
    } catch (error) {
      console.error(error);
      设置标签整理错误(
        error instanceof Error && error.message.trim() !== "" ? error.message : "更新标签失败。"
      );
      设置标签整理成功提示("");
    } finally {
      设置正在保存标签(false);
    }
  };

  const 调整标签排序 = async (标签ID: number, 方向: "上移" | "下移") => {
    try {
      设置正在保存标签(true);
      设置标签整理错误("");
      设置标签整理成功提示("");
      const 响应 = await fetch(构建题库接口路径(当前题库键, `/标签/${标签ID}/同级排序`), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ 方向 }),
      });
      if (!响应.ok) {
        const 错误文本 = await 响应.text();
        throw new Error(错误文本 || "调整排序失败。");
      }
      设置标签整理成功提示("标签顺序已更新。");
      await 读取标签基础数据(当前题库键);
    } catch (error) {
      console.error(error);
      设置标签整理错误(
        error instanceof Error && error.message.trim() !== "" ? error.message : "调整排序失败。"
      );
      设置标签整理成功提示("");
    } finally {
      设置正在保存标签(false);
    }
  };

  const 删除标签 = async (标签ID: number) => {
    try {
      设置正在保存标签(true);
      设置标签整理错误("");
      设置标签整理成功提示("");
      const 响应 = await fetch(构建题库接口路径(当前题库键, `/标签/${标签ID}`), {
        method: "DELETE",
      });
      if (!响应.ok) {
        const 错误文本 = await 响应.text();
        throw new Error(错误文本 || "删除标签失败。");
      }
      if (编辑标签表单?.id === 标签ID) {
        设置编辑标签表单(null);
      }
      设置标签整理成功提示("标签已删除。");
      await 读取标签基础数据(当前题库键);
    } catch (error) {
      console.error(error);
      设置标签整理错误(
        error instanceof Error && error.message.trim() !== "" ? error.message : "删除标签失败。"
      );
      设置标签整理成功提示("");
    } finally {
      设置正在保存标签(false);
    }
  };

  const 获取父标签候选列表 = React.useCallback(
    (标签种类ID: number, 排除标签ID?: number) =>
      获取指定种类扁平标签列表(标签种类ID).filter((标签) => 标签.id !== 排除标签ID),
    [获取指定种类扁平标签列表]
  );

  const 渲染已选标签摘要 = (
    已选标签ID映射: 标签选择映射,
    移除标签?: (标签种类ID: number, 标签ID: number) => void,
    交互模式: "默认" | "点击标签移除" = "默认"
  ) => {
    const 已选分组列表 = 获取已选标签分组(已选标签ID映射);
    if (已选分组列表.length === 0) {
      return <p className={styles.noteText}>当前还没有选择标签。</p>;
    }
    return (
      <div className={styles.column}>
        {已选分组列表.map((项目) => (
          <div key={项目.标签种类.id} className={styles.column}>
            <span className={styles.label}>{项目.标签种类.名称}</span>
            <div className={styles.chipRow}>
              {项目.标签列表.map((标签) =>
                交互模式 === "点击标签移除" ? (
                  <TagBadge
                    key={标签.id}
                    文本={获取标签显示文本(标签)}
                    强调
                    onClick={
                      移除标签 ? () => 移除标签(项目.标签种类.id, 标签.id) : undefined
                    }
                  />
                ) : (
                  <span key={标签.id} className={`${styles.chip} ${styles.selectedChip}`}>
                    {获取标签显示文本(标签)}
                    {移除标签 && (
                      <button
                        type="button"
                        className={styles.secondaryButton}
                        onClick={() => 移除标签(项目.标签种类.id, 标签.id)}
                      >
                        取消
                      </button>
                    )}
                  </span>
                )
              )}
            </div>
          </div>
        ))}
      </div>
    );
  };

  const 渲染平铺选择 = (
    标签种类: 标签种类项,
    标签列表: 标签项[],
    已选标签ID映射: 标签选择映射,
    点击标签: (标签种类: 标签种类项, 标签ID: number) => void
  ) => {
    if (标签列表.length === 0) {
      return <p className={styles.noteText}>当前没有可选标签。</p>;
    }
    const 已选标签ID列表 = 已选标签ID映射[标签种类.id] ?? [];
    return (
      <div className={styles.chipRow}>
        {标签列表.map((标签) => {
          const 已选中 = 已选标签ID列表.includes(标签.id);
          return (
            <button
              key={标签.id}
              type="button"
              className={`${styles.chip} ${已选中 ? styles.selectedChip : ""}`}
              onClick={() => 点击标签(标签种类, 标签.id)}
            >
              {获取标签显示文本(标签)}
            </button>
          );
        })}
      </div>
    );
  };

  const 渲染快速新增表单 = (标签种类: 标签种类项, 父标签名称?: string | null) => (
    <QuickAddTagForm
      标题={
        父标签名称
          ? `新增 ${标签种类.名称} 子标签`
          : 标签种类.是否树形
            ? `新增 ${标签种类.名称} 根标签`
            : `新增 ${标签种类.名称}`
      }
      父标签名称={父标签名称}
      表单={快速新增表单}
      是否显示数值输入={标签种类.id === 系统标签种类.难度}
      错误信息={快速新增错误}
      正在保存={正在保存标签}
      onChange={更新快速新增表单字段}
      onSubmit={提交快速新增标签}
      onCancel={关闭快速新增表单}
    />
  );

  const 渲染管理树 = (标签列表: 标签项[], 层级 = 0): React.ReactNode => {
    if (标签列表.length === 0) {
      return <p className={styles.noteText}>当前还没有标签。</p>;
    }
    return (
      <div className={styles.treeBlock}>
        {标签列表.map((标签) => (
          <div key={标签.id} className={styles.treeBlock}>
            <div className={styles.treeRow} style={{ paddingLeft: `${层级 * 18}px` }}>
              <div className={styles.treeInfo}>
                <span className={styles.tagName}>{获取标签显示文本(标签)}</span>
                <span className={styles.meta}>
                  {标签.description ? `描述：${标签.description} ` : ""}
                  {标签.numericValue !== null && 标签.numericValue !== undefined
                    ? `数值：${标签.numericValue} `
                    : ""}
                  {!标签.isEnabled ? "已停用" : ""}
                </span>
              </div>
              <div className={styles.row}>
                <button
                  type="button"
                  className={styles.secondaryButton}
                  onClick={() => 开始编辑标签(标签)}
                >
                  编辑
                </button>
                <button
                  type="button"
                  className={styles.secondaryButton}
                  onClick={() => 调整标签排序(标签.id, "上移")}
                >
                  上移
                </button>
                <button
                  type="button"
                  className={styles.secondaryButton}
                  onClick={() => 调整标签排序(标签.id, "下移")}
                >
                  下移
                </button>
                <button
                  type="button"
                  className={styles.secondaryButton}
                  onClick={() => 删除标签(标签.id)}
                >
                  删除
                </button>
              </div>
            </div>
            {标签.子标签列表 && 标签.子标签列表.length > 0 && 渲染管理树(标签.子标签列表, 层级 + 1)}
          </div>
        ))}
      </div>
    );
  };

  const 渲染管理平铺 = (标签列表: 标签项[]) => {
    if (标签列表.length === 0) {
      return <p className={styles.noteText}>当前还没有标签。</p>;
    }
    return (
      <div className={styles.treeBlock}>
        {标签列表.map((标签) => (
          <div key={标签.id} className={styles.treeRow}>
            <div className={styles.treeInfo}>
              <span className={styles.tagName}>{获取标签显示文本(标签)}</span>
              <span className={styles.meta}>
                {标签.description ? `描述：${标签.description} ` : ""}
                {标签.numericValue !== null && 标签.numericValue !== undefined
                  ? `数值：${标签.numericValue} `
                  : ""}
                {!标签.isEnabled ? "已停用" : ""}
              </span>
            </div>
            <div className={styles.row}>
              <button
                type="button"
                className={styles.secondaryButton}
                onClick={() => 开始编辑标签(标签)}
              >
                编辑
              </button>
              <button
                type="button"
                className={styles.secondaryButton}
                onClick={() => 调整标签排序(标签.id, "上移")}
              >
                上移
              </button>
              <button
                type="button"
                className={styles.secondaryButton}
                onClick={() => 调整标签排序(标签.id, "下移")}
              >
                下移
              </button>
              <button
                type="button"
                className={styles.secondaryButton}
                onClick={() => 删除标签(标签.id)}
              >
                删除
              </button>
            </div>
          </div>
        ))}
      </div>
    );
  };
  const 渲染首页 = () => (
    <div className={styles.root}>
      <div className={styles.container}>
        <h1 className={styles.title}>{props.title}</h1>
        <p className={styles.subtitle}>
          当前系统已经切到多题库结构。先在首页选择题库，再进入录题、筛题或标签整理。
        </p>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>当前题库</h2>
          {正在加载题库实例 && <p className={styles.noteText}>正在读取题库实例...</p>}
          {题库实例错误 !== "" && <p className={styles.errorText}>{题库实例错误}</p>}
          {题库实例错误 === "" && (
            <div className={styles.column}>
              <label className={styles.label} htmlFor="bank-selector">
                首页全局题库选择
              </label>
              <select
                id="bank-selector"
                className={styles.select}
                value={当前题库键}
                onChange={(事件) => 设置当前题库键(事件.target.value)}
              >
                {题库实例列表.map((题库实例) => (
                  <option key={题库实例.题库键} value={题库实例.题库键}>
                    {题库实例.显示名称}（{题库实例.题库键}）
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>当前题库摘要</h2>
          {!当前题库实例 && <p className={styles.noteText}>当前还没有可用题库。</p>}
          {当前题库实例 && (
            <div className={styles.column}>
              <p className={styles.noteText}>题库键：{当前题库实例.题库键}</p>
              <p className={styles.noteText}>题目数量：{当前题库实例.题目数量}</p>
              <p className={styles.noteText}>标签数量：{当前题库实例.标签数量}</p>
              <p className={styles.noteText}>
                标签基础数据：
                {正在加载标签基础数据
                  ? "正在读取..."
                  : 标签基础数据错误 !== ""
                    ? 标签基础数据错误
                    : "已就绪"}
              </p>
            </div>
          )}
        </div>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>主入口</h2>
          <div className={styles.actionGrid}>
            <button
              type="button"
              className={styles.actionButton}
              onClick={() => 设置当前页面("录题页")}
            >
              <p className={styles.actionName}>录入题目</p>
              <p className={styles.actionDescription}>
                在当前题库下，从 Word 选区录题，并按新标签体系分区选择标签。
              </p>
            </button>
            <button
              type="button"
              className={styles.actionButton}
              onClick={() => 设置当前页面("筛题页")}
            >
              <p className={styles.actionName}>筛选题目</p>
              <p className={styles.actionDescription}>
                按章节、做题方法、难度、附加标签和试卷题型分区筛题，并继续插入已选题目。
              </p>
            </button>
            <button
              type="button"
              className={styles.actionButton}
              onClick={() => 设置当前页面("标签整理页")}
            >
              <p className={styles.actionName}>标签整理</p>
              <p className={styles.actionDescription}>
                维护章节树、做题方法树、难度、附加标签、试卷题型和迁移期的待整理标签。
              </p>
            </button>
          </div>
        </div>
      </div>
    </div>
  );

  const 渲染录题页 = () => (
    <div className={styles.root}>
      <div className={styles.container}>
        <button type="button" className={styles.backButton} onClick={() => 设置当前页面("首页")}>
          返回首页
        </button>
        <div className={styles.bankBanner}>当前题库：{当前题库键}</div>
        <h1 className={styles.title}>录入题目</h1>
        <p className={styles.subtitle}>
          先在 Word 中选中题目内容，再填写描述、选择标签，然后从当前选区录入。
        </p>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>题目描述</h2>
          <textarea
            className={styles.textArea}
            value={录题描述}
            onChange={(事件) => 设置录题描述(事件.target.value)}
          />
        </div>
        <div className={styles.section}>
          <div className={styles.selectedTagsCardHeader}>
            <h2 className={styles.sectionTitle}>已选标签</h2>
            <div className={styles.selectedTagsCardActions}>
              <ToggleButton
                checked={连续录入已开启}
                appearance="outline"
                className={styles.continuousToggle}
                style={
                  连续录入已开启
                    ? {
                        borderColor: "#c9952e",
                        backgroundColor: "#fde8b2",
                        color: "#5c3b00",
                        boxShadow: "0 4px 10px rgba(160, 112, 9, 0.16)",
                        fontWeight: 700,
                      }
                    : {
                        borderColor: "#d8cfc0",
                        backgroundColor: "#ffffff",
                        color: "#5a544a",
                        boxShadow: "none",
                      }
                }
                onClick={() => 设置连续录入已开启((当前值) => !当前值)}
              >
                连续录入
              </ToggleButton>
              <button
                type="button"
                className={`${styles.button} ${styles.selectedTagsRecordButton}`}
                onClick={录入当前选区题目}
                disabled={正在录题 || 正在更新题目}
              >
                {录题按钮文本}
              </button>
              <button
                type="button"
                className={`${styles.secondaryButton} ${styles.selectedTagsRecordButton}`}
                onClick={更新当前题目}
                disabled={正在录题 || 正在更新题目}
              >
                {更新题目按钮文本}
              </button>
            </div>
          </div>
          {渲染已选标签摘要(
            录题标签选择,
            (标签种类ID, 标签ID) => {
              const 标签种类 = 标签种类字典.get(标签种类ID);
              if (标签种类) {
                切换录题标签(标签种类, 标签ID);
              }
            },
            "点击标签移除"
          )}
        </div>
        <div className={styles.section}>
          <TagSearchPanel
            标题="标签关键字搜索"
            提示文本="输入关键字，直接搜索并选中标签"
            标签搜索项列表={正式标签搜索项列表}
            已选标签ID列表={Object.values(录题标签选择).flat()}
            选择标签={通过搜索选择录题标签}
          />
        </div>
        {正式标签种类列表.map((标签种类) => (
          <div key={标签种类.id} className={styles.section}>
            <div className={styles.sectionHeaderRow}>
              <h2 className={styles.sectionTitle}>{标签种类.名称}</h2>
              <button
                type="button"
                className={styles.secondaryButton}
                onClick={() => 打开快速新增表单(标签种类)}
              >
                {标签种类.是否树形 ? "新增根标签" : "新增标签"}
              </button>
            </div>
            {快速新增目标?.标签种类ID === 标签种类.id && 快速新增目标.父标签ID === null && (
              <div className={styles.inlineFormHost}>{渲染快速新增表单(标签种类)}</div>
            )}
            {标签种类.是否树形
              ? (
                  <TagSelectionTree
                    树名称={`${标签种类.名称}标签树`}
                    标签列表={获取指定种类标签列表(标签种类.id)}
                    已选标签ID列表={录题标签选择[标签种类.id] ?? []}
                    获取标签显示文本={获取标签显示文本}
                    切换标签={(标签ID) => 切换录题标签(标签种类, 标签ID)}
                    渲染节点附加操作={(标签) => (
                      <button
                        type="button"
                        className={styles.secondaryButton}
                        onClick={(事件) => {
                          事件.preventDefault();
                          事件.stopPropagation();
                          打开快速新增表单(标签种类, 标签 as 标签项);
                        }}
                      >
                        新增子标签
                      </button>
                    )}
                    渲染节点下方内容={(标签) =>
                      快速新增目标?.标签种类ID === 标签种类.id && 快速新增目标.父标签ID === 标签.id ? (
                        <div className={styles.inlineFormHost}>
                          {渲染快速新增表单(标签种类, 获取标签显示文本(标签 as 标签项))}
                        </div>
                      ) : null
                    }
                  />
                )
              : 渲染平铺选择(
                  标签种类,
                  获取指定种类标签列表(标签种类.id),
                  录题标签选择,
                  切换录题标签
                )}
          </div>
        ))}
        <div className={styles.section}>
          {录题错误 !== "" && <p className={styles.errorText}>{录题错误}</p>}
          {录题成功提示 !== "" && <p className={styles.successText}>{录题成功提示}</p>}
          {更新题目错误 !== "" && <p className={styles.errorText}>{更新题目错误}</p>}
          {更新题目成功提示 !== "" && <p className={styles.successText}>{更新题目成功提示}</p>}
          <div className={styles.row}>
            <button
              type="button"
              className={styles.button}
              onClick={录入当前选区题目}
              disabled={正在录题 || 正在更新题目}
            >
              {录题按钮文本}
            </button>
            <button
              type="button"
              className={styles.secondaryButton}
              onClick={更新当前题目}
              disabled={正在录题 || 正在更新题目}
            >
              {更新题目按钮文本}
            </button>
          </div>
        </div>
      </div>
    </div>
  );

  const 渲染筛题页 = () => (
    <div className={styles.root}>
      <div className={styles.container}>
        <button type="button" className={styles.backButton} onClick={() => 设置当前页面("首页")}>
          返回首页
        </button>
        <div className={styles.bankBanner}>当前题库：{当前题库键}</div>
        <h1 className={styles.title}>筛选题目</h1>
        <p className={styles.subtitle}>
          每一步按标签种类分区筛选，步骤内默认交集，可切换并集；下一步继续在当前结果上筛选。
        </p>
        <div className={styles.column}>
          {筛选步骤列表.map((步骤) => {
            const 是否当前步骤 = 当前筛选步骤?.步骤编号 === 步骤.步骤编号;
            return (
              <div key={步骤.步骤编号} className={styles.section}>
                <div className={styles.row}>
                  <h2 className={styles.sectionTitle}>第{步骤.步骤编号}步筛选条件</h2>
                  {是否当前步骤 && (
                    <label className={styles.row}>
                      <span className={styles.noteText}>并集</span>
                      <input
                        type="checkbox"
                        checked={步骤.组合方式 === "并集"}
                        onChange={切换当前步骤组合方式}
                      />
                    </label>
                  )}
                </div>
                <div className={styles.section}>
                  <h2 className={styles.sectionTitle}>已选标签</h2>
                  {渲染已选标签摘要(
                    步骤.已选标签ID映射,
                    是否当前步骤
                      ? (标签种类ID, 标签ID) => 移除筛选步骤标签(步骤.步骤编号, 标签种类ID, 标签ID)
                      : undefined
                  )}
                </div>
                {是否当前步骤 && (
                  <div className={styles.section}>
                    <TagSearchPanel
                      标题="标签关键字搜索"
                      提示文本="输入关键字，直接加入当前筛选步骤"
                      标签搜索项列表={正式标签搜索项列表}
                      已选标签ID列表={Object.values(步骤.已选标签ID映射).flat()}
                      选择标签={(标签ID, 标签种类ID) =>
                        通过搜索选择筛选步骤标签(步骤.步骤编号, 标签ID, 标签种类ID)
                      }
                    />
                  </div>
                )}
                {正式标签种类列表.map((标签种类) => (
                  <div key={标签种类.id} className={styles.kindSection}>
                    <h2 className={styles.sectionTitle}>{标签种类.名称}</h2>
                    {是否当前步骤
                      ? 标签种类.是否树形
                        ? (
                            <TagSelectionTree
                              树名称={`${标签种类.名称}筛选树`}
                              标签列表={获取指定种类标签列表(标签种类.id)}
                              已选标签ID列表={步骤.已选标签ID映射[标签种类.id] ?? []}
                              获取标签显示文本={获取标签显示文本}
                              切换标签={(标签ID) => 切换筛选步骤标签(步骤.步骤编号, 标签种类, 标签ID)}
                            />
                          )
                        : 渲染平铺选择(
                            标签种类,
                            获取指定种类标签列表(标签种类.id),
                            步骤.已选标签ID映射,
                            (当前标签种类, 标签ID) =>
                              切换筛选步骤标签(步骤.步骤编号, 当前标签种类, 标签ID)
                          )
                      : (() => {
                          const 已选标签列表 = (步骤.已选标签ID映射[标签种类.id] ?? [])
                            .map((标签ID) => 扁平标签字典.get(标签ID))
                            .filter((标签): 标签 is 标签项 => Boolean(标签));
                          return 已选标签列表.length === 0 ? (
                            <p className={styles.noteText}>本步骤未选择该种类标签。</p>
                          ) : (
                            <div className={styles.chipRow}>
                              {已选标签列表.map((标签) => (
                                <span
                                  key={标签.id}
                                  className={`${styles.chip} ${styles.selectedChip}`}
                                >
                                  {获取标签显示文本(标签)}
                                </span>
                              ))}
                            </div>
                          );
                        })()}
                  </div>
                ))}
              </div>
            );
          })}
        </div>
        <div className={styles.row}>
          <button type="button" className={styles.secondaryButton} onClick={开始下一步筛选}>
            开始下一步筛选
          </button>
          <button type="button" className={styles.button} onClick={执行筛题} disabled={正在筛题}>
            {正在筛题 ? "正在筛题..." : "执行筛题"}
          </button>
        </div>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>筛题结果</h2>
          <div className={styles.row}>
            <p className={styles.noteText}>已选：{当前结果已选题目数量} 道</p>
            <p className={styles.noteText}>筛中：{当前结果题目总数} 道</p>
            <button
              type="button"
              className={styles.secondaryButton}
              onClick={切换全选当前结果}
              disabled={当前结果题目总数 === 0}
            >
              {当前结果已全选 ? "取消全选" : "全选当前结果"}
            </button>
            <button
              type="button"
              className={styles.button}
              onClick={插入已选题目}
              disabled={正在插题 || 当前结果已选题目数量 === 0}
            >
              {正在插题 ? "正在插入..." : "插入已选题目"}
            </button>
          </div>
          {筛题错误 !== "" && <p className={styles.errorText}>{筛题错误}</p>}
          {删除题目错误 !== "" && <p className={styles.errorText}>{删除题目错误}</p>}
          {插题错误 !== "" && <p className={styles.errorText}>{插题错误}</p>}
          {插题成功提示 !== "" && <p className={styles.successText}>{插题成功提示}</p>}
          {筛题错误 === "" && 正在筛题 && <p className={styles.noteText}>正在加载筛题结果...</p>}
          {筛题错误 === "" && !正在筛题 && !已执行筛题 && (
            <p className={styles.noteText}>先完成筛选步骤，再执行筛题。</p>
          )}
          {筛题错误 === "" && !正在筛题 && 已执行筛题 && 筛题结果卡片列表.length === 0 && (
            <p className={styles.noteText}>当前筛选条件下还没有匹配到题目。</p>
          )}
          {筛题错误 === "" && 筛题结果卡片列表.length > 0 && (
            <div className={styles.column}>
              {筛题结果卡片列表.map((题目卡片) => {
                const 已选中 = 已选题目ID列表.includes(题目卡片.id);
                return (
                  <QuestionPreviewCard
                    key={题目卡片.id}
                    题目ID={题目卡片.id}
                    描述={题目卡片.description}
                    标签文本列表={题目卡片.标签列表.map((标签) => 获取标签显示文本(标签))}
                    预览Html={题目卡片.预览Html}
                    已选中={已选中}
                    切换选择={() => 切换题目选择状态(题目卡片.id)}
                    删除按钮阶段={
                      删除确认状态?.题目ID !== 题目卡片.id
                        ? "默认"
                        : 删除确认状态.阶段 === 1
                        ? "确认"
                        : "最终确认"
                    }
                    正在删除={正在删除题目ID === 题目卡片.id}
                    点击删除按钮={() => {
                      void 点击删除题目按钮(题目卡片.id);
                    }}
                  />
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );

  const 渲染标签整理页 = () => (
    <div className={styles.root}>
      <div className={styles.container}>
        <button type="button" className={styles.backButton} onClick={() => 设置当前页面("首页")}>
          返回首页
        </button>
        <div className={styles.bankBanner}>当前题库：{当前题库键}</div>
        <h1 className={styles.title}>标签整理</h1>
        <p className={styles.subtitle}>
          这里负责整理章节树、做题方法树、难度、附加标签、试卷题型，以及迁移期的待整理标签。
        </p>
        {标签整理错误 !== "" && (
          <div className={styles.section}>
            <p className={styles.errorText}>{标签整理错误}</p>
          </div>
        )}
        {标签整理成功提示 !== "" && (
          <div className={styles.section}>
            <p className={styles.successText}>{标签整理成功提示}</p>
          </div>
        )}
        {编辑标签表单 && (
          <div className={styles.section}>
            <h2 className={styles.sectionTitle}>编辑标签</h2>
            <div className={styles.column}>
              <label className={styles.label}>标签名称</label>
              <input
                className={styles.input}
                value={编辑标签表单.名称}
                onChange={(事件) => 设置编辑标签表单({ ...编辑标签表单, 名称: 事件.target.value })}
              />
              <label className={styles.label}>标签描述</label>
              <textarea
                className={styles.textArea}
                value={编辑标签表单.描述}
                onChange={(事件) => 设置编辑标签表单({ ...编辑标签表单, 描述: 事件.target.value })}
              />
              <label className={styles.label}>目标种类</label>
              <select
                className={styles.select}
                value={编辑标签表单.标签种类ID}
                onChange={(事件) =>
                  设置编辑标签表单({
                    ...编辑标签表单,
                    标签种类ID: Number(事件.target.value),
                    // 改种类后原父标签通常已不再合法，先清空，避免跨种类父子关系报错。
                    父标签ID文本: "",
                  })
                }
              >
                {标签种类列表.map((标签种类) => (
                  <option key={标签种类.id} value={标签种类.id}>
                    {标签种类.名称}
                  </option>
                ))}
              </select>
              {(编辑标签表单.标签种类ID === 系统标签种类.章节 ||
                编辑标签表单.标签种类ID === 系统标签种类.做题方法 ||
                编辑标签表单.标签种类ID === 系统标签种类.待整理) && (
                <>
                  <label className={styles.label}>父标签</label>
                  <select
                    className={styles.select}
                    value={编辑标签表单.父标签ID文本}
                    onChange={(事件) =>
                      设置编辑标签表单({ ...编辑标签表单, 父标签ID文本: 事件.target.value })
                    }
                  >
                    <option value="">设为根标签</option>
                    {获取父标签候选列表(编辑标签表单.标签种类ID, 编辑标签表单.id).map((标签) => (
                      <option key={标签.id} value={标签.id}>
                        {获取标签显示文本(标签)}
                      </option>
                    ))}
                  </select>
                </>
              )}
              {编辑标签表单.标签种类ID === 系统标签种类.难度 && (
                <>
                  <label className={styles.label}>难度数值</label>
                  <input
                    className={styles.input}
                    value={编辑标签表单.数值文本}
                    onChange={(事件) =>
                      设置编辑标签表单({ ...编辑标签表单, 数值文本: 事件.target.value })
                    }
                  />
                </>
              )}
              <label className={styles.row}>
                <input
                  type="checkbox"
                  checked={编辑标签表单.isEnabled}
                  onChange={(事件) =>
                    设置编辑标签表单({ ...编辑标签表单, isEnabled: 事件.target.checked })
                  }
                />
                启用标签
              </label>
            </div>
            <div className={styles.row}>
              <button
                type="button"
                className={styles.button}
                onClick={保存标签编辑}
                disabled={正在保存标签}
              >
                {正在保存标签 ? "正在保存..." : "保存编辑"}
              </button>
              <button
                type="button"
                className={styles.secondaryButton}
                onClick={() => 设置编辑标签表单(null)}
              >
                取消编辑
              </button>
            </div>
          </div>
        )}
        {标签种类列表.map((标签种类) => {
          const 当前新增表单 = 新增标签表单映射[标签种类.id] ?? 创建空新增表单();
          const 当前标签列表 = 获取指定种类标签列表(标签种类.id);
          const 父标签候选列表 = 获取父标签候选列表(标签种类.id);
          return (
            <div key={标签种类.id} className={styles.section}>
              <h2 className={styles.sectionTitle}>{标签种类.名称}</h2>
              <div className={styles.column}>
                <label className={styles.label}>新增标签名称</label>
                <input
                  className={styles.input}
                  value={当前新增表单.名称}
                  onChange={(事件) => 更新新增标签表单字段(标签种类.id, "名称", 事件.target.value)}
                />
                <label className={styles.label}>新增标签描述</label>
                <textarea
                  className={styles.textArea}
                  value={当前新增表单.描述}
                  onChange={(事件) => 更新新增标签表单字段(标签种类.id, "描述", 事件.target.value)}
                />
                {标签种类.是否树形 && (
                  <>
                    <label className={styles.label}>父标签</label>
                    <select
                      className={styles.select}
                      value={当前新增表单.父标签ID文本}
                      onChange={(事件) =>
                        更新新增标签表单字段(标签种类.id, "父标签ID文本", 事件.target.value)
                      }
                    >
                      <option value="">设为根标签</option>
                      {父标签候选列表.map((标签) => (
                        <option key={标签.id} value={标签.id}>
                          {获取标签显示文本(标签)}
                        </option>
                      ))}
                    </select>
                  </>
                )}
                {标签种类.id === 系统标签种类.难度 && (
                  <>
                    <label className={styles.label}>难度数值</label>
                    <input
                      className={styles.input}
                      value={当前新增表单.数值文本}
                      onChange={(事件) =>
                        更新新增标签表单字段(标签种类.id, "数值文本", 事件.target.value)
                      }
                    />
                  </>
                )}
              </div>
              <div className={styles.row}>
                <button
                  type="button"
                  className={styles.button}
                  onClick={() => 新增标签(标签种类)}
                  disabled={正在保存标签}
                >
                  {正在保存标签 ? "正在保存..." : `新增到${标签种类.名称}`}
                </button>
              </div>
              <div className={styles.section}>
                <h2 className={styles.sectionTitle}>当前标签</h2>
                {标签种类.是否树形 ? 渲染管理树(当前标签列表) : 渲染管理平铺(当前标签列表)}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );

  if (当前页面 === "录题页") {
    return 渲染录题页();
  }
  if (当前页面 === "筛题页") {
    return 渲染筛题页();
  }
  if (当前页面 === "标签整理页") {
    return 渲染标签整理页();
  }
  return 渲染首页();
}
