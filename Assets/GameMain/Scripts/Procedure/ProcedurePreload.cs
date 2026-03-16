//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AVGGame.Procedure
{
    /// <summary>
    /// 预加载流程 - 游戏启动后的第一个流程
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        #region 字段

        private bool m_PreloadComplete = false;

        #endregion

        #region 生命周期

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedurePreload] Enter");

            // 初始化 UI 环境
            InitializeUIEnvironment();

            // TODO: 在这里进行资源预加载、数据表加载等

            // 模拟加载完成
            m_PreloadComplete = true;
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_PreloadComplete)
            {
                ChangeState<ProcedureSplash>(procedureOwner);
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            Log.Info("[ProcedurePreload] Leave");
        }

        #endregion

        #region 初始化

        private void InitializeUIEnvironment()
        {
            CreateMainCanvas();
            CreateEventSystemIfNeeded();
            Log.Info("[ProcedurePreload] UI environment initialized");
        }

        private void CreateMainCanvas()
        {
            if (GameObject.Find("MainCanvas") != null)
            {
                return;
            }

            GameObject canvasGo = new GameObject("MainCanvas");

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            Object.DontDestroyOnLoad(canvasGo);

            Log.Info("[ProcedurePreload] Main Canvas created");
        }

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