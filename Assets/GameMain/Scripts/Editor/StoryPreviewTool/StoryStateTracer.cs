using System.Collections.Generic;
using UnityEngine;
using XNode;
using AVGGame;
using UnityEditor;

namespace AVGGame.Editor
{
    /// <summary>
    /// 剧情溯源引擎：正向遍历 xNode 图表，从起始节点遍历到目标节点，收集完整剧情状态
    /// </summary>
    public static class StoryStateTracer
    {
        /// <summary>
        /// 记忆本：记录策划点击过的节点，用于解决分支选择
        /// </summary>
        private static List<DialogueNode> s_ClickHistory = new List<DialogueNode>();

        /// <summary>
        /// 记录策划点击了哪个节点
        /// </summary>
        public static void RecordClick(DialogueNode node)
        {
            if (node == null) return;

            // 如果这个节点已经在历史里，把它后面的都删掉（重新选择分支）
            int index = s_ClickHistory.IndexOf(node);
            if (index >= 0)
            {
                s_ClickHistory.RemoveRange(index, s_ClickHistory.Count - index);
            }

            s_ClickHistory.Add(node);
        }

        /// <summary>
        /// 从起始节点正向遍历到目标节点，生成状态快照
        /// </summary>
        public static StoryStateSnapshot GetSnapshot(DialogueNode targetNode)
        {
            StoryStateSnapshot snapshot = new StoryStateSnapshot();

            if (targetNode == null) return snapshot;

            // 清空立绘追踪状态
            s_LastActionNodeDict.Clear();

            // 找到图表中的起始节点（没有输入连接的节点）
            Node startNode = FindStartNode(targetNode);
            if (startNode == null)
            {
                Debug.LogWarning("[StoryStateTracer] 未找到起始节点");
                return snapshot;
            }

            // 从起始节点正向遍历到目标节点
            TraverseForward(startNode, targetNode, snapshot);

            return snapshot;
        }

        /// <summary>
        /// 找到图表的起始节点（没有输入连接的 DialogueNode 或 ChoiceNode）
        /// </summary>
        private static Node FindStartNode(DialogueNode anyNode)
        {
            if (anyNode == null) return null;

            var graph = anyNode.graph;
            if (graph == null) return null;

            // 遍历图表中所有节点，找到没有输入连接的节点
            foreach (var node in graph.nodes)
            {
                // 只处理对话节点和选择节点
                if (!(node is DialogueNode) && !(node is ChoiceNode)) continue;

                var inputPort = node.GetInputPort("Entry");
                if (inputPort == null) continue;

                var connections = inputPort.GetConnections();
                if (connections == null || connections.Count == 0)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// 从起始节点正向遍历到目标节点，沿途收集数据
        /// </summary>
        private static void TraverseForward(Node startNode, DialogueNode targetNode, StoryStateSnapshot snapshot)
        {
            HashSet<Node> visited = new HashSet<Node>();
            Node currentNode = startNode;
            int maxIterations = 1000;
            int iterations = 0;

            while (currentNode != null && iterations < maxIterations)
            {
                iterations++;

                // 检查是否为目标节点
                bool isTarget = (currentNode == targetNode);

                // 只从对话节点收集数据
                if (currentNode is DialogueNode dialogueNode)
                {
                    CollectNodeDataForward(dialogueNode, snapshot, isTarget);
                }

                // 到达目标节点，停止遍历
                if (isTarget)
                {
                    break;
                }

                // 标记已访问
                visited.Add(currentNode);

                // 获取下一个节点
                currentNode = GetNextNode(currentNode, visited);
            }
        }

        /// <summary>
        /// 记录每个位置最后一次操作的节点（用于错误提示）
        /// </summary>
        private static Dictionary<CharacterPosition, DialogueNode> s_LastActionNodeDict = new Dictionary<CharacterPosition, DialogueNode>();

        /// <summary>
        /// 收集单个节点的数据（正向遍历模式）
        /// </summary>
        private static void CollectNodeDataForward(DialogueNode node, StoryStateSnapshot snapshot, bool isTargetNode = false)
        {
            if (node == null) return;

            // 收集背景音乐（覆盖式，最新的生效）
            if (!string.IsNullOrEmpty(node.BgmPath))
            {
                snapshot.BgmPath = node.BgmPath;
            }

            // 收集背景图（覆盖式，最新的生效）
            if (!string.IsNullOrEmpty(node.BackgroundPath))
            {
                snapshot.BackgroundPath = node.BackgroundPath;
            }

            // 收集立绘（指令式处理）
            if (node.CharacterDisplays != null && node.CharacterDisplays.Count > 0)
            {
                foreach (var display in node.CharacterDisplays)
                {
                    // 忽略 EX1-EX4 扩展位置
                    int posIndex = (int)display.Position;
                    if (posIndex > 2) continue;

                    ProcessCharacterAction(node, display, snapshot);
                }
            }

            // 只有目标节点才收集台词和角色名
            if (isTargetNode)
            {
                snapshot.DialogText = node.DialogText ?? "";
                snapshot.CharacterName = node.SpeakerName ?? "";
            }
        }

        /// <summary>
        /// 处理单条立绘指令
        /// </summary>
        private static void ProcessCharacterAction(DialogueNode currentNode, CharacterDisplayData display, StoryStateSnapshot snapshot)
        {
            CharacterPosition position = display.Position;
            bool hasCharacterAtPosition = snapshot.CharacterRoster.Exists(c => c.Position == position);

            switch (display.ActionType)
            {
                case CharacterActionType.Enter:
                    // 加入指令：位置上不能已有立绘
                    if (hasCharacterAtPosition)
                    {
                        DialogueNode lastNode = null;
                        s_LastActionNodeDict.TryGetValue(position, out lastNode);

                        string lastDialog = (lastNode != null) ? (lastNode.DialogText ?? "(无台词)") : "(未知)";
                        string currentDialog = currentNode.DialogText ?? "(无台词)";

                        EditorUtility.DisplayDialog(
                            "⚠️ 立绘指令逻辑错误",
                            $"检测到连续对立绘位置 [{position}] 执行 [加入] 操作！\n\n" +
                            $"位置 [{position}] 上已有立绘，不能再次加入。\n\n" +
                            $"━━━ 上次操作的节点 ━━━\n" +
                            $"台词: \"{lastDialog}\"\n\n" +
                            $"━━━ 当前节点 ━━━\n" +
                            $"台词: \"{currentDialog}\"\n\n" +
                            $"请检查这两个节点的立绘指令设置。",
                            "确定");
                    }
                    else
                    {
                        snapshot.CharacterRoster.Add(display);
                        s_LastActionNodeDict[position] = currentNode;
                    }
                    break;

                case CharacterActionType.Leave:
                    // 离开指令：位置上必须有立绘
                    if (!hasCharacterAtPosition)
                    {
                        DialogueNode lastNode = null;
                        s_LastActionNodeDict.TryGetValue(position, out lastNode);

                        string lastDialog = (lastNode != null) ? (lastNode.DialogText ?? "(无台词)") : "(无历史记录)";
                        string currentDialog = currentNode.DialogText ?? "(无台词)";

                        EditorUtility.DisplayDialog(
                            "⚠️ 立绘指令逻辑错误",
                            $"检测到连续对立绘位置 [{position}] 执行 [离开] 操作！\n\n" +
                            $"位置 [{position}] 上没有立绘，不能执行离开。\n\n" +
                            $"━━━ 上次操作的节点 ━━━\n" +
                            $"台词: \"{lastDialog}\"\n\n" +
                            $"━━━ 当前节点 ━━━\n" +
                            $"台词: \"{currentDialog}\"\n\n" +
                            $"请检查这两个节点的立绘指令设置。",
                            "确定");
                    }
                    else
                    {
                        snapshot.CharacterRoster.RemoveAll(c => c.Position == position);
                        s_LastActionNodeDict[position] = currentNode;
                    }
                    break;

                case CharacterActionType.ChangeSprite:
                    // 修改指令：位置上必须有立绘
                    if (!hasCharacterAtPosition)
                    {
                        DialogueNode lastNode = null;
                        s_LastActionNodeDict.TryGetValue(position, out lastNode);

                        string lastDialog = (lastNode != null) ? (lastNode.DialogText ?? "(无台词)") : "(无历史记录)";
                        string currentDialog = currentNode.DialogText ?? "(无台词)";

                        EditorUtility.DisplayDialog(
                            "⚠️ 立绘指令逻辑错误",
                            $"检测到对立绘位置 [{position}] 执行 [修改] 操作，但该位置没有立绘！\n\n" +
                            $"━━━ 上次操作的节点 ━━━\n" +
                            $"台词: \"{lastDialog}\"\n\n" +
                            $"━━━ 当前节点 ━━━\n" +
                            $"台词: \"{currentDialog}\"\n\n" +
                            $"请检查这两个节点的立绘指令设置。",
                            "确定");
                    }
                    else
                    {
                        snapshot.CharacterRoster.RemoveAll(c => c.Position == position);
                        snapshot.CharacterRoster.Add(display);
                        s_LastActionNodeDict[position] = currentNode;
                    }
                    break;
            }
        }

        /// <summary>
        /// 获取当前节点的下一个节点
        /// </summary>
        private static Node GetNextNode(Node currentNode, HashSet<Node> visited)
        {
            if (currentNode == null) return null;

            // 检查节点类型
            if (currentNode is DialogueNode dialogueNode)
            {
                // 对话节点：一对一，直接走唯一输出
                return GetNextFromDialogueNode(dialogueNode, visited);
            }
            else if (currentNode is ChoiceNode choiceNode)
            {
                // 选择节点：一对多，需要分支选择
                return GetNextFromChoiceNode(choiceNode, visited);
            }

            return null;
        }

        /// <summary>
        /// 从对话节点获取下一个节点（一对一）
        /// </summary>
        private static Node GetNextFromDialogueNode(DialogueNode node, HashSet<Node> visited)
        {
            var outputPort = node.GetOutputPort("Exit");
            if (outputPort == null) return null;

            var connections = outputPort.GetConnections();
            if (connections == null || connections.Count == 0) return null;

            // 对话节点只有一个输出
            var nextNode = connections[0].node;
            if (visited.Contains(nextNode))
            {
                return null; // 防止循环
            }
            return nextNode;
        }

        /// <summary>
        /// 从选择节点获取下一个节点（一对多，需要分支选择）
        /// </summary>
        private static Node GetNextFromChoiceNode(ChoiceNode choiceNode, HashSet<Node> visited)
        {
            // 获取所有动态选项端口
            var ports = new List<NodePort>();
            int index = 0;
            while (true)
            {
                var port = choiceNode.GetOutputPort($"Choices {index}");
                if (port == null) break;
                ports.Add(port);
                index++;
            }

            if (ports.Count == 0) return null;

            // 如果只有一个选项，直接返回
            if (ports.Count == 1)
            {
                var connections = ports[0].GetConnections();
                if (connections == null || connections.Count == 0) return null;
                var nextNode = connections[0].node;
                if (visited.Contains(nextNode)) return null;
                return nextNode;
            }

            // 多个选项，需要分支选择
            int selectedIndex = SelectBranchIndex(choiceNode, ports, visited);

            var selectedConnections = ports[selectedIndex].GetConnections();
            if (selectedConnections == null || selectedConnections.Count == 0) return null;

            return selectedConnections[0].node;
        }

        /// <summary>
        /// 选择分支：根据历史记录决定走哪条分支
        /// </summary>
        private static int SelectBranchIndex(ChoiceNode choiceNode, List<NodePort> ports, HashSet<Node> visited)
        {
            // 尝试从历史记录中找到最近的节点，确定它在哪个分支上
            for (int i = s_ClickHistory.Count - 1; i >= 0; i--)
            {
                DialogueNode historyNode = s_ClickHistory[i];

                // 检查这个历史节点在哪个分支路径上
                for (int branchIndex = 0; branchIndex < ports.Count; branchIndex++)
                {
                    var connections = ports[branchIndex].GetConnections();
                    if (connections == null || connections.Count == 0) continue;

                    var branchStartNode = connections[0].node;
                    if (branchStartNode == null) continue;

                    // 检查历史节点是否在这个分支的下游
                    if (IsNodeInBranchPath(branchStartNode, historyNode, visited))
                    {
                        // 获取选项文本
                        string choiceText = "";
                        if (choiceNode.Choices != null && branchIndex < choiceNode.Choices.Count)
                        {
                            choiceText = choiceNode.Choices[branchIndex].ChoiceText ?? "";
                        }

                        Debug.Log($"[StoryStateTracer] 分支选择: 根据历史记录 → 分支 {branchIndex + 1}\n" +
                            $"   选择节点: {choiceNode.name}\n" +
                            $"   选项: \"{choiceText}\"\n" +
                            $"   目标节点: {branchStartNode.name}");
                        return branchIndex;
                    }
                }
            }

            // 没有历史记录，使用默认首选法则（第一条分支）
            var defaultConnections = ports[0].GetConnections();
            var defaultNode = (defaultConnections != null && defaultConnections.Count > 0) ? defaultConnections[0].node : null;

            string defaultChoiceText = "";
            if (choiceNode.Choices != null && choiceNode.Choices.Count > 0)
            {
                defaultChoiceText = choiceNode.Choices[0].ChoiceText ?? "";
            }

            Debug.Log($"[StoryStateTracer] 分支选择: 未找到历史记录，使用默认首选法则 → 分支 1\n" +
                $"   选择节点: {choiceNode.name}\n" +
                $"   选项: \"{defaultChoiceText}\"\n" +
                $"   目标节点: {defaultNode?.name ?? "null"}");
            return 0;
        }

        /// <summary>
        /// 检查目标节点是否在从分支起点开始的路径上
        /// </summary>
        private static bool IsNodeInBranchPath(Node branchStart, DialogueNode targetNode, HashSet<Node> globalVisited)
        {
            if (branchStart == null || targetNode == null) return false;
            if (branchStart == targetNode) return true;

            HashSet<Node> localVisited = new HashSet<Node>();
            Queue<Node> queue = new Queue<Node>();

            queue.Enqueue(branchStart);
            localVisited.Add(branchStart);

            int maxIterations = 500;
            int iterations = 0;

            while (queue.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                var current = queue.Dequeue();

                if (current == targetNode)
                {
                    return true;
                }

                // 获取当前节点的输出
                NodePort outputPort = null;
                if (current is DialogueNode dialogueNode)
                {
                    outputPort = dialogueNode.GetOutputPort("Exit");
                }
                else if (current is ChoiceNode choiceNode)
                {
                    // 选择节点：检查所有分支
                    int idx = 0;
                    while (true)
                    {
                        var port = choiceNode.GetOutputPort($"Choices {idx}");
                        if (port == null) break;

                        var connections = port.GetConnections();
                        if (connections != null)
                        {
                            foreach (var conn in connections)
                            {
                                var nextNode = conn.node;
                                if (nextNode != null && !localVisited.Contains(nextNode) && !globalVisited.Contains(nextNode))
                                {
                                    localVisited.Add(nextNode);
                                    queue.Enqueue(nextNode);
                                }
                            }
                        }
                        idx++;
                    }
                    continue;
                }

                if (outputPort == null) continue;

                var outputConnections = outputPort.GetConnections();
                if (outputConnections == null) continue;

                foreach (var conn in outputConnections)
                {
                    var nextNode = conn.node;
                    if (nextNode != null && !localVisited.Contains(nextNode) && !globalVisited.Contains(nextNode))
                    {
                        localVisited.Add(nextNode);
                        queue.Enqueue(nextNode);
                    }
                }
            }

            return false;
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
