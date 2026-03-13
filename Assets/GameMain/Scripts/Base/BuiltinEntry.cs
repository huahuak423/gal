//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using System.Collections.Generic;
using GameFramework.Config;
using GameFramework.DataNode;
using GameFramework.DataTable;
using GameFramework.Download;
using GameFramework.Entity;
using GameFramework.Event;
using GameFramework.FileSystem;
using GameFramework.Fsm;
using GameFramework.Localization;
using GameFramework.Network;
using GameFramework.ObjectPool;
using GameFramework.Procedure;
using GameFramework.Resource;
using GameFramework.Debugger;
using GameFramework.Scene;
using GameFramework.Setting;
using GameFramework.Sound;
using GameFramework.UI;
using GameFramework.WebRequest;
using UnityEngine;
using UnityGameFramework.Runtime;
using static GameFramework.ReferencePool;

namespace AVGGame.Entry
{
    /// <summary>
    /// 框架内置组件入口
    /// 静态类，管理所有GameFramework核心组件
    /// </summary>
    public static class BuiltinEntry
    {
        #region 组件引用

        private static BaseComponent s_BaseComponent;
        private static EventComponent s_EventComponent;
        private static FsmComponent s_FsmComponent;
        private static ObjectPoolComponent s_ObjectPoolComponent;
        private static ReferencePoolComponent s_ReferencePoolComponent;
        private static DataNodeComponent s_DataNodeComponent;
        private static DataTableComponent s_DataTableComponent;
        private static ConfigComponent s_ConfigComponent;
        private static ResourceComponent s_ResourceComponent;
        private static EditorResourceComponent s_EditorResourceComponent;
        private static EntityComponent s_EntityComponent;
        private static UIComponent s_UIComponent;
        private static SoundComponent s_SoundComponent;
        private static SceneComponent s_SceneComponent;
        private static LocalizationComponent s_LocalizationComponent;
        private static SettingComponent s_SettingComponent;
        private static DownloadComponent s_DownloadComponent;
        private static NetworkComponent s_NetworkComponent;
        private static FileSystemComponent s_FileSystemComponent;
        private static WebRequestComponent s_WebRequestComponent;
        private static DebuggerComponent s_DebuggerComponent;
        private static ProcedureComponent s_ProcedureComponent;

        private static readonly Dictionary<System.Type, GameFrameworkComponent> s_Components
            = new Dictionary<System.Type, GameFrameworkComponent>();

        #endregion

        #region 属性

        public static bool IsInitialized { get; private set; }
        public static BaseComponent Base => s_BaseComponent;
        public static EventComponent Event => s_EventComponent;
        public static FsmComponent Fsm => s_FsmComponent;
        public static ObjectPoolComponent ObjectPool => s_ObjectPoolComponent;
        public static ReferencePoolComponent ReferencePool => s_ReferencePoolComponent;
        public static DataNodeComponent DataNode => s_DataNodeComponent;
        public static DataTableComponent DataTable => s_DataTableComponent;
        public static ConfigComponent Config => s_ConfigComponent;
        public static ResourceComponent Resource => s_ResourceComponent;
        public static EditorResourceComponent EditorResource => s_EditorResourceComponent;
        public static EntityComponent Entity => s_EntityComponent;
        public static UIComponent UI => s_UIComponent;
        public static SoundComponent Sound => s_SoundComponent;
        public static SceneComponent Scene => s_SceneComponent;
        public static LocalizationComponent Localization => s_LocalizationComponent;
        public static SettingComponent Setting => s_SettingComponent;
        public static DownloadComponent Download => s_DownloadComponent;
        public static NetworkComponent Network => s_NetworkComponent;
        public static FileSystemComponent FileSystem => s_FileSystemComponent;
        public static WebRequestComponent WebRequest => s_WebRequestComponent;
        public static DebuggerComponent Debugger => s_DebuggerComponent;
        public static ProcedureComponent Procedure => s_ProcedureComponent;
        public static int ComponentCount => s_Components.Count;

        #endregion

        #region 初始化

        public static void Initialize(GameObject root)
        {
            if (root == null)
            {
                Log.Error("[BuiltinEntry] Root gameObject is null!");
                return;
            }

            if (IsInitialized)
            {
                Log.Warning("[BuiltinEntry] Already initialized!");
                return;
            }

            Log.Info("[BuiltinEntry] Initializing builtin components...");

            // 从子节点获取已存在的框架组件（预制体中已配置）
            // 核心组件
            s_BaseComponent = GetComponentInChildren<BaseComponent>(root);
            s_EventComponent = GetComponentInChildren<EventComponent>(root);
            s_FsmComponent = GetComponentInChildren<FsmComponent>(root);
            s_ObjectPoolComponent = GetComponentInChildren<ObjectPoolComponent>(root);
            s_ReferencePoolComponent = GetComponentInChildren<ReferencePoolComponent>(root);

            // 数据组件
            s_DataNodeComponent = GetComponentInChildren<DataNodeComponent>(root);
            s_DataTableComponent = GetComponentInChildren<DataTableComponent>(root);
            s_ConfigComponent = GetComponentInChildren<ConfigComponent>(root);

            // 资源组件
            s_EditorResourceComponent = root.GetComponentInChildren<EditorResourceComponent>();
            s_ResourceComponent = GetComponentInChildren<ResourceComponent>(root);

            // 游戏对象组件
            s_EntityComponent = GetComponentInChildren<EntityComponent>(root);
            s_UIComponent = GetComponentInChildren<UIComponent>(root);
            s_SoundComponent = GetComponentInChildren<SoundComponent>(root);
            s_SceneComponent = GetComponentInChildren<SceneComponent>(root);

            // 功能组件
            s_LocalizationComponent = GetComponentInChildren<LocalizationComponent>(root);
            s_SettingComponent = GetComponentInChildren<SettingComponent>(root);
            s_DownloadComponent = GetComponentInChildren<DownloadComponent>(root);
            s_NetworkComponent = GetComponentInChildren<NetworkComponent>(root);
            s_FileSystemComponent = GetComponentInChildren<FileSystemComponent>(root);
            s_WebRequestComponent = GetComponentInChildren<WebRequestComponent>(root);
            s_DebuggerComponent = GetComponentInChildren<DebuggerComponent>(root);

            // 流程组件（最后获取）
            s_ProcedureComponent = GetComponentInChildren<ProcedureComponent>(root);

            // 注册组件到字典
            RegisterComponent(s_BaseComponent);
            RegisterComponent(s_EventComponent);
            RegisterComponent(s_FsmComponent);
            RegisterComponent(s_ObjectPoolComponent);
            RegisterComponent(s_ReferencePoolComponent);
            RegisterComponent(s_DataNodeComponent);
            RegisterComponent(s_DataTableComponent);
            RegisterComponent(s_ConfigComponent);
            RegisterComponent(s_ResourceComponent);
            RegisterComponent(s_EntityComponent);
            RegisterComponent(s_UIComponent);
            RegisterComponent(s_SoundComponent);
            RegisterComponent(s_SceneComponent);
            RegisterComponent(s_LocalizationComponent);
            RegisterComponent(s_SettingComponent);
            RegisterComponent(s_DownloadComponent);
            RegisterComponent(s_NetworkComponent);
            RegisterComponent(s_FileSystemComponent);
            RegisterComponent(s_WebRequestComponent);
            RegisterComponent(s_DebuggerComponent);
            RegisterComponent(s_ProcedureComponent);

            IsInitialized = true;
            Log.Info($"[BuiltinEntry] Builtin components initialized! Total: {s_Components.Count}");
        }

        public static void Shutdown()
        {
            Log.Info("[BuiltinEntry] Shutting down...");

            s_ProcedureComponent = null;
            s_DebuggerComponent = null;
            s_WebRequestComponent = null;
            s_FileSystemComponent = null;
            s_NetworkComponent = null;
            s_DownloadComponent = null;
            s_SettingComponent = null;
            s_LocalizationComponent = null;
            s_SceneComponent = null;
            s_SoundComponent = null;
            s_UIComponent = null;
            s_EntityComponent = null;
            s_ResourceComponent = null;
            s_ConfigComponent = null;
            s_DataTableComponent = null;
            s_DataNodeComponent = null;
            s_ReferencePoolComponent = null;
            s_ObjectPoolComponent = null;
            s_FsmComponent = null;
            s_EventComponent = null;
            s_BaseComponent = null;

            s_Components.Clear();
            IsInitialized = false;

            Log.Info("[BuiltinEntry] Shutdown complete.");
        }

        private static T GetComponentInChildren<T>(GameObject root) where T : GameFrameworkComponent
        {
            // 先检查根物体
            T component = root.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            // 再检查子物体
            component = root.GetComponentInChildren<T>();
            if (component == null)
            {
                Log.Error($"[BuiltinEntry] Failed to find {typeof(T).Name} in {root.name} or its children!");
            }
            return component;
        }

        private static void RegisterComponent<T>(T component) where T : GameFrameworkComponent
        {
            if (component != null)
            {
                s_Components[typeof(T)] = component;
            }
        }

        #endregion

        #region 组件获取

        public static T GetComponent<T>() where T : GameFrameworkComponent
        {
            s_Components.TryGetValue(typeof(T), out var component);
            return component as T;
        }

        public static bool TryGetComponent<T>(out T component) where T : GameFrameworkComponent
        {
            component = GetComponent<T>();
            return component != null;
        }

        #endregion

        #region 调试

        public static void LogComponentsStatus()
        {
            Log.Info("=== Builtin Components ===");
            Log.Info($"Base: {(s_BaseComponent != null ? "OK" : "NULL")}");
            Log.Info($"Event: {(s_EventComponent != null ? "OK" : "NULL")}");
            Log.Info($"Fsm: {(s_FsmComponent != null ? "OK" : "NULL")}");
            Log.Info($"ObjectPool: {(s_ObjectPoolComponent != null ? "OK" : "NULL")}");
            Log.Info($"ReferencePool: {(s_ReferencePoolComponent != null ? "OK" : "NULL")}");
            Log.Info($"DataNode: {(s_DataNodeComponent != null ? "OK" : "NULL")}");
            Log.Info($"DataTable: {(s_DataTableComponent != null ? "OK" : "NULL")}");
            Log.Info($"Config: {(s_ConfigComponent != null ? "OK" : "NULL")}");
            Log.Info($"Resource: {(s_ResourceComponent != null ? "OK" : "NULL")}");
            Log.Info($"Entity: {(s_EntityComponent != null ? "OK" : "NULL")}");
            Log.Info($"UI: {(s_UIComponent != null ? "OK" : "NULL")}");
            Log.Info($"Sound: {(s_SoundComponent != null ? "OK" : "NULL")}");
            Log.Info($"Scene: {(s_SceneComponent != null ? "OK" : "NULL")}");
            Log.Info($"Localization: {(s_LocalizationComponent != null ? "OK" : "NULL")}");
            Log.Info($"Setting: {(s_SettingComponent != null ? "OK" : "NULL")}");
            Log.Info($"Download: {(s_DownloadComponent != null ? "OK" : "NULL")}");
            Log.Info($"Network: {(s_NetworkComponent != null ? "OK" : "NULL")}");
            Log.Info($"FileSystem: {(s_FileSystemComponent != null ? "OK" : "NULL")}");
            Log.Info($"WebRequest: {(s_WebRequestComponent != null ? "OK" : "NULL")}");
            Log.Info($"Debugger: {(s_DebuggerComponent != null ? "OK" : "NULL")}");
            Log.Info($"Procedure: {(s_ProcedureComponent != null ? "OK" : "NULL")}");
            Log.Info($"Total: {s_Components.Count}");
        }

        #endregion
    }
}