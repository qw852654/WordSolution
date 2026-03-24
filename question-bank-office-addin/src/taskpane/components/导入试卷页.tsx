import * as React from "react";
import { makeStyles } from "@fluentui/react-components";
import QuickAddTagForm from "./QuickAddTagForm";
import SingleSelectChipGroup from "./SingleSelectChipGroup";
import TagBadge from "./TagBadge";
import TagSearchPanel from "./TagSearchPanel";
import TagSelectionTree from "./TagSelectionTree";
import 标签工作台 from "./标签工作台";
import type { 标签搜索项 } from "../search/tagSearch";

interface 简单标签项 { id: number; 名称: string; }
interface 可映射标签项 extends 简单标签项 {
  标签种类ID: number;
  父标签ID?: number | null;
  标签种类名称: string;
  描述?: string | null;
  numericValue?: number | null;
  isEnabled?: boolean;
}
interface 树形标签节点 extends 可映射标签项 { description?: string | null; 子标签列表: 树形标签节点[]; }
interface 标签种类项 { id: number; 名称: string; 是否树形: boolean; 是否允许多选: boolean; }
interface 题型定义项 { id: number; 名称: string; 描述?: string | null; 排序值: number; }
interface 知识点映射展示项 { 原始知识点文本: string; 是否已解决: boolean; 目标标签ID?: number | null; 目标标签名称?: string | null; 是否抛弃: boolean; }
interface 当前导入题目结果 {
  试卷记录ID: number; 试卷题目项ID: number; 草稿题序号: number; 题号文本: string; 题目摘要: string; 题目预览Html: string;
  推荐题型ID?: number | null; 推荐题型名称?: string | null; 识别说明: string; 置信度: number; 可选题型列表: 题型定义项[];
  原始难度文本: string; 知识点列表: 知识点映射展示项[]; 预填标签ID列表: number[]; 剩余数量: number;
}
interface 开始导入试卷结果 { 试卷记录ID: number; 当前题目?: 当前导入题目结果 | null; 已完成: boolean; }
interface 试卷记录列表项 { 试卷记录ID: number; 显示名称: string; 年份标签ID: number; 年份标签名称: string; 来源标签ID: number; 来源标签名称: string; 总题数: number; 已确认数: number; 已跳过数: number; 状态: string; }
interface 导入试卷页Props {
  当前题库显示名称: string;
  当前题库键: string; 标签种类列表: 标签种类项[]; 年份标签列表: 简单标签项[]; 来源标签列表: 简单标签项[]; 难度标签列表: 简单标签项[];
  可映射标签列表: 可映射标签项[]; 标签搜索项列表: 标签搜索项[]; 构建题库接口路径: (子路径: string) => string; 返回首页: () => void; 刷新标签基础数据: () => Promise<void>;
}
interface 知识点本地决策 { 目标标签ID: number | null; 是否抛弃: boolean; }
interface 快速新增标签表单 { 名称: string; 描述: string; 数值文本: string; }

const 难度标签种类ID = 3;
const 年份标签种类ID = 7;
const 来源标签种类ID = 8;
const useStyles = makeStyles({
  root: { minHeight: "100vh", backgroundColor: "#f6f1e7" },
  container: { padding: "24px 18px 28px" },
  title: { fontSize: "28px", fontWeight: "700", margin: "0 0 8px 0", color: "#1f1f1f" },
  subtitle: { fontSize: "14px", lineHeight: "22px", margin: "0 0 20px 0", color: "#5f5a50" },
  backButton: { padding: "8px 12px", borderRadius: "8px", border: "1px solid #d8cfc0", backgroundColor: "#fff", cursor: "pointer", marginBottom: "16px" },
  bankBanner: { marginBottom: "14px", padding: "10px 12px", borderRadius: "10px", backgroundColor: "#fff8ec", color: "#6a5800", fontSize: "12px" },
  section: { marginTop: "18px", padding: "16px", borderRadius: "14px", backgroundColor: "#fff", border: "1px solid #e4ddd2", display: "grid", gap: "10px" },
  sectionTitle: { fontSize: "16px", fontWeight: "600", margin: 0, color: "#2d2a26" },
  row: { display: "flex", gap: "10px", flexWrap: "wrap", alignItems: "center" },
  between: { display: "flex", gap: "10px", flexWrap: "wrap", alignItems: "center", justifyContent: "space-between" },
  column: { display: "grid", gap: "10px" },
  gridTwo: { display: "grid", gap: "12px", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))" },
  input: { width: "100%", padding: "10px 12px", borderRadius: "8px", border: "1px solid #d8cfc0", fontSize: "13px", boxSizing: "border-box" },
  select: { width: "100%", padding: "10px 12px", borderRadius: "8px", border: "1px solid #d8cfc0", fontSize: "13px", boxSizing: "border-box" },
  button: { padding: "10px 14px", borderRadius: "8px", border: "1px solid #b8860b", backgroundColor: "#f3c86a", color: "#3b2a00", cursor: "pointer", fontSize: "13px" },
  secondaryButton: { padding: "8px 12px", borderRadius: "8px", border: "1px solid #d8cfc0", backgroundColor: "#fff", color: "#2f2a25", cursor: "pointer", fontSize: "12px" },
  noteText: { margin: 0, fontSize: "12px", lineHeight: "18px", color: "#756d60" },
  successText: { margin: 0, fontSize: "13px", lineHeight: "20px", color: "#0f7b0f" },
  errorText: { margin: 0, fontSize: "12px", lineHeight: "18px", color: "#b42318" },
  chipRow: { display: "flex", gap: "8px", flexWrap: "wrap" },
  chip: { padding: "6px 10px", borderRadius: "999px", border: "1px solid #dfd3bc", backgroundColor: "#fff", color: "#524c43", cursor: "pointer", fontSize: "12px" },
  selectedChip: { border: "1px solid #b8860b", backgroundColor: "#f3c86a", color: "#3b2a00" },
  card: { padding: "14px", borderRadius: "12px", border: "1px solid #e4ddd2", backgroundColor: "#fffdf9", display: "grid", gap: "8px" },
  preview: { padding: "14px", borderRadius: "12px", backgroundColor: "#fffdf9", border: "1px solid #ece4d7", overflowX: "auto" },
  mappingItem: { padding: "12px", borderRadius: "10px", border: "1px solid #ece4d7", backgroundColor: "#fffdf9", display: "grid", gap: "8px", transition: "border-color 160ms ease, box-shadow 160ms ease, background-color 160ms ease, transform 160ms ease" },
  processedMappingItem: { backgroundColor: "#fff6db", border: "2px solid #c97800", boxShadow: "0 0 0 2px rgba(201, 120, 0, 0.15), 0 10px 18px rgba(90, 65, 20, 0.12)" },
  resolved: { padding: "10px 12px", borderRadius: "10px", border: "2px solid #c97800", backgroundColor: "#fff6db", boxShadow: "0 0 0 2px rgba(201, 120, 0, 0.12), 0 8px 16px rgba(90, 65, 20, 0.1)" },
  kindSection: { padding: "12px", borderRadius: "12px", backgroundColor: "#fffdf9", border: "1px solid #ece4d7", display: "grid", gap: "10px" },
  quickAddBox: { padding: "12px", borderRadius: "10px", border: "1px solid #e8dcc7", backgroundColor: "#fffaf0", display: "grid", gap: "8px" },
  fileInput: { fontSize: "13px" },
});

const 空新增标签表单 = (): 快速新增标签表单 => ({ 名称: "", 描述: "", 数值文本: "" });
const 获取错误信息 = (error: unknown, 默认文案: string) => error instanceof Error && error.message.trim() !== "" ? error.message : 默认文案;
const 去重 = (ids: number[]) => Array.from(new Set(ids));
const 构建树形标签列表 = (标签列表: 可映射标签项[]): 树形标签节点[] => {
  const 映射 = new Map<number, 树形标签节点>();
  标签列表.forEach((标签) => 映射.set(标签.id, { ...标签, description: 标签.描述 ?? null, 子标签列表: [] }));
  const 根节点: 树形标签节点[] = [];
  映射.forEach((节点) => {
    const 父标签ID = 节点.父标签ID ?? null;
    if (父标签ID !== null && 映射.has(父标签ID)) {
      映射.get(父标签ID)?.子标签列表.push(节点);
      return;
    }
    根节点.push(节点);
  });
  const 排序 = (节点列表: 树形标签节点[]) => { 节点列表.sort((a, b) => a.名称.localeCompare(b.名称, "zh-Hans-CN")); 节点列表.forEach((节点) => 排序(节点.子标签列表)); };
  排序(根节点);
  return 根节点;
};

export default function 导入试卷页(props: 导入试卷页Props) {
  const styles = useStyles();
  const 页面顶部Ref = React.useRef<HTMLDivElement | null>(null);
  const [试卷列表, 设置试卷列表] = React.useState<试卷记录列表项[]>([]);
  const [正在加载试卷列表, 设置正在加载试卷列表] = React.useState(true);
  const [选中文件, 设置选中文件] = React.useState<File | null>(null);
  const [年份标签ID, 设置年份标签ID] = React.useState<number | null>(props.年份标签列表[0]?.id ?? null);
  const [来源标签ID, 设置来源标签ID] = React.useState<number | null>(props.来源标签列表[0]?.id ?? null);
  const [新增年份名称, 设置新增年份名称] = React.useState("");
  const [新增来源名称, 设置新增来源名称] = React.useState("");
  const [当前试卷记录ID, 设置当前试卷记录ID] = React.useState<number | null>(null);
  const [当前题目, 设置当前题目] = React.useState<当前导入题目结果 | null>(null);
  const [当前题型ID, 设置当前题型ID] = React.useState<number | null>(null);
  const [当前难度标签ID, 设置当前难度标签ID] = React.useState<number | null>(null);
  const [知识点决策映射, 设置知识点决策映射] = React.useState<Record<string, 知识点本地决策>>({});
  const [手动附加标签ID列表, 设置手动附加标签ID列表] = React.useState<number[]>([]);
  const [排除标签ID列表, 设置排除标签ID列表] = React.useState<number[]>([]);
  const [页面错误, 设置页面错误] = React.useState("");
  const [完成提示, 设置完成提示] = React.useState("");
  const [正在开始导入, 设置正在开始导入] = React.useState(false);
  const [正在确认, 设置正在确认] = React.useState(false);
  const [正在跳过, 设置正在跳过] = React.useState(false);
  const [正在新增年份, 设置正在新增年份] = React.useState(false);
  const [正在新增来源, 设置正在新增来源] = React.useState(false);
  const [新增标签目标种类ID, 设置新增标签目标种类ID] = React.useState<number | null>(null);
  const [新增标签父标签ID, 设置新增标签父标签ID] = React.useState<number | null>(null);
  const [新增标签表单, 设置新增标签表单] = React.useState<快速新增标签表单>(空新增标签表单);
  const [新增标签错误, 设置新增标签错误] = React.useState("");
  const [正在新增标签, 设置正在新增标签] = React.useState(false);

  const 当前试卷记录 = React.useMemo(() => 试卷列表.find((item) => item.试卷记录ID === 当前试卷记录ID) ?? null, [试卷列表, 当前试卷记录ID]);
  const 未解决知识点列表 = React.useMemo(() => (当前题目?.知识点列表 ?? []).filter((item) => !item.是否已解决), [当前题目]);
  const 标签种类字典 = React.useMemo(() => new Map(props.标签种类列表.map((kind) => [kind.id, kind])), [props.标签种类列表]);
  const 可编辑标签种类列表 = React.useMemo(
    () =>
      props.标签种类列表.filter(
        (kind) =>
          kind.id !== 难度标签种类ID &&
          kind.id !== 年份标签种类ID &&
          kind.id !== 来源标签种类ID
      ),
    [props.标签种类列表]
  );
  const 按种类标签 = React.useMemo(() => {
    const map: Record<number, 可映射标签项[]> = {};
    props.可映射标签列表.forEach((tag) => { if (!map[tag.标签种类ID]) { map[tag.标签种类ID] = []; } map[tag.标签种类ID].push(tag); });
    Object.values(map).forEach((list) => list.sort((a, b) => a.名称.localeCompare(b.名称, "zh-Hans-CN")));
    return map;
  }, [props.可映射标签列表]);
  const 按种类树 = React.useMemo(() => {
    const map: Record<number, 树形标签节点[]> = {};
    Object.entries(按种类标签).forEach(([kindId, tags]) => { map[Number(kindId)] = 构建树形标签列表(tags); });
    return map;
  }, [按种类标签]);
  const 当前新增标签种类 = React.useMemo(() => 新增标签目标种类ID === null ? null : props.标签种类列表.find((kind) => kind.id === 新增标签目标种类ID) ?? null, [props.标签种类列表, 新增标签目标种类ID]);
  const 当前新增父标签名称 = React.useMemo(() => 新增标签父标签ID === null || 新增标签目标种类ID === null ? null : 按种类标签[新增标签目标种类ID]?.find((tag) => tag.id === 新增标签父标签ID)?.名称 ?? null, [新增标签父标签ID, 新增标签目标种类ID, 按种类标签]);
  const 固定标签ID列表 = React.useMemo(() => {
    const yearId = 当前试卷记录?.年份标签ID ?? 年份标签ID;
    const sourceId = 当前试卷记录?.来源标签ID ?? 来源标签ID;
    return [yearId, sourceId].filter((id): id is number => id !== null);
  }, [当前试卷记录, 年份标签ID, 来源标签ID]);
  const 映射生成标签ID列表 = React.useMemo(() => {
    const ids = (当前题目?.知识点列表 ?? []).filter((item) => item.是否已解决 && !item.是否抛弃 && item.目标标签ID).map((item) => item.目标标签ID as number);
    Object.values(知识点决策映射).forEach((decision) => { if (!decision.是否抛弃 && decision.目标标签ID !== null) { ids.push(decision.目标标签ID); } });
    return 去重(ids);
  }, [当前题目, 知识点决策映射]);
  const 最终可编辑标签ID列表 = React.useMemo(() => {
    const excluded = new Set(排除标签ID列表);
    const result = new Set<number>();
    映射生成标签ID列表.forEach((id) => { if (!excluded.has(id)) { result.add(id); } });
    手动附加标签ID列表.forEach((id) => { if (!excluded.has(id)) { result.add(id); } });
    return Array.from(result);
  }, [映射生成标签ID列表, 手动附加标签ID列表, 排除标签ID列表]);
  const 最终标签ID列表 = React.useMemo(() => 去重([...固定标签ID列表, ...(当前难度标签ID === null ? [] : [当前难度标签ID]), ...最终可编辑标签ID列表]), [固定标签ID列表, 当前难度标签ID, 最终可编辑标签ID列表]);
  const 可确认 = React.useMemo(() => {
    if (!当前题目 || 当前题型ID === null || 当前难度标签ID === null) { return false; }
    return 未解决知识点列表.every((item) => { const d = 知识点决策映射[item.原始知识点文本]; return Boolean(d) && (d.是否抛弃 || d.目标标签ID !== null); });
  }, [当前题目, 当前题型ID, 当前难度标签ID, 未解决知识点列表, 知识点决策映射]);

  const 加载试卷列表 = React.useCallback(async () => {
    try {
      设置正在加载试卷列表(true);
      const 响应 = await fetch(props.构建题库接口路径("/试卷导入/试卷列表"));
      if (!响应.ok) { throw new Error(await 响应.text() || "加载试卷列表失败。"); }
      const 数据 = await 响应.json() as 试卷记录列表项[];
      设置试卷列表(数据);
      return 数据;
    } catch (error) {
      console.error(error);
      设置页面错误(获取错误信息(error, "加载试卷列表失败。"));
      return [] as 试卷记录列表项[];
    } finally { 设置正在加载试卷列表(false); }
  }, [props]);

  React.useEffect(() => { void 加载试卷列表(); }, [加载试卷列表]);
  React.useEffect(() => {
    if (!当前题目) {
      设置当前题型ID(null); 设置当前难度标签ID(null); 设置知识点决策映射({}); 设置手动附加标签ID列表([]); 设置排除标签ID列表([]); 设置新增标签目标种类ID(null); 设置新增标签错误("");
      return;
    }
    设置当前题型ID(当前题目.推荐题型ID ?? 当前题目.可选题型列表[0]?.id ?? null);
    设置当前难度标签ID(null);
    设置知识点决策映射(未解决知识点列表.reduce<Record<string, 知识点本地决策>>((acc, item) => { acc[item.原始知识点文本] = { 目标标签ID: null, 是否抛弃: false }; return acc; }, {}));
    设置手动附加标签ID列表([]); 设置排除标签ID列表([]); 设置新增标签目标种类ID(null); 设置新增标签错误("");
  }, [当前题目, 未解决知识点列表]);
  React.useEffect(() => { if (!当前新增标签种类?.是否树形) { 设置新增标签父标签ID(null); } }, [当前新增标签种类]);

  const 滚动到页面顶部 = React.useCallback(() => {
    window.requestAnimationFrame(() => {
      页面顶部Ref.current?.scrollIntoView({ behavior: "smooth", block: "start" });
      window.scrollTo({ top: 0, behavior: "smooth" });
    });
  }, []);

  const 创建年份或来源标签 = async (标签种类ID: number, 名称: string) => {
    const 修整名称 = 名称.trim();
    if (修整名称 === "") { throw new Error("请输入标签名称。"); }
    const 响应 = await fetch(props.构建题库接口路径("/标签"), { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ 标签种类ID, 名称: 修整名称, Description: null, ParentId: null, NumericValue: null, IsEnabled: true }) });
    if (!响应.ok) { throw new Error(await 响应.text() || "新增标签失败。"); }
    return await 响应.json() as { id: number; 名称: string };
  };

  const 继续导入试卷 = async (试卷: 试卷记录列表项) => {
    try {
      设置页面错误(""); 设置完成提示(""); 设置当前试卷记录ID(试卷.试卷记录ID); 设置年份标签ID(试卷.年份标签ID); 设置来源标签ID(试卷.来源标签ID);
      const 响应 = await fetch(props.构建题库接口路径(`/试卷导入/${试卷.试卷记录ID}/当前题`));
      if (响应.status === 204) { 设置当前题目(null); await 加载试卷列表(); 设置完成提示("当前试卷已经全部处理完成。"); return; }
      if (!响应.ok) { throw new Error(await 响应.text() || "继续导入试卷失败。"); }
      设置当前题目(await 响应.json() as 当前导入题目结果);
    } catch (error) { console.error(error); 设置页面错误(获取错误信息(error, "继续导入试卷失败。")); }
  };

  const 下载试卷 = (试卷记录ID: number) => {
    const 下载地址 = props.构建题库接口路径(`/试卷导入/${试卷记录ID}/下载`);
    window.open(下载地址, "_blank", "noopener,noreferrer");
  };

  const 开始导入 = async () => {
    if (!选中文件) { 设置页面错误("请先选择要导入的 docx 文件。"); return; }
    if (年份标签ID === null || 来源标签ID === null) { 设置页面错误("请先选择年份和来源。"); return; }
    try {
      设置正在开始导入(true); 设置页面错误(""); 设置完成提示("");
      const formData = new FormData(); formData.append("file", 选中文件); formData.append("年份标签ID", String(年份标签ID)); formData.append("来源标签ID", String(来源标签ID));
      const 响应 = await fetch(props.构建题库接口路径("/试卷导入/开始"), { method: "POST", body: formData });
      if (!响应.ok) { throw new Error(await 响应.text() || "开始导入试卷失败。"); }
      const 结果 = await 响应.json() as 开始导入试卷结果;
      设置当前试卷记录ID(结果.试卷记录ID); 设置当前题目(结果.当前题目 ?? null);
      const 列表 = await 加载试卷列表();
      if (!结果.当前题目 && 列表.find((item) => item.试卷记录ID === 结果.试卷记录ID)?.状态 === "已完成") { 设置完成提示("当前试卷已经全部处理完成。"); }
    } catch (error) { console.error(error); 设置页面错误(获取错误信息(error, "开始导入试卷失败。")); }
    finally { 设置正在开始导入(false); }
  };

  const 更新知识点决策 = (原始知识点文本: string, 决策: 知识点本地决策) => {
    设置知识点决策映射((current) => ({ ...current, [原始知识点文本]: 决策 }));
    if (!决策.是否抛弃 && 决策.目标标签ID !== null) { 设置排除标签ID列表((current) => current.filter((id) => id !== 决策.目标标签ID)); }
  };

  const 切换题目标签 = (标签种类: 标签种类项, 标签ID: number) => {
    const 映射集合 = new Set(映射生成标签ID列表);
    const 当前最终集合 = new Set(最终可编辑标签ID列表);
    const 当前种类标签ID列表 = (按种类标签[标签种类.id] ?? []).map((标签) => 标签.id);
    const 已选中 = 当前最终集合.has(标签ID);
    const 移除手动标签 = (id: number) => 设置手动附加标签ID列表((current) => current.filter((item) => item !== id));
    const 添加手动标签 = (id: number) => {
      设置排除标签ID列表((current) => current.filter((item) => item !== id));
      设置手动附加标签ID列表((current) => 去重([...current, id]));
    };
    const 排除标签 = (id: number) => 设置排除标签ID列表((current) => 去重([...current, id]));
    const 恢复标签 = (id: number) => 设置排除标签ID列表((current) => current.filter((item) => item !== id));

    if (标签种类.是否允许多选) {
      if (已选中) { 映射集合.has(标签ID) ? 排除标签(标签ID) : 移除手动标签(标签ID); }
      else { 映射集合.has(标签ID) ? 恢复标签(标签ID) : 添加手动标签(标签ID); }
      return;
    }

    当前种类标签ID列表.filter((id) => 当前最终集合.has(id)).forEach((id) => { 映射集合.has(id) ? 排除标签(id) : 移除手动标签(id); });
    if (!已选中) { 映射集合.has(标签ID) ? 恢复标签(标签ID) : 添加手动标签(标签ID); }
  };

  const 通过搜索选择题目标签 = React.useCallback((标签ID: number, 标签种类ID: number) => {
    const 标签种类 = 标签种类字典.get(标签种类ID);
    if (!标签种类) {
      return;
    }
    const 映射集合 = new Set(映射生成标签ID列表);
    if (映射集合.has(标签ID)) {
      设置排除标签ID列表((current) => current.filter((item) => item !== 标签ID));
      return;
    }
    设置排除标签ID列表((current) => current.filter((item) => item !== 标签ID));
    设置手动附加标签ID列表((current) => 去重([...current, 标签ID]));
  }, [标签种类字典, 映射生成标签ID列表]);

  const 导入标签检查已选映射 = React.useMemo(() => {
    const 映射: Record<number, number[]> = {};
    可编辑标签种类列表.forEach((标签种类) => {
      映射[标签种类.id] = 最终可编辑标签ID列表.filter((标签ID) =>
        (按种类标签[标签种类.id] ?? []).some((标签) => 标签.id === 标签ID)
      );
    });
    return 映射;
  }, [可编辑标签种类列表, 最终可编辑标签ID列表, 按种类标签]);

  const 新增导入工作台标签 = React.useCallback(
    async ({
      标签种类,
      父标签ID,
      名称,
      描述,
      数值文本,
    }: {
      标签种类: 标签种类项;
      父标签ID: number | null;
      名称: string;
      描述: string;
      数值文本: string;
    }) => {
      const 响应 = await fetch(props.构建题库接口路径("/标签"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          标签种类ID: 标签种类.id,
          名称,
          Description: 描述 === "" ? null : 描述,
          ParentId: 标签种类.是否树形 ? 父标签ID : null,
          NumericValue: 标签种类.id === 难度标签种类ID ? (数值文本 === "" ? null : Number.parseFloat(数值文本)) : null,
          IsEnabled: true,
        }),
      });
      if (!响应.ok) {
        throw new Error(await 响应.text() || "新增标签失败。");
      }
      const 新标签 = await 响应.json() as { id: number; 名称: string };
      await props.刷新标签基础数据();
      return { id: 新标签.id, 名称: 新标签.名称 };
    },
    [props]
  );

  const 编辑导入工作台标签 = React.useCallback(
    async ({
      标签,
      标签种类,
      名称,
      描述,
      数值文本,
    }: {
      标签: 可映射标签项;
      标签种类: 标签种类项;
      名称: string;
      描述: string;
      数值文本: string;
    }) => {
      const 响应 = await fetch(props.构建题库接口路径(`/标签/${标签.id}`), {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          标签种类ID: 标签种类.id,
          名称,
          Description: 描述 === "" ? null : 描述,
          NumericValue: 标签种类.id === 难度标签种类ID ? (数值文本 === "" ? null : Number.parseFloat(数值文本)) : null,
          IsEnabled: 标签.isEnabled ?? true,
        }),
      });
      if (!响应.ok) {
        throw new Error(await 响应.text() || "更新标签失败。");
      }
      await props.刷新标签基础数据();
    },
    [props]
  );

  const 确认并下一题 = async () => {
    if (!当前题目 || 当前试卷记录ID === null || 当前题型ID === null || 当前难度标签ID === null) { 设置页面错误("请先完成当前题目的确认。"); return; }
    try {
      设置正在确认(true); 设置页面错误(""); 设置完成提示("");
      const 新建知识点映射列表 = 未解决知识点列表.map((item) => {
        const 决策 = 知识点决策映射[item.原始知识点文本];
        return { 原始知识点文本: item.原始知识点文本, 目标标签ID: 决策?.是否抛弃 ? null : 决策?.目标标签ID ?? null, 是否抛弃: 决策?.是否抛弃 ?? false };
      });
      const 响应 = await fetch(props.构建题库接口路径(`/试卷导入/${当前试卷记录ID}/确认`), {
        method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ 试卷题目项ID: 当前题目.试卷题目项ID, 题型ID: 当前题型ID, 难度标签ID: 当前难度标签ID, 最终标签ID列表, 新建知识点映射列表 })
      });
      if (响应.status === 204) { 设置当前题目(null); await 加载试卷列表(); 设置完成提示("当前试卷已经全部处理完成。"); 滚动到页面顶部(); return; }
      if (!响应.ok) { throw new Error(await 响应.text() || "确认当前题失败。"); }
      设置当前题目(await 响应.json() as 当前导入题目结果); await 加载试卷列表(); 滚动到页面顶部();
    } catch (error) { console.error(error); 设置页面错误(获取错误信息(error, "确认当前题失败。")); }
    finally { 设置正在确认(false); }
  };

  const 跳过当前题 = async () => {
    if (!当前题目 || 当前试卷记录ID === null) { return; }
    try {
      设置正在跳过(true); 设置页面错误(""); 设置完成提示("");
      const 响应 = await fetch(props.构建题库接口路径(`/试卷导入/${当前试卷记录ID}/跳过`), { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ 试卷题目项ID: 当前题目.试卷题目项ID }) });
      if (响应.status === 204) { 设置当前题目(null); await 加载试卷列表(); 设置完成提示("当前试卷已经全部处理完成。"); return; }
      if (!响应.ok) { throw new Error(await 响应.text() || "跳过当前题失败。"); }
      设置当前题目(await 响应.json() as 当前导入题目结果); await 加载试卷列表();
    } catch (error) { console.error(error); 设置页面错误(获取错误信息(error, "跳过当前题失败。")); }
    finally { 设置正在跳过(false); }
  };

  const 退出当前导入 = () => { 设置当前试卷记录ID(null); 设置当前题目(null); 设置页面错误(""); 设置完成提示(""); };
  const 打开新增标签表单 = (标签种类: 标签种类项) => { 设置新增标签目标种类ID(标签种类.id); 设置新增标签父标签ID(null); 设置新增标签表单(空新增标签表单()); 设置新增标签错误(""); };
  const 关闭新增标签表单 = () => { 设置新增标签目标种类ID(null); 设置新增标签父标签ID(null); 设置新增标签表单(空新增标签表单()); 设置新增标签错误(""); };
  const 更新新增标签表单字段 = (字段: keyof 快速新增标签表单, 值: string) => 设置新增标签表单((current) => ({ ...current, [字段]: 值 }));
  const 开始新增子标签 = (标签种类: 标签种类项, 标签: 树形标签节点) => { 设置新增标签目标种类ID(标签种类.id); 设置新增标签父标签ID(标签.id); 设置新增标签表单(空新增标签表单()); 设置新增标签错误(""); };
  const 提交新增映射标签 = async () => {
    if (!当前新增标签种类) { 设置新增标签错误("请选择标签种类。"); return; }
    if (新增标签表单.名称.trim() === "") { 设置新增标签错误("标签名称不能为空。"); return; }
    try {
      设置正在新增标签(true); 设置新增标签错误("");
      const 响应 = await fetch(props.构建题库接口路径("/标签"), {
        method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ 标签种类ID: 当前新增标签种类.id, 名称: 新增标签表单.名称.trim(), Description: 新增标签表单.描述.trim() === "" ? null : 新增标签表单.描述.trim(), ParentId: 当前新增标签种类.是否树形 ? 新增标签父标签ID : null, NumericValue: 当前新增标签种类.id === 难度标签种类ID ? (新增标签表单.数值文本.trim() === "" ? null : Number.parseFloat(新增标签表单.数值文本)) : null, IsEnabled: true })
      });
      if (!响应.ok) { throw new Error(await 响应.text() || "新增标签失败。"); }
      const 新标签 = await 响应.json() as { id: number; 名称: string };
      await props.刷新标签基础数据();
      切换题目标签(当前新增标签种类, 新标签.id);
      关闭新增标签表单();
    } catch (error) { console.error(error); 设置新增标签错误(获取错误信息(error, "新增标签失败。")); }
    finally { 设置正在新增标签(false); }
  };

  const 渲染新增标签表单 = (标题: string, 父标签名称?: string | null) => (
    <QuickAddTagForm 标题={标题} 父标签名称={父标签名称} 表单={新增标签表单} 是否显示数值输入={当前新增标签种类?.id === 难度标签种类ID} 错误信息={新增标签错误} 正在保存={正在新增标签} onChange={更新新增标签表单字段} onSubmit={() => void 提交新增映射标签()} onCancel={关闭新增标签表单} />
  );

  const 渲染已选标签摘要 = () => {
    const 已选分组 = 可编辑标签种类列表
      .map((kind) => {
        const kindTags = 按种类标签[kind.id] ?? [];
        const 标签列表 = kindTags.filter((tag) => 最终可编辑标签ID列表.includes(tag.id));
        return { 标签种类: kind, 标签列表 };
      })
      .filter((item) => item.标签列表.length > 0);

    if (已选分组.length === 0) {
      return <p className={styles.noteText}>当前还没有额外选择标签。</p>;
    }

    return (
      <div className={styles.column}>
        {已选分组.map((项目) => (
          <div key={项目.标签种类.id} className={styles.column}>
            <span className={styles.noteText}>{项目.标签种类.名称}</span>
            <div className={styles.chipRow}>
              {项目.标签列表.map((标签) => (
                <TagBadge
                  key={标签.id}
                  文本={标签.名称}
                  强调
                  onClick={() => 切换题目标签(项目.标签种类, 标签.id)}
                />
              ))}
            </div>
          </div>
        ))}
      </div>
    );
  };

  return (
    <div className={styles.root}>
      <div className={styles.container}>
        <div ref={页面顶部Ref} />
        <button type="button" className={styles.backButton} onClick={props.返回首页}>返回首页</button>
        <div className={styles.bankBanner}>当前题库：{props.当前题库显示名称}</div>
        <h1 className={styles.title}>导入试卷</h1>
        <p className={styles.subtitle}>当前模板会按题号拆题，并用灰底答案区提取难度和知识点。每次确认或跳过都会立即更新当前试卷进度。</p>

        {当前试卷记录ID === null && !当前题目 && (
          <>
            <div className={styles.section}>
              <h2 className={styles.sectionTitle}>导入开始</h2>
              <label className={styles.noteText}>试卷文件（仅支持 docx）</label>
              <input className={styles.fileInput} type="file" accept=".docx" onChange={(e) => 设置选中文件(e.target.files?.[0] ?? null)} />
              <div className={styles.gridTwo}>
                <div className={styles.column}>
                  <label className={styles.noteText}>年份</label>
                  <select className={styles.select} value={年份标签ID ?? ""} onChange={(e) => 设置年份标签ID(e.target.value === "" ? null : Number(e.target.value))}>
                    <option value="">请选择年份</option>
                    {props.年份标签列表.map((tag) => <option key={tag.id} value={tag.id}>{tag.名称}</option>)}
                  </select>
                  <div className={styles.quickAddBox}>
                    <label className={styles.noteText}>快速新增年份</label>
                    <input className={styles.input} value={新增年份名称} onChange={(e) => 设置新增年份名称(e.target.value)} />
                    <button type="button" className={styles.secondaryButton} onClick={() => void (async () => { try { 设置正在新增年份(true); const 新标签 = await 创建年份或来源标签(7, 新增年份名称); await props.刷新标签基础数据(); 设置年份标签ID(新标签.id); 设置新增年份名称(""); } catch (error) { console.error(error); 设置页面错误(获取错误信息(error, "新增年份失败。")); } finally { 设置正在新增年份(false); } })()} disabled={正在新增年份}>{正在新增年份 ? "正在新增..." : "新增年份"}</button>
                  </div>
                </div>
                <div className={styles.column}>
                  <label className={styles.noteText}>来源</label>
                  <select className={styles.select} value={来源标签ID ?? ""} onChange={(e) => 设置来源标签ID(e.target.value === "" ? null : Number(e.target.value))}>
                    <option value="">请选择来源</option>
                    {props.来源标签列表.map((tag) => <option key={tag.id} value={tag.id}>{tag.名称}</option>)}
                  </select>
                  <div className={styles.quickAddBox}>
                    <label className={styles.noteText}>快速新增来源</label>
                    <input className={styles.input} value={新增来源名称} onChange={(e) => 设置新增来源名称(e.target.value)} />
                    <button type="button" className={styles.secondaryButton} onClick={() => void (async () => { try { 设置正在新增来源(true); const 新标签 = await 创建年份或来源标签(8, 新增来源名称); await props.刷新标签基础数据(); 设置来源标签ID(新标签.id); 设置新增来源名称(""); } catch (error) { console.error(error); 设置页面错误(获取错误信息(error, "新增来源失败。")); } finally { 设置正在新增来源(false); } })()} disabled={正在新增来源}>{正在新增来源 ? "正在新增..." : "新增来源"}</button>
                  </div>
                </div>
              </div>
              {页面错误 !== "" && <p className={styles.errorText}>{页面错误}</p>}
              {完成提示 !== "" && <p className={styles.successText}>{完成提示}</p>}
              <div className={styles.row}><button type="button" className={styles.button} onClick={() => void 开始导入()} disabled={正在开始导入}>{正在开始导入 ? "正在拆题..." : "开始导入"}</button></div>
            </div>

            <div className={styles.section}>
              <div className={styles.between}>
                <h2 className={styles.sectionTitle}>题库中的试卷</h2>
                <button type="button" className={styles.secondaryButton} onClick={() => void 加载试卷列表()} disabled={正在加载试卷列表}>{正在加载试卷列表 ? "正在刷新..." : "刷新列表"}</button>
              </div>
              {正在加载试卷列表 && <p className={styles.noteText}>正在加载试卷列表...</p>}
              {!正在加载试卷列表 && 试卷列表.length === 0 && <p className={styles.noteText}>当前题库还没有导入过试卷。</p>}
              {!正在加载试卷列表 && 试卷列表.length > 0 && (
                <div className={styles.column}>
                  {试卷列表.map((试卷) => (
                    <div key={试卷.试卷记录ID} className={styles.card}>
                      <p className={styles.sectionTitle}>{试卷.显示名称}</p>
                      <div className={styles.row}>
                        <TagBadge 文本={`年份：${试卷.年份标签名称}`} />
                        <TagBadge 文本={`来源：${试卷.来源标签名称}`} />
                        <TagBadge 文本={`进度：${试卷.已确认数}/${试卷.总题数}`} 强调 />
                        <TagBadge 文本={`已跳过：${试卷.已跳过数}`} />
                      </div>
                      <div className={styles.row}>
                        <button type="button" className={styles.secondaryButton} onClick={() => 下载试卷(试卷.试卷记录ID)}>
                          下载试卷
                        </button>
                        {试卷.状态 !== "已完成" && (
                          <button type="button" className={styles.secondaryButton} onClick={() => void 继续导入试卷(试卷)}>
                            继续导入
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </>
        )}

        {(当前试卷记录ID !== null || 当前题目) && (
          当前题目 ? (
            <div className={styles.section}>
              <div className={styles.between}>
                <div className={styles.column}>
                  <h2 className={styles.sectionTitle}>逐题确认</h2>
                  <p className={styles.noteText}>当前题号：{当前题目.题号文本 || 当前题目.草稿题序号}，剩余待处理：{当前题目.剩余数量} 道</p>
                </div>
                <button type="button" className={styles.secondaryButton} onClick={退出当前导入}>退出</button>
              </div>
              {页面错误 !== "" && <p className={styles.errorText}>{页面错误}</p>}
              <div className={styles.section}>
                <h2 className={styles.sectionTitle}>知识点映射</h2>
                {(当前题目.知识点列表 ?? []).length === 0 && <p className={styles.noteText}>当前题目未提取到知识点。</p>}
                {(当前题目.知识点列表 ?? []).filter((item) => item.是否已解决).map((item) => <div key={`resolved-${item.原始知识点文本}`} className={styles.resolved}><p className={styles.noteText}>{item.原始知识点文本}：{item.是否抛弃 ? "已设置为抛弃" : `已映射到 ${item.目标标签名称 ?? "未知标签"}`}</p></div>)}
                {未解决知识点列表.map((item) => {
                  const 决策 = 知识点决策映射[item.原始知识点文本] ?? { 目标标签ID: null, 是否抛弃: false };
                  const 当前已选标签 = 决策.目标标签ID === null ? null : props.可映射标签列表.find((tag) => tag.id === 决策.目标标签ID) ?? null;
                  const 已处理 = 决策.是否抛弃 || 决策.目标标签ID !== null;
                  return <div key={item.原始知识点文本} className={`${styles.mappingItem} ${已处理 ? styles.processedMappingItem : ""}`}><p className={styles.noteText}>原始知识点：{item.原始知识点文本}</p>{!决策.是否抛弃 && <TagSearchPanel 标题="标签关键词搜索" 提示文本="输入关键字，搜索并选中映射标签；如果还没有这个标签，可在下方“题目标签检查”里新增" 标签搜索项列表={props.标签搜索项列表} 已选标签ID列表={决策.目标标签ID === null ? [] : [决策.目标标签ID]} 选择标签={(标签ID) => 更新知识点决策(item.原始知识点文本, { 目标标签ID: 标签ID, 是否抛弃: false })} />}{当前已选标签 && !决策.是否抛弃 && <p className={styles.noteText}>当前映射：{当前已选标签.标签种类名称} · {当前已选标签.名称}</p>}{决策.是否抛弃 && <p className={styles.noteText}>当前处理：已设置为抛弃</p>}<label className={styles.noteText}><input type="checkbox" checked={决策.是否抛弃} onChange={(e) => 更新知识点决策(item.原始知识点文本, { 是否抛弃: e.target.checked, 目标标签ID: e.target.checked ? null : 决策.目标标签ID })} /> 抛弃这个知识点</label></div>;
                })}
              </div>
              <div className={styles.section}><h2 className={styles.sectionTitle}>题目预览</h2>{当前题目.题目摘要 !== "" && <p className={styles.noteText}>题目摘要：{当前题目.题目摘要}</p>}<div className={styles.preview} dangerouslySetInnerHTML={{ __html: 当前题目.题目预览Html }} /></div>
              <div className={styles.section}><h2 className={styles.sectionTitle}>推荐题型</h2><p className={styles.noteText}>推荐结果：{当前题目.推荐题型名称 ?? "暂未给出推荐"}。置信度：{当前题目.置信度.toFixed(2)}</p><p className={styles.noteText}>识别说明：{当前题目.识别说明}</p></div>
              <div className={styles.section}>
                <h2 className={styles.sectionTitle}>确认题型</h2>
                <SingleSelectChipGroup
                  选项列表={当前题目.可选题型列表.map((type) => ({ id: type.id, 名称: type.名称 }))}
                  当前选中ID={当前题型ID}
                  选择选项={(id) => 设置当前题型ID(Number(id))}
                  空提示文本="当前没有可选题型。"
                />
              </div>
              <div className={styles.section}>
                <h2 className={styles.sectionTitle}>难度确认</h2>
                <p className={styles.noteText}>提取结果：{当前题目.原始难度文本 || "未提取到难度文本"}</p>
                <SingleSelectChipGroup
                  选项列表={props.难度标签列表.map((tag) => ({ id: tag.id, 名称: tag.名称 }))}
                  当前选中ID={当前难度标签ID}
                  选择选项={(id) => 设置当前难度标签ID(Number(id))}
                  空提示文本="当前题库还没有难度标签。"
                />
              </div>
              <div className={styles.section}>
                <h2 className={styles.sectionTitle}>题目标签检查</h2>
                <div className={styles.row}>{固定标签ID列表.map((id) => { const year = props.年份标签列表.find((tag) => tag.id === id); const source = props.来源标签列表.find((tag) => tag.id === id); const name = year?.名称 ?? source?.名称; return name ? <TagBadge key={`fixed-${id}`} 文本={name} 强调 /> : null; })}{当前难度标签ID !== null && <TagBadge 文本={`难度：${props.难度标签列表.find((tag) => tag.id === 当前难度标签ID)?.名称 ?? "未命名"}`} 强调 />}</div>
                <p className={styles.noteText}>系统会先根据年份、来源、难度和知识点映射预填标签，这里再做最终人工检查。</p>
                <标签工作台
                  模式="导入标签检查"
                  标签种类列表={可编辑标签种类列表}
                  已选标签ID映射={导入标签检查已选映射}
                  标签搜索项列表={props.标签搜索项列表}
                  获取指定种类标签列表={(标签种类ID) =>
                    标签种类字典.get(标签种类ID)?.是否树形
                      ? (按种类树[标签种类ID] ?? [])
                      : (按种类标签[标签种类ID] ?? [])
                  }
                  获取标签显示文本={(标签) => 标签.名称}
                  切换标签={切换题目标签}
                  通过搜索选择标签={通过搜索选择题目标签}
                  新增标签={新增导入工作台标签}
                  编辑标签={编辑导入工作台标签}
                />
              </div>
              <div className={styles.row}><button type="button" className={styles.button} onClick={() => void 确认并下一题()} disabled={!可确认 || 正在确认}>{正在确认 ? "正在确认..." : "确认并下一题"}</button><button type="button" className={styles.secondaryButton} onClick={() => void 跳过当前题()} disabled={正在跳过}>{正在跳过 ? "正在跳过..." : "跳过"}</button><button type="button" className={styles.secondaryButton} onClick={退出当前导入}>退出</button></div>
            </div>
          ) : (
            <div className={styles.section}><h2 className={styles.sectionTitle}>导入完成</h2><p className={styles.successText}>{完成提示 || "当前试卷已经全部处理完成。"}</p><div className={styles.row}><button type="button" className={styles.secondaryButton} onClick={退出当前导入}>返回试卷列表</button></div></div>
          )
        )}
      </div>
    </div>
  );
}
