//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using System.Collections.Generic;
using GameFramework.DataTable;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    /// <summary>
    /// 游戏主流程 - AVG 游戏的核心流程，处理对话、剧情等
    /// </summary>
    public class ProcedureGame : ProcedureBase
    {
        #region 字段
        // 唯一的子页面记录器：大地图、剧情对话，谁打开就记录谁
        private int m_CurrentSubFormId = -1;

        // 游戏核心数据
        private int m_CurrentAP = 10;

        // 流程拥有者引用（用于状态切换）
        private IFsm<IProcedureManager> m_ProcedureOwner;

        // 加载剧本类
        private StoryGraphLoader m_StoryGraphLoader;

        // 对话状态管理
        private int m_CurrentDialogueId = 0;

        // 当前交互的 NPC ID（用于情报界面）
        private int m_CurrentNpcId = 0;

        // 序章标记（新游戏时先播序章再开地图）
        private bool m_PlayPrologue = false;

        // 标记：在 OnUpdate 中打开大地图（避免在回调中直接调用导致时序问题）
        private bool m_PendingOpenMap = false;

        // 存档加载标记（EventPool加载完成后用于判断是否断点续传）
        private bool m_IsLoadingFromSave = false;

        // 当前是否为重玩事件（影响快进行为）
        public bool IsReplayEvent { get; private set; }

        
        #endregion

        #region 生命周期
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_ProcedureOwner = procedureOwner;
            Debug.Log("[ProcedureGame] 正式进入游戏核心流程！");

            // 判断是否为存档加载（设置标记，供 OnLoadDataTableSuccess 使用）
            if (SaveLoadContext.IsLoadingFromSave)
            {
                Debug.Log("[ProcedureGame] 检测到存档加载，等待事件表加载后恢复进度");
                m_IsLoadingFromSave = true;
                m_PendingOpenMap = false;
            }
            else
            {
                Debug.Log("[ProcedureGame] 新游戏流程，重置玩家数据并等待事件表加载后直接进入大地图");
                // 保护玩家取名：ResetGame会清空名字，但名字已在CreateName中设置
                string savedName = CustomEntry.PlayerData.PlayerName;
                CustomEntry.PlayerData.ResetGame();
                CustomEntry.PlayerData.SetPlayerName(savedName);
                Debug.Log($"[ProcedureGame] 保留玩家取名: '{savedName}'");
                m_PlayPrologue = false; // 序章已暂时剔除，直接进大地图
                m_PendingOpenMap = false;
            }

            //读取事件表成功（失败）回调
            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);

            //剧本加载类成功（失败）回调
            if (m_StoryGraphLoader == null)
            {
                m_StoryGraphLoader = new StoryGraphLoader();
            }
            m_StoryGraphLoader.InitListener(this);

            //检查内存里是不是已经有这本账本了
            if (GameEntry.DataTable.HasDataTable<EventRowData>())
            {
                // 如果有，为了防止重复装填，我们可以先把它销毁
                GameEntry.DataTable.DestroyDataTable<EventRowData>();
            }

            // 在这里加载 EventPool数据表（加载完会执行回调函数）
            IDataTable<EventRowData> emptyTable = GameEntry.DataTable.CreateDataTable<EventRowData>();
            DataTableBase tableBase = (DataTableBase)emptyTable;
            string realPath = "Assets/GameMain/DataTables/EventPool.txt";
            tableBase.ReadData(realPath, 0, this);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            // 关闭当前打开的子界面（如果有）
            if (m_CurrentSubFormId != -1)
            {
                GameEntry.UI.CloseUIForm(m_CurrentSubFormId);
                m_CurrentSubFormId = -1;
            }

            // 检查 GameEntry 是否还有效（游戏关闭时可能已被销毁）
            if (GameEntry.IsInitialized && GameEntry.Event != null)
            {
                GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
                GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            }

            if (m_StoryGraphLoader != null)
            {
                m_StoryGraphLoader.RemoveListener();
                m_StoryGraphLoader = null;
            }

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 在 OnUpdate 中打开大地图，避免在回调中直接调用导致状态机时序问题
            if (m_PendingOpenMap)
            {
                m_PendingOpenMap = false;
                OpenMap();
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 根据故事名称获取事件ID
        /// </summary>
        private int GetEventIdFromStoryName(string storyName)
        {
            if (string.IsNullOrEmpty(storyName))
                return 0;

            try
            {
                if (GameEntry.DataTable.HasDataTable<EventRowData>())
                {
                    var eventTable = GameEntry.DataTable.GetDataTable<EventRowData>();
                    foreach (var eventData in eventTable)
                    {
                        if (eventData.Title == storyName || eventData.TargetGraphName == storyName)
                        {
                            return eventData.Id;
                        }
                    }
                }
                Debug.LogWarning($"[ProcedureGame] 未找到匹配的故事: {storyName}");
                return 0;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProcedureGame] 根据故事名称查找事件ID失败: {e.Message}");
                return 0;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取当前交互的 NPC ID
        /// </summary>

        /// <summary>
        /// 功能1：判断哪些事件应该显示在选项框上 (可见条件筛选)
        /// </summary>
        public List<EventRowData> GetVisibleEventsInMap(int mapId)
        {
            List<EventRowData> result = new List<EventRowData>();

            // 获取该地图所有的原始事件 (从 DataTable 获取或从你的字典获取)
            List<EventRowData> allEventsInMap = GetRawEventsByMapId(mapId);

            Debug.Log($"[ProcedureGame] GetVisibleEventsInMap - 地图{mapId}，原始事件数量: {allEventsInMap.Count}");

            foreach (var evt in allEventsInMap)
            {
                Debug.Log($"[ProcedureGame] 检查事件: ID={evt.Id}, Title={evt.Title}, EventType={evt.EventType}, VisibleConditions='{evt.VisibleConditions}'");

                // EventType == 1 是地图入口，始终显示
                // EventType == 2 是角色事件，完成后需要隐藏
                if (evt.EventType == 2 && CustomEntry.PlayerData.HasCompletedEvent(evt.Id))
                {
                    Debug.Log($"[ProcedureGame] 跳过已完成的事件: {evt.Id} - {evt.Title}");
                    continue;
                }

                // 使用刚刚写好的工具检查 VisibleConditions 列
                bool conditionMet = ConditionChecker.CheckCondition(evt.VisibleConditions, CustomEntry.PlayerData);
                Debug.Log($"[ProcedureGame] 条件检查结果: {conditionMet}");

                if (conditionMet)
                {
                    result.Add(evt);
                    Debug.Log($"[ProcedureGame] 添加到结果: {evt.Title}");
                }
            }

            Debug.Log($"[ProcedureGame] GetVisibleEventsInMap - 地图{mapId}，可见事件数量: {result.Count}");
            return result;
        }

        /// <summary>
        /// 从底层的全量数据表中，实时筛选出属于某个地图的所有事件（不含任何条件过滤）
        /// </summary>
        public List<EventRowData> GetRawEventsByMapId(int mapId)
        {
            List<EventRowData> result = new List<EventRowData>();

            // 1. 向 UGF 大管家伸手，拿到内存中已经加载好的事件表
            IDataTable<EventRowData> dtEvent = GameEntry.DataTable.GetDataTable<EventRowData>();

            if (dtEvent == null)
            {
                Debug.LogWarning($"[ProcedureGame] 找不到 EventRowData 数据表！");
                return result;
            }

            Debug.Log($"[ProcedureGame] GetRawEventsByMapId - 数据表存在，开始遍历，目标地图ID: {mapId}");

            // 2. 实时遍历整张表
            int totalCount = 0;
            foreach (EventRowData row in dtEvent)
            {
                totalCount++;
                Debug.Log($"[ProcedureGame] 遍历事件 - ID: {row.Id}, MapId: {row.MapId}, Title: {row.Title}");

                // 只要所在地图匹配，就把它加进待选列表
                if (row.MapId == mapId)
                {
                    result.Add(row);
                    Debug.Log($"[ProcedureGame] 匹配地图 {mapId}，添加事件: {row.Title}");
                }
            }

            Debug.Log($"[ProcedureGame] GetRawEventsByMapId - 总事件数: {totalCount}, 匹配事件数: {result.Count}");
            return result;
        }

        /// <summary>
        /// 获取女主当前数值（供 InformationPanel 显示）
        /// </summary>
        public PlayerStatsData GetPlayerStats()
        {
            var playerData = CustomEntry.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[ProcedureGame] PlayerData is null");
                return null;
            }

            return new PlayerStatsData
            {
                ActionPoints = playerData.CurrentActionPoints,
                MaxActionPoints = playerData.MaxActionPoints,
                CurrentRound = playerData.CurrentRound
            };
        }

        /// <summary>
        /// 获取当前交互的 NPC ID
        /// </summary>
        public int GetCurrentNpcId()
        {
            return m_CurrentNpcId;
        }

        /// <summary>
        /// 设置当前交互的 NPC ID
        /// </summary>
        public void SetCurrentNpcId(int npcId)
        {
            m_CurrentNpcId = npcId;
            Debug.Log($"[ProcedureGame] SetCurrentNpcId: {npcId}");
        }

        /// <summary>
        /// 获取指定 NPC 的已完成事件列表（按事件号排序）
        /// </summary>
        public List<EventRowData> GetCompletedEventsByNpcId(int npcId)
        {
            List<EventRowData> result = new List<EventRowData>();
            IDataTable<EventRowData> dtEvent = GameEntry.DataTable.GetDataTable<EventRowData>();
            if (dtEvent == null) return result;

            foreach (EventRowData evt in dtEvent)
            {
                if (evt.EventType != 2) continue;
                if (string.IsNullOrEmpty(evt.EventNum)) continue;

                string[] parts = evt.EventNum.Split('_');
                if (parts.Length != 2) continue;
                if (!int.TryParse(parts[0], out int evtNpcId) || evtNpcId != npcId) continue;
                if (!int.TryParse(parts[1], out int eventId)) continue;

                if (CustomEntry.PlayerData.HasCompletedNpcEvent(npcId, eventId))
                {
                    result.Add(evt);
                }
            }

            // 按事件号排序
            result.Sort((a, b) =>
            {
                string[] aParts = a.EventNum.Split('_');
                string[] bParts = b.EventNum.Split('_');
                int aSeq = aParts.Length == 2 && int.TryParse(aParts[1], out int aVal) ? aVal : 0;
                int bSeq = bParts.Length == 2 && int.TryParse(bParts[1], out int bVal) ? bVal : 0;
                return aSeq.CompareTo(bSeq);
            });

            return result;
        }

        /// <summary>
        /// 判断某个显示出来的事件，按钮是否亮起可点 (点击条件筛选)
        /// </summary>
        public bool IsEventPlayable(EventRowData evt)
        {
            // 如果连体力都不够，直接不可玩
            if (CustomEntry.PlayerData.CurrentActionPoints < evt.CostAP)
            {
                return false;
            }

            // 使用工具检查 PlayableConditions 列
            return ConditionChecker.CheckCondition(evt.PlayableConditions, CustomEntry.PlayerData);
        }

        /// <summary>
        /// 打开大地图 (被 OnEnter 或 剧情结束时调用)
        /// </summary>
        public void OpenMap()
        {
            // Debug.Log("[ProcedureGame] 切换到大地图界面");
            SwitchSubForm(AssetUtility.GetUIFormAsset(UIFormId.Map), this);
        }

        /// <summary>
        /// 打开小菜单
        /// </summary>
        public void OpenMenu()
        {
            // Debug.Log("[ProcedureGame] 打开小菜单界面");
            GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset(UIFormId.Menu), "Popup", this);
        }

        /// <summary>
        /// 打开情报界面
        /// </summary>
        public void OpenInformation()
        {
            GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset(UIFormId.Information), "Popup", this);
        }

        /// <summary>
        /// 打开背包界面
        /// </summary>
        public void OpenInventory()
        {
            GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset(UIFormId.Inventory), "Popup", this);
        }

        /// <summary>
        /// 加载剧情
        /// </summary>
        public void LoadStory(int eventId, bool isResume = false)
        {
            EventRowData currentData = GameEntry.DataTable.GetDataTable<EventRowData>().GetDataRow(eventId);
            Debug.Log($"[ProcedureGame] LoadStory 被调用, eventId: {eventId}, isResume: {isResume}");
            Debug.Log($"[ProcedureGame] TargetGraphName: {currentData.TargetGraphName}");

            // 记录当前进行中的事件ID（用于存档断点续传）
            CustomEntry.PlayerData.SetCurrentEventId(eventId);

            // 非断点续传时才消耗AP（续传时AP已在首次进入时扣过）
            if (!isResume)
            {
                // 消耗行动点
                if (currentData.CostAP > 0)
                {
                    if (!CustomEntry.PlayerData.ConsumeActionPoints(currentData.CostAP))
                    {
                        Debug.LogWarning($"[ProcedureGame] AP 不足，无法开始事件: {eventId}，需要 {currentData.CostAP}，当前 {CustomEntry.PlayerData.CurrentActionPoints}");
                        CustomEntry.PlayerData.SetCurrentEventId(0);
                        return;
                    }
                    Debug.Log($"[ProcedureGame] 消耗 AP: {currentData.CostAP}，剩余: {CustomEntry.PlayerData.CurrentActionPoints}");
                }
            }

            // EventType == 2 是角色特殊事件
            if (currentData.EventType == 2)
            {
                // 提取 NPC ID 并设置为当前 NPC
                if (!string.IsNullOrEmpty(currentData.EventNum))
                {
                    string[] parts = currentData.EventNum.Split('_');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int npcId))
                    {
                        m_CurrentNpcId = npcId;
                    }
                }
            }

            //异步加载目标故事剧本
            m_StoryGraphLoader.LoadGraph(currentData.TargetGraphName);
        }

        /// <summary>
        /// 进入剧情（由 StoryGraphLoader 回调）
        /// </summary>
        public void StartStory(string graphName)
        {
            Debug.Log($"[ProcedureGame] === StartStory 被调用 ===");
            Debug.Log($"[ProcedureGame] graphName: {graphName}");

            // 设置当前剧情图名
            CustomEntry.PlayerData.SetCurrentStoryGarphName(graphName);

            // 断点续传：如果存档中有对话进度，从断点恢复；否则从头开始
            int savedDialogueId = CustomEntry.PlayerData.CurrentDialogueId;
            if (savedDialogueId > 0)
            {
                m_CurrentDialogueId = savedDialogueId;
                Debug.Log($"[ProcedureGame] 断点续传，从对话ID {m_CurrentDialogueId} 恢复");
            }
            else
            {
                m_CurrentDialogueId = 10000;
                Debug.Log($"[ProcedureGame] 新故事，从对话ID 10000 开始");
            }

            // 同步对话ID到PlayerData
            CustomEntry.PlayerData.SetCurrentDialogueId(m_CurrentDialogueId);

            // 判断是否为重玩事件（事件已完成说明是重来，快进行为不同）
            int currentEventId = CustomEntry.PlayerData.CurrentEventId;
            IsReplayEvent = currentEventId > 0 && CustomEntry.PlayerData.HasCompletedEvent(currentEventId);
            Debug.Log($"[ProcedureGame] StartStory - 事件ID: {currentEventId}, 重玩: {IsReplayEvent}");

            // 进入对话页面
            SwitchSubForm(AssetUtility.GetUIFormAsset(UIFormId.Dialogue), this);
        }

        /// <summary>
        /// 获取当前数据表（动态获取）
        /// </summary>
        private IDataTable<StoryRowData> GetCurrentStoryTable()
        {
            string graphName = CustomEntry.PlayerData.currentStoryGarphName;
            if (string.IsNullOrEmpty(graphName))
            {
                Debug.LogWarning("[ProcedureGame] currentStoryGarphName 为空");
                return null;
            }

            IDataTable<StoryRowData> table = GameEntry.DataTable.GetDataTable<StoryRowData>(graphName);
            if (table == null)
            {
                Debug.LogWarning($"[ProcedureGame] 找不到剧情表: {graphName}");
                return null;
            }

            return table;
        }

        /// <summary>
        /// 获取当前对话的显示数据（供 DialoguePanel 调用）
        /// </summary>
        public DialogueDisplayData GetCurrentDialogue()
        {
            Debug.Log($"[ProcedureGame] === GetCurrentDialogue 被调用 ===");
            Debug.Log($"[ProcedureGame] m_CurrentDialogueId = {m_CurrentDialogueId}");

            IDataTable<StoryRowData> storyTable = GetCurrentStoryTable();
            if (storyTable == null)
            {
                Debug.LogWarning("[ProcedureGame] 获取剧情表失败");
                return null;
            }

            StoryRowData row = storyTable.GetDataRow(m_CurrentDialogueId);
            if (row == null)
            {
                Debug.LogWarning($"[ProcedureGame] 找不到对话数据, ID: {m_CurrentDialogueId}");
                return null;
            }

            // 奖励节点：应用奖励后自动推进（UI层完全无感知）
            if (row.NodeType == 3)
            {
                ApplyRewardsFromRow(row);

                if (row.NextId == 0)
                {
                    Debug.Log("[ProcedureGame] 奖励节点是最后一条，结束剧情");
                    EndStoryAndReturnToMap(storyTable.Name);
                    return null;
                }

                m_CurrentDialogueId = row.NextId;
                CustomEntry.PlayerData.SetCurrentDialogueId(m_CurrentDialogueId);
                return GetCurrentDialogue(); // 递归处理下一条
            }

            Debug.Log($"[ProcedureGame] 成功获取对话数据!");
            Debug.Log($"[ProcedureGame] - ID: {row.Id}");
            Debug.Log($"[ProcedureGame] - 说话人: {row.SpeakerName}");
            Debug.Log($"[ProcedureGame] - 文本: {row.DialogText}");

            // 将剧情文本中的"主控名"替换为玩家取名
            string playerName = CustomEntry.PlayerData?.PlayerName ?? "";
            string speakerName = row.SpeakerName;
            string dialogText = row.DialogText;
            Debug.Log($"[ProcedureGame] 主控名替换检查: PlayerName='{playerName}', 原始SpeakerName='{speakerName}', 原始DialogText='{dialogText}'");
            if (!string.IsNullOrEmpty(playerName))
            {
                if (speakerName.Contains("主控名") || dialogText.Contains("主控名"))
                {
                    speakerName = speakerName.Replace("主控名", playerName);
                    dialogText = dialogText.Replace("主控名", playerName);
                    Debug.Log($"[ProcedureGame] 主控名替换完成: SpeakerName='{speakerName}', DialogText='{dialogText}'");
                }
            }
            else
            {
                Debug.LogWarning("[ProcedureGame] PlayerName 为空，跳过主控名替换");
            }

            return new DialogueDisplayData
            {
                SpeakerName = speakerName,
                DialogText = dialogText,
                NextId = row.NextId,
                CurrentNodeId = row.Id,
                NodeType = row.NodeType,
                ChoicesJson = row.ChoicesJson,
                CharacterActionsJson = row.CharacterActionsJson,
                BackgroundPath = row.BackgroundPath,
                BgmPath = row.BgmPath,
                VoicePath = row.VoicePath,
                SePath = row.SePath,
                HideDialoguePanel = row.HideDialoguePanel,
                VideoPath = row.VideoPath
            };
        }

        /// <summary>
        /// 前进到下一条对话（供 DialoguePanel 调用）
        /// </summary>
        public DialogueDisplayData GoToNextDialogue()
        {
            Debug.Log($"[ProcedureGame] GoToNextDialogue 被调用，当前ID: {m_CurrentDialogueId}");

            IDataTable<StoryRowData> storyTable = GetCurrentStoryTable();
            if (storyTable == null)
            {
                Debug.LogWarning("[ProcedureGame] 获取剧情表失败");
                return null;
            }

            StoryRowData currentRow = storyTable.GetDataRow(m_CurrentDialogueId);
            if (currentRow == null)
            {
                Debug.LogWarning($"[ProcedureGame] 找不到当前对话, ID: {m_CurrentDialogueId}");
                return null;
            }

            Debug.Log($"[ProcedureGame] 当前节点 - ID: {currentRow.Id}, NodeType: {currentRow.NodeType}, NextId: {currentRow.NextId}");

            // 如果当前是选项节点，直接返回，让DialoguePanel处理
            if (currentRow.NodeType == 1)
            {
                Debug.Log("[ProcedureGame] 检测到选项节点，直接返回数据");
                return GetCurrentDialogue();
            }

            // 检查是否结束 (普通对话节点的 NextId = 0 表示结束)
            if (currentRow.NextId == 0)
            {
                Debug.Log("[ProcedureGame] 对话结束，准备返回大地图");
                EndStoryAndReturnToMap(storyTable.Name);
                return null;
            }

            // 更新当前ID并返回下一条数据
            m_CurrentDialogueId = currentRow.NextId;
            CustomEntry.PlayerData.SetCurrentDialogueId(m_CurrentDialogueId);
            Debug.Log($"[ProcedureGame] 切换到下一条对话, ID: {m_CurrentDialogueId}");
            return GetCurrentDialogue();
        }

        /// <summary>
        /// 手动设置下一句对话ID（用于选项分支）
        /// </summary>
        public void SetNextDialogueId(int nextDialogueId)
        {
            Debug.Log($"[ProcedureGame] 手动设置下一句对话ID: {nextDialogueId}");
            m_CurrentDialogueId = nextDialogueId;
            CustomEntry.PlayerData.SetCurrentDialogueId(m_CurrentDialogueId);
        }

        /// <summary>
        /// 应用奖励节点的奖励数据
        /// </summary>
        private void ApplyRewardsFromRow(StoryRowData row)
        {
            if (string.IsNullOrEmpty(row.RewardsJson)) return;

            try
            {
                var wrapper = JsonUtility.FromJson<RuntimeRewardListWrapper>(row.RewardsJson);
                if (wrapper != null && wrapper.Rewards != null && wrapper.Rewards.Count > 0)
                {
                    CustomEntry.PlayerData.ApplyRewards(wrapper.Rewards);
                    Debug.Log($"[ProcedureGame] 奖励节点({row.Id})已应用 {wrapper.Rewards.Count} 个奖励");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ProcedureGame] 奖励JSON解析失败, 节点ID: {row.Id}, 错误: {e.Message}");
            }
        }

        /// <summary>
        /// 应用选项带来的奖励（供 DialoguePanel 调用）
        /// </summary>
        public void ApplyChoiceRewards(List<ChoiceReward> rewards)
        {
            if (rewards == null || rewards.Count == 0) return;
            CustomEntry.PlayerData.ApplyRewards(rewards);
            Debug.Log($"[ProcedureGame] 选项奖励已应用 {rewards.Count} 个");
        }

        /// <summary>
        /// 加载测试故事（用于测试选项功能）
        /// </summary>
        public void LoadTestChoiceStory()
        {
            Debug.Log("[ProcedureGame] 加载测试故事 ChoiceTest");

            // 设置当前故事名称
            CustomEntry.PlayerData.SetCurrentStoryGarphName("ChoiceTest");

            // 从第一句开始
            m_CurrentDialogueId = 10000;

            // 确保故事表已加载
            if (!GameEntry.DataTable.HasDataTable<StoryRowData>())
            {
                IDataTable<StoryRowData> emptyTable = GameEntry.DataTable.CreateDataTable<StoryRowData>();
                DataTableBase tableBase = (DataTableBase)emptyTable;
                tableBase.ReadData("Assets/GameMain/Scripts/UI/Data/ChoiceTest.txt", 0, this);
            }
        }

        /// <summary>
        /// 剧情结束，返回大地图 (由 StoryForm 播放完最后一句时调用)
        /// </summary>
        public void EndStoryAndReturnToMap(string graphName)
        {
            Debug.Log($"[ProcedureGame] 剧情结束，准备卸载: {graphName}");

            // 标记事件已完成（移到结束时才标记，避免中途存档被误标完成）
            int completedEventId = CustomEntry.PlayerData.CurrentEventId;
            bool isEndingEvent = false;
            if (completedEventId > 0)
            {
                // 检查是否为结局事件（EventType=3）
                IDataTable<EventRowData> dtEvent = GameEntry.DataTable.GetDataTable<EventRowData>();
                if (dtEvent != null)
                {
                    EventRowData completedEvent = dtEvent.GetDataRow(completedEventId);
                    isEndingEvent = completedEvent != null && completedEvent.EventType == 3;
                }
                CustomEntry.PlayerData.MarkEventCompleted(completedEventId);
            }

            // 记录 NPC 进度（在卸载剧情表和清理 graphName 之前）
            UpdateNpcProgress(graphName);

            // 卸载剧情表
            m_StoryGraphLoader.UnloadGraph(graphName);

            // 清理 PlayerData 中的当前剧情图名
            CustomEntry.PlayerData.SetCurrentStoryGarphName(null);

            // 重置对话ID和事件ID
            m_CurrentDialogueId = 0;
            CustomEntry.PlayerData.SetCurrentDialogueId(0);
            CustomEntry.PlayerData.SetCurrentEventId(0);

            // 结局故事播完 → 直接周目结算，不再二次判定
            if (isEndingEvent)
            {
                Debug.Log("[ProcedureGame] 结局故事播放完毕，进入周目结算");
                TriggerNormalEnding();
                return;
            }

            // 检查 AP 是否耗尽 → 触发结局判定
            if (CustomEntry.PlayerData.CurrentActionPoints <= 0)
            {
                Debug.Log("[ProcedureGame] AP 已耗尽，进入结局判定流程");
                CheckAndTriggerEnding();
                return;
            }

            // 返回大地图
            OpenMap();
        }

        /// <summary>
        /// AP 耗尽时检查并触发结局（框架占位，实际判定逻辑待后续填充）
        /// </summary>
        private void CheckAndTriggerEnding()
        {
            Debug.Log("[ProcedureGame] ===== 结局判定开始 =====");
            Debug.Log($"[ProcedureGame] 当前周目: {CustomEntry.PlayerData.CurrentRound}");

            // TODO: 结局判定逻辑，按优先级检查：
            // 1. 男主单线结局（好感度 + 专属事件完成度）
            // 2. 常规结局（属性阈值判定）
            // 返回结局ID，0 = 常规结局

            int endingId = DetermineEnding();

            if (endingId > 0)
            {
                TriggerEnding(endingId);
            }
            else
            {
                TriggerNormalEnding();
            }
        }

        /// <summary>
        /// 结局判定核心逻辑
        /// 遍历 EventPool 中 EventType=3 的结局事件，依次检查条件
        /// 第一个满足条件的事件即为当前结局
        /// </summary>
        /// <returns>结局事件ID，0 表示无结局（常规结局）</returns>
        private int DetermineEnding()
        {
            IDataTable<EventRowData> dtEvent = GameEntry.DataTable.GetDataTable<EventRowData>();
            if (dtEvent == null)
            {
                Debug.LogWarning("[ProcedureGame] 事件表为空，无法判定结局");
                return 0;
            }

            Debug.Log("[ProcedureGame] ===== 遍历结局事件表，寻找符合条件的结局 =====");

            // 按事件ID顺序遍历（EventPool 中顺序即为优先级）
            foreach (EventRowData evt in dtEvent)
            {
                // 只处理 EventType=3 的结局事件
                if (evt.EventType != 3)
                    continue;

                Debug.Log($"[ProcedureGame] 检查结局事件: ID={evt.Id}, Title='{evt.Title}', VisibleConditions='{evt.VisibleConditions}', PlayableConditions='{evt.PlayableConditions}'");

                // 检查可见条件（VisibleConditions）
                if (!ConditionChecker.CheckCondition(evt.VisibleConditions, CustomEntry.PlayerData))
                {
                    Debug.Log($"[ProcedureGame] 结局 '{evt.Title}' 可见条件不满足，跳过");
                    continue;
                }

                // 检查触发条件（PlayableConditions）
                if (!ConditionChecker.CheckCondition(evt.PlayableConditions, CustomEntry.PlayerData))
                {
                    Debug.Log($"[ProcedureGame] 结局 '{evt.Title}' 触发条件不满足，跳过");
                    continue;
                }

                // 找到第一个满足条件的结局
                Debug.Log($"[ProcedureGame] ===== 结局命中: ID={evt.Id}, Title='{evt.Title}' =====");
                return evt.Id;
            }

            Debug.Log("[ProcedureGame] 无符合条件的结果，返回常规结局");
            return 0;
        }

        /// <summary>
        /// 触发特定结局的剧情
        /// </summary>
        private void TriggerEnding(int endingId)
        {
            Debug.Log($"[ProcedureGame] 触发结局: {endingId}");

            // 加载对应结局剧情图
            LoadStory(endingId, isResume: false);
        }

        /// <summary>
        /// 触发常规结局 / 周目结算
        /// </summary>
        private void TriggerNormalEnding()
        {
            Debug.Log("[ProcedureGame] 触发常规结局 / 周目结算");

            // 结束当前周目，计算继承加成
            CustomEntry.PlayerData.EndRound();

            // TODO: 后续替换为周目结算界面，暂时直接返回主菜单
            Debug.Log($"[ProcedureGame] 第 {CustomEntry.PlayerData.CurrentRound} 周目结束，返回主菜单");
            ReturnToMainMenu();
        }

        /// <summary>
        /// 根据当前剧情图名更新 NPC 进度
        /// </summary>
        private void UpdateNpcProgress(string graphName)
        {
            if (string.IsNullOrEmpty(graphName)) return;

            IDataTable<EventRowData> dtEvent = GameEntry.DataTable.GetDataTable<EventRowData>();
            if (dtEvent == null) return;

            // 在事件表中查找 TargetGraphName 匹配的行
            foreach (EventRowData evt in dtEvent)
            {
                if (evt.TargetGraphName == graphName)
                {
                    string eventNum = evt.EventNum;
                    if (string.IsNullOrEmpty(eventNum)) break;

                    // EventNum 格式: "npcId_eventId"，如 "1_2" 表示 NPC1 的事件2
                    string[] parts = eventNum.Split('_');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int npcId) &&
                        int.TryParse(parts[1], out int eventId))
                    {
                        CustomEntry.PlayerData.AddNpcEventProgress(npcId, eventId);
                        Debug.Log($"[ProcedureGame] NPC进度更新: NPC_{npcId} 事件_{eventId} (EventNum: {eventNum})");
                    }
                    else
                    {
                        Debug.LogWarning($"[ProcedureGame] EventNum 格式无效: {eventNum}");
                    }

                    break; // 只匹配第一条
                }
            }
        }

        #endregion

        #region 私有方法

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this || ne.DataTableAssetName.Contains("EventPool") == false) return;

            Debug.Log("[ProcedureGame] EventPool数据表加载成功！");

            // 存档断点续传
            if (m_IsLoadingFromSave)
            {
                m_IsLoadingFromSave = false;
                SaveLoadContext.ClearContext();

                var playerData = CustomEntry.PlayerData;
                int savedEventId = playerData.CurrentEventId;
                string savedStoryName = playerData.currentStoryGarphName;

                if (savedEventId > 0)
                {
                    // 有进行中的事件 → 断点续传到存档时的事件和对话进度
                    Debug.Log($"[ProcedureGame] 存档断点续传，恢复事件: {savedEventId}，对话ID: {playerData.CurrentDialogueId}");
                    LoadStory(savedEventId, isResume: true);
                }
                else if (!string.IsNullOrEmpty(savedStoryName) && savedStoryName != "DefaultStory")
                {
                    // 兼容旧存档：只有故事名没有事件ID
                    int storyEventId = GetEventIdFromStoryName(savedStoryName);
                    if (storyEventId > 0)
                    {
                        Debug.Log($"[ProcedureGame] 兼容旧存档，根据故事名恢复: {storyEventId}");
                        LoadStory(storyEventId, isResume: true);
                    }
                    else
                    {
                        Debug.Log("[ProcedureGame] 未能找到对应事件，打开大地图");
                        m_PendingOpenMap = true;
                    }
                }
                else
                {
                    // 存档时在地图上（无进行中事件）
                    Debug.Log("[ProcedureGame] 存档位于大地图，打开大地图");
                    m_PendingOpenMap = true;
                }
            }
            else if (m_PlayPrologue)
            {
                m_PlayPrologue = false;
                Debug.Log("[ProcedureGame] 新游戏流程，开始播放序章");
                m_StoryGraphLoader.LoadGraph("序章");
            }
            else
            {
                Debug.Log("[ProcedureGame] 直接打开大地图");
                // 设为待处理标志，由 OnUpdate 下一帧打开，避免时序问题
                m_PendingOpenMap = true;
            }
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this) return;
            Debug.LogError($"[ProcedureGame] EventPool加载失败！报错信息: {ne.ErrorMessage}");
        }

        /// <summary>
        /// 页面切换器
        /// </summary>
        private void SwitchSubForm(string uiFormAssetName, object userData = null)
        {
            if (m_CurrentSubFormId != -1)
            {
                GameEntry.UI.CloseUIForm(m_CurrentSubFormId);
                m_CurrentSubFormId = -1;
            }
            m_CurrentSubFormId = GameEntry.UI.OpenUIForm(uiFormAssetName, "Main", userData);
        }
        
        /// <summary>
        /// 流程游戏退出
        /// </summary>
        public void QuitGame()
        {
            CustomEntry.PlayerData.SaveOnExit();
            GameEntry.ShutdownGame(ShutdownType.Quit);
        }

        /// <summary>
        /// 返回主菜单（不自动存档，由调用方负责存档逻辑）
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[ProcedureGame] Returning to main menu");

            // 切换到主菜单流程
            ChangeState<ProcedureMainMenu>(m_ProcedureOwner);
        }

        #endregion
    }
}