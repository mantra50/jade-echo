// ============================================================
// SettlementUI.cs — 战斗结算界面（胜负弹窗）
// 东方古风卡通写实风格
// ============================================================

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardMatch.UI
{
    /// <summary>
    /// 战斗结束时的结算界面（胜利 / 失败 / 平局）
    /// 负责：显示结果 + 统计数据 + 按钮跳转
    /// </summary>
    public class SettlementUI : MonoBehaviour
    {
        public static SettlementUI Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject _root;

        [Header("Result Banner")]
        [SerializeField] private TextMeshProUGUI _resultText;       // "胜利" / "失败"
        [SerializeField] private Image _resultBannerBg;           // 背景图
        [SerializeField] private GameObject _victoryVFXRoot;        // 胜利特效根节点
        [SerializeField] private GameObject _defeatVFXRoot;         // 失败特效根节点

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _turnsSurvivedText;
        [SerializeField] private TextMeshProUGUI _damageDealtText;
        [SerializeField] private TextMeshProUGUI _comboCountText;
        [SerializeField] private TextMeshProUGUI _cardsPlayedText;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Colors")]
        [SerializeField] private Color _victoryColor  = new Color(1.0f, 0.686f, 0.075f);   // 金黄
        [SerializeField] private Color _defeatColor   = new Color(0.902f, 0.224f, 0.275f); // #E63946
        [SerializeField] private Color _neutralColor  = new Color(0.608f, 0.365f, 0.898f); // #9B5DE5

        [Header("Animation Settings")]
        [SerializeField] private float _entryDuration = 0.5f;
        [SerializeField] private float _buttonDelay  = 0.3f;

        // ── Runtime Stats ──────────────────────────────────────
        private int _turnsSurvived;
        private int _damageDealt;
        private int _comboCount;
        private int _cardsPlayed;
        private bool _isVictory;

        // ── Events ─────────────────────────────────────────────
        public event Action OnRetryClicked;
        public event Action OnNextLevelClicked;
        public event Action OnMainMenuClicked;

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
            _retryButton?.onClick.AddListener(() => OnRetryClicked?.Invoke());
            _nextLevelButton?.onClick.AddListener(() => OnNextLevelClicked?.Invoke());
            _mainMenuButton?.onClick.AddListener(() => OnMainMenuClicked?.Invoke());

            // 默认隐藏
            if (_root != null) _root.SetActive(false);
        }

        // ── Public API ─────────────────────────────────────────

        /// <summary>显示胜利界面</summary>
        public void ShowVictory(int turns, int damageDealt, int comboCount, int cardsPlayed)
        {
            _isVictory = true;
            _turnsSurvived = turns;
            _damageDealt = damageDealt;
            _comboCount = comboCount;
            _cardsPlayed = cardsPlayed;

            PopulateStats();

            if (_resultText != null) _resultText.text = "胜利";
            if (_resultBannerBg != null) _resultBannerBg.color = _victoryColor;
            if (_victoryVFXRoot != null) _victoryVFXRoot.SetActive(true);
            if (_defeatVFXRoot != null) _defeatVFXRoot.SetActive(false);

            _nextLevelButton?.gameObject?.SetActive(true);
            _retryButton?.gameObject?.SetActive(true);

            Show();
        }

        /// <summary>显示失败界面</summary>
        public void ShowDefeat(int turns, int damageDealt, int comboCount, int cardsPlayed)
        {
            _isVictory = false;
            _turnsSurvived = turns;
            _damageDealt = damageDealt;
            _comboCount = comboCount;
            _cardsPlayed = cardsPlayed;

            PopulateStats();

            if (_resultText != null) _resultText.text = "失败";
            if (_resultBannerBg != null) _resultBannerBg.color = _defeatColor;
            if (_victoryVFXRoot != null) _victoryVFXRoot.SetActive(false);
            if (_defeatVFXRoot != null) _defeatVFXRoot.SetActive(true);

            _nextLevelButton?.gameObject?.SetActive(false);
            _retryButton?.gameObject?.SetActive(true);

            Show();
        }

        /// <summary>隐藏结算界面</summary>
        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        // ── Private Methods ────────────────────────────────────

        private void PopulateStats()
        {
            if (_turnsSurvivedText != null) _turnsSurvivedText.text = $"存活回合：{_turnsSurvived}";
            if (_damageDealtText != null) _damageDealtText.text = $"造成伤害：{_damageDealt}";
            if (_comboCountText != null) _comboCountText.text = $"最高连消：{_comboCount}";
            if (_cardsPlayedText != null) _cardsPlayedText.text = $"使用卡牌：{_cardsPlayed}";
        }

        private void Show()
        {
            if (_root == null) return;
            _root.SetActive(true);

            // 入场动画：scale 从 0.5 → 1.0 + alpha 淡入
            var rt = _root.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one * 0.5f;
                StartCoroutine(AnimateEntry());
            }
        }

        private System.Collections.IEnumerator AnimateEntry()
        {
            float elapsed = 0f;
            while (elapsed < _entryDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(elapsed / _entryDuration);
                var rt = _root.GetComponent<RectTransform>();
                if (rt != null) rt.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, t);
                yield return null;
            }

            var finalRt = _root.GetComponent<RectTransform>();
            if (finalRt != null) finalRt.localScale = Vector3.one;

            // 按钮延迟入场
            yield return new WaitForSeconds(_buttonDelay);
            if (_retryButton != null)
            {
                _retryButton.gameObject.SetActive(true);
                // 按钮入场动画
                yield return AnimateButton(_retryButton.transform as RectTransform);
            }
            if (_nextLevelButton != null && _nextLevelButton.gameObject.activeSelf)
            {
                yield return new WaitForSeconds(0.1f);
                yield return AnimateButton(_nextLevelButton.transform as RectTransform);
            }
        }

        private System.Collections.IEnumerator AnimateButton(RectTransform buttonRt)
        {
            if (buttonRt == null) yield break;
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 startScale = Vector3.one * 0.8f;
            buttonRt.localScale = startScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(elapsed / duration);
                buttonRt.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                yield return null;
            }
            buttonRt.localScale = Vector3.one;
        }

        private static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}