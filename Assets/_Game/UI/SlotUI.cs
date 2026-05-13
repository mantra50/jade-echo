using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardMatch.UI
{
    /// <summary>
    /// 存档槽位UI — 3个存档槽，支持存读档操作
    /// 对接 CardMatch.SaveSystem.SlotData 模块
    /// 东方古风卡通写实美术风格
    /// </summary>
    public class SlotUI : MonoBehaviour
    {
        public static SlotUI Instance { get; private set; }

        public const int SlotCount = 3;

        [Header("Slot Container")]
        [SerializeField] private RectTransform _slotContainer;

        [Header("Slot Prefab")]
        [SerializeField] private GameObject _slotPrefab;

        [Header("Slot Panel")]
        [SerializeField] private CanvasGroup _slotPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Empty Slot Indicator")]
        [SerializeField] private Sprite _emptySlotSprite;
        [SerializeField] private Sprite _filledSlotSprite;

        [Header("Colors")]
        [SerializeField] private Color _slotEmptyColor  = new Color(0.353f, 0.353f, 0.431f, 0.5f);
        [SerializeField] private Color _slotFilledColor = new Color(0.722f, 0.525f, 0.043f);
        [SerializeField] private Color _buttonNormalColor = new Color(0.271f, 0.482f, 0.616f);
        [SerializeField] private Color _buttonHoverColor  = new Color(0.361f, 0.627f, 0.800f);

        [Header("Animations")]
        [SerializeField] private float _slotRevealDuration = 0.3f;
        [SerializeField] private float _slotRevealDelay     = 0.08f;

        // ── Runtime State ──────────────────────────────────────
        private readonly List<SaveSlotItem> _slots = new();
        private bool _isVisible;
        private bool _isSaveMode;

        // ── Events ─────────────────────────────────────────────
        public event Action<int> OnSaveRequested;
        public event Action<int> OnLoadRequested;
        public event Action OnCloseRequested;

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
            _closeButton?.onClick.AddListener(() => OnCloseRequested?.Invoke());
            BuildSlots();
        }

        // ══════════════════════════════════════════════════════
        //  Public API
        // ══════════════════════════════════════════════════════

        /// <summary>显示存档槽（存/读档模式）</summary>
        public void Show(bool saveMode)
        {
            _isSaveMode = saveMode;
            _isVisible = true;
            gameObject.SetActive(true);
            RefreshAllSlots();
            StartCoroutine(AnimateShow());
        }

        /// <summary>隐藏存档槽</summary>
        public void Hide()
        {
            _isVisible = false;
            StartCoroutine(AnimateHide());
        }

        /// <summary>刷新所有槽位</summary>
        public void RefreshAllSlots()
        {
            for (int i = 0; i < SlotCount; i++)
                RefreshSlot(i);
        }

        /// <summary>刷新单个槽位</summary>
        public void RefreshSlot(int index)
        {
            if (index < 0 || index >= _slots.Count) return;
            var slot = _slots[index];
            var saveData = CardMatch.SaveSystem.SlotData.Load(index);

            if (saveData == null)
                SetSlotEmpty(slot);
            else
                SetSlotFilled(slot, saveData);
        }

        // ══════════════════════════════════════════════════════
        //  Private Methods
        // ══════════════════════════════════════════════════════

        private void BuildSlots()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                var go = Instantiate(_slotPrefab, _slotContainer);
                var slot = new SaveSlotItem
                {
                    Index = i,
                    GameObject = go,
                    Root = go.GetComponent<RectTransform>()
                };

                slot.SlotIndexText = go.transform.Find("IndexText")?.GetComponent<TextMeshProUGUI>();
                slot.SlotNameText  = go.transform.Find("SlotNameText")?.GetComponent<TextMeshProUGUI>();
                slot.SlotDateText  = go.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
                slot.SlotLevelText = go.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
                slot.SlotThumbnail = go.transform.Find("Thumbnail")?.GetComponent<Image>();
                slot.SaveButton    = go.transform.Find("SaveButton")?.GetComponent<Button>();
                slot.LoadButton    = go.transform.Find("LoadButton")?.GetComponent<Button>();
                slot.DeleteButton  = go.transform.Find("DeleteButton")?.GetComponent<Button>();

                if (slot.SlotIndexText != null)
                    slot.SlotIndexText.text = $"第 {i + 1} 槽";

                int idx = i;
                slot.SaveButton?.onClick.AddListener(() => RequestSave(idx));
                slot.LoadButton?.onClick.AddListener(() => RequestLoad(idx));
                slot.DeleteButton?.onClick.AddListener(() => RequestDelete(idx));

                _slots.Add(slot);
            }
        }

        private void SetSlotEmpty(SaveSlotItem slot)
        {
            if (slot.SlotNameText != null)
            {
                slot.SlotNameText.text = "( 空存档槽 )";
                slot.SlotNameText.color = _slotEmptyColor;
            }
            if (slot.SlotDateText != null) slot.SlotDateText.text = "";
            if (slot.SlotLevelText != null) slot.SlotLevelText.text = "";
            if (slot.SlotThumbnail != null) slot.SlotThumbnail.sprite = _emptySlotSprite;

            SetButtonInteractable(slot, false, false);
        }

        private void SetSlotFilled(SaveSlotItem slot, CardMatch.SaveSystem.SlotData.Data data)
        {
            if (slot.SlotNameText != null)
            {
                slot.SlotNameText.text = data.SaveName ?? $"存档 #{data.Index + 1}";
                slot.SlotNameText.color = _slotFilledColor;
            }
            if (slot.SlotDateText != null)
                slot.SlotDateText.text = data.Timestamp != null
                    ? data.Timestamp.Value.ToString("yyyy-MM-dd HH:mm") : "";
            if (slot.SlotLevelText != null)
                slot.SlotLevelText.text = $"等级 {data.Level}";
            if (slot.SlotThumbnail != null && _filledSlotSprite != null)
                slot.SlotThumbnail.sprite = _filledSlotSprite;

            SetButtonInteractable(slot, true, true);
        }

        private void SetButtonInteractable(SaveSlotItem slot, bool canSave, bool canLoad)
        {
            if (slot.SaveButton != null)
                slot.SaveButton.interactable = canSave || _isSaveMode;
            if (slot.LoadButton != null)
                slot.LoadButton.interactable = canLoad;
            if (slot.DeleteButton != null)
                slot.DeleteButton.interactable = canLoad;
        }

        private void RequestSave(int index)
        {
            OnSaveRequested?.Invoke(index);
            Hide();
        }

        private void RequestLoad(int index)
        {
            OnLoadRequested?.Invoke(index);
            Hide();
        }

        private void RequestDelete(int index)
        {
            CardMatch.SaveSystem.SlotData.Delete(index);
            RefreshSlot(index);
        }

        // ══════════════════════════════════════════════════════
        //  Animations
        // ══════════════════════════════════════════════════════

        private System.Collections.IEnumerator AnimateShow()
        {
            _slotPanel.alpha = 0f;
            _slotPanel.blocksRaycasts = false;

            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot?.GameObject == null) continue;

                slot.GameObject.transform.localScale = Vector3.zero;
                float elapsed = 0f;

                while (elapsed < _slotRevealDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / _slotRevealDuration;
                    float spring = Mathf.Sin(t * Mathf.PI * 4) * (1f - t) + t;
                    slot.GameObject.transform.localScale = Vector3.one * spring;
                    yield return null;
                }
                slot.GameObject.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(_slotRevealDelay);
            }

            float panelElapsed = 0f;
            while (panelElapsed < 0.3f)
            {
                panelElapsed += Time.deltaTime;
                float t = panelElapsed / 0.3f;
                _slotPanel.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            _slotPanel.alpha = 1f;
            _slotPanel.blocksRaycasts = true;
        }

        private System.Collections.IEnumerator AnimateHide()
        {
            _slotPanel.blocksRaycasts = false;
            float elapsed = 0f;

            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                _slotPanel.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            _slotPanel.alpha = 0f;
            gameObject.SetActive(false);
        }

        // ══════════════════════════════════════════════════════
        //  Supporting Types
        // ══════════════════════════════════════════════════════

        private class SaveSlotItem
        {
            public int Index;
            public GameObject GameObject;
            public RectTransform Root;
            public TextMeshProUGUI SlotIndexText;
            public TextMeshProUGUI SlotNameText;
            public TextMeshProUGUI SlotDateText;
            public TextMeshProUGUI SlotLevelText;
            public Image SlotThumbnail;
            public Button SaveButton;
            public Button LoadButton;
            public Button DeleteButton;
        }
    }
}