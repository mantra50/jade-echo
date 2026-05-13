# 卡牌颜色规范 · Card Color Specification

> 所有 AI 生成图片必须严格遵循本规范的颜色指令。
> 颜色关键词直接嵌入 `color palette:` 提示词最前端，确保主次分明。

---

## 一、四种类型精确色值

| 类型 | 主色调 | 十六进制 | 中文名 | 副色调 | 十六进制 | 中文名 | 背景色 | 十六进制 |
|------|--------|----------|--------|--------|----------|--------|--------|----------|
| **Attack** | 朱红 | `#E63946` | Crimson Red | 橙黄 | `#FF6B35` | Flame Orange | 深褐黑 | `#1A0505` |
| **Defense** | 藏青 | `#457B9D` | Dark Teal Blue | 银灰 | `#A8DADC` | Pale Steel | 深海蓝黑 | `#0A1520` |
| **Transform** | 紫藤 | `#9B5DE5` | Violet Purple | 粉紫 | `#E0AAFF` | Lavender Mist | 深紫黑 | `#120A1A` |
| **Utility** | 青绿 | `#2A9D8F` | Jade Green | 宝蓝 | `#4169E1` | Royal Blue | 深蓝黑 | `#0A0A1E` |

---

## 二、AI 颜色关键词列表

### Attack · 攻击型（#E63946 朱红）

**主色关键词（按优先级嵌入）：**
```
crimson, scarlet, flame red, blood red, vermillion, alizarin red,
ruby, firebrick, indian red, carmine, cherry red, ember red,
bright carmine, chinese red,rosso corsa, rust red, fire red, lava red
```

**次色/对比色搭配：**
```
orange, flame orange, golden yellow, amber, tangerine,
gold, bright orange, burnt sienna, copper, bronze
```

**背景色建议：**
```
dark brown, deep burgundy, charcoal, near black with red tint,
deep mahogany, dark crimson black, blood red black
```

---

### Defense · 防御型（#457B9D 藏青）

**主色关键词（按优先级嵌入）：**
```
dark teal blue, slate blue, deep navy, prussian blue, steel blue,
indigo, cobalt blue, sapphire, navy, dark cerulean,
blue steel, oxford blue, yale blue, persian green blue,
teal blue, midnight blue, dark blue grey
```

**次色/对比色搭配：**
```
pale steel blue, light cyan, silver, silver grey, pale aqua,
sky blue, powder blue, alice blue, lavender, pearl white
```

**背景色建议：**
```
deep navy black, dark ocean blue, midnight teal, abyssal blue,
dark slate, deep blue grey, near black with blue tint
```

---

### Transform · 变形式（#9B5DE5 紫藤）

**主色关键词（按优先级嵌入）：**
```
violet purple, lavender, amethyst, orchid purple, soft violet,
deep violet, electric purple, vivid purple, bright violet,
royal purple, magenta, iris, mulberry, byzantine purple,
dark orchid, plum, grape, mulberry purple
```

**次色/对比色搭配：**
```
lavender mist, pale lilac, soft pink, rose quartz,
silver mist, pearl, soft white, pale rose,
light violet, soft orchid, blush pink
```

**背景色建议：**
```
deep purple black, dark violet, midnight purple,
near black with violet tint, dark amethyst, deep grape
```

---

### Utility · 功能型（#2A9D8F 青绿）

**主色关键词（按优先级嵌入）：**
```
jade green, teal green, emerald, jade, dark jade,
sea green, pine green, forest jade, malachite green,
viridian, hunter green, deep jade, celadon jade,
jadeite, nephrite jade, moss green, deep teal
```

**次色/对比色搭配：**
```
royal blue, sky blue, cyan, aquamarine, light teal,
gold, pale jade, seafoam, mint green, light jade
```

**背景色建议：**
```
deep blue black, dark teal, deep sea,
dark forest green, near black with green tint,
deep jade black, midnight teal green
```

---

## 三、标准负面提示词（Negative Prompt）

所有 AI 生成图片统一在提示词末尾附加：

```
ugly, blurry, low quality, watermark, text overlay, signature,
deformed, disfigured, extra limbs, missing limbs, malformed,
bad anatomy, incorrect anatomy, asymmetric face, cloned face,
poorly drawn hands, extra fingers, missing fingers, fused fingers,
bad proportions, unnatural pose, stiff pose, lifeless,
low resolution, pixelated, jpeg artifacts, compression artifacts,
oversaturated, undersaturated, washed out colors, muddy colors,
cropped, out of frame, cut off, frame edge,
noise, grain, dust, scratches, dust particles,
amateur, amateurish, unprofessional, clipart, stock image,
3d render, realistic photograph, selfie, self-portrait,
anime, manga style, western cartoon, comic book style
```

> **注意：** 如需特定风格（如"古风水墨"或"卡通写实"），请在负面提示词前保留风格限定词。
> 卡牌美术统一使用 **cartoon realism**（卡通写实）风格。

---

## 四、配色公式速查

### 格式模板
```
color palette: #[主色十六进制] dominant, [主色关键词], [次色关键词], [场景描述]
```

### 示例（Attack）
```
color palette: #E63946 dominant, crimson and flame orange, fierce warrior...
```

### 示例（Defense）
```
color palette: #457B9D dominant, dark teal blue and pale steel, armored guardian...
```

### 示例（Transform）
```
color palette: #9B5DE5 dominant, violet purple and soft lavender, mystical mage...
```

### 示例（Utility）
```
color palette: #2A9D8F dominant, jade green and royal blue, celestial sage...
```

---

*本文档由 Artoria 制定，用于统一卡牌 AI 美术风格。所有图片资源生成必须参照此规范。*
