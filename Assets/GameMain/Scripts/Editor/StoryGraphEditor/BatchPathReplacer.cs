#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using XNode;
using AVGGame.Editor;

namespace AVGGame.Editor
{
    /// <summary>
    /// 批量修改节点图中的资源路径
    /// </summary>
    public class BatchPathReplacer : EditorWindow
    {
        private ReplaceTargetType m_ReplaceTarget = ReplaceTargetType.BackgroundPath;
        private string m_OldPath = "";
        private string m_NewPath = "";
        private List<string> m_SelectedGraphPaths = new List<string>();
        private string m_SelectedFolderPath = "";

        private bool m_IsReplacing = false;

        public enum ReplaceTargetType
        {
            BackgroundPath,
            SePath,
            BgmPath,
            CharacterSpritePath
        }

        [MenuItem("AVG Tools/批量修改/节点图资源路径")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchPathReplacer>("批量路径替换");
            window.minSize = new Vector2(500, 450);
        }

        private void OnGUI()
        {
            GUILayout.Label("━━━ 批量修改节点图资源路径 ━━━", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "功能说明：\n" +
                "1. 选择要替换的属性类型\n" +
                "2. 添加节点图文件或选择文件夹\n" +
                "3. 输入原路径和新路径\n" +
                "4. 点击执行替换",
                MessageType.Info);

            EditorGUILayout.Space();

            // 1. 选择替换目标
            GUILayout.Label("1. 选择要替换的属性", EditorStyles.boldLabel);
            m_ReplaceTarget = (ReplaceTargetType)EditorGUILayout.EnumPopup("替换目标", m_ReplaceTarget);

            string targetHint = m_ReplaceTarget switch
            {
                ReplaceTargetType.BackgroundPath => "背景图路径",
                ReplaceTargetType.SePath => "音效路径",
                ReplaceTargetType.BgmPath => "背景音乐路径",
                ReplaceTargetType.CharacterSpritePath => "立绘路径（CharacterDisplays 内的 SpritePath）",
                _ => ""
            };
            EditorGUILayout.HelpBox($"将替换选中节点的: {targetHint}", MessageType.None);

            EditorGUILayout.Space();

            // 2. 添加节点图
            GUILayout.Label("2. 选择节点图文件", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加节点图文件"))
            {
                string path = EditorUtility.OpenFilePanel("选择节点图", "Assets", "asset");
                if (!string.IsNullOrEmpty(path) && !m_SelectedGraphPaths.Contains(path))
                {
                    m_SelectedGraphPaths.Add(path);
                }
            }

            if (GUILayout.Button("从文件夹选择"))
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    m_SelectedFolderPath = path;
                    var guids = AssetDatabase.FindAssets("t:NodeGraph", new[] { path });
                    foreach (var guid in guids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (!m_SelectedGraphPaths.Contains(assetPath))
                        {
                            m_SelectedGraphPaths.Add(assetPath);
                        }
                    }
                }
            }

            if (GUILayout.Button("清空列表"))
            {
                m_SelectedGraphPaths.Clear();
                m_SelectedFolderPath = "";
            }
            EditorGUILayout.EndHorizontal();

            // 显示已选择的节点图
            if (m_SelectedGraphPaths.Count > 0)
            {
                EditorGUILayout.LabelField($"已选择 {m_SelectedGraphPaths.Count} 个节点图:", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
                foreach (var graphPath in m_SelectedGraphPaths.ToList())
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(graphPath, GUILayout.MaxWidth(350));
                    if (GUILayout.Button("移除", GUILayout.Width(50)))
                    {
                        m_SelectedGraphPaths.Remove(graphPath);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("尚未选择节点图文件", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // 3. 输入路径
            GUILayout.Label("3. 输入路径", EditorStyles.boldLabel);
            m_OldPath = EditorGUILayout.TextField("原路径", m_OldPath);
            m_NewPath = EditorGUILayout.TextField("新路径", m_NewPath);

            if (!string.IsNullOrEmpty(m_OldPath) && m_OldPath == m_NewPath)
            {
                EditorGUILayout.HelpBox("原路径和新路径相同，无需替换", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // 4. 执行按钮
            GUI.enabled = !m_IsReplacing && m_SelectedGraphPaths.Count > 0 && !string.IsNullOrEmpty(m_OldPath) && !string.IsNullOrEmpty(m_NewPath);
            if (GUILayout.Button("执行替换", GUILayout.Height(40)))
            {
                ExecuteReplace();
            }
            GUI.enabled = true;

            if (m_IsReplacing)
            {
                EditorGUILayout.HelpBox("替换中...", MessageType.Info);
            }
        }

        private void ExecuteReplace()
        {
            m_IsReplacing = true;
            int totalReplaced = 0;
            int totalNodes = 0;
            List<string> modifiedGraphs = new List<string>();

            try
            {
                foreach (var graphPath in m_SelectedGraphPaths)
                {
                    var graph = AssetDatabase.LoadAssetAtPath<NodeGraph>(graphPath);
                    if (graph == null)
                    {
                        Debug.LogWarning($"[BatchPathReplacer] 无法加载节点图: {graphPath}");
                        continue;
                    }

                    int graphReplaced = 0;
                    int graphNodes = 0;

                    foreach (var node in graph.nodes)
                    {
                        if (node is DialogueNode dialogueNode)
                        {
                            graphNodes++;
                            bool modified = false;

                            switch (m_ReplaceTarget)
                            {
                                case ReplaceTargetType.BackgroundPath:
                                    if (dialogueNode.BackgroundPath == m_OldPath)
                                    {
                                        dialogueNode.BackgroundPath = m_NewPath;
                                        modified = true;
                                    }
                                    break;

                                case ReplaceTargetType.SePath:
                                    if (dialogueNode.SePath == m_OldPath)
                                    {
                                        dialogueNode.SePath = m_NewPath;
                                        modified = true;
                                    }
                                    break;

                                case ReplaceTargetType.BgmPath:
                                    if (dialogueNode.BgmPath == m_OldPath)
                                    {
                                        dialogueNode.BgmPath = m_NewPath;
                                        modified = true;
                                    }
                                    break;

                                case ReplaceTargetType.CharacterSpritePath:
                                    foreach (var charDisplay in dialogueNode.CharacterDisplays)
                                    {
                                        if (charDisplay.SpritePath == m_OldPath)
                                        {
                                            charDisplay.SpritePath = m_NewPath;
                                            modified = true;
                                        }
                                    }
                                    break;
                            }

                            if (modified)
                            {
                                graphReplaced++;
                                EditorUtility.SetDirty(dialogueNode);
                            }
                        }
                    }

                    if (graphReplaced > 0)
                    {
                        AssetDatabase.SaveAssets();
                        modifiedGraphs.Add($"{Path.GetFileName(graphPath)}: 替换了 {graphReplaced} 处");
                        totalReplaced += graphReplaced;
                        totalNodes += graphNodes;
                    }
                }

                string message = $"替换完成!\n\n" +
                    $"总计替换: {totalReplaced} 处\n" +
                    $"涉及节点图: {modifiedGraphs.Count} 个\n\n" +
                    $"详细结果:\n" + string.Join("\n", modifiedGraphs);

                Debug.Log($"[BatchPathReplacer] 替换完成: 共替换 {totalReplaced} 处，涉及 {modifiedGraphs.Count} 个节点图");
                EditorUtility.DisplayDialog("替换完成", message, "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BatchPathReplacer] 替换失败: {e.Message}");
                EditorUtility.DisplayDialog("替换失败", $"替换过程中发生错误:\n{e.Message}", "确定");
            }
            finally
            {
                m_IsReplacing = false;
            }
        }
    }
}
#endif
