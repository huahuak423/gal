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
        
        [Header("音频设置 (Audio)")]
        [Tooltip("填入 BGM 的资源路径或名称，留空代表不切换（延续前面）")]
        public string BgmPath; 
        [Tooltip("填入 临时音效 的资源路径或名称，留空代表无音效")]
        public string SePath;
        
        [HideInInspector] 
        public bool NeedsAttention = false; 
        public string EditorNote;

        public override object GetValue(NodePort port) { return null; }
    }
}