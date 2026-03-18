//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameMain.Scripts.UI.Base;
using GameMain.Scripts.UI.Extension;
using GameFramework.Fsm;
using GameFramework.Procedure;

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
        [SerializeField] private Button m_ButtonContine;
        [SerializeField] private Button m_ButtonCgShows;
        [SerializeField] private Button m_ButtonSetting;
        [SerializeField] private Button m_ButtonExit;

        #endregion

        #region 私有字段

        private IFsm<IProcedureManager> m_ProcedureFsm;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 绑定按钮事件
            if (m_ButtonNewGame != null)
                m_ButtonNewGame.onClick.AddListener(OnNewGameClick);

            if (m_ButtonContine != null)
                m_ButtonContine.onClick.AddListener(OnContinueClick);

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
            m_ProcedureFsm = userData as IFsm<IProcedureManager>;

            // 检查是否有存档
            bool hasSave = CheckHasSave();
            if (m_ButtonContine != null)
            {
                m_ButtonContine.interactable = hasSave;
            }

            Log.Info("[MainMenuPanel] Opened");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            m_ProcedureFsm = null;
        }

        #endregion

        #region 按钮事件

        /// <summary>
        /// 新游戏 - 打开存档选择界面（新游戏模式）
        /// </summary>
        private void OnNewGameClick()
        {
            Log.Info("[MainMenuPanel] New Game clicked");
            CloseSelf();
            // 打开存档选择界面，传入 true 表示新游戏模式
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Archive),
                UIGroupDefinition.Main,
                Constant.AssetPriority.UIAsset,
                true
            );
        }

        /// <summary>
        /// 继续游戏 - 打开存档选择界面（继续游戏模式）
        /// </summary>
        private void OnContinueClick()
        {
            Log.Info("[MainMenuPanel] Continue clicked");
            CloseSelf();
            // 打开存档选择界面，传入 false 表示继续游戏模式
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Archive),
                UIGroupDefinition.Main,
                Constant.AssetPriority.UIAsset,
                false
            );
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
            // TODO: 打开设置界面
        }

        /// <summary>
        /// 退出
        /// </summary>
        private void OnExitClick()
        {
            Log.Info("[MainMenuPanel] Exit clicked");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region 私有方法

        private bool CheckHasSave()
        {
            // TODO: 检查是否有存档
            return true; // 暂时返回 true 方便测试
        }

        #endregion
    }
}