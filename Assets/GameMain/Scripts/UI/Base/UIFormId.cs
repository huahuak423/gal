//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

namespace GameMain.Scripts.UI.Base
{
    /// <summary>
    /// UI 界面 ID 定义
    /// </summary>
    public static class UIFormId
    {
        // 系统UI (1000-1999)
        public const int Loading = 1000;
        public const int MessageBox = 1001;

        // 主菜单 (2000-2999)
        public const int MainMenu = 2000;
        public const int Settings = 2001;
        public const int Archive = 2002;        // 存档选择界面

        // 游戏内UI (3000-3999)
        public const int Dialogue = 3000;
        public const int Choice = 3001;
        public const int Menu = 3002;           // 游戏内菜单
        public const int Map = 3003;            // 地图界面
    }
}