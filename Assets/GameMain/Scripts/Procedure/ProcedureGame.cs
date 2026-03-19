//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

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
        
        #endregion
        
        #region 生命周期
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedureGame] 正式进入游戏核心流程！");

            // 1. 读取主菜单传过来的数据
            //bool isNewGame = procedureOwner.GetData<VarBool>("IsNewGame");
            
            // TODO: 2. 在这里加载 EventPool 等数据表！

            // 3. 游戏开始，默认拉起大地图
            //OpenMap();
        }
        #endregion
        
        #region 公共方法
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
        /// 进入剧情 (由 MapForm 点击地点时调用)
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
