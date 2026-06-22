//------------------------------------------------------------
// Game Framework
// AVG Game Project
// 存档系统 - 负责数据的加密、持久化、读取
// 解耦设计：独立于 PlayerDataComponent，通过读取/写入方式交互
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    /// <summary>
    /// 存档数据结构
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // 玩家信息
        public string PlayerName;

        // 行动点
        public int CurrentActionPoints;
        public int MaxActionPoints;

        // 周目
        public int CurrentRound;
        public int BonusActionPoints;

        // NPC好感度
        public SerializableDictionary<int, int> NpcFavorability;

        // 特殊物品
        public int[] OwnedItems;

        // 已完成事件
        public int[] CompletedEvents;

        // 已完成特殊事件
        public int[] CompletedSpecialEvents;

        // NPC进度
        public SerializableDictionary<int, int[]> NpcProgress;

        // 存档时间
        public string SaveTime;

        // 当前所处故事
        public string CurrentStoryGarphName;

        // 当前对话进度ID（0 = 不在对话中）
        public int CurrentDialogueId;

        // 当前进行中的事件ID（0 = 无进行中事件）
        public int CurrentEventId;

        // 存档版本（用于未来兼容性）
        public int Version = 3;
    }

    /// <summary>
    /// 可序列化的字典（用于JSON序列化）
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        public TKey[] Keys;
        public TValue[] Values;

        public SerializableDictionary() { }

        public SerializableDictionary(Dictionary<TKey, TValue> dict)
        {
            if (dict != null && dict.Count > 0)
            {
                Keys = new TKey[dict.Count];
                Values = new TValue[dict.Count];
                int i = 0;
                foreach (var kvp in dict)
                {
                    Keys[i] = kvp.Key;
                    Values[i] = kvp.Value;
                    i++;
                }
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var result = new Dictionary<TKey, TValue>();
            if (Keys != null && Values != null)
            {
                for (int i = 0; i < Keys.Length; i++)
                {
                    result[Keys[i]] = Values[i];
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 存档系统 - 独立组件，负责存档的读取、写入、加密、持久化
    /// 与 PlayerDataComponent 解耦，通过 CustomEntry.PlayerData 访问运行时数据
    /// 通过 CustomEntry 访问
    /// </summary>
    public class SaveSystem : GameFrameworkComponent
    {
        // 存档文件名格式
        private const string SAVE_FILE_PREFIX = "save_";
        private const string SAVE_FILE_EXTENSION = ".dat";

        // 加密密钥
        [SerializeField] private string m_EncryptionKey = "AVG_GAME_SECRET_KEY_2024";

        // 存档目录
        private string SaveDirectory
        {
            get
            {
                string path = Path.Combine(Application.persistentDataPath, "Saves");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        #region 公共方法 - 存档操作

        /// <summary>
        /// 保存当前游戏状态到指定槽位
        /// 从 PlayerDataComponent 读取运行时数据并持久化
        /// </summary>
        /// <param name="slotId">存档槽位ID，默认为1</param>
        /// <returns>是否保存成功</returns>
        public bool Save(int slotId = 1)
        {
            var playerData = CustomEntry.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[SaveSystem] PlayerData 不存在，无法保存");
                return false;
            }

            try
            {
                // 1. 从 PlayerDataComponent 获取存档数据
                SaveData saveData = playerData.GetSaveData();

                // 2. 序列化为JSON
                string json = JsonUtility.ToJson(saveData, true);

                // 3. 加密数据
                string encrypted = Encrypt(json);

                // 4. 写入文件
                string filePath = GetSaveFilePath(slotId);
                File.WriteAllText(filePath, encrypted);

                Debug.Log($"[SaveSystem] 游戏已保存到槽位 {slotId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 保存失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从指定槽位加载游戏数据并写入 PlayerDataComponent
        /// </summary>
        /// <param name="slotId">存档槽位ID</param>
        /// <returns>是否加载成功</returns>
        public bool Load(int slotId = 1)
        {
            var playerData = CustomEntry.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[SaveSystem] PlayerData 不存在，无法加载");
                return false;
            }

            try
            {
                string filePath = GetSaveFilePath(slotId);
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveSystem] 存档文件不存在: {filePath}");
                    return false;
                }

                // 1. 读取文件
                string encrypted = File.ReadAllText(filePath);

                // 2. 解密数据
                string json = Decrypt(encrypted);

                // 3. 反序列化
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                // 4. 安全处理存档数据
                saveData = SafeSaveData(saveData);

                // 5. 应用到 PlayerDataComponent
                playerData.LoadFromSaveData(saveData);

                Debug.Log($"[SaveSystem] 存档已加载，周目: {saveData.CurrentRound}, 故事: {saveData.CurrentStoryGarphName ?? "无"}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 加载失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查指定槽位是否有存档
        /// </summary>
        /// <param name="slotId">存档槽位ID</param>
        /// <returns>是否存在存档</returns>
        public bool HasSave(int slotId = 1)
        {
            string filePath = GetSaveFilePath(slotId);
            return File.Exists(filePath);
        }

        /// <summary>
        /// 删除指定槽位的存档
        /// </summary>
        /// <param name="slotId">存档槽位ID</param>
        /// <returns>是否删除成功</returns>
        public bool DeleteSave(int slotId = 1)
        {
            try
            {
                string filePath = GetSaveFilePath(slotId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[SaveSystem] 存档已删除: 槽位 {slotId}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 删除失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取存档槽位信息（用于存档列表UI显示）
        /// </summary>
        /// <param name="slotId">存档槽位ID</param>
        /// <returns>存档简要信息</returns>
        public SaveSlotInfo GetSaveSlotInfo(int slotId = 1)
        {
            if (!HasSave(slotId))
            {
                return new SaveSlotInfo { SlotId = slotId, HasSave = false };
            }

            try
            {
                string filePath = GetSaveFilePath(slotId);
                string encrypted = File.ReadAllText(filePath);
                string json = Decrypt(encrypted);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                return new SaveSlotInfo
                {
                    SlotId = slotId,
                    HasSave = true,
                    SaveTime = saveData.SaveTime,
                    CurrentRound = saveData.CurrentRound
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 读取存档信息失败: {e.Message}");
                return new SaveSlotInfo { SlotId = slotId, HasSave = false };
            }
        }

        /// <summary>
        /// 获取所有存档槽位信息
        /// </summary>
        /// <param name="maxSlots">最大槽位数，默认12</param>
        /// <returns>存档槽位数组</returns>
        public SaveSlotInfo[] GetAllSaveSlotInfos(int maxSlots = 12)
        {
            // 确保最大槽位数为正值
            if (maxSlots <= 0)
            {
                Debug.LogWarning("[SaveSystem] 最大槽位数必须为正数，使用默认值12");
                maxSlots = 12;
            }

            var infos = new SaveSlotInfo[maxSlots];
            for (int i = 1; i <= maxSlots; i++)
            {
                infos[i - 1] = GetSaveSlotInfo(i);
            }
            return infos;
        }

        #endregion

        #region 公共方法 - 数据转换（供外部使用）

        /// <summary>
        /// 从 PlayerDataComponent 提取存档数据
        /// </summary>
        public SaveData ExtractSaveData()
        {
            var playerData = CustomEntry.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[SaveSystem] PlayerData 不存在，无法提取存档数据");
                return null;
            }

            return playerData.GetSaveData();
        }

        /// <summary>
        /// 将存档数据应用到 PlayerDataComponent
        /// </summary>
        public void ApplySaveData(SaveData saveData)
        {
            var playerData = CustomEntry.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[SaveSystem] PlayerData 不存在，无法应用存档数据");
                return;
            }

            if (saveData == null)
            {
                Debug.LogWarning("[SaveSystem] 存档数据为空，无法应用");
                return;
            }

            playerData.LoadFromSaveData(saveData);
            Debug.Log($"[SaveSystem] 存档数据已应用，周目: {saveData.CurrentRound}");
        }

        #endregion

        #region 安全数据处理方法

        /// <summary>
        /// 安全处理存档数据，确保关键数据不为空
        /// </summary>
        private SaveData SafeSaveData(SaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("[SaveSystem] 存档数据为空，创建默认数据");
                return CreateDefaultSaveData();
            }

            // 确保基本数值有效
            if (saveData.CurrentRound <= 0)
            {
                Debug.LogWarning($"[SaveSystem] 当前周目无效 ({saveData.CurrentRound})，重置为1");
                saveData.CurrentRound = 1;
            }

            if (saveData.CurrentActionPoints <= 0)
            {
                Debug.LogWarning($"[SaveSystem] 行动点无效 ({saveData.CurrentActionPoints})，重置为默认值");
                saveData.CurrentActionPoints = saveData.MaxActionPoints > 0 ? saveData.MaxActionPoints : 10;
            }

            if (saveData.MaxActionPoints <= 0)
            {
                Debug.LogWarning($"[SaveSystem] 最大行动点无效 ({saveData.MaxActionPoints})，重置为10");
                saveData.MaxActionPoints = 10;
            }

            // 确保继承加成合理
            saveData.BonusActionPoints = Mathf.Max(0, saveData.BonusActionPoints);

            // 确保当前故事名称不为空（如果为空，设置为默认值）
            if (string.IsNullOrEmpty(saveData.CurrentStoryGarphName))
            {
                Debug.LogWarning("[SaveSystem] 当前故事为空，设置为默认值");
                saveData.CurrentStoryGarphName = "DefaultStory";
            }

            // 确保字典数据有效
            if (saveData.NpcFavorability == null)
            {
                saveData.NpcFavorability = new SerializableDictionary<int, int>();
            }
            else
            {
                // 清理无效的键值对
                var validDict = new SerializableDictionary<int, int>();
                int keysLength = saveData.NpcFavorability?.Keys?.Length ?? 0;
                for (int i = 0; i < keysLength; i++)
                {
                    int key = saveData.NpcFavorability.Keys[i];
                    if (key > 0)
                    {
                        var keyList = new List<int>(validDict.Keys);
                        keyList.Add(key);
                        validDict.Keys = keyList.ToArray();

                        var tempList = new List<int>(validDict.Values);
                        tempList.Add(saveData.NpcFavorability.Values[i]);
                        validDict.Values = tempList.ToArray();
                    
                    }
                }
                saveData.NpcFavorability = validDict;
            }

            // 确保数组数据有效
            if (saveData.OwnedItems == null)
            {
                saveData.OwnedItems = new int[0];
            }
            else
            {
                // 过滤无效ID
                var validItems = new List<int>();
                foreach (int id in saveData.OwnedItems)
                {
                    if (id > 0)
                    {
                        validItems.Add(id);
                    }
                }
                saveData.OwnedItems = validItems.ToArray();
            }

            if (saveData.CompletedEvents == null)
            {
                saveData.CompletedEvents = new int[0];
            }
            else
            {
                // 过滤无效ID并去重
                var validEvents = new List<int>();
                var seenEvents = new HashSet<int>();
                foreach (int id in saveData.CompletedEvents)
                {
                    if (id > 0 && !seenEvents.Contains(id))
                    {
                        validEvents.Add(id);
                        seenEvents.Add(id);
                    }
                }
                saveData.CompletedEvents = validEvents.ToArray();
            }

            if (saveData.CompletedSpecialEvents == null)
            {
                saveData.CompletedSpecialEvents = new int[0];
            }
            else
            {
                // 过滤无效ID并去重
                var validSpecialEvents = new List<int>();
                var seenSpecialEvents = new HashSet<int>();
                foreach (int id in saveData.CompletedSpecialEvents)
                {
                    if (id > 0 && !seenSpecialEvents.Contains(id))
                    {
                        validSpecialEvents.Add(id);
                        seenSpecialEvents.Add(id);
                    }
                }
                saveData.CompletedSpecialEvents = validSpecialEvents.ToArray();
            }

            // 确保NPC进度有效
            if (saveData.NpcProgress == null)
            {
                saveData.NpcProgress = new SerializableDictionary<int, int[]>();
            }

            // 确保对话进度和事件ID有效
            saveData.CurrentDialogueId = Mathf.Max(0, saveData.CurrentDialogueId);
            saveData.CurrentEventId = Mathf.Max(0, saveData.CurrentEventId);

            Debug.Log("[SaveSystem] 存档数据安全处理完成");
            return saveData;
        }

        /// <summary>
        /// 创建默认存档数据
        /// </summary>
        private SaveData CreateDefaultSaveData()
        {
            Debug.Log("[SaveSystem] 创建默认存档数据");
            return new SaveData
            {
                CurrentActionPoints = 10,
                MaxActionPoints = 10,
                CurrentRound = 1,
                BonusActionPoints = 0,
                NpcFavorability = new SerializableDictionary<int, int>(),
                OwnedItems = new int[0],
                CompletedEvents = new int[0],
                CompletedSpecialEvents = new int[0],
                NpcProgress = new SerializableDictionary<int, int[]>(),
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                CurrentStoryGarphName = "DefaultStory",
                CurrentDialogueId = 0,
                CurrentEventId = 0,
                Version = 2
            };
        }

        #endregion

        #region 私有方法 - 加密/解密

        /// <summary>
        /// 加密数据 (AES)
        /// </summary>
        private string Encrypt(string plainText)
        {
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(m_EncryptionKey.PadRight(32).Substring(0, 32));

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    // 生成随机IV提高安全性
                    aes.GenerateIV();

                    // 将IV添加到加密数据前面（用于解密时使用）
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        byte[] iv = aes.IV;
                        byte[] result = new byte[iv.Length + encrypted.Length];
                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
                        return Convert.ToBase64String(result);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 加密失败: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 解密数据 (AES)
        /// </summary>
        private string Decrypt(string cipherText)
        {
            try
            {
                byte[] allBytes = Convert.FromBase64String(cipherText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(m_EncryptionKey.PadRight(32).Substring(0, 32));

                // 提取IV（前16字节）
                byte[] iv = new byte[16];
                Buffer.BlockCopy(allBytes, 0, iv, 0, iv.Length);

                // 提取加密数据（第16字节之后）
                byte[] encrypted = new byte[allBytes.Length - 16];
                Buffer.BlockCopy(allBytes, 16, encrypted, 0, encrypted.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = iv;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        byte[] decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                        return Encoding.UTF8.GetString(decrypted);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 解密失败: {e.Message}");
                throw;
            }
        }

        #endregion

        #region 私有方法 - 文件操作

        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        private string GetSaveFilePath(int slotId)
        {
            return Path.Combine(SaveDirectory, $"{SAVE_FILE_PREFIX}{slotId}{SAVE_FILE_EXTENSION}");
        }

        #endregion
    }

    /// <summary>
    /// 存档槽位信息（用于存档列表UI显示）
    /// </summary>
    public class SaveSlotInfo
    {
        public int SlotId { get; set; }
        public bool HasSave { get; set; }
        public string SaveTime { get; set; }
        public int CurrentRound { get; set; }
        public int PlayTimeSeconds { get; set; }  // 游戏时长（秒）
        public string CurrentLocation { get; set; }  // 当前位置名称
    }
}