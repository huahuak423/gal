using UnityEngine;
using XNode;

namespace AVGGame.Editor
{
    [CreateNodeMenu("AVG/子图跳转 (Sub Graph)")]
    [NodeTint("#4CAF50")] 
    [NodeWidth(280)]      
    public class SubGraphNode : Node
    {
        [Input] public int Entry;

        public NodeGraph TargetGraphAsset;
        public string TargetGraphNameFallback;

        public string GetTargetGraphName()
        {
            if (TargetGraphAsset != null) return TargetGraphAsset.name;
            return TargetGraphNameFallback;
        }

        public override object GetValue(NodePort port) { return null; }
    }
}