using System;
using System.Collections.Generic;
using CardMatch.Gameplay.CardSystem;
using CardMatch.Gameplay.MatchSystem;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Combo 系统 — 实时追踪连续消除次数，管理火花充能与终极技能激活
    ///
    /// 核心规则：
    /// - Combo = 一次回合内连续消除的次数（每次 cascade 触发计 +1）
    /// - Spark（火花）= 蓄力机制，Combo 达到 3+ 时充能
    /// - 火花累积达 3 格 → 激活手牌中的"终极技能"卡（一次性）
    /// - 每回合结束时 Combo 重置，但 Spark 保留（不清零）
    /// </summary>
    public class ComboManager
    {
        // ── 状态 ────────────────────────────────────────────────
        private int _currentCombo;    // 当前连锁数
        private int _sparkLevel;      // 火花等级（0-3，3级激活终极技能）
        private int _sparkCharge;     // 火花充能进度（达到阈值后升级）

        // 火花升级阈值：每 3 Combo 充一级
        private const int SPARK_CHARGE_PER_LEVEL = 3;
        private const int MAX_SPARK_LEVEL        = 3;

        // 终极技能已激活列表（本回合内不会重复激活同一张卡）
        private readonly HashSet<string> _activatedUltimateIds = new HashSet<string>();

        // ── 事件 ──────────────────────────────────────────────
        public event Action<int>     OnComboChanged;       // combo 数变化
        public event Action<int>     OnSparkCharged;       // 火花升级 (newLevel)
        public event Action          OnSparkMaxed;         // 火花达到 3 级
        public event Action<CardData> OnUltimateActivated; // 终极技能激活
        public event Action          OnComboReset;         // Combo 重置（每回合）

        // ── 公开属性 ───────────────────────────────────────────
        public int CurrentCombo   => _currentCombo;
        public int SparkLevel     => _sparkLevel;
        public int SparkCharge    => _sparkCharge;
        public int SparkThreshold => SPARK_CHARGE_PER_LEVEL;
        public bool IsSparkMaxed  => _sparkLevel >= MAX_SPARK_LEVEL;

        /// <summary>是否已激活过某终极技能卡（防止同一卡重复触发）</summary>
        public bool IsUltimateUsed(string cardId) => _activatedUltimateIds.Contains(cardId);

        // ── API ───────────────────────────────────────────────

        /// <summary>
        /// 当 CascadeController 检测到一次消除时调用
        /// 传入本次消除的棋子数（用于判断是否触发火花充能）
        /// </summary>
        public void RecordElimination(int pieceCount)
        {
            _currentCombo++;
            OnComboChanged?.Invoke(_currentCombo);

            // Combo ≥ 3 → 火花充能
            if (_currentCombo >= SPARK_CHARGE_PER_LEVEL)
            {
                ChargeSpark(_currentCombo);
            }
        }

        /// <summary>
        /// 手动增加火花等级（由 BidirectionalLinker 或 BattleFlowManager 调用）
        /// </summary>
        public void ChargeSpark(int comboCount)
        {
            if (comboCount < SPARK_CHARGE_PER_LEVEL) return;

            // 计算本次可充能的级数（每次3连充1级）
            int charges = comboCount / SPARK_CHARGE_PER_LEVEL;
            int oldLevel = _sparkLevel;

            for (int i = 0; i < charges; i++)
            {
                if (_sparkLevel < MAX_SPARK_LEVEL)
                {
                    _sparkLevel++;
                    _sparkCharge = 0;
                    OnSparkCharged?.Invoke(_sparkLevel);

                    if (IsSparkMaxed)
                    {
                        OnSparkMaxed?.Invoke();
                    }
                }
            }

            Debug.Log($"[ComboManager] Spark charged: {_sparkLevel}/{MAX_SPARK_LEVEL} (combo={comboCount})");
        }

        /// <summary>
        /// 尝试激活终极技能（返回被激活的卡牌，若火花不足返回 null）
        /// 由 BattleFlowManager 在 Combo ≥ 3 且有 Ultimate 卡时调用
        /// </summary>
        public CardData TryActivateUltimate(IEnumerable<CardData> handCards)
        {
            if (!IsSparkMaxed) return null;

            foreach (var card in handCards)
            {
                if (card.CardType == ECardType.Utility && !_activatedUltimateIds.Contains(card.CardId))
                {
                    _activatedUltimateIds.Add(card.CardId);
                    _sparkLevel = 0;
                    _sparkCharge = 0;
                    OnUltimateActivated?.Invoke(card);
                    Debug.Log($"[ComboManager] Ultimate activated: {card.CardName}");
                    return card;
                }
            }
            return null;
        }

        /// <summary>
        /// 每回合开始时调用，重置 Combo（不清 Spark）
        /// </summary>
        public void ResetCombo()
        {
            int old = _currentCombo;
            _currentCombo = 0;
            if (old > 0)
                OnComboReset?.Invoke();
        }

        /// <summary>
        /// 每场战斗开始时调用，完全重置所有状态
        /// </summary>
        public void ResetAll()
        {
            _currentCombo = 0;
            _sparkLevel   = 0;
            _sparkCharge  = 0;
            _activatedUltimateIds.Clear();
        }

        /// <summary>
        /// 获取当前 Combo 等级描述（用于 UI 显示）
        /// </summary>
        public string GetComboGrade()
        {
            if (_currentCombo >= 10) return "SSS";
            if (_currentCombo >= 7)  return "SS";
            if (_currentCombo >= 5)  return "S";
            if (_currentCombo >= 4)  return "A";
            if (_currentCombo >= 3)  return "B";
            if (_currentCombo >= 2)  return "C";
            return "D";
        }

        /// <summary>
        /// 获取火花充能百分比（用于进度条显示）
        /// </summary>
        public float GetSparkProgress()
        {
            // 当火花已满时返回 1.0
            if (IsSparkMaxed) return 1f;
            return _sparkCharge / (float)SPARK_CHARGE_PER_LEVEL;
        }

        /// <summary>
        /// 获取 Combo 预期能量倍率（Combo 越高伤害/能量倍数越高）
        /// </summary>
        public float GetComboMultiplier()
        {
            if (_currentCombo >= 10) return 5.0f;
            if (_currentCombo >= 7)   return 3.5f;
            if (_currentCombo >= 5)   return 2.5f;
            if (_currentCombo >= 4)   return 2.0f;
            if (_currentCombo >= 3)   return 1.5f;
            if (_currentCombo >= 2)   return 1.2f;
            return 1.0f;
        }
    }
}
