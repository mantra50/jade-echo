using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardMatch.Gameplay.CardSystem;
using CardMatch.Gameplay.MatchSystem;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// 战斗主流程管理器
    /// 回合循环：消除阶段 → 卡牌阶段 → 结算阶段
    /// 协调 MatchSystem 与 CardSystem 的执行顺序和事件总线
    /// </summary>
    public class BattleFlowManager
    {
        // ── 组件引用 ──────────────────────────────────────────────
        private readonly GridManager       _grid;
        private readonly MatchDetector    _matcher;
        private readonly CascadeController _cascade;
        private readonly GravitySystem    _gravity;
        private readonly CardExecutor    _cardExecutor;
        private readonly HandManager     _hand;
        private readonly ComboManager     _combo;
        private Boss.BossBase _boss;

        // ── 状态 ──────────────────────────────────────────────────
        private int _turnNumber;
        private int _playerHp   = 100;
        private int _playerEnergy = 30;
        private bool _battleActive;

        // ── 敌人状态 ───────────────────────────────────────────────
        private string _enemyName     = "Unknown";
        private int    _enemyMaxHp   = 50;
        private int    _enemyHp      = 50;
        private int    _enemyAttack  = 5;
        private int    _enemyShield  = 0;
        private int    _piecesPerDamage = 3;  // 每消除N个棋子造成1点伤害
        private int    _maxTurns     = 30;
        private int    _currentTurns = 0;

        // ── 事件 ──────────────────────────────────────────────────
        public event Action<BattlePhase>    OnPhaseChanged;
        public event Action<int>           OnTurnStarted;
        public event Action<int, int>      OnTurnEnded;          // (turn, energyProduced)
        public event Action<int>           OnPlayerDamaged;      // damage amount
        public event Action                 OnBattleFinished;
        public event Action<int, int> OnEnemyHpChanged;   // (currentHp, maxHp)
        public event Action OnBossPhaseChanged;
        public event Action<int> OnBossShieldChanged;
        public event Action<int> OnEnemyAttacked;   // (damage)
        public event Action<int, List<Piece>> OnEliminationOccurred; // (comboCount, pieces)
        public event Action<CardData, List<Piece>> OnCardEffectTriggered;

        /// <summary>当前战斗是否进行中</summary>
        public bool IsBattleActive => _battleActive;

        /// <summary>当前回合数</summary>
        public int TurnNumber => _turnNumber;

        /// <summary>当前玩家生命值</summary>
        public int PlayerHp => _playerHp;

        /// <summary>当前玩家能量</summary>
        public int PlayerEnergy => _playerEnergy;

        // ── Boss 绑定 ──────────────────────────────────────────────
        /// <summary>绑定 Boss 实例（在场景加载时由 GameLauncher 注入）</summary>
        public void SetBoss(Boss.BossBase boss)
        {
            if (_boss != null)
            {
                _boss.OnBossDied -= HandleBossDied;
            }
            _boss = boss;
            if (_boss != null)
            {
                _boss.OnBossDied += HandleBossDied;
            }
        }

        // ── 构造 ──────────────────────────────────────────────────
        public BattleFlowManager(
            GridManager        grid,
            MatchDetector      matcher,
            CascadeController  cascade,
            GravitySystem      gravity,
            CardExecutor       cardExecutor,
            HandManager        hand,
            ComboManager       combo,
            Boss.BossBase      boss = null)
        {
            _grid         = grid;
            _matcher      = matcher;
            _cascade      = cascade;
            _gravity      = gravity;
            _cardExecutor = cardExecutor;
            _hand         = hand;
            _combo        = combo;
            _boss         = boss;

            // 订阅级联事件用于双向联动
            _cascade.OnPiecesEliminated  += HandlePiecesEliminated;
            _cascade.OnComboIncremented  += HandleComboIncremented;
            _cascade.OnCascadeFinished   += HandleCascadeFinished;

            _combo.OnSparkCharged        += HandleSparkCharged;
            _combo.OnUltimateActivated   += HandleUltimateActivated;

            // 订阅 Boss 事件
            if (_boss != null)
            {
                _boss.OnBossDied         += HandleBossDied;
            }
        }

        // ── 公开 API ─────────────────────────────────────────────

        /// <summary>
        /// 加载关卡配置
        /// 在 StartBattle 之前调用，设置敌人属性和战斗参数
        /// </summary>
        public void LoadLevelConfig(LevelConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("[BattleFlow] LevelConfig is null, using defaults");
                return;
            }

            _enemyName     = config.EnemyName;
            _enemyMaxHp    = config.EnemyHp;
            _enemyHp       = config.EnemyHp;
            _enemyAttack   = config.EnemyAttack;
            _enemyShield   = config.EnemyShield;
            _piecesPerDamage = config.PiecesPerDamage;
            _maxTurns      = config.MaxTurns;

            Debug.Log($"[BattleFlow] 关卡加载: {config.LevelName} | 敌人: {config.EnemyName} HP:{config.EnemyHp}");
        }

        /// <summary>开始一场战斗（初始化棋盘 → 第1回合）</summary>
        public async void StartBattle()
        {
            _turnNumber   = 1;
            _playerHp     = 100;
            _playerEnergy = 30;
            _battleActive = true;

            _grid.Initialize();
            _hand.SetEnergy(_playerEnergy);

            // 初始抽卡
            for (int i = 0; i < 5; i++)
                _hand.TryDrawCard();

            ChangePhase(BattlePhase.PlayerInput);
            await Task.Delay(500); // 让棋盘初始化动画完成
        }

        /// <summary>
        /// 玩家在 (x, y) 交换两个相邻棋子
        /// 由 View 层在拖拽结束后调用
        /// </summary>
        public async Task<bool> TrySwapAsync(int x1, int y1, int x2, int y2)
        {
            if (CurrentPhase != BattlePhase.PlayerInput) return false;

            // 简单验证相邻
            if (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) != 1) return false;

            // 执交换
            var p1 = _grid.Get(x1, y1);
            var p2 = _grid.Get(x2, y2);
            if (p1 == null || p2 == null) return false;

            _grid.Set(x1, y1, p2);
            _grid.Set(x2, y2, p1);
            p1.X = x2; p1.Y = y2;
            p2.X = x1; p2.Y = y1;

            // 进入消除阶段
            ChangePhase(BattlePhase.Eliminating);
            await RunEliminationCascadeAsync();

            return true;
        }

        // ── 简化版 Cell Click（单步点击，用于Demo） ─────────────────

        private Piece _selectedPiece;

        /// <summary>
        /// 处理棋盘格子点击（Demo用简化版）
        /// 第一次点击选中，第二次点击相邻格子则交换
        /// </summary>
        public void HandleCellClick(Piece piece)
        {
            if (CurrentPhase != BattlePhase.PlayerInput) return;
            if (piece == null) return;

            if (_selectedPiece == null)
            {
                // 第一次点击：选中
                _selectedPiece = piece;
                piece.State = PieceState.Selected;
                Debug.Log($"[Battle] 选中: {piece}");
            }
            else if (_selectedPiece == piece)
            {
                // 点击同一格子：取消选中
                _selectedPiece.State = PieceState.Idle;
                _selectedPiece = null;
                Debug.Log("[Battle] 取消选中");
            }
            else
            {
                // 第二次点击：尝试交换
                var p1 = _selectedPiece;
                var p2 = piece;
                p1.State = PieceState.Idle;
                _selectedPiece = null;

                // 检查是否相邻
                if (Mathf.Abs(p1.X - p2.X) + Mathf.Abs(p1.Y - p2.Y) == 1)
                {
                    Debug.Log($"[Battle] 交换: {p1} <-> {p2}");
                    _ = TrySwapAsync(p1.X, p1.Y, p2.X, p2.Y);
                }
                else
                {
                    Debug.Log("[Battle] 非相邻格子，重新选中");
                    _selectedPiece = p2;
                    p2.State = PieceState.Selected;
                }
            }
        }

        /// <summary>
        /// 处理玩家出牌（简化版，无目标指定）
        /// </summary>
        public void HandlePlayCard(int handIndex)
        {
            if (CurrentPhase != BattlePhase.PlayerInput) return;

            // 找到第一个非空棋子作为目标（简化）
            for (int x = 0; x < GridManager.Width; x++)
            {
                for (int y = 0; y < GridManager.Height; y++)
                {
                    var piece = _grid.Get(x, y);
                    if (piece != null && piece.Type != ElementType.None)
                    {
                        PlayCard(handIndex, x, y);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 玩家打出卡牌（从手牌选中并指定目标位置）
        /// </summary>
        public CardData PlayCard(int handIndex, int targetX, int targetY)
        {
            if (CurrentPhase != BattlePhase.PlayerInput) return null;

            var card = _hand.TryPlayCard(handIndex);
            if (card == null) return null;

            _playerEnergy -= card.EnergyCost;

            // 执行卡牌效果 → 返回受影响棋子
            var affected = _cardExecutor.ExecuteCard(card, targetX, targetY, _grid);

            // 通知双向联动器：卡牌 → 棋盘
            BidirectionalLinker.Instance.OnCardExecuted(card, affected, _grid);

            OnCardEffectTriggered?.Invoke(card, affected);

            if (affected.Count > 0)
            {
                // 有棋子受影响 → 进入消除阶段
                ChangePhase(BattlePhase.Eliminating);
                _ = RunEliminationCascadeAsync();
            }
            else
            {
                // 无棋盘变化 → 立即进入结算
                ChangePhase(BattlePhase.Settlement);
                RunSettlement();
            }

            return card;
        }

        /// <summary>
        /// 跳过本回合（玩家不操作）
        /// </summary>
        public void SkipTurn()
        {
            if (CurrentPhase != BattlePhase.PlayerInput) return;
            ChangePhase(BattlePhase.Settlement);
            RunSettlement();
        }

        // ── 阶段运行 ─────────────────────────────────────────────

        private async Task RunEliminationCascadeAsync()
        {
            // 重置 Combo 计数
            _combo.Reset();

            // 运行连锁直到无匹配
            await _cascade.RunCascadeAsync();

            // 连锁结束 → 进入卡牌阶段或结算
            if (BidirectionalLinker.Instance.HasPendingCardTriggers)
            {
                ChangePhase(BattlePhase.CardPhase);
                await RunCardTriggersAsync();
            }

            ChangePhase(BattlePhase.Settlement);
            RunSettlement();
        }

        private async Task RunCardTriggersAsync()
        {
            var triggers = BidirectionalLinker.Instance.FlushPendingTriggers();
            foreach (var trigger in triggers)
            {
                // 自动触发已装备对应属性卡牌效果
                var equipped = GetEquippedCardsOfElement(trigger.Element);
                foreach (var card in equipped)
                {
                    var affected = _cardExecutor.ExecuteCard(card, trigger.CenterX, trigger.CenterY, _grid);
                    BidirectionalLinker.Instance.OnCardExecuted(card, affected, _grid);
                    OnCardEffectTriggered?.Invoke(card, affected);
                    await Task.Delay(200);
                }
            }

            // Combo ≥ 3 额外触发已装备终极卡
            if (_combo.CurrentCombo >= 3)
            {
                var ultimates = GetUltimateCards();
                foreach (var card in ultimates)
                {
                    var affected = _cardExecutor.ExecuteCard(card, -1, -1, _grid); // 目标-1表示全局
                    BidirectionalLinker.Instance.OnCardExecuted(card, affected, _grid);
                    OnCardEffectTriggered?.Invoke(card, affected);
                    await Task.Delay(200);
                }
            }
        }

        private void RunSettlement()
        {
            // 能量结算
            int produced = _cascade.EnergyProduced;
            _playerEnergy += produced;
            _hand.SetEnergy(_playerEnergy);

            // 火花充能
            _combo.ChargeSpark(_cascade.ComboCount);

            OnTurnEnded?.Invoke(_turnNumber, produced);

            // ── Boss 回合：普攻 ─────────────────────────────────
            if (_boss != null && !_boss.IsDead)
            {
                // 处理特殊攻击队列
                bool needPause = _boss.Tick(0);

                // 普攻伤害应用给玩家
                int bossDmg = _boss.Attack;
                if (bossDmg > 0)
                {
                    _playerHp = Mathf.Max(0, _playerHp - bossDmg);
                    OnPlayerDamaged?.Invoke(bossDmg);
                    OnEnemyAttacked?.Invoke(bossDmg);
                    Debug.Log($"[BattleFlow] Boss 回合攻击: 玩家受到 {bossDmg} 点伤害，剩余HP: {_playerHp}");
                }

                if (_playerHp <= 0)
                {
                    _battleActive = false;
                    ChangePhase(BattlePhase.Idle);
                    OnBattleFinished?.Invoke();
                    return;
                }
            }

            // 回合数 +1，准备下一回合
            _turnNumber++;
            OnTurnStarted?.Invoke(_turnNumber);

            // 抽卡
            _hand.TryDrawCard();

            ChangePhase(BattlePhase.PlayerInput);
        }

        // ── 事件处理 ─────────────────────────────────────────────

        private void HandlePiecesEliminated(List<Piece> pieces)
        {
            // 通知 View 层播放消除特效
            OnEliminationOccurred?.Invoke(_combo.CurrentCombo, pieces);

            // ── 消除 → Boss 伤害 ──────────────────────────────
            if (_boss != null && !_boss.IsDead && pieces.Count > 0)
            {
                int damage = pieces.Count / _piecesPerDamage;
                if (damage > 0)
                {
                    _boss.TakeDamage(damage);
                    Debug.Log($"[BattleFlow] 消除{pieces.Count}个棋子，对Boss造成 {damage} 点伤害");
                }
            }

            // 通知双向联动器：消除 → 卡牌触发
            BidirectionalLinker.Instance.OnPiecesEliminated(pieces, _matcher, _grid);
        }

        private void HandleComboIncremented(int combo)
        {
            // Combo 变化时通知 UI
        }

        private void HandleCascadeFinished(int finalCombo)
        {
            // 连锁结束，可做收尾处理
        }

        private void HandleSparkCharged(int sparkLevel)
        {
            // 火花升级通知（UI 反馈）
            Debug.Log($"[BattleFlow] Spark charged: Level {sparkLevel}");
        }

        private void HandleUltimateActivated(CardData card)
        {
            Debug.Log($"[BattleFlow] Ultimate activated: {card.CardName}");
        }

        private void HandleBossDied()
        {
            Debug.Log("[BattleFlow] Boss 被击败！");
            _battleActive = false;
            ChangePhase(BattlePhase.Idle);
            OnBattleFinished?.Invoke();
        }

        // ── 辅助 ─────────────────────────────────────────────────

        private void ChangePhase(BattlePhase phase)
        {
            CurrentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        /// <summary>获取指定元素已装备的卡牌列表（示例：取手牌中匹配元素的卡）</summary>
        private List<CardData> GetEquippedCardsOfElement(ElementType element)
        {
            var result = new List<CardData>();
            foreach (var card in _hand.Hand)
            {
                if (card.TargetElement == element)
                    result.Add(card);
            }
            return result;
        }

        /// <summary>获取手牌中所有终极技能卡</summary>
        private List<CardData> GetUltimateCards()
        {
            var result = new List<CardData>();
            foreach (var card in _hand.Hand)
            {
                // 火花达到3级且卡牌类型为全局类
                if (_combo.SparkLevel >= 3 && card.CardType == CardType.EnergyBoost)
                    result.Add(card);
            }
            return result;
        }

        // ── 枚举 ─────────────────────────────────────────────────

        public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Idle;
    }

    public enum BattlePhase
    {
        Idle           = 0,
        PlayerInput    = 1,   // 玩家操作阶段（拖拽/选卡）
        Eliminating    = 2,   // 消除连锁阶段
        CardPhase      = 3,   // 卡牌触发阶段（4消/L/T/Combo触发的自动效果）
        Settlement     = 4    // 回合结算阶段（能量/火花/抽卡）
    }
}
