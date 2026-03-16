//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain.Scripts.UI.Base
{
    /// <summary>
    /// UI 界面基类
    /// 所有 UI 界面都应继承此类
    /// </summary>
    public abstract class UIFormBase : UIFormLogic
    {
        #region 字段

        private Canvas m_CachedCanvas;
        private CanvasGroup m_CanvasGroup;
        private RectTransform m_RectTransform;

        #endregion

        #region 属性

        /// <summary>
        /// 获取缓存 Canvas
        /// </summary>
        public Canvas CachedCanvas
        {
            get
            {
                if (m_CachedCanvas == null)
                {
                    m_CachedCanvas = GetComponent<Canvas>();
                }
                return m_CachedCanvas;
            }
        }

        /// <summary>
        /// 获取 CanvasGroup
        /// </summary>
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (m_CanvasGroup == null)
                {
                    m_CanvasGroup = GetComponent<CanvasGroup>();
                    if (m_CanvasGroup == null)
                    {
                        m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }
                }
                return m_CanvasGroup;
            }
        }

        /// <summary>
        /// 获取 RectTransform
        /// </summary>
        public RectTransform Rect
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }
                return m_RectTransform;
            }
        }

        /// <summary>
        /// UI 层级
        /// </summary>
        public virtual int SortingOrder => 0;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Log.Info($"[UI] {GetType().Name} OnInit");
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Log.Info($"[UI] {GetType().Name} OnOpen");

            if (CachedCanvas != null)
            {
                CachedCanvas.sortingOrder = SortingOrder;
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            Log.Info($"[UI] {GetType().Name} OnClose");
        }

        protected override void OnPause()
        {
            base.OnPause();
            CanvasGroup.blocksRaycasts = false;
        }

        protected override void OnResume()
        {
            base.OnResume();
            CanvasGroup.blocksRaycasts = true;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取子物体组件
        /// </summary>
        protected T GetChild<T>(string path) where T : Component
        {
            Transform child = transform.Find(path);
            if (child == null)
            {
                Log.Warning($"[UI] Child not found: {path}");
                return null;
            }
            return child.GetComponent<T>();
        }

        /// <summary>
        /// 关闭当前 UI
        /// </summary>
        protected virtual void CloseSelf()
        {
            // 使用框架的 GameEntry 关闭 UI
            UnityGameFramework.Runtime.GameEntry.GetComponent<UIComponent>().CloseUIForm(this.UIForm);
        }

        #endregion
    }
}