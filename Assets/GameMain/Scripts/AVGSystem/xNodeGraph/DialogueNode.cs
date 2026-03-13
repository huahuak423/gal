using UnityEngine;
using XNode;
using System.Collections.Generic;

namespace AVGGame.Editor
{
    // 角色表现指令结构
    public enum CharacterActionType { Enter, Leave, ChangeSprite }
    public enum CharacterPosition { Left, Center, Right }

    [System.Serializable]
    public class CharacterDisplayData
    {
        public string CharacterName;
        public CharacterActionType ActionType;
        public CharacterPosition Position;
        public string SpritePath;
    }

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