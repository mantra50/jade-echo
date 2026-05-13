// ============================================================
// DamagePopup.cs — 伤害数字弹出系统
// 东方古风卡通写实风格
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardMatch.UI
{
    /// <summary>
    /// 伤害数字弹出管理（挂载在 HUD 层级或专属 Canvas 上）
    /// 支持：普通伤害 / 暴击 / 治疗 / 魔力消耗 等类型
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        public static DamagePopup Instance { get; private set; }

        [Header("Prefab")]
        [SerializeField] private GameObject _popupPrefab;

        [Header("Spawn Anchor")]
        [SerializeField] private Transform _anchor;    // 世界坐标锚点（Boss 位置或伤害发生位置）

        [Header("Animation Settings")]
        [SerializeField] private float _lifeTime      = 1.2f;
        [SerializeField] private float _floatDistance = 80f;
        [SerializeField] private float _scaleInDuration = 0.1f;
        [SerializeField] private float _scaleOutStart  = 0.8f;  // 开始缩小的时间点（lifeTime 的比率）

        [Header("Colors")]
        [SerializeField] private Color _damageColor    = new Color(0.902f, 0.224f, 0.275f); // #E63946 红
        [SerializeField] private Color _critColor      = new Color(1.0f,   0.686f, 0.075f); // 金黄
        [SerializeField] private Color _healColor     = new Color(0.165f, 0.616f, 0.561f); // #2A9D8F 青绿
        [SerializeField] private Color _energyColor   = new Color(0.608f, 0.365f, 0.898f); // #9B5DE5 紫
        [SerializeField] private Color _bossColor     = new Color(0.902f, 0.224f, 0.275f, 0.8f); // 半透明红（Boss受伤）

        private readonly Queue<GameObject> _pool = new();
        private readonly Queue<GameObject> _active = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            // 缓慢移除已完成动画的 popup（延迟清理）
            while (_active.Count > 0 && _active.Peek() == null)
                _active.Dequeue();
        }

        // ── Public API ─────────────────────────────────────────

        /// <summary>弹出伤害数字（玩家对 Boss）</summary>
        public void ShowDamage(int amount, Vector2? worldPos = null)
        {
            Show(amount, EPopupType.Damage, worldPos ?? GetAnchorPosition());
        }

        /// <summary>弹出暴击伤害数字</summary>
        public void ShowCrit(int amount, Vector2? worldPos = null)
        {
            Show(amount, EPopupType.Crit, worldPos ?? GetAnchorPosition());
        }

        /// <summary>弹出治疗数字</summary>
        public void ShowHeal(int amount, Vector2? worldPos = null)
        {
            Show(amount, EPopupType.Heal, worldPos ?? GetAnchorPosition());
        }

        /// <summary>弹出能量消耗数字</summary>
        public void ShowEnergy(int amount, Vector2? worldPos = null)
        {
            Show(amount, EPopupType.Energy, worldPos ?? GetAnchorPosition());
        }

        /// <summary>弹出 Boss 受伤数字（较大，半透明）</summary>
        public void ShowBossDamage(int amount, Vector2? worldPos = null)
        {
            Show(amount, EPopupType.BossDamage, worldPos ?? GetAnchorPosition());
        }

        // ── Internal ──────────────────────────────────────────

        private void Show(int amount, EPopupType type, Vector2 worldPos)
        {
            if (_popupPrefab == null) return;

            GameObject popup;
            if (_pool.Count > 0)
            {
                popup = _pool.Dequeue();
                popup.SetActive(true);
            }
            else
            {
                popup = Instantiate(_popupPrefab, worldPos, Quaternion.identity, transform);
            }

            _active.Enqueue(popup);

            // 设置 TextMeshPro 内容
            var tmp = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                string prefix = type switch
                {
                    EPopupType.Heal       => "+",
                    EPopupType.Crit       => "暴击! ",
                    EPopupType.Energy     => "-",
                    _                      => ""
                };
                tmp.text = $"{prefix}{amount}";
                tmp.color = GetColor(type);
            }

            // 缩放弹性入场
            popup.transform.localScale = Vector3.zero;
            StartCoroutine(AnimatePopup(popup, worldPos, type));
        }

        private System.Collections.IEnumerator AnimatePopup(GameObject popup, Vector2 startPos, EPopupType type)
        {
            float elapsed = 0f;
            float totalLife = _lifeTime;
            float halfLife = totalLife * 0.5f;

            // 入场：弹性放大 (0 → 1.2 → 1.0)
            while (elapsed < halfLife)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfLife;

                // 弹簧缓动
                float scale = t < 0.5f
                    ? Mathf.Lerp(0f, 1.2f, EaseOutBack(t * 2f))
                    : Mathf.Lerp(1.2f, 1.0f, EaseOutQuad((t - 0.5f) * 2f));

                popup.transform.localScale = Vector3.one * scale;

                // 上浮
                float yOffset = Mathf.Lerp(0f, _floatDistance, EaseOutQuad(t));
                popup.transform.position = startPos + new Vector2(0, yOffset);

                yield return null;
            }

            // 悬浮一段时间
            float stableTime = totalLife * 0.3f;
            float stableElapsed = 0f;
            while (stableElapsed < stableTime)
            {
                stableElapsed += Time.deltaTime;
                yield return null;
            }

            // 淡出缩小
            float fadeElapsed = 0f;
            float fadeStart = totalLife * _scaleOutStart;
            float fadeDuration = totalLife - fadeStart;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float t = fadeElapsed / fadeDuration;

                // 快速缩小并淡出
                float scale = Mathf.Lerp(1f, 0.3f, EaseInQuad(t));
                popup.transform.localScale = Vector3.one * scale;

                var tmp = popup.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    Color c = tmp.color;
                    c.a = Mathf.Lerp(1f, 0f, EaseInQuad(t));
                    tmp.color = c;
                }

                yield return null;
            }

            // 回收
            popup.SetActive(false);
            _pool.Enqueue(popup);

            // 从 active 队列移除（通过延迟检查处理）
        }

        private Color GetColor(EPopupType type) => type switch
        {
            EPopupType.Damage     => _damageColor,
            EPopupType.Crit       => _critColor,
            EPopupType.Heal       => _healColor,
            EPopupType.Energy     => _energyColor,
            EPopupType.BossDamage  => _bossColor,
            _                      => _damageColor
        };

        private Vector2 GetAnchorPosition()
        {
            if (_anchor != null) return _anchor.position;
            return Camera.main != null ? Camera.main.transform.position : Vector2.zero;
        }

        // ── Easing Functions ──────────────────────────────────

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInQuad(float t) => t * t;
        private static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        // ── Types ────────────────────────────────────────────

        private enum EPopupType
        {
            Damage,
            Crit,
            Heal,
            Energy,
            BossDamage
        }
    }
}