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

        // 加载剧本类
        private StoryGraphLoader m_StoryGraphLoader;

        // 对话状态管理
        private int m_CurrentDialogueId = 0;

        #endregion

        #region 生命周期
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Debug.Log("[ProcedureGame] 正式进入游戏核心流程！");

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

        #region 公共方法

        /// <summary>
        /// 功能1：判断哪些事件应该显示在选项框上 (可见条件筛选)
        /// </summary>
        public List<EventRowData> GetVisibleEventsInMap(int mapId)
        {
            List<EventRowData> result = new List<EventRowData>();

            // 获取该地图所有的原始事件 (从 DataTable 获取或从你的字典获取)
            List<EventRowData> allEventsInMap = GetRawEventsByMapId(mapId);

            foreach (var evt in allEventsInMap)
            {
                // 过滤已完成的特殊事件（EventType == 1 表示特殊事件）
                if (evt.EventType == 1 && CustomEntry.PlayerData.HasCompletedSpecialEvent(evt.Id))
                {
                    Debug.Log($"[ProcedureGame] 跳过已完成的特殊事件: {evt.Id} - {evt.Title}");
                    continue;
                }

                // 使用刚刚写好的工具检查 VisibleConditions 列
                if (ConditionChecker.CheckCondition(evt.VisibleConditions, CustomEntry.PlayerData))
                {
                    result.Add(evt);
                }
            }

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

            // 2. 实时遍历整张表
            foreach (EventRowData row in dtEvent)
            {
                // 只要所在地图匹配，就把它加进待选列表
                if (row.MapId == mapId)
                {
                    result.Add(row);
                }
            }

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
            Debug.Log("[ProcedureGame] 切换到大地图界面");
            SwitchSubForm(AssetUtility.GetUIFormAsset(UIFormId.Map), this);
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

            // 如果是特殊事件（EventType == 1），额外标记为已完成的特殊事件
            if (currentData.EventType == 1)
            {
                CustomEntry.PlayerData.MarkSpecialEventCompleted(eventId);
                Debug.Log($"[ProcedureGame] 特殊事件已完成标记: {eventId}");
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
                ChoicesJson = row.ChoicesJson
            };
        }

        /// <summary>
        /// 前进到下一条对话（供 DialoguePanel 调用）
        /// </summary>
        public DialogueDisplayData GoToNextDialogue()
        {
            Debug.Log($"[ProcedureGame] GoToNextDialogue 被调用");

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

            // 检查是否结束 (NextId = 0 表示结束)
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
        #endregion
    }
}