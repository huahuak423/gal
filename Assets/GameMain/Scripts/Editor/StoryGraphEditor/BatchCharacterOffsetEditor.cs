#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using XNode;
using AVGGame.Editor;

namespace AVGGame.Editor
{
    /// <summary>
    /// 批量修改节点图内立绘进入指令的偏移缩放
    /// </summary>
    public class BatchCharacterOffsetEditor : EditorWindow
    {
        private NodeGraph m_SelectedGraph;
        private string m_SearchSpritePath = "";
        private string m_SearchSpriteNameFilter = "";

        // 匹配结果
        private List<CharacterMatchEntry> m_MatchEntries = new List<CharacterMatchEntry>();
        private Vector2 m_ScrollPosition;

        // 修改参数
        private float m_NewOffsetX = 0f;
        private float m_NewOffsetY = 0f;
        private float m_NewScale = 1f;
        private bool m_ModifyOffsetX = true;
        private bool m_ModifyOffsetY = true;
        private bool m_ModifyScale = true;

        // 匹配条目
        private class CharacterMatchEntry
        {
            public DialogueNode Node;
            public CharacterDisplayData DisplayData;
            public bool IsSelected = true;
            public int DisplayIndex;
        }

        [MenuItem("AVG Tools/批量修改/立绘偏移缩放")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchCharacterOffsetEditor>("立绘偏移批量修改");
            window.minSize = new Vector2(700, 500);
        }

        private void OnGUI()
        {
            GUILayout.Label("━━━ 批量修改立绘进入偏移缩放 ━━━", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "功能说明：\n" +
                "1. 选择节点图\n" +
                "2. 输入立绘路径或文件名进行搜索\n" +
                "3. 在匹配列表中勾选要修改的条目\n" +
                "4. 输入新的偏移缩放值，点击应用",
                MessageType.Info);

            EditorGUILayout.Space();

            // 1. 选择节点图
            GUILayout.Label("1. 选择节点图", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            m_SelectedGraph = (NodeGraph)EditorGUILayout.ObjectField("节点图", m_SelectedGraph, typeof(NodeGraph), false);
            if (EditorGUI.EndChangeCheck())
            {
                m_MatchEntries.Clear();
            }

            EditorGUILayout.Space();

            // 2. 搜索条件
            GUILayout.Label("2. 搜索条件（仅搜索 Enter 类型立绘）", EditorStyles.boldLabel);
            m_SearchSpritePath = EditorGUILayout.TextField("立绘路径（包含匹配）", m_SearchSpritePath);
            m_SearchSpriteNameFilter = EditorGUILayout.TextField("立绘文件名（包含匹配，可选）", m_SearchSpriteNameFilter);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("搜索", GUILayout.Height(30)))
            {
                SearchMatches();
            }
            if (GUILayout.Button("清空结果", GUILayout.Height(30)))
            {
                m_MatchEntries.Clear();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 3. 匹配结果列表
            if (m_MatchEntries.Count > 0)
            {
                GUILayout.Label($"3. 匹配结果 ({m_MatchEntries.Count} 条)", EditorStyles.boldLabel);

                // 全选/取消全选
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("全选"))
                {
                    foreach (var entry in m_MatchEntries)
                        entry.IsSelected = true;
                }
                if (GUILayout.Button("取消全选"))
                {
                    foreach (var entry in m_MatchEntries)
                        entry.IsSelected = false;
                }
                int selectedCount = m_MatchEntries.Count(e => e.IsSelected);
                GUILayout.Label($"已选中: {selectedCount}/{m_MatchEntries.Count}");
                EditorGUILayout.EndHorizontal();

                // 列表
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.Height(250));
                EditorGUILayout.BeginVertical("box");

                // 表头
                EditorGUILayout.BeginHorizontal("box");
                GUILayout.Label("选", GUILayout.Width(30));
                GUILayout.Label("节点名", GUILayout.Width(120));
                GUILayout.Label("角色名", GUILayout.Width(80));
                GUILayout.Label("台词(前20字)", GUILayout.Width(150));
                GUILayout.Label("偏移X", GUILayout.Width(60));
                GUILayout.Label("偏移Y", GUILayout.Width(60));
                GUILayout.Label("缩放", GUILayout.Width(50));
                GUILayout.Label("立绘路径", GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                foreach (var entry in m_MatchEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    entry.IsSelected = EditorGUILayout.Toggle(entry.IsSelected, GUILayout.Width(30));
                    GUILayout.Label(entry.Node.name, GUILayout.Width(120));

                    string charName = entry.Node.SpeakerName;
                    if (string.IsNullOrEmpty(charName)) charName = "-";
                    GUILayout.Label(charName, GUILayout.Width(80));

                    string dialogText = entry.Node.DialogText;
                    if (dialogText.Length > 20) dialogText = dialogText.Substring(0, 20) + "...";
                    GUILayout.Label(dialogText, GUILayout.Width(150));

                    GUILayout.Label(entry.DisplayData.OffsetX.ToString("F1"), GUILayout.Width(60));
                    GUILayout.Label(entry.DisplayData.OffsetY.ToString("F1"), GUILayout.Width(60));
                    GUILayout.Label(entry.DisplayData.Scale.ToString("F2"), GUILayout.Width(50));

                    string spritePath = entry.DisplayData.SpritePath;
                    if (spritePath.Length > 25) spritePath = "..." + spritePath.Substring(spritePath.Length - 22);
                    GUILayout.Label(spritePath, GUILayout.Width(150));

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();

                // 4. 修改参数
                GUILayout.Label("4. 修改参数", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                m_ModifyOffsetX = EditorGUILayout.ToggleLeft("偏移X", m_ModifyOffsetX, GUILayout.Width(80));
                m_ModifyOffsetY = EditorGUILayout.ToggleLeft("偏移Y", m_ModifyOffsetY, GUILayout.Width(80));
                m_ModifyScale = EditorGUILayout.ToggleLeft("缩放", m_ModifyScale, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (m_ModifyOffsetX)
                {
                    m_NewOffsetX = EditorGUILayout.FloatField("新偏移X", m_NewOffsetX);
                }
                if (m_ModifyOffsetY)
                {
                    m_NewOffsetY = EditorGUILayout.FloatField("新偏移Y", m_NewOffsetY);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (m_ModifyScale)
                {
                    m_NewScale = EditorGUILayout.FloatField("新缩放", m_NewScale);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                // 应用按钮
                int selectedCountFinal = m_MatchEntries.Count(e => e.IsSelected);
                GUI.enabled = selectedCountFinal > 0;
                if (GUILayout.Button($"应用到 {selectedCountFinal} 个选中项", GUILayout.Height(40)))
                {
                    ApplyChanges();
                }
                GUI.enabled = true;
            }
            else if (m_SelectedGraph != null)
            {
                EditorGUILayout.HelpBox("点击「搜索」按钮开始搜索匹配的立绘", MessageType.Info);
            }
        }

        private void SearchMatches()
        {
            m_MatchEntries.Clear();

            if (m_SelectedGraph == null)
            {
                EditorUtility.DisplayDialog("提示", "请先选择节点图", "确定");
                return;
            }

            if (string.IsNullOrEmpty(m_SearchSpritePath) && string.IsNullOrEmpty(m_SearchSpriteNameFilter))
            {
                EditorUtility.DisplayDialog("提示", "请输入搜索条件", "确定");
                return;
            }

            foreach (var node in m_SelectedGraph.nodes)
            {
                if (node is DialogueNode dialogueNode)
                {
                    for (int i = 0; i < dialogueNode.CharacterDisplays.Count; i++)
                    {
                        var display = dialogueNode.CharacterDisplays[i];

                        // 只搜索 Enter 类型
                        if (display.ActionType != CharacterActionType.Enter)
                            continue;

                        // 路径匹配
                        bool pathMatch = string.IsNullOrEmpty(m_SearchSpritePath) ||
                                         display.SpritePath.Contains(m_SearchSpritePath);

                        // 文件名匹配
                        bool nameMatch = string.IsNullOrEmpty(m_SearchSpriteNameFilter);
                        if (!nameMatch)
                        {
                            string fileName = System.IO.Path.GetFileNameWithoutExtension(display.SpritePath);
                            nameMatch = fileName.Contains(m_SearchSpriteNameFilter);
                        }

                        if (pathMatch && nameMatch)
                        {
                            m_MatchEntries.Add(new CharacterMatchEntry
                            {
                                Node = dialogueNode,
                                DisplayData = display,
                                DisplayIndex = i,
                                IsSelected = true
                            });
                        }
                    }
                }
            }

            Debug.Log($"[BatchCharacterOffsetEditor] 搜索完成，找到 {m_MatchEntries.Count} 条匹配记录");
        }

        private void ApplyChanges()
        {
            var selectedEntries = m_MatchEntries.Where(e => e.IsSelected).ToList();
            if (selectedEntries.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先勾选要修改的条目", "确定");
                return;
            }

            int modifiedCount = 0;
            HashSet<DialogueNode> modifiedNodes = new HashSet<DialogueNode>();

            foreach (var entry in selectedEntries)
            {
                if (m_ModifyOffsetX)
                    entry.DisplayData.OffsetX = m_NewOffsetX;
                if (m_ModifyOffsetY)
                    entry.DisplayData.OffsetY = m_NewOffsetY;
                if (m_ModifyScale)
                    entry.DisplayData.Scale = m_NewScale;

                modifiedNodes.Add(entry.Node);
                modifiedCount++;
            }

            // 标记节点为脏
            foreach (var node in modifiedNodes)
            {
                EditorUtility.SetDirty(node);
            }

            AssetDatabase.SaveAssets();

            Debug.Log($"[BatchCharacterOffsetEditor] 已修改 {modifiedCount} 条记录，涉及 {modifiedNodes.Count} 个节点");
            EditorUtility.DisplayDialog("修改完成",
                $"已修改 {modifiedCount} 条记录\n涉及 {modifiedNodes.Count} 个节点", "确定");
        }
    }
}
#endif
