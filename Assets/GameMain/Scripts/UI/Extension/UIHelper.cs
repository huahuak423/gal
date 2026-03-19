//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityGameFramework.Runtime;
using AVGGame;
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

        #region 主菜单相关

        /// <summary>
        /// 打开主菜单
        /// </summary>
        public static void OpenMainMenu()
        {
            UIComponent.OpenUIFormById(UIFormId.MainMenu, UIGroupDefinition.Main);
        }

        /// <summary>
        /// 打开存档选择界面
        /// </summary>
        /// <param name="isNewGame">是否是新游戏模式（true=新游戏，false=继续游戏）</param>
        public static void OpenArchive(bool isNewGame = false)
        {
            UIComponent.OpenUIFormById(UIFormId.Archive, UIGroupDefinition.Main, isNewGame);
        }

        /// <summary>
        /// 打开设置面板
        /// </summary>
        public static void OpenSettings()
        {
            UIComponent.OpenUIFormById(UIFormId.Settings, UIGroupDefinition.Popup);
        }

        #endregion

        #region 游戏内UI

        /// <summary>
        /// 打开对话面板
        /// </summary>
        public static void OpenDialogue()
        {
            UIComponent.OpenUIFormById(UIFormId.Dialogue, UIGroupDefinition.Scene);
        }

        /// <summary>
        /// 打开对话面板（带存档数据）
        /// </summary>
        /// <param name="saveData">存档数据</param>
        public static void OpenDialogue(object saveData)
        {
            UIComponent.OpenUIFormById(UIFormId.Dialogue, UIGroupDefinition.Scene, saveData);
        }

        /// <summary>
        /// 打开游戏内菜单
        /// </summary>
        public static void OpenMenu()
        {
            UIComponent.OpenUIFormById(UIFormId.Menu, UIGroupDefinition.Popup);
        }

        /// <summary>
        /// 打开地图界面
        /// </summary>
        public static void OpenMap()
        {
            UIComponent.OpenUIFormById(UIFormId.Map, UIGroupDefinition.Main);
        }

        #endregion

        #region 系统UI

        /// <summary>
        /// 打开加载面板
        /// </summary>
        public static void OpenLoading()
        {
            UIComponent.OpenUIFormById(UIFormId.Loading, UIGroupDefinition.Top);
        }

        #endregion

        #region 关闭UI

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

        #endregion
    }
}