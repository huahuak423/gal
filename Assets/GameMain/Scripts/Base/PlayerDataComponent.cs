//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

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
        [SerializeField] private int m_Charm = 0;           // 魅力
        [SerializeField] private int m_Inspiration = 0;     // 灵感
        [SerializeField] private int m_Sanity = 0;          // 理智

        public int Charm => m_Charm;
        public int Inspiration => m_Inspiration;
        public int Sanity => m_Sanity;

        #endregion

        #region 行动点

        [Header("行动点")]
        [SerializeField] private int m_MaxActionPoints = 10;
        private int m_CurrentActionPoints = 0;

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

        private HashSet<string> m_SpecialItems = new HashSet<string>();

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
            m_SpecialItems.Clear();

            Log.Info($"[PlayerDataComponent] 新周目初始化完成！周目: {m_CurrentRound}, 行动点: {m_CurrentActionPoints}");
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
            m_SpecialItems.Clear();

            Log.Info("[PlayerDataComponent] 游戏已重置！");
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
                    m_Charm += value;
                    break;
                case PlayerAttributeType.Inspiration:
                    m_Inspiration += value;
                    break;
                case PlayerAttributeType.Sanity:
                    m_Sanity += value;
                    break;
                default:
                    Log.Warning($"[PlayerDataComponent] 未知的属性类型: {type}");
                    return;
            }

            Log.Info($"[PlayerDataComponent] 属性变化: {type} +{value}");
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
                Log.Warning($"[PlayerDataComponent] 行动点不足！当前: {m_CurrentActionPoints}, 需要: {amount}");
                return false;
            }

            m_CurrentActionPoints -= amount;

            // 触发行动点变化事件
            if (GameEntry.IsInitialized)
            {
               //GameEntry.Event.Fire(this, ActionPointChangedEventArgs.Create(m_CurrentActionPoints));
            }

            Log.Info($"[PlayerDataComponent] 消耗行动点: {amount}, 剩余: {m_CurrentActionPoints}");
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

            Log.Info($"[PlayerDataComponent] NPC好感度变化: NPC_{npcId} +{value}, 当前: {m_NpcFavorability[npcId]}");
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
        public void AddItem(string itemId)
        {
            if (m_SpecialItems.Add(itemId))
            {
                Log.Info($"[PlayerDataComponent] 获得物品: {itemId}");
            }
        }

        /// <summary>
        /// 移除物品
        /// </summary>
        public bool RemoveItem(string itemId)
        {
            if (m_SpecialItems.Remove(itemId))
            {
                Log.Info($"[PlayerDataComponent] 失去物品: {itemId}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否拥有物品
        /// </summary>
        public bool HasItem(string itemId)
        {
            return m_SpecialItems.Contains(itemId);
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
                    bool hasItem = HasItem(condition.ItemId);
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
                        AddItem(reward.ItemId);
                    }
                    else
                    {
                        RemoveItem(reward.ItemId);
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

            Log.Info($"[PlayerDataComponent] 第 {m_CurrentRound} 周目结束！");
        }

        /// <summary>
        /// 开始新周目
        /// </summary>
        public void StartNewRound()
        {
            m_CurrentRound++;
            InitializeNewRound();

            Log.Info($"[PlayerDataComponent] 开始第 {m_CurrentRound} 周目！");
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

            Log.Info($"[PlayerDataComponent] 周目继承加成: 行动点+{1}, 魅力+{m_Charm/10}, 灵感+{m_Inspiration/10}, 理智+{m_Sanity/10}");
        }

        #endregion

        #region 调试

        /// <summary>
        /// 打印当前状态（调试用）
        /// </summary>
        public void DebugPrintStatus()
        {
            Log.Info("=== PlayerDataComponent Status ===");
            Log.Info($"周目: {m_CurrentRound}");
            Log.Info($"行动点: {m_CurrentActionPoints}/{m_MaxActionPoints}");
            Log.Info($"属性: 魅力={m_Charm}, 灵感={m_Inspiration}, 理智={m_Sanity}");
            Log.Info($"继承加成: 行动点+{BonusActionPoints}, 魅力+{BonusCharm}, 灵感+{BonusInspiration}, 理智+{BonusSanity}");
            Log.Info($"物品数量: {m_SpecialItems.Count}");
            Log.Info($"NPC好感度数量: {m_NpcFavorability.Count}");
        }

        #endregion
    }
}
