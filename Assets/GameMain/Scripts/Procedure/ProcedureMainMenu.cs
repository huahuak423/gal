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
                try
                {
                    GameEntry.UI.CloseUIForm(m_CurrentSubFormId);
                    Log.Info($"[ProcedureMainMenu] Closed sub form: {m_CurrentSubFormId}");
                }
                catch (System.Exception e)
                {
                    Log.Warning($"[ProcedureMainMenu] Failed to close sub form {m_CurrentSubFormId}: {e.Message}");
                }
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
            Debug.Log("[ProcedureMainMenu] 打开存档界面, isNewGame: " + isNewGame);
            Debug.Log("[ProcedureMainMenu] UIFormId.Archive: " + UIFormId.Archive);

            // 检查 AssetUtility 是否有对应的方法
            string archiveAsset = AssetUtility.GetUIFormAsset(UIFormId.Archive);
            Debug.Log("[ProcedureMainMenu] Archive asset path: " + archiveAsset);

            if (string.IsNullOrEmpty(archiveAsset))
            {
                Debug.Log("[ProcedureMainMenu] Archive asset path is null or empty!");
                return;
            }

            // 传递 ArchiveMode 参数：true=新游戏(保存模式), false=继续游戏(加载模式)
            ArchivePanel.ArchiveMode mode = isNewGame ? ArchivePanel.ArchiveMode.Save : ArchivePanel.ArchiveMode.Load;
            Debug.Log("[ProcedureMainMenu] Mode: " + mode);

            Debug.Log("[ProcedureMainMenu] Calling SwitchSubForm...");
            SwitchSubForm(archiveAsset, mode);
            Debug.Log("[ProcedureMainMenu] SwitchSubForm completed");
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
            Debug.Log($"[ProcedureMainMenu] SwitchSubForm - uiFormAssetName: {uiFormAssetName}, userData: {userData}");
            Debug.Log($"[ProcedureMainMenu] Current m_CurrentSubFormId: {m_CurrentSubFormId}");

            // 打开新表单前保存当前ID
            int previousFormId = m_CurrentSubFormId;

            // 直接打开新表单，不主动关闭旧的
            Debug.Log("[ProcedureMainMenu] Opening new UI form...");
            try
            {
                m_CurrentSubFormId = GameEntry.UI.OpenUIForm(uiFormAssetName, "Main", userData);
                Debug.Log($"[ProcedureMainMenu] UI form opened successfully, ID: {m_CurrentSubFormId}");

                // 新表单打开成功后，再尝试关闭旧的
                if (previousFormId != -1)
                {
                    Debug.Log($"[ProcedureMainMenu] Attempting to close previous form: {previousFormId}");

                    // 使用 Try-Catch 避免关闭失败影响流程
                    try
                    {
                        GameEntry.UI.CloseUIForm(previousFormId);
                        Debug.Log($"[ProcedureMainMenu] Successfully closed previous form");
                    }
                    catch (System.Exception closeEx)
                    {
                        Debug.LogWarning($"[ProcedureMainMenu] Failed to close previous form {previousFormId}: {closeEx.Message}");
                        // 不重新抛出异常，保持流程继续
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProcedureMainMenu] Failed to open UI form: {e.Message}");
                throw;
            }
        }

        #endregion
    }
}