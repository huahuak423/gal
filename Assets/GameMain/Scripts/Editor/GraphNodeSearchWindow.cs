#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using XNode;
using XNodeEditor;

namespace AVGGame.Editor
{
    /// <summary>
    /// 快速定位工具：在复杂的 Graph 中通过文本或类型搜索节点
    /// </summary>
    public class GraphNodeSearchWindow : EditorWindow
    {
        private string searchString = "";
        private SearchType filterType = SearchType.All;
        private Vector2 scrollPos;
        private List<Node> searchResults = new List<Node>();

        public enum SearchType { All, Dialogue, Choice, SubGraph }

        [MenuItem("AVG Tools/3. 节点快速导航器")]
        public static void ShowWindow()
        {
            GraphNodeSearchWindow window = GetWindow<GraphNodeSearchWindow>("剧情节点导航");
            window.minSize = new Vector2(350, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("🔍 节点快速搜索", EditorStyles.boldLabel);
            
            // 搜索设置区
            EditorGUILayout.BeginHorizontal();
            searchString = EditorGUILayout.TextField(searchString, GUILayout.Height(20));
            if (GUILayout.Button("清除", GUILayout.Width(40))) searchString = "";
            EditorGUILayout.EndHorizontal();

            filterType = (SearchType)EditorGUILayout.EnumPopup("过滤类型", filterType);
            EditorGUILayout.EndVertical();

            // 执行搜索逻辑
            UpdateSearch();

            // 结果显示区
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (searchResults.Count > 0)
            {
                foreach (var node in searchResults)
                {
                    DrawNodeResult(node);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未找到匹配的节点，请尝试切换关键词或图文件。", MessageType.Info);
            }
            EditorGUILayout.EndScrollView();

            if (NodeEditorWindow.current == null)
            {
                EditorGUILayout.HelpBox("请先打开一个 xNode 编辑器窗口！", MessageType.Warning);
            }
        }

        private void UpdateSearch()
        {
            searchResults.Clear();
            
            // 必须先打开一个 xNode 图
            if (NodeEditorWindow.current == null || NodeEditorWindow.current.graph == null) return;

            NodeGraph graph = NodeEditorWindow.current.graph;

            foreach (var node in graph.nodes)
            {
                if (node == null) continue;

                bool typeMatch = false;
                bool contentMatch = false;

                // 1. 类型过滤
                if (filterType == SearchType.All) typeMatch = true;
                else if (filterType == SearchType.Dialogue && node is DialogueNode) typeMatch = true;
                else if (filterType == SearchType.Choice && node is ChoiceNode) typeMatch = true;
                else if (filterType == SearchType.SubGraph && node is SubGraphNode) typeMatch = true;

                if (!typeMatch) continue;

                // 2. 内容搜索
                if (string.IsNullOrEmpty(searchString))
                {
                    contentMatch = true;
                }
                else
                {
                    string s = searchString.ToLower();
                    if (node is DialogueNode dNode)
                    {
                        contentMatch = dNode.DialogText.ToLower().Contains(s) || dNode.SpeakerName.ToLower().Contains(s);
                    }
                    else if (node is ChoiceNode cNode)
                    {
                        // 搜索所有选项文本
                        contentMatch = cNode.Choices.Any(c => c.ChoiceText.ToLower().Contains(s));
                    }
                    else if (node is SubGraphNode sNode)
                    {
                        // 搜索跳转目标名
                        string targetName = sNode.GetTargetGraphName();
                        contentMatch = targetName.ToLower().Contains(s);
                    }
                }

                if (contentMatch) searchResults.Add(node);
            }
        }

        private void DrawNodeResult(Node node)
        {
            EditorGUILayout.BeginHorizontal("button");
            
            // 图标与标题
            string nodeInfo = GetNodeSummary(node);
            Color originalColor = GUI.color;
            GUI.color = GetNodeColor(node);
            GUILayout.Box("", GUILayout.Width(10), GUILayout.Height(10));
            GUI.color = originalColor;

            if (GUILayout.Button($"{node.name}: {nodeInfo}", EditorStyles.label))
            {
                FocusNode(node);
            }

            if (GUILayout.Button("跳转", GUILayout.Width(50)))
            {
                FocusNode(node);
            }

            EditorGUILayout.EndHorizontal();
        }

        private string GetNodeSummary(Node node)
        {
            if (node is DialogueNode dNode) return dNode.DialogText.Length > 20 ? dNode.DialogText.Substring(0, 17) + "..." : dNode.DialogText;
            if (node is ChoiceNode cNode) return $"共 {cNode.Choices.Count} 个选项";
            if (node is SubGraphNode sNode) return $"目标: {sNode.GetTargetGraphName()}";
            return "";
        }

        private Color GetNodeColor(Node node)
        {
            if (node is DialogueNode) return Color.white;
            if (node is ChoiceNode) return new Color(0.32f, 0.44f, 1f);
            if (node is SubGraphNode) return Color.green;
            return Color.gray;
        }

        /// <summary>
        /// 核心：将编辑器视角移动到指定节点
        /// </summary>
        private void FocusNode(Node node)
        {
            if (NodeEditorWindow.current == null) return;

            // 1. 选中该节点
            Selection.activeObject = node;

            // 2. 调整 xNode 编辑器的视野 (居中该节点)
            // xNode 的 panOffset 是以窗口中心为原点的偏移量
            NodeEditorWindow.current.panOffset = -node.position;
            
            // 顺便强制重绘一下
            NodeEditorWindow.current.Repaint();
        }
    }
}
#endif