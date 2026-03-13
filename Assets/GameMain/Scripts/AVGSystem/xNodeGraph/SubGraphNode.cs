using UnityEngine;
using XNode;

namespace AVGGame.Editor
{
    /// <summary>
    /// 宏观流程控制：用于代表一个剧情片段或跳转到另一个图
    /// </summary>
    [CreateNodeMenu("AVG/子图跳转 (Sub Graph)")]
    [NodeTint("#4CAF50")] 
    [NodeWidth(280)]      
    public class SubGraphNode : Node
    {
        [Input] public int Entry;
        
        // 【新增】出口端口，用于在宏观图中串联流程
        [Output] public int Exit;

        [Header("目标子图设置")]
        [Tooltip("直接将要跳转到的 xNode 图资产拖拽到这里")]
        public NodeGraph TargetGraphAsset;

        [Tooltip("备用：如果没拖资产，可以手动写图的名字")]
        public string TargetGraphNameFallback;

        public string GetTargetGraphName()
        {
            if (TargetGraphAsset != null) return TargetGraphAsset.name;
            return TargetGraphNameFallback;
        }

        public override object GetValue(NodePort port) => null;
    }
}