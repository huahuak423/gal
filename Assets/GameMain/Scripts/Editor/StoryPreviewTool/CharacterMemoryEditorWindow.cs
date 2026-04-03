#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using AVGGame;

namespace AVGGame.Editor
{
    /// <summary>
    /// 立绘记忆可视化编辑器窗口
    /// </summary>
    public class CharacterMemoryEditorWindow : EditorWindow
    {
        private Vector2 m_ScrollPosition;
        private string m_SearchFilter = "";
        private string m_SelectedGraphFilter = "";
        private string m_SelectedSpriteFilter = "";

        // 分组折叠状态
        private bool m_GroupByGraph = true;
        private bool m_GroupBySprite = false;

        // 参考复制相关
        private string m_TargetGraphName = "";
        private string m_SourceGraphName = "";
        private string m_SelectedSpriteForCopy = "";

        [MenuItem("AVG Tools/立绘记忆管理")]
        public static void ShowWindow()
        {
            var window = GetWindow<CharacterMemoryEditorWindow>("立绘记忆");
            window.minSize = new Vector2(700, 500);
        }

        private void OnEnable()
        {
            CharacterMemoryManager.Instance.Load();
        }

        private void OnDisable()
        {
            // 保存记忆
            CharacterMemoryManager.Instance.Save();
        }

        private void OnGUI()
        {
            // 标题
            GUILayout.Label("立绘偏移记忆管理器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "功能说明：\n" +
                "• 记录每个节点图中、每个立绘、在每个槽位的偏移和缩放\n" +
                "• 按 Z 退出编辑时自动记录记忆\n" +
                "• 可视化查看、编辑、复制、删除记忆",
                MessageType.Info);

            EditorGUILayout.Space();

            // 工具栏
            DrawToolbar();

            EditorGUILayout.Space();

            // 记忆路径设置
            DrawPathSettings();

            EditorGUILayout.Space();

            // 参考复制功能
            DrawReferenceCopySection();

            EditorGUILayout.Space();

            // 搜索过滤
            GUILayout.Label("━━━ 记忆列表 ━━━", EditorStyles.boldLabel);
            m_SearchFilter = EditorGUILayout.TextField("搜索", m_SearchFilter);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("节点图过滤:", GUILayout.Width(70));
            m_SelectedGraphFilter = EditorGUILayout.TextField(m_SelectedGraphFilter);
            if (GUILayout.Button("清除", GUILayout.Width(50)))
            {
                m_SelectedGraphFilter = "";
            }
            EditorGUILayout.EndHorizontal();

            // 分组选项
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("分组显示:", GUILayout.Width(70));
            m_GroupByGraph = GUILayout.Toggle(m_GroupByGraph, "按节点图");
            m_GroupBySprite = GUILayout.Toggle(m_GroupBySprite, "按立绘");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 记忆列表
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawMemoryList();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 底部操作
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新"))
            {
                CharacterMemoryManager.Instance.Load();
                Repaint();
            }

            if (GUILayout.Button("保存"))
            {
                CharacterMemoryManager.Instance.Save();
                EditorUtility.DisplayDialog("保存成功", "记忆已保存到磁盘", "确定");
            }

            if (GUILayout.Button("清除所有记忆"))
            {
                if (EditorUtility.DisplayDialog("确认清除", "确定要清除所有记忆吗？此操作不可撤销", "确定", "取消"))
                {
                    CharacterMemoryManager.Instance.ClearAll();
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("从节点图导入记忆", GUILayout.Width(140)))
            {
                ImportFromGraph();
            }

            if (GUILayout.Button("导出记忆到CSV", GUILayout.Width(120)))
            {
                ExportToCsv();
            }

            if (GUILayout.Button("从CSV导入", GUILayout.Width(100)))
            {
                ImportFromCsv();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPathSettings()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("记忆文件路径:", GUILayout.Width(90));

            EditorGUI.BeginChangeCheck();
            string newPath = EditorGUILayout.TextField(CharacterMemoryManager.Instance.CurrentSavePath);
            if (EditorGUI.EndChangeCheck())
            {
                CharacterMemoryManager.Instance.CurrentSavePath = newPath;
            }

            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string path = EditorUtility.SaveFilePanel(
                    "设置记忆文件路径",
                    System.IO.Path.GetDirectoryName(CharacterMemoryManager.Instance.CurrentSavePath),
                    System.IO.Path.GetFileName(CharacterMemoryManager.Instance.CurrentSavePath),
                    "json"
                );

                if (!string.IsNullOrEmpty(path))
                {
                    CharacterMemoryManager.Instance.CurrentSavePath = path;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawReferenceCopySection()
        {
            // 参考复制功能
            GUILayout.Label("━━━ 参考复制 ━━━", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "当同一个立绘需要换到另一个节点图时，可以先在源节点图设置好偏移和缩放，\n" +
                "然后在这里复制到目标节点图，省去重新调整的麻烦。",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("源节点图:", GUILayout.Width(70));
            m_SourceGraphName = EditorGUILayout.TextField(m_SourceGraphName);

            GUILayout.Label("目标节点图:", GUILayout.Width(80));
            m_TargetGraphName = EditorGUILayout.TextField(m_TargetGraphName);

            if (GUILayout.Button("复制记忆", GUILayout.Width(80)))
            {
                if (string.IsNullOrEmpty(m_SourceGraphName) || string.IsNullOrEmpty(m_TargetGraphName))
                {
                    EditorUtility.DisplayDialog("错误", "请填写源节点图和目标节点图名称", "确定");
                }
                else
                {
                    CharacterMemoryManager.Instance.CopyMemoriesFrom(m_SourceGraphName, m_TargetGraphName);
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();

            // 获取所有已知的立绘路径用于复制
            var allSprites = new HashSet<string>();
            foreach (var entry in CharacterMemoryManager.Instance.GetAllMemories())
            {
                if (!string.IsNullOrEmpty(entry.SpritePath))
                {
                    allSprites.Add(entry.SpritePath);
                }
            }

            if (allSprites.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("指定立绘复制:", GUILayout.Width(90));

                var spriteList = allSprites.ToList();
                var displayOptions = new List<string> { "<请选择立绘>" };
                displayOptions.AddRange(spriteList.Select(s => System.IO.Path.GetFileNameWithoutExtension(s)));

                int currentIndex = spriteList.IndexOf(m_SelectedSpriteForCopy);
                currentIndex = EditorGUILayout.Popup(currentIndex + 1, displayOptions.ToArray()) - 1;
                if (currentIndex >= 0)
                {
                    m_SelectedSpriteForCopy = spriteList[currentIndex];
                }
                else
                {
                    m_SelectedSpriteForCopy = "";
                }

                if (GUILayout.Button("复制此立绘记忆", GUILayout.Width(100)))
                {
                    if (string.IsNullOrEmpty(m_SelectedSpriteForCopy) || string.IsNullOrEmpty(m_TargetGraphName))
                    {
                        EditorUtility.DisplayDialog("错误", "请选择立绘和目标节点图", "确定");
                    }
                    else
                    {
                        CopySpriteMemoryToGraph(m_SelectedSpriteForCopy, m_TargetGraphName);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawMemoryList()
        {
            var allMemories = CharacterMemoryManager.Instance.GetAllMemories().ToList();

            if (allMemories.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无记忆记录", MessageType.Info);
                return;
            }

            // 过滤
            if (!string.IsNullOrEmpty(m_SearchFilter))
            {
                allMemories = allMemories.Where(m =>
                    (m.GraphName != null && m.GraphName.Contains(m_SearchFilter)) ||
                    (m.SpritePath != null && m.SpritePath.Contains(m_SearchFilter))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(m_SelectedGraphFilter))
            {
                allMemories = allMemories.Where(m =>
                    m.GraphName != null && m.GraphName.Contains(m_SelectedGraphFilter)
                ).ToList();
            }

            if (allMemories.Count == 0)
            {
                EditorGUILayout.HelpBox("没有符合条件的记忆", MessageType.Info);
                return;
            }

            if (m_GroupByGraph)
            {
                // 按节点图分组
                var grouped = allMemories.GroupBy(m => m.GraphName).OrderBy(g => g.Key);
                foreach (var group in grouped)
                {
                    DrawGroupHeader($"{group.Key} ({group.Count()} 条)");
                    foreach (var entry in group.OrderBy(e => e.Position))
                    {
                        DrawMemoryEntry(entry);
                    }
                }
            }
            else if (m_GroupBySprite)
            {
                // 按立绘分组
                var grouped = allMemories.GroupBy(m => m.SpritePath).OrderBy(g => g.Key);
                foreach (var group in grouped)
                {
                    DrawGroupHeader($"{System.IO.Path.GetFileNameWithoutExtension(group.Key)} ({group.Count()} 条)");
                    foreach (var entry in group.OrderBy(e => e.GraphName))
                    {
                        DrawMemoryEntry(entry);
                    }
                }
            }
            else
            {
                // 扁平列表
                foreach (var entry in allMemories.OrderBy(m => m.GraphName).ThenBy(m => m.SpritePath))
                {
                    DrawMemoryEntry(entry);
                }
            }
        }

        private void DrawGroupHeader(string title)
        {
            EditorGUILayout.BeginHorizontal("Toolbar");
            GUILayout.Label(title, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMemoryEntry(CharacterMemoryEntry entry)
        {
            EditorGUILayout.BeginHorizontal("box");

            // 槽位标签
            string posName = entry.Position.ToString();
            Color oldColor = GUI.backgroundColor;
            switch (entry.Position)
            {
                case CharacterPosition.Left:
                    GUI.backgroundColor = new Color(1f, 0.3f, 0.4f);
                    break;
                case CharacterPosition.Center:
                    GUI.backgroundColor = new Color(0.3f, 1f, 0.4f);
                    break;
                case CharacterPosition.Right:
                    GUI.backgroundColor = new Color(0.3f, 0.3f, 1f);
                    break;
            }
            GUILayout.Label(posName, GUILayout.Width(60));
            GUI.backgroundColor = oldColor;

            // 节点图
            GUILayout.Label($"[{entry.GraphName}]", GUILayout.Width(120));

            // 立绘名称
            string spriteName = System.IO.Path.GetFileNameWithoutExtension(entry.SpritePath);
            GUILayout.Label(spriteName, GUILayout.Width(100));

            // 偏移
            EditorGUI.BeginChangeCheck();
            float newOffsetX = EditorGUILayout.FloatField(entry.OffsetX, GUILayout.Width(60));
            float newOffsetY = EditorGUILayout.FloatField(entry.OffsetY, GUILayout.Width(60));
            float newScale = EditorGUILayout.FloatField(entry.Scale, GUILayout.Width(50));

            if (EditorGUI.EndChangeCheck())
            {
                entry.OffsetX = newOffsetX;
                entry.OffsetY = newOffsetY;
                entry.Scale = newScale;
            }

            GUILayout.FlexibleSpace();

            // 删除按钮
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                CharacterMemoryManager.Instance.RemoveMemory(entry.GraphName, entry.SpritePath, entry.Position);
                Repaint();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ImportFromGraph()
        {
            string path = EditorUtility.OpenFilePanel("选择节点图资产", "Assets", "asset");
            if (string.IsNullOrEmpty(path)) return;

            // 从路径获取资产
            string assetPath = path.Replace(Application.dataPath, "Assets");
            var graph = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

            if (graph == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载节点图资产", "确定");
                return;
            }

            // 遍历节点获取记忆
            // 这里需要根据实际情况实现
            EditorUtility.DisplayDialog("导入完成", "已从节点图导入记忆（功能开发中）", "确定");
        }

        private void ExportToCsv()
        {
            string path = EditorUtility.SaveFilePanel("导出CSV", "", "CharacterMemory", "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("节点图,立绘路径,槽位,偏移X,偏移Y,缩放,最后节点GUID");

                foreach (var entry in CharacterMemoryManager.Instance.GetAllMemories())
                {
                    sb.AppendLine($"\"{entry.GraphName}\",\"{entry.SpritePath}\",{(int)entry.Position},{entry.OffsetX},{entry.OffsetY},{entry.Scale},\"{entry.LastNodeGuid}\"");
                }

                System.IO.File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
                EditorUtility.DisplayDialog("导出成功", $"CSV 已导出到:\n{path}", "确定");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("导出失败", e.Message, "确定");
            }
        }

        private void ImportFromCsv()
        {
            string path = EditorUtility.OpenFilePanel("导入CSV", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
                int imported = 0;

                for (int i = 1; i < lines.Length; i++) // 跳过表头
                {
                    var fields = ParseCsvLine(lines[i]);
                    if (fields.Length < 7) continue;

                    var entry = new CharacterMemoryEntry
                    {
                        GraphName = fields[0],
                        SpritePath = fields[1],
                        Position = (CharacterPosition)int.Parse(fields[2]),
                        OffsetX = float.Parse(fields[3]),
                        OffsetY = float.Parse(fields[4]),
                        Scale = float.Parse(fields[5]),
                        LastNodeGuid = fields[6]
                    };

                    string key = entry.GetKey();
                    CharacterMemoryManager.Instance.RecordMemory(
                        entry.GraphName, entry.SpritePath, entry.Position,
                        entry.OffsetX, entry.OffsetY, entry.Scale, entry.LastNodeGuid
                    );
                    imported++;
                }

                CharacterMemoryManager.Instance.Save();
                EditorUtility.DisplayDialog("导入成功", $"成功导入 {imported} 条记忆", "确定");
                Repaint();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("导入失败", e.Message, "确定");
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());

            return result.ToArray();
        }

        private void CopySpriteMemoryToGraph(string spritePath, string targetGraphName)
        {
            int count = 0;
            foreach (var entry in CharacterMemoryManager.Instance.GetAllMemories())
            {
                if (entry.SpritePath == spritePath)
                {
                    CharacterMemoryManager.Instance.RecordMemory(
                        targetGraphName,
                        entry.SpritePath,
                        entry.Position,
                        entry.OffsetX,
                        entry.OffsetY,
                        entry.Scale,
                        ""
                    );
                    count++;
                }
            }

            CharacterMemoryManager.Instance.Save();
            EditorUtility.DisplayDialog("复制完成", $"已将 '{System.IO.Path.GetFileNameWithoutExtension(spritePath)}' 的 {count} 条记忆复制到 '{targetGraphName}'", "确定");
            Repaint();
        }
    }
}
#endif
