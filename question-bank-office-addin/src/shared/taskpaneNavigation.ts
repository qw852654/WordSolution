export type Ribbon目标页面 = "首页" | "录题页" | "筛题页";

interface 导航请求 {
  目标页面: Ribbon目标页面;
  导航令牌: string;
}

const 待处理导航存储键 = "questionBankPendingNavigation";
const 导航事件名 = "question-bank:navigate";

let 最近导航请求: 导航请求 | null = null;

function 当前窗口可用() {
  return typeof window !== "undefined";
}

function 创建导航令牌() {
  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

function 读取本地导航请求(): 导航请求 | null {
  if (!当前窗口可用()) {
    return null;
  }

  try {
    const 原始值 = window.localStorage.getItem(待处理导航存储键);
    if (!原始值) {
      return null;
    }

    const 请求 = JSON.parse(原始值) as Partial<导航请求>;
    if (
      (请求.目标页面 === "首页" || 请求.目标页面 === "录题页" || 请求.目标页面 === "筛题页") &&
      typeof 请求.导航令牌 === "string"
    ) {
      return 请求 as 导航请求;
    }
  } catch {}

  return null;
}

function 保存本地导航请求(请求: 导航请求) {
  if (!当前窗口可用()) {
    return;
  }

  try {
    window.localStorage.setItem(待处理导航存储键, JSON.stringify(请求));
  } catch {}
}

function 分发导航事件(请求: 导航请求) {
  if (!当前窗口可用()) {
    return;
  }

  window.dispatchEvent(new CustomEvent<导航请求>(导航事件名, { detail: 请求 }));
}

export function 请求导航到页面(目标页面: Ribbon目标页面) {
  const 请求: 导航请求 = {
    目标页面,
    导航令牌: 创建导航令牌(),
  };

  最近导航请求 = 请求;
  保存本地导航请求(请求);
  分发导航事件(请求);

  return 请求;
}

export function 读取待处理导航请求() {
  if (最近导航请求) {
    return 最近导航请求;
  }

  const 本地请求 = 读取本地导航请求();
  if (本地请求) {
    最近导航请求 = 本地请求;
  }

  return 本地请求;
}

export function 清除待处理导航请求(导航令牌?: string) {
  if (导航令牌 && 最近导航请求 && 最近导航请求.导航令牌 !== 导航令牌) {
    return;
  }

  if (导航令牌) {
    const 本地请求 = 读取本地导航请求();
    if (本地请求 && 本地请求.导航令牌 !== 导航令牌) {
      return;
    }
  }

  最近导航请求 = null;

  if (!当前窗口可用()) {
    return;
  }

  try {
    window.localStorage.removeItem(待处理导航存储键);
  } catch {}
}

export function 监听导航请求(回调: (请求: 导航请求) => void) {
  if (!当前窗口可用()) {
    return () => undefined;
  }

  const 事件处理器 = (事件: Event) => {
    const 自定义事件 = 事件 as CustomEvent<导航请求>;
    if (!自定义事件.detail) {
      return;
    }

    最近导航请求 = 自定义事件.detail;
    回调(自定义事件.detail);
  };

  window.addEventListener(导航事件名, 事件处理器 as EventListener);

  return () => {
    window.removeEventListener(导航事件名, 事件处理器 as EventListener);
  };
}
