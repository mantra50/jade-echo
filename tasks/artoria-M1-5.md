# Artoria M1-5 任务清单

> **角色：** Artoria（美术总监）
> **任务：** UI 对接 — 读懂现有 UI 代码，输出对接方案
> **日期：** 2026-05-13

---

## ✅ 已完成

### T1 — 读取现有 UI 代码
- [x] CanvasLayout.cs — 7 层 Canvas 层级结构
- [x] BoardUI.cs — 8×7 棋盘格子、棋子状态、特效触发点
- [x] CardPanelUI.cs — 手牌拖拽/扇形布局/选中高亮
- [x] HUDUI.cs — 能量条/血量/波次/倒计时/结束回合按钮
- [x] BattleFlowManager.cs — 核心事件总线（OnPhaseChanged / OnEliminationOccurred 等）

### T2 — 输出 UI 对接方案
- [x] `art-output/ui-integration.md` 已写入
  - 涵盖：BoardUI / CardPanelUI / HUDUI / CanvasLayout
  - 每个面板需要的接口、数据类型、事件订阅方式
  - 动画规格（入场/悬停/选中/拖拽/消除特效）
  - BattlePhase → UI 状态映射
  - Orion 需要实现的接口清单
  - 已知缺口列表

---

## 🔲 待完成（需要 Orion 配合）

### T3 — 确认 CardUI 组件
Orion 或程序需要实现 `CardUI.cs` 并提供以下接口：
```csharp
void Init(CardData data);
void SetHovered(bool hover);
void SetSelected(bool selected);
void SetDragging(bool dragging);
CardData Data { get; }
```
**关联：** CardPanelUI 依赖此组件，`_cardPrefab` 需要挂载此脚本。

### T4 — 确认 ECardType vs CardType 统一
CardPanelUI 引用了 `ECardType`（Attack/Defense/Transform/Utility），但 CardData 中定义的是 `CardType`。
需要 Orion 确认：
- 是补充 `ECardType` 枚举，还是 CardPanelUI 改用 `CardType`？

### T5 — 确认棋盘数据来源
UI 需要在初始化和更新时从 Gameplay 层获取棋盘状态。
Orion 需要确认：
- `GridManager` 实例访问方式（单例还是注入）
- 棋子图标/颜色由谁来提供（Resources 路径？Atlas？）

### T6 — ShopUI / Popup 层对接
本次 M1-5 未涉及，后续需对接时再补充。

---

## 📤 产出物

| 文件 | 路径 |
|---|---|
| UI 对接方案 | `art-output/ui-integration.md` |
| 本任务清单 | `tasks/artoria-M1-5.md` |