//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameFramework.Resource;

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
            }

            Log.Info($"[HistoryPanel] 填充了 {entries.Count} 条历史记录");
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
