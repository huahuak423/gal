//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using AVGGame;
using GameFramework.Fsm;
using GameFramework.Procedure;
using System;

namespace AVGGame
{
    /// <summary>
    /// 主菜单面板
    /// </summary>
    public class MainMenuPanel : UIFormBase
    {
        #region 序列化字段

        [Header("按钮 - ButtonPlate 下")]
        [SerializeField] private Button m_ButtonNewGame;
        [SerializeField] private Button m_ButtonContinue;
        [SerializeField] private Button m_ButtonCgShows;
        [SerializeField] private Button m_ButtonSetting;
        [SerializeField] private Button m_ButtonExit;

        #endregion

        #region 私有字段

        private ProcedureMainMenu m_ProcedureMainMenu = null;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 挂载组件引用
            m_ButtonNewGame = this.GetComponentByPath<Button>("Canvas/Background/ButtonPlate/ButtonNewGame");
            m_ButtonContinue = this.GetComponentByPath<Button>("Canvas/Background/ButtonPlate/ButtonContine");
            m_ButtonCgShows = this.GetComponentByPath<Button>("Canvas/Background/ButtonPlate/ButtonCgShows");
            m_ButtonSetting = this.GetComponentByPath<Button>("Canvas/Background/ButtonPlate/ButtonSetting");
            m_ButtonExit = this.GetComponentByPath<Button>("Canvas/Background/ButtonPlate/ButtonExit");

            // 绑定按钮事件
            if (m_ButtonNewGame != null)
                m_ButtonNewGame.onClick.AddListener(OnNewGameClick);

            if (m_ButtonContinue != null)
                m_ButtonContinue.onClick.AddListener(OnContinueClick);

            if (m_ButtonCgShows != null)
                m_ButtonCgShows.onClick.AddListener(OnCgShowsClick);

            if (m_ButtonSetting != null)
                m_ButtonSetting.onClick.AddListener(OnSettingClick);

            if (m_ButtonExit != null)
                m_ButtonExit.onClick.AddListener(OnExitClick);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 保存流程引用
            m_ProcedureMainMenu = userData as ProcedureMainMenu;

            // 检查是否有存档
            bool hasSave = CheckHasSave();
            if (m_ButtonContinue != null)
            {
                m_ButtonContinue.interactable = hasSave;
            }

            Log.Info("[MainMenuPanel] Opened");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            m_ProcedureMainMenu = null;
            base.OnClose(isShutdown, userData);
        }

        #endregion

        #region 按钮事件

        /// <summary>
        /// 新游戏 - 直接打开大地图
        /// </summary>
        private void OnNewGameClick()
        {
            Log.Info("[MainMenuPanel] New Game clicked");
            if (m_ProcedureMainMenu != null)
            {
                m_ProcedureMainMenu.StartGame();
            }
            
        }

        /// <summary>
        /// 继续游戏 - 打开存档选择界面（继续游戏模式）
        /// </summary>
        private void OnContinueClick()
        {
            Debug.Log("[MainMenuPanel] Continue clicked");
            Debug.Log("[MainMenuPanel] m_ProcedureMainMenu: " + (m_ProcedureMainMenu != null));

            // 先保存 ProcedureMainMenu 的引用，再关闭当前页面
            ProcedureMainMenu procedureMainMenu = m_ProcedureMainMenu;

            CloseSelf();

            // 打开存档选择界面（false=继续游戏=加载模式）
            if (procedureMainMenu != null)
            {
                Debug.Log("[MainMenuPanel] Calling OpenArchive(false)");
                procedureMainMenu.OpenArchive(false);
                Debug.Log("[MainMenuPanel] OpenArchive called successfully");
            }
            else
            {
                Debug.Log("[MainMenuPanel] m_ProcedureMainMenu is null!");
            }
        }

        /// <summary>
        /// 画廊
        /// </summary>
        private void OnCgShowsClick()
        {
            Log.Info("[MainMenuPanel] CG Gallery clicked");
            // TODO: 打开画廊界面
        }

        /// <summary>
        /// 设置
        /// </summary>
        private void OnSettingClick()
        {
            Log.Info("[MainMenuPanel] Settings clicked");
            CloseSelf();

            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Settings),
                UIGroupDefinition.Popup,
                Constant.AssetPriority.UIAsset,
                null
            );
        }

        /// <summary>
        /// 退出
        /// </summary>
        private void OnExitClick()
        {
            Log.Info("[MainMenuPanel] Exit clicked");
            if (m_ProcedureMainMenu != null)
            {
                m_ProcedureMainMenu.QuitGame(); 
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检查是否有任何存档槽位存档
        /// </summary>
        /// <returns>是否有存档</returns>
        private bool CheckHasSave()
        {
            try
            {
                // 检查所有12个槽位是否有任何一个有存档
                for (int slotId = 1; slotId <= 12; slotId++)
                {
                    if (CustomEntry.SaveSystem?.HasSave(slotId) ?? false)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Error($"[MainMenuPanel] 检查存档失败: {e.Message}");
                return false;
            }
        }

        #endregion
    }
}