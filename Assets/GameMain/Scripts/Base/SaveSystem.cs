//------------------------------------------------------------
// Game Framework
// AVG Game Project
// 存档系统 - 负责数据的加密、持久化、读取
// 解耦设计：独立于 PlayerDataComponent，通过读取/写入方式交互
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
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
        // 玩家属性
        public int Charm;
        public int Inspiration;
        public int Sanity;

        // 行动点
        public int CurrentActionPoints;
        public int MaxActionPoints;

        // 周目
        public int CurrentRound;
        public int BonusActionPoints;
        public int BonusCharm;
        public int BonusInspiration;
        public int BonusSanity;

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

        // 存档版本（用于未来兼容性）
        public int Version = 1;
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
                Log.Warning("[SaveSystem] PlayerData 不存在，无法保存");
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

                Log.Info($"[SaveSystem] 游戏已保存到槽位 {slotId}");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"[SaveSystem] 保存失败: {e.Message}");
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
                Log.Warning("[SaveSystem] PlayerData 不存在，无法加载");
                return false;
            }

            try
            {
                string filePath = GetSaveFilePath(slotId);
                if (!File.Exists(filePath))
                {
                    Log.Warning($"[SaveSystem] 存档文件不存在: {filePath}");
                    return false;
                }

                // 1. 读取文件
                string encrypted = File.ReadAllText(filePath);

                // 2. 解密数据
                string json = Decrypt(encrypted);

                // 3. 反序列化
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                // 4. 应用到 PlayerDataComponent
                playerData.LoadFromSaveData(saveData);

                Log.Info($"[SaveSystem] 存档已加载，周目: {saveData.CurrentRound}");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"[SaveSystem] 加载失败: {e.Message}");
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
                    Log.Info($"[SaveSystem] 存档已删除: 槽位 {slotId}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Error($"[SaveSystem] 删除失败: {e.Message}");
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
                Log.Error($"[SaveSystem] 读取存档信息失败: {e.Message}");
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
                Log.Warning("[SaveSystem] 最大槽位数必须为正数，使用默认值12");
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
                Log.Warning("[SaveSystem] PlayerData 不存在，无法提取存档数据");
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
                Log.Warning("[SaveSystem] PlayerData 不存在，无法应用存档数据");
                return;
            }

            if (saveData == null)
            {
                Log.Warning("[SaveSystem] 存档数据为空，无法应用");
                return;
            }

            playerData.LoadFromSaveData(saveData);
            Log.Info($"[SaveSystem] 存档数据已应用，周目: {saveData.CurrentRound}");
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
                Log.Error($"[SaveSystem] 加密失败: {e.Message}");
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
                Log.Error($"[SaveSystem] 解密失败: {e.Message}");
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