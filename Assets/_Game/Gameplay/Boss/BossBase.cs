using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay.Boss
{
    /// <summary>
    /// Boss基类 — 所有Boss的公共父类
    /// 提供：两阶段切换 / 血条管理 / 特殊攻击队列 / 回合Tick / UI通知
    /// </summary>
    public abstract class BossBase : MonoBehaviour
    {
        // ── 基本属性 ──────────────────────────────────────────────
        [Header("基础属性")]
        [SerializeField] protected string bossName = "Unknown Boss";
        [SerializeField] protected int maxHp = 100;
        [SerializeField] protected int currentHp = 100;
        [SerializeField] protected int armor = 0;
        [SerializeField] protected int attack = 10;

        // ── 两阶段 ──────────────────────────────────────────────
        [Header("两阶段机制")]
        [SerializeField] protected float phaseTwoThreshold = 0.5f; // HP ≤ 50% 触发阶段二
        protected int _phase = 1;
        protected bool _phaseTwoTriggered = false;

        // ── 特殊攻击队列 ────────────────────────────────────────
        protected Queue<BossSpecialAttack> _specialAttackQueue = new Queue<BossSpecialAttack>();
        protected BossSpecialAttack _currentSpecialAttack = null;
        protected int _turnsUntilNextSpecial = 0;

        // ── 状态 ────────────────────────────────────────────────
        protected bool _isDead = false;
        protected int _shield = 0;
        protected int _turnCount = 0;

        // ── 事件 ────────────────────────────────────────────────
        public event Action<string> OnAttackIssued;       // (attackName)
        public event Action<int, int> OnHpChanged;        // (currentHp, maxHp)
        public event Action OnPhaseChanged;               // 阶段切换
        public event Action OnBossDied;
        public event Action<int> OnShieldChanged;         // shield value

        // ── Properties ─────────────────────────────────────────
        public string BossName => bossName;
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public int Armor => armor;
        public int Attack => attack;
        public int Phase => _phase;
        public bool IsDead => _isDead;
        public int Shield => _shield;

        // ── 通用 API ────────────────────────────────────────────

        /// <summary>对Boss造成伤害（可由玩家卡牌或消除触发）</summary>
        public virtual void TakeDamage(int damage)
        {
            if (_isDead) return;

            int actualDamage = Mathf.Max(1, damage - armor);
            if (_shield > 0)
            {
                int absorbed = Mathf.Min(_shield, actualDamage);
                _shield -= absorbed;
                actualDamage -= absorbed;
                OnShieldChanged?.Invoke(_shield);
            }

            currentHp = Mathf.Max(0, currentHp - actualDamage);
            OnHpChanged?.Invoke(currentHp, maxHp);

            Debug.Log($"[Boss] {bossName} 受到 {actualDamage} 伤害，剩余HP: {currentHp}/{maxHp}");

            TryTriggerPhaseTwo();
            CheckDeath();
        }

        /// <summary>获得护盾</summary>
        public virtual void AddShield(int amount)
        {
            _shield += amount;
            OnShieldChanged?.Invoke(_shield);
            Debug.Log($"[Boss] {bossName} 获得 {amount} 护盾，当前护盾: {_shield}");
        }

        /// <summary>清除护盾</summary>
        public virtual void ClearShield()
        {
            _shield = 0;
            OnShieldChanged?.Invoke(_shield);
        }

        /// <summary>恢复生命</summary>
        public virtual void Heal(int amount)
        {
            if (_isDead) return;
            currentHp = Mathf.Min(maxHp, currentHp + amount);
            OnHpChanged?.Invoke(currentHp, maxHp);
        }

        /// <summary>设置HP（用于特殊机制如时间逆转）</summary>
        public virtual void SetHp(int hp)
        {
            currentHp = Mathf.Max(0, Mathf.Min(maxHp, hp));
            OnHpChanged?.Invoke(currentHp, maxHp);
        }

        /// <summary>增加特殊攻击到队列</summary>
        protected void EnqueueSpecialAttack(BossSpecialAttack attack)
        {
            _specialAttackQueue.Enqueue(attack);
        }

        /// <summary>
        /// 回合Tick — 每回合由 BattleFlowManager 调用
        /// 返回值：是否需要暂停玩家操作（如需播放动画）
        /// </summary>
        public virtual bool Tick(int playerDamageFromMatching)
        {
            if (_isDead) return false;

            _turnCount++;

            // 处理特殊攻击队列
            if (_currentSpecialAttack == null && _specialAttackQueue.Count > 0)
            {
                _currentSpecialAttack = _specialAttackQueue.Dequeue();
                _turnsUntilNextSpecial = _currentSpecialAttack.TurnsRemaining;
            }

            bool pauseForAnimation = false;
            if (_currentSpecialAttack != null)
            {
                _turnsUntilNextSpecial--;
                if (_turnsUntilNextSpecial <= 0)
                {
                    // 执行特殊攻击
                    ExecuteSpecialAttack(_currentSpecialAttack);
                    _currentSpecialAttack = null;
                    pauseForAnimation = true;
                }
            }

            // 触发Boss回合普攻
            IssueNormalAttack();

            return pauseForAnimation;
        }

        /// <summary>Boss普攻 — 对玩家造成固定伤害</summary>
        protected virtual void IssueNormalAttack()
        {
            int dmg = attack;
            if (_phase == 2) dmg = Mathf.RoundToInt(dmg * GetPhaseTwoDamageMultiplier());
            OnAttackIssued?.Invoke($"普攻 {dmg}");
            Debug.Log($"[Boss] {bossName} 普攻造成 {dmg} 伤害");
        }

        /// <summary>执行特殊攻击</summary>
        protected virtual void ExecuteSpecialAttack(BossSpecialAttack attack)
        {
            OnAttackIssued?.Invoke(attack.AttackName);
            Debug.Log($"[Boss] {bossName} 发动特殊攻击: {attack.AttackName}");
        }

        /// <summary>检查是否进入第二阶段</summary>
        protected virtual void TryTriggerPhaseTwo()
        {
            if (_phaseTwoTriggered) return;
            float hpRatio = (float)currentHp / maxHp;
            if (hpRatio <= phaseTwoThreshold)
            {
                _phaseTwoTriggered = true;
                _phase = 2;
                OnPhaseChanged?.Invoke();
                Debug.Log($"[Boss] {bossName} 进入第二阶段！");
                OnEnterPhaseTwo();
            }
        }

        /// <summary>进入第二阶段的额外初始化（子类Override）</summary>
        protected virtual void OnEnterPhaseTwo() { }

        /// <summary>阶段二的伤害倍率（子类可Override）</summary>
        protected virtual float GetPhaseTwoDamageMultiplier() => 1.0f;

        /// <summary>检查死亡</summary>
        protected virtual void CheckDeath()
        {
            if (currentHp <= 0 && !_isDead)
            {
                _isDead = true;
                currentHp = 0;
                OnBossDied?.Invoke();
                Debug.Log($"[Boss] {bossName} 被击败！");
            }
        }

        /// <summary>获取HP百分比（0~1）</summary>
        public float GetHpRatio() => maxHp > 0 ? (float)currentHp / maxHp : 0f;

        /// <summary>获取阶段（1或2）</summary>
        public int GetPhase() => _phase;
    }

    /// <summary>
    /// Boss特殊攻击描述结构
    /// </summary>
    [Serializable]
    public class BossSpecialAttack
    {
        public string AttackName;
        public int TurnsRemaining;  // 多少回合后执行
        public int Damage;          // 伤害值（0表示无伤害）
        public bool IsAreaOfEffect; // 是否AOE
        public string Description;

        public BossSpecialAttack() { }

        public BossSpecialAttack(string name, int turns, int damage = 0, bool isAoe = false, string desc = "")
        {
            AttackName = name;
            TurnsRemaining = turns;
            Damage = damage;
            IsAreaOfEffect = isAoe;
            Description = desc;
        }
    }
}
