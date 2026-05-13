// ============================================================
// CardUI.cs — 单张卡牌 UI 组件（挂载在卡牌预制体上）
// 东方古风卡通写实风格
// ============================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardMatch.Gameplay.CardSystem;

namespace CardMatch.UI
{
    /// <summary>
    /// 单张卡牌的显示与交互组件。
    /// 负责：卡面渲染（名称/能耗/图标/类型色）/ 悬停/选中/拖拽状态
    /// </summary>
    public class CardUI : MonoBehaviour
    {
        [Header("Card Elements")]
        [SerializeField] private Image       _cardBgImage;     // 卡牌背景
        [SerializeField] private Image       _iconImage;       // 卡牌图标
        [SerializeField] private TextMeshProUGUI _nameText;    // 卡牌名称
        [SerializeField] private TextMeshProUGUI _costText;    // 能量消耗
        [SerializeField] private TextMeshProUGUI _descText;    // 描述文字
        [SerializeField] private Image       _typeIconImage;   // 类型图标

        [Header("Colors — 类型色")]
        [SerializeField] private Color _attackColor   = new Color(0.902f, 0.224f, 0.275f);  // #E63946
        [SerializeField] private Color _defenseColor  = new Color(0.271f, 0.482f, 0.616f); // #457B9D
        [SerializeField] private Color _transformColor = new Color(0.608f, 0.365f, 0.898f); // #9B5DE5
        [SerializeField] private Color _utilityColor   = new Color(0.165f, 0.616f, 0.561f); // #2A9D8F
        [SerializeField] private Color _neutralColor   = Color.white;

        [Header("Glow / Border")]
        [SerializeField] private Image       _glowBorder;      // 选中发光边框
        [SerializeField] private GameObject _hoverRoot;       // 悬停状态根节点
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float _hoverScale    = 1.05f;
        [SerializeField] private float _selectedScale = 1.08f;
        [SerializeField] private float _dragScale    = 1.12f;
        [SerializeField] private float _animSpeed    = 12f;

        // ── State ────────────────────────────────────────────────
        private CardData _data;
        private bool _isHovered;
        private bool _isSelected;
        private bool _isDragging;
        private Vector3 _baseLocalPos;
        private Quaternion _baseLocalRot;

        // ── Public ───────────────────────────────────────────────
        public CardData Data => _data;

        public void Init(CardData data)
        {
            _data = data;
            Refresh();
        }

        public void Refresh()
        {
            if (_data == null) return;

            if (_nameText   != null) _nameText.text   = _data.CardName;
            if (_costText   != null) _costText.text   = _data.EnergyCost.ToString();
            if (_descText   != null) _descText.text   = _data.Description;

            // 类型色
            var typeColor = GetTypeColor(_data.CardType);
            if (_cardBgImage != null)
                _cardBgImage.color = typeColor;

            // 加载图标
            if (!string.IsNullOrEmpty(_data.IconPath))
            {
                var sprite = Resources.Load<Sprite>(_data.IconPath);
                if (_iconImage != null)
                {
                    _iconImage.sprite = sprite;
                    _iconImage.enabled = sprite != null;
                }
            }

            // 发光边框默认关闭
            if (_glowBorder != null)
                _glowBorder.gameObject.SetActive(false);
        }

        // ── State Setters ────────────────────────────────────────

        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            RefreshVisuals();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            if (_glowBorder != null)
                _glowBorder.gameObject.SetActive(selected);
            RefreshVisuals();
        }

        public void SetDragging(bool dragging)
        {
            _isDragging = dragging;
            RefreshVisuals();
        }

        // ── Private ─────────────────────────────────────────────

        private void Awake()
        {
            _baseLocalPos = transform.localPosition;
            _baseLocalRot = transform.localRotation;
        }

        private void RefreshVisuals()
        {
            float targetScale = 1f;
            if (_isDragging)  targetScale = _dragScale;
            else if (_isSelected) targetScale = _selectedScale;
            else if (_isHovered)  targetScale = _hoverScale;

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * _animSpeed);

            // Hover 时轻微上移
            Vector3 targetPos = _baseLocalPos;
            if (_isHovered && !_isDragging)
                targetPos.y += 15f;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetPos,
                Time.deltaTime * _animSpeed);
        }

        private Color GetTypeColor(ECardType type) => type switch
        {
            ECardType.Attack    => _attackColor,
            ECardType.Defense   => _defenseColor,
            ECardType.Transform => _transformColor,
            ECardType.Utility  => _utilityColor,
            _                    => _neutralColor
        };

        /// <summary>
        /// 从 CardType（内部类型）映射到 ECardType（UI 类型）
        /// 用于 UI 颜色匹配
        /// </summary>
        public static ECardType MapCardType(CardType cardType) => cardType switch
        {
            CardType.ClearRow   => ECardType.Attack,
            CardType.ClearCol   => ECardType.Attack,
            CardType.ClearArea  => ECardType.Attack,
            CardType.Bomb       => ECardType.Attack,
            CardType.Swap       => ECardType.Transform,
            CardType.Shuffle    => ECardType.Utility,
            CardType.EnergyBoost => ECardType.Utility,
            CardType.Heal       => ECardType.Defense,
            _                     => ECardType.None
        };
    }
}
