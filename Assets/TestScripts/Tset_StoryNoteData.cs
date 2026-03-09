using System;
using System.Collections.Generic;
using UnityEngine;

namespace AVGGame.Editor
{
    /// <summary>
    /// 节点类型枚举，方便编辑器区分渲染不同颜色的节点
    /// </summary>
    public enum StoryNodeType
    {
        Dialogue,   // 普通对话节点 (包含文字、立绘、背景切换)
        Choice,     // 分支选项节点
        Event       // 纯事件节点 (比如纯粹为了加减属性，不显示文字)
    }

    /// <summary>
    /// 选项数据结构
    /// </summary>
    [Serializable]
    public class StoryChoiceData
    {
        public string ChoiceText;       // 选项显示的文字
        public string NextNodeGUID;     // 玩家点击该选项后，连接到的下一个节点ID
        public string Condition;        // 【进阶】显示条件，例如 "Intelligence>50" (可先留空)
    }

    /// <summary>
    /// 核心：单个剧情节点的数据结构
    /// </summary>
    [Serializable]
    public class StoryNodeData
    {
        // ================== 编辑器专属数据 ==================
        public string GUID;             // 节点的唯一标识符 (非常重要！连线全靠它)
        public Rect Position;           // 节点在编辑器视图里的位置 (x, y)
        public StoryNodeType NodeType;  // 节点类型

        // ================== 核心视觉小说数据 (P0) ==================
        public string SpeakerName;      // 说话人
        [TextArea(3, 5)]
        public string DialogText;       // 台词内容

        // 视听表现 (如果为空，表示延续上一个节点的状态)
        public string BgPath;           // 背景图路径
        public string SpritePath;       // 立绘路径
        public string BgmPath;          // 背景音乐路径
        public string SfxPath;          // 特殊音效路径

        // ================== 扩展玩法数据 (P1/P2) ==================
        // 比如："Intelligence|+5", "Clue|A001"
        public List<string> AttributeChanges = new List<string>(); 

        // ================== 连线关系 ==================
        // 普通对话只会连接到一个节点
        public string NextNodeGUID;     

        // 如果是 Choice 节点，使用这个列表来存储多个分支选项
        public List<StoryChoiceData> Choices = new List<StoryChoiceData>(); 
    }

    /// <summary>
    /// 整个剧本图的数据容器 (继承 ScriptableObject 方便在 Unity 项目中保存为资产文件)
    /// </summary>
    [CreateAssetMenu(fileName = "NewStoryGraph", menuName = "AVG Tool/Story Graph")]
    public class StoryGraphData : ScriptableObject
    {
        // 保存当前剧本的所有节点
        public List<StoryNodeData> Nodes = new List<StoryNodeData>();
        
        // 记录剧本的起始节点 ID
        public string EntryNodeGUID;
    }
}