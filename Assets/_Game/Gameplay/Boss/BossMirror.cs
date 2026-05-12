using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay.Boss
{
    /// <summary>
    /// 镜魔 — 镜像复制机制Boss
    /// 
    /// 机制：
    /// - 镜像能量：每回合+10，最大100，≥50时HP≤40%可释放镜像洪流
    /// - 镜花水月：每3回合复制玩家上一张卡牌，效果减半
    /// - 碎裂镜像：场上每有1个特殊棋子，Boss受到5点反噬
    /// - 镜影步：50%闪避，闪避成功后下次攻击+100%
    /// - 阶段二（HP≤40%）：镜像洪流（全体50伤害），真实形态（清负面），每回合能量+20
    /// </summary>
    public class BossMirror : BossBase
    {
        // ── 镜像能量 ────────────────────────────────────────────────
        [SerializeField] private int mirrorEnergyMax = 100;
        [SerializeField] private int mirrorEnergyPerTurn = 10;
        private int _mirrorEnergy = 0;

        // ── 闪避 ───────────────────────────────────────────────────
        private bool _mirrorStepActive = false;
        private bool _mirrorStepDodged = false;
        private bool _nextAttackEmpowered = false;
        private const float DodgeChancePhase1 = 0.5f;

        // ── 混乱效果 ──────────────────────────────────────────────
        private int _confusedCellsPhase1 = 3;
        private int _confusedCellsPhase2 = 5;

        // ── 上次复制记录 ──────────────────────────────────────────
        private CardMatch.Gameplay.CardSystem.CardData _lastPlayerCard = null;

        // ── 事件 ──────────────────────────────────────────────────
        public event System.Action<int> OnMirrorEnergyChanged;    // 能量变化
        public event System.Action<int> OnMirrorFloodTriggered;  // 镜像洪流触发
        public event System.Action<string> OnMirrorCopied;       // 镜花水月触发，参数：卡牌名
        public event System.Action<int> OnMirrorReflectDamage;  // 碎裂镜像反噬伤害

        // ── Properties ─────────────────────────────────────────
        public int MirrorEnergy => _mirrorEnergy;
        public int MirrorEnergyMax => mirrorEnergyMax;

        protected override void Start()
        {
            base.Start();
            bossName = "镜魔";
            maxHp = 250;
            currentHp = 250;
            armor = 0;
            attack = 30;
            mirrorEnergyMax = 100;
            mirrorEnergyPerTurn = 10;

            ResetSpecialAttackQueue();
        }

        public override bool Tick(int playerDamageFromMatching)
        {
            if (_isDead) return false;

            _turnCount++;

            // 镜像能量增长（阶段二加速）
            int perTurn = (_phase == 2) ? mirrorEnergyPerTurn + 10 : mirrorEnergyPerTurn;
            _mirrorEnergy = Mathf.Min(mirrorEnergyMax, _mirrorEnergy + perTurn);
            OnMirrorEnergyChanged?.Invoke(_mirrorEnergy);

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

            // 普攻（含镜影步加成）
            IssueNormalAttack();

            // 碎裂镜像：场上每有1个特殊棋子Boss受到5点反噬
            int reflectDamage = GetReflectDamage();
            if (reflectDamage > 0)
            {
                TakeDamage(reflectDamage);
                OnMirrorReflectDamage?.Invoke(reflectDamage);
            }

            return pauseForAnimation;
        }

        protected override void IssueNormalAttack()
        {
            int baseDmg = attack;
            if (_phase == 2) baseDmg = Mathf.RoundToInt(baseDmg * 1.3f);
            if (_nextAttackEmpowered) baseDmg *= 2;

            OnAttackIssued?.Invoke($"普攻 {baseDmg}");

            if (_nextAttackEmpowered) _nextAttackEmpowered = false;
        }

        protected override void ExecuteSpecialAttack(BossSpecialAttack attack)
        {
            switch (attack.AttackName)
            {
                case "镜花水月":
                    OnMirrorCopied?.Invoke(_lastPlayerCard != null ? _lastPlayerCard.CardName : "未知");
                    break;

                case "镜影步":
                    // 闪避判定在外部（玩家攻击时）进行，这里只是记录
                    _mirrorStepActive = true;
                    break;

                case "镜魔乱舞":
                    OnAttackIssued?.Invoke($"镜魔乱舞 全体35");
                    break;

                case "镜像洪流":
                    _mirrorEnergy = 0;
                    OnMirrorEnergyChanged?.Invoke(_mirrorEnergy);
                    OnMirrorFloodTriggered?.Invoke(50);
                    break;

                case "真实形态":
                    // 清除自身负面状态，由外部调用
                    break;

                default:
                    OnAttackIssued?.Invoke(attack.AttackName);
                    break;
            }
        }

        /// <summary>
        /// 记录玩家打出的卡牌，用于镜花水月复制
        /// </summary>
        public void RecordPlayerCard(CardMatch.Gameplay.CardSystem.CardData card)
        {
            _lastPlayerCard = card;
        }

        /// <summary>
        /// 检查是否闪避成功（玩家攻击时调用）
        /// </summary>
        public bool TryMirrorDodge()
        {
            float chance = (_phase == 2) ? DodgeChancePhase1 * 0.5f : DodgeChancePhase1;
            bool dodged = Random.value < chance;
            if (dodged)
            {
                _nextAttackEmpowered = true;
                _mirrorStepDodged = true;
            }
            else
            {
                _mirrorStepDodged = false;
            }
            return dodged;
        }

        /// <summary>
        /// 获取碎裂镜像反噬伤害（由外部棋盘状态决定）
        /// </summary>
        public int GetReflectDamage(int specialPieceCount = 0)
        {
            return specialPieceCount * 5;
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
            EnqueueSpecialAttack(new BossSpecialAttack("镜花水月", 3, 0, false, "复制玩家卡牌效果减半"));
            EnqueueSpecialAttack(new BossSpecialAttack("镜影步", 2, 0, false, "闪避下次攻击"));
            EnqueueSpecialAttack(new BossSpecialAttack("镜魔乱舞", 5, 35, true, "全体35伤害+混乱"));
        }

        private void ResetSpecialAttackQueuePhase2()
        {
            _specialAttackQueue.Clear();
            EnqueueSpecialAttack(new BossSpecialAttack("镜像洪流", 2, 50, true, "消耗50能量全体50伤害"));
            EnqueueSpecialAttack(new BossSpecialAttack("镜花水月", 2, 0, false, "复制玩家卡牌"));
            EnqueueSpecialAttack(new BossSpecialAttack("镜影步", 2, 0, false, "闪避下次攻击"));
            EnqueueSpecialAttack(new BossSpecialAttack("真实形态", 1, 0, false, "清除负面状态"));
            EnqueueSpecialAttack(new BossSpecialAttack("镜魔乱舞", 3, 35, true, "全体35伤害+混乱"));
        }
    }
}
