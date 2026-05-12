# CardMatch - Unity项目结构文档
**版本：** v1.0  
**作者：** Orion（技术总监）  
**日期：** 2026-06-01  
**项目：** 卡牌+消除融合游戏  
**平台：** Steam PC（Unity 2022.3 LTS）

---

## 1. 目录结构

```
cardmatch/
├── Assets/
│   ├── _Game/                          # 🔥 可热更新逻辑（HybridCLR范围）
│   │   ├── EliminationCore/             # 消除核心子系统
│   │   │   ├── Board/                  # 棋盘数据结构
│   │   │   ├── Match/                  # 匹配检测引擎
│   │   │   ├── StateMachine/           # 消除状态机
│   │   │   ├── Gravity/                # 重力下落
│   │   │   └── Combo/                  # 连锁计数
│   │   │
│   │   ├── CardSystem/                 # 卡牌子系统
│   │   │   ├── Card/                   # 卡牌实体
│   │   │   ├── Hand/                   # 手牌管理
│   │   │   ├── Deck/                   # 牌组管理
│   │   │   └── CardData/               # 卡牌配置数据
│   │   │
│   │   ├── BattleFlow/                 # 战斗流程子系统
│   │   │   ├── Controller/             # 战斗主控制器
│   │   │   ├── Phase/                  # 战斗阶段管理
│   │   │   ├── Action/                 # 行动解析与执行
│   │   │   └── Energy/                 # 能量系统
│   │   │
│   │   ├── LevelManager/               # 关卡管理子系统
│   │   │   ├── Level/                  # 单关逻辑
│   │   │   ├── Chapter/                # 章节逻辑
│   │   │   ├── Progress/               # 存档进度
│   │   │   └── Event/                  # 事件系统
│   │   │
│   │   ├── UIManager/                  # UI管理子系统
│   │   │   ├── Battle/                 # 战斗界面
│   │   │   ├── Menu/                   # 菜单界面
│   │   │   ├── Popup/                  # 弹窗
│   │   │   └── Common/                 # 通用UI组件
│   │   │
│   │   ├── SaveSystem/                 # 存档子系统
│   │   │   ├── Data/                   # 存档数据结构
│   │   │   ├── Serializer/             # 序列化（JSON）
│   │   │   └── Version/                # 版本迁移
│   │   │
│   │   └── AudioManager/               # 音频子系统
│   │       ├── BGM/                    # 背景音乐
│   │       ├── SFX/                    # 音效
│   │       ├── Voice/                  # 语音
│   │       └── Config/                 # 音频配置
│   │
│   ├── Art/                            # 美术资产（YooAsset ResourceGroup）
│   │   ├── Cards/                      # 卡牌立绘
│   │   ├── UI/                         # UI元素
│   │   ├── Effects/                    # 特效（粒子/序列帧）
│   │   ├── Backgrounds/                # 场景背景
│   │   ├── Characters/                 # 角色立绘
│   │   └── Shared/                     # 共享资源
│   │
│   ├── Audio/                          # 音频文件
│   │   ├── BGM/
│   │   └── SFX/
│   │
│   ├── Configs/                        # 配置表（Excel→JSON）
│   │
│   ├── Scenes/                         # Unity场景
│   │
│   ├── ThirdParty/                     # 第三方插件
│   │   ├── HybridCLR/                  # HybridCLR数据
│   │   ├── YooAsset/                   # YooAsset数据
│   │   └── Steamworks/                # Steamworks.NET
│   │
│   └── Bootstrap/                      # 🔒 AOT入口（不可热更）
│       ├── Bootstrap.cs               # 程序入口
│       ├── AOTBinding/                 # AOT泛型绑定
│       └── Platform/                   # 平台适配层
│
├── ProjectSettings/                    # Unity项目配置
├── Packages/                           # npm包（UPM）
└── tech-output/                        # 技术输出物（本目录）
```

---

## 2. 七大子系统职责

### 子系统 1：EliminationCore（消除核心）
**职责：** 8×8棋盘的Match3玩法核心逻辑  
**模块：**
| 模块 | 职责 |
|------|------|
| `Board/` | 棋盘数据结构、格子读写、元素交换 |
| `Match/` | 匹配检测（水平+垂直扫描，3连起消，L/T形特殊匹配） |
| `StateMachine/` | 消除状态机（IDLE→SELECTED→SWAPPING→ELIMINATING→FALLING→CASCADE→IDLE） |
| `Gravity/` | 重力下落算法，消除后填位 |
| `Combo/` | 连锁计数、Combo倍数加成 |

**关键API：**  
`Board.Swap(a,b)`、`MatchDetector.Scan()`、`Gravity.Apply()`、`ComboCounter.Increment()`

---

### 子系统 2：CardSystem（卡牌系统）
**职责：** 30张卡牌的创建、消耗、手牌管理  
**模块：**
| 模块 | 职责 |
|------|------|
| `Card/` | 卡牌实体（立绘+数据+状态） |
| `Hand/` | 手牌容器（最多N张，出牌逻辑） |
| `Deck/` | 牌组（抽牌、洗牌） |
| `CardData/` | 卡牌配置数据（攻击力/类型/效果） |

**关键API：**  
`Hand.Play(card)`、`Deck.Draw()`、`CardData.GetById()`

---

### 子系统 3：BattleFlow（战斗流程）
**职责：** 回合制战斗流程编排、消除与卡牌联动  
**模块：**
| 模块 | 职责 |
|------|------|
| `Controller/` | 战斗主控制器，协调各子系统 |
| `Phase/` | 战斗阶段管理（玩家回合/Boss回合/消除阶段/卡牌阶段） |
| `Action/` | 行动解析与执行（使用卡牌、触发效果） |
| `Energy/` | 能量系统（消除产出能量，卡牌消耗能量） |

**关键API：**  
`BattleController.StartBattle()`、`PhaseManager.NextPhase()`、`EnergySystem.Consume()`

---

### 子系统 4：LevelManager（关卡管理）
**职责：** 章节/关卡加载、进度追踪、Boss机制  
**模块：**
| 模块 | 职责 |
|------|------|
| `Level/` | 单关关卡逻辑（敌人、波次、胜利条件） |
| `Chapter/` | 章节逻辑（10关+中间Boss+章末Boss） |
| `Progress/` | 玩家进度存档 |
| `Event/` | 随机事件、战斗事件 |

**关键API：**  
`LevelLoader.Load(levelId)`、`ChapterManager.Complete()`、`EventSystem.Trigger()`

---

### 子系统 5：UIManager（UI管理）
**职责：** 所有界面显示与交互  
**模块：**
| 模块 | 职责 |
|------|------|
| `Battle/` | 战斗界面（棋盘+手牌+能量条+HP） |
| `Menu/` | 主菜单、暂停、设置 |
| `Popup/` | 弹窗（获得卡牌、Boss受伤、升级） |
| `Common/` | 通用组件（按钮、进度条、图标） |

**关键API：**  
`UIManager.ShowBattle()`、`PopupSystem.Show(card)`、`Common.SetProgress()`

---

### 子系统 6：SaveSystem（存档系统）
**职责：** JSON本地存档（Steam云存档支持）  
**模块：**
| 模块 | 职责 |
|------|------|
| `Data/` | 存档数据结构 |
| `Serializer/` | JSON序列化/反序列化 |
| `Version/` | 版本迁移（v1.0→v1.1数据迁移） |

**关键API：**  
`SaveSystem.Save()`、`SaveSystem.Load()`、`VersionMigrator.Migrate()`

---

### 子系统 7：AudioManager（音频管理）
**职责：** BGM、SFX、语音统一播放管理  
**模块：**
| 模块 | 职责 |
|------|------|
| `BGM/` | 背景音乐（主界面/战斗/Boss） |
| `SFX/` | 音效（消除/出牌/攻击/受击） |
| `Voice/` | 角色语音 |
| `Config/` | 音量配置、音频分组 |

**关键API：**  
`AudioManager.PlayBGM(name)`、`AudioManager.PlaySFX(sfxId)`、`AudioManager.SetVolume()`

---

## 3. 热更新边界

```
┌─────────────────────────────────────────────────────┐
│  Bootstrap/（AOT）    不可热更，IL2CPP编译时固定      │
│  ├── Bootstrap.cs                                    │
│  └── Platform/（Steam/Win平台适配）                   │
├─────────────────────────────────────────────────────┤
│  _Game/（全部可热更）   HybridCLR运行时加载           │
│  ├── EliminationCore/                                │
│  ├── CardSystem/                                     │
│  ├── BattleFlow/                                     │
│  ├── LevelManager/                                   │
│  ├── UIManager/                                      │
│  ├── SaveSystem/                                     │
│  └── AudioManager/                                   │
└─────────────────────────────────────────────────────┘
```

---

## 4. YooAsset ResourceGroup 分组

| 分组 | 路径 | 加载策略 | 示例 |
|------|------|---------|------|
| `common` | Art/Shared/ | **常驻** | 共享图集、通用特效 |
| `cards` | Art/Cards/ | **流式** | 卡牌立绘（按需加载） |
| `scenes` | Scenes/ | **场景** | 各关卡Scene包 |
| `audio` | Audio/ | **流式** | BGM/SFX按场景加载 |
| `ui` | Art/UI/ | **常驻** | UI贴图、字体 |

---

## 5. 技术栈汇总

| 组件 | 版本 | 用途 |
|------|------|------|
| Unity | 2022.3 LTS | 引擎 |
| HybridCLR | 3.4.x | C#热更新 |
| YooAsset | 1.5.x | 资源打包与加载 |
| Steamworks.NET | 1.9.x | Steam成就/云存档 |
| C# | 10.0 | 语言特性（record/pattern） |

---

**文档状态：** ✅ 完成  
**下次更新：** M1阶段根据PoC验证结果修正