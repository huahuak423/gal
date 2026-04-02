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
        [MenuItem("AVG Tools/2. 导出 xNode 为 UGF 数据表")]
        public static void ExportGraphToUGF()
        {
            NodeGraph targetGraph = Selection.activeObject as NodeGraph;
            if (targetGraph == null) return;

            Dictionary<Node, int> nodeToIdMap = new Dictionary<Node, int>();
            int currentId = 10000; 
            foreach (Node node in targetGraph.nodes)
            {
                if (node != null) nodeToIdMap.Add(node, currentId++);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Id\tNextId\tNodeType\tSpeakerName\tDialogText\tCharacterActionsJson\tChoicesJson\tTargetGraphName\tRewardsJson\tBgmPath\tSePath\tPerformanceKey\tBackgroundPath");
            sb.AppendLine("int\tint\tint\tstring\tstring\tstring\tstring\tstring\tstring\tstring\tstring\tstring\tstring");
            sb.AppendLine("编号\t下一句ID\t类型(0对话1选项2跳转3奖励)\t说话人\t台词\t动作配置\t选项配置\t目标图名称\t奖励配置\t背景音乐\t音效\t动画效果Key\t背景图");

            foreach (Node node in targetGraph.nodes)
            {
                if (node == null) continue;
                int myId = nodeToIdMap[node];

                if (node is DialogueNode dNode)
                {
                    int nextId = GetNextNodeId(node, "Exit", nodeToIdMap);
                    string actionsJson = dNode.CharacterDisplays.Count > 0 ? JsonUtility.ToJson(new RuntimeActionListWrapper { actions = dNode.CharacterDisplays }) : "";
                    
                    // 【修改】最后加上 Bgm 和 Se 的变量。中间用 \t 跳过 Choices, TargetGraph, Rewards
                    sb.AppendLine($"{myId}\t{nextId}\t0\t{dNode.SpeakerName}\t{dNode.DialogText}\t{actionsJson}\t\t\t\t{dNode.BgmPath}\t{dNode.SePath}\t{dNode.PerformanceKey}\t{dNode.BackgroundPath}"); 
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
                    sb.AppendLine($"{myId}\t0\t1\t\t\t\t{choicesJson}\t\t\t\t\t\t");
                }
                else if (node is SubGraphNode subNode)
                {
                    string targetName = subNode.GetTargetGraphName(); 
                    int nextId = GetNextNodeId(node, "Exit", nodeToIdMap);
                    sb.AppendLine($"{myId}\t{nextId}\t2\t\t\t\t\t{targetName}\t\t\t\t\t");
                }
                else if (node is RewardNode rNode) 
                {
                    string rewardsJson = JsonUtility.ToJson(new RuntimeRewardListWrapper { RewardTitle = rNode.RewardTitle, Rewards = rNode.Rewards });
                    int nextId = GetNextNodeId(node, "Exit", nodeToIdMap);
                    sb.AppendLine($"{myId}\t{nextId}\t3\t\t\t\t\t\t{rewardsJson}\t\t\t\t");
                }
            }

            string savePath = EditorUtility.SaveFilePanel("保存 UGF 数据表", "Assets/", targetGraph.name + ".txt", "txt");
            
            if (!string.IsNullOrEmpty(savePath))
            {
                File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
                AssetDatabase.Refresh();
            }
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