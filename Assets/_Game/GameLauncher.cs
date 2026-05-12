using System;
using System.Threading.Tasks;
using CardMatch.Gameplay;
using CardMatch.Gameplay.CardSystem;
using CardMatch.Gameplay.MatchSystem;
using UnityEngine;

namespace CardMatch
{
    /// <summary>
    /// 🎮 GameLauncher — 游戏入口
    /// 负责初始化所有子系统并启动战斗流程
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        // ── 子系统实例 ────────────────────────────────────────────
        private GridManager        _grid;
        private MatchDetector      _matcher;
        private GravitySystem      _gravity;
        private CascadeController  _cascade;
        private ComboManager       _combo;
        private HandManager        _hand;
        private CardExecutor       _cardExecutor;
        private BattleFlowManager  _battleFlow;
        private BidirectionalLinker _linker;

        // ── UI 引用 ──────────────────────────────────────────────
        [Header("UI References")]
        [SerializeField] private Transform boardRoot;
        [SerializeField] private Transform handRoot;
        [SerializeField] private Transform hudRoot;

        // ── 关卡配置 ─────────────────────────────────────────────
        [Header("Level Config")]
        [SerializeField] private TextAsset chapter1Level1Config;

        // ── Unity 生命周期 ──────────────────────────────────────

        private async void Start()
        {
            Debug.Log("[Launcher] CardMatch Chapter1 Demo 启动中...");

            try
            {
                // Step 1: 初始化消除核心
                InitializeEliminationCore();

                // Step 2: 初始化卡牌系统
                InitializeCardSystem();

                // Step 3: 初始化战斗流程
                InitializeBattleFlow();

                // Step 4: 建立双向联动
                InitializeLinker();

                // Step 5: 加载 Chapter1 Level1 关卡
                LoadLevel();

                // Step 6: 启动战斗
                _battleFlow.StartBattle();

                Debug.Log("[Launcher] 所有系统初始化完成，战斗开始！");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Launcher] 初始化失败: {ex}");
            }
        }

        // ── 初始化步骤 ──────────────────────────────────────────

        /// <summary>
        /// Step 1: 消除核心子系统
        /// </summary>
        private void InitializeEliminationCore()
        {
            _grid     = new GridManager();
            _matcher  = new MatchDetector(_grid);
            _gravity  = new GravitySystem(_grid);
            _cascade  = new CascadeController(_grid, _matcher, _gravity);
            _combo    = new ComboManager();

            // 初始化棋盘
            _grid.Initialize();

            Debug.Log("[Launcher] EliminationCore 初始化完成 (6x6 Grid)");
        }

        /// <summary>
        /// Step 2: 卡牌系统子系统
        /// </summary>
        private void InitializeCardSystem()
        {
            // 创建初始手牌
            _hand = new HandManager(maxHandSize: 5);
            _hand.DrawInitialCards(3);

            // 创建卡牌执行器
            _cardExecutor = new CardExecutor(_hand, _grid);

            Debug.Log($"[Launcher] CardSystem 初始化完成，手牌数量: {_hand.HandCount}");
        }

        /// <summary>
        /// Step 3: 战斗流程子系统
        /// </summary>
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

            // 订阅战斗事件（用于调试/日志）
            _battleFlow.OnPhaseChanged      += phase => Debug.Log($"[Battle] Phase: {phase}");
            _battleFlow.OnPlayerDamaged      += dmg   => Debug.Log($"[Battle] 玩家受伤: {dmg}");
            _battleFlow.OnEliminationOccurred += (combo, pieces) =>
                Debug.Log($"[Battle] 消除! Combo×{combo}, 数量: {pieces.Count}");
            _battleFlow.OnBattleFinished    += ()     => Debug.Log("[Battle] 战斗结束!");

            Debug.Log("[Launcher] BattleFlowManager 初始化完成");
        }

        /// <summary>
        /// Step 4: 建立双向联动（消除 ↔ 卡牌）
        /// </summary>
        private void InitializeLinker()
        {
            _linker = new BidirectionalLinker(
                battleFlow:   _battleFlow,
                cardExecutor: _cardExecutor,
                comboManager: _combo,
                handManager:  _hand
            );

            Debug.Log("[Launcher] BidirectionalLinker 初始化完成");
        }

        /// <summary>
        /// Step 5: 加载关卡配置
        /// </summary>
        private void LoadLevel()
        {
            // 加载 Chapter1 Level1 配置
            var levelConfig = ScriptableObject.CreateInstance<Gameplay.LevelConfig>();
            levelConfig.LoadFromData();

            Debug.Log($"[Launcher] 加载关卡: {levelConfig.LevelName}");
            Debug.Log($"[Launcher] 敌人: {levelConfig.EnemyName}, HP: {levelConfig.EnemyHp}");
            Debug.Log($"[Launcher] 波次: {levelConfig.TotalWaves}");

            // 将关卡配置应用到战斗流程
            _battleFlow.LoadLevelConfig(levelConfig);
        }

        // ── 公开 API（供 UI 按钮调用）─────────────────────────────

        /// <summary>
        /// 玩家点击棋盘格子（由 BoardUI 调用）
        /// </summary>
        public void OnCellClicked(int x, int y)
        {
            if (_battleFlow == null || !_battleFlow.IsBattleActive) return;

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
            if (_battleFlow == null || !_battleFlow.IsBattleActive) return;
            _battleFlow.HandlePlayCard(handIndex);
        }

        /// <summary>
        /// 结束当前回合（由 HUDUI 调用）
        /// </summary>
        public void EndTurn()
        {
            _battleFlow?.EndTurn();
        }
    }
}
