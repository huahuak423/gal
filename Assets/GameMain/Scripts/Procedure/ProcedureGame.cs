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

        #endregion

        #region 生命周期
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_ProcedureOwner = procedureOwner;
            Debug.Log("[ProcedureGame] 正式进入游戏核心流程！");

            // 检查是否从存档加载
            if (SaveLoadContext.IsLoadingFromSave)
            {
                Debug.Log("[ProcedureGame] 检测到存档加载，准备进入相应剧情或地图");

                var (hasTarget, storyName, eventId) = SaveLoadContext.GetTargetInfo();
                if (hasTarget)
                {
                    if (eventId > 0)
                    {
                        Debug.Log($"[ProcedureGame] 直接进入事件: {eventId}");
                        LoadStory(eventId);
                    }
                    else if (!string.IsNullOrEmpty(storyName))
                    {
                        // 根据故事名称查找事件ID
                        int storyEventId = GetEventIdFromStoryName(storyName);
                        if (storyEventId > 0)
                        {
                            Debug.Log($"[ProcedureGame] 根据故事名称进入事件: {storyEventId}");
                            LoadStory(storyEventId);
                        }
                        else
                        {
                            Debug.Log("[ProcedureGame] 未能找到对应事件，打开大地图");
                            OpenMap();
                        }
                    }
                }
                else
                {
                    Debug.Log("[ProcedureGame] 没有找到具体目标，打开大地图");
                    OpenMap();
                }

                // 清除上下文（只清除一次）
                SaveLoadContext.ClearContext();
            }
            else
            {
                Debug.Log("[ProcedureGame] 新游戏流程，打开大地图");
                OpenMap();
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

        /// <summary>
        /// 检查选项条件是否满足（供外部使用）
        /// </summary>
        public bool CheckChoiceConditions(string npcId, ConditionType type, int value, ConditionOperator op = ConditionOperator.GreaterThanOrEqual)
        {
            var playerData = CustomEntry.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[ProcedureGame] PlayerData is null");
                return false;
            }

            switch (type)
            {
                case ConditionType.PlayerAttribute:
                    switch (op)
                    {
                        case ConditionOperator.GreaterThanOrEqual:
                            return GetPlayerAttribute(npcId) >= value;
                        case ConditionOperator.LessThanOrEqual:
                            return GetPlayerAttribute(npcId) <= value;
                        case ConditionOperator.Equal:
                            return GetPlayerAttribute(npcId) == value;
                    }
                    break;

                case ConditionType.NpcFavorability:
                    if (int.TryParse(npcId, out int npcIdInt))
                    {
                        int favorability = playerData.GetFavorability(npcIdInt);
                        switch (op)
                        {
                            case ConditionOperator.GreaterThanOrEqual:
                                return favorability >= value;
                            case ConditionOperator.LessThanOrEqual:
                                return favorability <= value;
                            case ConditionOperator.Equal:
                                return favorability == value;
                        }
                    }
                    break;

                case ConditionType.SpecialItem:
                    if (int.TryParse(npcId, out int itemId))
                    {
                        bool hasItem = playerData.HasItem(itemId);
                        switch (op)
                        {
                            case ConditionOperator.Equal:
                                return hasItem;
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 获取玩家属性值
        /// </summary>
        private int GetPlayerAttribute(string attributeType)
        {
            switch (attributeType.ToLower())
            {
                case "charm":
                    return CustomEntry.PlayerData?.Charm ?? 0;
                case "inspiration":
                    return CustomEntry.PlayerData?.Inspiration ?? 0;
                case "sanity":
                    return CustomEntry.PlayerData?.Sanity ?? 0;
                default:
                    Debug.LogWarning($"[ProcedureGame] Unknown attribute type: {attributeType}");
                    return 0;
            }
        }

        #endregion

        #region 公共方法

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
        /// 加载剧情
        /// </summary>
        public void LoadStory(int eventId)
        {
            EventRowData currentData = GameEntry.DataTable.GetDataTable<EventRowData>().GetDataRow(eventId);
            Debug.Log($"[ProcedureGame] LoadStory 被调用, eventId: {eventId}");
            Debug.Log($"[ProcedureGame] TargetGraphName: {currentData.TargetGraphName}");

            // 标记事件已完成（用于存档）
            CustomEntry.PlayerData.MarkEventCompleted(eventId);

            // EventType == 2 是角色特殊事件，完成后不再显示
            if (currentData.EventType == 2)
            {
                Debug.Log($"[ProcedureGame] 角色事件已完成: {eventId} - {currentData.Title}");
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

            // 从第一条开始（默认起始ID为10000）
            m_CurrentDialogueId = 10000;

            Debug.Log($"[ProcedureGame] m_CurrentDialogueId 设置为: {m_CurrentDialogueId}");

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

            Debug.Log($"[ProcedureGame] 成功获取对话数据!");
            Debug.Log($"[ProcedureGame] - ID: {row.Id}");
            Debug.Log($"[ProcedureGame] - 说话人: {row.SpeakerName}");
            Debug.Log($"[ProcedureGame] - 文本: {row.DialogText}");

            return new DialogueDisplayData
            {
                SpeakerName = row.SpeakerName,
                DialogText = row.DialogText,
                NextId = row.NextId,
                CurrentNodeId = row.Id,
                NodeType = row.NodeType,
                ChoicesJson = row.ChoicesJson,
                CharacterActionsJson = row.CharacterActionsJson,
                BackgroundPath = row.BackgroundPath
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

            // 卸载剧情表
            m_StoryGraphLoader.UnloadGraph(graphName);

            // 清理 PlayerData 中的当前剧情图名（避免后续误用）
            CustomEntry.PlayerData.SetCurrentStoryGarphName(null);

            // 重置对话ID
            m_CurrentDialogueId = 0;

            // 返回大地图
            OpenMap();
        }

        #endregion

        #region 私有方法

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this || ne.DataTableAssetName.Contains("EventPool") == false) return;

            Debug.Log("[ProcedureGame] EventPool数据表加载成功！直接打开大地图！");
            OpenMap();
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
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[ProcedureGame] Returning to main menu");

            // 保存当前游戏状态
            CustomEntry.PlayerData.SaveOnExit();

            // 切换到主菜单流程
            ChangeState<ProcedureMainMenu>(m_ProcedureOwner);
        }

        #endregion
    }
}