# Batch1 卡牌立绘 — AI 批量生产工作流

## 概述

本目录存放 AI 批量生成的卡牌立绘资源。团队使用 Midjourney / Stable Diffusion 配合以下提示词模板，批量产出符合风格指南的卡牌原画。

---

## 统一风格指南

在开始生成前，所有提示词需包含以下控制词：

- **画幅比例：** `9:16`（竖版卡牌）
- **风格：** `digital art, flat illustration, clean lines, vibrant colors`
- **背景：** `transparent background, no background`（方便 Unity 切割）
- **输出格式：** 提交时选 `V5` / `SDXL --beta --upbeta` 输出高分辨率 PNG
- **卡牌边框：** 生成后用 Figma/Photoshop 套边框模板

---

## 提示词模板（Midjourney）

### 基础卡牌（普通/精英）

```
[卡牌类型], [角色种族/职业], [姿态描述], standing, front view, flat illustration style, transparent background, high detail, vibrant color palette, card game art, --ar 9:16 --s 200 --v 6.1
```

**示例：**
```
Elven Archer, female, standing pose with bow raised, front view, flat illustration style, transparent background, high detail, vibrant color palette, card game art --ar 9:16 --s 200 --v 6.1
```

### 技能卡（法术/道具）

```
[元素属性], magical aura, glowing effect, mystical energy, centered composition, transparent background, flat illustration, vibrant colors, card game art --ar 9:16 --s 250 --v 6.1
```

### Boss/传说级卡牌

```
[角色名], [种族], dramatic pose, ornate armor, glowing eyes, dark background, cinematic lighting, highly detailed, card game art, ultra quality --ar 9:16 --s 400 --v 6.1
```

---

## Stable Diffusion (ComfyUI) 工作流

### 批量生成节点流程

1. **Load Checkpoint:** realisticVision_v60 / animePastelXX
2. **KSampler:** Steps 30, CFG 7.5, DPM++ 2M Karras
3. **ControlNet (Tile):** 保持角色一致性
4. **放大流程:** 512×928 → 4x UltraSharp 放大到 2048×3712

### A1111 批量脚本示例

```python
# 批量生成脚本 (A1111 / SD Web UI)
prompts = [
    "Elven Archer, female, standing pose with bow raised...",
    "Fire Mage, male, casting spell with both hands...",
    # ...更多
]

for i, prompt in enumerate(prompts):
    print(f"Generating card {i+1}/50...")
    # 调用 API 或直接操作 UI
```

---

## 文件命名规范

```
{编号}_{角色英文名}_{稀有度}_{日期}.png

示例：
001_Elven_Archer_Common_20260512.png
002_Fire_Mage_Rare_20260512.png
003_Dragon_Lord_Legendary_20260512.png
```

**稀有度后缀：**
- `Common` — 普通（白）
- `Rare` — 精英（蓝）
- `Epic` — 史诗（紫）
- `Legendary` — 传说（金）

---

## 后期处理流程

1. **抠图:** 使用 remove.bg 或 PS 钢笔工具，导出透明 PNG
2. **套边框:** 用 Unity 预设的 CardFrame 模板（Figma/Sketch 批量导出）
3. **检查分辨率:** 确保长边 ≥ 2048px，300 DPI
4. **导入 Unity:** 放到 `Assets/_Game/Art/Cards/Batch1/` 目录

---

## 当前 Batch1 清单

| 文件名 | 状态 | 备注 |
|--------|------|------|
| 001_Elven_Archer_Common.png | 待生成 | 第一批优先 |
| 002_Fire_Mage_Rare.png | 待生成 | |
| 003_Dragon_Lord_Legendary.png | 待生成 | |

> ⚠️ 以上清单为占位，待实际生成后更新状态。

---

## 注意事项

- 所有立绘必须与卡牌数据（CardData.cs）中的 ID 对应
- 背景层与角色层分开导出，方便 Unity 叠加特效
- 避免过度依赖 AI 签名/水印，生成后需移除