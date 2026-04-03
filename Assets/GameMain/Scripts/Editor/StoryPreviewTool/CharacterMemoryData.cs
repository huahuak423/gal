using UnityEngine;
using System;
using System.Collections.Generic;
using AVGGame;

namespace AVGGame.Editor
{
    /// <summary>
    /// 立绘偏移记忆条目
    /// </summary>
    [Serializable]
    public class CharacterMemoryEntry
    {
        public string GraphName;          // 节点图名称
        public string SpritePath;         // 立绘路径
        public CharacterPosition Position; // 槽位 (Left/Center/Right)
        public float OffsetX;
        public float OffsetY;
        public float Scale = 1f;
        public string LastNodeGuid;       // 最后一次设置的节点GUID

        public string GetKey()
        {
            return $"{GraphName}|{SpritePath}|{(int)Position}";
        }
    }

    /// <summary>
    /// 立绘偏移记忆数据（用于JSON序列化）
    /// </summary>
    [Serializable]
    public class CharacterMemoryData
    {
        public List<CharacterMemoryEntry> Entries = new List<CharacterMemoryEntry>();
    }

    /// <summary>
    /// 立绘偏移记忆管理器
    /// </summary>
    public class CharacterMemoryManager
    {
        private static CharacterMemoryManager s_Instance;
        public static CharacterMemoryManager Instance => s_Instance ?? (s_Instance = new CharacterMemoryManager());

        // 内存缓存
        private Dictionary<string, CharacterMemoryEntry> m_Memories = new Dictionary<string, CharacterMemoryEntry>();

        // 默认保存路径
        private string m_DefaultSavePath => System.IO.Path.Combine(
            Application.dataPath.Replace("Assets", ""),
            "UserData",
            "CharacterMemory.json"
        );

        public string CurrentSavePath { get; set; }

        public CharacterMemoryManager()
        {
            CurrentSavePath = m_DefaultSavePath;
        }

        /// <summary>
        /// 生成记忆key
        /// </summary>
        private string MakeKey(string graphName, string spritePath, CharacterPosition position)
        {
            return $"{graphName}|{spritePath}|{(int)position}";
        }

        /// <summary>
        /// 记录记忆
        /// </summary>
        public void RecordMemory(string graphName, string spritePath, CharacterPosition position,
            float offsetX, float offsetY, float scale, string nodeGuid = "")
        {
            string key = MakeKey(graphName, spritePath, position);

            var entry = new CharacterMemoryEntry
            {
                GraphName = graphName,
                SpritePath = spritePath,
                Position = position,
                OffsetX = offsetX,
                OffsetY = offsetY,
                Scale = scale,
                LastNodeGuid = nodeGuid
            };

            m_Memories[key] = entry;
        }

        /// <summary>
        /// 获取记忆
        /// </summary>
        public bool TryGetMemory(string graphName, string spritePath, CharacterPosition position,
            out float offsetX, out float offsetY, out float scale)
        {
            string key = MakeKey(graphName, spritePath, position);

            if (m_Memories.TryGetValue(key, out var entry))
            {
                offsetX = entry.OffsetX;
                offsetY = entry.OffsetY;
                scale = entry.Scale;
                return true;
            }

            offsetX = 0;
            offsetY = 0;
            scale = 1f;
            return false;
        }

        /// <summary>
        /// 获取指定节点图和立绘的所有位置记忆
        /// </summary>
        public List<CharacterMemoryEntry> GetMemoriesByGraphAndSprite(string graphName, string spritePath)
        {
            var result = new List<CharacterMemoryEntry>();
            foreach (var entry in m_Memories.Values)
            {
                if (entry.GraphName == graphName && entry.SpritePath == spritePath)
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定节点图的所有记忆
        /// </summary>
        public List<CharacterMemoryEntry> GetMemoriesByGraph(string graphName)
        {
            var result = new List<CharacterMemoryEntry>();
            foreach (var entry in m_Memories.Values)
            {
                if (entry.GraphName == graphName)
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定立绘的所有记忆（跨节点图）
        /// </summary>
        public List<CharacterMemoryEntry> GetMemoriesBySprite(string spritePath)
        {
            var result = new List<CharacterMemoryEntry>();
            foreach (var entry in m_Memories.Values)
            {
                if (entry.SpritePath == spritePath)
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        /// <summary>
        /// 从磁盘加载
        /// </summary>
        public void Load()
        {
            if (!System.IO.File.Exists(CurrentSavePath))
            {
                Debug.Log($"[MemoryManager] 记忆文件不存在: {CurrentSavePath}");
                return;
            }

            try
            {
                string json = System.IO.File.ReadAllText(CurrentSavePath);
                var data = JsonUtility.FromJson<CharacterMemoryData>(json);

                m_Memories.Clear();
                if (data?.Entries != null)
                {
                    foreach (var entry in data.Entries)
                    {
                        string key = entry.GetKey();
                        m_Memories[key] = entry;
                    }
                }

                Debug.Log($"[MemoryManager] 加载记忆成功: {m_Memories.Count} 条");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemoryManager] 加载记忆失败: {e.Message}");
            }
        }

        /// <summary>
        /// 保存到磁盘
        /// </summary>
        public void Save()
        {
            try
            {
                // 确保目录存在
                string dir = System.IO.Path.GetDirectoryName(CurrentSavePath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                var data = new CharacterMemoryData
                {
                    Entries = new List<CharacterMemoryEntry>(m_Memories.Values)
                };

                string json = JsonUtility.ToJson(data, true);
                System.IO.File.WriteAllText(CurrentSavePath, json);

                Debug.Log($"[MemoryManager] 保存记忆成功: {CurrentSavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemoryManager] 保存记忆失败: {e.Message}");
            }
        }

        /// <summary>
        /// 清除指定节点图的所有记忆
        /// </summary>
        public void ClearGraphMemory(string graphName)
        {
            var keysToRemove = new List<string>();
            foreach (var kvp in m_Memories)
            {
                if (kvp.Value.GraphName == graphName)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                m_Memories.Remove(key);
            }

            Debug.Log($"[MemoryManager] 清除节点图 {graphName} 的记忆: {keysToRemove.Count} 条");
        }

        /// <summary>
        /// 清除所有记忆
        /// </summary>
        public void ClearAll()
        {
            m_Memories.Clear();
            Debug.Log("[MemoryManager] 清除所有记忆");
        }

        /// <summary>
        /// 删除指定记忆条目
        /// </summary>
        public void RemoveMemory(string graphName, string spritePath, CharacterPosition position)
        {
            string key = MakeKey(graphName, spritePath, position);
            if (m_Memories.Remove(key))
            {
                Debug.Log($"[MemoryManager] 删除记忆: {key}");
            }
        }

        /// <summary>
        /// 获取所有记忆条目
        /// </summary>
        public IEnumerable<CharacterMemoryEntry> GetAllMemories()
        {
            return m_Memories.Values;
        }

        /// <summary>
        /// 从指定节点图复制记忆到当前节点图
        /// </summary>
        public void CopyMemoriesFrom(string sourceGraphName, string targetGraphName)
        {
            int count = 0;
            foreach (var entry in m_Memories.Values)
            {
                if (entry.GraphName == sourceGraphName)
                {
                    string newKey = MakeKey(targetGraphName, entry.SpritePath, entry.Position);
                    m_Memories[newKey] = new CharacterMemoryEntry
                    {
                        GraphName = targetGraphName,
                        SpritePath = entry.SpritePath,
                        Position = entry.Position,
                        OffsetX = entry.OffsetX,
                        OffsetY = entry.OffsetY,
                        Scale = entry.Scale,
                        LastNodeGuid = ""
                    };
                    count++;
                }
            }
            Debug.Log($"[MemoryManager] 从 {sourceGraphName} 复制 {count} 条记忆到 {targetGraphName}");
        }
    }
}
