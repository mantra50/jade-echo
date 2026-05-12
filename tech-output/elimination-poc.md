# CardMatch - 消除核心PoC文档
**版本：** v1.0  
**作者：** Orion（技术总监）  
**日期：** 2026-06-01  
**目标：** 验证Match3消除算法、连锁检测、状态机驱动的核心循环

---

## 1. Match3 核心算法

### 1.1 棋盘数据结构

```csharp
// 棋盘：8×8，元素类型 0~5（共6种）
public enum ElementType { Red = 0, Blue = 1, Green = 2, Yellow = 3, Purple = 4, Orange = 5 }

public class Cell {
    public int x, y;
    public ElementType type;
    public bool isEmpty => type == ElementType.None;
}

public class Board {
    const int WIDTH = 8;
    const int HEIGHT = 8;
    Cell[,] cells = new Cell[WIDTH, HEIGHT];

    public Cell Get(int x, int y) => cells[x, y];
    public void Set(int x, int y, ElementType type) => cells[x, y].type = type;
    public void Swap(Cell a, Cell b) { (a.type, b.type) = (b.type, a.type); }
    public bool IsValid(int x, int y) => x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
}
```

### 1.2 匹配检测算法（扫描线）

```csharp
public class MatchResult {
    public List<Cell> cells = new();      // 所有匹配格
    public bool isLTShape = false;        // L/T形特殊匹配
    public (int x, int y)? intersection; // L/T交叉点
}

public MatchResult ScanAll(Board board) {
    var result = new MatchResult();

    // ── 水平扫描 ──
    for (int y = 0; y < 8; y++) {
        int x = 0;
        while (x < 8) {
            var type = board.Get(x, y).type;
            int start = x;
            while (x < 8 && board.Get(x, y).type == type) x++;
            if (x - start >= 3) {
                for (int i = start; i < x; i++) result.cells.Add(board.Get(i, y));
            }
        }
    }

    // ── 垂直扫描 ──
    for (int x = 0; x < 8; x++) {
        int y = 0;
        while (y < 8) {
            var type = board.Get(x, y).type;
            int start = y;
            while (y < 8 && board.Get(x, y).type == type) y++;
            if (y - start >= 3) {
                for (int j = start; j < y; j++) result.cells.Add(board.Get(x, j));
            }
        }
    }

    // ── L/T形检测：找到同时出现在水平段和垂直段中的交叉点 ──
    var grouped = result.cells
        .GroupBy(c => (c.x, c.y))
        .Where(g => g.Count() >= 2)
        .ToList();

    if (grouped.Any()) {
        result.isLTShape = true;
        var first = grouped.First();
        result.intersection = (first.Key.x, first.Key.y);
    }

    return result;
}
```

---

## 2. 连锁检测（Cascade）伪代码

```python
# ── 连锁检测伪代码 ──
def cascade_loop(board):
    combo = 0
    while True:
        matches = scan_all(board)

        if not matches.cells:
            break  # 无匹配，退出循环

        combo += 1

        # ── 步骤1：消除匹配格 ──
        for cell in matches.cells:
            board.set_empty(cell.x, cell.y)
        trigger_elimination_effects(matches)

        # ── 步骤2：重力下落 ──
        gravity_apply(board)

        # ── 步骤3：填满空位（生成新元素）──
        fill_empty(board)

        # ── 步骤4：播放下落动画（协程等待）──
        yield wait_for_animation(0.3s)

        # ── 步骤5：再次扫描（进入下一轮连锁）──
    return combo
```

---

## 3. 消除状态机

```
┌──────────┐
│   IDLE   │ ← 初始/连锁结束
└────┬─────┘
     │ cellClicked
     ▼
┌──────────┐
│ SELECTED │ ← 已选中一个格子
└────┬─────┘
     │ cellClicked(nearby)
     ▼
┌───────────┐
│ SWAPPING  │ ← 正在交换（播放动画）
└─────┬─────┘
      │ animationDone
      ▼
 ┌────────────┐   noMatch    ┌───────────┐
 │ELIMINATING │ ──────────→ │  CASCADE  │ ← 交换无匹配，恢复原位
 └─────┬──────┘             └───────────┘
       │ hasMatch
       ▼
 ┌───────────┐
 │  FALLING  │ ← 消除后下落
 └─────┬─────┘
       │ fallDone
       ▼
 ┌───────────┐
 │  CASCADE  │ ← 再次检测匹配
 └─────┬─────┘
       │ noMoreMatches
       ▼
   [返回 IDLE]
```

**状态说明：**

| 状态 | 含义 | 触发条件 | 退出条件 |
|------|------|---------|---------|
| `IDLE` | 等待玩家点击 | 初始化/连锁结束 | 点击格子 |
| `SELECTED` | 已选中一个元素 | IDLE+点击 | 再次点击 |
| `SWAPPING` | 交换动画播放中 | SELECTED+点击相邻格 | 动画结束 |
| `ELIMINATING` | 消除动画播放中 | SWAPPING+有匹配 | 动画结束 |
| `FALLING` | 下落动画播放中 | 消除完成 | 下落到位 |
| `CASCADE` | 下一轮检测 | 下落完成 | 无匹配→IDLE |

---

## 4. PoC 项目结构

```
elimination-poc/
├── Board.cs              # 棋盘数据
├── MatchDetector.cs      # 匹配检测
├── GravitySystem.cs      # 重力下落
├── ComboCounter.cs       # 连锁计数
├── EliminationStateMachine.cs  # 状态机
├── EliminationController.cs    # 主控制器
├── CellView.cs           # UI显示（Unity MonoBehaviour）
└── POCRunner.cs          # 自动化测试
```

---

## 5. 核心算法要点

### 5.1 匹配规则
- **3连起消**：水平或垂直方向连续3个相同元素即匹配
- **L形**：两个匹配段在交叉点垂直+水平都≥3，触发双倍技能
- **T形**：同上，交叉点为T字中心

### 5.2 连锁流程
```
玩家交换 → 检测匹配 → 消除动画 → 下落 → 填位 → 再检测
                                                ↑
                                          循环直到无匹配
```

### 5.3 Combo计数
- 每次触发新匹配连锁 +1
- 回合结束归零
- UI实时显示Combo数（×1, ×2, ×3...）

---

**文档状态：** ✅ 完成  
**待验证：** 实际Unity运行时需美术提供6种元素图标及消除特效