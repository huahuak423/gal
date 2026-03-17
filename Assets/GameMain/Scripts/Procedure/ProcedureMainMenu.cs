//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
using AVGGame;
using GameMain.Scripts.UI.Base;
using GameMain.Scripts.UI.Extension;
using GameEntry = AVGGame.GameEntry;

namespace AVGGame
{
    /// <summary>
    /// 主菜单流程 - 显示游戏主界面、开始游戏按钮等
    /// </summary>
    public class ProcedureMainMenu : ProcedureBase
    {
        #region 字段

        private bool m_StartNewGame;
        private bool m_ContinueGame;

        #endregion

        #region 生命周期

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedureMainMenu] Enter");

            m_StartNewGame = false;
            m_ContinueGame = false;

            OpenMainMenuUI();
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            Log.Info("[ProcedureMainMenu] Leave");
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_StartNewGame)
            {
                ChangeState<ProcedureGame>(procedureOwner);
                return;
            }

            if (m_ContinueGame)
            {
                ChangeState<ProcedureGame>(procedureOwner);
            }
        }

        #endregion

        #region 公共方法

        public void StartNewGame()
        {
            m_StartNewGame = true;
        }

        public void ContinueGame()
        {
            m_ContinueGame = true;
        }

        public void QuitGame()
        {
            GameEntry.ShutdownGame(ShutdownType.Quit);
        }

        #endregion

        #region 私有方法

        private void OpenMainMenuUI()
        {
            var uiComponent = GameEntry.UI;
            if (uiComponent == null)
            {
                Log.Error("[ProcedureMainMenu] UIComponent is null!");
                return;
            }

            string uiFormAssetName = AssetUtility.GetUIFormAsset(UIFormId.MainMenu);
            GameEntry.UI.OpenUIForm(uiFormAssetName, UIGroupDefinition.Main, Constant.AssetPriority.UIAsset);
        }

        #endregion
    }
}