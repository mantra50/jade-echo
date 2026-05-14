# 能耗重映射 & 双枚举清理方案

**编写人：** Orion（技术总监）
**日期：** 2026-05-14
**状态：** 待实施

---

## 一、能耗重映射表

美术规范能耗区间为 **5-35**，原数值设计 0-6 作为参考基准。

由于美术规范已直接使用 5-35 区间，重映射为**直接映射**（无需线性变换）。

### 完整 20 张卡映射

| 卡牌ID | 中文名 | 类型 | 稀有度 | 美术能耗 | 映射能耗 |
|--------|--------|------|--------|----------|----------|
| card-001 | 破风斩 | Attack | N | 15 | 15 |
| card-002 | 烈焰冲击 | Attack | R | 20 | 20 |
| card-003 | 剑气纵横 | Attack | R | 25 | 25 |
| card-004 | 穿云箭 | Attack | R | 18 | 18 |
| card-005 | 雷罚天降 | Attack | SR | 35 | 35 |
| card-006 | 护体金钟 | Defense | N | 10 | 10 |
| card-007 | 气盾术 | Defense | R | 20 | 20 |
| card-008 | 磐石壁垒 | Defense | R | 25 | 25 |
| card-009 | 玄冰护甲 | Defense | R | 15 | 15 |
| card-010 | 万灵归元 | Defense | SR | 30 | 30 |
| card-011 | 玉石转化 | Transform | R | 15 | 15 |
| card-012 | 元素乱流 | Transform | R | 20 | 20 |
| card-013 | 火焰同调 | Transform | R | 18 | 18 |
| card-014 | 疾风驱散 | Transform | N | 12 | 12 |
| card-015 | 灵泉涌现 | Transform | SR | 22 | 22 |
| card-016 | 灵气吸纳 | Utility | N | 5 | 5 |
| card-017 | 卡牌回收 | Utility | R | 0 | 0 |
| card-018 | 玉石祝福 | Utility | R | 8 | 8 |
| card-019 | 绝境逢生 | Utility | SR | 0 | 0 |
| card-020 | 天眼通 | Utility | SR | 15 | 15 |

**映射结论：** 美术规范能耗值直接使用，无数值转换需求。

### 能耗分布统计

| 类型 | 最小 | 最大 | 中位 |
|------|------|------|------|
| Attack | 15 | 35 | 21.5 |
| Defense | 10 | 30 | 18 |
| Transform | 12 | 22 | 16.5 |
| Utility | 0 | 15 | 6.5 |

---

## 二、双枚举问题分析

### 问题根源

`CardData.cs` 中同时存在两个枚举：

**旧枚举（已废弃）：**
```csharp
public enum CardType  // 在 CardData.cs 内部定义
{
    None        = 0,
    ClearRow    = 1,
    ClearCol    = 2,
    ClearArea   = 3,
    Bomb        = 4,
    Shuffle     = 5,
    Swap        = 6,
    EnergyBoost = 7,
    Heal        = 8
}
```

**新枚举（ECardType.cs）：**
```csharp
public enum ECardType
{
    None        = 0,
    Attack      = 1,
    Defense     = 2,
    Transform   = 3,
    Utility     = 4
}
```

**CardData 字段声明：**
```csharp
public ECardType CardType;  // 已是 ECardType，但旧 enum CardType 还定义在同文件
```

### 影响范围

| 文件 | 用途 | 问题 |
|------|------|------|
| `CardData.cs` | 枚举定义 + 字段声明 | 旧 enum 与 ECardType 重名冲突 |
| `HandManager.cs` | 测试卡牌创建 | 赋值旧 enum 值给 ECardType 字段 |
| `CardExecutor.cs` | 卡牌效果执行 | switch case 使用旧 enum 值 |
| `BidirectionalLinker.cs` | 连锁反应 | switch case 使用旧 enum 值 |
| `BattleFlowManager.cs` | 战斗流程 | 比较 `CardType.EnergyBoost` |
| `ComboManager.cs` | 连击管理 | 比较 `CardType.EnergyBoost` |
| `CardUI.cs` | UI 显示 | 读取 `ECardType`（正常） |

### 枚举清理方案

**步骤 1：移除旧 CardType enum 定义**
- 从 `CardData.cs` 中删除整个 `public enum CardType { ... }` 块

**步骤 2：统一使用 ECardType**
- 所有卡牌类型判断改用 ECardType（Attack/Defense/Transform/Utility）
- 旧枚举的 9 种效果类型（ClearRow/ClearCol/ClearArea/Bomb/Shuffle/Swap/EnergyBoost/Heal）重新归类到 4 类

**效果类型重映射表：**

| 旧效果类型 | 归类 | ECardType |
|-----------|------|-----------|
| ClearRow | Attack（攻击棋盘行） | Attack |
| ClearCol | Attack（攻击棋盘列） | Attack |
| ClearArea | Attack（区域清除） | Attack |
| Bomb | Attack（元素轰炸） | Attack |
| Shuffle | Transform（重置棋盘） | Transform |
| Swap | Utility（交换棋子） | Utility |
| EnergyBoost | Utility（能量获取） | Utility |
| Heal | Defense（回复生命） | Defense |

**步骤 3：CardExecutor 架构调整**
- switch (card.CardType) 从 8 个 case 简化为 4 个（按 ECardType）
- 具体效果逻辑通过 card.Range、card.Value 等字段区分

**步骤 4：BidirectionalLinker 调整**
- 同上，switch 简化为 4 类

---

## 三、代码改动清单

### 3.1 CardData.cs（关键改动）

**改动点 1：删除旧枚举定义**
```csharp
// 删除以下代码块：
public enum CardType
{
    None        = 0,
    ClearRow    = 1,
    ClearCol    = 2,
    ClearArea   = 3,
    Bomb        = 4,
    Shuffle     = 5,
    Swap        = 6,
    EnergyBoost = 7,
    Heal        = 8
}
```

**CardData 字段保持不变（已是 ECardType）：**
```csharp
public ECardType CardType;
public int EnergyCost;  // 能耗值直接使用美术规范（5-35区间）
```

### 3.2 HandManager.cs

**改动点：CreateTestCardPool() 中的测试卡牌类型**
```csharp
// 旧：
clearRow.CardType = CardType.ClearRow;
bomb.CardType = CardType.Bomb;
clearArea.CardType = CardType.ClearArea;
energyBoost.CardType = CardType.EnergyBoost;

// 新：
clearRow.CardType = ECardType.Attack;
bomb.CardType = ECardType.Attack;
clearArea.CardType = ECardType.Attack;
energyBoost.CardType = ECardType.Utility;
```

### 3.3 CardExecutor.cs

**改动点：switch (card.CardType) 从 8 case 到 4 case**

```csharp
// 旧：
switch (card.CardType)
{
    case CardType.ClearRow: return ExecuteClearRow(targetY, grid);
    case CardType.ClearCol: return ExecuteClearCol(targetX, grid);
    case CardType.ClearArea: return ExecuteClearArea(targetX, targetY, card.Range, grid);
    case CardType.Bomb: return ExecuteBomb(card.TargetElement, grid);
    case CardType.Shuffle: return ExecuteShuffle(grid);
    case CardType.Swap: return new List<Piece>();
    case CardType.EnergyBoost: return new List<Piece>();
    case CardType.Heal: return new List<Piece>();
}

// 新（按 ECardType 4分）：
switch (card.CardType)
{
    case ECardType.Attack:
        // 根据 Range 判断：AOE范围用 ClearArea，否则用横向/纵向清除
        if (card.Range > 1)
            return ExecuteClearArea(targetX, targetY, card.Range, grid);
        else if (card.TargetElement != ElementType.None)
            return ExecuteBomb(card.TargetElement, grid);
        else
            return ExecuteClearRow(targetY, grid); // 默认行清除
        
    case ECardType.Defense:
        // 防御类目前无棋盘消除逻辑，返回空
        return new List<Piece>();
        
    case ECardType.Transform:
        return ExecuteShuffle(grid);
        
    case ECardType.Utility:
        // 能量获取/卡牌回收无棋盘消除
        return new List<Piece>();
        
    default:
        return new List<Piece>();
}
```

### 3.4 BidirectionalLinker.cs

**改动点：OnCardExecuted() switch 简化**

```csharp
// 旧：
switch (card.CardType)
{
    case CardType.ClearArea:
    case CardType.ClearCol:
    case CardType.ClearRow:
        CreateColorBomb AtCenter(affected, grid); break;
    case CardType.Bomb:
        CreateBlastZone(affected, grid); break;
    case CardType.Shuffle:
        break;
}

// 新：
switch (card.CardType)
{
    case ECardType.Attack:
        if (card.TargetElement != ElementType.None)
            CreateBlastZone(affected, grid);
        else
            CreateColorBomb AtCenter(affected, grid);
        break;
    case ECardType.Transform:
        // 洗牌无特殊格子生成
        break;
    default:
        break;
}
```

### 3.5 BattleFlowManager.cs

**改动点：ultimate 筛选逻辑**

```csharp
// 旧：
if (_combo.SparkLevel >= 3 && card.CardType == CardType.EnergyBoost)

// 新：
if (_combo.SparkLevel >= 3 && card.CardType == ECardType.Utility)
```

### 3.6 ComboManager.cs

**改动点：ultimate 激活判断**

```csharp
// 旧：
if (card.CardType == CardType.EnergyBoost && !_activatedUltimateIds.Contains(card.CardId))

// 新：
if (card.CardType == ECardType.Utility && !_activatedUltimateIds.Contains(card.CardId))
```

---

## 四、文件清单汇总

| 文件 | 改动类型 | 改动内容 |
|------|----------|----------|
| `CardData.cs` | 删除枚举 | 移除 `public enum CardType { ... }` |
| `HandManager.cs` | 字段赋值 | 4处 `CardType.XXX` → `ECardType.XXX` |
| `CardExecutor.cs` | switch 重构 | 8 case → 4 case（按 ECardType） |
| `BidirectionalLinker.cs` | switch 重构 | 简化为 4 case |
| `BattleFlowManager.cs` | 比较逻辑 | `CardType.EnergyBoost` → `ECardType.Utility` |
| `ComboManager.cs` | 比较逻辑 | `CardType.EnergyBoost` → `ECardType.Utility` |

---

## 五、git 提交信息

```
fix(card): 统一 ECardType，移除旧 CardType 枚举

- 删除 CardData.cs 中废弃的 CardType enum（9值）
- 能耗值直接采用美术规范（5-35区间，无转换）
- CardExecutor/BidirectionalLinker switch 重构为4类
- BattleFlowManager/ComboManager EnergyBoost→Utility重分类
- HandManager 测试卡牌类型同步更新
```
