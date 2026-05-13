# 卡牌美术资产清单 v2 — Artoria 美术总监版

> **版本：** v2.0
> **日期：** 2026-05-13
> **美术总监：** Artoria
> **用途：** Midjourney / Stable Diffusion / DALL-E AI生图
> **风格规范：** 东方古风卡通写实，7.5头身，墨韵线条留白，水墨晕染

---

## 📌 审查结论

**卡牌数量：** 20张（card-001 ~ card-020）

**与 design-output/card-numbers-v2.md 对照结果：**
- ⚠️ **重大发现：** `card-numbers-v2.md` 描述的是另一套 28 张卡牌体系（card_001 ~ card_028），卡名/类型/能耗/稀有度与本 manifest **完全不同**
- 本 manifest 的 20 张卡是 **美术资产专用**卡池，与游戏数值表为**平行文档**，不一一对应
- 以下字段核对了本 manifest 内部一致性（manifest.md ↔ 各 card-XXX.json）：名称/类型/稀有度/能耗 — **全部一致**

| 核对项 | 结果 |
|--------|------|
| manifest.md 名称 ↔ JSON 名称 | ✅ 一致（20/20） |
| manifest.md 类型 ↔ JSON type | ✅ 一致（20/20） |
| manifest.md 稀有度 ↔ JSON rarity | ✅ 一致（20/20） |
| manifest.md 能耗 ↔ JSON energyCost | ✅ 一致（20/20） |
| manifest.md 类型色 ↔ JSON palette.primary | ⚠️ 需修正 |

**palette 修正说明：**
- 原 JSON 中各卡牌 primary 色值混乱，不符合类型色值规范
- v2 统一将 palette.primary 调整为类型标准色

---

## 类型色值规范（严格遵守）

| 类型 | 色值 | 色调描述 | AI 颜色关键词 |
|------|------|---------|--------------|
| **Attack（攻击）** | `#E63946` | 朱红 | crimson, scarlet, vivid red, flame red, blood red |
| **Defense（防御）** | `#457B9D` | 藏青 | dark teal blue, slate blue, deep navy, steel blue |
| **Transform（转化）** | `#9B5DE5` | 紫藤 | violet purple, lavender, amethyst, orchid purple |
| **Utility（功能）** | `#2A9D8F` | 青绿 | jade green, teal green, seafoam green, mint green |

---

## 美术资产总览

| # | 卡牌ID | 中文名 | 类型 | 稀有度 | 能耗 | 色调 | 状态 |
|---|--------|--------|------|--------|------|------|------|
| 1 | card-001 | 破风斩 | Attack | N | 15 | #E63946 | ✅ |
| 2 | card-002 | 烈焰冲击 | Attack | R | 20 | #E63946 | ✅ |
| 3 | card-003 | 剑气纵横 | Attack | R | 25 | #E63946 | ✅ |
| 4 | card-004 | 穿云箭 | Attack | R | 18 | #E63946 | ✅ |
| 5 | card-005 | 雷罚天降 | Attack | SR | 35 | #E63946 | ✅ |
| 6 | card-006 | 护体金钟 | Defense | N | 10 | #457B9D | ✅ |
| 7 | card-007 | 气盾术 | Defense | R | 20 | #457B9D | ✅ |
| 8 | card-008 | 磐石壁垒 | Defense | R | 25 | #457B9D | ✅ |
| 9 | card-009 | 玄冰护甲 | Defense | R | 15 | #457B9D | ✅ |
| 10 | card-010 | 万灵归元 | Defense | SR | 30 | #457B9D | ✅ |
| 11 | card-011 | 玉石转化 | Transform | R | 15 | #9B5DE5 | ✅ |
| 12 | card-012 | 元素乱流 | Transform | R | 20 | #9B5DE5 | ✅ |
| 13 | card-013 | 火焰同调 | Transform | R | 18 | #9B5DE5 | ✅ |
| 14 | card-014 | 疾风驱散 | Transform | N | 12 | #9B5DE5 | ✅ |
| 15 | card-015 | 灵泉涌现 | Transform | SR | 22 | #9B5DE5 | ✅ |
| 16 | card-016 | 灵气吸纳 | Utility | N | 5 | #2A9D8F | ✅ |
| 17 | card-017 | 卡牌回收 | Utility | R | 0 | #2A9D8F | ✅ |
| 18 | card-018 | 玉石祝福 | Utility | R | 8 | #2A9D8F | ✅ |
| 19 | card-019 | 绝境逢生 | Utility | SR | 0 | #2A9D8F | ✅ |
| 20 | card-020 | 天眼通 | Utility | SR | 15 | #2A9D8F | ✅ |

---

## 详细卡牌数据（v2 标准提示词）

---

### card-001 · 破风斩

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-001 |
| **中文名** | 破风斩 |
| **英文名** | Wind-Breaking Slash |
| **类型** | Attack |
| **能耗** | 15 |
| **稀有度** | N（★☆☆☆） |
| **主色调** | `#E63946`（朱红） |
| **副色调** | `#FFD700`（金黄） |
| **背景色** | `#1A0A0A`（深褐黑） |

**AI 提示词（v2）：**
> color palette: #E63946 dominant, vivid crimson and scarlet, elegant female warrior in celestial golden armor mid-leap slashing a massive crimson dragon-shaped qi wave from her jian sword, dragon energy coiling like a serpent of fire and blood, battle ribbons and hair streaming in the wind, intricate gold filigree on armor, ink brushstroke outlines with white space at edges, shattered mountain peaks and war clouds in background, dramatic rim lighting from crimson dragon energy, cinematic action composition, Chinese xianxia mythology style, cartoon realism with soft cel shading, hyperdetailed armor textures, 4k ultra detailed illustration

**特效：** 金色龙形剑气从剑身爆发 | 龙鳞护甲反射火光 | 破碎山峰背景

---

### card-002 · 烈焰冲击

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-002 |
| **中文名** | 烈焰冲击 |
| **英文名** | Blazing Inferno Strike |
| **类型** | Attack |
| **能耗** | 20 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#E63946`（朱红） |
| **副色调** | `#FF4500`（橙红） |
| **背景色** | `#1C0A05`（深褐红） |

**AI 提示词（v2）：**
> color palette: #E63946 dominant, deep crimson and flame orange, elegant woman in flowing red qipao battle dress with gold phoenix embroidery, both palms raised overhead holding a giant blooming flaming lotus, intense heat shimmer distorting air around her, long wavy black hair whipping upward from thermal updraft, fierce burning eyes, scorched stone ground and ash particles floating in background, dramatic low-angle shot framed by fire, golden orange light on face and body from lotus flame, Chinese mythology fire element style, cartoon realism with painterly brush textures, rich crimson gold and charcoal black palette, hyperdetailed flame petal textures, 4k ultra detailed illustration

**特效：** 赤红莲花从掌心绽放 | 热浪扭曲效果 | 燃烧粒子与灰烬飘散

---

### card-003 · 剑气纵横

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-003 |
| **中文名** | 剑气纵横 |
| **英文名** | Sword Qi Sweep |
| **类型** | Attack |
| **能耗** | 25 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#E63946`（朱红） |
| **副色调** | `#FFD700`（金黄） |
| **背景色** | `#0A1520`（深蓝黑） |

**AI 提示词（v2）：**
> color palette: #E63946 dominant, scarlet crimson and golden amber, beautiful female warrior soaring upward through dark storm cloud break, wrapped in massive wings of crimson and gold phoenix flames, twin swords pointing skyward in cross guard, elegant hanfu with phoenix feather embroidery flowing upward like flames, jade hair ornaments loosened and streaming fire trails, divine fire phoenix entity descending merging into wings, dramatic backlit golden sunlight breaking through dark clouds creating halo, mountain ranges and scattered clouds below, ink wash background, epic sweeping upward diagonal composition, Chinese mythology phoenix style, cartoon realism with luminous fire effects, hyperdetailed phoenix feather textures, 4k ultra detailed illustration

**特效：** 凤凰羽翼展开火焰羽毛飞散 | 剑气横贯画面 | 云层破洞山峦云海

---

### card-004 · 穿云箭

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-004 |
| **中文名** | 穿云箭 |
| **英文名** | Sky-Piercing Arrow |
| **类型** | Attack |
| **能耗** | 18 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#E63946`（朱红） |
| **副色调** | `#8B4513`（赭石） |
| **背景色** | `#0D0D1A`（深夜蓝黑） |

**AI 提示词（v2）：**
> color palette: #E63946 dominant, deep crimson and earthy brown, armored warrior kneeling on one knee gripping a heavy broadsword planted into cracked stone ground, massive black turtle-snake Xuanwu rising behind him as protective spirit shield, serpentine tail coiling around warrior body, dark scaled armor covered in glowing crimson ipu seals and mystical runes, ominous skull mask helmet, dramatic low angle looking up at towering guardian spirit beast, misty graveyard background with dead trees and broken tombstones, dark crimson shadow and black palette with crimson seal glow, ink brushstrokes heavy on shadows, Chinese mythology guardian spirit style, cartoon realism with heavy atmosphere, detailed scale textures, dramatic rim lighting from crimson supernatural glow, 4k ultra detailed illustration

**特效：** 玄武黑龟灵蛇护卫形态 | 红色玄冥符文发光 | 阴森墓园枯树残碑

---

### card-005 · 雷罚天降

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-005 |
| **中文名** | 雷罚天降 |
| **英文名** | Thunder Divine Punishment |
| **类型** | Attack |
| **能耗** | 35 |
| **稀有度** | SR（★★★☆） |
| **主色调** | `#E63946`（朱红） |
| **副色调** | `#FFD700`（金黄） |
| **背景色** | `#050510`（极深蓝黑） |

**AI 提示词（v2）：**
> color palette: #E63946 dominant, crimson and golden light, beautiful noble warrior woman standing in confident stance inside a dome of solid crimson tiger spirit energy, giant white tiger with golden stripes visible within translucent protective barrier behind her, white and gold battle attire with tiger stripe embroidery, one hand raised palm outward maintaining shield, serene battle-ready expression, long black hair with tiger claw ornament, cherry blossom petals and crimson spiritual energy particles floating around barrier, soft pink dawn clouds and ancient chinese palace rooftops in background, crimson gold soft pink and ethereal blue palette, divine glow emanating from tiger barrier, Chinese mythology guardian spirit style, cartoon realism with luminous barrier effects, hyperdetailed tiger fur texture within semi-transparent shield, 4k ultra detailed illustration

**特效：** 白虎护盾穹顶虎灵若隐若现 | 樱花瓣与灵气粒子漂浮 | 黎明粉霞宫殿屋顶背景

---

### card-006 · 护体金钟

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-006 |
| **中文名** | 护体金钟 |
| **英文名** | Golden Bell Shield |
| **类型** | Defense |
| **能耗** | 10 |
| **稀有度** | N（★☆☆☆） |
| **主色调** | `#457B9D`（藏青） |
| **副色调** | `#FFD700`（金黄） |
| **背景色** | `#1A1408`（深褐） |

**AI 提示词（v2）：**
> color palette: #457B9D dominant, dark teal blue and warm gold, elderly chinese martial arts master standing with perfect posture, one hand raised in Buddhist mudra seal gesture, massive ornate golden ancient bell manifesting as translucent protective dome surrounding him, bell surface covered in intricate Sanskrit sutra characters and Chinese calligraphy glowing with warm light, simple brown monk robes with subtle cloth texture, halo of golden divine light above shaved head, stone temple courtyard floor with moss patterns, scattered golden maple leaves on ground, contemplative serene expression with closed eyes, dark teal blue and deep brown palette, ink linework with golden light highlights, Chinese martial arts divine protection style, cartoon realism with sacred geometry details on bell, soft candlelight glow effect, 4k ultra detailed illustration

**特效：** 金钟护罩呈半透明穹顶古钟符文发光 | 禅师蒙昧手印 | 石板青苔金色枫叶地面

---

### card-007 · 气盾术

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-007 |
| **中文名** | 气盾术 |
| **英文名** | Qi Barrier |
| **类型** | Defense |
| **能耗** | 20 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#457B9D`（藏青） |
| **副色调** | `#87CEFA`（浅蓝） |
| **背景色** | `#080D20`（深蓝黑） |

**AI 提示词（v2）：**
> color palette: #457B9D dominant, dark teal blue and pale blue, stunning kitsune fox spirit beauty caught in transformation from human to fox, one elegant human face on left side beautiful with delicate features, transforming demonic fox face on right side with sharp fangs and fierce eyes, nine massive ethereal fox tails materializing and spreading wide behind her like a peacock of flame, fox ears visible through disheveled beautiful hair, traditional tang dynasty palace dress with floral peony embroidery now half-shredded merging with ghostly fox fur, surrounded by swirling peach blossom petals and fox fire flames in pale blue, enchanting seductive dangerous expression, moonlit classical chinese garden background with curved bridges and willow trees, dark teal blue white pale blue and deep crimson palette, dreamy soft lighting with mystical glow, ink brush outlines with deliberate white space areas, Chinese fox spirit mythology style, cartoon realism with supernatural transformation effects, hyperdetailed fox fur and silk dress textures, 4k ultra detailed illustration

**特效：** 碧蓝色气体护盾环绕角色 | 狐妖九尾半现桃红花瓣幽蓝狐火 | 月色古典园林背景

---

### card-008 · 磐石壁垒

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-008 |
| **中文名** | 磐石壁垒 |
| **英文名** | Stone Fortress |
| **类型** | Defense |
| **能耗** | 25 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#457B9D`（藏青） |
| **副色调** | `#8B4513`（赭石） |
| **背景色** | `#0E0A06`（深棕黑） |

**AI 提示词（v2）：**
> color palette: #457B9D dominant, dark teal blue and earth brown, young male swordsman mid-transformation with golden dragon spirit, half his body covered in radiant golden dragon scales with glowing seams where human skin transitions to scaled flesh, elegant jade-green dragon horns emerging from his hair, one hand gripping sheathed sword vibrating with dragon energy, flowing martial artist robes torn and reforming with dragon scale patterns and flowing mane-like fur collar, fierce determined eyes now partially glowing with dragon spirit, dark storm mountain peak background with lightning crackling behind him, dark teal blue golden amber and deep brown palette, dramatic lightning-illuminated backlighting, ink wash heavy clouds swirling behind figure, Chinese xianxia dragon fusion style, cartoon realism with luminous dragon scale textures, hyperdetailed scale gradient transition from human skin to armored dragon hide, 4k ultra detailed illustration

**特效：** 龙鳞渐变覆盖半身金色龙角生长 | 巨岩屏障从地面升起 | 风暴山顶雷电轮廓

---

### card-009 · 玄冰护甲

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-009 |
| **中文名** | 玄冰护甲 |
| **英文名** | Frost Ice Armor |
| **类型** | Defense |
| **能耗** | 15 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#457B9D`（藏青） |
| **副色调** | `#E0FFFF`（浅青白） |
| **背景色** | `#0A1A2E`（深蓝黑） |

**AI 提示词（v2）：**
> color palette: #457B9D dominant, dark teal blue and icy cyan white, ancient chinese military commander corpse rising from shattered wooden coffin, decaying green-gray skin with exposed muscle fiber and ancient battle scars, wide staring eyes with glowing yellow-green pinprick pupils, jagged broken teeth with one fang longer, mouth open in silent roar, muscular male body in tattered remains of ancient imperial armor with rusted metal plates, glowing blue-white ritual seals inscribed on forehead chest and arms burning with supernatural light, thick black mist and grave soil pouring from broken coffin below, broken stone tomb chamber walls visible at edges, cracked earth ground with grave offerings scattered, bones and shattered weapons half-buried, eerie dark teal fog spreading, dark teal deep purple black and bone white palette, dramatic low angle looking up at rising undead commander, horror awe-inspiring atmosphere, Chinese hoppy vampire zombie mythology style, cartoon realism with detailed rotting flesh textures, ink splatter effects of grave dirt and supernatural energy, 4k ultra detailed illustration

**特效：** 冰晶铠甲覆盖全身蓝色霜雾缭绕 | 僵尸符文与冰霜并存 | 古墓背景幽绿瘴气

---

### card-010 · 万灵归元

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-010 |
| **中文名** | 万灵归元 |
| **英文名** | Soul Convergence |
| **类型** | Defense |
| **能耗** | 30 |
| **稀有度** | SR（★★★☆） |
| **主色调** | `#457B9D`（藏青） |
| **副色调** | `#90EE90`（浅绿） |
| **背景色** | `#061406`（深绿黑） |

**AI 提示词（v2）：**
> color palette: #457B9D dominant, dark teal blue and fresh green, beautiful fairy herb master floating gently above lush mountain meadow, arms cradling bundles of glowing medicinal herbs and massive pristine ganoderma lucidum mushroom radiating soft golden light, flowing white and fresh green robes with embroidered peony lotus and medicinal plant motifs, long flowing black hair with small flower ornaments and strands of jade beads, serene kind compassionate expression with gentle closed-eye smile, bare feet hovering above grass, soft morning sunlight breaking through misty bamboo forest behind her with visible dust motes and pollen particles, small orange tabby cat sitting peacefully on one arm among herbs, butterflies and dragonflies circling, spring meadow with countless small flowers and edible herbs at her feet, dark teal blue fresh green white soft gold and warm amber palette, dreamy soft diffused lighting like early morning golden hour, Chinese mythology healing fairy style, cartoon realism with luminous herbal glow effects, hyperdetailed botanical textures on each herb and mushroom, 4k ultra detailed illustration

**特效：** 绿色灵气旋涡环绕角色自然治愈光效 | 灵芝发光药草漂浮 | 竹林晨光柔和漫射光

---

### card-011 · 玉石转化

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-011 |
| **中文名** | 玉石转化 |
| **英文名** | Jade Transformation |
| **类型** | Transform |
| **能耗** | 15 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#9B5DE5`（紫藤） |
| **副色调** | `#00FA9A`（翠绿） |
| **背景色** | `#041208`（深绿黑） |

**AI 提示词（v2）：**
> color palette: #9B5DE5 dominant, violet purple and emerald jade green, beautiful young female daoist sorceress kneeling in meditation pose inside a ring of ancient Chinese jigsaw puzzle pieces slowly transforming into glowing emerald jade tiles floating in mid-air around her, simple elegant green daoist robes with subtle cloud patterns now shimmering with faint jade glow, one hand extended palm upward with a single chess piece transforming from ordinary stone to luminous jade, long black hair in traditional bun hairstyle with simple jade hairpin ornaments, serene focused expression with closed eyes, floating green jade fragments and dust particles swirling in mystic circle around her, ancient stone temple floor with carved ritual circle pattern, soft purple and jade green mystical light emanating from the jade tiles, violet purple emerald white and deep forest palette, ink brush linework with jade luminosity glow effects, Chinese mythology jade magic style, cartoon realism with translucent jade texture effects, hyperdetailed stone-to-jade transformation shimmer, 4k ultra detailed illustration

**特效：** 棋子被翡翠玉石光芒包裹转变 | 翡翠碎片灵气粒子旋转 | 古老石台符文阵法地面

---

### card-012 · 元素乱流

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-012 |
| **中文名** | 元素乱流 |
| **英文名** | Elemental Surge |
| **类型** | Transform |
| **能耗** | 20 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#9B5DE5`（紫藤） |
| **副色调** | `#FF8C00`（橙黄） |
| **背景色** | `#0D0518`（深紫黑） |

**AI 提示词（v2）：**
> color palette: #9B5DE5 dominant, violet purple and flame orange, powerful male elemental master standing on one knee in dramatic pose, four elemental spirits converging around him in violent tempest, massive red fire phoenix spiraling upward on his right side, giant blue water dragon coiling below his feet, ancient green wooden tree spirit roots and branches spreading behind him, golden earth goddess spirit manifesting from ground beneath him, tattered elemental robes constantly shifting between fire water earth and wind textures, wild disheveled long hair with leaves and flame licks and water droplets intermingled, fierce battle-ready expression with glowing multicolor eyes, chaotic swirling vortex background with all four elements in violent collision, violet purple orange teal gold and crimson chaotic palette, ink splatter effects of elemental particles everywhere, dynamic diagonal composition with figure at center of elemental convergence, Chinese mythology five element master style, cartoon realism with intense elemental collision effects, hyperdetailed spirit creature textures for each element, 4k ultra detailed illustration

**特效：** 四元素风暴席卷棋盘火水土木精灵交汇 | 斗篷不断变换材质 | 混沌旋涡能量粒子四散

---

### card-013 · 火焰同调

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-013 |
| **中文名** | 火焰同调 |
| **英文名** | Flame Sync |
| **类型** | Transform |
| **能耗** | 18 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#9B5DE5`（紫藤） |
| **副色调** | `#E63946`（朱红） |
| **背景色** | `#1C0805`（深褐红） |

**AI 提示词（v2）：**
> color palette: #9B5DE5 dominant, violet purple and deep crimson fire, beautiful female fire mage with long flowing crimson hair erupting with flames, standing amidst a circle of burning chess pieces that are igniting in sequence, wearing elegant black and crimson robes with fire pattern embroidery, both hands raised commanding the synchronized flame explosion, fierce focused expression with eyes glowing like embers, surrounding chess pieces transforming from normal stone to white-hot burning pieces in perfect unison, ancient stone arena floor with ritual fire circle carved into ground, flames spreading outward in perfect rings from center, orange red violet purple and black palette, dramatic upward composition with fire rising everywhere, ink brush linework with flame lick effects, Chinese mythology fire synchronization magic style, cartoon realism with synchronized flame ignition effects, hyperdetailed flame spreading patterns across each chess piece, 4k ultra detailed illustration

**特效：** 同色棋子集体燃烧爆炸火光冲天 | 火焰按圆环同步点燃 | 古老石台火焰符文

---

### card-014 · 疾风驱散

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-014 |
| **中文名** | 疾风驱散 |
| **英文名** | Wind Purge |
| **类型** | Transform |
| **能耗** | 12 |
| **稀有度** | N（★☆☆☆） |
| **主色调** | `#9B5DE5`（紫藤） |
| **副色调** | `#98FB98`（浅绿） |
| **背景色** | `#081808`（深绿黑） |

**AI 提示词（v2）：**
> color palette: #9B5DE5 dominant, violet purple and fresh green wind, elegant female wind mage with long silver-white hair flowing in powerful gale winds, standing with arms spread wide commanding a massive green wind vortex sweeping across the board, wearing flowing white and green robes with wind wave embroidery, fierce determined expression, long hair and robes streaming horizontally in powerful gusts, surrounding chess pieces being blown away and dissipated by the green wind current, scattered light debris and energy particles carried by the wind, ancient chinese temple stone floor with dust patterns being swept clean, violet purple fresh green white and sky blue palette, dramatic side composition with wind lines showing movement direction, ink brush linework with wind streak effects, Chinese mythology wind spirit style, cartoon realism with powerful gale visualization effects, hyperdetailed wind particle trajectories and debris scatter patterns, 4k ultra detailed illustration

**特效：** 疾风卷过棋盘清场绿色清风扫除杂质 | 棋子被风吹散 | 尘埃与灵气粒子飘散

---

### card-015 · 灵泉涌现

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-015 |
| **中文名** | 灵泉涌现 |
| **英文名** | Spirit Spring Surge |
| **类型** | Transform |
| **能耗** | 22 |
| **稀有度** | SR（★★★☆） |
| **主色调** | `#9B5DE5`（紫藤） |
| **副色调** | `#40E0D0`（青绿） |
| **背景色** | `#040E14`（深青黑） |

**AI 提示词（v2）：**
> color palette: #9B5DE5 dominant, violet purple and turquoise teal, beautiful female water spirit rising from a newly burst sacred spring on the board, flowing robes made of water and light, long blue-black hair streaming with water currents, both hands cupped upward releasing glowing water spirit energy, serene ethereal expression, pure water spring gushing from stone board floor with enormous pressure, water particles and luminous spirit dots floating upward with the flow, ancient stone board surface with water channels and mystical patterns being activated, violet purple turquoise white and deep blue palette, dramatic upward composition with water burst and spirit light, dreamy soft lighting with water refraction effects, ink brush linework with water flow effects, Chinese mythology water spirit style, cartoon realism with sacred spring eruption effects, hyperdetailed water particle trajectories and luminous spirit dot patterns, 4k ultra detailed illustration

**特效：** 清泉从棋盘涌出流水质感 | 绿色光点漂浮上升 | 古老石板水渠激活

---

### card-016 · 灵气吸纳

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-016 |
| **中文名** | 灵气吸纳 |
| **英文名** | Spirit Siphon |
| **类型** | Utility |
| **能耗** | 5 |
| **稀有度** | N（★☆☆☆） |
| **主色调** | `#2A9D8F`（青绿） |
| **副色调** | `#ADFF2F`（黄绿） |
| **背景色** | `#061A0A`（深绿黑） |

**AI 提示词（v2）：**
> color palette: #2A9D8F dominant, jade green and yellow-green, gentle elderly chinese herbalist sage with kind grandfatherly appearance sitting in meditation under sacred ginkgo tree, one hand resting on knee palm facing upward in receiving gesture, other hand extended with fingers gently pulling visible streams of green glowing life energy from the surrounding environment into his body, simple cream and brown traditional robes with subtle leaf pattern embroidery, bald head with wisps of white hair at temples, wise warm smile with closed eyes, small magical familiar rabbit sitting peacefully in his lap, glowing green spirit energy streams flowing from flowers and plants and small spirits around him into his body like gentle rivers of light, scattered autumn ginkgo leaves falling slowly with one landing on his shoulder, peaceful forest clearing with soft morning light filtering through tree canopy, small fireflies and light motes floating in sunbeams, jade green brown pale yellow and soft gold palette, warm gentle lighting like afternoon golden hour, ink brush linework with soft energy stream effects, Chinese mythology taoist hermit healer style, cartoon realism with flowing energy absorption effects, hyperdetailed ginkgo leaf textures and gentle spirit stream patterns, 4k ultra detailed illustration

**特效：** 绿色生命灵气如细流汇入体内 | 银杏树下坐禅秋天落叶 | 花草精灵释放能源小兔子陪伴

---

### card-017 · 卡牌回收

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-017 |
| **中文名** | 卡牌回收 |
| **英文名** | Card Recall |
| **类型** | Utility |
| **能耗** | 0 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#2A9D8F`（青绿） |
| **副色调** | `#9370DB`（紫罗兰） |
| **背景色** | `#0D0818`（深紫黑） |

**AI 提示词（v2）：**
> color palette: #2A9D8F dominant, jade green and soft purple, elegant female court scholar in flowing purple and silver brocade robes with crane and cloud embroidery, standing with one arm gracefully extended toward a floating ancient scroll that is unfurling and glowing with mystical green light, other hand held to her chest in a gesture of focused concentration, hair in elaborate traditional Chinese updo hairstyle with silver hairpins and purple jade ornaments, serene intelligent expression with slightly narrowed eyes of deep focus, multiple floating card silhouettes made of jade green light energy flying back toward her like homing pigeons returning home, an antique chinese wooden card discard pile to her left with one card already lifting off and rejoining the flow, traditional chinese study room background with bamboo bookshelf scrolls and a small koi fish pond with curved bridge, jade green purple silver white and deep indigo palette, mystical green glow emanating from the recalled cards creating light trails, ink brush linework with green energy stream effects, Chinese mythology scholarly magic style, cartoon realism with card flight trajectory effects, hyperdetailed brocade fabric patterns and scroll calligraphy textures, 4k ultra detailed illustration

**特效：** 卡牌化为绿光流光飞回手中 | 古风卷轴展开卡牌排列归位 | 书房典雅背景鲤鱼池塘

---

### card-018 · 玉石祝福

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-018 |
| **中文名** | 玉石祝福 |
| **英文名** | Jade Blessing |
| **类型** | Utility |
| **能耗** | 8 |
| **稀有度** | R（★★☆☆） |
| **主色调** | `#2A9D8F`（青绿） |
| **副色调** | `#FFD700`（金黄） |
| **背景色** | `#041008`（深绿黑） |

**AI 提示词（v2）：**
> color palette: #2A9D8F dominant, jade green and golden light, beautiful celestial maiden floating above the board with gentle smile, wearing elegant white and jade green robes with flower embroidery, both hands raised blessing the board with flowing jade energy, serene compassionate expression, glowing jade light ring blessing the entire board surface below her, jade fragments and golden sparkles raining down onto the board with each elimination, ancient stone board surface glowing with activated jade patterns, soft green mist rising from the board creating ethereal atmosphere, jade green white gold and deep forest palette, dreamy soft lighting with blessing aura effects, ink brush linework with jade shimmer effects, Chinese mythology celestial blessing style, cartoon realism with blessing light effects, hyperdetailed jade fragment textures and golden sparkle patterns, 4k ultra detailed illustration

**特效：** 玉石光环笼罩棋盘 | 金色光点伴随消除飘落 | 古老石板玉石纹路发光

---

### card-019 · 绝境逢生

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-019 |
| **中文名** | 绝境逢生 |
| **英文名** | Last Stand |
| **类型** | Utility |
| **能耗** | 0 |
| **稀有度** | SR（★★★☆） |
| **主色调** | `#2A9D8F`（青绿） |
| **副色调** | `#E63946`（朱红） |
| **背景色** | `#1C0805`（深褐红） |

**AI 提示词（v2）：**
> color palette: #2A9D8F dominant, jade green and crimson fire, wounded warrior king kneeling on one knee on scorched battlefield about to collapse, his magnificent jade-green and crimson dragon scaled battle armor cracked and broken in multiple places with sparks of dying flames still flickering in the cracks, one fist slammed into the ground in defiance holding up his body from total collapse, broken legendary sword embedded in stone ground in front of him, massive phoenix spirit of jade-green fire rising rebirth flames erupting from his back in dramatic upward explosion of feathers and fire, his tattered jade battle banner with dragon emblem fluttering behind him, intense defiant expression with eyes squeezed shut teeth clenched gritting through pain, surrounded by the bodies of fallen enemies and shattered weapons, dramatic upward diagonal composition with the phoenix and fire representing rebirth breaking upward, in the distance a jade-green sunrise breaking through dark storm clouds representing hope, jade green crimson gold and charcoal black palette, intense dramatic rim lighting from phoenix flames creating silhouette effect, ink splatter effects of battle damage and fire sparks, Chinese mythology dying king reborn style, cartoon realism with phoenix flame rebirth effects, hyperdetailed cracked armor textures and individual flame feather patterns, 4k ultra detailed illustration

**特效：** 金色火焰凤凰从背后涅槃重生 | 破碎铠甲迸发火花 | 战场废墟背景远处金色日出

---

### card-020 · 天眼通

| 字段 | 内容 |
|------|------|
| **卡牌ID** | card-020 |
| **中文名** | 天眼通 |
| **英文名** | Heaven Sight |
| **类型** | Utility |
| **能耗** | 15 |
| **稀有度** | SR（★★★☆） |
| **主色调** | `#2A9D8F`（青绿） |
| **副色调** | `#4169E1`（宝蓝） |
| **背景色** | `#0A0A1E`（深蓝黑） |

**AI 提示词（v2）：**
> color palette: #2A9D8F dominant, jade green and royal blue, majestic celestial being in form of beautiful young male celestial officer with perfect symmetrical face, third eye vertically centered on forehead open and glowing with intense jade-green divine light, wearing magnificent celestial armor of interlocking gold and jade-green plates with intricate star map engravings, one hand holding a massive ancient celestial longbow of white jade and gold, other hand raised with palm facing outward in a gesture of cosmic awareness, long flowing hair of deep blue black being blown by ethereal celestial wind, gentle serene confident expression with eyes of deep cosmic wisdom, a halo of jade-green divine light radiating outward from his third eye creating an aperture of pure celestial sight, multiple concentric rings of glowing jade and royal blue sacred geometry expanding outward from his third eye like cosmic ripples, ethereal jade-green light tendrils and star lines connecting his third eye to hidden celestial knowledge streams in the cosmos, floating celestial artifacts of jade and gold around him including ancient astrolabe and star maps, majestic celestial realm background with deep space filled with distant galaxies stars nebulae and cosmic phenomena, he stands on a platform of white jade floating in the cosmos, behind him a massive vaulted celestial dome ceiling filled with intricate golden star map patterns glowing softly, jade green royal blue white gold and deep cosmic indigo palette, intense glowing third eye light effects with divine ray beams and sacred geometry patterns, ink brush linework with cosmic light effects, Chinese mythology celestial observer heaven seer style, cartoon realism with third eye vision beam effects and expanding sacred geometry rings, hyperdetailed celestial armor textures and star map engraving patterns, 4k ultra detailed illustration

**特效：** 第三眼睁眼绽放青绿神光 | 黄金与宝蓝星纹线条从眼部向外扩散 | 苍穹宝顶星图背景紫青气晕
