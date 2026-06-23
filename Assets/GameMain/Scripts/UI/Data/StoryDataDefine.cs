using System;
using System.Collections.Generic;
using UnityEngine;

namespace AVGGame
{
    // ==========================================
    // 1. 基础枚举定义
    // ==========================================
    public enum ConditionOperator { GreaterThanOrEqual, LessThanOrEqual, Equal }
    public enum ConditionType { NpcFavorability, SpecialItem }
    // 角色表现指令结构
    public enum CharacterActionType { Enter, Leave, ChangeSprite }
    public enum CharacterPosition { Left, Center, Right, EX1, EX2, EX3, EX4 }

    
    // ==========================================
    // 2. 选择条件与奖励结构
    // ==========================================
    [Serializable]
    public class ChoiceCondition
    {
        [Tooltip("条件类型：好感度 / 特殊物品")]
        public ConditionType Type = ConditionType.NpcFavorability;

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
        [Tooltip("奖励类型：改变好感度 / 获得物品")]
        public ConditionType Type = ConditionType.NpcFavorability;

        public string NpcId;
        public int Value;
        public string ItemId;
    }

    // ==========================================
    // 3. 玩家数值数据（流程层传递给InformationPanel）
    // ==========================================
    /// <summary>
    /// 传递给 InformationPanel 的玩家数值
    /// </summary>
    public class PlayerStatsData
    {
        public int ActionPoints;    // 当前行动点
        public int MaxActionPoints; // 最大行动点
        public int CurrentRound;    // 当前周目
    }

    // ==========================================
    // 4. 对话显示数据（流程层传递给UI）
    // ==========================================
    /// <summary>
    /// 传递给 DialoguePanel 的显示数据
    /// </summary>
    public class DialogueDisplayData
    {
        public string SpeakerName;
        public string DialogText;
        public int NextId;           // 下一条ID（0表示结束）
        public int CurrentNodeId;    // 当前节点ID
        public int NodeType;         // 节点类型（对话、选项等）
        public string ChoicesJson;   // 选项配置（如果是选项节点）
        public string CharacterActionsJson; // 立绘动作配置JSON
        public string BackgroundPath;       // 背景图资源路径
        public string BgmPath;              // 背景音乐资源路径
        public string VoicePath;            // 语音资源路径
        public string SePath;               // 音效资源路径
        public bool HideDialoguePanel;      // 是否隐藏对话框区域
    }
}