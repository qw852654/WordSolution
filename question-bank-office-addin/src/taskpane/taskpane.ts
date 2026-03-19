/* global Word console Office */

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
