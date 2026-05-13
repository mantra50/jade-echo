# CardMatch - 项目目录结构

> 目录重组：程序 / 美术 / 文档 三类完全分离

## 目录概览

```
cardmatch/
├── Assets/
│   ├── _Game/            ← 程序代码（热更新逻辑）
│   ├── Art/               ← Unity 静态美术资源
│   ├── Audio/             ← 音频文件（BGM/SFX）
│   └── Configs/           ← Unity 配置表
│
├── ArtDocuments/           ← 美术文档（AI提示词/色值规范/特效规格）
├── DesignDocuments/        ← 游戏设计文档（GDD/卡牌数值/敌人配置）
└── TechDocuments/         ← 技术文档（接口/架构/项目结构）
```

## Assets/

### `_Game/` — 程序代码（热更新）

| 目录 | 说明 |
|---|---|
| `UI/` | UI 逻辑代码 |
| `Gameplay/` | 游戏玩法逻辑 |
| `AudioManager/` | 音频管理模块 |
| `CardSystem/` | 卡牌数据与执行 |
| `GameLauncher.cs` | 游戏入口 |

### `Art/` — Unity 静态美术资源

| 目录 | 说明 |
|---|---|
| `Backgrounds/` | 场景背景图 |
| `Cards/` | 卡牌美术资源 |
| `Characters/` | 角色立绘 |
| `Effects/` | 特效资源 |
| `Shared/` | 通用水墨/字体等共享资源 |

### `Audio/` — 音频文件

| 目录 | 说明 |
|---|---|
| `BGM/` | 背景音乐 |
| `SFX/` | 音效文件 |

### `Configs/` — Unity 配置表

存放关卡配置、敌人注册表等 JSON 配置。

---

_最后更新：2026-05-13_