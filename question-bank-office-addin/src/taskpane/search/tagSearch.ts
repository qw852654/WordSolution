import Fuse from "fuse.js";

export interface 标签搜索项 {
  标签ID: number;
  标签种类ID: number;
  标签种类名称: string;
  标签名称: string;
  完整路径文本: string;
  路径文本: string;
  展示路径文本: string;
  是否树形标签: boolean;
}

export interface 标签搜索结果 {
  标签ID: number;
  标签种类ID: number;
  标签种类名称: string;
  标签名称: string;
  完整路径文本: string;
  路径文本: string;
  展示路径文本: string;
  是否树形标签: boolean;
  命中方式: "标签名" | "路径" | "模糊";
}

function 规范化文本(文本: string) {
  return 文本.trim().toLocaleLowerCase();
}

function 转为结果(
  搜索项: 标签搜索项,
  命中方式: 标签搜索结果["命中方式"]
): 标签搜索结果 {
  return {
    标签ID: 搜索项.标签ID,
    标签种类ID: 搜索项.标签种类ID,
    标签种类名称: 搜索项.标签种类名称,
    标签名称: 搜索项.标签名称,
    完整路径文本: 搜索项.完整路径文本,
    路径文本: 搜索项.路径文本,
    展示路径文本: 搜索项.展示路径文本,
    是否树形标签: 搜索项.是否树形标签,
    命中方式,
  };
}

export function 搜索标签(标签搜索项列表: 标签搜索项[], 原始关键字: string): 标签搜索结果[] {
  const 关键字 = 规范化文本(原始关键字);
  if (关键字 === "") {
    return [];
  }

  const 已加入标签ID集合 = new Set<number>();
  const 结果列表: 标签搜索结果[] = [];

  标签搜索项列表.forEach((搜索项) => {
    if (规范化文本(搜索项.标签名称).includes(关键字)) {
      已加入标签ID集合.add(搜索项.标签ID);
      结果列表.push(转为结果(搜索项, "标签名"));
    }
  });

  标签搜索项列表.forEach((搜索项) => {
    if (已加入标签ID集合.has(搜索项.标签ID)) {
      return;
    }

    if (规范化文本(搜索项.完整路径文本).includes(关键字)) {
      已加入标签ID集合.add(搜索项.标签ID);
      结果列表.push(转为结果(搜索项, "路径"));
    }
  });

  const fuse = new Fuse(标签搜索项列表, {
    includeScore: true,
    threshold: 0.35,
    ignoreLocation: true,
    keys: [
      { name: "标签名称", weight: 0.75 },
      { name: "完整路径文本", weight: 0.25 },
    ],
  });

  fuse.search(原始关键字, { limit: 12 }).forEach((命中项) => {
    if (已加入标签ID集合.has(命中项.item.标签ID)) {
      return;
    }

    已加入标签ID集合.add(命中项.item.标签ID);
    结果列表.push(转为结果(命中项.item, "模糊"));
  });

  const 命中方式优先级: Record<标签搜索结果["命中方式"], number> = {
    标签名: 0,
    路径: 1,
    模糊: 2,
  };

  return 结果列表.sort((前一个, 后一个) => {
    if (命中方式优先级[前一个.命中方式] !== 命中方式优先级[后一个.命中方式]) {
      return 命中方式优先级[前一个.命中方式] - 命中方式优先级[后一个.命中方式];
    }

    if (前一个.标签种类ID !== 后一个.标签种类ID) {
      return 前一个.标签种类ID - 后一个.标签种类ID;
    }

    return 前一个.完整路径文本.localeCompare(后一个.完整路径文本, "zh-CN");
  });
}
