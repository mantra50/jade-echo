# CardMatch - Chapter 1 Demo

**项目：** 卡牌+消除融合游戏  
**版本：** Chapter1 Demo v0.1  
**作者：** Orion（技术总监）  
**日期：** 2026-05-12  
**平台：** Unity 2022.3 LTS

---

## 🎮 如何编译运行

### 环境要求

- Unity 2022.3 LTS 或更高版本
- C# 10.0

### 编译步骤

1. 打开 Unity Hub
2. 点击 **Open** → 选择 `cardmatch/` 文件夹
3. 等待 Unity 导入所有资源
4. 在 Project 窗口中找到 `Assets/_Game/GameLauncher.cs`
5. 点击 **Play** 运行游戏

### 键盘/鼠标操作

| 操作 | 功能 |
|------|------|
| 鼠标点击格子 | 选中棋子 |
| 再次点击相邻格子 | 交换棋子并触发消除 |
| 点击手牌 | 使用卡牌 |
| 点击 End Turn | 结束当前回合 |

---

## 📁 项目结构

```
cardmatch/
├── Assets/
│   ├── _Game/                          # 🔥 可热更新逻辑（HybridCLR范围）
│   │   ├── GameLauncher.cs            # 🎮 游戏入口（系统初始化）
│   │   │
│   │   ├── Gameplay/                   # 游戏核心玩法
│   │   │   ├── BattleFlowManager.cs    # 战斗流程主控制器
│   │   │   ├── BidirectionalLinker.cs # 消除↔卡牌双向联动
│   │   │   ├── ComboManager.cs         # Combo连锁计数
│   │   │   ├── LevelConfig.cs          # Chapter1 第1关敌人配置
│   │   │   │
│   │   │   ├── CardSystem/             # 卡牌子系统
│   │   │   │   ├── CardData.cs         # 卡牌数据
│   │   │   │   ├── CardExecutor.cs     # 卡牌效果执行器
│   │   │   │   └── HandManager.cs      # 手牌管理
│   │   │   │
│   │   │   ├── MatchSystem/            # 消除子系统
│   │   │   │   ├── GridManager.cs      # 6×6 棋盘管理
│   │   │   │   ├── MatchDetector.cs    # 匹配检测引擎
│   │   │   │   ├── GravitySystem.cs    # 重力下落
│   │   │   │   └── CascadeController.cs # 连锁控制器
│   │   │   │
│   │   │   └── Shared/                 # 共享数据
│   │   │       └── Piece.cs            # 棋子数据模型
│   │   │
│   │   ├── UI/                         # UI层
│   │   │   ├── BoardUI.cs              # 棋盘UI
│   │   │   ├── CardPanelUI.cs          # 手牌UI
│   │   │   ├── HUDUI.cs                # 战斗HUD
│   │   │   ├── ShopUI.cs               # 商店UI
│   │   │   └── CanvasLayout.cs         # 画布布局
│   │   │
│   │   ├── BattleFlow/                 # 战斗流程（预留）
│   │   ├── CardSystem/                 # 卡牌系统（预留）
│   │   ├── EliminationCore/            # 消除核心（预留）
│   │   ├── LevelManager/               # 关卡管理（预留）
│   │   ├── SaveSystem/                 # 存档系统（预留）
│   │   ├── AudioManager/               # 音频管理（预留）
│   │   ├── Art/                        # 美术资产
│   │   └── UIManager/                  # UI管理（预留）
│   │
│   ├── Bootstrap/                      # 🔒 AOT入口（不可热更）
│   ├── Configs/                        # 配置表
│   ├── Scenes/                         # Unity场景
│   └── ThirdParty/                     # 第三方插件
│
├── tech-output/                        # 技术文档
│   ├── project-structure.md            # 项目结构文档
│   └── elimination-poc.md              # 消除核心PoC文档
│
├── Packages/                           # UPM包
├── ProjectSettings/                    # Unity项目配置
└── README.md                          # 本文档
```

---

## 🧩 模块职责说明

### 核心模块

| 模块 | 职责 | 关键类 |
|------|------|--------|
| **GameLauncher** | 游戏入口，系统初始化 | `GameLauncher.cs` |
| **BattleFlowManager** | 战斗流程控制（回合循环） | `BattleFlowManager.cs` |
| **BidirectionalLinker** | 消除↔卡牌双向联动 | `BidirectionalLinker.cs` |
| **ComboManager** | Combo连锁计数与爆发 | `ComboManager.cs` |
| **LevelConfig** | Chapter1 第1关敌人配置 | `LevelConfig.cs` |

### 消除子系统（MatchSystem）

| 类 | 职责 |
|----|------|
| `GridManager` | 6×6棋盘数据管理，棋子读写 |
| `MatchDetector` | 匹配检测（水平/垂直扫描，3连起消） |
| `GravitySystem` | 重力下落算法 |
| `CascadeController` | 连锁控制器，驱动整个消除循环 |

### 卡牌子系统（CardSystem）

| 类 | 职责 |
|----|------|
| `CardData` | 卡牌属性数据（攻击力/能量消耗/效果） |
| `HandManager` | 手牌容器，抽牌/出牌逻辑 |
| `CardExecutor` | 卡牌效果执行器 |

### UI层

| 类 | 职责 |
|----|------|
| `BoardUI` | 棋盘显示与点击事件 |
| `CardPanelUI` | 手牌面板 |
| `HUDUI` | 战斗HUD（HP/Energy/Combo） |
| `ShopUI` | 商店界面 |
| `CanvasLayout` | 画布布局管理 |

---

## 🎯 Chapter1 第1关 — 暗影猎手

### 敌人信息

| 属性 | 值 |
|------|-----|
| 名称 | 暗影猎手 |
| HP | 15 |
| 攻击 | 2 |
| 波次 | 1 |
| 每消除暗影元素×3 | 造成1点伤害 |

### 伤害配置

- 棋盘：6×6（36格）
- 暗影元素比例：20%（约7个紫色TypeF格子）
- 每消除3个暗影元素对敌人造成1点伤害
- 胜利：消灭敌人（15HP = 45次消除）

### 战斗流程

```
玩家回合
├── 消除阶段（Match3操作）→ 对敌人造成伤害
├── 卡牌阶段（使用手牌）→ 执行特殊效果
└── 结束回合 → 敌人攻击 → 进入下一回合
```

---

## 🔥 热更新边界

```
┌─────────────────────────────────────────────┐
│  Bootstrap/（AOT）   不可热更，IL2CPP编译时固定 │
│  ├── Bootstrap.cs                               │
│  └── Platform/（Steam/Win平台适配）              │
├─────────────────────────────────────────────┤
│  _Game/（全部可热更）  HybridCLR运行时加载        │
│  ├── GameLauncher.cs                           │
│  ├── Gameplay/                                 │
│  ├── UI/                                       │
│  └── CardSystem/                               │
└─────────────────────────────────────────────┘
```

---

## 📋 M1 任务状态

| 任务 | 状态 | 负责人 |
|------|------|--------|
| M1-1 Match3消除系统 | ✅ done | Orion |
| M1-2 卡牌系统 MVP | ✅ done | Orion |
| M1-3 消除×卡牌双向联动 | ✅ done | Orion |
| M1-4 战斗系统整合 | 🔄 进行中 | - |
| M1-5 UI对接 | 🔄 进行中 | Artoria |
| M1-6 UI/UX原型设计 | ✅ done | Artoria |
| M1-7 音效与特效 | 📋 待开始 | - |
| **M1-8 Chapter1 Demo** | ✅ **done** | **Orion** |

---

## 🚀 待办事项

- [ ] GridManager 6×6 → 8×8 扩展
- [ ] UI层完整对接（BoardUI/HUDUI/CardPanelUI）
- [ ] 音效与特效集成
- [ ] YooAsset 资源打包配置
- [ ] HybridCLR 热更新配置
- [ ] Chapter1 完整10关敌人配置

---

**文档状态：** ✅ M1-8 完成  
**下次更新：** UI对接完成后更新运行说明
