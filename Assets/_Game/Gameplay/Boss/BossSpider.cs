using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay.Boss
{
    /// <summary>
    /// 蜃楼蛛母 — Chapter 1 最终Boss（第10关）
    /// 
    /// 机制：
    /// - 蛛网标记：每次攻击给玩家附加「蛛网标记」，叠满5层触发定身（无法消除棋子）
    /// - 终焉时刻：HP≤20%时Boss进入狂暴状态，所有攻击附带额外蛛网标记
    /// - 蜃楼幻象：每4回合召唤幻象协助战斗
    /// </summary>
    public class BossSpider : BossBase
    {
        // ── 蛛网标记 ────────────────────────────────────────────────
        private int _webMarkCount = 0;
        [SerializeField] private int webMarkMax = 5;
        [SerializeField] private int webDamage = 3; // 每层蛛网每回合伤害

        // ── 定身 ──────────────────────────────────────────────────
        private bool _isPlayerParalyzed = false;
        private int _paralyzeTurnsRemaining = 0;

        // ── 幻象 ──────────────────────────────────────────────────
        private int _turnsSinceLastSummon = 0;
        private const int SummonInterval = 4;
        private const int IllusionHp = 20;
        private const int IllusionAttack = 10;

        // ── 终焉 ──────────────────────────────────────────────────
        private bool _ultimateTriggered = false;

        // ── 事件 ──────────────────────────────────────────────────
        public event Action<int> OnWebMarkChanged;        // 蛛网标记变化
        public event Action OnPlayerParalyzed;           // 玩家被定身
        public event Action OnPlayerUnparalyzed;         // 玩家解除定身
        public event Action OnSummonIllusion;            // 召唤幻象
        public event Action OnUltimateTriggered;          // 终焉时刻触发

        // ── Properties ─────────────────────────────────────────
        public int WebMarkCount => _webMarkCount;
        public int WebMarkMax => webMarkMax;
        public bool IsPlayerParalyzed => _isPlayerParalyzed;

        protected override void Start()
        {
            base.Start();
            bossName = "蜃楼蛛母";
            maxHp = 350;
            currentHp = 350;
            armor = 15;
            attack = 22;
            webMarkMax = 5;

            ResetSpecialAttackQueue();
        }

        public override bool Tick(int playerDamageFromMatching)
        {
            if (_isDead) return false;

            _turnCount++;

            // 处理定身倒计时
            if (_isPlayerParalyzed)
            {
                _paralyzeTurnsRemaining--;
                if (_paralyzeTurnsRemaining <= 0)
                {
                    _isPlayerParalyzed = false;
                    OnPlayerUnparalyzed?.Invoke();
                }
            }

            // 处理特殊攻击队列
            bool pauseForAnimation = false;
            if (_currentSpecialAttack == null && _specialAttackQueue.Count > 0)
            {
                _currentSpecialAttack = _specialAttackQueue.Dequeue();
                _turnsUntilNextSpecial = _currentSpecialAttack.TurnsRemaining;
            }

            if (_currentSpecialAttack != null)
            {
                _turnsUntilNextSpecial--;
                if (_turnsUntilNextSpecial <= 0)
                {
                    ExecuteSpecialAttack(_currentSpecialAttack);
                    _currentSpecialAttack = null;
                    pauseForAnimation = true;
                }
            }

            // 普攻 + 附加蛛网标记
            int dmg = attack;
            if (_phase == 2) dmg = Mathf.RoundToInt(dmg * 1.3f);
            IssueWebAttack(dmg);

            // 蜃楼幻象回合Tick
            _turnsSinceLastSummon++;
            if (_turnsSinceLastSummon >= SummonInterval)
            {
                _turnsSinceLastSummon = 0;
                // 幻象伤害由外部处理，这里只发事件
            }

            // 蛛网DOT每回合伤害
            if (_webMarkCount > 0)
            {
                int dotDamage = _webMarkCount * webDamage;
                OnAttackIssued?.Invoke($"蛛网毒害 -{dotDamage}");
            }

            return pauseForAnimation;
        }

        /// <summary>
        /// 蛛母攻击（带蛛网标记附加）
        /// </summary>
        private void IssueWebAttack(int damage)
        {
            OnAttackIssued?.Invoke($"蜃楼吐丝 {damage}");

            // 附加蛛网标记（阶段二额外+1层）
            int markGain = (_phase == 2) ? 2 : 1;
            AddWebMark(markGain);
        }

        /// <summary>
        /// 增加蛛网标记
        /// </summary>
        public void AddWebMark(int count = 1)
        {
            _webMarkCount = Mathf.Min(webMarkMax, _webMarkCount + count);
            OnWebMarkChanged?.Invoke(_webMarkCount);

            Debug.Log($"[BossSpider] 蛛网标记 +{count}，当前 {_webMarkCount}/{webMarkMax}");

            if (_webMarkCount >= webMarkMax && !_isPlayerParalyzed)
            {
                TriggerParalyze();
            }
        }

        /// <summary>
        /// 触发定身效果
        /// </summary>
        private void TriggerParalyze()
        {
            _isPlayerParalyzed = true;
            _paralyzeTurnsRemaining = 2; // 默认定身2回合
            OnPlayerParalyzed?.Invoke();
            Debug.Log("[BossSpider] 玩家被定身！无法消除棋子");
        }

        /// <summary>
        /// 移除蛛网标记（玩家用净化技能时调用）
        /// </summary>
        public void RemoveWebMark(int count = 1)
        {
            _webMarkCount = Mathf.Max(0, _webMarkCount - count);
            OnWebMarkChanged?.Invoke(_webMarkCount);

            if (_webMarkCount < webMarkMax && _isPlayerParalyzed)
            {
                _isPlayerParalyzed = false;
                _paralyzeTurnsRemaining = 0;
                OnPlayerUnparalyzed?.Invoke();
                Debug.Log("[BossSpider] 蛛网标记清除，玩家解除定身");
            }
        }

        protected override void ExecuteSpecialAttack(BossSpecialAttack attack)
        {
            switch (attack.AttackName)
            {
                case "蜃楼幻象":
                    OnSummonIllusion?.Invoke();
                    break;
                case "终焉时刻":
                    TriggerUltimate();
                    break;
                default:
                    OnAttackIssued?.Invoke(attack.AttackName);
                    break;
            }
        }

        /// <summary>
        /// 触发终焉时刻 — 阶段二强化
        /// </summary>
        private void TriggerUltimate()
        {
            if (_ultimateTriggered) return;
            _ultimateTriggered = true;
            OnUltimateTriggered?.Invoke();
            Debug.Log("[BossSpider] 终焉时刻触发！蜃楼蛛母进入狂暴状态");
        }

        protected override void OnEnterPhaseTwo()
        {
            base.OnEnterPhaseTwo();
            ResetSpecialAttackQueuePhase2();
        }

        protected override float GetPhaseTwoDamageMultiplier() => 1.3f;

        private void ResetSpecialAttackQueue()
        {
            _specialAttackQueue.Clear();
            EnqueueSpecialAttack(new BossSpecialAttack("蜃楼幻象", SummonInterval, 0, false, "召唤幻象协助战斗"));
        }

        private void ResetSpecialAttackQueuePhase2()
        {
            _specialAttackQueue.Clear();
            EnqueueSpecialAttack(new BossSpecialAttack("终焉时刻", 1, 0, false, "触发终焉狂暴"));
            EnqueueSpecialAttack(new BossSpecialAttack("蜃楼幻象", SummonInterval - 1, 0, false, "加速召唤幻象"));
        }

        /// <summary>
        /// 获取当前蛛网DOT每回合伤害
        /// </summary>
        public int GetWebDotDamage() => _webMarkCount * webDamage;

        /// <summary>
        /// 幻象相关属性（供外部调用）
        /// </summary>
        public int IllusionAttackValue => IllusionAttack;
    }
}
