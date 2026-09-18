//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AVGGame
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
                    m_CachedCanvas = GetComponentInChildren<Canvas>();
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

            ApplyButtonTransitions();
        }

        /// <summary>
        /// 批量为本界面所有按钮设置颜色过渡，实现亮暗变化反馈：
        /// 常态稍暗 → 悬停变亮 → 按下变暗；仅对未配置过渡的按钮生效，不覆盖美术已有的 Sprite Swap。
        /// </summary>
        private void ApplyButtonTransitions()
        {
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn == null || btn.transition != Button.Transition.None) continue;

                btn.transition = Button.Transition.ColorTint;
                var colors = btn.colors;
                colors.fadeDuration = 0.1f;
                colors.normalColor = new Color(0.82f, 0.82f, 0.82f, 1f);   // 常态：稍暗
                colors.highlightedColor = new Color(1f, 1f, 1f, 1f);       // 悬停：变亮
                colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);     // 按下：变暗
                colors.selectedColor = new Color(1f, 1f, 1f, 1f);          // 选中：亮
                colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.5f); // 禁用：灰暗
                btn.colors = colors;
            }
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
        /// 获取指定路径的游戏对象的组件
        /// </summary>
        /// <param name="path">游戏对象路径</param>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>游戏组件</returns>
        protected T GetComponentByPath<T>(string path) where T : Component
        {
            GameObject  go = GameObject.Find(path);
            if (go == null)
            {
                Log.Warning($"[UI] gameObject not found: {path}");
                return null;
            }
            return go.GetComponent<T>();
        }
        

        /// <summary>
        /// 关闭当前 UI
        /// </summary>
        protected virtual void CloseSelf()
        {
            // 防重复关闭保护：UGF 在界面回收时会把 SerialId 重置为 0，
            // 若对已关闭的界面再次调用 CloseUIForm，框架会抛出
            // "Can not find UI form info for serial id '0'" 异常，这里先行拦截。
            if (UIForm == null || UIForm.SerialId == 0)
            {
                Log.Warning($"[UI] {GetType().Name} CloseSelf 被重复调用，界面已关闭，忽略本次请求");
                return;
            }
            // 使用框架的 GameEntry 关闭 UI
            UnityGameFramework.Runtime.GameEntry.GetComponent<UIComponent>().CloseUIForm(this.UIForm);
        }

        #endregion
    }
}