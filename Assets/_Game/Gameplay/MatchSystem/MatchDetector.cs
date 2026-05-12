using System.Collections.Generic;
using System.Linq;

namespace CardMatch.Gameplay.MatchSystem
{
    /// <summary>
    /// Match3 检测器 — 扫描线算法，支持 3连、4连、L形、T形检测
    /// </summary>
    public class MatchDetector
    {
        /// <summary>返回所有待消除的棋子集合</summary>
        public List<Piece> FindAllMatches(GridManager grid)
        {
            var result = new HashSet<Piece>();

            // 横向检测
            for (int y = 0; y < GridManager.Height; y++)
            {
                int runStart = 0;
                for (int x = 1; x <= GridManager.Width; x++)
                {
                    bool same = x < GridManager.Width
                        && SameType(grid.Get(x, y), grid.Get(runStart, y));

                    if (!same)
                    {
                        int len = x - runStart;
                        if (len >= 3)
                            for (int i = runStart; i < x; i++)
                                result.Add(grid.Get(i, y));
                        runStart = x;
                    }
                }
            }

            // 纵向检测
            for (int x = 0; x < GridManager.Width; x++)
            {
                int runStart = 0;
                for (int y = 1; y <= GridManager.Height; y++)
                {
                    bool same = y < GridManager.Height
                        && SameType(grid.Get(x, y), grid.Get(x, runStart));

                    if (!same)
                    {
                        int len = y - runStart;
                        if (len >= 3)
                            for (int i = runStart; i < y; i++)
                                result.Add(grid.Get(x, i));
                        runStart = y;
                    }
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// 返回特殊匹配（L形、T形）的核心交叉点棋子列表
        /// </summary>
        public List<Piece> FindSpecialMatches(GridManager grid)
        {
            var specials = new HashSet<Piece>();

            // 记录每个位置在横向和纵向的连续段长度
            var hLen = new int[GridManager.Width, GridManager.Height];
            var vLen = new int[GridManager.Width, GridManager.Height];

            // 横向扫描
            for (int y = 0; y < GridManager.Height; y++)
            {
                int runStart = 0;
                for (int x = 1; x <= GridManager.Width; x++)
                {
                    bool same = x < GridManager.Width
                        && SameType(grid.Get(x, y), grid.Get(runStart, y));

                    if (!same)
                    {
                        int len = x - runStart;
                        if (len >= 3)
                            for (int i = runStart; i < x; i++)
                                hLen[i, y] = len;
                        runStart = x;
                    }
                }
            }

            // 纵向扫描
            for (int x = 0; x < GridManager.Width; x++)
            {
                int runStart = 0;
                for (int y = 1; y <= GridManager.Height; y++)
                {
                    bool same = y < GridManager.Height
                        && SameType(grid.Get(x, y), grid.Get(x, runStart));

                    if (!same)
                    {
                        int len = y - runStart;
                        if (len >= 3)
                            for (int i = runStart; i < y; i++)
                                vLen[x, i] = len;
                        runStart = y;
                    }
                }
            }

            // 检测 L/T 形交叉点
            for (int x = 0; x < GridManager.Width; x++)
            {
                for (int y = 0; y < GridManager.Height; y++)
                {
                    if (hLen[x, y] >= 3 && vLen[x, y] >= 3)
                    {
                        // T 形：十字交叉
                        specials.Add(grid.Get(x, y));
                    }
                    else if (hLen[x, y] >= 3 && vLen[x, y] == 2)
                    {
                        // L 形：水平段端点 + 垂直分支
                        bool isLeftEdge  = x == 0 || !SameType(grid.Get(x - 1, y), grid.Get(x, y));
                        bool isRightEdge  = x == GridManager.Width - 1 || !SameType(grid.Get(x + 1, y), grid.Get(x, y));
                        if (isLeftEdge || isRightEdge)
                            specials.Add(grid.Get(x, y));
                    }
                    else if (vLen[x, y] >= 3 && hLen[x, y] == 2)
                    {
                        // 镜像 L 形（垂直段为主）
                        bool isBottomEdge = y == 0 || !SameType(grid.Get(x, y - 1), grid.Get(x, y));
                        bool isTopEdge     = y == GridManager.Height - 1 || !SameType(grid.Get(x, y + 1), grid.Get(x, y));
                        if (isBottomEdge || isTopEdge)
                            specials.Add(grid.Get(x, y));
                    }
                }
            }

            return specials.ToList();
        }

        /// <summary>返回 4 连及以上长度的所有棋子（用于能量/道具触发）</summary>
        public List<Piece> FindLineClears(GridManager grid)
        {
            var result = new HashSet<Piece>();

            // 横向 4+
            for (int y = 0; y < GridManager.Height; y++)
            {
                int runStart = 0;
                for (int x = 1; x <= GridManager.Width; x++)
                {
                    bool same = x < GridManager.Width
                        && SameType(grid.Get(x, y), grid.Get(runStart, y));

                    if (!same)
                    {
                        int len = x - runStart;
                        if (len >= 4)
                            for (int i = runStart; i < x; i++)
                                result.Add(grid.Get(i, y));
                        runStart = x;
                    }
                }
            }

            // 纵向 4+
            for (int x = 0; x < GridManager.Width; x++)
            {
                int runStart = 0;
                for (int y = 1; y <= GridManager.Height; y++)
                {
                    bool same = y < GridManager.Height
                        && SameType(grid.Get(x, y), grid.Get(x, runStart));

                    if (!same)
                    {
                        int len = y - runStart;
                        if (len >= 4)
                            for (int i = runStart; i < y; i++)
                                result.Add(grid.Get(x, i));
                        runStart = y;
                    }
                }
            }

            return result.ToList();
        }

        private static bool SameType(Piece a, Piece b) =>
            a != null && b != null && a.Type != ElementType.None && a.Type == b.Type;
    }
}
