using System;
using UnityEngine;

namespace CardMatch.Gameplay.Boss
{
    /// <summary>
    /// 时间沙漏 — 最终Boss（第10关）
    /// 
    /// 机制：
    /// - 时间沙漏：每回合+1，叠满10格触发「时间逆转」（全单位HP/状态回战斗开始）
    /// - 阶段一（HP 100%~20%）：每回合沙漏+1，每3回合瞬间加速（跳过棋盘操作），
    ///   每4回合时间停滞（冻结棋盘1回合），每5回合时光回溯（随机删2张手牌）
    /// - 阶段二（HP≤20%）：触发「终焉时刻」消耗全部沙漏对全体造成50点无视护盾伤害，
    ///   攻击力+50%，每回合沙漏+2，沙暴来袭伤害提升
    /// </summary>
    public class BossHourglass : BossBase
    {
        // ── 时间沙漏 ────────────────────────────────────────────────
        [SerializeField] private int hourglassCapacity = 10;
        private int _hourglassCount = 0;
        private int _hourglassPerTurn = 1;
        private int _hourglassPerTurnPhase2 = 2;

        // ── 阶段二终焉 ─────────────────────────────────────────────
        private bool _ultimateTriggered = false;
        [SerializeField] private int ultimateDamage = 50;

        // ── 事件 ──────────────────────────────────────────────────
        public event Action<int> OnHourglassChanged;     // 沙漏格数变化
        public event Action OnTimeRewindTriggered;      // 时间逆转触发
        public event Action OnTimeStopTriggered;        // 时间停滞触发
        public event Action OnUltimateTriggered;         // 终焉时刻触发
        public event Action<int> OnCardDiscarded;       // 删卡数量

        // ── Properties ─────────────────────────────────────────
        public int HourglassCount => _hourglassCount;
        public int HourglassCapacity => hourglassCapacity;
        public bool IsHourglassFull => _hourglassCount >= hourglassCapacity;

        protected override void Start()
        {
            base.Start();
            bossName = "时间沙漏";
            maxHp = 400;
            currentHp = 400;
            armor = 20;
            attack = 20;
            hourglassCapacity = 10;
            _hourglassCount = 0;
            _hourglassPerTurn = 1;

            ResetSpecialAttackQueue();
        }

        public override bool Tick(int playerDamageFromMatching)
        {
            if (_isDead) return false;

            _turnCount++;

            // 沙漏计数
            int perTurn = (_phase == 2) ? _hourglassPerTurnPhase2 : _hourglassPerTurn;
            _hourglassCount = Mathf.Min(hourglassCapacity, _hourglassCount + perTurn);
            OnHourglassChanged?.Invoke(_hourglassCount);

            // 检查时间逆转（叠满10格）
            if (_hourglassCount >= hourglassCapacity && _phase == 1)
            {
                TriggerTimeRewind();
                _hourglassCount = 0;
                OnHourglassChanged?.Invoke(_hourglassCount);
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

            // 普攻（含阶段二强化）
            IssueNormalAttack();

            return pauseForAnimation;
        }

        protected override void IssueNormalAttack()
        {
            int dmg = attack;
            if (_phase == 2) dmg = Mathf.RoundToInt(dmg * 1.5f); // 攻击力+50%
            OnAttackIssued?.Invoke($"普攻 {dmg}");
        }

        protected override void ExecuteSpecialAttack(BossSpecialAttack attack)
        {
            switch (attack.AttackName)
            {
                case "瞬间加速":
                    OnAttackIssued?.Invoke($"瞬间加速 30");
                    break;

                case "时间停滞":
                    OnTimeStopTriggered?.Invoke();
                    break;

                case "时光回溯":
                    // 随机删2张手牌，由外部BattleFlowManager处理
                    OnCardDiscarded?.Invoke(2);
                    break;

                case "沙暴来袭":
                    int sandstormDamage = (_phase == 2) ? 30 : 20;
                    OnAttackIssued?.Invoke($"沙暴来袭 全体{sandstormDamage}");
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
        /// 触发时间逆转 — 所有单位HP/状态回到战斗开始
        /// </summary>
        private void TriggerTimeRewind()
        {
            Debug.Log("[BossHourglass] 时间逆转触发！所有单位HP和状态回复");
            OnTimeRewindTriggered?.Invoke();
            // 具体HP回复由外部BattleFlowManager处理
        }

        /// <summary>
        /// 触发终焉时刻 — 消耗全部沙漏对全体造成50点无视护盾伤害
        /// </summary>
        private void TriggerUltimate()
        {
            if (_ultimateTriggered) return;
            _ultimateTriggered = true;

            _hourglassCount = 0;
            OnHourglassChanged?.Invoke(_hourglassCount);
            OnUltimateTriggered?.Invoke();

            Debug.Log($"[BossHourglass] 终焉时刻！对全体造成 {ultimateDamage} 点无视护盾伤害");
            // 无视护盾伤害由外部BattleFlowManager处理
        }

        protected override void OnEnterPhaseTwo()
        {
            base.OnEnterPhaseTwo();
            _hourglassPerTurn = _hourglassPerTurnPhase2;
            ResetSpecialAttackQueuePhase2();
        }

        protected override float GetPhaseTwoDamageMultiplier() => 1.5f;

        private void ResetSpecialAttackQueue()
        {
            _specialAttackQueue.Clear();
            EnqueueSpecialAttack(new BossSpecialAttack("瞬间加速", 3, 30, false, "跳过玩家回合造成30伤害"));
            EnqueueSpecialAttack(new BossSpecialAttack("时间停滞", 4, 0, false, "冻结棋盘1回合"));
            EnqueueSpecialAttack(new BossSpecialAttack("时光回溯", 5, 0, false, "随机删除玩家2张手牌"));
            EnqueueSpecialAttack(new BossSpecialAttack("沙暴来袭", 1, 20, true, "全体20伤害"));
        }

        private void ResetSpecialAttackQueuePhase2()
        {
            _specialAttackQueue.Clear();
            EnqueueSpecialAttack(new BossSpecialAttack("终焉时刻", 1, 0, false, "消耗沙漏全体50伤害"));
            EnqueueSpecialAttack(new BossSpecialAttack("瞬间加速", 2, 30, false, "跳过玩家回合造成30伤害"));
            EnqueueSpecialAttack(new BossSpecialAttack("时间停滞", 3, 0, false, "冻结棋盘1回合"));
            EnqueueSpecialAttack(new BossSpecialAttack("沙暴来袭", 1, 30, true, "全体30伤害+每负面状态+8"));
            EnqueueSpecialAttack(new BossSpecialAttack("时光回溯", 4, 0, false, "随机删除玩家2张手牌"));
        }

        /// <summary>
        /// 外部设置沙漏值（用于时间逆转后恢复）
        /// </summary>
        public void SetHourglass(int value)
        {
            _hourglassCount = Mathf.Clamp(value, 0, hourglassCapacity);
            OnHourglassChanged?.Invoke(_hourglassCount);
        }

        /// <summary>
        /// 获取沙漏百分比（0~1）
        /// </summary>
        public float GetHourglassRatio() => hourglassCapacity > 0 ? (float)_hourglassCount / hourglassCapacity : 0f;
    }
}
