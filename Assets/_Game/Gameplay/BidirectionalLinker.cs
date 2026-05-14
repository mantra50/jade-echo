using System;
using System.Collections.Generic;
using System.Linq;
using CardMatch.Gameplay.CardSystem;
using CardMatch.Gameplay.MatchSystem;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// 双向联动核心 — 桥接 MatchSystem（消除）和 CardSystem（卡牌）
    ///
    /// 联动方向 A（消除 → 卡牌）：
    ///   MatchDetector 检测到 4连/L/T 时，收集匹配信息并暂存为待触发卡牌事件
    ///
    /// 联动方向 B（卡牌 → 消除）：
    ///   CardExecutor 执行后，在指定位置生成特殊格子（炸弹/变色）改变棋盘
    ///
    /// 连锁触发：
    ///   Combo ≥ 3 时，额外触发已装备卡牌效果
    ///
    /// 本类为单例，供 BattleFlowManager 和 CascadeController 访问
    /// </summary>
    public class BidirectionalLinker
    {
        // ── 单例 ────────────────────────────────────────────────
        public static BidirectionalLinker Instance { get; } = new BidirectionalLinker();

        private BidirectionalLinker() { }

        // ── 状态 ────────────────────────────────────────────────

        /// <summary>暂存待触发的卡牌联动事件</summary>
        private readonly List<CardTriggerEvent> _pendingTriggers = new List<CardTriggerEvent>();

        /// <summary>当前已生成的特殊格子列表</summary>
        private readonly List<SpecialCell> _specialCells = new List<SpecialCell>();

        // ── 事件定义 ────────────────────────────────────────────

        /// <summary>
        /// 当消除触发卡牌效果时抛出（供 View 层高亮显示）
        /// </summary>
        public event Action<CardTriggerEvent> OnTriggerQueued;

        /// <summary>
        /// 当卡牌执行后生成特殊格子时抛出
        /// </summary>
        public event Action<SpecialCell> OnSpecialCellCreated;

        /// <summary>
        /// 当特殊格子被消费（爆炸/变色）时抛出
        /// </summary>
        public event Action<SpecialCell, List<Piece>> OnSpecialCellConsumed;

        // ── 公开属性 ────────────────────────────────────────────

        /// <summary>是否存在待处理的卡牌触发事件</summary>
        public bool HasPendingCardTriggers => _pendingTriggers.Count > 0;

        /// <summary>获取所有待触发事件（消费后清空）</summary>
        public IReadOnlyList<CardTriggerEvent> PendingTriggers =>
            _pendingTriggers.ToList().AsReadOnly();

        // ── 方向 A：消除 → 卡牌 ────────────────────────────────

        /// <summary>
        /// 由 CascadeController 在每次消除后调用
        /// 分析本次消除是否构成 4连/L/T，生成对应卡牌触发事件
        /// </summary>
        public void OnPiecesEliminated(
            List<Piece> pieces,
            MatchDetector matcher,
            GridManager grid)
        {
            if (pieces == null || pieces.Count == 0) return;

            // 检测是否包含 4+ 连
            var lineClears = matcher.FindLineClears(grid);
            bool hasLineClear = pieces.Any(p => lineClears.Contains(p));

            // 检测 L / T 形特殊匹配
            var specials = matcher.FindSpecialMatches(grid);
            bool hasSpecial = pieces.Any(p => specials.Contains(p));

            // 统计各元素数量，找最多者作为触发目标
            var elementCounts = CountElements(pieces);
            ElementType dominant = elementCounts.OrderByDescending(kv => kv.Value).First().Key;

            // 找本次匹配的中心点（用于卡牌效果定位）
            int centerX = (int)pieces.Average(p => p.X);
            int centerY = (int)pieces.Average(p => p.Y);

            if (hasLineClear)
            {
                // 4+ 连：触发属性强化卡
                _pendingTriggers.Add(new CardTriggerEvent
                {
                    TriggerType   = TriggerType.LineClear4Plus,
                    Element       = dominant,
                    CenterX       = centerX,
                    CenterY       = centerY,
                    SourcePieces   = pieces,
                    Description    = $"4+ 连消除，属性：{dominant}"
                });
                OnTriggerQueued?.Invoke(_pendingTriggers[_pendingTriggers.Count - 1]);
            }

            if (hasSpecial)
            {
                // L / T 形：触发属性爆发卡
                var specialPieces = pieces.Where(p => specials.Contains(p)).ToList();
                _pendingTriggers.Add(new CardTriggerEvent
                {
                    TriggerType   = TriggerType.SpecialLT,
                    Element       = dominant,
                    CenterX       = centerX,
                    CenterY       = centerY,
                    SourcePieces   = specialPieces,
                    Description    = $"L/T 形特殊消除，属性：{dominant}"
                });
                OnTriggerQueued?.Invoke(_pendingTriggers[_pendingTriggers.Count - 1]);
            }
        }

        /// <summary>
        /// 刷新所有待处理触发事件（BattleFlowManager 消费后调用）
        /// </summary>
        public List<CardTriggerEvent> FlushPendingTriggers()
        {
            var result = new List<CardTriggerEvent>(_pendingTriggers);
            _pendingTriggers.Clear();
            return result;
        }

        // ── 方向 B：卡牌 → 棋盘（特殊格子） ───────────────────

        /// <summary>
        /// 由 CardExecutor 执行后调用，在指定位置生成特殊格子
        /// </summary>
        public void OnCardExecuted(CardData card, List<Piece> affected, GridManager grid)
        {
            if (card == null || affected == null || affected.Count == 0) return;

            switch (card.CardType)
            {
                case ECardType.Attack:
                    // 指定元素 → 生成爆炸区；否则生成变色炸弹
                    if (card.TargetElement != ElementType.None)
                        CreateBlastZone(affected, grid);
                    else
                        CreateColorBomb AtCenter(affected, grid);
                    break;

                case ECardType.Transform:
                    // 洗牌后全屏随机，不生成特殊格子
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 在指定坐标生成"炸弹"特殊格子
        /// 下一轮消除时自动触发（消除周围3×3）
        /// </summary>
        public void CreateBomb(int x, int y, ElementType convertTo, GridManager grid)
        {
            if (!grid.InBounds(x, y)) return;

            var existing = grid.Get(x, y);
            if (existing != null)
            {
                // 替换为特殊棋子，保留原类型供炸弹效果用
                existing.Type = convertTo;
                existing.State = PieceState.Idle;
            }

            var cell = new SpecialCell
            {
                X          = x,
                Y          = y,
                CellType   = SpecialCellType.Bomb,
                ConvertTo  = convertTo,
                SourcePiece = existing
            };
            _specialCells.Add(cell);
            OnSpecialCellCreated?.Invoke(cell);
        }

        /// <summary>
        /// 在指定坐标生成"变色"特殊格子
        /// 下一轮被消除时自动转为指定元素
        /// </summary>
        public void CreateColorChanger(int x, int y, ElementType convertTo, GridManager grid)
        {
            if (!grid.InBounds(x, y)) return;

            var existing = grid.Get(x, y);
            if (existing != null)
                existing.Type = convertTo;

            var cell = new SpecialCell
            {
                X          = x,
                Y          = y,
                CellType   = SpecialCellType.ColorChanger,
                ConvertTo  = convertTo,
                SourcePiece = existing
            };
            _specialCells.Add(cell);
            OnSpecialCellCreated?.Invoke(cell);
        }

        // ── 特殊格子消费 ────────────────────────────────────────

        /// <summary>
        /// 当特殊格子被消除时调用，由 CascadeController 触发
        /// 返回因特殊格子效果而额外消除的棋子
        /// </summary>
        public List<Piece> ConsumeSpecialCell(Piece piece, GridManager grid)
        {
            var extra = new List<Piece>();

            var cell = _specialCells.FirstOrDefault(c => c.X == piece.X && c.Y == piece.Y);
            if (cell == null) return extra;

            switch (cell.CellType)
            {
                case SpecialCellType.Bomb:
                    // 3×3 范围爆炸
                    extra = GetBombArea(cell.X, cell.Y, grid);
                    foreach (var p in extra)
                        p.State = PieceState.Eliminating;
                    OnSpecialCellConsumed?.Invoke(cell, extra);
                    break;

                case SpecialCellType.ColorChanger:
                    // 变色：不消除，但转化周围同色棋子
                    var neighbors = GetNeighbors(cell.X, cell.Y, grid);
                    foreach (var n in neighbors)
                        if (n.Type == cell.ConvertTo)
                            n.State = PieceState.Eliminating;
                    OnSpecialCellConsumed?.Invoke(cell, neighbors);
                    break;
            }

            _specialCells.Remove(cell);
            return extra;
        }

        /// <summary>获取 (cx, cy) 周围的同色邻居（用于变色效果）</summary>
        private List<Piece> GetNeighbors(int cx, int cy, GridManager grid)
        {
            var result = new List<Piece>();
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (dx != 0 || dy != 0)
                    {
                        var p = grid.Get(cx + dx, cy + dy);
                        if (p != null) result.Add(p);
                    }
            return result;
        }

        /// <summary>获取炸弹范围 3×3 内的所有棋子</summary>
        private List<Piece> GetBombArea(int cx, int cy, GridManager grid)
        {
            var result = new List<Piece>();
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    var p = grid.Get(cx + dx, cy + dy);
                    if (p != null) result.Add(p);
                }
            return result;
        }

        // ── 私有辅助 ────────────────────────────────────────────

        /// <summary>在受影响区域中心生成变色炸弹</summary>
        private void CreateColorBomb(List<Piece> affected, GridManager grid)
        {
            int cx = (int)affected.Average(p => p.X);
            int cy = (int)affected.Average(p => p.Y);
            cx = Mathf.Clamp(cx, 0, GridManager.Width  - 1);
            cy = Mathf.Clamp(cy, 0, GridManager.Height - 1);

            // 以本次消除最多的元素为变色目标
            var counts = CountElements(affected);
            var dominant = counts.OrderByDescending(kv => kv.Value).First().Key;

            CreateColorChanger(cx, cy, dominant, grid);
        }

        /// <summary>在受影响最大聚集区生成爆炸区</summary>
        private void CreateBlastZone(List<Piece> affected, GridManager grid)
        {
            if (affected.Count == 0) return;

            // 找最多元素类型
            var counts = CountElements(affected);
            var dominant = counts.OrderByDescending(kv => kv.Value).First().Key;

            // 在棋盘中央放置炸弹
            int cx = GridManager.Width  / 2;
            int cy = GridManager.Height / 2;
            CreateBomb(cx, cy, dominant, grid);
        }

        /// <summary>统计各元素数量</summary>
        private Dictionary<ElementType, int> CountElements(List<Piece> pieces)
        {
            var dict = new Dictionary<ElementType, int>();
            foreach (var p in pieces)
            {
                if (p.Type == ElementType.None) continue;
                if (!dict.ContainsKey(p.Type)) dict[p.Type] = 0;
                dict[p.Type]++;
            }
            return dict;
        }
    }

    // ══════════════════════════════════════════════════════════════
    // 数据结构
    // ══════════════════════════════════════════════════════════════

    /// <summary>卡牌触发事件</summary>
    public class CardTriggerEvent
    {
        public TriggerType  TriggerType { get; set; }
        public ElementType  Element     { get; set; }
        public int          CenterX     { get; set; }
        public int          CenterY     { get; set; }
        public List<Piece>  SourcePieces { get; set; }
        public string       Description { get; set; }
    }

    public enum TriggerType
    {
        None          = 0,
        LineClear4Plus = 1,  // 4+ 连消除
        SpecialLT     = 2,    // L/T 形特殊消除
        ComboChain    = 3     // Combo ≥ 3 触发
    }

    /// <summary>特殊格子类型</summary>
    public enum SpecialCellType
    {
        None          = 0,
        Bomb          = 1,   // 炸弹（3×3 爆炸）
        ColorChanger  = 2    // 变色（被消除时转为指定元素）
    }

    /// <summary>特殊格子数据</summary>
    public class SpecialCell
    {
        public int             X          { get; set; }
        public int             Y          { get; set; }
        public SpecialCellType CellType   { get; set; }
        public ElementType     ConvertTo  { get; set; }
        public Piece           SourcePiece { get; set; }
    }
}
