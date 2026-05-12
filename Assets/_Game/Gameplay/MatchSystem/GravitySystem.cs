using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay.MatchSystem
{
    /// <summary>
    /// 重力下落系统 — 处理列塌陷、空中填补、新棋子生成
    /// </summary>
    public class GravitySystem
    {
        /// <summary>
        /// 对整张棋盘执行重力下落，返回所有移动信息
        /// </summary>
        public GravityResult ApplyGravity(GridManager grid)
        {
            var result = new GravityResult();

            for (int x = 0; x < GridManager.Width; x++)
            {
                // 步骤1：列塌陷 — 把非空棋子向下挤
                var columnMoves = grid.CollapseColumn(x);
                foreach (var (piece, fromY, toY) in columnMoves)
                    result.AddMove(piece, x, fromY, toY);

                // 步骤2：填补空洞 — 在顶部生成新棋子
                var filled = grid.FillColumn(x);
                foreach (var piece in filled)
                    result.AddNewPiece(piece, x, piece.Y);
            }

            return result;
        }

        /// <summary>
        /// 对单列执行重力下落（用于局部刷新优化）
        /// </summary>
        public GravityResult ApplyGravityToColumn(GridManager grid, int x)
        {
            var result = new GravityResult();
            var columnMoves = grid.CollapseColumn(x);
            foreach (var (piece, fromY, toY) in columnMoves)
                result.AddMove(piece, x, fromY, toY);

            var filled = grid.FillColumn(x);
            foreach (var piece in filled)
                result.AddNewPiece(piece, x, piece.Y);

            return result;
        }
    }

    /// <summary>
    /// 重力下落的移动结果数据包装
    /// </summary>
    public class GravityResult
    {
        /// <summary>从 (fromX, fromY) 移动到 (toX, toY) 的棋子</summary>
        public List<MoveInfo> Moves { get; } = new List<MoveInfo>();

        /// <summary>新生成的棋子及其落位</summary>
        public List<Piece> NewPieces { get; } = new List<Piece>();

        public void AddMove(Piece piece, int toX, int fromY, int toY)
        {
            Moves.Add(new MoveInfo(piece, piece.X, fromY, toX, toY));
        }

        public void AddNewPiece(Piece piece, int x, int y)
        {
            NewPieces.Add(piece);
            Moves.Add(new MoveInfo(piece, x, y, x, y) { IsNew = true });
        }
    }

    public struct MoveInfo
    {
        public Piece Piece { get; }
        public int FromX { get; }
        public int FromY { get; }
        public int ToX   { get; }
        public int ToY   { get; }
        public bool IsNew { get; }

        public MoveInfo(Piece piece, int fromX, int fromY, int toX, int toY)
        {
            Piece  = piece;
            FromX  = fromX;
            FromY  = fromY;
            ToX    = toX;
            ToY    = toY;
            IsNew  = false;
        }

        /// <summary>下落格数（用于动画时长计算）</summary>
        public int FallDistance => Mathf.Abs(ToY - FromY);
    }
}
