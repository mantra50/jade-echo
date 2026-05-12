using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CardMatch.Gameplay.MatchSystem
{
    /// <summary>
    /// 连锁控制器 — 编排 消除→下落→再检测 的循环，负责 Combo 计数与能量产出
    /// </summary>
    public class CascadeController
    {
        private readonly GridManager    _grid;
        private readonly MatchDetector  _matcher;
        private readonly GravitySystem  _gravity;

        /// <summary>当前 Combo 数（从 1 开始计）</summary>
        public int ComboCount { get; private set; }

        /// <summary>本次操作产生的能量总值</summary>
        public int EnergyProduced { get; private set; }

        /// <summary>连锁阶段结束事件（参数：最终 Combo 数）</summary>
        public event Action<int> OnCascadeFinished;

        /// <summary>每次消除产生的新 Combo 数（用于 UI 更新）</summary>
        public event Action<int> OnComboIncremented;

        /// <summary>消除事件（参数：被消除的棋子列表）</summary>
        public event Action<List<Piece>> OnPiecesEliminated;

        public CascadeController(GridManager grid, MatchDetector matcher, GravitySystem gravity)
        {
            _grid    = grid;
            _matcher = matcher;
            _gravity = gravity;
        }

        /// <summary>
        /// 执行一次完整的连锁流程（异步，协程友好版）
        /// </summary>
        public async Task RunCascadeAsync()
        {
            ComboCount = 0;
            EnergyProduced = 0;

            while (true)
            {
                var matches = _matcher.FindAllMatches(_grid);
                if (matches.Count == 0)
                    break;

                // Combo +1
                ComboCount++;
                OnComboIncremented?.Invoke(ComboCount);

                // 特殊棋子（L/T 形核心）额外奖励
                var specials = _matcher.FindSpecialMatches(_grid);
                foreach (var s in specials)
                    if (!matches.Contains(s))
                        matches.Add(s);

                // 能量产出：3消=10能量，4消+=30，L/T=50
                EnergyProduced += CalculateEnergy(matches, specials);

                // 标记所有匹配棋子为 Eliminating
                foreach (var piece in matches)
                    piece.State = PieceState.Eliminating;

                OnPiecesEliminated?.Invoke(matches);

                // 等待消除动画（0.3s）
                await Task.Delay(300);

                // 物理消除
                foreach (var piece in matches)
                {
                    _grid.Clear(piece.X, piece.Y);
                    // GameObject 销毁由 View 层处理
                }

                // 重力下落
                var gravityResult = _gravity.ApplyGravity(_grid);

                // 等待下落动画（每格 0.2s，最多 1s）
                int maxFall = 0;
                foreach (var m in gravityResult.Moves)
                    if (m.FallDistance > maxFall)
                        maxFall = m.FallDistance;
                int fallMs = Mathf.Min(maxFall * 200, 1000);
                if (fallMs > 0)
                    await Task.Delay(fallMs);
            }

            OnCascadeFinished?.Invoke(ComboCount);
        }

        /// <summary>
        /// 同步版（Unity 协程使用）
        /// 返回下一个要等待的时间（毫秒），0 表示结束
        /// </summary>
        public CascadePhase Step()
        {
            if (ComboCount == 0)
            {
                // 第一次检测
                var matches = _matcher.FindAllMatches(_grid);
                if (matches.Count == 0)
                    return new CascadePhase { Done = true };

                ComboCount = 1;
                OnComboIncremented?.Invoke(ComboCount);
                var specials = _matcher.FindSpecialMatches(_grid);
                EnergyProduced += CalculateEnergy(matches, specials);

                foreach (var piece in matches)
                    piece.State = PieceState.Eliminating;
                OnPiecesEliminated?.Invoke(matches);

                return new CascadePhase
                {
                    Phase       = Phase.Eliminating,
                    Matches     = matches,
                    Specials    = specials,
                    WaitMs      = 300
                };
            }
            else
            {
                // 消除后的下落阶段
                var gravityResult = _gravity.ApplyGravity(_grid);
                int maxFall = 0;
                foreach (var m in gravityResult.Moves)
                    if (m.FallDistance > maxFall)
                        maxFall = m.FallDistance;
                int fallMs = Mathf.Min(maxFall * 200, 1000);

                return new CascadePhase
                {
                    Phase          = Phase.Falling,
                    GravityResult  = gravityResult,
                    WaitMs         = fallMs
                };
            }
        }

        private int CalculateEnergy(List<Piece> matches, List<Piece> specials)
        {
            int energy = 0;
            energy += matches.Count * 10;

            // 4 连及以上额外奖励
            var lineClears = _matcher.FindLineClears(_grid);
            energy += lineClears.Count * 20;

            // L/T 形额外奖励
            energy += specials.Count * 30;

            return energy;
        }
    }

    public enum Phase { Eliminating, Falling }

    public class CascadePhase
    {
        public bool Done { get; set; }
        public Phase Phase { get; set; }
        public List<Piece> Matches { get; set; }
        public List<Piece> Specials { get; set; }
        public GravityResult GravityResult { get; set; }
        public int WaitMs { get; set; }
    }
}
