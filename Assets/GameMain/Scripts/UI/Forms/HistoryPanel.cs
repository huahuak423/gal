//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameFramework.Resource;
using GameFramework.Sound;

namespace AVGGame
{
    /// <summary>
    /// 历史对话回顾面板
    /// 接收 List&lt;HistoryEntry&gt; 作为 userData，在滚动列表中展示。
    /// 根据有无说话人，分别使用 HistoryDialogue / HistoryNarration 模板。
    /// </summary>
    public class HistoryPanel : UIFormBase
    {
        #region 序列化字段

        [Header("滚动列表")]
        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private Transform m_ContentContainer;

        [Header("按钮")]
        [SerializeField] private Button m_ButtonClose;

        #endregion

        #region 属性

        public override int SortingOrder => 250;

        #endregion

        #region 私有字段

        // 两种条目模板（运行时从资源加载）
        private GameObject m_DialogueTemplate;
        private GameObject m_NarrationTemplate;
        private bool m_TemplatesLoaded = false;

        // 模板资源路径
        private const string c_DialogueTemplatePath = "Assets/GameMain/UI/Forms/Component/HistoryDialogue.prefab";
        private const string c_NarrationTemplatePath = "Assets/GameMain/UI/Forms/Component/HistoryNarration.prefab";

        // 按钮图标路径
        private const string c_LoveOnPath  = "Assets/GameMain/Art/UI_Common/UI/历史回顾/收藏-亮.png";
        private const string c_LoveOffPath = "Assets/GameMain/Art/UI_Common/UI/历史回顾/收藏-暗.png";
        private const string c_AudioOnPath = "Assets/GameMain/Art/UI_Common/UI/历史回顾/播放语音-亮.png";
        private const string c_AudioOffPath= "Assets/GameMain/Art/UI_Common/UI/历史回顾/播放语音-暗.png";

        // 缓存的按钮图标
        private Sprite m_LoveOnSprite;
        private Sprite m_LoveOffSprite;
        private Sprite m_AudioOnSprite;
        private Sprite m_AudioOffSprite;

        // 当前正在播放语音的按钮（用于播完时复位）
        private Button m_CurrentAudioBtn = null;

        // 缓存的数据，等模板加载完后再填充
        private List<HistoryEntry> m_PendingEntries;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 挂载组件引用
            m_ScrollRect = this.GetComponentByPath<ScrollRect>("Canvas/Background/HistoryDialoguePanel");
            m_ContentContainer = this.GetComponentByPath<Transform>("Canvas/Background/HistoryDialoguePanel/Viewport/Content");
            m_ButtonClose = this.GetComponentByPath<Button>("Canvas/Background/ButtonBack");

            // 绑定关闭按钮
            if (m_ButtonClose != null)
            {
                m_ButtonClose.onClick.AddListener(OnButtonCloseClick);

                // ====== 图片文件挂载 ======
                // m_ButtonClose.image — Canvas/Background/ButtonBack
            }

            // 加载两种条目模板
            LoadTemplates();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 接收历史记录数据
            if (userData is List<HistoryEntry> entries)
            {
                if (m_TemplatesLoaded)
                {
                    // 模板已就绪，直接填充
                    ClearEntries();
                    PopulateEntries(entries);
                    ScrollToBottom();
                }
                else
                {
                    // 模板还在加载，缓存数据等回调
                    m_PendingEntries = entries;
                }
            }

            Log.Info("[HistoryPanel] OnOpen");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            ClearEntries();
            m_PendingEntries = null;
            m_CurrentAudioBtn = null;

            // 停止语音播放
            GameEntry.Sound.GetSoundGroup("Voice")?.StopAllLoadedSounds();
        }

        #endregion

        #region 模板加载

        private void LoadTemplates()
        {
            GameEntry.Resource.LoadAsset(
                c_DialogueTemplatePath,
                typeof(GameObject),
                new LoadAssetCallbacks(
                    (assetName, asset, duration, ud) =>
                    {
                        m_DialogueTemplate = asset as GameObject;
                        Debug.Log($"[HistoryPanel] 对话模板加载成功: {assetName}");
                        CheckTemplatesReady();
                    },
                    (assetName, status, errorMessage, ud) =>
                        Debug.LogWarning($"[HistoryPanel] 对话模板加载失败: {assetName}, {errorMessage}")
                )
            );

            GameEntry.Resource.LoadAsset(
                c_NarrationTemplatePath,
                typeof(GameObject),
                new LoadAssetCallbacks(
                    (assetName, asset, duration, ud) =>
                    {
                        m_NarrationTemplate = asset as GameObject;
                        Debug.Log($"[HistoryPanel] 旁白模板加载成功: {assetName}");
                        CheckTemplatesReady();
                    },
                    (assetName, status, errorMessage, ud) =>
                        Debug.LogWarning($"[HistoryPanel] 旁白模板加载失败: {assetName}, {errorMessage}")
                )
            );

            // 加载4个按钮图标
            LoadButtonSprite(c_LoveOnPath,  sprite => m_LoveOnSprite = sprite);
            LoadButtonSprite(c_LoveOffPath, sprite => m_LoveOffSprite = sprite);
            LoadButtonSprite(c_AudioOnPath, sprite => m_AudioOnSprite = sprite);
            LoadButtonSprite(c_AudioOffPath,sprite => m_AudioOffSprite = sprite);
        }

        private void LoadButtonSprite(string path, System.Action<Sprite> onLoaded)
        {
            GameEntry.Resource.LoadAsset(
                path,
                typeof(Sprite),
                new LoadAssetCallbacks(
                    (assetName, asset, duration, ud) =>
                    {
                        Sprite sprite = asset as Sprite;
                        if (sprite != null) onLoaded(sprite);
                    },
                    (assetName, status, errorMessage, ud) =>
                        Debug.LogWarning($"[HistoryPanel] 按钮图标加载失败: {assetName}, {errorMessage}")
                )
            );
        }

        private void CheckTemplatesReady()
        {
            if (m_DialogueTemplate != null && m_NarrationTemplate != null)
            {
                m_TemplatesLoaded = true;
                Debug.Log("[HistoryPanel] 双模板加载完毕");

                // 如果 OnOpen 时数据已缓存，现在填充
                if (m_PendingEntries != null)
                {
                    ClearEntries();
                    PopulateEntries(m_PendingEntries);
                    ScrollToBottom();
                    m_PendingEntries = null;
                }
            }
        }

        #endregion

        #region 私有方法

        private void OnButtonCloseClick()
        {
            CloseSelf();
        }

        /// <summary>
        /// 清空列表条目
        /// </summary>
        private void ClearEntries()
        {
            if (m_ContentContainer == null) return;

            for (int i = m_ContentContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(m_ContentContainer.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 根据历史记录列表填充条目
        /// </summary>
        private void PopulateEntries(List<HistoryEntry> entries)
        {
            if (m_ContentContainer == null)
            {
                Log.Warning("[HistoryPanel] Content容器为空");
                return;
            }

            foreach (var entry in entries)
            {
                // 有说话人 → Dialogue 模板，无说话人 → Narration 模板
                bool hasSpeaker = !string.IsNullOrEmpty(entry.SpeakerName);
                GameObject template = hasSpeaker ? m_DialogueTemplate : m_NarrationTemplate;

                if (template == null)
                {
                    // 某个模板加载失败时，用另一个兜底
                    template = m_DialogueTemplate ?? m_NarrationTemplate;
                    if (template == null)
                    {
                        Log.Warning("[HistoryPanel] 无可用模板，跳过该条目");
                        continue;
                    }
                }

                // 实例化条目
                GameObject entryObj = Instantiate(template, m_ContentContainer);
                entryObj.SetActive(true);

                // 填充文本（子物体名：TextConstSpeaker / TextConstDialogue）
                SetText(entryObj.transform, "TextConstSpeaker", entry.SpeakerName);
                SetText(entryObj.transform, "TextConstDialogue", entry.DialogText);

                // 绑定播放语音按钮
                BindAudioButton(entryObj, entry);

                // 绑定收藏按钮
                BindLoveButton(entryObj, entry);
            }

            Log.Info($"[HistoryPanel] 填充了 {entries.Count} 条历史记录");
        }

        /// <summary>
        /// 绑定播放语音按钮
        /// </summary>
        private void BindAudioButton(GameObject entryObj, HistoryEntry entry)
        {
            var audioBtnTrans = entryObj.transform.Find("ButtonAudio");
            if (audioBtnTrans == null) return;

            var audioBtn = audioBtnTrans.GetComponent<Button>();
            if (audioBtn == null) return;

            // 没有语音路径时禁用按钮
            if (string.IsNullOrEmpty(entry.VoicePath))
            {
                audioBtn.gameObject.SetActive(false);
                return;
            }

            // 初始设为暗色图标
            SetAudioButtonSprite(audioBtn, false);

            // 捕获路径到局部变量，避免闭包引用 entry 对象
            string voicePath = entry.VoicePath;
            audioBtn.onClick.AddListener(() => OnAudioButtonClick(voicePath, audioBtn));
        }

        /// <summary>
        /// 绑定收藏按钮
        /// </summary>
        private void BindLoveButton(GameObject entryObj, HistoryEntry entry)
        {
            var loveBtnTrans = entryObj.transform.Find("ButtonLove");
            if (loveBtnTrans == null) return;

            var loveBtn = loveBtnTrans.GetComponent<Button>();
            if (loveBtn == null) return;

            // 初始设为暗色图标
            var loveImage = loveBtn.GetComponent<Image>();
            if (loveImage != null && m_LoveOffSprite != null)
            {
                loveImage.sprite = m_LoveOffSprite;
            }

            var playerData = CustomEntry.PlayerData;
            bool isFavorited = playerData != null && playerData.IsFavorite(entry);

            // 已收藏的设为亮色
            UpdateLoveButtonVisual(loveBtn, isFavorited);

            // 捕获必要数据，避免闭包问题
            string speaker = entry.SpeakerName;
            string text = entry.DialogText;
            string voice = entry.VoicePath;
            loveBtn.onClick.AddListener(() => OnLoveButtonClick(loveBtn, speaker, text, voice));
        }

        /// <summary>
        /// 播放语音按钮回调
        /// </summary>
        private void OnAudioButtonClick(string voicePath, Button btn)
        {
            if (string.IsNullOrEmpty(voicePath)) return;

            // 复位上一个正在播放的按钮
            if (m_CurrentAudioBtn != null && m_CurrentAudioBtn != btn)
            {
                SetAudioButtonSprite(m_CurrentAudioBtn, false);
            }
            m_CurrentAudioBtn = btn;

            // 切换为亮色
            SetAudioButtonSprite(btn, true);

            // 播放语音
            var voiceParams = new PlaySoundParams
            {
                Loop = false,
                Priority = 0,
                VolumeInSoundGroup = 1f,
                FadeInSeconds = 0f,
                Pitch = 1f,
                PanStereo = 0f,
                SpatialBlend = 0f,
                MaxDistance = 100f,
                DopplerLevel = 0f
            };
            GameEntry.Sound.PlaySound(voicePath, "Voice", voiceParams);
            Debug.Log($"[HistoryPanel] 回顾播放语音: {voicePath}");

            // 异步加载AudioClip获取时长，播完后切回暗色
            StartCoroutine(ResetAudioButtonAfterPlay(voicePath, btn));
        }

        /// <summary>
        /// 语音播完后将按钮切回暗色
        /// </summary>
        private IEnumerator ResetAudioButtonAfterPlay(string voicePath, Button btn)
        {
            float clipDuration = 3f; // 默认3秒兜底
            bool loaded = false;

            GameEntry.Resource.LoadAsset(
                voicePath,
                typeof(AudioClip),
                new LoadAssetCallbacks(
                    (assetName, asset, duration, ud) =>
                    {
                        AudioClip clip = asset as AudioClip;
                        if (clip != null) clipDuration = clip.length;
                        loaded = true;
                    },
                    (assetName, status, errorMessage, ud) => { loaded = true; }
                )
            );

            yield return new WaitUntil(() => loaded);
            yield return new WaitForSeconds(clipDuration);

            // 切回暗色
            SetAudioButtonSprite(btn, false);
            if (m_CurrentAudioBtn == btn)
            {
                m_CurrentAudioBtn = null;
            }
        }

        /// <summary>
        /// 收藏按钮回调（切换收藏状态）
        /// </summary>
        private void OnLoveButtonClick(Button btn, string speaker, string text, string voice)
        {
            var playerData = CustomEntry.PlayerData;
            if (playerData == null) return;

            // 构造临时条目用于匹配
            var entry = new HistoryEntry
            {
                SpeakerName = speaker,
                DialogText = text,
                VoicePath = voice
            };

            if (playerData.IsFavorite(entry))
            {
                playerData.RemoveFavorite(entry);
                UpdateLoveButtonVisual(btn, false);
            }
            else
            {
                playerData.AddFavorite(entry);
                UpdateLoveButtonVisual(btn, true);
            }
        }

        /// <summary>
        /// 更新收藏按钮视觉状态（替换图标）
        /// </summary>
        private void UpdateLoveButtonVisual(Button btn, bool isFavorited)
        {
            if (btn == null) return;

            var image = btn.GetComponent<Image>();
            if (image == null) return;

            Sprite target = isFavorited ? m_LoveOnSprite : m_LoveOffSprite;
            if (target != null)
            {
                image.sprite = target;
            }
        }

        /// <summary>
        /// 设置语音按钮图标（亮/暗）
        /// </summary>
        private void SetAudioButtonSprite(Button btn, bool isPlaying)
        {
            if (btn == null) return;

            var image = btn.GetComponent<Image>();
            if (image == null) return;

            Sprite target = isPlaying ? m_AudioOnSprite : m_AudioOffSprite;
            if (target != null)
            {
                image.sprite = target;
            }
        }

        /// <summary>
        /// 在指定父级下查找子物体并设置 Text 文本
        /// </summary>
        private void SetText(Transform parent, string childName, string text)
        {
            var child = parent.Find(childName);
            if (child != null)
            {
                var uiText = child.GetComponent<Text>();
                if (uiText != null)
                {
                    uiText.text = text;
                }
            }
        }

        /// <summary>
        /// 滚动到底部（最新一条）
        /// </summary>
        private void ScrollToBottom()
        {
            if (m_ScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                m_ScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        #endregion
    }
}
