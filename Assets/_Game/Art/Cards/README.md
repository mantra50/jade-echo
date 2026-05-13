# Card Assets · 卡牌美术资源目录

## 目录结构

```
Assets/_Game/Art/Cards/
├── CardAssets/          ← AI 生成图片资源放这里（.png / .jpg）
│   ├── card-001.png
│   ├── card-002.png
│   └── ...
├── card-asset-manifest-v1.md   ← ⚠️ 作废，旧版卡牌清单
└── ArtDocuments/        ← 美术文档（AI 提示词、色值规范等）
    ├── card-asset-manifest-v2.md
    └── card-color-spec.md
```

## 说明

- **图片资源**（AI 生成的卡牌立绘）统一放到 `CardAssets/`，命名格式：`card-XXX.png`
- **元数据/AI 提示词** 记录在 `ArtDocuments/` 下的 manifest 文件中
- 旧版 `card-asset-manifest-v1.md` 已作废，请勿使用
- 颜色规范参考 `ArtDocuments/card-color-spec.md`
