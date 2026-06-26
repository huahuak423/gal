//------------------------------------------------------------
// Game Framework
// AVG Game Project
// CG视频播放面板
//------------------------------------------------------------

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityGameFramework.Runtime;
using AVGGame;

namespace AVGGame
{
    /// <summary>
    /// CG视频播放面板
    /// 使用 Unity VideoPlayer + RawImage(RenderTexture) 播放全屏CG
    /// 视频文件放在 StreamingAssets/CG/ 目录下
    ///
    /// 预制体结构要求：
    /// CG (UIForm根)
    /// └── Canvas
    ///     └── Background          (全屏 RawImage，用于显示视频画面)
    ///     └── ButtonSkip          (跳过按钮，可选)
    /// </summary>
    public class CGPanel : UIFormBase
    {
        #region 字段

        private VideoPlayer m_VideoPlayer;
        private RawImage m_RawImage;
        private Button m_SkipButton;
        private RenderTexture m_RenderTexture;

        private CGPlayData m_PlayData;
        private bool m_HasCompleted = false;

        #endregion

        #region 属性

        /// <summary>
        /// 最高层级，覆盖所有UI
        /// </summary>
        public override int SortingOrder => 500;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_RawImage = this.GetComponentByPath<RawImage>("Canvas/Background");
            m_SkipButton = this.GetComponentByPath<Button>("Canvas/ButtonSkip");

            // 创建 VideoPlayer（运行时添加，预制体上不需要手动挂）
            m_VideoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (m_VideoPlayer == null)
            {
                m_VideoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
            m_VideoPlayer.playOnAwake = false;
            m_VideoPlayer.isLooping = false;
            m_VideoPlayer.skipOnDrop = true;
            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

            // 绑定跳过按钮
            if (m_SkipButton != null)
            {
                m_SkipButton.onClick.AddListener(OnSkipClick);
            }

            // ====== 图片文件挂载 ======
            // m_SkipButton.image — Canvas/ButtonSkip
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_HasCompleted = false;

            if (userData is CGPlayData playData)
            {
                m_PlayData = playData;
            }
            else
            {
                Log.Warning("[CGPanel] 未收到 CGPlayData，关闭面板");
                CloseSelf();
                return;
            }

            // 跳过按钮显隐
            if (m_SkipButton != null)
            {
                m_SkipButton.gameObject.SetActive(m_PlayData.CanSkip);
            }

            // 开始播放
            PlayVideo(m_PlayData);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            // 停止播放
            if (m_VideoPlayer != null)
            {
                m_VideoPlayer.loopPointReached -= OnVideoFinished;
                m_VideoPlayer.Stop();
            }

            // 释放 RenderTexture
            if (m_RenderTexture != null)
            {
                if (m_RawImage != null) m_RawImage.texture = null;
                m_RenderTexture.Release();
                Destroy(m_RenderTexture);
                m_RenderTexture = null;
            }

            // 触发完成回调（只触发一次）
            if (!m_HasCompleted)
            {
                m_HasCompleted = true;
                m_PlayData?.OnComplete?.Invoke();
            }

            m_PlayData = null;

            base.OnClose(isShutdown, userData);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 播放视频
        /// </summary>
        private void PlayVideo(CGPlayData data)
        {
            if (string.IsNullOrEmpty(data.VideoFileName))
            {
                Log.Warning("[CGPanel] 视频文件名为空，关闭面板");
                CloseSelf();
                return;
            }

            // 构建 StreamingAssets 路径
            string fullPath = Path.Combine(Application.streamingAssetsPath, "CG", data.VideoFileName);

            Log.Info($"[CGPanel] 播放CG视频: {fullPath}");

            // 创建 RenderTexture
            int width = Screen.width;
            int height = Screen.height;
            m_RenderTexture = new RenderTexture(width, height, 0);
            m_RawImage.texture = m_RenderTexture;

            // 配置 VideoPlayer
            m_VideoPlayer.targetTexture = m_RenderTexture;
            m_VideoPlayer.url = fullPath;
            m_VideoPlayer.loopPointReached += OnVideoFinished;

            // 开始播放
            m_VideoPlayer.Prepare();
            m_VideoPlayer.Play();
        }

        /// <summary>
        /// 视频播放完毕回调
        /// </summary>
        private void OnVideoFinished(VideoPlayer vp)
        {
            Log.Info("[CGPanel] CG视频播放完毕");
            CloseSelf();
        }

        /// <summary>
        /// 跳过按钮
        /// </summary>
        private void OnSkipClick()
        {
            Log.Info("[CGPanel] 玩家跳过CG");
            CloseSelf();
        }

        #endregion
    }
}
