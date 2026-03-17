using System;
using System.Collections.Generic;
using UnityEngine;

namespace AVGGame
{
    // ==========================================
    // 1. 基础枚举定义
    // ==========================================
    public enum PlayerAttributeType { None = 0, Charm, Inspiration, Sanity }
    public enum ConditionOperator { GreaterThanOrEqual, LessThanOrEqual, Equal }
    public enum ConditionType { PlayerAttribute, NpcFavorability, SpecialItem }
    // 角色表现指令结构
    public enum CharacterActionType { Enter, Leave, ChangeSprite }
    public enum CharacterPosition { Left, Center, Right }

    
    // ==========================================
    // 2. 选择条件与奖励结构
    // ==========================================
    [Serializable]
    public class ChoiceCondition
    {
        [Tooltip("条件类型：属性 / 好感度 / 特殊物品")]
        public ConditionType Type = ConditionType.PlayerAttribute;
        
        [Tooltip("玩家属性类型")]
        public PlayerAttributeType AttributeType = PlayerAttributeType.None;
        public string NpcId;
        [Tooltip("数值逻辑符合（大于等于、小于等于、等于）")]
        public ConditionOperator Operator = ConditionOperator.GreaterThanOrEqual;
        [Tooltip("数值")]
        public int Value; 

        [Tooltip("填入特殊物品的ID或名称")]
        public string ItemId;
        [Tooltip("打勾表示'必须拥有该物品'，不打勾表示'必须没有'")]
        public bool RequireItem = true; 
    }

    [Serializable]
    public class ChoiceReward
    {
        [Tooltip("奖励类型：改变属性 / 改变好感度 / 获得物品")]
        public ConditionType Type = ConditionType.PlayerAttribute;

        public PlayerAttributeType AttributeType = PlayerAttributeType.None;
        public string NpcId;
        public int Value; 
        public string ItemId;
    }

}