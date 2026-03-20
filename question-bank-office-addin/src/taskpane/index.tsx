import * as React from "react";
import { createRoot } from "react-dom/client";
import App from "./components/App";
import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import "../commands/commands";

/* global document, Office, module, require, HTMLElement */

const title = "题库";
const 默认任务窗格宽度 = 800;

const rootElement: HTMLElement | null = document.getElementById("container");
const root = rootElement ? createRoot(rootElement) : undefined;

function 尝试设置任务窗格宽度() {
  try {
    const 支持任务窗格宽度接口 =
      Office.context.requirements?.isSetSupported?.("TaskPaneApi", "1.1") ?? false;
    const 任务窗格 = (Office as any).extensionLifeCycle?.taskpane;

    // Office 只接受像素宽度，没有稳定的宿主窗口 50% 查询接口，所以这里取一个偏大的固定值。
    if (支持任务窗格宽度接口 && typeof 任务窗格?.setWidth === "function") {
      任务窗格.setWidth(默认任务窗格宽度);
    }
  } catch (error) {
    console.log("设置任务窗格宽度失败: " + error);
  }
}

/* Render application after Office initializes */
Office.onReady(() => {
  尝试设置任务窗格宽度();
  root?.render(
    <FluentProvider theme={webLightTheme}>
      <App title={title} />
    </FluentProvider>
  );
});

if ((module as any).hot) {
  (module as any).hot.accept("./components/App", () => {
    const NextApp = require("./components/App").default;
    root?.render(NextApp);
  });
}
