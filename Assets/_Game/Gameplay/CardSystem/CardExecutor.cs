using System;
using System.Collections.Generic;
using CardMatch.Gameplay.MatchSystem;
using UnityEngine;

namespace CardMatch.Gameplay.CardSystem
{
    /// <summary>
    /// 卡牌效果执行器 — 将 CardData 的效果应用到 MatchSystem
    /// 
    /// 提供的接口：
    /// - ExecuteCard(card, targetX, targetY, grid) — 执行卡牌效果
    /// - GetAffectedPieces(card, targetX, targetY, grid) — 获取受影响棋子（预览用）
    /// </summary>
    public class CardExecutor
    {
        /// <summary>
        /// 执行卡牌效果，返回受影响并将被消除的棋子列表（用于触发连锁）
        /// </summary>
        /// <param name="card">要打出的卡牌</param>
        /// <param name="targetX">目标坐标 X（卡牌指定位置）</param>
        /// <param name="targetY">目标坐标 Y</param>
        /// <param name="grid">棋盘引用</param>
        /// <returns>本次效果将消除的棋子列表（可传入 CascadeController 触发连锁）</returns>
        public List<Piece> ExecuteCard(CardData card, int targetX, int targetY, GridManager grid)
        {
            if (card == null) return new List<Piece>();

            switch (card.CardType)
            {
                case CardType.ClearRow:
                    return ExecuteClearRow(targetY, grid);

                case CardType.ClearCol:
                    return ExecuteClearCol(targetX, grid);

                case CardType.ClearArea:
                    return ExecuteClearArea(targetX, targetY, card.Range, grid);

                case CardType.Bomb:
                    return ExecuteBomb(card.TargetElement, grid);

                case CardType.Shuffle:
                    return ExecuteShuffle(grid);

                case CardType.Swap:
                    // Swap 需要两个坐标，暂不支持，跳过
                    return new List<Piece>();

                case CardType.EnergyBoost:
                    // 能量直接加，不涉及棋子消除，这里返回空列表
                    return new List<Piece>();

                case CardType.Heal:
                    // 回血不涉及棋盘，返回空列表
                    return new List<Piece>();

                default:
                    return new List<Piece>();
            }
        }

        /// <summary>
        /// 预览卡牌效果（不实际执行，用于 UI 高亮显示）
        /// </summary>
        public List<Piece> GetAffectedPieces(CardData card, int targetX, int targetY, GridManager grid)
        {
            return ExecuteCard(card, targetX, targetY, grid);
        }

        // ============================================================
        // 效果实现（私有）
        // ============================================================

        /// <summary>清除一整行</summary>
        private List<Piece> ExecuteClearRow(int rowY, GridManager grid)
        {
            var pieces = new List<Piece>();
            for (int x = 0; x < GridManager.Width; x++)
            {
                var p = grid.Get(x, rowY);
                if (p != null) pieces.Add(p);
            }
            return pieces;
        }

        /// <summary>清除一整列</summary>
        private List<Piece> ExecuteClearCol(int colX, GridManager grid)
        {
            var pieces = new List<Piece>();
            for (int y = 0; y < GridManager.Height; y++)
            {
                var p = grid.Get(colX, y);
                if (p != null) pieces.Add(p);
            }
            return pieces;
        }

        /// <summary>清除以 (cx, cy) 为中心、半径为 range 的方形区域</summary>
        private List<Piece> ExecuteClearArea(int cx, int cy, int range, GridManager grid)
        {
            var pieces = new List<Piece>();
            // range=1 → 3×3（含中心），range=2 → 5×5，以此类推
            int half = Mathf.Max(1, range);
            for (int dx = -half; dx <= half; dx++)
            {
                for (int dy = -half; dy <= half; dy++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (grid.InBounds(x, y))
                    {
                        var p = grid.Get(x, y);
                        if (p != null) pieces.Add(p);
                    }
                }
            }
            return pieces;
        }

        /// <summary>消除场上所有指定元素的棋子</summary>
        private List<Piece> ExecuteBomb(ElementType element, GridManager grid)
        {
            var pieces = new List<Piece>();
            foreach (var piece in grid.AllPieces())
            {
                if (piece.Type == element)
                    pieces.Add(piece);
            }
            return pieces;
        }

        /// <summary>重置棋盘（随机打乱所有棋子，返回旧棋子列表用于消除连锁）</summary>
        private List<Piece> ExecuteShuffle(GridManager grid)
        {
            var pieces = new List<Piece>();
            // 收集所有现有棋子
            foreach (var p in grid.AllPieces())
                pieces.Add(p);

            // 为每个位置重新随机生成（排除当前类型以确保变化）
            for (int x = 0; x < GridManager.Width; x++)
            {
                for (int y = 0; y < GridManager.Height; y++)
                {
                    var oldPiece = grid.Get(x, y);
                    if (oldPiece == null) continue;

                    // 生成一个不同的元素
                    var newType = GetShuffledType(oldPiece.Type);
                    oldPiece.Type = newType;
                }
            }
            return pieces; // 洗牌不触发消除，棋盘内容有变化但无消除
        }

        private ElementType GetShuffledType(ElementType current)
        {
            var pool = new List<ElementType>();
            for (int i = 1; i <= 6; i++)
            {
                var t = (ElementType)i;
                if (t != current) pool.Add(t);
            }
            return pool.Count > 0
                ? pool[UnityEngine.Random.Range(0, pool.Count)]
                : ElementType.TypeA;
        }

        // ============================================================
        // 辅助：物理消除（外部调用时用）
        // ============================================================

        /// <summary>
        /// 将棋子列表标记为 Eliminating 并从棋盘物理清除（供 View 层调用）
        /// </summary>
        public void ApplyElimination(List<Piece> pieces, GridManager grid)
        {
            foreach (var piece in pieces)
            {
                piece.State = PieceState.Eliminating;
                grid.Clear(piece.X, piece.Y);
            }
        }
    }
}