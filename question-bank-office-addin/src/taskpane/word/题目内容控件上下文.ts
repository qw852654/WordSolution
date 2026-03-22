/* global Word */

export interface 题目内容控件上下文 {
  题目ID: number;
  内容控件ID: number;
  内容控件标题: string;
  内容控件标签: string;
  OOXML内容: string;
}

export function 从内容控件标签解析题目ID(内容控件标签: string) {
  const 匹配结果 = 内容控件标签.match(/题目ID=(\d+)/);
  if (!匹配结果) {
    return null;
  }

  const 题目ID = Number.parseInt(匹配结果[1], 10);
  return Number.isNaN(题目ID) ? null : 题目ID;
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

    while (true) {
      const 内容控件标签 = (当前内容控件.tag ?? "").trim();
      const 题目ID = 从内容控件标签解析题目ID(内容控件标签);
      if (题目ID !== null) {
        const 内容范围 = 当前内容控件.getRange(Word.RangeLocation.content);
        const Ooxml结果 = 内容范围.getOoxml();
        await context.sync();

        const Ooxml内容 = (Ooxml结果.value ?? "").trim();
        if (Ooxml内容 === "") {
          throw new Error("当前题目内容为空，无法更新。");
        }

        return {
          题目ID,
          内容控件ID: 当前内容控件.id,
          内容控件标题: 当前内容控件.title ?? "",
          内容控件标签: 内容控件标签,
          OOXML内容: Ooxml内容,
        };
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
