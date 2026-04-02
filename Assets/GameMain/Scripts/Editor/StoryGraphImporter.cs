#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using XNode;
using AVGGame; // 引用运行时里的数据定义

namespace AVGGame.Editor
{
    /// <summary>
    /// 策划专用：将 CSV 配置表一键导入为 xNode 连线图，并自动映射角色名称
    /// </summary>
    public class StoryGraphImporter : EditorWindow
    {
        [MenuItem("AVG Tools/1. 从 CSV 导入剧本到 xNode")]
        public static void ImportCSVToGraph()
        {
            // 1. 确保当前选中了一个 Graph
            NodeGraph targetGraph = Selection.activeObject as NodeGraph;
            if (targetGraph == null)
            {
                EditorUtility.DisplayDialog("提示", "请先在 Project 窗口选中或创建一个 xNode 的 Graph 文件！", "确定");
                return;
            }

            // 2. 选择策划导出的 CSV 文件
            string filePath = EditorUtility.OpenFilePanel("选择策划配置表", "", "csv");
            if (string.IsNullOrEmpty(filePath)) return;

            string[] lines = File.ReadAllLines(filePath);
            DialogueNode previousNode = null;
            Vector2 spawnPosition = new Vector2(0, 0);

            // 记录原始节点数量，用于判断是否需要保存
            int originalNodeCount = (targetGraph.nodes != null) ? targetGraph.nodes.Count : 0;

            // 获取 Graph 的资产路径，用于添加子资产
            string graphPath = AssetDatabase.GetAssetPath(targetGraph);

            // 3. 开始解析并生成 (从第2行开始，跳过表头)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = ParseCSVLine(line);

                if (parts.Length >= 3)
                {
                    DialogueNode newNode = targetGraph.AddNode<DialogueNode>();

                    // 关键：将节点作为子资产添加到 Graph
                    if (!string.IsNullOrEmpty(graphPath))
                    {
                        AssetDatabase.AddObjectToAsset(newNode, targetGraph);
                    }

                    newNode.name = $"对话_{i}_{parts[2].Substring(0, Mathf.Min(10, parts[2].Length))}"; // 更有意义的名称
                    
                    // --- 核心修改：通过角色编号映射角色名称 ---
                    string roleID = parts[0].Trim();
                    newNode.SpeakerName = GetCharacterName(roleID);
                    
                    // 第3列 (索引2)：节点文本
                    newNode.DialogText = parts[2].Trim();
                    
                    // 第4列 (索引3)：策划特殊标记
                    if (parts.Length >= 4 && !string.IsNullOrWhiteSpace(parts[3]))
                    {
                        string marker = parts[3].Trim();
                        newNode.NeedsAttention = true;
                        newNode.EditorNote = "【立绘/演出标记】: " + marker;
                    }

                    newNode.position = spawnPosition;
                    spawnPosition.x += 450; 

                    if (previousNode != null)
                    {
                        NodePort outPort = previousNode.GetPort("Exit");
                        NodePort inPort = newNode.GetPort("Entry");
                        if (outPort != null && inPort != null) outPort.Connect(inPort);
                    }
                    
                    previousNode = newNode;
                }
            }

            // 标记 Graph 为脏，确保序列化
            EditorUtility.SetDirty(targetGraph);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 强制重新加载 Graph
            if (!string.IsNullOrEmpty(graphPath))
            {
                AssetDatabase.ImportAsset(graphPath, ImportAssetOptions.ForceUpdate);
            }

            EditorUtility.DisplayDialog("成功", $"剧本导入完毕！角色名已根据编号自动转换。\n\n共导入 {targetGraph.nodes.Count - originalNodeCount} 个节点。", "确定");
        }

        /// <summary>
        /// 根据角色编号获取对应的角色名称
        /// </summary>
        private static string GetCharacterName(string id)
        {
            switch (id)
            {
                case "00": return "";          // 旁白：返回空字符串
                case "01": return "女主名";
                case "02": return "许映月";
                case "03": return "周杉";
                case "04": return "陈予宁";
                case "05": return "陈予荣";
                case "06": return "温叙";
                case "07": return "何行舟";
                case "08": return "群众";
                default:
                    // 如果出现了未定义的 ID，暂时保留 ID 以便排查，也可以返回空
                    return id; 
            }
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