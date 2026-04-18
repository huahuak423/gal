//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    /// <summary>
    /// 存档加载管理器 - 处理从存档加载后的流程进入
    ///
    /// 设计说明：
    /// 1. 这个类负责协调存档加载和流程进入
    /// 2. 提供了清晰的接口供其他模块使用
    /// 3. 使用协程确保流程的正确顺序
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        #region 单例

        private static SaveLoadManager m_Instance;

        public static SaveLoadManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    var go = new GameObject("SaveLoadManager");
                    m_Instance = go.AddComponent<SaveLoadManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("[SaveLoadManager] Instance created");
                }
                return m_Instance;
            }
        }

        #endregion

        #region 私有方法

        private void Awake()
        {
            if (m_Instance != null && m_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            m_Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 从存档加载后进入游戏流程
        /// </summary>
        /// <param name="slotId">存档槽位ID</param>
        /// <returns>协程</returns>
        public System.Collections.IEnumerator LoadAndEnterGame(int slotId)
        {
            Debug.Log($"[SaveLoadManager] Load and enter game from slot {slotId}");

            // 检查SaveLoadContext是否已经被ArchivePanel设置
            if (!SaveLoadContext.IsLoadingFromSave)
            {
                // 如果没有设置，从存档中读取故事名称
                string storyFromSave = GetStoryFromSave(slotId);
                SaveLoadContext.SetSaveLoadContext(slotId, storyFromSave);
            }
            else
            {
                Debug.Log("[SaveLoadManager] SaveLoadContext already set, using existing data");
            }

            // 等待一帧，确保上下文被设置
            yield return null;

            // 检查当前流程
            var currentProcedure = GameEntry.Procedure?.CurrentProcedure;
            if (currentProcedure != null)
            {
                Debug.Log("[SaveLoadManager] Current procedure found, entering game flow");

                // 如果是主菜单流程，直接开始游戏
                if (currentProcedure is ProcedureMainMenu mainMenu)
                {
                    mainMenu.StartGame();
                }
                // 如果已经在游戏流程中，直接处理
                else if (currentProcedure is ProcedureGame gameProcedure)
                {
                    var (hasTarget, storyName, eventId) = SaveLoadContext.GetTargetInfo();
                    if (hasTarget)
                    {
                        if (eventId > 0)
                        {
                            gameProcedure.LoadStory(eventId);
                        }
                        else if (!string.IsNullOrEmpty(storyName))
                        {
                            int storyEventId = GetEventIdFromStoryName(storyName);
                            if (storyEventId > 0)
                            {
                                gameProcedure.LoadStory(storyEventId);
                            }
                            else
                            {
                                gameProcedure.OpenMap();
                            }
                        }
                    }
                    else
                    {
                        gameProcedure.OpenMap();
                    }
                }
            }
            else
            {
                Debug.LogWarning("[SaveLoadManager] No current procedure found");
            }

            // 注意：不在这里清理上下文！
            // ProcedureGame.OnEnter 会读取 SaveLoadContext 并在自己内部调用 ClearContext()
            // 如果在这里提前清除，ProcedureGame.OnEnter 拿到的 IsLoadingFromSave 就会是 false
        }

        /// <summary>
        /// 直接进入游戏流程（不加载存档）
        /// </summary>
        /// <param name="eventId">事件ID，如果为0则打开大地图</param>
        /// <returns>协程</returns>
        public System.Collections.IEnumerator EnterGameFlow(int eventId = 0)
        {
            Debug.Log($"[SaveLoadManager] Enter game flow with event ID: {eventId}");

            if (eventId > 0)
            {
                SaveLoadContext.SetEventLoadContext(eventId);
            }
            else
            {
                SaveLoadContext.ClearContext();
            }

            yield return null;

            var currentProcedure = GameEntry.Procedure?.CurrentProcedure;
            if (currentProcedure is ProcedureGame gameProcedure)
            {
                if (eventId > 0)
                {
                    var (hasTarget, storyName, targetEventId) = SaveLoadContext.GetTargetInfo();
                    if (hasTarget)
                    {
                        if (targetEventId > 0)
                        {
                            gameProcedure.LoadStory(targetEventId);
                        }
                        else if (!string.IsNullOrEmpty(storyName))
                        {
                            int storyEventId = GetEventIdFromStoryName(storyName);
                            if (storyEventId > 0)
                            {
                                gameProcedure.LoadStory(storyEventId);
                            }
                            else
                            {
                                gameProcedure.OpenMap();
                            }
                        }
                    }
                    else
                    {
                        gameProcedure.OpenMap();
                    }
                }
                else
                {
                    gameProcedure.OpenMap();
                }
            }
            else
            {
                Debug.LogWarning("[SaveLoadManager] Not in game procedure, cannot enter game flow");
            }

            if (eventId > 0)
            {
                SaveLoadContext.ClearContext();
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从存档中获取故事名称
        /// </summary>
        private string GetStoryFromSave(int slotId)
        {
            if (CustomEntry.SaveSystem?.HasSave(slotId) ?? false)
            {
                // 临时加载获取故事名称
                if (CustomEntry.SaveSystem.Load(slotId))
                {
                    var storyName = CustomEntry.PlayerData?.currentStoryGarphName;
                    Debug.Log($"[SaveLoadManager] Story from save {slotId}: {storyName}");
                    return storyName;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据故事名称获取事件ID
        /// </summary>
        private int GetEventIdFromStoryName(string storyName)
        {
            // TODO: 实现故事名称到事件ID的映射
            // 这里可以根据实际需求实现
            Debug.Log($"[SaveLoadManager] Get event ID for story: {storyName}");
            return 1; // 临时返回默认值
        }

        #endregion
    }
}