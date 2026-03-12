using UnityEngine;
using XNodeEditor; 

namespace AVGGame.Editor
{
    // 让带有标记的对话节点变成橙色
    [CustomNodeEditor(typeof(DialogueNode))]
    public class DialogueNodeEditor : NodeEditor
    {
        public override Color GetTint()
        {
            DialogueNode node = target as DialogueNode;
            if (node != null && node.NeedsAttention)
            {
                return new Color(1f, 0.6f, 0.2f); 
            }
            return base.GetTint();
        }
    }
}