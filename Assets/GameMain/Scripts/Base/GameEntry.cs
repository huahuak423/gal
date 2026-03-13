//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame.Entry
{
    /// <summary>
    /// 游戏启动入口
    /// 挂载到场景中的GameObject上，负责初始化所有框架组件
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        #region 序列化配置

        [Header("基础设置")]
        [SerializeField]
        [Tooltip("游戏帧率")]
        private int m_FrameRate = 60;

        [SerializeField]
        [Tooltip("游戏速度")]
        private float m_GameSpeed = 1f;

        [SerializeField]
        [Tooltip("是否后台运行")]
        private bool m_RunInBackground = true;

        [SerializeField]
        [Tooltip("是否禁止休眠")]
        private bool m_NeverSleep = true;

        [Header("资源设置")]
        [SerializeField]
        [Tooltip("编辑器资源模式（仅编辑器有效）")]
        private bool m_EditorResourceMode = true;

        [Header("调试设置")]
        [SerializeField]
        [Tooltip("是否打印初始化日志")]
        private bool m_LogInitialization = true;

        #endregion

        #region 静态访问点

        /// <summary>
        /// 游戏入口实例
        /// </summary>
        public static GameEntry Instance { get; private set; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized { get; private set; }

        #endregion

        #region 快捷访问 - 内置组件

        public static BaseComponent Base => BuiltinEntry.Base;
        public static EventComponent Event => BuiltinEntry.Event;
        public static FsmComponent Fsm => BuiltinEntry.Fsm;
        public static ObjectPoolComponent ObjectPool => BuiltinEntry.ObjectPool;
        public static ReferencePoolComponent ReferencePool => BuiltinEntry.ReferencePool;
        public static DataNodeComponent DataNode => BuiltinEntry.DataNode;
        public static DataTableComponent DataTable => BuiltinEntry.DataTable;
        public static ConfigComponent Config => BuiltinEntry.Config;
        public static ResourceComponent Resource => BuiltinEntry.Resource;
        public static EntityComponent Entity => BuiltinEntry.Entity;
        public static UIComponent UI => BuiltinEntry.UI;
        public static SoundComponent Sound => BuiltinEntry.Sound;
        public static SceneComponent Scene => BuiltinEntry.Scene;
        public static LocalizationComponent Localization => BuiltinEntry.Localization;
        public static SettingComponent Setting => BuiltinEntry.Setting;
        public static DownloadComponent Download => BuiltinEntry.Download;
        public static NetworkComponent Network => BuiltinEntry.Network;
        public static FileSystemComponent FileSystem => BuiltinEntry.FileSystem;
        public static WebRequestComponent WebRequest => BuiltinEntry.WebRequest;
        public static DebuggerComponent Debugger => BuiltinEntry.Debugger;
        public static ProcedureComponent Procedure => BuiltinEntry.Procedure;

        #endregion

        #region 生命周期

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Log.Warning("[GameEntry] Duplicate GameEntry detected! Destroying duplicate...");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (m_LogInitialization)
            {
                Log.Info("[GameEntry] ========== Game Entry Starting ==========");
            }

            InitializeComponents();

            if (m_LogInitialization)
            {
                Log.Info("[GameEntry] ========== Game Entry Started ==========");
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            if (m_LogInitialization)
            {
                Log.Info("[GameEntry] ========== Game Entry Shutting Down ==========");
            }

            CustomEntry.Shutdown();
            BuiltinEntry.Shutdown();

            IsInitialized = false;
            Instance = null;

            if (m_LogInitialization)
            {
                Log.Info("[GameEntry] ========== Game Entry Shutdown Complete ==========");
            }
        }

        #endregion

        #region 初始化

        private void InitializeComponents()
        {
            // 1. 初始化内置组件
            BuiltinEntry.Initialize(gameObject);

            // 2. 配置BaseComponent
            ConfigureBaseComponent();

            // 3. 初始化自定义组件
            CustomEntry.Initialize(gameObject);

            // 4. 标记初始化完成
            IsInitialized = true;

            if (m_LogInitialization)
            {
                LogComponentsStatus();
            }
        }

        private void ConfigureBaseComponent()
        {
            var baseComponent = BuiltinEntry.Base;
            if (baseComponent == null)
            {
                Log.Error("[GameEntry] BaseComponent is null! Cannot configure.");
                return;
            }

            baseComponent.FrameRate = m_FrameRate;
            baseComponent.GameSpeed = m_GameSpeed;
            baseComponent.RunInBackground = m_RunInBackground;
            baseComponent.NeverSleep = m_NeverSleep;
            baseComponent.EditorResourceMode = m_EditorResourceMode;

            if (m_LogInitialization)
            {
                Log.Info($"[GameEntry] BaseComponent configured: FrameRate={m_FrameRate}, GameSpeed={m_GameSpeed}");
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 关闭游戏
        /// </summary>
        public static void ShutdownGame(ShutdownType shutdownType)
        {
            Log.Info($"[GameEntry] Shutdown game with type: {shutdownType}");
            UnityGameFramework.Runtime.GameEntry.Shutdown(shutdownType);
        }

        /// <summary>
        /// 打印所有组件状态
        /// </summary>
        public void LogComponentsStatus()
        {
            Log.Info("========================================");
            Log.Info("        GameEntry Components Status");
            Log.Info("========================================");

            BuiltinEntry.LogComponentsStatus();
            CustomEntry.LogComponentsStatus();

            Log.Info("========================================");
        }

        /// <summary>
        /// 获取自定义组件
        /// </summary>
        public static T GetCustomComponent<T>() where T : MonoBehaviour
        {
            return CustomEntry.GetCustomComponent<T>();
        }

        /// <summary>
        /// 添加自定义组件
        /// </summary>
        public static T AddCustomComponent<T>() where T : MonoBehaviour
        {
            if (Instance == null)
            {
                Log.Error("[GameEntry] Instance is null! Cannot add custom component.");
                return null;
            }
            return CustomEntry.AddCustomComponent<T>(Instance.gameObject);
        }

        #endregion
    }
}