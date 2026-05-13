using System;
using UnityEngine;

namespace CardMatch.SaveSystem
{
    /// <summary>
    /// 简单存档管理器 — 协调游戏状态与存档槽位
    /// 与 SlotUI 配合使用
    /// </summary>
    public class SimpleSaveManager : MonoBehaviour
    {
        public static SimpleSaveManager Instance { get; private set; }

        [Header("Auto-save")]
        [SerializeField] private bool _autoSaveEnabled = true;

        // ── Events ─────────────────────────────────────────────
        public event Action<int> OnSaveCompleted;
        public event Action<int> OnLoadCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ══════════════════════════════════════════════════════
        //  Public API
        // ══════════════════════════════════════════════════════

        /// <summary>保存当前游戏到指定槽位</summary>
        public void SaveGame(int slotIndex)
        {
            var data = new SlotData.Data
            {
                Index = slotIndex,
                SaveName = GetDefaultSaveName(slotIndex),
                Timestamp = DateTime.Now,
                Level = GetCurrentLevel(),
                Gold = GetCurrentGold(),
                Chapter = GetCurrentChapter(),
                SceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            };

            SlotData.Save(slotIndex, data);
            OnSaveCompleted?.Invoke(slotIndex);
            Debug.Log($"[SimpleSaveManager] Game saved to slot {slotIndex}");
        }

        /// <summary>从指定槽位加载游戏</summary>
        public void LoadGame(int slotIndex)
        {
            var data = SlotData.Load(slotIndex);
            if (data == null)
            {
                Debug.LogWarning($"[SimpleSaveManager] Slot {slotIndex} is empty");
                return;
            }

            // 加载场景
            if (data.SceneIndex >= 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(data.SceneIndex);
            }

            // TODO：恢复游戏状态（玩家HP/能量/手牌/棋盘状态）
            // RestoreGameState(data);

            OnLoadCompleted?.Invoke(slotIndex);
            Debug.Log($"[SimpleSaveManager] Game loaded from slot {slotIndex}");
        }

        /// <summary>检查指定槽位是否有存档</summary>
        public bool HasSave(int slotIndex) => SlotData.Exists(slotIndex);

        /// <summary>删除指定槽位存档</summary>
        public void DeleteSave(int slotIndex) => SlotData.Delete(slotIndex);

        // ══════════════════════════════════════════════════════
        //  Placeholder — 连接游戏状态（待集成）
        // ══════════════════════════════════════════════════════

        private string GetDefaultSaveName(int slotIndex) => $"Chapter 1 - Slot {slotIndex + 1}";
        private int GetCurrentLevel() => 1;  // TODO: 替换为真实等级
        private int GetCurrentGold() => 0;    // TODO: 替换为真实金币
        private int GetCurrentChapter() => 1; // TODO: 替换为真实章节
    }
}