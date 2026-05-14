# 卡池技术评估

> **评估人：** Orion（技术总监）
> **日期：** 2026-05-14
> **文件版本：** 两套体系 v2 对比

---

## 一、技术差异对比

### 1. 卡牌 ID 体系

| 维度 | 美术规范（Manifest v2） | 数值设计（card-numbers v2） |
|------|----------------------|--------------------------|
| **ID 前缀** | `card-001` ~ `card-020`（kebab-case） | `card_001` ~ `card_028`（underscore_case） |
| **卡牌总数** | **20 张** | **28 张** |
| **编号位数** | 3 位数 | 3 位数 |
| **ID 格式冲突** | 是（分隔符不同） | 是 |

**结论：** 两套 ID 体系完全不兼容。数值设计还多了 8 张卡（card_021 ~ card_028）。

---

### 2. 卡牌类型枚举（ECardType）

| 维度 | 美术规范 | 数值设计 | 代码现状（ECardType.cs） |
|------|---------|---------|------------------------|
| **类型定义** | Attack / Defense / Transform / Utility | ClearRow / ClearCol / ClearArea / Bomb / Shuffle / Swap / EnergyBoost / Heal + 特殊 | Attack / Defense / Transform / Utility |
| **类型数量** | 4 种 | 8 种 + 1 种特殊（28号卡） | 4 种 |
| **与代码一致性** | ✅ 与 ECardType.cs 一致 | ❌ 与 ECardType.cs 不一致 | — |

**结论：** ECardType.cs 的 4 类型体系与美术规范一致，但与数值设计完全断裂。数值设计采用的是机制导向类型（按消除/效果分类），美术规范是主题导向类型（按战斗角色分类）。

---

### 3. 卡牌数据字段结构

美术规范字段（`card-asset-manifest-v2.md`）：
- `卡牌ID`、`中文名`、`英文名`、`类型`、`能耗`、`稀有度`
- `主色调`、`副色调`、`背景色`
- `AI 提示词`、`特效描述`

数值设计字段（`card-numbers-v2.md`）：
- `卡牌ID`、`中文名`、`类型`、`能耗`
- `效果范围`、`目标元素`、`伤害数值`、`额外效果`
- `稀有度`、`描述`、`设计备注`

**CardData.cs 当前字段：**
```csharp
CardId          // string — 兼容两套 ID 格式（但命名规则不同）
CardName        // string
ECardType       // 引用 ECardType — 4 类型体系
EnergyCost      // int
Description     // string
IconPath        // string
RarityColor     // Color
Range           // int — 效果范围
Value           // int — 效果数值
TargetElement   // ElementType — 目标元素
```

**关键差异：**
- 美术规范的色调/提示词字段不在 CardData.cs 中（属于美术资产管线）
- 数值设计的效果机制（Range/Value/TargetElement）在 CardData.cs 中有对应字段
- 数值设计的稀有度颜色规范（N=灰/R=蓝/SR=紫/UR=金）与代码当前 RarityColor 映射需确认一致性

---

## 二、代码现状

### ECardType.cs（路径：`Assets/_Game/Gameplay/CardSystem/ECardType.cs`）

```csharp
public enum ECardType
{
    None        = 0,
    Attack      = 1,   // 攻击类：直接造成伤害
    Defense     = 2,   // 防御类：生成护盾/减伤
    Transform   = 3,   // 转化类：改变棋盘棋子属性
    Utility     = 4    // 功能类：能量获取/抽卡/重排等
}
```

- **与美术规范一致：** ✅ 是（美术规范的 4 类型体系完全匹配）
- **与数值设计一致：** ❌ 否（数值设计用 8 种机制类型，不是这 4 种）

### CardData.cs（路径：`Assets/_Game/Gameplay/CardSystem/CardData.cs`）

```csharp
[CreateAssetMenu(fileName = "NewCard", menuName = "CardMatch/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基础信息")]
    public string CardId;
    public string CardName;
    public ECardType CardType;       // ← 引用 ECardType（4 类型）
    public int EnergyCost;
    [TextArea(2, 4)] public string Description;

    [Header("美术资源")]
    public string IconPath;
    public Color RarityColor = Color.white;

    [Header("效果参数")]
    public int Range;                // ClearArea/Bomb 用
    public int Value;                // EnergyBoost/Heal 用
    public ElementType TargetElement; // Bomb 用
}
```

**⚠️ 重要发现：** CardData.cs 文件内部还定义了一个 `CardType` 枚举（ClearRow/ClearCol/ClearArea/Bomb/Shuffle/Swap/EnergyBoost/Heal），但 CardData 的 `CardType` 字段声明用的是 `ECardType`（4 类型体系）。这说明代码处于**两套体系并存的混乱状态**——数值逻辑想用 8 机制类型，字段却绑着 4 类型枚举。

---

## 三、技术改动清单（若采用 28 张方案）

### 3.1 枚举体系重构

| 改动项 | 影响范围 | 优先级 |
|--------|---------|--------|
| 替换 `ECardType` 枚举值（4→8+1种） | ECardType.cs、CardData.cs、所有引用 ECardType 的 UI/逻辑代码 | 🔴 高 |
| 新增 `CardType` 枚举引用到 `CardData.CardType` 字段 | CardData.cs、CardExecutor.cs（卡牌效果执行逻辑） | 🔴 高 |
| 清理 CardData.cs 内置的冗余 `CardType` 枚举定义 | CardData.cs | 🟡 中 |

### 3.2 卡牌数据扩充（20 → 28 张）

| 改动项 | 说明 |
|--------|------|
| 新增 8 张卡牌 SO（card_021 ~ card_028） | 需创建对应 ScriptableObject 资产文件 |
| 更新卡组配置（HandManager 或卡池生成逻辑） | 支持 28 张卡牌随机抽取 |
| 稀有度掉率表更新 | N:60% / R:30% / SR:8% / UR:2% 权重不变（已有） |
| 卡牌获取来源表更新 | 需对应数值设计的关卡奖励分配 |

### 3.3 ID 体系统一

| 改动项 | 说明 |
|--------|------|
| 确定唯一卡牌 ID 格式（`card-XXX` 或 `card_XXX`） | 建议统一为一种，建议沿用 `card-XXX`（更清晰） |
| 建立美术 ID ↔ 数值 ID 映射表（如需双轨并存） | 可选，取决于是否保留两套体系的美术资产 |
| 更新所有加载/引用卡牌的代码（按 ID 查表） | CardExecutor、HandManager、CardPanelUI 等 |

### 3.4 UI / 效果系统适配

| 改动项 | 说明 |
|--------|------|
| 卡牌效果 UI（类型图标、能耗显示）适配新枚举 | 4 类型 → 8+ 类型，图标需重新设计 |
| CardExecutor.cs 执行逻辑适配 8 种机制类型 | 每个 ECardType 值对应的效果执行分支 |
| 新卡牌效果范围/数值填充 | card_021 ~ card_028 的 Range、Value、TargetElement |

---

## 四、技术风险

### 🔴 高风险

**1. 美术资产大规模报废**
美术规范（card-001~020）基于 4 类型体系制作，角色形象（攻击型女武士、防御型禅师等）与 Attack/Defense/Transform/Utility 强绑定。若切换到数值设计的 8 机制类型（ClearRow/Bomb/Shuffle 等），**所有卡牌形象需重新绘制**，对应 AI 提示词也需重写。

**2. ECardType 枚举分裂导致运行时错误**
CardData.cs 当前状态是 ECardType（4类型）字段 + CardType（8机制）枚举并存，编译可能警告或运行时效果逻辑与 UI 类型不匹配。重构枚举体系时需全面搜索所有引用点（`grep -r "ECardType" Assets/`），避免遗漏。

**3. 卡牌数量变化影响游戏经济**
从 20 增加到 28 张改变了抽卡概率分布、NPC 卡组结构和玩家卡组构建策略。需要重新计算稀有度掉率、Boss 卡组组合和卡牌获取节奏。

### 🟠 中风险

**4. 卡牌 ID 格式冲突**
两套文档使用不同的 ID 格式（`card-XXX` vs `card_XXX`），若同时存在将导致：
- Resources.Load 按 ID 加载失败
- SaveData 中存储的 cardId 序列化错误
- 美术资源和数值配置无法对应

**5. UI 类型图标需要重设计**
当前 UI 假设 4 种卡牌类型，每种有对应图标。扩展到 8+ 类型后，CardPanelUI / CardUI 的图标渲染逻辑需要改造。

**6. 卡牌效果参数边界**
数值设计（card-numbers-v2.md）的 SR/UR 卡有 5×5 范围（25格）和「HP 减半」等强力效果，需确认现有 `Range`（int）和 `Value`（int）字段是否足够承载，以及 BoardUI 格子清除动画是否能承受 25 格同时消除。

### 🟡 低风险

**7. ScriptableObject 资产重建**
新增 8 张卡牌需要新建 8 个 `.asset` 文件，手动填充字段。建议使用 `CreateInstance<CardData>()` 批量创建脚本辅助。

**8. 文档版本同步**
技术侧改动后需同步更新设计文档，避免「代码 v2」和「文档 v1」不一致的问题再次出现。

---

## 五、核心结论

| 问题 | 结论 |
|------|------|
| **ID 体系** | 完全冲突（格式+数量），需决策统一用哪套 |
| **类型枚举** | 代码现状是 4 类型（ECardType.cs）与美术一致，但数值设计用 8 机制类型，CardData.cs 处于两套体系混用状态 |
| **字段结构** | CardData.cs 有效果参数字段（Range/Value/TargetElement），可承载数值设计的 28 张卡；但需扩充卡牌数量 |
| **采用 28 张方案的技术成本** | 中高——需重构枚举体系、新增 8 张 SO、更新 UI 和效果执行逻辑，最大风险是美术资产需全部重建 |

**建议路径（供大王决策）：**
1. **方案 A（推荐）：** 美术规范向数值设计靠拢——美术重新按 8 机制类型（ClearRow/Bomb 等关键词）生成 28 张卡牌形象，ECardType 枚举全量重构
2. **方案 B：** 数值设计向美术规范靠拢——将数值设计文档的 28 张卡牌映射到现有 4 类型体系中（合并/重命名类型），保留美术资产
3. **方案 C：** 双轨并行——代码层用数值设计的 8 机制类型（`card_XXX`），美术资产另建 `card-XXX` 体系，通过映射表关联，两套 ID 长期并存
