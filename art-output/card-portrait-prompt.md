# 卡牌立绘 AI 生成提示词

> **作者：** Artoria（美术总监）
> **版本：** v0.1
> **日期：** 2026-05-13
> **用途：** 使用 AI 生图工具（如 Stable Diffusion、Midjourney）批量生成卡牌立绘
> **参考风格：** `art-output/ui-integration.md` 东方古风卡通写实美术规范

---

## 一、基础模板

每张卡牌立绘均为 **半身像 + 卡牌边框**，竖版 2:3 比例（如 512×768 或 1024×1536）。

### 通用正向提示词结构

```
[角色描述], [服饰细节], [东方古风], [卡通写实渲染],
semi-realistic anime style, detailed cloth texture,
golden border frame, card art composition,
cinematic lighting, high detail, 8K
```

### 通用负向提示词

```
low quality, blurry, watermark, text, username,
deformed hands, bad anatomy, extra limbs, ugly,
oversaturated, anime screenshot, pixel art
```

---

## 二、按卡牌类型分类

### 2.1 攻击类卡牌（Attack）— 颜色主题：#E63946 朱红

**代表卡牌：横斩、元素爆弹、星爆**

```
正向：
A graceful Chinese warrior maiden wielding a glowing jade sword,
her long black hair whipping in the wind, silver ornamental armor
with red silk sash, determined expression, eastern fantasy style,
semi-realistic anime, card portrait, upper body shot,
dramatic backlighting, cherry blossom petals in the air,
golden mandarin duck motifs on her armor, golden card border frame

负向：
anime screenshot, low quality, deformed, bad anatomy
```

**变体 — 范围攻击（星爆）：**
```
A powerful Chinese sorceress with flowing violet robes,
hands raised summoning a burst of stellar energy,
star-shaped magical array behind her, glowing purple runes,
fierce battle pose, eastern fantasy, semi-realistic anime,
card portrait, upper body, golden card border frame,
dramatic magical illumination, cherry blossom petals
```

**变体 — 元素爆弹：**
```
A fierce Chinese martial artist in crimson battle gear,
palm thrusts forward releasing a concentrated orb of elemental fire,
explosive energy swirling around her hand, flowing red ribbons,
angry battle-ready expression, eastern fantasy style,
semi-realistic anime, card portrait, upper body,
golden card border frame, dynamic energy effects
```

---

### 2.2 防御类卡牌（Defense）— 颜色主题：#457B9D 藏青

**代表卡牌：护盾壁垒、回血**

```
正向：
A serene Chinese celestial guardian in flowing sapphire-blue
court robes, one hand holding an ancient jade shield,
mystical light swirling around her protective stance,
calm wise expression, eastern fantasy style,
semi-realistic anime, card portrait, upper body shot,
pearl and moon motifs on robes, soft blue magical aura,
golden card border frame, gentle divine lighting

负向：
anime screenshot, low quality, deformed, bad anatomy
```

**变体 — 回血（Heal）：**
```
A kind Chinese medicine goddess in pale blue healers robes,
hands cupped holding a glowing green life essence orb,
ethereal peach blossom petals floating around her,
compassionate gentle expression, eastern fantasy style,
semi-realistic anime, card portrait, upper body,
golden card border frame, warm soft lighting, spiritual energy
```

---

### 2.3 转化类卡牌（Transform）— 颜色主题：#9B5DE5 紫藤

**代表卡牌：镜花水月、Swap、转化**

```
正向：
A mysterious Chinese mirror spirit maiden in flowing
iridescent lavender qipao, holding an ancient bronze
mirrored pendant, enigmatic knowing smile, spirit-like
ethereal presence, eastern fantasy style,
semi-realistic anime, card portrait, upper body shot,
purple lotus patterns, shimmering reflective surface,
golden card border frame, mystical arcane atmosphere

负向：
anime screenshot, low quality, deformed, bad anatomy
```

**变体 — 蜃楼幻象（Spider Boss 幻象）：**
```
A ghostly Chinese illusionist in translucent silver-white
robes that shift and shimmer like mirages, hands weaving
visible threads of illusion magic, mysterious alluring
expression, eastern fantasy style, semi-realistic anime,
card portrait, upper body, golden card border frame,
prismatic light effects, butterfly wing patterns in magic
```

---

### 2.4 功能类卡牌（Utility）— 颜色主题：#2A9D8F 青绿

**代表卡牌：能量灌注、重排、抽卡**

```
正向：
An elegant Chinese wind spirit maiden in flowing teal-green
robes with cloud and wave embroidery, one hand scattering
golden energy particles, joyful confident expression,
eastern fantasy style, semi-realistic anime,
card portrait, upper body shot, phoenix feather ornaments,
golden card border frame, vibrant energy particles,
warm golden and teal lighting

负向：
anime screenshot, low quality, deformed, bad anatomy
```

**变体 — 能量灌注：**
```
A radiant Chinese energy master in golden-amber robes,
body surrounded by swirling orbs of pure golden qi energy,
both palms facing upward summoning vast power,
exalted powerful expression, eastern fantasy style,
semi-realistic anime, card portrait, upper body,
golden card border frame, golden light rays, energy vortex
```

**变体 — 重排（Shuffle）：**
```
A whimsical Chinese time spirit in gradient teal-to-gold
robes with hourglass patterns, holding a glowing ancient
hourglass, playful mischievous expression, eastern fantasy style,
semi-realistic anime, card portrait, upper body,
golden card border frame, floating clock gears in background,
sparkling magical particles
```

---

## 三、Boss 专属卡牌立绘

### 3.1 BossGuardian 守护者 — 冰与石

```
A mighty Chinese stone guardian deity in massive geometric
jade armor with ice crystal inclusions, one hand gripping
a glowing ice-bound sword, fierce protective expression,
eastern fantasy style, semi-realistic anime, card portrait,
upper body, golden card border frame, frost aura,
ice particle effects, cracked stone texture on armor
```

### 3.2 BossMirror 镜中花 — 镜像与幻

```
A hauntingly beautiful Chinese mirror goddess with
split identities visible in a shattered mirror mask,
one side serene white, one side corrupted dark purple,
flowing silver-to-black robes, eastern fantasy style,
semi-realistic anime, card portrait, upper body,
golden card border frame, mirror fragment effects,
doppelganger silhouettes in background
```

### 3.3 BossHourglass 沙漏 — 时间

```
An ancient Chinese time keeper in robes made of
constantly shifting golden sand, holding a massive
ornate hourglass that bleeds golden time particles,
time-worn wise face with glowing golden eyes,
eastern fantasy style, semi-realistic anime,
card portrait, upper body, golden card border frame,
temporal distortion effects, floating clock fragments
```

### 3.4 BossSpider 蛛神 — 幻与网

```
A terrifyingly beautiful Chinese spider goddess with
six elegant arms in flowing black-and-crimson robes,
a jeweled spider tiara, hypnotic piercing eyes,
eastern fantasy style, semi-realistic anime,
card portrait, upper body, golden card border frame,
silk web patterns, purple mystical fog, spider silk threads
```

---

## 四、生成参数建议

| 参数 | 建议值 |
|---|---|
| 分辨率 | 1024×1536（2:3 竖版） |
| 模型 | Pony Diffusion XL / SDXL Turbo / Juggernaut |
| 采样器 | DPM++ 2M Karras / Euler a |
| 步数 | 20-30 |
| CFG Scale | 7-9 |
| 风格引导 | anime-style, detailed cloth, cinematic lighting |

## 五、批量生成建议

1. 先用基础模板生成 4 张不同类型的样稿
2. 确认颜色风格后，用对应类型提示词批量生成剩余卡牌
3. 统一用 `golden card border frame` 确保系列一致性
4. 生成后用 `cleanpng` 或类似工具去背景，导出为 PNG

## 六、卡牌边框与 UI 配套

所有卡牌立绘输出后，需要配套以下 UI 元素：

- **金色卡牌边框**（统一 4px 金边 #B8860B）
- **能耗角标**（右上角圆形，#2A9D8F 底色）
- **稀有度光效**（N=白 / R=蓝 / SR=金 / SSR=彩虹流光）
- **类型色条**（左边缘 4px 宽，颜色按 Attack/Defense/Transform/Utility）

---

_Artoria — 美术资产输出 v0.1_