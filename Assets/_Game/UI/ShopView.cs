using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardMatch.UI
{
    /// <summary>
    /// 商店界面视图 — 完善商品卡片布局、购买按钮状态、货币显示
    /// 继承自 ShopUI 的功能，并扩展商品卡片网格布局
    /// 东方古风卡通写实美术风格
    /// </summary>
    public class ShopView : MonoBehaviour
    {
        public static ShopView Instance { get; private set; }

        [Header("Shop View Container")]
        [SerializeField] private RectTransform _shopViewContainer;
        [SerializeField] private CanvasGroup _shopViewGroup;

        [Header("Card Layout")]
        [SerializeField] private GridLayoutGroup _cardGrid;
        [SerializeField] private int _cardsPerRow = 4;
        [SerializeField] private float _cardWidth = 160f;
        [SerializeField] private float _cardHeight = 220f;
        [SerializeField] private float _cardSpacingX = 16f;
        [SerializeField] private float _cardSpacingY = 16f;

        [Header("Currency Display")]
        [SerializeField] private Image _currencyIcon;
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private Color _currencyColor = new Color(0.722f, 0.525f, 0.043f); // #B8860B

        [Header("Buy Button States")]
        [SerializeField] private Color _affordableColor = new Color(0.271f, 0.616f, 0.373f); // #45B973
        [SerializeField] private Color _unaffordableColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        [SerializeField] private Color _pressedColor = new Color(0.165f, 0.392f, 0.247f);

        [Header("Card Prefab")]
        [SerializeField] private GameObject _shopCardPrefab;

        [Header("Animations")]
        [SerializeField] private float _dealDuration = 0.35f;
        [SerializeField] private float _dealDelay = 0.1f;

        // ── Runtime State ──────────────────────────────────────
        private readonly List<ShopCardItem> _cardItems = new();
        private int _currentCurrency;
        private bool _isVisible;

        // ── Events ─────────────────────────────────────────────
        public event Action<int> OnCurrencyChanged;
        public event Action<ShopCardItem> OnCardClicked;

        // ───────────────────────────────────────────────────────
        // Lifecycle
        // ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            gameObject.SetActive(false);
        }

        private void Start()
        {
            // 默认布局参数
            if (_cardGrid != null)
            {
                _cardGrid.cellSize = new Vector2(_cardWidth, _cardHeight);
                _cardGrid.spacing = new Vector2(_cardSpacingX, _cardSpacingY);
                _cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                _cardGrid.constraintCount = _cardsPerRow;
            }
        }

        // ══════════════════════════════════════════════════════
        //  Public API
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// 显示商店视图
        /// </summary>
        public void Show(int currency)
        {
            _currentCurrency = currency;
            _isVisible = true;
            gameObject.SetActive(true);
            UpdateCurrencyDisplay();
            StartCoroutine(AnimateShow());
        }

        /// <summary>
        /// 隐藏商店视图
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            StartCoroutine(AnimateHide());
        }

        /// <summary>
        /// 更新货币显示
        /// </summary>
        public void SetCurrency(int amount)
        {
            _currentCurrency = amount;
            UpdateCurrencyDisplay();
            RefreshAffordability();
        }

        /// <summary>
        /// 刷新商店卡牌（外部调用：reroll 或刷新时）
        /// </summary>
        public void RefreshCards(List<ShopCardData> cards)
        {
            ClearCards();
            foreach (var card in cards)
                AddCard(card);
            StartCoroutine(AnimateDealCards());
        }

        // ══════════════════════════════════════════════════════
        //  Private Methods
        // ══════════════════════════════════════════════════════

        private void UpdateCurrencyDisplay()
        {
            if (_currencyText != null)
            {
                _currencyText.text = $"🪙 {_currentCurrency}";
                _currencyText.color = _currencyColor;
            }
            if (_currencyIcon != null)
            {
                _currencyIcon.color = _currencyColor;
            }
        }

        private void ClearCards()
        {
            foreach (var item in _cardItems)
                if (item != null && item.GameObject != null)
                    Destroy(item.GameObject);
            _cardItems.Clear();
        }

        private void AddCard(ShopCardData data)
        {
            if (_shopCardPrefab == null || _cardGrid == null) return;

            var go = Instantiate(_shopCardPrefab, _cardGrid.transform);
            var item = new ShopCardItem
            {
                GameObject = go,
                Data = data,
                Price = data.Price
            };

            // 初始化卡牌 UI
            var nameText = go.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var priceText = go.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            var artImage = go.transform.Find("ArtImage")?.GetComponent<UnityEngine.UI.Image>();
            var buyBtn = go.transform.Find("BuyButton")?.GetComponent<Button>();

            if (nameText != null) nameText.text = data.CardName;
            if (priceText != null) priceText.text = $"🪙 {data.Price}";
            if (artImage != null && data.ArtSprite != null) artImage.sprite = data.ArtSprite;

            // 设置购买按钮状态
            bool affordable = _currentCurrency >= data.Price;
            if (buyBtn != null)
            {
                buyBtn.interactable = affordable;
                var btnColors = buyBtn.colors;
                btnColors.normalColor = affordable ? _affordableColor : _unaffordableColor;
                buyBtn.colors = btnColors;
                buyBtn.onClick.RemoveAllListeners();
                buyBtn.onClick.AddListener(() => OnBuyCardClicked(item));
            }

            // Rarity 边框颜色
            var frame = go.transform.Find("Frame")?.GetComponent<UnityEngine.UI.Image>();
            if (frame != null)
                frame.color = GetRarityColor(data.Rarity);

            _cardItems.Add(item);
        }

        private void RefreshAffordability()
        {
            foreach (var item in _cardItems)
            {
                if (item?.GameObject == null) continue;
                var buyBtn = item.GameObject.transform.Find("BuyButton")?.GetComponent<Button>();
                if (buyBtn == null) continue;

                bool affordable = _currentCurrency >= item.Price;
                buyBtn.interactable = affordable;
                var btnColors = buyBtn.colors;
                btnColors.normalColor = affordable ? _affordableColor : _unaffordableColor;
                buyBtn.colors = btnColors;
            }
        }

        private void OnBuyCardClicked(ShopCardItem item)
        {
            if (_currentCurrency < item.Price) return;

            _currentCurrency -= item.Price;
            UpdateCurrencyDisplay();
            OnCardClicked?.Invoke(item);
            RefreshAffordability();
        }

        private Color GetRarityColor(ECardRarity rarity) => rarity switch
        {
            ECardRarity.Common    => new Color(0.353f, 0.353f, 0.431f),
            ECardRarity.Rare      => new Color(0.118f, 0.420f, 0.549f),
            ECardRarity.Epic     => new Color(0.420f, 0.176f, 0.545f),
            ECardRarity.Legendary => _currencyColor,
            _ => Color.gray
        };

        // ══════════════════════════════════════════════════════
        //  Animations
        // ══════════════════════════════════════════════════════

        private System.Collections.IEnumerator AnimateShow()
        {
            _shopViewGroup.alpha = 0f;
            _shopViewGroup.blocksRaycasts = false;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _shopViewGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            _shopViewGroup.alpha = 1f;
            _shopViewGroup.blocksRaycasts = true;
        }

        private System.Collections.IEnumerator AnimateHide()
        {
            _shopViewGroup.blocksRaycasts = false;
            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _shopViewGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            _shopViewGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator AnimateDealCards()
        {
            for (int i = 0; i < _cardItems.Count; i++)
            {
                var go = _cardItems[i]?.GameObject;
                if (go == null) continue;

                // 从下方滑入
                var rt = go.GetComponent<RectTransform>();
                var targetPos = rt.anchoredPosition;
                var startPos = targetPos + new Vector2(0, -100f);
                rt.anchoredPosition = startPos;

                float elapsed = 0f;
                while (elapsed < _dealDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / _dealDuration;
                    float ease = 1f - (1f - t) * (1f - t);
                    rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
                    yield return null;
                }
                rt.anchoredPosition = targetPos;

                yield return new WaitForSeconds(_dealDelay);
            }
        }

        // ══════════════════════════════════════════════════════
        //  Supporting Types
        // ══════════════════════════════════════════════════════

        private class ShopCardItem
        {
            public GameObject GameObject;
            public ShopCardData Data;
            public int Price;
        }
    }

    /// <summary>
    /// 商店卡牌数据（传递给 ShopView）
    /// </summary>
    [System.Serializable]
    public class ShopCardData
    {
        public string CardName;
        public int Price;
        public Sprite ArtSprite;
        public ECardRarity Rarity;
        public int Attack;
        public int Defense;
        public string Description;
    }

    public enum ECardRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}