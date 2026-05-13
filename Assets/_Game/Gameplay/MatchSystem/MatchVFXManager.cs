using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay.MatchSystem
{
    /// <summary>
    /// 消除特效类型 — 对应不同的视觉反馈
    /// </summary>
    public enum EMatchType
    {
        Normal,      // 3连基础消除
        FourLine,    // 4连/5连剑气横贯
        LShape,      // L型终极消除
        TShape,      // T型终极消除
        CrossUltimate // 终极连锁（十字爆炸）
    }

    /// <summary>
    /// 消除特效管理器 — 集成到 CascadeController / GridManager
    /// 根据匹配类型触发对应的粒子特效或占位符颜色块
    /// 东方古风卡通写实美术风格
    /// </summary>
    public class MatchVFXManager : MonoBehaviour
    {
        public static MatchVFXManager Instance { get; private set; }

        [Header("VFX Container")]
        [SerializeField] private Transform _vfxContainer;

        [Header("VFX Prefabs — 消除特效")]
        [SerializeField] private GameObject _vfxNormal;     // 3连：樱花+墨迹
        [SerializeField] private GameObject _vfxFourLine;   // 4连：东方剑气
        [SerializeField] private GameObject _vfxLShape;      // L型：东方符阵
        [SerializeField] private GameObject _vfxTShape;     // T型：东方符阵
        [SerializeField] private GameObject _vfxCrossUltimate; // 十字终极连锁

        [Header("VFX Prefabs — 辅助特效")]
        [SerializeField] private GameObject _damagePopupPrefab; // 伤害数字
        [SerializeField] private GameObject _energyBoostPrefab;  // 能量灌注

        [Header("Color Palette — 东方古风")]
        [SerializeField] private Color _sakuraPink   = new Color(1.000f, 0.718f, 0.773f); // #FFB7C5
        [SerializeField] private Color _inkGray      = new Color(0.176f, 0.176f, 0.227f); // #2D2D3A
        [SerializeField] private Color _goldAccent   = new Color(1.000f, 0.843f, 0.000f); // #FFD700
        [SerializeField] private Color _purpleRune   = new Color(0.608f, 0.365f, 0.898f); // #9B5DE5

        // ── Runtime ───────────────────────────────────────────
        private readonly Queue<GameObject> _activeVFX = new();
        private const int MAX_VFX = 20;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ══════════════════════════════════════════════════════
        //  Public API — 供 CascadeController / BoardUI 调用
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// 播放消除特效（棋子坐标列表 + 匹配类型）
        /// </summary>
        public void PlayEliminationVFX(List<Vector2Int> cells, EMatchType matchType)
        {
            if (cells == null || cells.Count == 0) return;

            // 根据匹配类型选择 VFX 预制体
            var prefab = GetVFXPrefab(matchType);
            if (prefab == null)
            {
                // Fallback：使用普通占位符颜色块
                PlayColorBlockVFX(cells, matchType);
                return;
            }

            foreach (var pos in cells)
            {
                var worldPos = GridToWorld(pos.x, pos.y);
                var vfx = Instantiate(prefab, worldPos, Quaternion.identity, _vfxContainer);
                _activeVFX.Enqueue(vfx);

                var ps = vfx.GetComponent<ParticleSystem>();
                float duration = ps != null ? ps.main.duration + 0.5f : 1.5f;
                Destroy(vfx, duration);
            }

            TrimVFXQueue();
        }

        /// <summary>
        /// 播放伤害数字弹出（敌人受伤害）
        /// </summary>
        public void PlayDamagePopup(Vector2 worldPos, int damage, bool isCrit)
        {
            if (_damagePopupPrefab == null) return;

            var popup = Instantiate(_damagePopupPrefab, worldPos, Quaternion.identity, _vfxContainer);
            var text = popup.GetComponent<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"-{damage}";
                text.color = isCrit ? _goldAccent : new Color(0.902f, 0.224f, 0.275f); // #E63946
                text.fontSize = isCrit ? 44 : 32;
                if (isCrit) text.text += " CRIT!";
            }

            // 动画：Bounce弹出 + 上移消散
            StartCoroutine(AnimateDamagePopup(popup, isCrit));
        }

        /// <summary>
        /// 播放治疗数字弹出
        /// </summary>
        public void PlayHealPopup(Vector2 worldPos, int amount)
        {
            if (_damagePopupPrefab == null) return;

            var popup = Instantiate(_damagePopupPrefab, worldPos, Quaternion.identity, _vfxContainer);
            var text = popup.GetComponent<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"+{amount}";
                text.color = new Color(0.165f, 0.616f, 0.561f); // #2A9D8F
                text.fontSize = 28;
            }
            StartCoroutine(AnimateDamagePopup(popup, false));
        }

        /// <summary>
        /// 播放能量灌注特效
        /// </summary>
        public void PlayEnergyBoost(Vector2 from, Vector2 to)
        {
            if (_energyBoostPrefab == null) return;
            var vfx = Instantiate(_energyBoostPrefab, from, Quaternion.identity, _vfxContainer);
            StartCoroutine(AnimateEnergyBoost(vfx, from, to));
        }

        // ══════════════════════════════════════════════════════
        //  Private Methods
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// 根据匹配类型返回 VFX 预制体
        /// </summary>
        private GameObject GetVFXPrefab(EMatchType matchType) => matchType switch
        {
            EMatchType.Normal      => _vfxNormal,
            EMatchType.FourLine    => _vfxFourLine,
            EMatchType.LShape      => _vfxLShape,
            EMatchType.TShape      => _vfxTShape,
            EMatchType.CrossUltimate => _vfxCrossUltimate,
            _ => _vfxNormal
        };

        /// <summary>
        /// 将 Grid 坐标转换为世界坐标（棋盘格中心点）
        /// </summary>
        private Vector2 GridToWorld(int col, int row)
        {
            // 假设棋盘 8×8，CellSize = 80，Gap = 4
            float cellSize = 80f;
            float gap = 4f;
            float totalW = 8 * (cellSize + gap) - gap;
            float totalH = 8 * (cellSize + gap) - gap;
            float x = col * (cellSize + gap) - totalW / 2f + cellSize / 2f;
            float y = -(row * (cellSize + gap)) + totalH / 2f - cellSize / 2f;
            return new Vector2(x, y);
        }

        /// <summary>
        /// 占位符颜色块 VFX（当预制体不存在时使用）
        /// 根据不同匹配类型显示不同颜色
        /// </summary>
        private void PlayColorBlockVFX(List<Vector2Int> cells, EMatchType matchType)
        {
            Color blockColor = matchType switch
            {
                EMatchType.Normal      => _sakuraPink,
                EMatchType.FourLine    => _goldAccent,
                EMatchType.LShape      => _purpleRune,
                EMatchType.TShape      => _purpleRune,
                EMatchType.CrossUltimate => new Color(1f, 0.4f, 0.8f),
                _ => _sakuraPink
            };

            foreach (var pos in cells)
            {
                var worldPos = GridToWorld(pos.x, pos.y);
                var block = GameObject.CreatePrimitive(PrimitiveType.Quad);
                block.transform.position = worldPos;
                block.transform.localScale = Vector3.one * 60f;
                block.transform.SetParent(_vfxContainer);

                var renderer = block.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = blockColor;

                // 动画：放大 → 淡出 → 销毁
                StartCoroutine(AnimateColorBlock(block, matchType));
            }
        }

        private System.Collections.IEnumerator AnimateColorBlock(GameObject block, EMatchType matchType)
        {
            float duration = matchType switch
            {
                EMatchType.Normal => 1.0f,
                EMatchType.FourLine => 1.2f,
                _ => 1.5f
            };

            float elapsed = 0f;
            float delay = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 放大
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
                block.transform.localScale = Vector3.one * 60f * scale;

                // 透明度衰减（ease-out）
                float alpha = 1f - (1f - t) * (1f - t);
                var col = block.GetComponent<Renderer>().material.color;
                col.a = 1f - alpha;
                block.GetComponent<Renderer>().material.color = col;

                yield return null;
            }
            Destroy(block);
        }

        private System.Collections.IEnumerator AnimateDamagePopup(GameObject popup, bool isCrit)
        {
            var text = popup.GetComponent<TMPro.TextMeshProUGUI>();
            if (text == null) { Destroy(popup); yield break; }

            float duration = isCrit ? 1.2f : 1.0f;
            float elapsed = 0f;

            // Bounce 弹出（前0.2s）
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.2f;
                float bounce = Mathf.Sin(t * Mathf.PI * 3) * (1f - t) + t;
                float yOffset = bounce * 50f;
                popup.transform.localPosition += new Vector3(0, yOffset * Time.deltaTime * 5f, 0);
                yield return null;
            }

            // 消散（0.2s 后）
            yield return new WaitForSeconds(0.5f);
            elapsed = 0f;
            float startAlpha = text.color.a;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                var col = text.color;
                col.a = Mathf.Lerp(startAlpha, 0f, t);
                text.color = col;
                popup.transform.localPosition += new Vector3(0, 20f * Time.deltaTime, 0);
                yield return null;
            }
            Destroy(popup);
        }

        private System.Collections.IEnumerator AnimateEnergyBoost(GameObject vfx, Vector2 from, Vector2 to)
        {
            float duration = 1.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // 弧线运动
                float x = Mathf.Lerp(from.x, to.x, t);
                float y = Mathf.Lerp(from.y, to.y, t) + Mathf.Sin(t * Mathf.PI) * 50f;
                vfx.transform.position = new Vector3(x, y, 0);
                yield return null;
            }
            Destroy(vfx, 0.5f);
        }

        private void TrimVFXQueue()
        {
            while (_activeVFX.Count > MAX_VFX)
            {
                var old = _activeVFX.Dequeue();
                if (old != null) Destroy(old);
            }
        }
    }

    // ═════════════════════════════════════════════════════════
    //  MatchVFX 扩展 — 与 CascadeController 集成
    // ═════════════════════════════════════════════════════════

    /// <summary>
    /// 扩展 CascadeController，自动触发 VFX
    /// 使用方式：在场景中放置 MatchVFXManager，CascadeController 触发时会自动调用
    /// </summary>
    public static class CascadeVFXBinder
    {
        public static void BindToCascade(CascadeController cascade)
        {
            cascade.OnPiecesEliminated += pieces =>
            {
                if (MatchVFXManager.Instance == null) return;

                // 根据消除数量和形状判断匹配类型
                var cells = new List<Vector2Int>();
                foreach (var p in pieces)
                    cells.Add(new Vector2Int(p.X, p.Y));

                var matchType = DetermineMatchType(pieces);
                MatchVFXManager.Instance.PlayEliminationVFX(cells, matchType);
            };

            cascade.OnComboIncremented += combo =>
            {
                // Combo 数字提示（后续扩展）
            };
        }

        private static EMatchType DetermineMatchType(List<Piece> pieces)
        {
            if (pieces.Count >= 8) return EMatchType.CrossUltimate;
            if (pieces.Count >= 6) return EMatchType.LShape; // 假设L/T
            return EMatchType.Normal;
        }
    }
}