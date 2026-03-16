//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

namespace GameMain.Scripts.UI.Base
{
    /// <summary>
    /// UI 分组定义
    /// 需要在 GameFramework 的 UIComponent 中配置对应的分组
    /// </summary>
    public static class UIGroupDefinition
    {
        /// <summary>
        /// 底层 UI（背景）
        /// </summary>
        public const string Background = "Background";

        /// <summary>
        /// 场景 UI（对话框、剧情）
        /// </summary>
        public const string Scene = "Scene";

        /// <summary>
        /// 主界面 UI（菜单、HUD）
        /// </summary>
        public const string Main = "Main";

        /// <summary>
        /// 弹窗 UI（提示、确认框）
        /// </summary>
        public const string Popup = "Popup";

        /// <summary>
        /// 顶层 UI（Loading、Toast）
        /// </summary>
        public const string Top = "Top";
    }
}