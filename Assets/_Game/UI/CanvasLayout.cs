// ============================================================
// CanvasLayout.cs — 主界面Canvas布局，含各层级Z序
// 东方古风卡通写实风格
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardMatch.UI
{
    /// <summary>
    /// 管理主界面 Canvas 的层级结构和 Z 序。
    /// 负责所有 UI 面板的加载顺序与深度控制。
    /// </summary>
    public class CanvasLayout : MonoBehaviour
    {
        public static CanvasLayout Instance { get; private set; }

        [Header("Canvas References")]
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private RectTransform _root;

        [Header("Layer Canvases")]
        [SerializeField] private Canvas _boardCanvas;      // 棋盘层
        [SerializeField] private Canvas _cardCanvas;        // 卡牌层
        [SerializeField] private Canvas _hudCanvas;          // HUD层
        [SerializeField] private Canvas _shopCanvas;         // 商店层
        [SerializeField] private Canvas _popupCanvas;        // 弹窗层
        [SerializeField] private Canvas _overlayCanvas;      // 特效遮罩层
        [SerializeField] private Canvas _topmostCanvas;      // 最顶层（加载/提示）

        [Header("Sort Order（从小到大）")]
        [Tooltip("Layer值越大，渲染越在上层")]
        public int BoardSortOrder      = 10;
        public int CardSortOrder       = 20;  // 手牌在棋盘上方
        public int HUDSortOrder        = 30;
        public int ShopSortOrder       = 40;
        public int PopupSortOrder      = 50;
        public int OverlaySortOrder    = 60;  // 特效动画
        public int TopmostSortOrder    = 70;

        // 层级深度（Z轴）
        private const float Z_BOARD     = 0f;
        private const float Z_CARD      = -10f;
        private const float Z_HUD       = -20f;
        private const float Z_SHOP      = -30f;
        private const float Z_POPUP     = -40f;
        private const float Z_OVERLAY   = -50f;
        private const float Z_TOPMOST   = -60f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ApplySortOrders();
            ApplyZOrder();
        }

        private void Start()
        {
            // 默认所有层关闭，由各Manager控制显隐
            SetLayerActive(ELayer.Shop, false);
            SetLayerActive(ELayer.Popup, false);
            SetLayerActive(ELayer.Topmost, false);
        }

        /// <summary>
        /// 设置指定层级的激活状态
        /// </summary>
        public void SetLayerActive(ELayer layer, bool active)
        {
            Canvas c = GetLayerCanvas(layer);
            if (c != null)
                c.gameObject.SetActive(active);
        }

        /// <summary>
        /// 获取层级对应的 Canvas
        /// </summary>
        public Canvas GetLayerCanvas(ELayer layer)
        {
            return layer switch
            {
                ELayer.Board    => _boardCanvas,
                ELayer.Card     => _cardCanvas,
                ELayer.HUD      => _hudCanvas,
                ELayer.Shop     => _shopCanvas,
                ELayer.Popup    => _popupCanvas,
                ELayer.Overlay  => _overlayCanvas,
                ELayer.Topmost  => _topmostCanvas,
                _ => null
            };
        }

        /// <summary>
        /// 临时提升某层SortOrder以显示在最前
        /// </summary>
        public void BringToFront(ELayer layer, int delta = 10)
        {
            Canvas c = GetLayerCanvas(layer);
            if (c != null)
                c.sortingOrder += delta;
        }

        /// <summary>
        /// 恢复指定层的SortOrder
        /// </summary>
        public void ResetSortOrder(ELayer layer)
        {
            Canvas c = GetLayerCanvas(layer);
            if (c == null) return;

            int order = layer switch
            {
                ELayer.Board    => BoardSortOrder,
                ELayer.Card     => CardSortOrder,
                ELayer.HUD      => HUDSortOrder,
                ELayer.Shop     => ShopSortOrder,
                ELayer.Popup    => PopupSortOrder,
                ELayer.Overlay  => OverlaySortOrder,
                ELayer.Topmost  => TopmostSortOrder,
                _ => 0
            };
            c.sortingOrder = order;
        }

        private void ApplySortOrders()
        {
            if (_boardCanvas    != null) _boardCanvas.sortingOrder    = BoardSortOrder;
            if (_cardCanvas     != null) _cardCanvas.sortingOrder     = CardSortOrder;
            if (_hudCanvas      != null) _hudCanvas.sortingOrder      = HUDSortOrder;
            if (_shopCanvas     != null) _shopCanvas.sortingOrder     = ShopSortOrder;
            if (_popupCanvas    != null) _popupCanvas.sortingOrder    = PopupSortOrder;
            if (_overlayCanvas  != null) _overlayCanvas.sortingOrder  = OverlaySortOrder;
            if (_topmostCanvas  != null) _topmostCanvas.sortingOrder = TopmostSortOrder;
        }

        private void ApplyZOrder()
        {
            SetLocalZ(_boardCanvas,   Z_BOARD);
            SetLocalZ(_cardCanvas,    Z_CARD);
            SetLocalZ(_hudCanvas,     Z_HUD);
            SetLocalZ(_shopCanvas,    Z_SHOP);
            SetLocalZ(_popupCanvas,   Z_POPUP);
            SetLocalZ(_overlayCanvas,Z_OVERLAY);
            SetLocalZ(_topmostCanvas, Z_TOPMOST);
        }

        private void SetLocalZ(Component c, float z)
        {
            if (c == null) return;
            var rt = c.transform as RectTransform;
            if (rt != null)
                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, z);
            else
                c.transform.localPosition = new Vector3(
                    c.transform.localPosition.x, c.transform.localPosition.y, z);
        }

        public enum ELayer
        {
            Board,
            Card,
            HUD,
            Shop,
            Popup,
            Overlay,
            Topmost
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Sort Orders")]
        private void EditorApply()
        {
            ApplySortOrders();
            ApplyZOrder();
        }
#endif
    }
}