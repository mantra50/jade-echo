# 战斗系统 ↔ UI 接口文档
> 版本：v1.0
> 日期：2026-05-13
> 编写：Orion（技术总监）
> 状态：**草稿**，待 Artoria 确认

---

## 一、概述

本文档定义战斗系统（BattleFlowManager + BossBase）与 UI 层（BoardUI、CardPanelUI、HUDUI）之间的所有交互接口。

- **事件方向**：战斗系统 → UI（通知，UI 订阅）
- **调用方向**：UI → 战斗系统（操作，UI 主动调用）

---

## 二、事件总线（战斗系统 → UI）

### 2.1 BattleFlowManager 事件

#### `OnPhaseChanged(BattlePhase phase)`
- **说明**：战斗阶段切换
- **参数**：
  - `phase`: `BattlePhase` 枚举值
    - `Idle = 0` — 未开始
    - `PlayerInput = 1` — 玩家操作阶段
    - `Eliminating = 2` — 消除连锁阶段
    - `CardPhase = 3` — 卡牌触发阶段
    - `Settlement = 4` — 回合结算阶段
- **UI 响应**：HUD 更新"你的回合/敌人回合"提示；CardPanel 启用/禁用交互

#### `OnTurnStarted(int turnNumber)`
- **说明**：新回合开始
- **参数**：`turnNumber` 从 1 开始
- **UI 响应**：`HUDUI.SetTurn(turnNumber)`；HUD 启用结束回合按钮

#### `OnTurnEnded(int turn, int energyProduced)`
- **说明**：回合结算完成
- **参数**：
  - `turn`：本回合编号
  - `energyProduced`：本次产生的能量
- **UI 响应**：`HUDUI.PlayEnergyGainVFX()`；能量条动画更新

#### `OnPlayerDamaged(int damage)`
- **说明**：玩家受到伤害（来自 Boss 普攻）
- **参数**：`damage` 伤害值（正整数）
- **UI 响应**：HUD 扣血动画、红色闪烁、屏幕震动（建议由 UI 实现）

#### `OnBattleFinished`
- **说明**：战斗结束（玩家胜利或失败）
- **UI 响应**：弹出结算界面（全屏遮罩 + 结果展示）

#### `OnEliminationOccurred(int comboCount, List<Piece> pieces)`
- **说明**：发生棋子消除
- **参数**：
  - `comboCount`：本次连锁数（≥1）
  - `pieces`：被消除的棋子列表
- **UI 响应**：`BoardUI.PlayMatchVFX(cells)` 消除特效；连消数字飘字
- **备注**：`pieces` 中每个 `Piece` 包含 `X`, `Y` 坐标（`int`）和 `Type`（`ElementType` 枚举）

#### `OnCardEffectTriggered(CardData card, List<Piece> affected)`
- **说明**：卡牌效果被触发
- **参数**：
  - `card`：被触发的卡牌数据
  - `affected`：受影响的棋子列表（可为空）
- **UI 响应**：`BoardUI.PlaySpellVFX(pos)` 法术特效；在目标格子高亮

---

### 2.2 BossBase 事件

#### `OnAttackIssued(string attackName)`
- **说明**：Boss 发动攻击（普攻或特殊攻击）
- **参数**：`attackName` 如 `"普攻 5"`、`"暗影冲击"`
- **UI 响应**：HUD 红色警告文字；Boss 头像攻击动画

#### `OnHpChanged(int currentHp, int maxHp)`
- **说明**：Boss 生命值变化
- **参数**：
  - `currentHp`：当前 HP
  - `maxHp`：最大 HP
- **UI 响应**：`HUDUI.SetEnemyHP(currentHp, maxHp)`；血条动画
- **伤害格式**：`currentHp / maxHp` 显示，如 `"45/100"`

#### `OnPhaseChanged`
- **说明**：Boss 进入第二阶段（HP ≤ 50%）
- **UI 响应**：Boss 造型切换；HUD 特殊提示"Phase 2"；Boss 血条变色

#### `OnBossDied`
- **说明**：Boss 被击败
- **UI 响应**：`BoardUI.PlayShatterVFX(pos)` 碎裂特效；HUD 显示胜利结算

#### `OnShieldChanged(int shieldValue)`
- **说明**：Boss 护盾值变化
- **UI 响应**：护盾条显示/更新

---

## 三、UI 调用的战斗系统方法（UI → 战斗系统）

### 3.1 棋子交换

```csharp
Task<bool> BattleFlowManager.TrySwapAsync(int x1, int y1, int x2, int y2)
```
- **调用方**：`BoardUI`（在玩家拖拽/点击交换后）
- **参数**：两个相邻格子的坐标（0-indexed）
- **返回值**：`true` = 交换成功；`false` = 相邻判断失败或阶段不对
- **阶段要求**：仅在 `BattlePhase.PlayerInput` 时有效

### 3.2 棋盘格子点击（简化交互）

```csharp
void BattleFlowManager.HandleCellClick(Piece piece)
```
- **调用方**：`BoardUI`（单步点击模式：第一次选、第二次换）
- **参数**：`piece` 为被点击的棋子引用
- **阶段要求**：`BattlePhase.PlayerInput`

### 3.3 出牌

```csharp
// 无目标出牌（自动选第一个棋子）
void BattleFlowManager.HandlePlayCard(int handIndex)

// 有目标出牌
CardData BattleFlowManager.PlayCard(int handIndex, int targetX, int targetY)
```
- **调用方**：`CardPanelUI`（玩家点击/拖拽卡牌）
- **参数**：
  - `handIndex`：手牌槽位索引
  - `targetX, targetY`：目标棋盘坐标（-1 表示全局效果）
- **返回值**：`CardData` 成功打出时返回卡牌数据；`null` = 阶段不对或能量不足
- **阶段要求**：`BattlePhase.PlayerInput`

### 3.4 跳过回合

```csharp
void BattleFlowManager.SkipTurn()
```
- **调用方**：`HUDUI`（结束回合按钮）
- **阶段要求**：`BattlePhase.PlayerInput`

### 3.5 Boss 伤害

```csharp
void BossBase.TakeDamage(int damage)
void BossBase.AddShield(int amount)
void BossBase.ClearShield()
void BossBase.Heal(int amount)
void BossBase.SetHp(int hp)
```
- **调用方**：通常由 `CardExecutor` 或消除规则自动调用；UI **不直接调用**

---

## 四、辅助数据结构和枚举

### 4.1 Piece 相关

```csharp
class Piece {
    int X, Y;              // 棋盘坐标 (0-indexed)
    ElementType Type;      // 元素类型枚举
    PieceState State;      // Idle / Selected
}
```

### 4.2 CardData 相关

```csharp
class CardData {
    string CardName;       // 卡牌名称
    int EnergyCost;        // 能量消耗
    ECardType CardType;    // Attack / Defense / Transform / Utility
    ElementType TargetElement; // 触发元素的属性
}
```

### 4.3 伤害数字格式规范

- **显示格式**：`"{current}/{max}"`（如 `45/100`）
- **伤害飘字**：统一使用红色，格式为 `"-{damage}"`（如 `"-5"`）
- **治疗飘字**：统一使用绿色，格式为 `"+{heal}"`（如 `+10`）
- **Boss 伤害飘字**：在 Boss 头像附近显示，格式同上

---

## 五、坐标系统

- **棋盘**：`COLS = 8`（列）× `ROWS = 7`（行）
- **坐标系**：左上角为 `(0, 0)`，向右 X+1，向下 Y+1
- **玩家区**：row 4~6（4行，最下方）
- **敌人区**：row 0~3（3行，最上方）
- **格子间距**：`4px`

---

## 六、建议的 UI 行为对照表（Artoria 需对接）

| 战斗系统事件 | 期望 UI 行为 | 特效类型 |
|---|---|---|
| `OnPhaseChanged(PlayerInput)` | 启用卡牌交互、结束回合按钮 | — |
| `OnPhaseChanged(Eliminating)` | 禁用卡牌交互 | — |
| `OnEliminationOccurred` | 消除格子播放 VFX + 连消数字 | `PlayMatchVFX` |
| `OnCardEffectTriggered` | 目标格子法术特效 | `PlaySpellVFX` |
| `OnPlayerDamaged` | 屏幕红色闪烁 + 震动 | — |
| `OnHpChanged(Boss)` | Boss 血条动画 | — |
| `OnPhaseChanged(Boss)` | Boss 造型切换动画 | — |
| `OnBossDied` | Boss 碎裂特效 | `PlayShatterVFX` |
| `OnAttackIssued` | Boss 攻击动画 + HUD 警告 | — |
| `OnTurnEnded` | 能量增加动画 | `PlayEnergyGainVFX` |
| `OnBattleFinished` | 弹出结算界面 | — |

---

## 七、尚未完全定义的接口（待补充）

以下接口在当前代码中为 TODO 或不完整，**不影响 M1-5**，但需后续对齐：

1. **消除伤害计算**：每消除 N 个棋子 = 1 点伤害，具体规则待定（`PiecesPerDamage` 来自 `LevelConfig`）
2. **卡牌效果具体数值**：卡牌消耗、伤害值、效果类型由 CardSystem 决定，UI 只负责显示
3. **棋盘 Cell 类型映射**：`BoardCellUI.SetContent` 需要 `Sprite icon`，需 Artoria 提供图标资源规范

---

_文档版本 v1.0，如有问题请在群里 @Orion 确认。_