using UnityEngine;
using XNode;
using System.Collections.Generic;
using AVGGame.Runtime; // 引用运行时里的数据定义

namespace AVGGame.Editor
{
    
    
    [CreateNodeMenu("AVG/对话节点 (Dialogue)")]
    [NodeWidth(350)] 
    public class DialogueNode : Node
    {
        [Input] public int Entry;
        [Output] public int Exit;

        public string SpeakerName;
        [TextArea(3, 5)]
        public string DialogText;

        public List<CharacterDisplayData> CharacterDisplays = new List<CharacterDisplayData>();

        [HideInInspector] 
        public bool NeedsAttention = false; 
        public string EditorNote;

        public override object GetValue(NodePort port) { return null; }
    }
}