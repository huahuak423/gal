//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityGameFramework.Runtime;
using GameMain.Scripts.UI.Base;
using GameMain.Scripts.UI.Extension;
using GameEntry = AVGGame.GameEntry;

namespace GameMain.Scripts.UI
{
    /// <summary>
    /// UI 辅助工具类
    /// 提供便捷的 UI 操作方法
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// 获取 UI 组件
        /// </summary>
        private static UnityGameFramework.Runtime.UIComponent UIComponent
        {
            get { return GameEntry.UI; }
        }

        /// <summary>
        /// 打开主菜单
        /// </summary>
        public static void OpenMainMenu()
        {
            UIComponent.OpenUIFormById(UIFormId.MainMenu, UIGroupDefinition.Main);
        }

        /// <summary>
        /// 打开设置面板
        /// </summary>
        public static void OpenSettings()
        {
            UIComponent.OpenUIFormById(UIFormId.Settings, UIGroupDefinition.Popup);
        }

        /// <summary>
        /// 打开对话面板
        /// </summary>
        public static void OpenDialogue()
        {
            UIComponent.OpenUIFormById(UIFormId.Dialogue, UIGroupDefinition.Scene);
        }

        /// <summary>
        /// 打开加载面板
        /// </summary>
        public static void OpenLoading()
        {
            UIComponent.OpenUIFormById(UIFormId.Loading, UIGroupDefinition.Top);
        }

        /// <summary>
        /// 关闭所有 UI
        /// </summary>
        public static void CloseAllUI()
        {
            UIComponent.CloseAllLoadedUIForms();
        }

        /// <summary>
        /// 关闭指定分组的 UI
        /// </summary>
        public static void CloseUIGroup(string groupName)
        {
            UIComponent.CloseUIFormsByGroup(groupName);
        }
    }
}