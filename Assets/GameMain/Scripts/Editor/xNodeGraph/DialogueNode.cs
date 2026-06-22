using UnityEngine;
using XNode;
using System.Collections.Generic;
using AVGGame; // 引用运行时里的数据定义

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
        
        [Header("音频设置和背景图")]
        [Tooltip("填入 BGM 的资源路径或名称，留空代表不切换（延续前面）")]
        public string BgmPath;
        [Tooltip("填入 临时音效 的资源路径或名称，留空代表无音效")]
        public string SePath;
        [Tooltip("填入 背景图 的资源路径或名称，留空代表无背景图")]
        public string BackgroundPath;
        [Tooltip("填入 语音 的资源路径或名称，留空代表无语音")]
        public string VoicePath;

        [Header("动画效果设置")]
        public string PerformanceKey;

        [Header("面板控制")]
        [Tooltip("勾选后，运行到此节点时隐藏对话框区域（保留背景和立绘）")]
        public bool HideDialoguePanel = false;

        [HideInInspector]
        public bool NeedsAttention = false;
        public string EditorNote;

        public override object GetValue(NodePort port) { return null; }
    }
}  