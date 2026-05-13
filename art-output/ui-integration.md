# UI 层完整对接方案

> **作者：** Artoria（美术总监）
> **版本：** v0.1（基于现有 UI 代码 + BattleFlowManager 接口）
> **日期：** 2026-05-13

---

## 一、整体架构

```
BattleFlowManager（Gameplay 层）
        │
        ├── OnPhaseChanged    → 驱动 UI 状态切换
        ├── OnTurnStarted    → 更新回合数/能量/抽卡
        ├── OnTurnEnded       → 回合结算数据刷新
        ├── OnEliminationOccurred → 消除特效触发
        ├── OnCardEffectTriggered → 卡牌效果反馈
        └── OnPlayerDamaged   → 血量变更动画
        │
        ▼
   各 UI 面板（View 层）
        ├── BoardUI    — 棋盘格子、棋子显示、消除特效
        ├── CardPanelUI — 手牌展示、拖拽出牌、选中高亮
        ├── HUDUI      — 能量条、血量、波次、倒计时、结束回合
        └── CanvasLayout — 层级管理
```

**数据流向：** BattleFlowManager 是核心驱动源，各 UI 面板通过订阅其事件来驱动自身状态和动画。

---

## 二、BoardUI 对接

### 2.1 事件订阅

```csharp
// 在某个 ViewManager 或 BattleUI 初始化时订阅
BattleFlowManager.Instance.OnEliminationOccurred += HandleElimination;
BattleFlowManager.Instance.OnPhaseChanged        += HandlePhaseChange;

// BattleFlowManager 还暴露了这些你需要的数据：
// GridManager / GridManager.Instance 获取棋盘状态
// GridManager.Instance.Get(x, y) → Piece
```

### 2.2 需要 Orion 提供的接口 / 数据

| 我需要的数据 | 数据类型 | 用途 |
|---|---|---|
| `GridManager.Instance.Get(x, y)` | `Piece` | 获取格子当前棋子类型、状态 |
| `Piece.Type` | `ElementType` 枚举 | 显示对应图标 |
| `Piece.State` | `PieceState` 枚举 | Selected / Idle / Matching 等状态 |
| 棋子 Sprite/颜色映射 | `Sprite` 或 `Color` | 每个 ElementType 对应一张东方古风图标 |

### 2.3 消除特效触发

```csharp
// Orion 需要在 MatchDetector 检测到 3+ 连消时调用
BoardUI.Instance.PlayMatchVFX(List<Vector2Int> cells);

// 东方古风特效要求（美术需求）：
// - MatchVFX：樱花飘落 + 墨迹消散（粉色系 + 水墨灰）
// - AttackVFX：剑气横贯（从 from 飞向 to）
// - ShatterVFX：玉碎裂开（金色/翠绿碎粒）
// - SpellVFX：符咒爆发（紫色光圈 + 符文旋转）
```

### 2.4 格子高亮状态

```csharp
// Orion 在玩家拖拽出牌过程中高亮有效/无效目标
BoardUI.Instance.SetCellHighlight(col, row, EBoardCellHighlight.Valid);   // 绿透明
BoardUI.Instance.SetCellHighlight(col, row, EBoardCellHighlight.Invalid);  // 红透明
BoardUI.Instance.SetCellHighlight(col, row, EBoardCellHighlight.Special);   // 紫透明（连消提示）
BoardUI.Instance.SetCellHighlight(col, row, EBoardCellHighlight.None);      // 清除
```

### 2.5 棋子放置

```csharp
// Orion 在卡牌效果执行后，更新棋盘格子内容
BoardUI.Instance.SetCellContent(col, row, EBoardCellType type, Sprite icon);
// type：Empty / PlayerMinion / EnemyMinion / Obstacle
```

---

## 三、CardPanelUI 对接

### 3.1 事件订阅

```csharp
CardPanelUI.Instance.OnCardPlayed   += HandleCardPlayed;   // 卡牌被打出（拖拽到棋盘 or 点击再次点击）
CardPanelUI.Instance.OnCardSelected += HandleCardSelected; // 卡牌被选中
```

### 3.2 需要 Orion 提供的接口 / 数据

| 我需要的数据 | 数据类型 | 用途 |
|---|---|---|
| `CardData` 列表（手牌） | `List<CardData>` | 初始化手牌显示 |
| 卡牌图标 | `Sprite`（通过 `CardData.IconPath` 加载） | 显示在卡牌正面 |
| 卡牌名称、能耗、描述 | `string` / `int` | 卡牌 UI 文字 |
| 稀有度颜色 | `Color`（`CardData.RarityColor`） | 卡牌边框发光色 |
| 卡牌类型 | `CardType` 枚举 | 决定卡牌颜色主题（Attack=红, Defense=蓝等） |

### 3.3 关键问题：CardUI 类未实现

> **⚠️ 注意：** `CardPanelUI.cs` 中引用了 `cardUI.GetComponent<CardUI>()`，但项目中未找到 `CardUI.cs` 文件。
>
> **Artoria 期望的 CardUI 组件接口：**

```csharp
public class CardUI : MonoBehaviour
{
    // 初始化（传入卡牌数据）
    public void Init(CardData data);

    // 悬停状态
    public void SetHovered(bool hover);

    // 选中状态（发光边框 + 上移）
    public void SetSelected(bool selected);

    // 拖拽状态（放大 + 跟随鼠标）
    public void SetDragging(bool dragging);

    // 读取当前卡牌数据（CardPanelUI 事件回调需要）
    public CardData Data { get; }
}
```

**CardUI 需要 Orion 或程序实现**，美术提供预制体和所有状态下的视觉规范。

### 3.4 手牌管理（Orion 提供）

```csharp
// 添加一张手牌
CardPanelUI.Instance.AddCard(CardData data);

// 移除一张手牌（打出后自动调用，无需 Orion 手动触发）
CardPanelUI.Instance.RemoveCard(CardUI card);

// 清空手牌（重新开始时）
CardPanelUI.Instance.ClearHand();

// 获取当前手牌数量
int count = CardPanelUI.Instance.HandCount;
```

### 3.5 出牌时的事件回调

```csharp
// CardPanelUI 触发 OnCardPlayed 时，携带 CardData
// Orion 在此回调中：
// 1. 记录出牌索引（或直接传 CardData）
// 2. 调用 BattleFlowManager.PlayCard(handIndex, targetX, targetY)
// 3. CardPanelUI 会自动播放飞走动画并移除该卡
```

### 3.6 卡牌拖拽出牌流程

```
玩家拖拽手牌 → 进入棋盘区域 → 释放
        │
        ▼
CardPanelUI.OnCardPlayed?.Invoke(CardData)
        │
        ▼
Orion 收到回调 → 调用 BattleFlowManager.PlayCard(index, x, y)
        │
        ▼
消除/效果触发 → UI 响应 OnEliminationOccurred / OnCardEffectTriggered
```

---

## 四、HUDUI 对接

### 4.1 事件订阅

```csharp
HUDUI.Instance.OnEndTurnClicked += HandleEndTurn; // 玩家点击"结束回合"按钮
```

### 4.2 需要 Orion 提供的接口 / 数据

| 我需要的数据 | 用途 |
|---|---|
| `SetEnergy(int current, int max)` | 每回合开始时设置能量 |
| `SetPlayerHP(int current, int max)` | 血量变化时更新 |
| `SetEnemyHP(int current, int max)` | 敌人血量更新 |
| `SetWave(int wave)` | 波次显示（如果有波次概念） |
| `SetTurn(int turn)` | 回合数 |
| `SetDeckCount(int count)` | 牌堆剩余数量 |
| `SetHandCount(int count)` | 手牌数量 |
| `StartPlayerTurn(energy, maxEnergy)` | 玩家回合开始（启动倒计时、激活按钮） |
| `EndPlayerTurn()` | 玩家回合结束（停止倒计时） |
| `PlayEnergyGainVFX()` | 能量增加时播放东方古风能量粒子（待实现） |

### 4.3 回合计时器

```csharp
// HUDUI 内部有 30 秒倒计时，到 0 自动结束回合
// Orion 可通过 StartPlayerTurn() 启动计时器
// 倒计时 UI 显示：⏱ 24 （整除秒）
```

### 4.4 "Your Turn!" / "Enemy Turn" 提示

```csharp
// HUDUI 内部管理 _yourTurnIndicator / _enemyTurnIndicator
// Orion 通过 StartPlayerTurn() / EndPlayerTurn() 控制这两个 GameObject 的显隐
```

---

## 五、CanvasLayout 对接

### 5.1 层级定义

| ELayer | SortOrder | Z轴 | 用途 |
|---|---|---|---|
| Board | 10 | 0 | 棋盘格子层 |
| Card | 20 | -10 | 手牌层（在棋盘上方） |
| HUD | 30 | -20 | HUD 信息层 |
| Shop | 40 | -30 | 商店层 |
| Popup | 50 | -40 | 弹窗层 |
| Overlay | 60 | -50 | 特效遮罩层 |
| Topmost | 70 | -60 | 加载/提示层 |

### 5.2 Orion 需要的接口

```csharp
// 切换层级的激活状态（Orion 控制各面板显隐）
CanvasLayout.Instance.SetLayerActive(ELayer.Popup, true);
CanvasLayout.Instance.SetLayerActive(ELayer.Popup, false);

// 临时提升某层到最前（弹窗需要打断时）
CanvasLayout.Instance.BringToFront(ELayer.Popup, 10);
CanvasLayout.Instance.ResetSortOrder(ELayer.Popup);
```

---

## 六、UI 动画规格

### 6.1 手牌入场动画

```
持续时间：0.35s
缓动函数：ease-out（平滑停止）
运动：从下方 Y-200 弹入到扇形排列位置
```

### 6.2 手牌出牌动画

```
持续时间：0.3s
缓动函数：ease-in（加速飞向棋盘方向）
目标点：屏幕上方中央（或棋盘中央的 Camera 方向）
```

### 6.3 手牌悬停动画

```
上移：Y + 30px
缩放：×1.05
过渡：平滑插值（Time.deltaTime * 12）
```

### 6.4 手牌选中动画

```
上移：Y + 18px（比悬停幅度小）
缩放：×1.02
边框发光：B8860B（金色）半透明
```

### 6.5 手牌拖拽动画

```
缩放：×1.08（比悬停更大）
跟随鼠标（实时，localPosition = 鼠标坐标）
```

### 6.6 消除特效（MatchVFX）

```
东方古风要求：
- 主粒子：樱花瓣（粉色，旋转飘落）
- 次粒子：墨迹消散（灰黑色，淡出）
- 持续时间：约 1.0s（含延迟销毁 0.5s）
```

### 6.7 能量增加动画

```
待实现（建议：能量球从中央爆发，光点飞向能量条）
```

---

## 七、状态定义汇总

### 7.1 BattlePhase → UI 状态映射

| BattlePhase | UI 响应 |
|---|---|
| `Idle` | 全部 UI 隐藏或等待 |
| `PlayerInput` | HUD 激活、手牌可交互、棋盘可点击 |
| `Eliminating` | 手牌不可交互、棋盘禁用、播放消除特效 |
| `CardPhase` | 展示卡牌触发效果（如有） |
| `Settlement` | 显示"回合结算"提示（可选），然后进入下一回合 |

### 7.2 BoardUI 格子状态

```csharp
enum EBoardCellType { Empty, PlayerMinion, EnemyMinion, Obstacle }
enum EBoardCellHighlight { None, Valid, Invalid, Special }
```

### 7.3 CardUI 状态

```
Normal → Hover → Selected → Dragging（可组合）
```

### 7.4 HUD 能量条颜色

```
能量比例 < 30%：暗绿色
能量比例 30%~70%：渐变过渡
能量比例 > 70%：明亮青绿色（#2A9D8F）
```

---

## 八、Orion 需要实现的核心接口清单

> 以下是 Artoria 期望 Orion 在 Gameplay 层提供的接口，
> UI 层将通过这些接口驱动所有视觉反馈。

### 8.1 棋盘同步

```csharp
// 棋盘初始化时，UI 读取所有格子数据并显示
GridManager.Instance.Get(x, y) → Piece
Piece.Type  → ElementType（显示对应图标）
Piece.State → PieceState（Selected / Idle 等）

// 消除发生后，更新格子内容
GridManager.Instance.Set(x, y, newPiece);
// 或者通过 GridManager 提供批量刷新接口
```

### 8.2 手牌数据源

```csharp
// 初始化时，UI 需要知道有哪些卡牌在手牌中
HandManager.Hand → List<CardData>

// 每次手牌变化（抽牌/出牌），UI 需要收到通知
// CardPanelUI 已有事件 OnCardPlayed，Orion 负责驱动 AddCard/RemoveCard
```

### 8.3 消除特效数据

```csharp
// MatchDetector 检测到消除时，提供受影响格子坐标列表
// Orion 调用：
BoardUI.Instance.PlayMatchVFX(cells); // List<Vector2Int>

// 同时更新格子状态（清空/刷新）
// （如果是新棋子生成，也需要 SetCellContent）
```

### 8.4 血量/能量更新

```csharp
// 每回合开始：BattleFlowManager 的 OnTurnStarted 触发 HUD 更新
// Orion 在 OnTurnStarted 回调中调用：
HUDUI.Instance.StartPlayerTurn(energy, maxEnergy);
HUDUI.Instance.SetPlayerHP(hp, maxHp);

// 敌人血量变化时：
HUDUI.Instance.SetEnemyHP(enemyHp, enemyMaxHp);
```

### 8.5 回合结束

```csharp
// HUD 结束回合按钮点击时：
// HUDUI 内部触发 OnEndTurnClicked，Orion 收到后：
// 1. HUDUI.EndPlayerTurn()
// 2. BattleFlowManager 处理敌人回合
```

---

## 九、已知缺口（需要 Orion 确认）

1. **CardUI.cs 不存在** — 需程序实现，美术提供预制体规范
2. **CardData.ECardType 枚举缺失** — CardPanelUI 引用了 `ECardType`，但 CardData 中只有 `CardType`；需要统一或补充
3. **ShopUI / 弹窗层** — 本次 M1-5 未涉及，需要时再对接
4. **消除音效** — CardPanelUI 有 SFX 字段但未实现音效管理器，需要统一 AudioSource 管理
5. **能量增加特效** — HUDUI.PlayEnergyGainVFX() 标注 TODO，需要美术规范或程序先占位

---

## 十、文件输出清单

| 文件 | 路径 |
|---|---|
| UI 对接方案（本文档） | `art-output/ui-integration.md` |
| 任务清单 | `tasks/artoria-M1-5.md` |
| UI 预制体规范（待输出） | `art-output/ui-spec.md`（M1-6） |
| 卡牌预制体规范（待输出） | `art-output/card-prefab-spec.md`（M1-6） |