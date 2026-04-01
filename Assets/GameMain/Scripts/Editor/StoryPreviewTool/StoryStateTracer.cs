using System.Collections.Generic;
using UnityEngine;
using XNode;
using AVGGame;
using UnityEditor;

namespace AVGGame.Editor
{
    /// <summary>
    /// 剧情溯源大脑：逆向遍历 xNode 图表，收集完整剧情状态
    /// </summary>
    public static class StoryStateTracer
    {
        /// <summary>
        /// 记忆本：记录策划点击过的节点，用于解决分支悖论
        /// </summary>
        private static List<DialogueNode> s_ClickHistory = new List<DialogueNode>();

        /// <summary>
        /// 记录策划点击了哪个节点
        /// </summary>
        public static void RecordClick(DialogueNode node)
        {
            if (node == null) return;

            // 如果这个节点已经在历史里，就把它后面的都删掉（重新选择分支）
            int index = s_ClickHistory.IndexOf(node);
            if (index >= 0)
            {
                s_ClickHistory.RemoveRange(index, s_ClickHistory.Count - index);
            }

            s_ClickHistory.Add(node);
        }

        /// <summary>
        /// 从指定节点逆向爬树，生成状态快照
        /// </summary>
        public static StoryStateSnapshot GetSnapshot(DialogueNode currentNode)
        {
            StoryStateSnapshot snapshot = new StoryStateSnapshot();

            // 从当前节点开始逆向遍历
            DialogueNode walker = currentNode;
            int maxIterations = 500; // 防止死循环
            int iterations = 0;

            while (walker != null && iterations < maxIterations)
            {
                iterations++;

                // 收集当前节点的数据
                CollectNodeData(walker, snapshot);

                // 获取上一个节点
                walker = GetPreviousNode(walker);
            }

            return snapshot;
        }

        /// <summary>
        /// 收集单个节点的数据到快照
        /// </summary>
        private static void CollectNodeData(DialogueNode node, StoryStateSnapshot snapshot)
        {
            if (node == null) return;

            // 收集台词（优先使用最新的）
            if (!string.IsNullOrEmpty(node.DialogText) && string.IsNullOrEmpty(snapshot.DialogText))
            {
                snapshot.DialogText = node.DialogText;
            }

            // 收集角色名
            if (!string.IsNullOrEmpty(node.SpeakerName) && string.IsNullOrEmpty(snapshot.CharacterName))
            {
                snapshot.CharacterName = node.SpeakerName;
            }

            // 收集立绘（只处理左中右三个基础位置）
            if (node.CharacterDisplays != null && node.CharacterDisplays.Count > 0)
            {
                foreach (var display in node.CharacterDisplays)
                {
                    // 忽略 EX1-EX4 扩展位置
                    int posIndex = (int)display.Position;
                    if (posIndex > 2) continue;

                    // 检查是否已经有该位置的立绘
                    bool exists = snapshot.CharacterRoster.Exists(c => c.Position == display.Position);
                    if (!exists)
                    {
                        snapshot.CharacterRoster.Add(display);
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前节点的上一个节点（处理分支选择）
        /// </summary>
        private static DialogueNode GetPreviousNode(DialogueNode node)
        {
            if (node == null) return null;

            // 获取所有连接到当前节点的输入端口
            var inputPort = node.GetInputPort("Input");
            if (inputPort == null) return null;

            var connections = inputPort.GetConnections();
            if (connections == null || connections.Count == 0) return null;

            // 如果只有一个连接，直接返回（无需弹窗）
            if (connections.Count == 1)
            {
                return connections[0].node as DialogueNode;
            }

            // 如果有多个连接（分支汇合），根据点击历史决定走哪条路
            foreach (var conn in connections)
            {
                var prevNode = conn.node as DialogueNode;
                if (prevNode != null && s_ClickHistory.Contains(prevNode))
                {
                    // 从历史记录中找到
                    EditorUtility.DisplayDialog(
                        "分支选择",
                        $"检测到多分支汇合点 [{node.name}]\n\n" +
                        $"✅ 已根据点击历史选择路径: {prevNode.name}",
                        "确定");
                    return prevNode;
                }
            }

            // 如果记忆本里没找到，触发"默认首选法则"
            var defaultNode = connections[0].node as DialogueNode;
            EditorUtility.DisplayDialog(
                "分支选择",
                $"检测到多分支汇合点 [{node.name}]\n\n" +
                $"⚠️ 未找到点击历史记录\n" +
                $"📌 使用默认首选法则: {defaultNode?.name ?? "null"}",
                "确定");
            return defaultNode;
        }

        /// <summary>
        /// 清空点击历史
        /// </summary>
        public static void ClearHistory()
        {
            s_ClickHistory.Clear();
        }
    }
}
