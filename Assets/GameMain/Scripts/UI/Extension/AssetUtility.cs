//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using AVGGame;

namespace AVGGame
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
                // 系统UI (1000-1999)
                case UIFormId.Loading: return "Loading";
                case UIFormId.MessageBox: return "MessageBoxPanel";

                // 主菜单 (2000-2999)
                case UIFormId.MainMenu: return "MainMenu";
                case UIFormId.Settings: return "GameSetting";
                case UIFormId.Archive: return "Archive";
                case UIFormId.CreateName: return "CreateName";

                // 游戏内UI (3000-3999)
                case UIFormId.Dialogue: return "Dialogue";
                case UIFormId.Menu: return "Menu";
                case UIFormId.Map: return "Map";
                case UIFormId.Information: return "Information";
                case UIFormId.Inventory: return "Inventory";

                default:
                    UnityEngine.Debug.LogWarning($"[AssetUtility] Unknown UIFormId: {uiFormId}");
                    return "UnknownUI";
            }
        }
    }
}