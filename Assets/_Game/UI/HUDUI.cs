// ============================================================
// HUDUI.cs — HUD（能量条/当前波次/结束回合按钮）
// 东方古风卡通写实风格
// ============================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardMatch.BattleFlow;

namespace CardMatch.UI
{
    /// <summary>
    /// 管理 HUD：玩家能量条、当前波次、结束回合按钮等。
    /// </summary>
    public class HUDUI : MonoBehaviour
    {
        public static HUDUI Instance { get; private set; }

        [Header("HUD Panel")]
        [SerializeField] private RectTransform _hudPanel;

        [Header("Energy Bar")]
        [SerializeField] private Slider _energySlider;
        [SerializeField] private TextMeshProUGUI _energyText;          // "3/5"
        [SerializeField] private Image _energyFillImage;
        [SerializeField] private GameObject _energyOrbPrefab;         // 能量球预制（满时显示）

        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI _playerHPText;       // "30/30"
        [SerializeField] private Slider _playerHPSlider;
        [SerializeField] private Image _playerHPFill;

        [Header("Enemy Info")]
        [SerializeField] private TextMeshProUGUI _enemyHPText;
        [SerializeField] private Slider _enemyHPSlider;
        [SerializeField] private Image _enemyHPFill;

        [Header("Wave / Turn")]
        [SerializeField] private TextMeshProUGUI _waveText;           // "Wave 3"
        [SerializeField] private TextMeshProUGUI _turnText;           // "Turn 5"
        [SerializeField] private TextMeshProUGUI _turnTimerText;       // "0:24"

        [Header("End Turn Button")]
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private TextMeshProUGUI _endTurnButtonText;
        [SerializeField] private GameObject _endTurnGlowObj;          // 激活时发光对象

        [Header("Action Indicators")]
        [SerializeField] private GameObject _yourTurnIndicator;       // "Your Turn!" 提示
        [SerializeField] private GameObject _enemyTurnIndicator;     // "Enemy Turn" 提示

        [Header("Deck / Hand Count")]
        [SerializeField] private TextMeshProUGUI _deckCountText;     // "Deck: 24"
        [SerializeField] private TextMeshProUGUI _handCountText;     // "Hand: 7"

        [Header("Color Scheme")]
        [SerializeField] private Color _energyColor       = new Color(0.165f, 0.616f, 0.561f);  // #2A9D8F
        [SerializeField] private Color _hpPlayerColor     = new Color(0.271f, 0.482f, 0.616f);  // #457B9D
        [SerializeField] private Color _hpEnemyColor      = new Color(0.902f, 0.224f, 0.275f);  // #E63946
        [SerializeField] private Color _endTurnActiveColor = new Color(0.165f, 0.616f, 0.561f); // #2A9D8F
        [SerializeField] private Color _endTurnInactiveColor = new Color(0.353f, 0.353f, 0.431f); // 灰色

        [Header("Timer Settings")]
        [SerializeField] private float _turnTimeLimit = 30f;          // 每回合30秒
        private float _currentTurnTime;
        private bool _timerRunning;

        // ── Runtime State ──────────────────────────────────────
        private int _currentEnergy;
        private int _maxEnergy;
        private int _playerHP;
        private int _playerMaxHP;
        private int _enemyHP;
        private int _enemyMaxHP;
        private int _currentWave;
        private int _currentTurn;
        private bool _isPlayerTurn;

        // ── Events ─────────────────────────────────────────────
        public event Action OnEndTurnClicked;

        // ───────────────────────────────────────────────────────
        // Lifecycle
        // ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _endTurnButton?.onClick.AddListener(HandleEndTurn);
            _endTurnButton.interactable = false;
            SetEndTurnGlow(false);

            if (_yourTurnIndicator != null) _yourTurnIndicator.SetActive(false);
            if (_enemyTurnIndicator != null) _enemyTurnIndicator.SetActive(false);
        }

        private void Update()
        {
            if (_timerRunning) UpdateTimer();
        }

        // ── Public API ─────────────────────────────────────────

        /// <summary>设置能量（当前/最大）</summary>
        public void SetEnergy(int current, int max)
        {
            _currentEnergy = current;
            _maxEnergy = max;

            if (_energySlider != null)
            {
                _energySlider.maxValue = max;
                _energySlider.value = current;
            }
            if (_energyText != null)
                _energyText.text = $"{current}/{max}";

            // 能量条颜色渐变（满能量时更亮）
            if (_energyFillImage != null)
            {
                float ratio = max > 0 ? (float)current / max : 0f;
                _energyFillImage.color = Color.Lerp(
                    new Color(0.165f, 0.4f, 0.361f),
                    _energyColor,
                    ratio);
            }
        }

        /// <summary>消耗能量</summary>
        public bool SpendEnergy(int amount)
        {
            if (_currentEnergy < amount) return false;
            SetEnergy(_currentEnergy - amount, _maxEnergy);
            return true;
        }

        /// <summary>设置玩家血量</summary>
        public void SetPlayerHP(int current, int max)
        {
            _playerHP = current;
            _playerMaxHP = max;
            if (_playerHPSlider != null)
            {
                _playerHPSlider.maxValue = max;
                _playerHPSlider.value = current;
            }
            if (_playerHPText != null)
                _playerHPText.text = $"{current}/{max}";

            if (_playerHPFill != null)
            {
                float ratio = max > 0 ? (float)current / max : 0f;
                _playerHPFill.color = ratio < 0.3f ? _hpEnemyColor : _hpPlayerColor;
            }
        }

        /// <summary>设置敌人血量</summary>
        public void SetEnemyHP(int current, int max)
        {
            _enemyHP = current;
            _enemyMaxHP = max;
            if (_enemyHPSlider != null)
            {
                _enemyHPSlider.maxValue = max;
                _enemyHPSlider.value = current;
            }
            if (_enemyHPText != null)
                _enemyHPText.text = $"{current}/{max}";

            if (_enemyHPFill != null)
            {
                float ratio = max > 0 ? (float)current / max : 0f;
                _enemyHPFill.color = ratio < 0.3f ? _hpEnemyColor : new Color(0.608f, 0.137f, 0.208f);
            }
        }

        /// <summary>设置波次显示</summary>
        public void SetWave(int wave)
        {
            _currentWave = wave;
            if (_waveText != null) _waveText.text = $"Wave {wave}";
        }

        /// <summary>设置回合数</summary>
        public void SetTurn(int turn)
        {
            _currentTurn = turn;
            if (_turnText != null) _turnText.text = $"Turn {turn}";
        }

        /// <summary>设置牌堆剩余数量</summary>
        public void SetDeckCount(int count)
        {
            if (_deckCountText != null)
                _deckCountText.text = $"🂠 {count}";
        }

        /// <summary>设置手牌数量</summary>
        public void SetHandCount(int count)
        {
            if (_handCountText != null)
                _handCountText.text = $"Hand: {count}";
        }

        /// <summary>开始玩家回合</summary>
        public void StartPlayerTurn(int energy, int maxEnergy)
        {
            _isPlayerTurn = true;
            SetEnergy(energy, maxEnergy);
            _endTurnButton.interactable = true;
            SetEndTurnGlow(true);
            _yourTurnIndicator?.SetActive(true);
            _enemyTurnIndicator?.SetActive(false);
            StartTimer();
        }

        /// <summary>结束玩家回合</summary>
        public void EndPlayerTurn()
        {
            _isPlayerTurn = false;
            _endTurnButton.interactable = false;
            SetEndTurnGlow(false);
            _yourTurnIndicator?.SetActive(false);
            _enemyTurnIndicator?.SetActive(true);
            StopTimer();
        }

        /// <summary>显示敌人生成倒计时提示</summary>
        public void ShowEnemyTurnWarning(string message)
        {
            if (_enemyTurnIndicator != null)
            {
                var text = _enemyTurnIndicator.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = message;
                _enemyTurnIndicator.SetActive(true);
            }
        }

        /// <summary>播放能量增加动画</summary>
        public void PlayEnergyGainVFX()
        {
            // TODO: 能量增加特效动画（东方古风粒子）
        }

        // ── Private Methods ────────────────────────────────────

        private void HandleEndTurn()
        {
            if (!_isPlayerTurn) return;
            StopTimer();
            OnEndTurnClicked?.Invoke();
        }

        private void SetEndTurnGlow(bool active)
        {
            if (_endTurnGlowObj != null)
                _endTurnGlowObj.SetActive(active);

            if (_endTurnButton != null)
            {
                var colors = _endTurnButton.colors;
                colors.normalColor = active ? _endTurnActiveColor : _endTurnInactiveColor;
                _endTurnButton.colors = colors;
            }
        }

        private void StartTimer()
        {
            _timerRunning = true;
            _currentTurnTime = _turnTimeLimit;
        }

        private void StopTimer()
        {
            _timerRunning = false;
        }

        private void UpdateTimer()
        {
            _currentTurnTime -= Time.deltaTime;
            if (_currentTurnTime < 0f) _currentTurnTime = 0f;

            int seconds = Mathf.CeilToInt(_currentTurnTime);
            if (_turnTimerText != null)
                _turnTimerText.text = $"⏱ {seconds}";

            // 时间耗尽自动结束回合
            if (_currentTurnTime <= 0f && _isPlayerTurn)
            {
                HandleEndTurn();
            }
        }

        // ── Optional: 暂停/恢复 HUD ───────────────────────────

        public void ShowHUD()
        {
            _hudPanel?.gameObject.SetActive(true);
        }

        public void HideHUD()
        {
            _hudPanel?.gameObject.SetActive(false);
        }
    }
}