//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityGameFramework.Runtime;
using AVGGame;
using System.Collections;

namespace AVGGame
{
    /// <summary>
    /// 对话面板 - AVG 游戏核心 UI
    /// </summary>
    public class DialoguePanel : UIFormBase
    {
        #region 序列化字段

        [Header("角色信息")]
        [SerializeField] private Image m_CharacterPortrait;
        [SerializeField] private TextMeshProUGUI m_CharacterNameText;

        [Header("对话内容")]
        [SerializeField] private TextMeshProUGUI m_DialogueText;
        [SerializeField] private GameObject m_ContinueIndicator;

        [Header("背景")]
        [SerializeField] private Image m_BackgroundImage;

        [Header("菜单按钮")]
        [SerializeField] private Button m_ButtonMenu;

        #endregion

        #region 属性

        public override int SortingOrder => 0;

        #endregion

        #region 私有字段

        private Coroutine m_TypeWriterCoroutine;
        private bool m_IsTyping = false;
        private string m_CurrentText = "";
        private System.Action m_OnComplete;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 初始化隐藏继续提示
            if (m_ContinueIndicator != null)
            {
                m_ContinueIndicator.SetActive(false);
            }

            // 绑定菜单按钮事件
            if (m_ButtonMenu != null)
            {
                m_ButtonMenu.onClick.AddListener(OnMenuClick);
            }
        }

        /// <summary>
        /// 打开游戏内菜单
        /// </summary>
        private void OnMenuClick()
        {
            Log.Info("[DialoguePanel] Menu clicked");
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Menu),
                UIGroupDefinition.Popup,
                Constant.AssetPriority.UIAsset,
                null
            );
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 绑定点击事件
            if (m_DialogueText != null)
            {
                m_DialogueText.text = "";
            }

            // 隐藏继续提示
            if (m_ContinueIndicator != null)
            {
                m_ContinueIndicator.SetActive(false);
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            // 停止打字机效果
            if (m_TypeWriterCoroutine != null)
            {
                StopCoroutine(m_TypeWriterCoroutine);
                m_TypeWriterCoroutine = null;
            }

            m_OnComplete = null;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置对话内容
        /// </summary>
        /// <param name="speakerName">说话者名称</param>
        /// <param name="dialogueText">对话文本</param>
        /// <param name="portrait">角色头像（可选）</param>
        public void SetDialogue(string speakerName, string dialogueText, Sprite portrait = null)
        {
            // 设置角色名
            if (m_CharacterNameText != null)
            {
                m_CharacterNameText.text = speakerName;
            }

            // 设置头像
            if (m_CharacterPortrait != null)
            {
                m_CharacterPortrait.sprite = portrait;
                m_CharacterPortrait.gameObject.SetActive(portrait != null);
            }

            // 打字机效果显示文本
            ShowTextWithTypewriter(dialogueText);
        }

        /// <summary>
        /// 设置背景图片
        /// </summary>
        public void SetBackground(Sprite background)
        {
            if (m_BackgroundImage != null)
            {
                m_BackgroundImage.sprite = background;
            }
        }

        /// <summary>
        /// 设置完成回调
        /// </summary>
        public void SetCompleteCallback(System.Action onComplete)
        {
            m_OnComplete = onComplete;
        }

        /// <summary>
        /// 跳过打字机效果，直接显示全部文本
        /// </summary>
        public void SkipTypewriter()
        {
            if (m_IsTyping && m_TypeWriterCoroutine != null)
            {
                StopCoroutine(m_TypeWriterCoroutine);
                m_TypeWriterCoroutine = null;
                m_IsTyping = false;

                // 直接显示全部文本
                if (m_DialogueText != null)
                {
                    m_DialogueText.text = m_CurrentText;
                }

                // 显示继续提示
                if (m_ContinueIndicator != null)
                {
                    m_ContinueIndicator.SetActive(true);
                }
            }
            else if (!m_IsTyping)
            {
                // 如果已经显示完毕，触发完成回调
                m_OnComplete?.Invoke();
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 打字机效果显示文本
        /// </summary>
        private void ShowTextWithTypewriter(string text)
        {
            m_CurrentText = text;

            if (m_TypeWriterCoroutine != null)
            {
                StopCoroutine(m_TypeWriterCoroutine);
            }

            m_TypeWriterCoroutine = StartCoroutine(TypewriterRoutine(text));
        }

        /// <summary>
        /// 打字机协程
        /// </summary>
        private IEnumerator TypewriterRoutine(string text)
        {
            m_IsTyping = true;

            if (m_DialogueText != null)
            {
                m_DialogueText.text = "";
            }

            if (m_ContinueIndicator != null)
            {
                m_ContinueIndicator.SetActive(false);
            }

            float typeSpeed = 0.03f; // 每个字符的显示间隔

            for (int i = 0; i < text.Length; i++)
            {
                if (m_DialogueText != null)
                {
                    m_DialogueText.text += text[i];
                }
                yield return new WaitForSeconds(typeSpeed);
            }

            m_IsTyping = false;

            // 显示继续提示
            if (m_ContinueIndicator != null)
            {
                m_ContinueIndicator.SetActive(true);
            }

            m_TypeWriterCoroutine = null;
        }

        #endregion
    }
}