using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay.Boss
{
    /// <summary>
    /// 裂隙守卫 — 第9关准Boss
    /// 
    /// 机制：
    /// - 冻结格：随机冻结棋盘2格（阶段二4格），使这些格子2回合内无法消除
    /// - 阶段一（HP 100%~50%）：每3回合冻结2格，每4回合召唤裂隙影
    /// - 阶段二（HP ≤ 50%）：狂暴状态，伤害+30%，沙漏加速，冻结4格，护盾壁垒冷却缩短
    /// </summary>
    public class BossGuardian : BossBase
    {
        // ── 冻结格机制 ────────────────────────────────────────────
        private HashSet<int> _frozenCells = new HashSet<int>(); // 序列化棋盘格子索引 (x + y*width)
        private Dictionary<int, int> _frozenTurnsRemaining = new Dictionary<int, int>(); // 每个冻结格剩余回合
        [SerializeField] private int freezeRadius = 2;       // 冻结格数量（阶段一2格，阶段二4格）
        [SerializeField] private int freezeDuration = 2;     // 冻结持续回合数

        // ── 沙漏计时 ──────────────────────────────────────────────
        [SerializeField] private int hourglassCapacity = 8; // 沙漏容量
        private int _hourglassCount = 0;
        private int _hourglassPerTurn = 1;

        // ── 裂隙影召唤 ─────────────────────────────────────────────
        private int _turnsSinceLastSummon = 0;
        private const int SummonIntervalPhase1 = 4;
        private const int SummonIntervalPhase2 = 3;

        // ── 护盾壁垒 ──────────────────────────────────────────────
        private int _shieldCooldown = 0;
        private const int ShieldCooldownPhase1 = 3;
        private const int ShieldCooldownPhase2 = 2;
        private int _shieldDuration = 0;
        private const int ShieldDuration = 2;

        // ── 事件 ──────────────────────────────────────────────────
        public event System.Action<List<int>> OnCellsFrozen;    // 冻结格子列表变化
        public event System.Action OnSummonRiftShadows;         // 召唤裂隙影
        public event System.Action OnRampageActivated;           // 狂暴触发

        // ── Properties ─────────────────────────────────────────
        public IReadOnlyHashSet<int> FrozenCells => _frozenCells;
        public int HourglassCount => _hourglassCount;
        public int HourglassCapacity => hourglassCapacity;

        protected override void Start()
        {
            base.Start();
            bossName = "裂隙守卫";
            maxHp = 200;
            currentHp = 200;
            armor = 30;
            attack = 25;
            hourglassCapacity = 8;

            // 初始特殊攻击队列
            ResetSpecialAttackQueue();
        }

        public override bool Tick(int playerDamageFromMatching)
        {
            if (_isDead) return false;

            _turnCount++;

            // 沙漏计数
            int hourglassPerTurn = (_phase == 2) ? _hourglassPerTurn + 2 : _hourglassPerTurn;
            _hourglassCount = Mathf.Min(hourglassCapacity, _hourglassCount + hourglassPerTurn);

            // 处理冻结格倒计时
            TickFrozenCells();

            // 处理护盾持续时间
            if (_shieldDuration > 0) _shieldDuration--;

            // 处理护盾冷却
            if (_shieldCooldown > 0) _shieldCooldown--;

            // 特殊攻击队列
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

            // 普攻
            IssueNormalAttack();

            return pauseForAnimation;
        }

        private void TickFrozenCells()
        {
            var toRemove = new List<int>();
            foreach (var cell in _frozenCells)
            {
                if (_frozenTurnsRemaining.ContainsKey(cell))
                {
                    _frozenTurnsRemaining[cell]--;
                    if (_frozenTurnsRemaining[cell] <= 0)
                        toRemove.Add(cell);
                }
                else
                {
                    toRemove.Add(cell);
                }
            }
            foreach (var cell in toRemove)
            {
                _frozenCells.Remove(cell);
                _frozenTurnsRemaining.Remove(cell);
            }
            if (toRemove.Count > 0)
                OnCellsFrozen?.Invoke(new List<int>(_frozenCells));
        }

        protected override void IssueNormalAttack()
        {
            int dmg = attack;
            if (_phase == 2) dmg = Mathf.RoundToInt(dmg * 1.3f);
            OnAttackIssued?.Invoke($"普攻 {dmg}");
        }

        protected override void ExecuteSpecialAttack(BossSpecialAttack attack)
        {
            switch (attack.AttackName)
            {
                case "裂隙斩":
                    OnAttackIssued?.Invoke($"裂隙斩 {attack.Damage}");
                    break;
                case "护盾壁垒":
                    AddShield(50);
                    _shieldDuration = ShieldDuration;
                    break;
                case "裂隙召唤":
                    OnSummonRiftShadows?.Invoke();
                    break;
                case "冻结格":
                    FreezeRandomCells(freezeRadius);
                    break;
                case "狂暴":
                    OnRampageActivated?.Invoke();
                    break;
                default:
                    OnAttackIssued?.Invoke($"{attack.AttackName} {attack.Damage}");
                    break;
            }
        }

        private void FreezeRandomCells(int count)
        {
            // 由外部棋盘管理器提供棋盘尺寸，这里用GridManager.Width/Height常量
            int width = 8; // default
            int height = 8;
            var gm = UnityEngine.Object.FindObjectOfType<CardMatch.Gameplay.GridManager>();
            if (gm != null)
            {
                // 反射或直接访问静态宽高
                width = CardMatch.Gameplay.GridManager.Width;
                height = CardMatch.Gameplay.GridManager.Height;
            }

            var available = new List<int>();
            for (int i = 0; i < width * height; i++)
            {
                if (!_frozenCells.Contains(i))
                    available.Add(i);
            }

            int toFreeze = Mathf.Min(count, available.Count);
            for (int i = 0; i < toFreeze; i++)
            {
                int idx = Random.Range(0, available.Count);
                int cell = available[idx];
                _frozenCells.Add(cell);
                _frozenTurnsRemaining[cell] = freezeDuration;
                available.RemoveAt(idx);
            }

            Debug.Log($"[BossGuardian] 冻结 {toFreeze} 格: {string.Join(",", _frozenCells)}");
            OnCellsFrozen?.Invoke(new List<int>(_frozenCells));
        }

        protected override void OnEnterPhaseTwo()
        {
            base.OnEnterPhaseTwo();
            _hourglassPerTurn = 3; // 每回合+3格，加速
            ResetSpecialAttackQueuePhase2();
            OnRampageActivated?.Invoke();
        }

        protected override float GetPhaseTwoDamageMultiplier() => 1.3f;

        private void ResetSpecialAttackQueue()
        {
            _specialAttackQueue.Clear();
            EnqueueSpecialAttack(new BossSpecialAttack("裂隙召唤", SummonIntervalPhase1, 0, false, "召唤2个裂隙影"));
            EnqueueSpecialAttack(new BossSpecialAttack("冻结格", 3, 0, false, "冻结棋盘2格"));
            EnqueueSpecialAttack(new BossSpecialAttack("护盾壁垒", 4, 0, false, "生成50护盾"));
            EnqueueSpecialAttack(new BossSpecialAttack("裂隙斩", 2, 25, false, "单体25伤害+裂伤DOT"));
        }

        private void ResetSpecialAttackQueuePhase2()
        {
            _specialAttackQueue.Clear();
            _shieldCooldown = 0;
            EnqueueSpecialAttack(new BossSpecialAttack("裂隙召唤", SummonIntervalPhase2, 0, false, "召唤2个裂隙影"));
            EnqueueSpecialAttack(new BossSpecialAttack("冻结格", 2, 0, false, "冻结棋盘4格"));
            EnqueueSpecialAttack(new BossSpecialAttack("护盾壁垒", ShieldCooldownPhase2, 0, false, "生成50护盾"));
            EnqueueSpecialAttack(new BossSpecialAttack("裂隙斩", 2, Mathf.RoundToInt(25 * 1.3f), false, "单体25伤害+裂伤DOT"));
            EnqueueSpecialAttack(new BossSpecialAttack("狂暴", 1, 0, false, "触发狂暴"));
        }

        /// <summary>检查某格子是否被冻结</summary>
        public bool IsCellFrozen(int cellIndex) => _frozenCells.Contains(cellIndex);

        /// <summary>手动解冻某格子（被玩家技能解除时调用）</summary>
        public void UnfreezeCell(int cellIndex)
        {
            if (_frozenCells.Contains(cellIndex))
            {
                _frozenCells.Remove(cellIndex);
                _frozenTurnsRemaining.Remove(cellIndex);
                OnCellsFrozen?.Invoke(new List<int>(_frozenCells));
            }
        }

        /// <summary>裂隙影死亡回复玩家HP</summary>
        public void OnRiftShadowKilled()
        {
            // 由外部BattleFlowManager调用，增加玩家回复
            Debug.Log("[BossGuardian] 裂隙影被击杀，回复10HP");
        }
    }
}
