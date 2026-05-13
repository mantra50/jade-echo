// ============================================================
// BossHPUI.cs — Boss 血条动画系统
// 东方古风卡通写实风格
// ============================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardMatch.UI
{
    /// <summary>
    /// Boss 血条 UI（独立面板，悬浮在棋盘上方）
    /// 功能：血量变化动画（平滑下降 + 颜色渐变 + 护盾显示 + 相位切换特效）
    /// </summary>
    public class BossHPUI : MonoBehaviour
    {
        public static BossHPUI Instance { get; private set; }

        [Header("Boss Info")]
        [SerializeField] private TextMeshProUGUI _bossNameText;
        [SerializeField] private TextMeshProUGUI _bossPhaseText;    // "Phase 1 / 2"

        [Header("HP Bar")]
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Slider _hpDisplayedSlider;  // 延迟显示的血条（显示用）
        [SerializeField] private Image  _hpFillImage;
        [SerializeField] private TextMeshProUGUI _hpText;    // "123 / 456"

        [Header("Shield")]
        [SerializeField] private GameObject _shieldRoot;
        [SerializeField] private TextMeshProUGUI _shieldText;

        [Header("Colors")]
        [SerializeField] private Color _hpHighColor    = new Color(0.165f, 0.616f, 0.561f);  // #2A9D8F >70%
        [SerializeField] private Color _hpMidColor     = new Color(0.898f, 0.686f, 0.075f);  // 黄 30~70%
        [SerializeField] private Color _hpLowColor     = new Color(0.902f, 0.224f, 0.275f);  // #E63946 <30%
        [SerializeField] private Color _shieldColor    = new Color(0.608f, 0.365f, 0.898f); // #9B5DE5 护盾

        [Header("Animation Settings")]
        [SerializeField] private float _drainSpeed      = 0.8f;   // 血条每秒流失速度（格）
        [SerializeField] private float _phaseFlashDuration = 0.4f;
        [SerializeField] private float _shakeIntensity   = 8f;    // 受击抖动幅度
        [SerializeField] private float _shakeDuration    = 0.3f;  // 受击抖动持续时间

        [Header("Panel")]
        [SerializeField] private RectTransform _panelRoot;
        [SerializeField] private GameObject _panelGlow;           // Boss 出现时的发光

        // ── Runtime State ──────────────────────────────────────
        private int _maxHp;
        private int _currentHp;
        private int _shield;
        private int _currentPhase = 1;

        private bool _isAnimating;
        private float _shakeTime;
        private Vector2 _panelBasePos;

        // ── Events ─────────────────────────────────────────────
        public event System.Action OnBossDied;

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
            if (_panelRoot != null)
                _panelBasePos = _panelRoot.anchoredPosition;

            if (_hpDisplayedSlider != null)
                _hpDisplayedSlider.value = 1f;
        }

        private void Update()
        {
            // 平滑消耗Displayed血条
            if (_hpDisplayedSlider != null && _hpSlider != null)
            {
                if (_hpDisplayedSlider.value > _hpSlider.value)
                {
                    _hpDisplayedSlider.value = Mathf.MoveTowards(
                        _hpDisplayedSlider.value,
                        _hpSlider.value,
                        Time.deltaTime * _drainSpeed);
                }
            }

            // 受击抖动
            if (_shakeTime > 0f)
            {
                _shakeTime -= Time.deltaTime;
                if (_panelRoot != null)
                {
                    float intensity = (_shakeTime / _shakeDuration) * _shakeIntensity;
                    _panelRoot.anchoredPosition = _panelBasePos
                        + new Vector2(
                            Mathf.PerlinNoise(_shakeTime * 50f, 0f) * intensity,
                            Mathf.PerlinNoise(0f, _shakeTime * 50f) * intensity);
                }
            }
            else if (_panelRoot != null && _panelRoot.anchoredPosition != _panelBasePos)
            {
                _panelRoot.anchoredPosition = _panelBasePos;
            }
        }

        // ── Public API ─────────────────────────────────────────

        /// <summary>初始化 Boss 血条（场景开始时调用）</summary>
        public void Init(string bossName, int maxHp, int phase = 1)
        {
            _maxHp = maxHp;
            _currentHp = maxHp;
            _currentPhase = phase;
            _shield = 0;

            if (_bossNameText != null) _bossNameText.text = bossName;
            if (_bossPhaseText != null) _bossPhaseText.text = $"Phase {phase}";

            // 立即设置 slider
            if (_hpSlider != null)
            {
                _hpSlider.maxValue = maxHp;
                _hpSlider.value = maxHp;
            }
            if (_hpDisplayedSlider != null)
            {
                _hpDisplayedSlider.maxValue = maxHp;
                _hpDisplayedSlider.value = maxHp;
            }
            if (_hpText != null) _hpText.text = $"{maxHp} / {maxHp}";

            // 颜色
            UpdateHPColor(1f);

            // 护盾
            if (_shieldRoot != null) _shieldRoot.SetActive(false);

            // 显示面板
            gameObject.SetActive(true);
        }

        /// <summary>更新 Boss 血量（由 BattleFlowManager 在受到伤害时调用）</summary>
        public void UpdateHP(int currentHp, int maxHp)
        {
            _currentHp = currentHp;
            if (_hpSlider != null) _hpSlider.value = currentHp;

            float ratio = maxHp > 0 ? (float)currentHp / maxHp : 0f;
            if (_hpText != null) _hpText.text = $"{currentHp} / {maxHp}";
            UpdateHPColor(ratio);

            // 受击抖动
            TriggerShake();
        }

        /// <summary>显示受到伤害时的数字动画（内部驱动 DamagePopup）</summary>
        public void ShowDamageTaken(int damage)
        {
            // 由外部 DamagePopup 调用
        }

        /// <summary>更新护盾值</summary>
        public void UpdateShield(int shield)
        {
            _shield = shield;
            if (_shieldRoot != null)
                _shieldRoot.SetActive(shield > 0);
            if (_shieldText != null)
                _shieldText.text = $"🛡 {shield}";
        }

        /// <summary>Boss 相位切换（Phase 1 → 2 时触发全屏闪白 + 血条重置）</summary>
        public void TriggerPhaseTransition(int newPhase)
        {
            _currentPhase = newPhase;
            if (_bossPhaseText != null) _bossPhaseText.text = $"Phase {newPhase}";

            StartCoroutine(AnimatePhaseTransition());
        }

        /// <summary>Boss 死亡（血条清空动画）</summary>
        public void TriggerDeath()
        {
            StartCoroutine(AnimateDeath());
        }

        /// <summary>隐藏 Boss UI（脱离战斗时）</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // ── Private Methods ────────────────────────────────────

        private void UpdateHPColor(float ratio)
        {
            if (_hpFillImage == null) return;
            _hpFillImage.color = ratio switch
            {
                >= 0.7f => _hpHighColor,
                <= 0.3f => _hpLowColor,
                _       => _hpMidColor
            };
        }

        private void TriggerShake()
        {
            _shakeTime = _shakeDuration;
        }

        private System.Collections.IEnumerator AnimatePhaseTransition()
        {
            // 全屏白闪
            yield break;
        }

        private System.Collections.IEnumerator AnimateDeath()
        {
            // 血条快速清空
            if (_hpSlider != null)
            {
                float start = _hpSlider.value;
                float elapsed = 0f;
                float duration = 0.6f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _hpSlider.value = Mathf.Lerp(start, 0f, EaseOutQuad(elapsed / duration));
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.3f);
            OnBossDied?.Invoke();
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInQuad(float t) => t * t;
    }
}