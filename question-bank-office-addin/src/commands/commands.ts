/* global Office */

import { 请求导航到页面, type Ribbon目标页面 } from "../shared/taskpaneNavigation";

function 打开题库任务窗格(目标页面: Ribbon目标页面, event: Office.AddinCommands.Event) {
  try {
    请求导航到页面(目标页面);

    const Addin命名空间 = (Office as any).addin;
    if (typeof Addin命名空间?.showAsTaskpane === "function") {
      Promise.resolve(Addin命名空间.showAsTaskpane()).catch((error) => {
        console.error("打开任务窗格失败", error);
      });
    }
  } catch (error) {
    console.error("处理 Ribbon 导航失败", error);
  } finally {
    event.completed();
  }
}

function 打开首页(event: Office.AddinCommands.Event) {
  打开题库任务窗格("首页", event);
}

function 打开录题页(event: Office.AddinCommands.Event) {
  打开题库任务窗格("录题页", event);
}

function 打开筛题页(event: Office.AddinCommands.Event) {
  打开题库任务窗格("筛题页", event);
}

Office.onReady(() => {
  Office.actions.associate("OpenQuestionBankHome", 打开首页);
  Office.actions.associate("OpenQuestionBankRecord", 打开录题页);
  Office.actions.associate("OpenQuestionBankFilter", 打开筛题页);
});
