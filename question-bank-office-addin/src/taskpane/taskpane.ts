/* global Word */

export interface 待插入题目项 {
  题目ID: number;
  文件Base64: string;
  标题: string;
  题目内容标题?: string;
  难度数值?: number | null;
  是否解答题?: boolean;
}

const 默认题目内容标题 = "题目内容";
const 答题区域标题 = "答题区域";

function 从整题块标签解析题目ID(标签: string) {
  const 匹配结果 = (标签 ?? "").trim().match(/^题目块ID=(\d+)$/);
  if (!匹配结果) {
    return null;
  }

  const 题目ID = Number.parseInt(匹配结果[1], 10);
  return Number.isNaN(题目ID) ? null : 题目ID;
}

function 构建题目内容标签(题目ID: number) {
  return `题目ID=${题目ID}`;
}

function 构建答题区域标签(题目ID: number) {
  return `答题区域归属ID=${题目ID}`;
}

function 从题目内容标签解析题目ID(标签: string) {
  const 匹配结果 = (标签 ?? "").trim().match(/^题目ID=(\d+)$/);
  if (!匹配结果) {
    return null;
  }

  const 题目ID = Number.parseInt(匹配结果[1], 10);
  return Number.isNaN(题目ID) ? null : 题目ID;
}

async function 查找当前题目插入锚点控件(context: Word.RequestContext, 当前选区: Word.Range) {
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

  let 最近题目内容控件: Word.ContentControl | null = null;

  while (当前内容控件) {
    const 标签 = 当前内容控件.tag ?? "";
    if (从整题块标签解析题目ID(标签) !== null) {
      return 当前内容控件;
    }
    if (!最近题目内容控件 && 从题目内容标签解析题目ID(标签) !== null) {
      最近题目内容控件 = 当前内容控件;
    }

    const 上级内容控件 = 当前内容控件.parentContentControlOrNullObject;
    上级内容控件.load("isNullObject,id,tag,title");
    await context.sync();

    if (上级内容控件.isNullObject) {
      break;
    }

    当前内容控件 = 上级内容控件;
  }

  return 最近题目内容控件;
}

function 将范围包装为题目内容控件(
  插入范围: Word.Range,
  题目ID: number,
  标题: string,
  题目内容标题?: string
) {
  const 题目内容 = 插入范围.insertContentControl();
  题目内容.tag = 构建题目内容标签(题目ID);
  题目内容.title =
    题目内容标题 && 题目内容标题.trim() !== ""
      ? 题目内容标题.trim()
      : 标题.trim() !== ""
      ? 标题.trim()
      : 默认题目内容标题;
  return 题目内容;
}

function 创建空题目内容控件(
  当前插入锚点: Word.Range,
  题目ID: number,
  标题: string,
  题目内容标题?: string
) {
  const 占位范围 = 当前插入锚点.insertText(" ", Word.InsertLocation.replace);
  return 将范围包装为题目内容控件(占位范围, 题目ID, 标题, 题目内容标题);
}

async function 插入答题区域到题目内容控件(
  context: Word.RequestContext,
  题目内容控件: Word.ContentControl,
  题目ID: number,
  难度数值?: number | null
) {
  const 首段 = 题目内容控件.insertParagraph("", Word.InsertLocation.end);
  const 答题区域 = 首段.getRange().insertContentControl();
  答题区域.tag = 构建答题区域标签(题目ID);
  答题区域.title = 答题区域标题;

  const 总段落数 = typeof 难度数值 === "number" && 难度数值 >= 2 ? 18 : 6;

  for (let 索引 = 1; 索引 < 总段落数; 索引 += 1) {
    答题区域.insertParagraph("", Word.InsertLocation.end);
  }

  const 段落集合 = 答题区域.getRange().paragraphs;
  段落集合.load("items");
  await context.sync();

  段落集合.items.forEach((段落) => {
    段落.style = "正文";
  });

  return 答题区域;
}

function 获取题目控件后的插入锚点(题目内容控件: Word.ContentControl) {
  return 题目内容控件.getRange(Word.RangeLocation.after);
}

export async function 获取当前选区Ooxml(): Promise<string> {
  return Word.run(async (context) => {
    const 当前选区 = context.document.getSelection();
    const Ooxml结果 = 当前选区.getOoxml();
    await context.sync();

    const Ooxml内容 = (Ooxml结果.value ?? "").trim();
    if (Ooxml内容 === "") {
      throw new Error("当前选区为空，无法录题。请先在 Word 中选中题目内容。");
    }

    return Ooxml内容;
  });
}

export async function 插入题目到当前文档(待插入题目列表: 待插入题目项[]) {
  if (待插入题目列表.length === 0) {
    return;
  }

  await Word.run(async (context) => {
    const 当前选区 = context.document.getSelection();
    const 当前题目锚点控件 = await 查找当前题目插入锚点控件(context, 当前选区);

    let 当前插入锚点 = 当前题目锚点控件
      ? 获取题目控件后的插入锚点(当前题目锚点控件)
      : 当前选区;

    let 最终光标范围: Word.Range | null = null;

    for (const 待插入题目 of 待插入题目列表) {
      const 题目内容控件 = 创建空题目内容控件(
        当前插入锚点,
        待插入题目.题目ID,
        待插入题目.标题,
        待插入题目.题目内容标题
      );
      const 题目内容范围 = 题目内容控件.getRange(Word.RangeLocation.content);
      题目内容范围.insertFileFromBase64(待插入题目.文件Base64, Word.InsertLocation.replace);

      if (待插入题目.是否解答题) {
        const 答题区域 = await 插入答题区域到题目内容控件(
          context,
          题目内容控件,
          待插入题目.题目ID,
          待插入题目.难度数值
        );
        最终光标范围 = 答题区域.getRange(Word.RangeLocation.content);
      } else {
        最终光标范围 = 获取题目控件后的插入锚点(题目内容控件);
      }

      当前插入锚点 = 获取题目控件后的插入锚点(题目内容控件);

      // 触发对象创建与属性设置，避免后续在同一批次里引用到未同步的结构。
      题目内容控件.load("id");
      if (待插入题目.是否解答题) {
        最终光标范围.load("text");
      }
      await context.sync();
    }

    if (最终光标范围) {
      最终光标范围.select("Start");
    }

    await context.sync();
  });
}
