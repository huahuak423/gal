//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameMain.Scripts.UI.Base;

namespace GameMain.Scripts.UI.Extension
{
    /// <summary>
    /// UI 资源工具类
    /// </summary>
    public static class AssetUtility
    {
        private const string UIFormPath = "Assets/GameMain/UI/Forms/";

        /// <summary>
        /// 根据 UIFormId 获取资源路径
        /// </summary>
        public static string GetUIFormAsset(int uiFormId)
        {
            string formName = GetUIFormName(uiFormId);
            return $"{UIFormPath}{formName}.prefab";
        }

        /// <summary>
        /// 根据 UIFormId 获取 UI 名称
        /// </summary>
        public static string GetUIFormName(int uiFormId)
        {
            switch (uiFormId)
            {
                // 系统UI
                case UIFormId.Loading: return "LoadingPanel";
                case UIFormId.MessageBox: return "MessageBoxPanel";

                // 主菜单
                case UIFormId.MainMenu: return "MainMenu";
                case UIFormId.Settings: return "Settings";

                // 游戏内UI
                case UIFormId.Dialogue: return "DialoguePanel";
                case UIFormId.Choice: return "ChoicePanel";
                

                default:
                    UnityEngine.Debug.LogWarning($"[AssetUtility] Unknown UIFormId: {uiFormId}");
                    return "UnknownUI";
            }
        }
    }
}