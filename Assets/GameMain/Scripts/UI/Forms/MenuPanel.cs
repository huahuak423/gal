//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameMain.Scripts.UI.Base;
using GameMain.Scripts.UI.Extension;

namespace AVGGame
{
    /// <summary>
    /// 游戏内菜单 - 作为弹窗覆盖层
    /// 注意：Menu打开时不会关闭当前页面，用于对话等场景
    /// </summary>
    public class MenuPanel : UIFormBase
    {
        #region 序列化字段 - 按钮

        [Header("菜单按钮")]
        [SerializeField] private Button m_ButtonResume;
        [SerializeField] private Button m_ButtonSave;
        [SerializeField] private Button m_ButtonSettings;
        [SerializeField] private Button m_ButtonBack;

        [Header("透明背景 - 用于隔绝用户其他操作")]
        [SerializeField] private Button m_TransparentBgButton;

        #endregion

        #region 私有字段

        private bool m_IsPaused = false;

        #endregion

        #region 属性

        /// <summary>
        /// 高层级显示，确保覆盖在其他UI上方
        /// </summary>
        public override int SortingOrder => 200;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 绑定按钮事件
            if (m_ButtonResume != null)
                m_ButtonResume.onClick.AddListener(OnResumeClick);

            if (m_ButtonSave != null)
                m_ButtonSave.onClick.AddListener(OnSaveClick);

            if (m_ButtonSettings != null)
                m_ButtonSettings.onClick.AddListener(OnSettingClick);

            if (m_ButtonBack != null)
                m_ButtonBack.onClick.AddListener(OnBackClick);

            // 透明背景点击也关闭菜单
            if (m_TransparentBgButton != null)
                m_TransparentBgButton.onClick.AddListener(OnResumeClick);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 暂停游戏
            PauseGame();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            // 恢复游戏
            ResumeGame();
        }

        #endregion

        #region 按钮事件

        /// <summary>
        /// 继续游戏 - 关闭菜单
        /// </summary>
        private void OnResumeClick()
        {
            Log.Info("[MenuPanel] Resume clicked");
            CloseSelf();
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        private void OnSaveClick()
        {
            Log.Info("[MenuPanel] Save game clicked");
            // TODO: 实际保存游戏逻辑
        }

        /// <summary>
        /// 打开设置界面
        /// </summary>
        private void OnSettingClick()
        {
            Log.Info("[MenuPanel] Settings clicked");
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Settings),
                UIGroupDefinition.Popup,
                Constant.AssetPriority.UIAsset,
                null
            );
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        private void OnBackClick()
        {
            Log.Info("[MenuPanel] Back to main menu clicked");

            // 关闭当前菜单
            CloseSelf();

            // TODO: 切换到主菜单场景
            // GameEntry.Scene.LoadScene("MainMenu");
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            m_IsPaused = true;
            Time.timeScale = 0f;
            Log.Info("[MenuPanel] Game paused");
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            m_IsPaused = false;
            Time.timeScale = 1f;
            Log.Info("[MenuPanel] Game resumed");
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}