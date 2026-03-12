#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using XNode;

namespace AVGGame.Editor
{
    public class StoryGraphImporter : EditorWindow
    {
        [MenuItem("AVG Tools/1. 从 CSV 导入剧本到 xNode")]
        public static void ImportCSVToGraph()
        {
            NodeGraph targetGraph = Selection.activeObject as NodeGraph;
            if (targetGraph == null)
            {
                EditorUtility.DisplayDialog("提示", "请先在 Project 窗口选中图文件！", "确定");
                return;
            }

            string filePath = EditorUtility.OpenFilePanel("选择策划配置表(CSV)", "", "csv");
            if (string.IsNullOrEmpty(filePath)) return;

            string[] lines = File.ReadAllLines(filePath);
            DialogueNode previousNode = null;
            Vector2 spawnPosition = new Vector2(0, 0);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = ParseCSVLine(line);

                if (parts.Length >= 3)
                {
                    DialogueNode newNode = targetGraph.AddNode<DialogueNode>();
                    newNode.name = "对话节点";
                    
                    newNode.SpeakerName = parts[0].Trim(); // 第1列：角色编号
                    newNode.DialogText = parts[2].Trim();  // 第3列：文本

                    if (parts.Length >= 4 && !string.IsNullOrWhiteSpace(parts[3]))
                    {
                        newNode.NeedsAttention = true;
                        newNode.EditorNote = "【立绘标记】: " + parts[3].Trim();
                        
                        newNode.CharacterDisplays.Add(new CharacterDisplayData 
                        { 
                            CharacterName = newNode.SpeakerName,
                            ActionType = CharacterActionType.ChangeSprite 
                        });
                    }

                    newNode.position = spawnPosition;
                    spawnPosition.y += 180; 

                    if (previousNode != null)
                    {
                        NodePort outPort = previousNode.GetPort("Exit");
                        NodePort inPort = newNode.GetPort("Entry");
                        if (outPort != null && inPort != null) outPort.Connect(inPort);
                    }
                    previousNode = newNode;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("成功", "剧本导入完毕！", "确定");
        }

        private static string[] ParseCSVLine(string line)
        {
            Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] fields = csvParser.Split(line);
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].TrimStart('"').TrimEnd('"').Replace("\"\"", "\"");
            }
            return fields;
        }
    }
}
#endif