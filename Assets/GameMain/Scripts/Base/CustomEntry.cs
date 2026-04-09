//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    /// <summary>
    /// 自定义组件入口
    /// 静态类，管理项目特有的自定义组件
    /// </summary>
    public static class CustomEntry
    {
        #region 字段

        private static readonly Dictionary<System.Type, MonoBehaviour> s_CustomComponents
            = new Dictionary<System.Type, MonoBehaviour>();

        private static readonly List<MonoBehaviour> s_CustomComponentList
            = new List<MonoBehaviour>();

        private static GameObject s_RootObject;

        #endregion

        #region 属性

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// 自定义组件数量
        /// </summary>
        public static int ComponentCount => s_CustomComponents.Count;

        // ========================================
        // 自定义组件属性（仿照BuiltinEntry）
        // ========================================

        /// <summary>
        /// 玩家数据组件
        /// </summary>
        public static PlayerDataComponent PlayerData { get; private set; }

        /// <summary>
        /// 存档系统组件
        /// </summary>
        public static SaveSystem SaveSystem { get; private set; }

        #endregion

        #region 初始化

        public static void Initialize(GameObject root)
        {
            if (root == null)
            {
                Debug.LogError("[CustomEntry] Root gameObject is null!");
                return;
            }

            if (IsInitialized)
            {
                Debug.LogWarning("[CustomEntry] Already initialized!");
                return;
            }

            s_RootObject = root;
            Debug.Log("[CustomEntry] Initializing custom components...");

            // ========================================
            // 在这里添加你的自定义组件
            // ========================================
            PlayerData = AddCustomComponent<PlayerDataComponent>();
            SaveSystem = AddCustomComponent<SaveSystem>();

            IsInitialized = true;
            Debug.Log($"[CustomEntry] Custom components initialized! Count: {s_CustomComponents.Count}");
        }

        public static void Shutdown()
        {
            Debug.Log("[CustomEntry] Shutting down...");

            // 反向清理
            for (int i = s_CustomComponentList.Count - 1; i >= 0; i--)
            {
                var component = s_CustomComponentList[i];
                if (component != null)
                {
                    Object.Destroy(component);
                }
            }

            s_CustomComponentList.Clear();
            s_CustomComponents.Clear();
            s_RootObject = null;

            // 清空自定义组件属性
            PlayerData = null;
            SaveSystem = null;

            IsInitialized = false;

            Debug.Log("[CustomEntry] Shutdown complete.");
        }

        #endregion

        #region 组件管理

        public static T AddCustomComponent<T>(GameObject root = null) where T : MonoBehaviour
        {
            var targetRoot = root ?? s_RootObject;
            if (targetRoot == null)
            {
                Debug.LogError("[CustomEntry] Root gameObject is null!");
                return null;
            }

            var type = typeof(T);
            if (s_CustomComponents.ContainsKey(type))
            {
                Debug.LogWarning($"[CustomEntry] Component {type.Name} already exists!");
                return GetCustomComponent<T>();
            }

            T component = targetRoot.AddComponent<T>();
            s_CustomComponents[type] = component;
            s_CustomComponentList.Add(component);

            Debug.Log($"[CustomEntry] Added: {type.Name}");
            return component;
        }

        public static T GetCustomComponent<T>() where T : MonoBehaviour
        {
            var type = typeof(T);
            if (s_CustomComponents.TryGetValue(type, out var component))
            {
                return component as T;
            }
            return null;
        }

        public static bool HasCustomComponent<T>() where T : MonoBehaviour
        {
            return s_CustomComponents.ContainsKey(typeof(T));
        }

        public static bool RemoveCustomComponent<T>() where T : MonoBehaviour
        {
            var type = typeof(T);
            if (s_CustomComponents.TryGetValue(type, out var component))
            {
                s_CustomComponents.Remove(type);
                s_CustomComponentList.Remove(component);

                if (component != null)
                {
                    Object.Destroy(component);
                }

                Debug.Log($"[CustomEntry] Removed: {type.Name}");
                return true;
            }
            return false;
        }

        public static IReadOnlyList<MonoBehaviour> GetAllCustomComponents()
        {
            return s_CustomComponentList.AsReadOnly();
        }

        #endregion

        #region 调试

        public static void LogComponentsStatus()
        {
            Debug.Log("=== Custom Components ===");
            Debug.Log($"Total: {s_CustomComponents.Count}");

            int index = 1;
            foreach (var kvp in s_CustomComponents)
            {
                string status = kvp.Value != null ? "OK" : "NULL";
                Debug.Log($"  [{index}] {kvp.Key.Name}: {status}");
                index++;
            }
        }

        #endregion
    }
}