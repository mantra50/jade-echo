// ============================================================
// ShopUI.cs — 商店界面（卡牌网格/价格显示/购买确认）
// 东方古风卡通写实风格
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardMatch.CardSystem;

namespace CardMatch.UI
{
    /// <summary>
    /// 管理商店界面：卡牌商品展示、购买、重投（reroll）。
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance { get; private set; }

        [Header("Shop Panel")]
        [SerializeField] private RectTransform _shopPanel;
        [SerializeField] private CanvasGroup _shopCanvasGroup;
        [SerializeField] private Button _leaveShopButton;

        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private Image _goldIcon;

        [Header("Card Grid")]
        [SerializeField] private RectTransform _cardGridContainer;
        [SerializeField] private GameObject _shopCardPrefab;  // 商店卡牌预制体
        [SerializeField] private int _slotsPerRefresh = 4;
        [SerializeField] private float _cardGap = 20f;

        [Header("Reroll")]
        [SerializeField] private Button _rerollButton;
        [SerializeField] private TextMeshProUGUI _rerollCostText;
        [SerializeField] private TextMeshProUGUI _rerollsUsedText; // "1/3 rerolls used"

        [Header("Shop Info")]
        [SerializeField] private TextMeshProUGUI _nextRefreshText; // "Next refresh: Turn 5"
        [SerializeField] private TextMeshProUGUI _levelText;      // 等级

        [Header("Color Scheme")]
        [SerializeField] private Color _goldColor      = new Color(0.722f, 0.525f, 0.043f); // #B8860B
        [SerializeField] private Color _affordableCardTint = Color.white;
        [SerializeField] private Color _unaffordableTint = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        [Header("Preview Popup")]
        [SerializeField] private RectTransform _previewPopup;   // 大尺寸预览弹窗
        [SerializeField] private TextMeshProUGUI _previewName;
        [SerializeField] private TextMeshProUGUI _previewStats;

        [Header("SFX")]
        [SerializeField] private AudioClip _buySFX;
        [SerializeField] private AudioClip _rerollSFX;
        [SerializeField] private AudioClip _rejectSFX;

        // ── Runtime State ──────────────────────────────────────
        private readonly List<ShopCardUI> _shopCards = new();
        private int _currentGold;
        private int _rerollCost = 2;
        private int _rerollsUsed;
        private int _maxRerolls = 3;
        private int _nextRefreshTurn;
        private bool _isVisible;

        // ── Events ─────────────────────────────────────────────
        public event Action<CardData> OnCardPurchased;
        public event Action OnRerollRequested;
        public event Action OnLeaveShop;

        // ───────────────────────────────────────────────────────
        // Lifecycle
        // ───────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _shopCanvasGroup.alpha = 0f;
            _shopCanvasGroup.blocksRaycasts = false;
        }

        private void Start()
        {
            _rerollButton?.onClick.AddListener(RequestReroll);
            _leaveShopButton?.onClick.AddListener(LeaveShop);

            _rerollCostText.text = $"{_rerollCost} Gold";
        }

        // ── Public API ─────────────────────────────────────────

        /// <summary>进入商店（播放入场动画）</summary>
        public void ShowShop(int gold, int turn)
        {
            _isVisible = true;
            _currentGold = gold;
            _nextRefreshTurn = turn;
            UpdateGoldDisplay();

            gameObject.SetActive(true);
            StartCoroutine(AnimateShowShop());
            RefreshShopDisplay();
        }

        /// <summary>离开商店</summary>
        public void HideShop()
        {
            _isVisible = false;
            StartCoroutine(AnimateHideShop());
        }

        /// <summary>设置当前金币（从外部更新）</summary>
        public void SetGold(int gold)
        {
            _currentGold = gold;
            UpdateGoldDisplay();
            RefreshCardAffordability();
        }

        /// <summary>刷新商店卡牌（重新随机生成）</summary>
        public void RefreshCards(List<CardData> newCards)
        {
            ClearShopCards();
            foreach (var card in newCards)
                AddShopCard(card);
            StartCoroutine(AnimateDealCards());
        }

        /// <summary>设置重投花费</summary>
        public void SetRerollCost(int cost)
        {
            _rerollCost = cost;
            _rerollCostText.text = $"{cost} Gold";
        }

        /// <summary>设置已用重投次数</summary>
        public void SetRerollsUsed(int used, int max)
        {
            _rerollsUsed = used;
            _maxRerolls = max;
            _rerollsUsedText.text = $"{used}/{max} rerolls used";
            _rerollButton.interactable = used < max && _currentGold >= _rerollCost;
        }

        // ── Private Methods ────────────────────────────────────

        private void ClearShopCards()
        {
            foreach (var c in _shopCards)
                if (c != null) Destroy(c.gameObject);
            _shopCards.Clear();
        }

        private void AddShopCard(CardData data)
        {
            var go = Instantiate(_shopCardPrefab, _cardGridContainer);
            var ui = go.GetComponent<ShopCardUI>();
            ui.Init(data, _currentGold >= data.ShopPrice, OnShopCardClicked, OnShopCardHover, OnShopCardHoverExit);
            _shopCards.Add(ui);
        }

        private void RefreshShopDisplay()
        {
            _nextRefreshText.text = $"Next refresh: Turn {_nextRefreshTurn}";
            UpdateGoldDisplay();
        }

        private void UpdateGoldDisplay()
        {
            if (_goldText != null)
            {
                _goldText.text = $"🪙 {_currentGold}";
                _goldText.color = _goldColor;
            }
        }

        private void RefreshCardAffordability()
        {
            foreach (var card in _shopCards)
                card?.SetAffordable(_currentGold >= card.Price);
        }

        private void OnShopCardClicked(ShopCardUI card)
        {
            if (_currentGold < card.Price)
            {
                PlaySFX(_rejectSFX);
                card.PlayRejectAnimation();
                return;
            }

            // 购买确认
            _currentGold -= card.Price;
            UpdateGoldDisplay();

            PlaySFX(_buySFX);
            StartCoroutine(AnimateCardPurchase(card));
            OnCardPurchased?.Invoke(card.Data);
        }

        private void OnShopCardHover(ShopCardUI card)
        {
            ShowPreview(card);
        }

        private void OnShopCardHoverExit(ShopCardUI card)
        {
            HidePreview();
        }

        private void ShowPreview(ShopCardUI card)
        {
            if (_previewPopup == null || card == null) return;
            _previewPopup.gameObject.SetActive(true);
            if (_previewName != null) _previewName.text = card.Data?.CardName ?? "";
            // 填充详细属性（攻击力/防御力/描述）
            if (_previewStats != null && card.Data != null)
                _previewStats.text = $"ATK: {card.Data.Attack}\nDEF: {card.Data.Defense}\n{card.Data.Description}";
        }

        private void HidePreview()
        {
            if (_previewPopup != null)
                _previewPopup.gameObject.SetActive(false);
        }

        private void RequestReroll()
        {
            if (_currentGold < _rerollCost) { PlaySFX(_rejectSFX); return; }
            if (_rerollsUsed >= _maxRerolls) return;

            _currentGold -= _rerollCost;
            _rerollsUsed++;
            UpdateGoldDisplay();
            _rerollsUsedText.text = $"{_rerollsUsed}/{_maxRerolls} rerolls used";

            PlaySFX(_rerollSFX);
            StartCoroutine(AnimateReroll());
            OnRerollRequested?.Invoke();
        }

        private void LeaveShop()
        {
            HidePreview();
            OnLeaveShop?.Invoke();
            HideShop();
        }

        // ── Animations ─────────────────────────────────────────

        private System.Collections.IEnumerator AnimateShowShop()
        {
            _shopCanvasGroup.blocksRaycasts = false;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _shopCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            _shopCanvasGroup.alpha = 1f;
            _shopCanvasGroup.blocksRaycasts = true;
        }

        private System.Collections.IEnumerator AnimateHideShop()
        {
            _shopCanvasGroup.blocksRaycasts = false;
            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _shopCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            _shopCanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator AnimateDealCards()
        {
            float delay = 0.1f;
            for (int i = 0; i < _shopCards.Count; i++)
            {
                var card = _shopCards[i];
                if (card == null) continue;

                var startPos = card.transform.localPosition + new Vector3(0, -100f, 0);
                var target = card.transform.localPosition;
                card.transform.localPosition = startPos;

                float elapsed = 0f;
                float duration = 0.35f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float ease = 1f - (1f - t) * (1f - t);
                    card.transform.localPosition = Vector3.Lerp(startPos, target, ease);
                    yield return null;
                }
                card.transform.localPosition = target;

                yield return new WaitForSeconds(delay);
            }
        }

        private System.Collections.IEnumerator AnimateCardPurchase(ShopCardUI card)
        {
            // 卡牌缩小飞向手牌区
            if (card == null) yield break;
            var start = card.transform.position;
            var target = new Vector3(Screen.width / 2f, 0, 0);

            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = 1f - (1f - t) * (1f - t);
                card.transform.position = Vector3.Lerp(start, target, ease);
                card.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, t);
                yield return null;
            }

            _shopCards.Remove(card);
            Destroy(card.gameObject);
            RefreshCardAffordability();
        }

        private System.Collections.IEnumerator AnimateReroll()
        {
            // 所有卡牌翻转 → 重新生成 → 再翻转回来
            foreach (var card in _shopCards)
            {
                if (card != null) card.PlayFlipAnimation();
            }
            yield return new WaitForSeconds(0.3f);
            // OnRerollRequested 触发后，外部逻辑会调用 RefreshCards
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            var src = gameObject.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = 0.6f;
            src.Play();
            Destroy(src, clip.length + 0.1f);
        }

        // ───────────────────────────────────────────────────────
        // ShopCardUI 子组件（挂载在 ShopCard Prefab 上）
        // ───────────────────────────────────────────────────────

        /// <summary>
        /// 商店中单张卡牌的UI组件
        /// </summary>
        public class ShopCardUI : MonoBehaviour
        {
            [SerializeField] private Image _frame;
            [SerializeField] private Image _artImage;
            [SerializeField] private TextMeshProUGUI _nameText;
            [SerializeField] private TextMeshProUGUI _priceText;
            [SerializeField] private GameObject _affordIndicator; // 绿/红边框

            public CardData Data { get; private set; }
            public int Price => Data?.ShopPrice ?? 0;

            private Action<ShopCardUI> _onClick;
            private Action<ShopCardUI> _onHover;
            private Action<ShopCardUI> _onHoverExit;
            private bool _isAffordable = true;

            public void Init(CardData data, bool affordable,
                Action<ShopCardUI> onClick,
                Action<ShopCardUI> onHover,
                Action<ShopCardUI> onHoverExit)
            {
                Data = data;
                _isAffordable = affordable;
                _onClick = onClick;
                _onHover = onHover;
                _onHoverExit = onHoverExit;

                if (_nameText != null) _nameText.text = data.CardName;
                if (_priceText != null) _priceText.text = $"🪙 {data.ShopPrice}";
                if (_artImage != null && data.CardArt != null) _artImage.sprite = data.CardArt;

                // 稀有度边框颜色
                SetRarityFrame(data.Rarity);
                SetAffordable(affordable);

                var trigger = gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = gameObject.AddComponent<EventTrigger>();

                var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                click.callback.AddListener(_ => _onClick?.Invoke(this));
                trigger.triggers.Add(click);

                var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                enter.callback.AddListener(_ => _onHover?.Invoke(this));
                trigger.triggers.Add(enter);

                var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                exit.callback.AddListener(_ => _onHoverExit?.Invoke(this));
                trigger.triggers.Add(exit);
            }

            public void SetAffordable(bool affordable)
            {
                _isAffordable = affordable;
                // 不可购买：灰阶 + 降低透明度
                var col = affordable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
                _frame.color = col;
                _artImage.color = col;
                _nameText.color = col;
                _priceText.color = affordable
                    ? new Color(0.722f, 0.525f, 0.043f)
                    : new Color(0.5f, 0.5f, 0.5f);
            }

            private void SetRarityFrame(ECardRarity rarity)
            {
                if (_frame == null) return;
                _frame.color = rarity switch
                {
                    ECardRarity.Common    => new Color(0.353f, 0.353f, 0.431f),    // 灰色
                    ECardRarity.Rare      => new Color(0.118f, 0.420f, 0.549f),   // 蓝色
                    ECardRarity.Epic      => new Color(0.420f, 0.176f, 0.545f),   // 紫色
                    ECardRarity.Legendary => new Color(0.722f, 0.525f, 0.043f),   // 金色
                    _ => Color.gray
                };
            }

            public void PlayRejectAnimation()
            {
                // 抖动 + 红闪
                StartCoroutine(ShakeAnim());
            }

            public void PlayFlipAnimation()
            {
                StartCoroutine(FlipAnim());
            }

            private System.Collections.IEnumerator ShakeAnim()
            {
                var start = transform.localPosition;
                for (int i = 0; i < 4; i++)
                {
                    float offset = (i % 2 == 0 ? 1f : -1f) * 8f;
                    transform.localPosition = start + new Vector3(offset, 0, 0);
                    yield return new WaitForSeconds(0.05f);
                }
                transform.localPosition = start;
            }

            private System.Collections.IEnumerator FlipAnim()
            {
                float elapsed = 0f;
                float duration = 0.15f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float scaleX = Mathf.Lerp(1f, 0f, t);
                    transform.localScale = new Vector3(scaleX, 1f, 1f);
                    yield return null;
                }
                transform.localScale = new Vector3(0f, 1f, 1f);
                yield return new WaitForSeconds(0.05f);
                while (elapsed > 0)
                {
                    elapsed -= Time.deltaTime;
                    float t = elapsed / duration;
                    float scaleX = Mathf.Lerp(0f, 1f, t);
                    transform.localScale = new Vector3(scaleX, 1f, 1f);
                    yield return null;
                }
                transform.localScale = Vector3.one;
            }
        }
    }
}