//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityGameFramework.Runtime;
using GameMain.Scripts.UI.Base;

namespace GameMain.Scripts.UI.Forms
{
    /// <summary>
    /// 主菜单面板
    /// </summary>
    public class MainMenuPanel : UIFormBase
    {
        #region 序列化字段

        [Header("按钮")]
        [SerializeField] private Button m_NewGameButton;
        [SerializeField] private Button m_ContinueButton;
        [SerializeField] private Button m_SettingsButton;
        [SerializeField] private Button m_QuitButton;

        [Header("标题")]
        [SerializeField] private TextMeshProUGUI m_TitleText;
        [SerializeField] private Image m_LogoImage;

        [Header("版本")]
        [SerializeField] private TextMeshProUGUI m_VersionText;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 绑定按钮事件
            if (m_NewGameButton != null)
            {
                m_NewGameButton.onClick.AddListener(OnNewGameButtonClick);
            }

            if (m_ContinueButton != null)
            {
                m_ContinueButton.onClick.AddListener(OnContinueButtonClick);
            }

            if (m_SettingsButton != null)
            {
                m_SettingsButton.onClick.AddListener(OnSettingsButtonClick);
            }

            if (m_QuitButton != null)
            {
                m_QuitButton.onClick.AddListener(OnQuitButtonClick);
            }

            // 设置版本号
            if (m_VersionText != null)
            {
                m_VersionText.text = $"Version {Application.version}";
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 检查是否有存档
            bool hasSave = CheckHasSave();
            if (m_ContinueButton != null)
            {
                m_ContinueButton.interactable = hasSave;
            }
        }

        #endregion

        #region 按钮事件

        private void OnNewGameButtonClick()
        {
            Log.Info("[MainMenu] New Game clicked");
            CloseSelf();

            // TODO: 开始新游戏
        }

        private void OnContinueButtonClick()
        {
            Log.Info("[MainMenu] Continue clicked");
            CloseSelf();

            // TODO: 继续游戏
        }

        private void OnSettingsButtonClick()
        {
            Log.Info("[MainMenu] Settings clicked");
            // TODO: 打开设置面板
        }

        private void OnQuitButtonClick()
        {
            Log.Info("[MainMenu] Quit clicked");

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
            return false;
        }

        #endregion
    }
}