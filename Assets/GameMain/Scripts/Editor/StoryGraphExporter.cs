#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using XNode;
using AVGGame.Runtime; 

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
            sb.AppendLine("Id\tNextId\tNodeType\tSpeakerName\tDialogText\tCharacterActionsJson\tChoicesJson\tTargetGraphName");
            sb.AppendLine("int\tint\tint\tstring\tstring\tstring\tstring\tstring");
            sb.AppendLine("编号\t下一句ID\t类型(0对话1选项2跳转)\t说话人\t台词\t动作配置\t选项配置\t目标图名称");

            foreach (Node node in targetGraph.nodes)
            {
                if (node == null) continue;
                int myId = nodeToIdMap[node];

                if (node is DialogueNode dNode)
                {
                    int nextId = GetNextNodeId(node, "Exit", nodeToIdMap);
                    string actionsJson = dNode.CharacterDisplays.Count > 0 ? JsonUtility.ToJson(new ActionListWrapper { actions = dNode.CharacterDisplays }) : "";
                    sb.AppendLine($"{myId}\t{nextId}\t0\t{dNode.SpeakerName}\t{dNode.DialogText}\t{actionsJson}\t\t");
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
                    sb.AppendLine($"{myId}\t0\t1\t\t\t\t{choicesJson}\t");
                }
                else if (node is SubGraphNode subNode)
                {
                    string targetName = subNode.GetTargetGraphName(); 
                    sb.AppendLine($"{myId}\t0\t2\t\t\t\t\t{targetName}");
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

        [System.Serializable]
        private class ActionListWrapper { public List<CharacterDisplayData> actions; }
    }
}
#endif