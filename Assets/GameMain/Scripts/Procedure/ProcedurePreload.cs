//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameMain.Scripts.Base;

namespace AVGGame.Procedure
{
    /// <summary>
    /// 预加载流程 - 游戏启动后的第一个流程
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        #region 字段

        private float m_LoadProgress = 0f;
        private bool m_PreloadComplete = false;

        #endregion

        #region 生命周期

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedurePreload] Enter preload procedure");

            // 初始化 UI 环境
            InitializeUIEnvironment();

            // TODO: 在这里进行资源预加载、数据表加载等

            // 模拟加载完成（实际项目中替换为真实加载逻辑）
            m_PreloadComplete = true;
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_PreloadComplete)
            {
                // 加载完成后切换到启动画面流程
                ChangeState<ProcedureSplash>(procedureOwner);
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            Log.Info("[ProcedurePreload] Leave preload procedure");
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化 UI 环境（Canvas 和 EventSystem）
        /// 框架会自动创建 InstanceRoot，我们只需要确保有 Canvas 和 EventSystem
        /// </summary>
        private void InitializeUIEnvironment()
        {
            // 创建主 Canvas（用于显示 UI）
            CreateMainCanvas();

            // 创建 EventSystem（如果场景中没有）
            CreateEventSystemIfNeeded();

            Log.Info("[ProcedurePreload] UI environment initialized");
        }

        /// <summary>
        /// 创建主 Canvas
        /// </summary>
        private void CreateMainCanvas()
        {
            // 检查是否已存在 Canvas
            if (GameObject.Find("MainCanvas") != null)
            {
                Log.Info("[ProcedurePreload] Main Canvas already exists");
                return;
            }

            // 创建 Canvas
            GameObject canvasGo = new GameObject("MainCanvas");

            // 配置 Canvas
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            // 配置 CanvasScaler
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // 添加 GraphicRaycaster
            canvasGo.AddComponent<GraphicRaycaster>();

            // 设置为 DontDestroyOnLoad
            Object.DontDestroyOnLoad(canvasGo);

            Log.Info("[ProcedurePreload] Main Canvas created");
        }

        /// <summary>
        /// 创建 EventSystem（如果需要）
        /// </summary>
        private void CreateEventSystemIfNeeded()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            Log.Info("[ProcedurePreload] EventSystem created");
        }

        #endregion
    }
}
