/* global Word */

const 题目标签正则 = /^题目ID=(\d+)$/;
const 旧整题块标签正则 = /^题目块ID=(\d+)$/;
const 答题区域标签正则 = /^答题区域归属ID=(\d+)$/;
const WordMl命名空间 = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

export interface 题目内容控件上下文 {
  题目ID: number;
  内容控件ID: number;
  内容控件标题: string;
  内容控件标签: string;
  OOXML内容: string;
}

export function 从内容控件标签解析题目ID(内容控件标签: string) {
  const 匹配结果 = (内容控件标签 ?? "").trim().match(题目标签正则);
  if (!匹配结果) {
    return null;
  }

  const 题目ID = Number.parseInt(匹配结果[1], 10);
  return Number.isNaN(题目ID) ? null : 题目ID;
}

function 从整题块标签解析题目ID(内容控件标签: string) {
  const 匹配结果 = (内容控件标签 ?? "").trim().match(旧整题块标签正则);
  if (!匹配结果) {
    return null;
  }

  const 题目ID = Number.parseInt(匹配结果[1], 10);
  return Number.isNaN(题目ID) ? null : 题目ID;
}

function 是答题区域标签(内容控件标签: string) {
  return 答题区域标签正则.test((内容控件标签 ?? "").trim());
}

function 读取WordXml属性值(元素: Element, 本地名: string) {
  return (
    元素.getAttributeNS(WordMl命名空间, 本地名) ??
    元素.getAttribute(`w:${本地名}`) ??
    元素.getAttribute(本地名) ??
    ""
  );
}

function 读取内容控件自身标签值(内容控件: Element) {
  const 属性节点 = 内容控件.getElementsByTagNameNS(WordMl命名空间, "sdtPr")[0];
  if (!属性节点) {
    return "";
  }

  const 标签节点 = 属性节点.getElementsByTagNameNS(WordMl命名空间, "tag")[0];
  if (!标签节点) {
    return "";
  }

  return 读取WordXml属性值(标签节点, "val");
}

function 清理答题区域控件Ooxml(Ooxml内容: string) {
  const 解析器 = new DOMParser();
  const 文档 = 解析器.parseFromString(Ooxml内容, "application/xml");
  if (文档.getElementsByTagName("parsererror").length > 0) {
    throw new Error("当前题目内容解析失败，无法过滤答题区域。");
  }

  const 所有内容控件 = Array.from(文档.getElementsByTagNameNS(WordMl命名空间, "sdt"));
  for (const 内容控件 of 所有内容控件) {
    const 自身标签值 = 读取内容控件自身标签值(内容控件);
    if (!是答题区域标签(自身标签值)) {
      continue;
    }

    内容控件.parentNode?.removeChild(内容控件);
  }

  return new XMLSerializer().serializeToString(文档).trim();
}

async function 从题目内容控件构建上下文(
  context: Word.RequestContext,
  题目内容控件: Word.ContentControl
): Promise<题目内容控件上下文> {
  题目内容控件.load("id,tag,title");
  const 内容范围 = 题目内容控件.getRange(Word.RangeLocation.content);
  const Ooxml结果 = 内容范围.getOoxml();
  await context.sync();

  const 内容控件标签 = (题目内容控件.tag ?? "").trim();
  const 题目ID = 从内容控件标签解析题目ID(内容控件标签);
  if (题目ID === null) {
    throw new Error("当前内容控件没有合法的题目ID。");
  }

  const 原始Ooxml内容 = (Ooxml结果.value ?? "").trim();
  if (原始Ooxml内容 === "") {
    throw new Error("当前题目内容为空，无法更新。");
  }

  const 过滤后的Ooxml内容 = 清理答题区域控件Ooxml(原始Ooxml内容);
  if (过滤后的Ooxml内容 === "") {
    throw new Error("当前题目内容为空，无法更新。");
  }

  return {
    题目ID,
    内容控件ID: 题目内容控件.id,
    内容控件标题: 题目内容控件.title ?? "",
    内容控件标签: 内容控件标签,
    OOXML内容: 过滤后的Ooxml内容,
  };
}

async function 从旧整题块中查找题目内容控件(
  context: Word.RequestContext,
  旧整题块: Word.ContentControl
) {
  const 子内容控件集合 = 旧整题块.getRange(Word.RangeLocation.content).getContentControls();
  子内容控件集合.load("items/id,tag,title");
  await context.sync();

  const 题目内容控件 = 子内容控件集合.items.find(
    (内容控件) => 从内容控件标签解析题目ID(内容控件.tag ?? "") !== null
  );

  if (!题目内容控件) {
    throw new Error("当前整题块中没有找到题目内容控件。");
  }

  return 题目内容控件;
}

export async function 获取当前题目内容控件上下文(): Promise<题目内容控件上下文> {
  return Word.run(async (context) => {
    const 当前选区 = context.document.getSelection();
    const 选区内容控件 = 当前选区.getContentControls().getFirstOrNullObject();
    const 父内容控件 = 当前选区.parentContentControlOrNullObject;

    选区内容控件.load("isNullObject,id,tag,title");
    父内容控件.load("isNullObject,id,tag,title");
    await context.sync();

    let 当前内容控件: Word.ContentControl | null = null;
    if (!选区内容控件.isNullObject) {
      当前内容控件 = 选区内容控件;
    } else if (!父内容控件.isNullObject) {
      当前内容控件 = 父内容控件;
    }

    if (!当前内容控件) {
      throw new Error("当前光标不在题目内容控件中。");
    }

    while (当前内容控件) {
      const 内容控件标签 = (当前内容控件.tag ?? "").trim();

      if (从内容控件标签解析题目ID(内容控件标签) !== null) {
        return 从题目内容控件构建上下文(context, 当前内容控件);
      }

      if (从整题块标签解析题目ID(内容控件标签) !== null) {
        const 题目内容控件 = await 从旧整题块中查找题目内容控件(context, 当前内容控件);
        return 从题目内容控件构建上下文(context, 题目内容控件);
      }

      const 上级内容控件 = 当前内容控件.parentContentControlOrNullObject;
      上级内容控件.load("isNullObject,id,tag,title");
      await context.sync();

      if (上级内容控件.isNullObject) {
        break;
      }

      当前内容控件 = 上级内容控件;
    }

    throw new Error("当前内容控件没有合法的题目ID。");
  });
}
