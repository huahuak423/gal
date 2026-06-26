//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;

namespace AVGGame
{
    /// <summary>
    /// 存档加载上下文 - 用于在流程切换时传递存档相关的临时数据
    ///
    /// 设计说明：
    /// 1. 这是一个静态类，用于临时存储存档加载过程中的数据
    /// 2. 数据在存档加载完成后应该被清理
    /// 3. 相比全局变量，这种设计更清晰，职责更明确
    /// 4. 放在 SaveAndLoad 文件夹中，符合模块化设计原则
    /// </summary>
    public static class SaveLoadContext
    {
        #region 字段

        // 是否正在从存档加载
        private static bool m_IsLoadingFromSave = false;

        // 当前要加载的存档槽位
        private static int m_CurrentSaveSlot = 1;

        // 当前故事名称（从存档中恢复）
        private static string m_CurrentStoryName = null;

        // 目标事件ID（如果直接进入某个事件）
        private static int m_TargetEventId = 0;

        /// <summary>
        /// 起名完成后的回调，由 ProcedureMainMenu 注册
        /// </summary>
        public static System.Action OnCreateNameComplete;

        /// <summary>
        /// 存档时的临时截图（由 DialoguePanel 在打开存档面板前捕获，ArchivePanel 保存时写入文件）
        /// </summary>
        public static Texture2D PendingScreenshot;

        #endregion

        #region 属性

        /// <summary>
        /// 是否正在从存档加载
        /// </summary>
        public static bool IsLoadingFromSave => m_IsLoadingFromSave;

        /// <summary>
        /// 当前要加载的存档槽位
        /// </summary>
        public static int CurrentSaveSlot => m_CurrentSaveSlot;

        /// <summary>
        /// 当前故事名称
        /// </summary>
        public static string CurrentStoryName => m_CurrentStoryName;

        /// <summary>
        /// 目标事件ID
        /// </summary>
        public static int TargetEventId => m_TargetEventId;

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置从存档加载的上下文
        /// </summary>
        /// <param name="slotId">存档槽位ID</param>
        /// <param name="storyName">故事名称</param>
        public static void SetSaveLoadContext(int slotId, string storyName)
        {
            m_IsLoadingFromSave = true;
            m_CurrentSaveSlot = slotId;
            m_CurrentStoryName = storyName;
            m_TargetEventId = 0;

            Debug.Log($"[SaveLoadContext] Set save load context - Slot: {slotId}, Story: {storyName}");
        }

        /// <summary>
        /// 设置直接进入事件的上下文
        /// </summary>
        /// <param name="eventId">事件ID</param>
        public static void SetEventLoadContext(int eventId)
        {
            m_IsLoadingFromSave = false;
            m_CurrentSaveSlot = 1;
            m_CurrentStoryName = null;
            m_TargetEventId = eventId;

            Debug.Log($"[SaveLoadContext] Set event load context - Event ID: {eventId}");
        }

        /// <summary>
        /// 获取当前要进入的故事或事件
        /// </summary>
        /// <returns>目标信息</returns>
        public static (bool hasTarget, string storyName, int eventId) GetTargetInfo()
        {
            if (!string.IsNullOrEmpty(m_CurrentStoryName))
            {
                return (true, m_CurrentStoryName, 0);
            }
            else if (m_TargetEventId > 0)
            {
                return (true, null, m_TargetEventId);
            }
            else
            {
                return (false, null, 0);
            }
        }

        /// <summary>
        /// 清除上下文数据
        /// </summary>
        public static void ClearContext()
        {
            m_IsLoadingFromSave = false;
            m_CurrentSaveSlot = 1;
            m_CurrentStoryName = null;
            m_TargetEventId = 0;

            Debug.Log("[SaveLoadContext] Context cleared");
        }

        /// <summary>
        /// 检查是否有有效的目标
        /// </summary>
        public static bool HasValidTarget()
        {
            return !string.IsNullOrEmpty(m_CurrentStoryName) || m_TargetEventId > 0;
        }

        #endregion
    }
}