/* global Word console Office */

interface 待插入题目项 {
  题目ID: number;
  文件Base64: string;
}

export async function insertText(text: string) {
  // Write text to the document.
  try {
    await Word.run(async (context) => {
      let body = context.document.body;
      body.insertParagraph(text, Word.InsertLocation.end);
      await context.sync();
    });
  } catch (error) {
    console.log("Error: " + error);
  }
}

export function 获取当前选区Ooxml(): Promise<string> {
  return new Promise((resolve, reject) => {
    Office.context.document.getSelectedDataAsync(Office.CoercionType.Ooxml, (结果) => {
      if (结果.status !== Office.AsyncResultStatus.Succeeded) {
        reject(new Error(结果.error.message));
        return;
      }

      const Ooxml内容 = 结果.value as string;
      if (Ooxml内容.trim() === "") {
        reject(new Error("当前选区没有可录入的内容。"));
        return;
      }

      resolve(Ooxml内容);
    });
  });
}

export async function 插入题目到当前文档(待插入题目列表: 待插入题目项[]) {
  if (待插入题目列表.length === 0) {
    throw new Error("没有可插入的题目。");
  }

  await Word.run(async (context) => {
    let 当前范围 = context.document.getSelection();

    待插入题目列表.forEach((待插入题目, 索引) => {
      const 插入位置 = 索引 === 0 ? Word.InsertLocation.replace : Word.InsertLocation.after;
      const 插入结果范围 = 当前范围.insertFileFromBase64(待插入题目.文件Base64, 插入位置);
      const 内容控件 = 插入结果范围.insertContentControl();

      内容控件.title = `题目 ${待插入题目.题目ID}`;
      内容控件.tag = `题目ID=${待插入题目.题目ID}`;

      当前范围 = 内容控件.getRange(Word.RangeLocation.after);
    });

    await context.sync();
  });
}
