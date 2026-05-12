// ============================================================
// BoardUI.cs — 棋盘UI（8×7格坐标/消除反馈/特效动画触发点）
// 东方古风卡通写实风格
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CardMatch.Gameplay.MatchSystem;

namespace CardMatch.UI
{
    /// <summary>
    /// 管理棋盘格子、消除反馈、特效触发。
    /// 棋盘为 8列（宽）× 7行（高），
    /// 消除反馈通过粒子系统和动画实现。
    /// </summary>
    public class BoardUI : MonoBehaviour
    {
        public static BoardUI Instance { get; private set; }

        [Header("Board Grid")]
        [SerializeField] private RectTransform _boardContainer;
        [SerializeField] private GameObject _cellPrefab;      // 格子预制体

        [Header("Grid Dimensions")]
        public const int COLS = 8;
        public const int ROWS = 7;   // 玩家区4行 + 敌人区3行（参考规格）
        [SerializeField] private float _cellWidth  = 80f;
        [SerializeField] private float _cellHeight = 80f;
        [SerializeField] private float _cellGap     = 4f;

        [Header("Board Color Scheme")]
        [SerializeField] private Color _playerZoneColor  = new Color(0.271f, 0.482f, 0.616f);  // #457B9D
        [SerializeField] private Color _enemyZoneColor   = new Color(0.608f, 0.137f, 0.208f); // #9B2335
        [SerializeField] private Color _emptySlotColor   = new Color(0.176f, 0.176f, 0.267f, 0.5f); // 半透明
        [SerializeField] private Color _laneDividerColor  = new Color(0.420f, 0.176f, 0.545f); // #6B2D8C

        [Header("Effect Anchors")]
        [SerializeField] private Transform _particleContainer;
        [SerializeField] private GameObject _matchVFXPrefab;   // 消除特效预制
        [SerializeField] private GameObject _attackVFXPrefab;  // 攻击特效预制
        [SerializeField] private GameObject _shatterVFXPrefab; // 死亡碎裂特效预制
        [SerializeField] private GameObject _spellVFXPrefab;   // 法术特效预制

        [Header("Lane Divider")]
        [SerializeField] private RectTransform _laneDivider;   // 玩家/敌人区分界线

        // ── Runtime State ──────────────────────────────────────
        private BoardCellUI[,] _cells;
        private readonly List<BoardCellUI> _playerRowCells = new();  // 玩家侧格子
        private readonly List<BoardCellUI> _enemyRowCells   = new();  // 敌人侧格子

        // 特效队列（连续消除时叠加）
        private readonly Queue<GameObject> _activeVFX = new();

        // ── Events ─────────────────────────────────────────────
        public event Action<int, int, EBoardCellType> OnCellClicked;    // (col, row)
        public event Action OnBoardCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            BuildBoard();
            UpdateLaneDivider();
        }

        // ── Public API ─────────────────────────────────────────

        /// <summary>
        /// 获取指定坐标的格子UI
        /// </summary>
        public BoardCellUI GetCell(int col, int row)
        {
            if (col < 0 || col >= COLS || row < 0 || row >= ROWS) return null;
            return _cells[col, row];
        }

        /// <summary>
        /// 播放消除特效（3个及以上连续）
        /// </summary>
        public void PlayMatchVFX(List<Vector2Int> cells)
        {
            if (_matchVFXPrefab == null) return;

            foreach (var pos in cells)
            {
                var cell = GetCell(pos.x, pos.y);
                if (cell == null) continue;

                var vfx = Instantiate(_matchVFXPrefab, cell.transform.position, Quaternion.identity, _particleContainer);
                _activeVFX.Enqueue(vfx);

                // 东方古风消除粒子：樱花/墨迹效果
                var ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    // 延迟自动销毁
                    Destroy(vfx, ps.main.duration + 0.5f);
                }
                else
                {
                    Destroy(vfx, 1f);
                }
            }

            // 限制同屏特效数量
            while (_activeVFX.Count > 20)
            {
                var old = _activeVFX.Dequeue();
                if (old != null) Destroy(old);
            }
        }

        /// <summary>
        /// 播放攻击特效
        /// </summary>
        public void PlayAttackVFX(Vector2 from, Vector2 to)
        {
            if (_attackVFXPrefab == null) return;
            var vfx = Instantiate(_attackVFXPrefab, from, Quaternion.identity, _particleContainer);
            // 动画：从from飞向to
            StartCoroutine(AnimateVFXPath(vfx, from, to, 0.25f));
        }

        /// <summary>
        /// 播放死亡碎裂特效
        /// </summary>
        public void PlayShatterVFX(Vector2 pos)
        {
            if (_shatterVFXPrefab == null) return;
            var vfx = Instantiate(_shatterVFXPrefab, pos, Quaternion.identity, _particleContainer);
            Destroy(vfx, 1.5f);
        }

        /// <summary>
        /// 播放法术/技能特效
        /// </summary>
        public void PlaySpellVFX(Vector2 pos)
        {
            if (_spellVFXPrefab == null) return;
            var vfx = Instantiate(_spellVFXPrefab, pos, Quaternion.identity, _particleContainer);
            Destroy(vfx, 1.2f);
        }

        /// <summary>
        /// 设置棋盘格子的状态（普通/锁定/高亮）
        /// </summary>
        public void SetCellHighlight(int col, int row, EBoardCellHighlight highlight)
        {
            var cell = GetCell(col, row);
            if (cell != null) cell.SetHighlight(highlight);
        }

        /// <summary>
        /// 刷新整个棋盘显示
        /// </summary>
        public void RefreshBoard()
        {
            if (_cells == null) return;
            for (int c = 0; c < COLS; c++)
                for (int r = 0; r < ROWS; r++)
                    _cells[c, r]?.Refresh();
        }

        /// <summary>
        /// 获得当前棋盘占用状态（供MatchDetector使用）
        /// </summary>
        public EBoardCellType[,] GetBoardState()
        {
            var state = new EBoardCellType[COLS, ROWS];
            if (_cells == null) return state;

            for (int c = 0; c < COLS; c++)
                for (int r = 0; r < ROWS; r++)
                    if (_cells[c, r] != null)
                        state[c, r] = _cells[c, r].CellType;
            return state;
        }

        /// <summary>
        /// 批量设置棋子（格子内容）
        /// </summary>
        public void SetCellContent(int col, int row, EBoardCellType type, Sprite icon)
        {
            var cell = GetCell(col, row);
            if (cell != null) cell.SetContent(type, icon);
        }

        // ── Private Methods ────────────────────────────────────

        private void BuildBoard()
        {
            _cells = new BoardCellUI[COLS, ROWS];

            float totalW = COLS * (_cellWidth + _cellGap) - _cellGap;
            float totalH = ROWS * (_cellHeight + _cellGap) - _cellGap;

            // 玩家区占下方4行，敌人区占上方3行（参考规格）
            int playerRows = 4;
            int enemyRows = ROWS - playerRows; // 3

            for (int col = 0; col < COLS; col++)
            {
                for (int row = 0; row < ROWS; row++)
                {
                    var go = Instantiate(_cellPrefab, _boardContainer);
                    var rt = go.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(_cellWidth, _cellHeight);

                    // 居中定位
                    float x = col * (_cellWidth + _cellGap) - totalW / 2f + _cellWidth / 2f;
                    // Y: row=0在最上（敌人区），row=ROWS-1在最下（玩家区）
                    float y = -(row * (_cellHeight + _cellGap)) + totalH / 2f - _cellHeight / 2f;

                    rt.anchoredPosition = new Vector2(x, y);

                    var cellUI = go.GetComponent<BoardCellUI>();
                    bool isPlayer = row < playerRows;
                    cellUI.Init(col, row, isPlayer);
                    _cells[col, row] = cellUI;

                    if (isPlayer) _playerRowCells.Add(cellUI);
                    else _enemyRowCells.Add(cellUI);

                    SetupCellEvents(cellUI, col, row);
                }
            }
        }

        private void SetupCellEvents(BoardCellUI cell, int col, int row)
        {
            var trigger = cell.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = cell.gameObject.AddComponent<EventTrigger>();

            var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            clickEntry.callback.AddListener(_ => OnCellClicked?.Invoke(col, row, cell.CellType));
            trigger.triggers.Add(clickEntry);
        }

        private void UpdateLaneDivider()
        {
            if (_laneDivider == null) return;
            // 定位在玩家区和敌人区分界线上
            float totalH = ROWS * (_cellHeight + _cellGap) - _cellGap;
            float playerRows = 4f;
            float y = totalH / 2f - playerRows * (_cellHeight + _cellGap) + _cellGap / 2f;
            _laneDivider.anchoredPosition = new Vector2(0, y);
        }

        private System.Collections.IEnumerator AnimateVFXPath(GameObject vfx, Vector2 from, Vector2 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && vfx != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                vfx.transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }
            if (vfx != null) Destroy(vfx);
        }

        // ── Board Cell UI 子组件 ──────────────────────────────

        /// <summary>
        /// 单个格子的UI组件（挂载在 Cell Prefab 上）
        /// </summary>
        public class BoardCellUI : MonoBehaviour
        {
            [SerializeField] private Image _bgImage;
            [SerializeField] private Image _iconImage;
            [SerializeField] private Image _highlightImage;
            [SerializeField] private GameObject _highlightRoot;

            public int Col { get; private set; }
            public int Row { get; private set; }
            public bool IsPlayer { get; private set; }
            public EBoardCellType CellType { get; private set; }

            private Color _baseColor;

            public void Init(int col, int row, bool isPlayer)
            {
                Col = col;
                Row = row;
                IsPlayer = isPlayer;
                CellType = EBoardCellType.Empty;
                _baseColor = isPlayer
                    ? new Color(0.176f, 0.176f, 0.267f, 0.4f)
                    : new Color(0.267f, 0.133f, 0.165f, 0.4f);
                if (_bgImage != null) _bgImage.color = _baseColor;
                if (_highlightRoot != null) _highlightRoot.SetActive(false);
            }

            public void SetContent(EBoardCellType type, Sprite icon)
            {
                CellType = type;
                if (_iconImage != null)
                {
                    _iconImage.sprite = icon;
                    _iconImage.enabled = icon != null;
                }
            }

            public void SetHighlight(EBoardCellHighlight highlight)
            {
                if (_highlightRoot == null) return;

                _highlightRoot.SetActive(highlight != EBoardCellHighlight.None);
                var col = highlight switch
                {
                    EBoardCellHighlight.Valid    => new Color(0.271f, 0.616f, 0.373f, 0.5f),
                    EBoardCellHighlight.Invalid => new Color(0.902f, 0.224f, 0.275f, 0.5f),
                    EBoardCellHighlight.Special => new Color(0.608f, 0.365f, 0.898f, 0.5f),
                    _ => Color.clear
                };
                if (_highlightImage != null) _highlightImage.color = col;
            }

            public void Refresh()
            {
                // 重置显示状态
            }
        }
    }

    // ── Supporting Enums ──────────────────────────────────────

    public enum EBoardCellType
    {
        Empty,
        PlayerMinion,
        EnemyMinion,
        Obstacle   // 障碍物（石头等）
    }

    public enum EBoardCellHighlight
    {
        None,
        Valid,     // 可放置
        Invalid,   // 不可放置
        Special    // 特殊高亮（如连消提示）
    }
}