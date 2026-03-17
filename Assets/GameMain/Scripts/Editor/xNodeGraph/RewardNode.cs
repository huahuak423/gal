using UnityEngine;
using XNode;
using System.Collections.Generic;
using AVGGame; // 引用运行时里的数据定义

namespace AVGGame.Editor
{
    [CreateNodeMenu("AVG/奖励与结算节点 (Reward Exit)")]
    [NodeWidth(350)]
    [NodeTint("#FFB300")] // 显眼的金色，代表奖励
    public class RewardNode : Node
    {
        
        [Input] public int Entry;
        [Output] public int Exit;
        
        [Header("结算 UI 表现")]
        [Tooltip("结算时可能会用到的文本（可空）")]
        public string RewardTitle;

        [Header("发放奖励与推进进度")]
        [Tooltip("填入对应的属性加减、道具获得、NPC进度推进")]
        public List<ChoiceReward> Rewards = new List<ChoiceReward>();

        public override object GetValue(NodePort port) { return null; }
    }
}