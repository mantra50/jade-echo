# CardMatch — Alpha 版本说明

> CardMatch 是一款融合 Match3 消除与卡牌战斗的游戏。
> 本文档描述 Alpha 版本的运行方式和已知问题。

---

## 如何编译运行

### 环境要求
- **Unity 2021.3 LTS** 或更高版本
- C# 9.0+
- 无需额外 SDK / API Key

### 编译步骤
1. 打开 Unity Hub
2. 点击 `Open` → 选择 `/root/.openclaw/workspace/cardmatch/` 文件夹
3. 等待 Unity 索引完成（首次可能需要 2-3 分钟）
4. 在 Project 窗口打开 `Assets/_Game/GameLauncher.cs` 所在场景
5. 点击 `Play` 按钮运行

### 运行游戏（无需UI，Debug模式可直连）
- `GameLauncher.cs` 已挂载在场景中
- 在 Unity Inspector 中点击 `GameLauncher` 的 **Context Menu → Debug: Run Full Battle Simulation** 即可模拟一局完整战斗（输出日志）

---

## 操作说明

### 游戏流程
```
主菜单 → 开始 Chapter1-1 → 战斗场景 → 胜利/失败弹窗 → 返回主菜单
```

### 战斗操作
| 操作 | 输入 |
|------|------|
| 选中棋子 | 鼠标点击棋盘格子 |
| 交换棋子 | 再次点击相邻格子（相邻判定：曼哈顿距离=1）|
| 出牌 | 点击手牌区的卡牌（Debug模式下自动寻靶）|
| 结束回合 | 点击 HUD 的 "End Turn" 按钮 |

### 伤害链路（核心）
1. **玩家交换** → 棋子交换到相邻格
2. **检测匹配** → 若有3+连续同色 → 进入消除连锁
3. **消除棋子** → 每消除 `_piecesPerDamage=3` 个，对 Boss 造成 **1 点伤害**
4. **Boss 回合** → Boss 普攻，对玩家造成 `attack=2` 点伤害
5. **回合结束** → 抽1张卡，恢复能量，进入下一回合
6. **Boss HP ≤ 0** → 胜利 / **玩家 HP ≤ 0** → 失败

---

## Chapter1-1 关卡配置

- **敌人：** 暗影猎手（Shadow Hunter）
- **HP：** 15
- **攻击：** 2 / 回合
- **护盾：** 0
- **伤害规则：** 每消除 3 个棋子 → 1 点伤害
- **波次：** 1 波（单波次）
- **最大回合：** 30（超时不扣血）

---

## 已实现模块

### 核心
- ✅ Match3 检测器（3连/4连/L形/T形）
- ✅ 8×8 棋盘 GridManager
- ✅ 重力下落 GravitySystem
- ✅ 连锁.CascadeController（消除→下落→再检测）
- ✅ 双向联动 BidirectionalLinker（消除→卡牌触发）

### 卡牌
- ✅ HandManager（抽卡/手牌管理/能量消耗）
- ✅ CardExecutor（ClearRow/ClearCol/ClearArea/Bomb/Shuffle/EnergyBoost/Heal）
- ✅ CardData ScriptableObject

### 战斗
- ✅ BattleFlowManager（回合循环：消除→卡牌→结算）
- ✅ BossBase（两阶段切换/普攻/特殊攻击队列/护盾）
- ✅ ComboManager（火花充能/终极技能触发）

### UI（骨架/事件绑定）
- ✅ BattleUIConnector（BattleFlowManager 事件 → UI 响应）
- ✅ BossHPUI（血条动画/抖动/相位切换）
- ✅ DamagePopup（伤害数字弹出）
- ✅ HUDUI（能量条/回合/EndTurn按钮）
- ✅ BoardUI（棋盘格子/消除特效）
- ✅ SettlementUI（胜负弹窗/统计按钮）

---

## 已知问题（Alpha）

| # | 问题 | 说明 | 优先级 |
|---|------|------|--------|
| 1 | 无 Unity 场景文件 | 目前没有 `.unity` 场景，需要手动创建一个包含所有 MonoBehaviour 组件的场景 | 高 |
| 2 | BoardUI/CanvasLayout 未完成 | 格子显示/背景等美术资源未配置 | 高 |
| 3 | CardPanelUI 未完成 | 手牌区 UI 面板未完整实现 | 中 |
| 4 | 无真实存档系统 | SimpleSaveManager 是占位符，SlotUI 对接后未测试 | 中 |
| 5 | Boss 反射创建 | Chapter1-1 用反射设置字段，非生产级方案 | 中 |
| 6 | HandManager 使用随机测试卡 | 未对接 DeckManager，正式版需替换卡池 | 低 |
| 7 | 无音效/AudioSource | 消除音效/BGM/攻击音效均未实现 | 低 |
| 8 | 无 Chapter1-2~10 关卡 UI 入口 | 只有 Chapter1-1 硬编码，其他关卡需关卡选择场景 | 低 |

---

## 文件结构

```
Assets/_Game/
├── GameLauncher.cs          ← 游戏入口/主菜单/战斗初始化
├── Gameplay/
│   ├── Shared/Piece.cs      ← 棋子数据结构
│   ├── LevelConfig.cs       ← Chapter1-1 关卡配置（ScriptableObject）
│   ├── LevelConfigs/        ← Chapter1-2~10、Chapter2-11~15 关卡配置
│   ├── MatchSystem/         ← GridManager / MatchDetector / GravitySystem / CascadeController
│   ├── CardSystem/          ← CardData / HandManager / CardExecutor
│   ├── Boss/                ← BossBase / BossGuardian / BossMirror / BossHourglass / BossSpider
│   ├── BattleFlowManager.cs ← 回合循环核心
│   ├── BidirectionalLinker.cs ← 消除↔卡牌双向联动
│   └── ComboManager.cs     ← Combo计数/火花充能
└── UI/
    ├── BattleUIConnector.cs ← 事件总线桥接
    ├── BoardUI.cs           ← 棋盘UI（格子/特效）
    ├── BossHPUI.cs          ← Boss血条
    ├── HUDUI.cs             ← HUD（能量/回合/按钮）
    ├── SettlementUI.cs      ← 胜负弹窗
    ├── DamagePopup.cs       ← 伤害数字
    └── CardPanelUI.cs       ← 手牌区UI
```

---

_最后更新：2026-05-13 by Orion（技术总监）_