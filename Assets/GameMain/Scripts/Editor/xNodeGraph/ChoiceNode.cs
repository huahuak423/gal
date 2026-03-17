using UnityEngine;
using XNode;
using System.Collections.Generic;
using AVGGame; // 引用运行时里的数据定义

namespace AVGGame.Editor
{
    
    
    [CreateNodeMenu("AVG/分支选项节点 (Choice)")]
    [NodeWidth(380)]
    [NodeTint("#5271FF")]
    public class ChoiceNode : Node
    {
        [Input] public int Entry;

        [Output(dynamicPortList = true)] 
        public List<ChoiceItemData> Choices = new List<ChoiceItemData>();

        public override object GetValue(NodePort port) { return null; }
    }
}