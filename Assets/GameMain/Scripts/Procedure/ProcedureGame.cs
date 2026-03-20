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
        
        //加载剧本类
        private StoryGraphLoader  m_StoryGraphLoader;
        
        #endregion
        
        #region 生命周期
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedureGame] 正式进入游戏核心流程！");

            // 1. 读取主菜单传过来的数据
            //bool isNewGame = procedureOwner.GetData<VarBool>("IsNewGame");
            
            //读取事件表成功（失败）回调
            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            
            //剧本加载类成功（失败）回调
            if(m_StoryGraphLoader == null)
            {
                m_StoryGraphLoader = new StoryGraphLoader();
            }
            m_StoryGraphLoader.InitListener();
            
            //检查内存里是不是已经有这本账本了
            if (GameEntry.DataTable.HasDataTable<EventRowData>())
            {
                // 如果有，为了防止重复装填，我们可以先把它销毁
                GameEntry.DataTable.DestroyDataTable<EventRowData>();
            }
            
            // 在这里加载 EventPool数据表
            IDataTable<EventRowData> emptyTable = GameEntry.DataTable.CreateDataTable<EventRowData>();
            DataTableBase tableBase = (DataTableBase)emptyTable;
            string realPath = "Assets/GameMain/DataTables/EventPool.txt";
            tableBase.ReadData(realPath, 0, this);
            
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            
            if (m_StoryGraphLoader != null)
            {
                m_StoryGraphLoader.RemoveListener(); 
                m_StoryGraphLoader = null;
            }
            
            base.OnLeave(procedureOwner,isShutdown);
        }
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 功能1：判断哪些事件应该显示在选项框上 (可见条件筛选)
        /// </summary>
        public List<EventRowData> GetVisibleEventsInMap(string mapId, PlayerDataComponent playerData)
        {
            List<EventRowData> result = new List<EventRowData>();

            // 获取该地图所有的原始事件 (从 DataTable 获取或从你的字典获取)
            List<EventRowData> allEventsInMap = GetRawEventsByMapId(mapId); 

            foreach (var evt in allEventsInMap)
            {
                // 使用刚刚写好的工具检查 VisibleConditions 列
                if (ConditionChecker.CheckCondition(evt.VisibleConditions, playerData))
                {
                    result.Add(evt);
                }
            }

            return result;
        }
        
        
        /// <summary>
        /// 从底层的全量数据表中，实时筛选出属于某个地图的所有事件（不含条件过滤）
        /// </summary>
        public List<EventRowData> GetRawEventsByMapId(string mapId)
        {
            List<EventRowData> result = new List<EventRowData>();

            // 1. 向 UGF 大管家伸手，拿到内存中已经加载好的事件表
            IDataTable<EventRowData> dtEvent = GameEntry.DataTable.GetDataTable<EventRowData>();

            if (dtEvent == null)
            {
                Log.Warning($"[EventManager] 找不到 EventRowData 数据表！请检查是否已成功加载。");
                return result;
            }

            // 2. 实时遍历整张表（对于几百上千条数据的单机游戏，耗时不到 1 毫秒）
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
        /// 如果你只想要纯粹的 TargetGraphName 列表（例如传给某些纯逻辑模块）：
        /// </summary>
        public List<string> GetVisibleGraphNamesInMap(string mapId, PlayerDataComponent playerData)
        {
            List<string> graphNames = new List<string>();
            List<EventRowData> visibleEvents = GetVisibleEventsInMap(mapId, playerData);
            
            foreach (var evt in visibleEvents)
            {
                graphNames.Add(evt.TargetGraphName);
            }
            return graphNames;
        }
        
        /// <summary>
        /// 判断某个显示出来的事件，按钮是否亮起可点 (可玩条件筛选)
        /// 玩家点击按钮或 UI 刷新时调用。
        /// </summary>
        public bool IsEventPlayable(EventRowData evt, PlayerDataComponent playerData)
        {
            // 如果连体力都不够，直接不可玩（假设体力判断在这里做）
            if (playerData.CurrentActionPoints < evt.CostAP)
            {
                return false;
            }

            // 使用工具检查 PlayableConditions 列
            return ConditionChecker.CheckCondition(evt.PlayableConditions, playerData);
        }
        
        /// <summary>
        /// 放回指定地图id里的所有事件
        /// </summary>
        /// <param name="mapId">地图id</param>
        /// <returns></returns>
        public List<EventRowData> GetEventsByMapId(string mapId)
        {
            List<EventRowData> result = new List<EventRowData>();

            // 1. 直接向 UGF 伸手要已经加载好的整张表（瞬间完成，不读硬盘）
            IDataTable<EventRowData> dtEvent = GameEntry.DataTable.GetDataTable<EventRowData>();

            // 2. 实时进行全表遍历筛选
            if (dtEvent != null)
            {
                foreach (EventRowData row in dtEvent)
                {
                    // 只要所属地图对得上，就塞进结果列表里
                    if (row.MapId == mapId)
                    {
                        result.Add(row);
                    }
                }
            }

            return result; // 把找出的事件列表交给秘书 (UI)
        }
        /// <summary>
        /// 打开大地图 (被 OnEnter 或 剧情结束时调用)
        /// </summary>
        public void OpenMap()
        {
            Log.Info("[ProcedureGame] 切换到大地图界面");
            // 把流程自己 (this) 传过去
            SwitchSubForm(AssetUtility.GetUIFormAsset(UIFormId.Map), this);
        }

        /// <summary>
        /// 进入小地图
        /// </summary>
        /// <param name="MapId">地图id</param>
        public void EntryPlace(int MapId)
        {
            //打开对话页面，去事件池里寻找地图1且当前已经解锁的事件
            
            
        }

        /// <summary>
        /// 进入剧情
        /// </summary>
        public void StartStory(int eventId, int costAp)
        {
            Log.Info($"[ProcedureGame] 扣除 {costAp} AP，开始播放事件: {eventId}");
            
            // 扣除体力
            
            // 把自己 (this) 和 目标事件 ID 打包传给 StoryForm
          
        }

        /// <summary>
        /// 剧情结束，返回大地图 (由 StoryForm 播放完最后一句时调用)
        /// </summary>
        public void EndStoryAndReturnToMap()
        {
            Log.Info("[ProcedureGame] 剧情结束，返回大地图");
            
            // 如果没体力了，直接进入结算流程（下一天/周目）

            // 还有体力，重新拉起大地图
            OpenMap();
        }
    
        #endregion
        
        #region 私有方法
        
        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e; if (ne.UserData != this || ne.DataTableAssetName.Contains("EventPool") == false) return;

            Log.Info("<color=green>[ProcedureGame] 数据表加载成功！直接打开大地图！</color>");
            // 表已经安稳地躺在 UGF 内存里了，直接干活！
            OpenMap(); 
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            // ... 报错日志 ...
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this) return;
            Log.Error($"加载失败了，快去检查路径对不对！报错信息: {ne.ErrorMessage}");
        }
        
        
        /// <summary>
        /// 页面切换器
        /// </summary>
        /// <param name="uiFormAssetName">页面id</param>
        /// <param name="userData">UI页面会用到的数据</param>
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
