using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay.MatchSystem
{
    /// <summary>
    /// 8×8 棋盘管理器 — 负责棋盘初始化、宝石生成、格子状态查询
    /// </summary>
    public class GridManager
    {
        public const int Width  = 8;
        public const int Height = 8;

        private Piece[,] _grid = new Piece[Width, Height];

        public event Action<Piece> OnPieceCreated;

        /// <summary>获取指定坐标的棋子，无坐标或越界返回 null</summary>
        public Piece Get(int x, int y) =>
            InBounds(x, y) ? _grid[x, y] : null;

        /// <summary>设置指定坐标的棋子</summary>
        public void Set(int x, int y, Piece piece)
        {
            if (!InBounds(x, y)) return;
            _grid[x, y] = piece;
            if (piece != null)
            {
                piece.X = x;
                piece.Y = y;
            }
        }

        public bool InBounds(int x, int y) =>
            x >= 0 && x < Width && y >= 0 && y < Height;

        /// <summary>
        /// 初始化全屏棋盘，随机生成 6 种元素，
        /// 同时保证不存在初始匹配（3连及以上）
        /// </summary>
        public void Initialize()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var type = GenerateValidElement(x, y);
                    var piece = new Piece { Type = type, State = PieceState.Idle, X = x, Y = y };
                    Set(x, y, piece);
                    OnPieceCreated?.Invoke(piece);
                }
            }
        }

        /// <summary>
        /// 生成在 (x,y) 位置不会导致 3 连的元素类型
        /// </summary>
        private ElementType GenerateValidElement(int x, int y)
        {
            var forbidden = new HashSet<ElementType>();

            // 检查左侧 2 格
            if (x >= 2)
            {
                var left1 = Get(x - 1, y)?.Type ?? ElementType.None;
                var left2 = Get(x - 2, y)?.Type ?? ElementType.None;
                if (left1 == left2 && left1 != ElementType.None)
                    forbidden.Add(left1);
            }

            // 检查下方 2 格
            if (y >= 2)
            {
                var down1 = Get(x, y - 1)?.Type ?? ElementType.None;
                var down2 = Get(x, y - 2)?.Type ?? ElementType.None;
                if (down1 == down2 && down1 != ElementType.None)
                    forbidden.Add(down1);
            }

            var pool = new List<ElementType>();
            for (int i = 1; i <= 6; i++)
                if (!forbidden.Contains((ElementType)i))
                    pool.Add((ElementType)i);

            return pool.Count > 0
                ? pool[UnityEngine.Random.Range(0, pool.Count)]
                : ElementType.TypeA;
        }

        /// <summary>遍历所有棋子</summary>
        public IEnumerable<Piece> AllPieces()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (_grid[x, y] != null)
                        yield return _grid[x, y];
        }

        /// <summary>清空某格（用于消除）</summary>
        public void Clear(int x, int y)
        {
            if (InBounds(x, y))
                _grid[x, y] = null;
        }

        /// <summary>整列塌陷：把 column 列的非空棋子移动到底部，返回受影响的棋子列表</summary>
        public List<(Piece piece, int fromY, int toY)> CollapseColumn(int x)
        {
            var moves = new List<(Piece, int, int)>();
            int writeY = 0;
            for (int readY = 0; readY < Height; readY++)
            {
                var piece = Get(x, readY);
                if (piece != null && piece.State != PieceState.Eliminating)
                {
                    if (readY != writeY)
                    {
                        Set(x, writeY, piece);
                        Set(x, readY, null);
                        piece.Y = writeY;
                        moves.Add((piece, readY, writeY));
                    }
                    writeY++;
                }
            }
            return moves;
        }

        /// <summary>在 column 列顶部填充新棋子，返回新产生的棋子列表</summary>
        public List<Piece> FillColumn(int x)
        {
            var filled = new List<Piece>();
            for (int y = 0; y < Height; y++)
            {
                if (Get(x, y) == null)
                {
                    var piece = new Piece
                    {
                        Type  = (ElementType)UnityEngine.Random.Range(1, 7),
                        State = PieceState.Idle,
                        X     = x,
                        Y     = y
                    };
                    Set(x, y, piece);
                    filled.Add(piece);
                    OnPieceCreated?.Invoke(piece);
                }
            }
            return filled;
        }
    }
}
