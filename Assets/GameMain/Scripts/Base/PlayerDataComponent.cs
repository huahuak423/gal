//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using AVGGame;

namespace AVGGame
{
    /// <summary>
    /// 玩家数据组件
    /// 存储玩家的所有运行时数据，包括属性、NPC好感度、物品、行动点等
    /// </summary>
    public class PlayerDataComponent : GameFrameworkComponent
    {
        #region 玩家属性

        [Header("玩家属性")]
        [SerializeField] private int m_Charm = 3;           // 魅力（上限100）
        [SerializeField] private int m_Inspiration = 3;     // 灵感（上限80）
        [SerializeField] private int m_Sanity = 3;          // 理智（上限100）

        public const int MaxCharm = 100;
        public const int MaxInspiration = 80;
        public const int MaxSanity = 100;
        
        public int Charm => m_Charm;
        public int Inspiration => m_Inspiration;
        public int Sanity => m_Sanity;
        #endregion

        #region NPC 进度数据
        // Key: NPC_ID, Value: 该 NPC 已经完成的事件 ID 集合
        private Dictionary<int, HashSet<int>> m_NpcProgress = new Dictionary<int, HashSet<int>>();
        #endregion
        #region 行动点

        [Header("行动点")]
        [SerializeField] private int m_MaxActionPoints = 12;
        private int m_CurrentActionPoints = 12;

        public int CurrentActionPoints => m_CurrentActionPoints;
        public int MaxActionPoints => m_MaxActionPoints;

        #endregion

        #region 周目

        [Header("周目")]
        [SerializeField] private int m_CurrentRound = 1;

        public int CurrentRound => m_CurrentRound;

        // 周目继承加成
        public int BonusActionPoints { get; private set; } = 0;
        public int BonusCharm { get; private set; } = 0;
        public int BonusInspiration { get; private set; } = 0;
        public int BonusSanity { get; private set; } = 0;

        #endregion

        #region NPC好感度

        private Dictionary<int, int> m_NpcFavorability = new Dictionary<int, int>();

        #endregion

        #region 特殊物品

        private HashSet<int> m_OwnedItems = new HashSet<int>();

        #endregion

        #region 事件完成记录

        // 已完成的所有事件（用于存档）
        private HashSet<int> m_CompletedEvents = new HashSet<int>();

        // 已完成的特殊事件（完成后不再显示）
        private HashSet<int> m_CompletedSpecialEvents = new HashSet<int>();

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化新周目的数据
        /// </summary>
        public void InitializeNewRound()
        {
            // 重置行动点（加上周目继承加成）
            m_CurrentActionPoints = m_MaxActionPoints + BonusActionPoints;

            // 重置属性（保留周目继承加成）
            m_Charm = BonusCharm;
            m_Inspiration = BonusInspiration;
            m_Sanity = BonusSanity;

            // 清空好感度和物品
            m_NpcFavorability.Clear();
            m_OwnedItems.Clear();

            Debug.Log($"[PlayerDataComponent] 新周目初始化完成！周目: {m_CurrentRound}, 行动点: {m_CurrentActionPoints}");
        }

        /// <summary>
        /// 重置游戏（从主菜单开始新游戏时调用）
        /// </summary>
        public void ResetGame()
        {
            m_CurrentRound = 1;
            m_Charm = 0;
            m_Inspiration = 0;
            m_Sanity = 0;
            m_CurrentActionPoints = m_MaxActionPoints;
            BonusActionPoints = 0;
            BonusCharm = 0;
            BonusInspiration = 0;
            BonusSanity = 0;
            m_NpcFavorability.Clear();
            m_OwnedItems.Clear();

            Debug.Log("[PlayerDataComponent] 游戏已重置！");
        }

        #endregion

        #region 属性操作

        /// <summary>
        /// 增加玩家属性
        /// </summary>
        public void AddAttribute(PlayerAttributeType type, int value)
        {
            switch (type)
            {
                case PlayerAttributeType.Charm:
                    m_Charm = Mathf.Clamp(m_Charm + value, 0, MaxCharm);
                    break;
                case PlayerAttributeType.Inspiration:
                    m_Inspiration = Mathf.Clamp(m_Inspiration + value, 0, MaxInspiration);
                    break;
                case PlayerAttributeType.Sanity:
                    m_Sanity = Mathf.Clamp(m_Sanity + value, 0, MaxSanity);
                    break;
                default:
                    Debug.LogWarning($"[PlayerDataComponent] 未知的属性类型: {type}");
                    return;
            }

            Debug.Log($"[PlayerDataComponent] 属性变化: {type} +{value}");
        }

        /// <summary>
        /// 获取玩家属性值
        /// </summary>
        public int GetAttribute(PlayerAttributeType type)
        {
            return type switch
            {
                PlayerAttributeType.Charm => m_Charm,
                PlayerAttributeType.Inspiration => m_Inspiration,
                PlayerAttributeType.Sanity => m_Sanity,
                _ => 0
            };
        }

        #endregion

        #region 行动点操作

        /// <summary>
        /// 消耗行动点
        /// </summary>
        /// <param name="amount">消耗数量</param>
        /// <returns>是否消耗成功</returns>
        public bool ConsumeActionPoints(int amount)
        {
            if (m_CurrentActionPoints < amount)
            {
                Debug.LogWarning($"[PlayerDataComponent] 行动点不足！当前: {m_CurrentActionPoints}, 需要: {amount}");
                return false;
            }

            m_CurrentActionPoints -= amount;

            // 触发行动点变化事件
            if (GameEntry.IsInitialized)
            {
               //GameEntry.Event.Fire(this, ActionPointChangedEventArgs.Create(m_CurrentActionPoints));
            }

            Debug.Log($"[PlayerDataComponent] 消耗行动点: {amount}, 剩余: {m_CurrentActionPoints}");
            return true;
        }

        /// <summary>
        /// 检查是否有足够的行动点
        /// </summary>
        public bool HasEnoughActionPoints(int amount)
        {
            return m_CurrentActionPoints >= amount;
        }

        #endregion

        #region NPC好感度操作

        /// <summary>
        /// 获取NPC好感度
        /// </summary>
        public int GetFavorability(int npcId)
        {
            return m_NpcFavorability.TryGetValue(npcId, out var value) ? value : 0;
        }

        /// <summary>
        /// 增加NPC好感度
        /// </summary>
        public void AddFavorability(int npcId, int value)
        {
            if (!m_NpcFavorability.ContainsKey(npcId))
            {
                m_NpcFavorability[npcId] = 0;
            }
            m_NpcFavorability[npcId] += value;

            Debug.Log($"[PlayerDataComponent] NPC好感度变化: NPC_{npcId} +{value}, 当前: {m_NpcFavorability[npcId]}");
        }

        /// <summary>
        /// 设置NPC好感度
        /// </summary>
        public void SetFavorability(int npcId, int value)
        {
            m_NpcFavorability[npcId] = value; 
        }

        #endregion

        #region 物品操作

        /// <summary>
        /// 添加物品
        /// </summary>
        public void AddItem(int itemId)
        {
            if (m_OwnedItems.Add(itemId))
            {
                Debug.Log($"[PlayerDataComponent] 获得物品: {itemId}");
            }
        }

        /// <summary>
        /// 移除物品
        /// </summary>
        public bool RemoveItem(int itemId)
        {
            if (m_OwnedItems.Remove(itemId))
            {
                Debug.Log($"[PlayerDataComponent] 失去物品: {itemId}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否拥有物品
        /// </summary>
        public bool HasItem(int itemId)
        {
            return m_OwnedItems.Contains(itemId);
        }

        #endregion
        
        #region NPC进度操作
        /// <summary>
        /// 记录 NPC 完成了某个事件
        /// </summary>
        public void AddNpcEventProgress(int npcId, int eventId)
        {
            if (!m_NpcProgress.ContainsKey(npcId))
            {
                m_NpcProgress[npcId] = new HashSet<int>();
            }
            m_NpcProgress[npcId].Add(eventId);
        }

        /// <summary>
        /// 检查 NPC 是否完成了某个前置事件
        /// </summary>
        public bool HasCompletedNpcEvent(int npcId, int eventId)
        {
            if (m_NpcProgress.TryGetValue(npcId, out HashSet<int> completedEvents))
            {
                return completedEvents.Contains(eventId);
            }
            return false;
        }
        #endregion

        #region 事件完成记录

        /// <summary>
        /// 标记事件已完成（通用，用于存档）
        /// </summary>
        public void MarkEventCompleted(int eventId)
        {
            m_CompletedEvents.Add(eventId);
            Debug.Log($"[PlayerDataComponent] 事件完成: {eventId}");
        }

        /// <summary>
        /// 标记特殊事件已完成（完成后不再显示）
        /// </summary>
        public void MarkSpecialEventCompleted(int eventId)
        {
            m_CompletedSpecialEvents.Add(eventId);
            Debug.Log($"[PlayerDataComponent] 特殊事件完成: {eventId}");
        }

        /// <summary>
        /// 检查事件是否已完成
        /// </summary>
        public bool HasCompletedEvent(int eventId)
        {
            return m_CompletedEvents.Contains(eventId);
        }

        /// <summary>
        /// 检查特殊事件是否已完成（用于过滤显示）
        /// </summary>
        public bool HasCompletedSpecialEvent(int eventId)
        {
            return m_CompletedSpecialEvents.Contains(eventId);
        }

        /// <summary>
        /// 获取已完成事件数量（调试用）
        /// </summary>
        public int GetCompletedEventsCount()
        {
            return m_CompletedEvents.Count;
        }

        #endregion

        #region 条件检查

        /// <summary>
        /// 检查条件是否满足
        /// </summary>
        public bool CheckCondition(ChoiceCondition condition)
        {
            switch (condition.Type)
            {
                case ConditionType.PlayerAttribute:
                    int attrValue = GetAttribute(condition.AttributeType);
                    return CheckValue(attrValue, condition.Value, condition.Operator);

                case ConditionType.NpcFavorability:
                    int favorValue = GetFavorability(int.Parse(condition.NpcId));
                    return CheckValue(favorValue, condition.Value, condition.Operator);

                case ConditionType.SpecialItem:
                    bool hasItem = HasItem(int.Parse(condition.ItemId));
                    return condition.RequireItem ? hasItem : !hasItem;

                default:
                    return true;
            }
        }

        /// <summary>
        /// 检查所有条件是否满足
        /// </summary>
        public bool CheckConditions(List<ChoiceCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            foreach (var condition in conditions)
            {
                if (!CheckCondition(condition))
                    return false;
            }
            return true;
        }

        private bool CheckValue(int currentValue, int targetValue, ConditionOperator op)
        {
            return op switch
            {
                ConditionOperator.GreaterThanOrEqual => currentValue >= targetValue,
                ConditionOperator.LessThanOrEqual => currentValue <= targetValue,
                ConditionOperator.Equal => currentValue == targetValue,
                _ => true
            };
        }

        #endregion
        
        //当前所处故事
        public string currentStoryGarphName {get; private set;}

        public void SetCurrentStoryGarphName(string storyGarphName)
        {
            currentStoryGarphName = storyGarphName;
        }

        #region 奖励应用

        /// <summary>
        /// 应用奖励
        /// </summary>
        public void ApplyReward(ChoiceReward reward)
        {
            switch (reward.Type)
            {
                case ConditionType.PlayerAttribute:
                    AddAttribute(reward.AttributeType, reward.Value);
                    break;

                case ConditionType.NpcFavorability:
                    if (int.TryParse(reward.NpcId, out int npcId))
                    {
                        AddFavorability(npcId, reward.Value);
                    }
                    break;

                case ConditionType.SpecialItem:
                    if (reward.Value > 0)
                    {
                        AddItem(int.Parse(reward.ItemId));
                    }
                    else
                    {
                        RemoveItem(int.Parse(reward.ItemId));
                    }
                    break;
            }
        }

        /// <summary>
        /// 应用多个奖励
        /// </summary>
        public void ApplyRewards(List<ChoiceReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
                return;

            foreach (var reward in rewards)
            {
                ApplyReward(reward);
            }
        }

        #endregion

        #region 周目操作

        /// <summary>
        /// 结束当前周目，计算继承加成
        /// </summary>
        public void EndRound()
        {
            // 计算下周目继承加成（可以根据当前周目表现计算）
            CalculateRoundBonus();

            Debug.Log($"[PlayerDataComponent] 第 {m_CurrentRound} 周目结束！");
        }

        /// <summary>
        /// 开始新周目
        /// </summary>
        public void StartNewRound()
        {
            m_CurrentRound++;
            InitializeNewRound();

            Debug.Log($"[PlayerDataComponent] 开始第 {m_CurrentRound} 周目！");
        }

        /// <summary>
        /// 计算周目继承加成
        /// </summary>
        private void CalculateRoundBonus()
        {
            // TODO: 根据游戏设计调整继承规则
            // 示例：每周目增加1点行动点
            BonusActionPoints += 1;

            // 示例：保留10%的属性作为加成
            BonusCharm += m_Charm / 10;
            BonusInspiration += m_Inspiration / 10;
            BonusSanity += m_Sanity / 10;

            Debug.Log($"[PlayerDataComponent] 周目继承加成: 行动点+{1}, 魅力+{m_Charm/10}, 灵感+{m_Inspiration/10}, 理智+{m_Sanity/10}");
        }

        #endregion

        #region 存档操作

        /// <summary>
        /// 获取当前玩家数据的存档快照
        /// </summary>
        public SaveData GetSaveData()
        {
            return new SaveData
            {
                // 玩家属性
                Charm = m_Charm,
                Inspiration = m_Inspiration,
                Sanity = m_Sanity,

                // 行动点
                CurrentActionPoints = m_CurrentActionPoints,
                MaxActionPoints = m_MaxActionPoints,

                // 周目
                CurrentRound = m_CurrentRound,
                BonusActionPoints = BonusActionPoints,
                BonusCharm = BonusCharm,
                BonusInspiration = BonusInspiration,
                BonusSanity = BonusSanity,

                // NPC好感度
                NpcFavorability = new SerializableDictionary<int, int>(m_NpcFavorability),

                // 特殊物品
                OwnedItems = new System.Collections.Generic.List<int>(m_OwnedItems).ToArray(),

                // 已完成事件
                CompletedEvents = new System.Collections.Generic.List<int>(m_CompletedEvents).ToArray(),

                // 已完成特殊事件
                CompletedSpecialEvents = new System.Collections.Generic.List<int>(m_CompletedSpecialEvents).ToArray(),

                // NPC进度
                NpcProgress = ConvertNpcProgressToSerializable(),

                // 存档时间
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),

                // 当前所处故事
                CurrentStoryGarphName = currentStoryGarphName
            };
        }

        /// <summary>
        /// 从存档数据恢复玩家状态
        /// </summary>
        public void LoadFromSaveData(SaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("[PlayerDataComponent] 存档数据为空，无法加载");
                return;
            }

            // 安全加载数据
            Debug.Log($"[PlayerDataComponent] 开始加载存档数据，版本: {saveData.Version}");

            // 玩家属性 - 确保在合理范围内
            m_Charm = Mathf.Max(0, saveData.Charm);
            m_Inspiration = Mathf.Max(0, saveData.Inspiration);
            m_Sanity = Mathf.Max(0, saveData.Sanity);

            // 行动点 - 确保合理
            m_CurrentActionPoints = Mathf.Max(0, saveData.CurrentActionPoints);
            m_MaxActionPoints = Mathf.Max(1, saveData.MaxActionPoints); // 最小为1

            // 周目 - 确保至少为1
            m_CurrentRound = Mathf.Max(1, saveData.CurrentRound);
            BonusActionPoints = Mathf.Max(0, saveData.BonusActionPoints);
            BonusCharm = Mathf.Max(0, saveData.BonusCharm);
            BonusInspiration = Mathf.Max(0, saveData.BonusInspiration);
            BonusSanity = Mathf.Max(0, saveData.BonusSanity);

            // NPC好感度
            m_NpcFavorability = saveData.NpcFavorability?.ToDictionary() ?? new Dictionary<int, int>();
            // 清理无效的NPC ID
            var validNpcFavorability = new Dictionary<int, int>();
            foreach (var kvp in m_NpcFavorability)
            {
                if (kvp.Key > 0)
                {
                    validNpcFavorability[kvp.Key] = Mathf.Max(0, kvp.Value);
                }
            }
            m_NpcFavorability = validNpcFavorability;

            // 特殊物品
            m_OwnedItems = new HashSet<int>();
            if (saveData.OwnedItems != null)
            {
                foreach (int id in saveData.OwnedItems)
                {
                    if (id > 0)
                    {
                        m_OwnedItems.Add(id);
                    }
                }
            }

            // 已完成事件
            m_CompletedEvents = new HashSet<int>();
            if (saveData.CompletedEvents != null)
            {
                foreach (int id in saveData.CompletedEvents)
                {
                    if (id > 0)
                    {
                        m_CompletedEvents.Add(id);
                    }
                }
            }

            // 已完成特殊事件
            m_CompletedSpecialEvents = new HashSet<int>();
            if (saveData.CompletedSpecialEvents != null)
            {
                foreach (int id in saveData.CompletedSpecialEvents)
                {
                    if (id > 0)
                    {
                        m_CompletedSpecialEvents.Add(id);
                    }
                }
            }

            // NPC进度
            m_NpcProgress = ConvertSerializableToNpcProgress(saveData.NpcProgress);
            // 清理无效的NPC进度
            var validNpcProgress = new Dictionary<int, HashSet<int>>();
            foreach (var kvp in m_NpcProgress)
            {
                if (kvp.Key > 0 && kvp.Value != null)
                {
                    var validEvents = new HashSet<int>();
                    foreach (int eventId in kvp.Value)
                    {
                        if (eventId > 0)
                        {
                            validEvents.Add(eventId);
                        }
                    }
                    validNpcProgress[kvp.Key] = validEvents;
                }
            }
            m_NpcProgress = validNpcProgress;

            // 恢复当前所处故事 - 如果为空则使用默认值
            currentStoryGarphName = string.IsNullOrEmpty(saveData.CurrentStoryGarphName)
                ? "DefaultStory"
                : saveData.CurrentStoryGarphName;

            // 记录加载详情
            Debug.Log($"[PlayerDataComponent] 存档加载完成！");
            Debug.Log($"- 周目: {m_CurrentRound}");
            Debug.Log($"- 行动点: {m_CurrentActionPoints}/{m_MaxActionPoints}");
            Debug.Log($"- 属性: 魅力={m_Charm}, 灵感={m_Inspiration}, 理智={m_Sanity}");
            Debug.Log($"- 继承加成: 行动点+{BonusActionPoints}, 魅力+{BonusCharm}, 灵感+{BonusInspiration}, 理智+{BonusSanity}");
            Debug.Log($"- 已完成事件数: {m_CompletedEvents.Count}");
            Debug.Log($"- 已完成特殊事件数: {m_CompletedSpecialEvents.Count}");
            Debug.Log($"- 拥有物品数: {m_OwnedItems.Count}");
            Debug.Log($"- NPC好感度数: {m_NpcFavorability.Count}");
            Debug.Log($"- 当前故事: {currentStoryGarphName}");
        }

        /// <summary>
        /// 退出时保存游戏（默认存档槽位1）
        /// </summary>
        public void SaveOnExit(int slotId = 1)
        {
            if (CustomEntry.SaveSystem == null)
            {
                Debug.LogWarning("[PlayerDataComponent] SaveSystem 未初始化，无法保存");
                return;
            }

            bool success = CustomEntry.SaveSystem.Save(slotId);

            if (success)
            {
                Debug.Log($"[PlayerDataComponent] 游戏已保存到槽位 {slotId}");
            }
            else
            {
                Debug.LogWarning($"[PlayerDataComponent] 保存失败！槽位: {slotId}");
            }
        }

        /// <summary>
        /// 手动保存游戏到指定槽位
        /// </summary>
        public bool SaveGame(int slotId)
        {
            if (CustomEntry.SaveSystem == null)
            {
                Debug.LogWarning("[PlayerDataComponent] SaveSystem 未初始化，无法保存");
                return false;
            }
            return CustomEntry.SaveSystem.Save(slotId);
        }

        /// <summary>
        /// 从指定槽位加载游戏
        /// </summary>
        public bool LoadGame(int slotId)
        {
            if (CustomEntry.SaveSystem == null)
            {
                Debug.LogWarning("[PlayerDataComponent] SaveSystem 未初始化，无法加载");
                return false;
            }
            return CustomEntry.SaveSystem.Load(slotId);
        }

        /// <summary>
        /// 转换NPC进度为可序列化格式
        /// </summary>
        private SerializableDictionary<int, int[]> ConvertNpcProgressToSerializable()
        {
            var result = new SerializableDictionary<int, int[]>();
            if (m_NpcProgress != null && m_NpcProgress.Count > 0)
            {
                result.Keys = new int[m_NpcProgress.Count];
                result.Values = new int[m_NpcProgress.Count][];

                int i = 0;
                foreach (var kvp in m_NpcProgress)
                {
                    result.Keys[i] = kvp.Key;
                    result.Values[i] = new List<int>(kvp.Value).ToArray();
                    i++;
                }
            }
            return result;
        }

        /// <summary>
        /// 从可序列化格式恢复NPC进度
        /// </summary>
        private Dictionary<int, HashSet<int>> ConvertSerializableToNpcProgress(SerializableDictionary<int, int[]> data)
        {
            var result = new Dictionary<int, HashSet<int>>();
            if (data?.Keys != null && data.Values != null)
            {
                for (int i = 0; i < data.Keys.Length; i++)
                {
                    result[data.Keys[i]] = new HashSet<int>((IEnumerable<int>)data.Values[i]);
                }
            }
            return result;
        }

        #endregion

        #region 调试

        /// <summary>
        /// 打印当前状态（调试用）
        /// </summary>
        public void DebugPrintStatus()
        {
            Debug.Log("=== PlayerDataComponent Status ===");
            Debug.Log($"周目: {m_CurrentRound}");
            Debug.Log($"行动点: {m_CurrentActionPoints}/{m_MaxActionPoints}");
            Debug.Log($"属性: 魅力={m_Charm}, 灵感={m_Inspiration}, 理智={m_Sanity}");
            Debug.Log($"继承加成: 行动点+{BonusActionPoints}, 魅力+{BonusCharm}, 灵感+{BonusInspiration}, 理智+{BonusSanity}");
            Debug.Log($"物品数量: {m_OwnedItems.Count}");
            Debug.Log($"NPC好感度数量: {m_NpcFavorability.Count}");
        }

        #endregion
    }
}
