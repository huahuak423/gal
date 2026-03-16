//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GameMain.Scripts.UI.Extension
{
    /// <summary>
    /// Toast 提示工具
    /// 显示简短的消息提示
    /// </summary>
    public static class Toast
    {
        private static ToastInstance s_Instance;

        /// <summary>
        /// 初始化 Toast 系统
        /// </summary>
        public static void Initialize(Transform parent)
        {
            if (s_Instance == null)
            {
                var go = new GameObject("ToastContainer");
                go.transform.SetParent(parent, false);
                s_Instance = go.AddComponent<ToastInstance>();
            }
        }

        /// <summary>
        /// 显示 Toast 消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="duration">显示时长（秒）</param>
        public static void Show(string message, float duration = 2f)
        {
            if (s_Instance == null)
            {
                Debug.LogWarning("[Toast] Toast system not initialized. Call Toast.Initialize() first.");
                return;
            }
            s_Instance.ShowToast(message, duration);
        }

        /// <summary>
        /// 显示成功提示
        /// </summary>
        public static void ShowSuccess(string message, float duration = 2f)
        {
            Show($"<color=green>{message}</color>", duration);
        }

        /// <summary>
        /// 显示错误提示
        /// </summary>
        public static void ShowError(string message, float duration = 3f)
        {
            Show($"<color=red>{message}</color>", duration);
        }

        /// <summary>
        /// 显示警告提示
        /// </summary>
        public static void ShowWarning(string message, float duration = 2.5f)
        {
            Show($"<color=yellow>{message}</color>", duration);
        }
    }

    /// <summary>
    /// Toast 实例 - 管理实际显示
    /// </summary>
    internal class ToastInstance : MonoBehaviour
    {
        private CanvasGroup m_CanvasGroup;
        private TextMeshProUGUI m_Text;
        private Coroutine m_CurrentCoroutine;
        private readonly float m_FadeDuration = 0.3f;

        private void Awake()
        {
            // 创建 CanvasGroup
            m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            m_CanvasGroup.alpha = 0f;
            m_CanvasGroup.blocksRaycasts = false;

            // 创建背景
            var backgroundGo = new GameObject("Background");
            backgroundGo.transform.SetParent(transform, false);
            var bgRect = backgroundGo.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.2f);
            bgRect.anchorMax = new Vector2(0.5f, 0.2f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(400, 60);

            var bgImage = backgroundGo.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);

            // 创建文本
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(backgroundGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 10);
            textRect.offsetMax = new Vector2(-20, -10);

            m_Text = textGo.AddComponent<TextMeshProUGUI>();
            m_Text.alignment = TextAlignmentOptions.Center;
            m_Text.fontSize = 24;
            m_Text.color = Color.white;
        }

        public void ShowToast(string message, float duration)
        {
            if (m_CurrentCoroutine != null)
            {
                StopCoroutine(m_CurrentCoroutine);
            }

            m_Text.text = message;
            m_CurrentCoroutine = StartCoroutine(ShowCoroutine(duration));
        }

        private IEnumerator ShowCoroutine(float duration)
        {
            // 淡入
            float elapsed = 0f;
            while (elapsed < m_FadeDuration)
            {
                elapsed += Time.deltaTime;
                m_CanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / m_FadeDuration);
                yield return null;
            }
            m_CanvasGroup.alpha = 1f;

            // 等待
            yield return new WaitForSeconds(duration);

            // 淡出
            elapsed = 0f;
            while (elapsed < m_FadeDuration)
            {
                elapsed += Time.deltaTime;
                m_CanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / m_FadeDuration);
                yield return null;
            }
            m_CanvasGroup.alpha = 0f;

            m_CurrentCoroutine = null;
        }
    }
}