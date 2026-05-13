// ============================================================
// BattleUIConnector.cs — 战斗 UI 事件总线桥接
// 将 BattleFlowManager 的事件连接到各个 UI 面板
// ============================================================

using CardMatch.Gameplay;
using CardMatch.Gameplay.MatchSystem;
using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.UI
{
    /// <summary>
    /// 桥接 BattleFlowManager 事件与各 UI 面板的响应。
    /// 挂载在 BattleUI Root 或 GameLauncher 上。
    /// </summary>
    public class BattleUIConnector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleFlowManager _battleFlow;

        [Header("Optional — Hook to existing components")]
        [SerializeField] private bool _autoFindBattleFlow = true;

        // 统计用
        private int _totalDamageDealt;
        private int _maxComboThisBattle;
        private int _cardsPlayedThisBattle;

        private void Start()
        {
            if (_autoFindBattleFlow && _battleFlow == null)
            {
                // 尝试从场景中找到 BattleFlowManager
                var bfm = FindObjectOfType<BattleFlowManager>();
                if (bfm != null) _battleFlow = bfm;
            }

            if (_battleFlow == null)
            {
                Debug.LogWarning("[BattleUIConnector] BattleFlowManager not found, disabling.");
                enabled = false;
                return;
            }

            SubscribeToBattleFlow();
        }

        private void OnDestroy()
        {
            UnsubscribeFromBattleFlow();
        }

        private void SubscribeToBattleFlow()
        {
            var bfm = _battleFlow;

            bfm.OnEnemyHpChanged       += HandleEnemyHpChanged;
            bfm.OnPlayerDamaged        += HandlePlayerDamaged;
            bfm.OnEnemyAttacked        += HandleEnemyAttacked;
            bfm.OnEliminationOccurred += HandleEliminationOccurred;
            bfm.OnCardEffectTriggered += HandleCardEffectTriggered;
            bfm.OnTurnEnded           += HandleTurnEnded;
            bfm.OnTurnStarted         += HandleTurnStarted;
            bfm.OnBattleFinished      += HandleBattleFinished;
            bfm.OnBossPhaseChanged    += HandleBossPhaseChanged;
            bfm.OnBossShieldChanged   += HandleBossShieldChanged;
        }

        private void UnsubscribeFromBattleFlow()
        {
            if (_battleFlow == null) return;
            var bfm = _battleFlow;

            bfm.OnEnemyHpChanged       -= HandleEnemyHpChanged;
            bfm.OnPlayerDamaged        -= HandlePlayerDamaged;
            bfm.OnEnemyAttacked        -= HandleEnemyAttacked;
            bfm.OnEliminationOccurred -= HandleEliminationOccurred;
            bfm.OnCardEffectTriggered -= HandleCardEffectTriggered;
            bfm.OnTurnEnded           -= HandleTurnEnded;
            bfm.OnTurnStarted         -= HandleTurnStarted;
            bfm.OnBattleFinished      -= HandleBattleFinished;
            bfm.OnBossPhaseChanged    -= HandleBossPhaseChanged;
            bfm.OnBossShieldChanged   -= HandleBossShieldChanged;
        }

        // ── HP / Damage Handlers ──────────────────────────────

        private void HandleEnemyHpChanged(int currentHp, int maxHp)
        {
            if (BossHPUI.Instance != null)
                BossHPUI.Instance.UpdateHP(currentHp, maxHp);

            // 显示伤害数字
            if (DamagePopup.Instance != null)
            {
                var anchor = FindBossAnchor();
                DamagePopup.Instance.ShowBossDamage(maxHp - currentHp, anchor);
            }

            // 累计伤害
            _totalDamageDealt += (maxHp - currentHp);
        }

        private void HandlePlayerDamaged(int damage)
        {
            if (HUDUI.Instance != null)
            {
                var hp = _battleFlow.PlayerHp;
                HUDUI.Instance.SetPlayerHP(hp, 100); // TODO: 从配置读取 maxHP
            }

            if (DamagePopup.Instance != null)
                DamagePopup.Instance.ShowDamage(damage);
        }

        private void HandleEnemyAttacked(int damage)
        {
            if (HUDUI.Instance != null)
            {
                var hp = _battleFlow.PlayerHp;
                HUDUI.Instance.SetPlayerHP(hp, 100);
            }
        }

        // ── Elimination / Combo Handlers ──────────────────────

        private void HandleEliminationOccurred(int comboCount, List<Piece> pieces)
        {
            if (comboCount > _maxComboThisBattle)
                _maxComboThisBattle = comboCount;

            // 播放消除特效
            if (BoardUI.Instance != null)
            {
                var cells = new List<Vector2Int>();
                foreach (var p in pieces)
                    cells.Add(new Vector2Int(p.X, p.Y));
                BoardUI.Instance.PlayMatchVFX(cells);
            }

            // 伤害数字弹出
            if (DamagePopup.Instance != null)
            {
                var anchor = FindBoardAnchor();
                DamagePopup.Instance.ShowDamage(pieces.Count, anchor);
            }
        }

        private void HandleCardEffectTriggered(CardData card, List<Piece> affected)
        {
            _cardsPlayedThisBattle++;

            if (DamagePopup.Instance != null && affected.Count > 0)
            {
                var anchor = FindBoardAnchor();
                DamagePopup.Instance.ShowEnergy(card.EnergyCost, anchor);
            }
        }

        // ── Turn Handlers ─────────────────────────────────────

        private void HandleTurnStarted(int turn)
        {
            if (HUDUI.Instance != null)
            {
                var energy = _battleFlow.PlayerEnergy;
                HUDUI.Instance.StartPlayerTurn(energy, 30); // TODO: maxEnergy from config
                HUDUI.Instance.SetTurn(turn);
            }
        }

        private void HandleTurnEnded(int turn, int energyProduced)
        {
            if (HUDUI.Instance != null)
            {
                var energy = _battleFlow.PlayerEnergy;
                HUDUI.Instance.SetEnergy(energy, 30);
            }
        }

        // ── Battle End Handler ────────────────────────────────

        private void HandleBattleFinished()
        {
            // 判断胜负
            bool victory = _battleFlow.PlayerHp > 0;
            if (SettlementUI.Instance != null)
            {
                if (victory)
                    SettlementUI.Instance.ShowVictory(
                        _battleFlow.TurnNumber,
                        _totalDamageDealt,
                        _maxComboThisBattle,
                        _cardsPlayedThisBattle);
                else
                    SettlementUI.Instance.ShowDefeat(
                        _battleFlow.TurnNumber,
                        _totalDamageDealt,
                        _maxComboThisBattle,
                        _cardsPlayedThisBattle);
            }
        }

        // ── Boss Phase / Shield Handlers ──────────────────────

        private void HandleBossPhaseChanged()
        {
            if (BossHPUI.Instance != null)
            {
                // Boss 相位切换动画
                // TODO: 从 Boss 读取当前 phase
            }
        }

        private void HandleBossShieldChanged(int shield)
        {
            if (BossHPUI.Instance != null)
                BossHPUI.Instance.UpdateShield(shield);
        }

        // ── Anchor Helpers ─────────────────────────────────────

        private Vector2? FindBossAnchor()
        {
            // 优先从 BossHPUI 获取锚点世界坐标
            if (BossHPUI.Instance != null)
            {
                var rt = BossHPUI.Instance.GetComponent<RectTransform>();
                if (rt != null) return rt.position;
            }
            return null;
        }

        private Vector2? FindBoardAnchor()
        {
            if (BoardUI.Instance != null)
            {
                var rt = BoardUI.Instance.GetComponent<RectTransform>();
                if (rt != null) return rt.position;
            }
            return null;
        }

        // ── External Reset ───────────────────────────────────

        /// <summary>
        /// 在新战斗开始前调用，重置统计
        /// </summary>
        public void ResetStats()
        {
            _totalDamageDealt = 0;
            _maxComboThisBattle = 0;
            _cardsPlayedThisBattle = 0;
        }
    }
}