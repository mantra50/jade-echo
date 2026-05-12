// ============================================================
// CardPanelUI.cs — 卡牌面板UI管理（手牌展示/拖拽/选中高亮）
// 东方古风卡通写实风格
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CardMatch.CardSystem;

namespace CardMatch.UI
{
    /// <summary>
    /// 管理玩家手牌区域的 UI 展示、拖拽交互、选中高亮。
    /// </summary>
    public class CardPanelUI : MonoBehaviour
    {
        public static CardPanelUI Instance { get; private set; }

        [Header("Card Panel Settings")]
        [SerializeField] private RectTransform _cardContainer;     // 手牌容器（水平排列）
        [SerializeField] private GameObject _cardPrefab;            // 手牌预制体
        [SerializeField] private int _maxHandSize = 10;

        [Header("Layout Settings")]
        [SerializeField] private float _cardSpacing = 160f;         // 卡牌间距
        [SerializeField] private float _cardArcHeight = -40f;       // 扇形弧度偏移
        [SerializeField] private float _hoverOffsetY = 30f;         // 悬停时上移距离

        [Header("Drag Settings")]
        [SerializeField] private Canvas _uiCanvas;
        [SerializeField] private RectTransform _boardArea;         // 棋盘有效区域
        [SerializeField] private float _dragScale = 1.08f;
        [SerializeField] private float _hoverScale = 1.05f;

        [Header("Colors — 东方古风色板")]
        [SerializeField] private Color _attackColor  = new Color(0.902f, 0.224f, 0.275f); // #E63946
        [SerializeField] private Color _defenseColor = new Color(0.271f, 0.482f, 0.616f); // #457B9D
        [SerializeField] private Color _transformColor = new Color(0.608f, 0.365f, 0.898f); // #9B5DE5
        [SerializeField] private Color _utilityColor  = new Color(0.165f, 0.616f, 0.561f); // #2A9D8F
        [SerializeField] private Color _selectedGlowColor = new Color(0.722f, 0.525f, 0.043f); // #B8860B

        [Header("SFX")]
        [SerializeField] private AudioClip _hoverSFX;
        [SerializeField] private AudioClip _selectSFX;
        [SerializeField] private AudioClip _playCardSFX;

        // ── Runtime State ──────────────────────────────────────
        private readonly List<CardUI> _handCards = new();
        private CardUI _selectedCard;
        private CardUI _hoveredCard;
        private CardUI _draggingCard;
        private bool _isDragging;

        // ── Events ─────────────────────────────────────────────
        public event Action<CardData> OnCardPlayed;       // 卡牌被打出
        public event Action<CardData> OnCardSelected;      // 卡牌被选中

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            UpdateCardPositions();
            HandleDrag();
        }

        // ── Public API ─────────────────────────────────────────

        /// <summary>添加一张手牌到面板</summary>
        public void AddCard(CardData data)
        {
            if (_handCards.Count >= _maxHandSize) return;
            StartCoroutine(AddCardAnimated(data));
        }

        /// <summary>从面板移除一张手牌（通常打出后）</summary>
        public void RemoveCard(CardUI card)
        {
            StartCoroutine(RemoveCardAnimated(card));
        }

        /// <summary>清理所有手牌</summary>
        public void ClearHand()
        {
            foreach (var c in _handCards)
                if (c != null) Destroy(c.gameObject);
            _handCards.Clear();
            _selectedCard = null;
        }

        /// <summary>设置某张卡牌为选中态</summary>
        public void SetSelected(CardUI card)
        {
            if (_selectedCard != null && _selectedCard != card)
                _selectedCard.SetSelected(false);

            _selectedCard = card;
            if (card != null)
            {
                card.SetSelected(true);
                OnCardSelected?.Invoke(card.Data);
            }
        }

        /// <summary>取消所有选中状态</summary>
        public void DeselectAll()
        {
            if (_selectedCard != null)
            {
                _selectedCard.SetSelected(false);
                _selectedCard = null;
            }
        }

        // ── Private Methods ────────────────────────────────────

        private System.Collections.IEnumerator AddCardAnimated(CardData data)
        {
            var go = Instantiate(_cardPrefab, _cardContainer);
            var cardUI = go.GetComponent<CardUI>();
            cardUI.Init(data);
            _handCards.Add(cardUI);

            SetupCardEvents(cardUI);
            UpdateCardPositions();

            // 入场动画：从下方弹入
            var startPos = go.transform.localPosition;
            go.transform.localPosition += new Vector3(0, -200f, 0);
            yield return SmoothMove(go, startPos, 0.35f);
        }

        private System.Collections.IEnumerator RemoveCardAnimated(CardUI card)
        {
            // 飞向棋盘方向动画
            Vector3 target = Camera.main != null
                ? Camera.main.transform.position + new Vector3(0, 0, 100f)
                : new Vector3(0, 200f, 0);

            var start = card.transform.position;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                card.transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            _handCards.Remove(card);
            Destroy(card.gameObject);
            UpdateCardPositions();
        }

        private void SetupCardEvents(CardUI cardUI)
        {
            var trigger = cardUI.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = cardUI.gameObject.AddComponent<EventTrigger>();

            // Hover
            var hoverEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            hoverEntry.callback.AddListener(_ => OnCardHover(cardUI));
            trigger.triggers.Add(hoverEntry);

            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => OnCardHoverExit(cardUI));
            trigger.triggers.Add(exitEntry);

            // Click
            var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            clickEntry.callback.AddListener(_ => OnCardClick(cardUI));
            trigger.triggers.Add(clickEntry);

            // BeginDrag
            var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            dragEntry.callback.AddListener(_ => OnBeginDrag(cardUI));
            trigger.triggers.Add(dragEntry);

            // EndDrag
            var endDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            endDragEntry.callback.AddListener(_ => OnEndDrag(cardUI));
            trigger.triggers.Add(endDragEntry);
        }

        private void OnCardHover(CardUI card)
        {
            if (_isDragging) return;
            _hoveredCard = card;
            card.SetHovered(true);
            // 弹起至中心位置
            card.transform.SetAsLastSibling();
            PlaySFX(_hoverSFX);
        }

        private void OnCardHoverExit(CardUI card)
        {
            if (_hoveredCard == card) _hoveredCard = null;
            card.SetHovered(false);
        }

        private void OnCardClick(CardUI card)
        {
            if (_isDragging) return;

            if (_selectedCard == card)
            {
                // 再次点击则尝试打出
                OnCardPlayed?.Invoke(card.Data);
            }
            else
            {
                SetSelected(card);
                PlaySFX(_selectSFX);
            }
        }

        private void OnBeginDrag(CardUI card)
        {
            _isDragging = true;
            _draggingCard = card;
            card.SetDragging(true);
            card.transform.SetAsLastSibling();
        }

        private void OnEndDrag(CardUI card)
        {
            if (!_isDragging) return;
            _isDragging = false;
            card.SetDragging(false);

            // 检测是否拖拽到棋盘区域
            if (IsOverBoardArea())
            {
                OnCardPlayed?.Invoke(card.Data);
                PlaySFX(_playCardSFX);
            }
            else
            {
                // 弹回原位
                StartCoroutine(SnapBack(card));
            }
            _draggingCard = null;
        }

        private void HandleDrag()
        {
            if (!_isDragging || _draggingCard == null) return;
            // 跟随鼠标
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _uiCanvas.GetComponent<RectTransform>(),
                Input.mousePosition,
                _uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _uiCanvas.worldCamera,
                out pos);
            _draggingCard.transform.localPosition = pos;
        }

        private bool IsOverBoardArea()
        {
            if (_boardArea == null) return false;
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _boardArea, Input.mousePosition, null, out mousePos);
            return _boardArea.rect.Contains(mousePos);
        }

        private System.Collections.IEnumerator SnapBack(CardUI card)
        {
            var target = card.transform.localPosition;
            var start = target + new Vector3(0, -30f, 0);
            card.transform.localPosition = start;
            yield return SmoothMove(card.gameObject, target, 0.2f);
        }

        private System.Collections.IEnumerator SmoothMove(GameObject go, Vector3 target, float duration)
        {
            Vector3 start = go.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // ease-out
                float ease = 1f - (1f - t) * (1f - t);
                go.transform.localPosition = Vector3.Lerp(start, target, ease);
                yield return null;
            }
            go.transform.localPosition = target;
        }

        /// <summary>
        /// 更新所有手牌的排列位置（扇形布局）
        /// </summary>
        private void UpdateCardPositions()
        {
            int count = _handCards.Count;
            if (count == 0) return;

            float totalWidth = (count - 1) * _cardSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                var card = _handCards[i];
                if (card == null) continue;

                float x = startX + i * _cardSpacing;
                // 扇形中间高，两边低
                float arcT = Mathf.Abs(i - (count - 1) / 2f) / Mathf.Max(1f, (count - 1) / 2f);
                float y = -arcT * _cardArcHeight;
                float z = -arcT * 5f; // 深度偏移

                Vector3 pos = new Vector3(x, y, z);
                float scale = 1f;

                if (_hoveredCard == card)
                {
                    pos.y += _hoverOffsetY;
                    scale = _hoverScale;
                }
                else if (_draggingCard == card)
                {
                    scale = _dragScale;
                }
                else if (_selectedCard == card)
                {
                    pos.y += _hoverOffsetY * 0.6f;
                    scale = 1.02f;
                }

                card.transform.localPosition = Vector3.Lerp(
                    card.transform.localPosition, pos, Time.deltaTime * 12f);
                card.transform.localScale = Vector3.Lerp(
                    card.transform.localScale, Vector3.one * scale, Time.deltaTime * 12f);
            }
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

        /// <summary>
        /// 根据卡牌类型获取对应颜色
        /// </summary>
        public Color GetTypeColor(ECardType type) => type switch
        {
            ECardType.Attack    => _attackColor,
            ECardType.Defense   => _defenseColor,
            ECardType.Transform => _transformColor,
            ECardType.Utility   => _utilityColor,
            _ => Color.white
        };

        public int HandCount => _handCards.Count;
    }
}