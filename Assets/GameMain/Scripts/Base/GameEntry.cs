//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain.Scripts.Base
{
    /// <summary>
    /// 游戏启动入口
    /// 挂载到场景中的GameObject上，负责初始化所有框架组件
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        #region 序列化配置

        [Header("基础设置")]
        [SerializeField] private int m_FrameRate = 60;
        [SerializeField] private float m_GameSpeed = 1f;
        [SerializeField] private bool m_RunInBackground = true;
        [SerializeField] private bool m_NeverSleep = true;

        [Header("资源设置")]
        [SerializeField] private bool m_EditorResourceMode = true;

        #endregion

        #region 静态访问点

        public static GameEntry Instance { get; private set; }
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
                Debug.LogWarning("[GameEntry] Duplicate GameEntry detected!");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeComponents();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            CustomEntry.Shutdown();
            BuiltinEntry.Shutdown();

            IsInitialized = false;
            Instance = null;
        }

        #endregion

        #region 初始化

        private void InitializeComponents()
        {
            BuiltinEntry.Initialize(gameObject);
            ConfigureBaseComponent();
            CustomEntry.Initialize(gameObject);
            IsInitialized = true;
        }

        private void ConfigureBaseComponent()
        {
            var baseComponent = BuiltinEntry.Base;
            if (baseComponent == null)
            {
                Log.Error("[GameEntry] BaseComponent is null!");
                return;
            }

            baseComponent.FrameRate = m_FrameRate;
            baseComponent.GameSpeed = m_GameSpeed;
            baseComponent.RunInBackground = m_RunInBackground;
            baseComponent.NeverSleep = m_NeverSleep;
            baseComponent.EditorResourceMode = m_EditorResourceMode;
        }

        #endregion

        #region 公共方法

        public static void ShutdownGame(ShutdownType shutdownType)
        {
            UnityGameFramework.Runtime.GameEntry.Shutdown(shutdownType);
        }

        public static T GetCustomComponent<T>() where T : MonoBehaviour
        {
            return CustomEntry.GetCustomComponent<T>();
        }

        public static T AddCustomComponent<T>() where T : MonoBehaviour
        {
            if (Instance == null)
            {
                Log.Error("[GameEntry] Instance is null!");
                return null;
            }
            return CustomEntry.AddCustomComponent<T>(Instance.gameObject);
        }

        #endregion
    }
}