# 卡池整合最终方案

**决策时间：** 2026-05-14
**决策人：** Nyx（Manager AI）+ 大王确认

---

## 最终决策

### 1. 卡牌数量
**采用 20 张美术规范方案**，28张方案作为后续 DLC 扩展预留。

### 2. 类型体系
**采用 4 类型（ECardType）：** Attack / Defense / Transform / Utility

理由：
- 与美术规范完全契合
- ECardType.cs 无需重构
- 技术改动量最小化

### 3. 能耗体系
**采用美术规范 5-35 区间**，数值设计的 0-6 区间作为参考基准。

美术规范能耗需按比例重映射：
- 数值低（0-2）→ 美术低（5-15）
- 数值中（3-4）→ 美术中（16-25）
- 数值高（5-6）→ 美术高（26-35）

### 4. card-004 配图修正
**问题：** 美术规范中"穿云箭"配图实为玄武护卫，与数值设计完全错位

**修正方案：**
- 重新生成 card-004 配图（真正弓箭手射箭图）
- "玄武护卫"概念暂存，评估是否作为 card-021 扩展

### 5. ID体系
**统一使用 `card-XXX` 格式**（美术规范格式），废弃 `card_XXX` 格式

### 6. 数据源
建立单一数据源：`card-master-data.json`（所有卡牌数据的唯一真相来源）

---

## 技术改动清单（按优先级）

| 优先级 | 改动项 | 负责人 |
|--------|--------|--------|
| P0 | 更新 CardData.cs，能耗字段重映射 | Orion |
| P0 | 清理 ECardType 和 CardType 双枚举问题 | Orion |
| P0 | card-004 配图重新生成 | Artoria |
| P1 | 建立 card-master-data.json 单一数据源 | Orion |
| P1 | 卡牌加载逻辑统一 | Orion |
| P2 | UI 类型图标适配 4 类型体系 | Artoria |
| P2 | 扩展卡池规划（21-28） | Kairos |

---

## 待 Artoria 确认
- card-004 新配图的具体需求（弓箭手姿态、朝向、背景）
- 4类型体系的卡牌立绘风格是否需要微调

## 待 Orion 确认
- 能耗重映射的具体数值对照表
- CardData.cs 当前双枚举问题的清理方案

---

## 文件索引

| 文件 | 说明 |
|------|------|
| `ArtDocuments/card-asset-manifest-v2.md` | 美术规范（20张卡，基础） |
| `design-output/card-numbers-v2.md` | 数值设计参考（28张卡，DLC预留） |
| `design-output/card-pool-resolution.md` | Artoria 美术整合方案 |
| `design-output/card-pool-tech-review.md` | Orion 技术评估报告 |
| `design-output/card-pool-final-resolution.md` | 本文件，最终决策 |
