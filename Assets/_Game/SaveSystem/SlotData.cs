using System;
using System.IO;

namespace CardMatch.SaveSystem
{
    /// <summary>
    /// 存档槽位数据管理 — 与 SlotUI / SimpleSaveManager 对接
    /// 3个存档槽位，JSON 序列化存储于 persistentDataPath
    /// </summary>
    public static class SlotData
    {
        private static readonly string[] SlotPaths = new string[3]
        {
            Path.Combine(Application.persistentDataPath, "save_slot_0.json"),
            Path.Combine(Application.persistentDataPath, "save_slot_1.json"),
            Path.Combine(Application.persistentDataPath, "save_slot_2.json")
        };

        [Serializable]
        public class Data
        {
            public int Index;
            public string SaveName;
            public DateTime? Timestamp;
            public int Level;
            public int Gold;
            public int Chapter;
            public int SceneIndex;

            // 扩展字段（游戏状态）
            public int PlayerHP;
            public int PlayerEnergy;
            public int CurrentBossHP;
            public string HandCardIDs; // JSON array of card ids
            public string BoardState;  // JSON grid state
        }

        /// <summary>加载指定槽位的存档数据（无存档返回 null）</summary>
        public static Data Load(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 3) return null;
            var path = SlotPaths[slotIndex];
            if (!File.Exists(path)) return null;

            try
            {
                var json = File.ReadAllText(path);
                return JsonUtility.FromJson<Data>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SlotData] Failed to load slot {slotIndex}: {ex.Message}");
                return null;
            }
        }

        /// <summary>保存数据到指定槽位</summary>
        public static void Save(int slotIndex, Data data)
        {
            if (slotIndex < 0 || slotIndex >= 3) return;
            data.Index = slotIndex;
            data.Timestamp = DateTime.Now;

            var json = JsonUtility.ToJson(data);
            File.WriteAllText(SlotPaths[slotIndex], json);
        }

        /// <summary>删除指定槽位存档</summary>
        public static void Delete(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 3) return;
            var path = SlotPaths[slotIndex];
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>检查指定槽位是否有存档</summary>
        public static bool Exists(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 3) return false;
            return File.Exists(SlotPaths[slotIndex]);
        }

        /// <summary>获取所有槽位的存档信息摘要</summary>
        public static Data[] LoadAllSlots()
        {
            var result = new Data[3];
            for (int i = 0; i < 3; i++)
                result[i] = Load(i);
            return result;
        }
    }
}