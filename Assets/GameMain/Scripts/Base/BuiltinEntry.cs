//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    /// <summary>
    /// 框架内置组件入口
    /// 静态类，管理所有GameFramework核心组件
    /// </summary>
    public static class BuiltinEntry
    {
        #region 属性

        public static bool IsInitialized { get; private set; }

        // 核心组件
        public static BaseComponent Base { get; private set; }
        public static EventComponent Event { get; private set; }
        public static FsmComponent Fsm { get; private set; }
        public static ObjectPoolComponent ObjectPool { get; private set; }
        public static ReferencePoolComponent ReferencePool { get; private set; }

        // 数据组件
        public static DataNodeComponent DataNode { get; private set; }
        public static DataTableComponent DataTable { get; private set; }
        public static ConfigComponent Config { get; private set; }

        // 资源组件
        public static ResourceComponent Resource { get; private set; }

        // 游戏对象组件
        public static EntityComponent Entity { get; private set; }
        public static UIComponent UI { get; private set; }
        public static SoundComponent Sound { get; private set; }
        public static SceneComponent Scene { get; private set; }

        // 功能组件
        public static LocalizationComponent Localization { get; private set; }
        public static SettingComponent Setting { get; private set; }
        public static DownloadComponent Download { get; private set; }
        public static NetworkComponent Network { get; private set; }
        public static FileSystemComponent FileSystem { get; private set; }
        public static WebRequestComponent WebRequest { get; private set; }
        public static DebuggerComponent Debugger { get; private set; }

        // 流程组件
        public static ProcedureComponent Procedure { get; private set; }

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

            // 核心组件
            Base = UnityGameFramework.Runtime.GameEntry.GetComponent<BaseComponent>();
            Event = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();
            Fsm = UnityGameFramework.Runtime.GameEntry.GetComponent<FsmComponent>();
            ObjectPool = UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
            ReferencePool = UnityGameFramework.Runtime.GameEntry.GetComponent<ReferencePoolComponent>();

            // 数据组件
            DataNode = UnityGameFramework.Runtime.GameEntry.GetComponent<DataNodeComponent>();
            DataTable = UnityGameFramework.Runtime.GameEntry.GetComponent<DataTableComponent>();
            Config = UnityGameFramework.Runtime.GameEntry.GetComponent<ConfigComponent>();

            // 资源组件
            Resource = UnityGameFramework.Runtime.GameEntry.GetComponent<ResourceComponent>();

            // 游戏对象组件
            Entity = UnityGameFramework.Runtime.GameEntry.GetComponent<EntityComponent>();
            UI = UnityGameFramework.Runtime.GameEntry.GetComponent<UIComponent>();
            Sound = UnityGameFramework.Runtime.GameEntry.GetComponent<SoundComponent>();
            Scene = UnityGameFramework.Runtime.GameEntry.GetComponent<SceneComponent>();

            // 功能组件
            Localization = UnityGameFramework.Runtime.GameEntry.GetComponent<LocalizationComponent>();
            Setting = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
            Download = UnityGameFramework.Runtime.GameEntry.GetComponent<DownloadComponent>();
            Network = UnityGameFramework.Runtime.GameEntry.GetComponent<NetworkComponent>();
            FileSystem = UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
            WebRequest = UnityGameFramework.Runtime.GameEntry.GetComponent<WebRequestComponent>();
            Debugger = UnityGameFramework.Runtime.GameEntry.GetComponent<DebuggerComponent>();

            // 流程组件
            Procedure = UnityGameFramework.Runtime.GameEntry.GetComponent<ProcedureComponent>();

            // 验证关键组件
            if (Base == null) Log.Error("[BuiltinEntry] BaseComponent is null!");
            if (Procedure == null) Log.Error("[BuiltinEntry] ProcedureComponent is null!");

            IsInitialized = true;
            Log.Info("[BuiltinEntry] Initialized");
        }

        public static void Shutdown()
        {
            Base = null;
            Event = null;
            Fsm = null;
            ObjectPool = null;
            ReferencePool = null;
            DataNode = null;
            DataTable = null;
            Config = null;
            Resource = null;
            Entity = null;
            UI = null;
            Sound = null;
            Scene = null;
            Localization = null;
            Setting = null;
            Download = null;
            Network = null;
            FileSystem = null;
            WebRequest = null;
            Debugger = null;
            Procedure = null;

            IsInitialized = false;
            Log.Info("[BuiltinEntry] Shutdown");
        }

        #endregion
    }
}