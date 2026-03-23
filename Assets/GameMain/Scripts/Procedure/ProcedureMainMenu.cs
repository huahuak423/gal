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
    /// 主菜单流程 - 显示游戏主界面、开始游戏按钮等
    /// </summary>
    public class ProcedureMainMenu : ProcedureBase
    {
        #region 字段
        
        // 唯一的页面记录器：画廊、存档、设置，谁打开就记录谁
        private int m_CurrentSubFormId = -1;
        
        private bool m_StartGame = false;

        #endregion

        #region 生命周期

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedureMainMenu] Enter");
            OpenMainMenuUI();
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            if (m_CurrentSubFormId != -1)
            {
                GameEntry.UI.CloseUIForm(m_CurrentSubFormId);
                m_CurrentSubFormId = -1;
            }
            
            base.OnLeave(procedureOwner, isShutdown);
            
            Log.Info("[ProcedureMainMenu] Leave");
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_StartGame)
            {
                Log.Info($"[ProcedureMainMenu] 准备进入游戏！");
                ChangeState<ProcedureGame>(procedureOwner);
            }
        }

        #endregion

        #region 公共方法

        public void StartGame()
        {
            m_StartGame = true;
        }
        
        public void QuitGame()
        {
            GameEntry.ShutdownGame(ShutdownType.Quit);
        }
        
        public void OpenArchive(bool isNewGame)
        {
            Log.Info("[ProcedureMainMenu] 打开存档界面, isNewGame: " + isNewGame);
            // 这里如果你之前定义了 ArchiveUserData，直接传进去
            SwitchSubForm(AssetUtility.GetUIFormAsset(UIFormId.Archive), this);
        }
        public void OpenGallery()
        {
            Log.Info("[ProcedureMainMenu] 打开画廊界面");
            
        }
        
        public void OpenSetting()
        {
            Log.Info("[ProcedureMainMenu] 打开设置界面");
            SwitchSubForm(AssetUtility.GetUIFormAsset(UIFormId.Settings), this);
        }
        
        public void ReturnToHome()
        {
            Log.Info("[ProcedureMainMenu] 返回主菜单首页");
            // 关掉当前不管是什么的页面（存档/画廊/设置）
            if (m_CurrentSubFormId != -1)
            {
                GameEntry.UI.CloseUIForm(m_CurrentSubFormId);
                m_CurrentSubFormId = -1;
            }
            // 重新拉起首页
            OpenMainMenuUI();
        }

        #endregion

        #region 私有方法

        private void OpenMainMenuUI()
        {
            if (m_CurrentSubFormId == -1)
            {
                m_CurrentSubFormId = GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset(UIFormId.MainMenu), UIGroupDefinition.Main, Constant.AssetPriority.UIAsset, this);
            }
        }
        /// <summary>
        /// 页面切换器
        /// </summary>
        /// <param name="uiFormAssetName">页面id</param>
        /// <param name="userData">UI页面会用到的数据</param>
        private void SwitchSubForm(string uiFormAssetName, object userData = null)
        {
            // 关掉前一个页面（保证绝对的互斥）
            if (m_CurrentSubFormId != -1)
            {
                GameEntry.UI.CloseUIForm(m_CurrentSubFormId);
                m_CurrentSubFormId = -1;
            }

            // 3. 打开新的子页面
            m_CurrentSubFormId = GameEntry.UI.OpenUIForm(uiFormAssetName, "Main", userData);
        }

        #endregion
    }
}