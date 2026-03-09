using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AVGGame.Editor
{
    // ---------------------------------------------------------
    // 基础枚举和数据类 (和运行时 Runtime 中的结构类似，但主要用于编辑器展示)
    // ---------------------------------------------------------
    public enum CharacterActionType { Enter, Leave, ChangeSprite }
    public enum CharacterPosition { Left, Center, Right }

    [Serializable]
    public class CharacterDisplayData
    {
        public string CharacterName;
        public CharacterActionType ActionType;
        public CharacterPosition Position;
        
        // 在编辑器里，我们不仅可以存路径，还可以直接拖拽 Sprite 图片进来
        // 导出 UGF 表格时，可以写一个脚本自动读取这个 Sprite 的路径
        public Sprite CharacterSprite; 
    }

    [Serializable]
    public class ChoiceData
    {
        public string ChoiceText;
        // xNode 中分支的连线稍微特殊一点，通常我们会给每个选项动态生成一个 Output 端口
        // 这里为了简单理解，我们先把它作为数据记录
    }
    
    [CreateAssetMenu(fileName = "Dialoguelayer", menuName = "国噶崛起/节点编辑器")]
    public class MazeLayerGraph : NodeGraph
    {
    }

    // ---------------------------------------------------------
    // 核心节点类：必须继承自 xNode.Node
    // ---------------------------------------------------------
    [CreateNodeMenu("AVG/对话节点 (Dialogue)")] // 在 xNode 图里右键菜单的名字
    public class DialogueNode : Node
    {
        // ==========================================
        // 1. 连线端口 (Ports) - 这是 xNode 的核心
        // ==========================================
        // [Input] 表示这是一个输入端口，别的节点可以连到这里
        [Input] public int Entry; 
        
        // [Output] 表示这是一个输出端口，从这里连向下一个剧情节点
        [Output] public int Exit;

        // ==========================================
        // 2. 核心剧情数据 (对应 StoryRowData 的字段)
        // ==========================================
        [Header("对话内容")]
        public string SpeakerName;
        
        [TextArea(3, 5)] // 让输入框大一点，方便写多行台词
        public string DialogText;

        [Header("角色表现指令")]
        // 策划可以在面板上随意添加、删除、排序这些角色动作
        public List<CharacterDisplayData> CharacterDisplays = new List<CharacterDisplayData>();

        [Header("分支选项 (如果有)")]
        // 如果这个列表有内容，说明这句话说完后要弹出选项
        public List<ChoiceData> Choices = new List<ChoiceData>();

        [Header("多媒体资源")]
        public Sprite BackgroundImage;
        public AudioClip BgmAudio;

        // ==========================================
        // 3. 编辑器辅助功能 (比如之前提到的高亮)
        // ==========================================
        [HideInInspector] // 隐藏起来，只通过代码控制
        public bool NeedsAttention = false;
        
        public string EditorNote; // 给策划写的备忘录或标记

        //TODO:重写颜色方法：如果带有策划标记，让节点在图里变成醒目的橙色
        

        // xNode 必须实现的方法：用来传递端口数据。
        // 在我们这种 AVG 流程控制中，通常不需要传递具体的值，返回 null 即可。
        public override object GetValue(NodePort port)
        {
            return null; 
        }
    }
}