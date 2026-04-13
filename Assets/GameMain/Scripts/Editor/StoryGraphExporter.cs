#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using XNode;
using AVGGame; 

namespace AVGGame.Editor
{
    public class StoryGraphExporter : EditorWindow
    {
        [MenuItem("AVG Tools/2. 导出 xNode 为 UGF 数据表（单个）")]
        public static void ExportGraphToUGF()
        {
            NodeGraph targetGraph = Selection.activeObject as NodeGraph;
            if (targetGraph == null)
            {
                Debug.LogWarning("[StoryGraphExporter] 请先选中一个 NodeGraph 资产");
                return;
            }

            string content = BuildExportContent(targetGraph);
            string savePath = Path.Combine(Application.dataPath, "GameMain/DataTables", targetGraph.name + ".txt");

            File.WriteAllText(savePath, content, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"[StoryGraphExporter] 导出完成: {savePath}");
        }

        [MenuItem("AVG Tools/3. 批量导出 xNode 为 UGF 数据表")]
        public static void BatchExportGraphsToUGF()
        {
            // 收集所有选中的 NodeGraph（包括文件夹内的）
            List<NodeGraph> graphs = new List<NodeGraph>();

            foreach (var obj in Selection.objects)
            {
                if (obj is NodeGraph graph)
                {
                    graphs.Add(graph);
                }
                else if (obj is DefaultAsset folderAsset)
                {
                    // 选中了文件夹，递归查找里面所有 NodeGraph
                    string folderPath = AssetDatabase.GetAssetPath(folderAsset);
                    string[] guids = AssetDatabase.FindAssets("t:NodeGraph", new[] { folderPath });
                    foreach (string guid in guids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        NodeGraph g = AssetDatabase.LoadAssetAtPath<NodeGraph>(assetPath);
                        if (g != null) graphs.Add(g);
                    }
                }
            }

            if (graphs.Count == 0)
            {
                Debug.LogWarning("[StoryGraphExporter] 没有找到可导出的 NodeGraph，请选中图资产或包含图的文件夹");
                return;
            }

            // 去重
            HashSet<string> seen = new HashSet<string>();
            for (int i = graphs.Count - 1; i >= 0; i--)
            {
                string path = AssetDatabase.GetAssetPath(graphs[i]);
                if (!seen.Add(path)) graphs.RemoveAt(i);
            }

            // 选择输出目录
            string outputFolder = EditorUtility.SaveFolderPanel("选择导出目录", "Assets/GameMain/DataTables", "");
            if (string.IsNullOrEmpty(outputFolder)) return;

            int successCount = 0;
            for (int i = 0; i < graphs.Count; i++)
            {
                NodeGraph g = graphs[i];
                if (EditorUtility.DisplayCancelableProgressBar("批量导出", $"正在导出 {g.name} ({i + 1}/{graphs.Count})", (float)i / graphs.Count))
                {
                    break;
                }

                string content = BuildExportContent(g);
                string savePath = Path.Combine(outputFolder, g.name + ".txt");
                File.WriteAllText(savePath, content, Encoding.UTF8);
                successCount++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            Debug.Log($"[StoryGraphExporter] 批量导出完成: {successCount}/{graphs.Count} 个图已导出到 {outputFolder}");

            if (successCount > 0)
            {
                EditorUtility.DisplayDialog("批量导出完成", $"成功导出 {successCount} 个剧情表到:\n{outputFolder}", "确定");
            }
        }

        /// <summary>
        /// 将单个 NodeGraph 构建为 UGF 数据表文本内容
        /// </summary>
        private static string BuildExportContent(NodeGraph targetGraph)
        {
            Dictionary<Node, int> nodeToIdMap = new Dictionary<Node, int>();
            int currentId = 10000;
            foreach (Node node in targetGraph.nodes)
            {
                if (node != null) nodeToIdMap.Add(node, currentId++);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("##Id\tNextId\tNodeType\tSpeakerName\tDialogText\tCharacterActionsJson\tChoicesJson\tTargetGraphName\tRewardsJson\tBgmPath\tSePath\tPerformanceKey\tBackgroundPath\tVoicePath");
            sb.AppendLine("##int\tint\tint\tstring\tstring\tstring\tstring\tstring\tstring\tstring\tstring\tstring\tstring\tstring");
            sb.AppendLine("##编号\t下一句ID\t类型(0对话1选项2跳转3奖励)\t说话人\t台词\t动作配置\t选项配置\t目标图名称\t奖励配置\t背景音乐\t音效\t动画效果Key\t背景图\t语音");

            foreach (Node node in targetGraph.nodes)
            {
                if (node == null) continue;
                int myId = nodeToIdMap[node];

                if (node is DialogueNode dNode)
                {
                    int nextId = GetNextNodeId(node, "Exit", nodeToIdMap);
                    string actionsJson = dNode.CharacterDisplays.Count > 0 ? JsonUtility.ToJson(new RuntimeActionListWrapper { actions = dNode.CharacterDisplays }) : "";
                    sb.AppendLine($"{myId}\t{nextId}\t0\t{dNode.SpeakerName}\t{dNode.DialogText}\t{actionsJson}\t\t\t\t{dNode.BgmPath}\t{dNode.SePath}\t{dNode.PerformanceKey}\t{dNode.BackgroundPath}\t{dNode.VoicePath}");
                }
                else if (node is ChoiceNode cNode)
                {
                    List<RuntimeChoiceData> runtimeChoices = new List<RuntimeChoiceData>();
                    for (int i = 0; i < cNode.Choices.Count; i++)
                    {
                        ChoiceItemData editorChoice = cNode.Choices[i];
                        int targetId = GetNextNodeId(node, "Choices " + i, nodeToIdMap);
                        runtimeChoices.Add(new RuntimeChoiceData { ChoiceText = editorChoice.ChoiceText, NextId = targetId, Conditions = editorChoice.Conditions, Rewards = editorChoice.Rewards });
                    }
                    string choicesJson = runtimeChoices.Count > 0 ? JsonUtility.ToJson(new RuntimeChoiceListWrapper { Choices = runtimeChoices }) : "";
                    sb.AppendLine($"{myId}\t0\t1\t\t\t\t{choicesJson}\t\t\t\t\t\t\t");
                }
                else if (node is SubGraphNode subNode)
                {
                    string targetName = subNode.GetTargetGraphName();
                    int nextId = GetNextNodeId(node, "Exit", nodeToIdMap);
                    sb.AppendLine($"{myId}\t{nextId}\t2\t\t\t\t\t{targetName}\t\t\t\t\t\t");
                }
                else if (node is RewardNode rNode)
                {
                    string rewardsJson = JsonUtility.ToJson(new RuntimeRewardListWrapper { RewardTitle = rNode.RewardTitle, Rewards = rNode.Rewards });
                    int nextId = GetNextNodeId(node, "Exit", nodeToIdMap);
                    sb.AppendLine($"{myId}\t{nextId}\t3\t\t\t\t\t\t{rewardsJson}\t\t\t\t\t");
                }
            }

            return sb.ToString();
        }

        private static int GetNextNodeId(Node currentNode, string portName, Dictionary<Node, int> idMap)
        {
            NodePort outPort = currentNode.GetPort(portName);
            if (outPort != null && outPort.Connection != null && outPort.Connection.node != null)
            {
                return idMap.ContainsKey(outPort.Connection.node) ? idMap[outPort.Connection.node] : 0;
            }
            return 0; 
        }

        
    }
}
#endif