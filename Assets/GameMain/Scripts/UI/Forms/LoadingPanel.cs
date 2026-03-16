//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityGameFramework.Runtime;
using GameMain.Scripts.UI.Base;

namespace GameMain.Scripts.UI.Forms
{
    /// <summary>
    /// 加载面板
    /// </summary>
    public class LoadingPanel : UIFormBase
    {
        #region 序列化字段

        [Header("进度条")]
        [SerializeField] private Slider m_ProgressSlider;
        [SerializeField] private Image m_ProgressFillImage;

        [Header("文本")]
        [SerializeField] private TextMeshProUGUI m_ProgressText;
        [SerializeField] private TextMeshProUGUI m_TipText;

        [Header("动画")]
        [SerializeField] private Image m_LoadingIcon;

        #endregion

        #region 属性

        public override int SortingOrder => 1000;

        #endregion

        #region 私有字段

        private float m_TargetProgress = 0f;
        private float m_CurrentProgress = 0f;
        private float m_SmoothSpeed = 5f;

        #endregion

        #region 生命周期

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_CurrentProgress = 0f;
            m_TargetProgress = 0f;
            UpdateProgressUI(0f);
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // 平滑进度条
            if (Mathf.Abs(m_CurrentProgress - m_TargetProgress) > 0.001f)
            {
                m_CurrentProgress = Mathf.Lerp(m_CurrentProgress, m_TargetProgress, m_SmoothSpeed * elapseSeconds);
                UpdateProgressUI(m_CurrentProgress);
            }

            // 加载图标旋转
            if (m_LoadingIcon != null)
            {
                m_LoadingIcon.transform.Rotate(0, 0, -180f * elapseSeconds);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置加载进度
        /// </summary>
        /// <param name="progress">0.0 - 1.0</param>
        public void SetProgress(float progress)
        {
            m_TargetProgress = Mathf.Clamp01(progress);
        }

        /// <summary>
        /// 设置提示文本
        /// </summary>
        public void SetTip(string tip)
        {
            if (m_TipText != null)
            {
                m_TipText.text = tip;
            }
        }

        /// <summary>
        /// 立即完成进度
        /// </summary>
        public void ForceComplete()
        {
            m_CurrentProgress = 1f;
            m_TargetProgress = 1f;
            UpdateProgressUI(1f);
        }

        #endregion

        #region 私有方法

        private void UpdateProgressUI(float progress)
        {
            if (m_ProgressSlider != null)
            {
                m_ProgressSlider.value = progress;
            }

            if (m_ProgressText != null)
            {
                m_ProgressText.text = $"{(int)(progress * 100)}%";
            }
        }

        #endregion
    }
}