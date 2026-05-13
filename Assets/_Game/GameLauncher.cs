using System;
using System.Threading.Tasks;
using CardMatch.Gameplay;
using CardMatch.Gameplay.CardSystem;
using CardMatch.Gameplay.MatchSystem;
using UnityEngine;

namespace CardMatch
{
    /// <summary>
    /// 🎮 GameLauncher — 游戏入口 / 主菜单
    /// 负责初始化所有子系统并启动战斗流程。
    /// 支持主菜单 → 选择 Chapter1-1 → 战斗 → 胜利/失败界面 → 返回主菜单。
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        // ── 子系统实例 ────────────────────────────────────────────
        private GridManager        _grid;
        private MatchDetector     _matcher;
        private GravitySystem      _gravity;
        private CascadeController  _cascade;
        private ComboManager       _combo;
        private HandManager        _hand;
        private CardExecutor       _cardExecutor;
        private BattleFlowManager  _battleFlow;
        private BidirectionalLinker _linker;

        // ── Boss ────────────────────────────────────────────────
        private Boss.BossBase _currentBoss;

        // ── UI 引用（Unity编辑器内赋值）───────────────────────────
        [Header("UI References")]
        [SerializeField] private Transform boardRoot;
        [SerializeField] private Transform handRoot;
        [SerializeField] private Transform hudRoot;

        // ── 当前关卡配置 ─────────────────────────────────────────
        private LevelConfig _currentLevelConfig;

        // ── 运行时状态 ──────────────────────────────────────────
        private bool _battleActive;

        // ── Unity 生命周期 ──────────────────────────────────────

        private void Start()
        {
            Debug.Log("[Launcher] CardMatch Alpha 启动，等待用户选择关卡...");
            ShowMainMenu();
        }

        // ══════════════════════════════════════════════════════════════
        // 主菜单
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 显示主菜单（当前为 Debug 日志示意，后续对接 UI 主菜单场景）
        /// </summary>
        public void ShowMainMenu()
        {
            _battleActive = false;
            Debug.Log("[Launcher] ═══════════════════════════════");
            Debug.Log("[Launcher]    CardMatch Alpha — Main Menu");
            Debug.Log("[Launcher]    Chapter1-1: 暗影猎手 (HP=15)");
            Debug.Log("[Launcher]    Press [Start Battle] to begin.");
            Debug.Log("[Launcher] ═══════════════════════════════");
        }

        // ══════════════════════════════════════════════════════════════
        // 关卡启动入口
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 开始 Chapter1-1 战斗（由主菜单 UI 按钮调用）
        /// </summary>
        public void StartChapter1Level1()
        {
            // 使用 LevelConfig.CreateChapter1Level1() 创建配置
            _currentLevelConfig = LevelConfig.CreateChapter1Level1();
            Debug.Log($"[Launcher] 开始 Chapter1-1: {_currentLevelConfig.EnemyName}");

            // 初始化所有系统
            InitializeEliminationCore();
            InitializeCardSystem();
            InitializeBattleFlow();
            InitializeLinker();

            // 创建 Chapter1-1 的简单敌人（Shadow Hunter = 直接扣血）
            CreateChapter1_1Enemy();

            // 加载关卡配置到 BattleFlowManager
            _battleFlow.LoadLevelConfig(_currentLevelConfig);
            _battleFlow.SetBoss(_currentBoss);

            // 订阅战斗结束事件
            _battleFlow.OnBattleFinished += HandleBattleFinished;
            _battleFlow.OnPlayerDamaged += dmg => Debug.Log($"[Launcher] 玩家受伤: {dmg}");
            _battleFlow.OnEliminationOccurred += (combo, pieces) =>
                Debug.Log($"[Launcher] 消除! Combo×{combo}, 数量: {pieces.Count}");
            _battleFlow.OnPhaseChanged += phase => Debug.Log($"[Launcher] Phase: {phase}");

            // 启动战斗
            _battleFlow.StartBattle();
            _battleActive = true;

            Debug.Log("[Launcher] 战斗开始！");
        }

        /// <summary>
        /// 创建 Chapter1-1 敌人（暗影猎手 — 简单 HP=15 敌人，无特殊机制）
        /// </summary>
        private void CreateChapter1_1Enemy()
        {
            // Chapter1-1 是序章教学关，敌人没有特殊技能，直接用 BossBase
            var go = new GameObject("ShadowHunter");
            _currentBoss = go.AddComponent<Boss.BossBase>();

            // 通过反射设置私有字段（因为 BossBase 的字段是 protected serialize）
            SetBossField(_currentBoss, "bossName", "暗影猎手");
            SetBossField(_currentBoss, "maxHp", 15);
            SetBossField(_currentBoss, "currentHp", 15);
            SetBossField(_currentBoss, "armor", 0);
            SetBossField(_currentBoss, "attack", 2);

            Debug.Log("[Launcher] 暗影猎手 生成完毕 HP=15 ATK=2");
        }

        private void SetBossField(Boss.BossBase boss, string fieldName, object value)
        {
            var field = typeof(Boss.BossBase).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            if (field != null)
                field.SetValue(boss, value);
        }

        // ══════════════════════════════════════════════════════════════
        // 子系统初始化
        // ══════════════════════════════════════════════════════════════

        private void InitializeEliminationCore()
        {
            _grid    = new GridManager();
            _matcher = new MatchDetector(_grid);
            _gravity = new GravitySystem(_grid);
            _cascade = new CascadeController(_grid, _matcher, _gravity);
            _combo  = new ComboManager();

            _grid.Initialize(); // 8×8 棋盘初始化

            Debug.Log("[Launcher] EliminationCore 初始化完成 (8×8 Grid)");
        }

        private void InitializeCardSystem()
        {
            _hand = new HandManager(maxHandSize: 5);
            _hand.DrawInitialCards(3);
            _cardExecutor = new CardExecutor(_hand, _grid);
            Debug.Log($"[Launcher] CardSystem 初始化完成，手牌数量: {_hand.HandCount}");
        }

        private void InitializeBattleFlow()
        {
            _battleFlow = new BattleFlowManager(
                grid:         _grid,
                matcher:      _matcher,
                cascade:      _cascade,
                gravity:      _gravity,
                cardExecutor: _cardExecutor,
                hand:         _hand,
                combo:        _combo
            );
            Debug.Log("[Launcher] BattleFlowManager 初始化完成");
        }

        private void InitializeLinker()
        {
            _linker = BidirectionalLinker.Instance;
            Debug.Log("[Launcher] BidirectionalLinker 初始化完成");
        }

        // ══════════════════════════════════════════════════════════════
        // 战斗事件处理
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 战斗结束回调（胜利或失败）
        /// </summary>
        private void HandleBattleFinished()
        {
            _battleActive = false;
            bool victory = _currentBoss != null && _currentBoss.IsDead;

            Debug.Log($"[Launcher] ═══════════════════════════════");
            Debug.Log($"[Launcher]    战斗结束 — {(victory ? "胜利！" : "失败")}");
            Debug.Log($"[Launcher]    回合数: {_battleFlow.TurnNumber}");
            Debug.Log($"[Launcher]    玩家剩余HP: {_battleFlow.PlayerHp}");
            Debug.Log($"[Launcher] ═══════════════════════════════");

            if (victory)
            {
                Debug.Log("[Launcher] ✅ 恭喜击败暗影猎手！Chapter1-1 通关！");
                // 后续：显示结算 UI -> 返回主菜单
            }
            else
            {
                Debug.Log("[Launcher] ❌ 玩家HP归零，战斗失败。重试请调用 StartChapter1Level1()");
                // 后续：显示失败 UI -> 返回主菜单
            }
        }

        // ══════════════════════════════════════════════════════════════
        // 公开 API（供 UI 按钮调用）
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 玩家点击棋盘格子（由 BoardUI 调用）
        /// 第一次点击选中，第二次点击相邻格子则交换
        /// </summary>
        public void OnCellClicked(int x, int y)
        {
            if (!_battleActive || _battleFlow == null) return;
            if (_battleFlow.CurrentPhase != BattlePhase.PlayerInput) return;

            var piece = _grid.Get(x, y);
            if (piece != null)
            {
                _battleFlow.HandleCellClick(piece);
            }
        }

        /// <summary>
        /// 玩家使用手牌（由 CardPanelUI 调用）
        /// </summary>
        public void OnPlayCard(int handIndex)
        {
            if (!_battleActive || _battleFlow == null) return;
            if (_battleFlow.CurrentPhase != BattlePhase.PlayerInput) return;
            _battleFlow.HandlePlayCard(handIndex);
        }

        /// <summary>
        /// 玩家点击结束回合按钮（由 HUDUI 调用）
        /// </summary>
        public void EndTurn()
        {
            if (!_battleActive || _battleFlow == null) return;
            _battleFlow.EndTurn();
        }

        // ── Debug 快速测试 ───────────────────────────────────────

        [ContextMenu("Debug: Run Full Battle Simulation")]
        public async void DebugRunBattleSimulation()
        {
            Debug.Log("[Launcher] === 开始战斗模拟 ===");
            StartChapter1Level1();

            // 模拟几轮玩家操作
            await Task.Delay(1000);

            // 模拟交换 (0,0) <-> (1,0)
            Debug.Log("[Launcher] 模拟交换 (0,0) <-> (1,0)");
            await _battleFlow.TrySwapAsync(0, 0, 1, 0);

            await Task.Delay(2000);

            // 模拟交换 (2,0) <-> (2,1)
            Debug.Log("[Launcher] 模拟交换 (2,0) <-> (2,1)");
            await _battleFlow.TrySwapAsync(2, 0, 2, 1);

            await Task.Delay(2000);

            Debug.Log("[Launcher] === 战斗模拟结束 ===");
        }
    }
}