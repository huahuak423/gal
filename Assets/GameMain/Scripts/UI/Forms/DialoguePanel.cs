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
using System.Collections.Generic;
using GameFramework.Resource;

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
        [SerializeField] private Text m_CharacterNameText;

        [Header("对话内容")]
        [SerializeField] private Text m_DialogueText;
        [SerializeField] private GameObject m_ContinueIndicator;

        [Header("背景")]
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private Button m_BackgroundImageButton;

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
        private ProcedureGame m_ProcedureGame;

        // 立绘相关
        private Image[] m_CharacterImages = new Image[3];
        private RectTransform[] m_CharacterRects = new RectTransform[3];
        private Vector2[] m_OriginalPositions = new Vector2[3];

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 挂载组件引用
            m_CharacterPortrait = this.GetComponentByPath<Image>("Canvas/Background/TextPlate/CharacterName");
            m_CharacterNameText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/CharacterName/TextConstCharacterName");
            m_DialogueText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/TextConstDialogue");
            m_BackgroundImage = this.GetComponentByPath<Image>("Canvas/Background");
            m_BackgroundImageButton = this.GetComponentByPath<Button>("Canvas/Background");
            m_ButtonMenu = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonMenu");

            // 挂载立绘组件引用 (0=Left, 1=Center, 2=Right)
            for (int i = 0; i < 3; i++)
            {
                m_CharacterImages[i] = this.GetComponentByPath<Image>($"Canvas/Background/CharacterPlate/CharacterImage{i}");
                if (m_CharacterImages[i] != null)
                {
                    m_CharacterRects[i] = m_CharacterImages[i].rectTransform;
                    m_OriginalPositions[i] = m_CharacterRects[i].anchoredPosition;
                    m_CharacterImages[i].gameObject.SetActive(false);
                }
            }

            /* 获取 ContinueIndicator (需要通过 Transform 获取 GameObject)
            Transform continueIndicatorTrans = this.GetComponentByPath<Transform>("Canvas/Background/ContinueIndicator");
            if (continueIndicatorTrans != null)
            {
                m_ContinueIndicator = continueIndicatorTrans.gameObject;
            }

            // 初始化隐藏继续提示
            if (m_ContinueIndicator != null)
            {
                m_ContinueIndicator.SetActive(false);
            }*/

            // 绑定菜单按钮事件
            if (m_ButtonMenu != null)
            {
                m_ButtonMenu.onClick.AddListener(OnMenuClick);
            }

            // 绑定背景按钮点击事件（用于继续对话）
            if (m_BackgroundImageButton != null)
            {
                m_BackgroundImageButton.onClick.AddListener(OnContinueClick);
            }

            m_ProcedureGame = (ProcedureGame)userData;
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

            // 清空文本
            if (m_DialogueText != null)
            {
                m_DialogueText.text = "";
            }

            // 隐藏继续提示
            if (m_ContinueIndicator != null)
            {
                m_ContinueIndicator.SetActive(false);
            }

            // 从流程层获取当前对话数据并显示
            ShowCurrentDialogue();
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
                //m_CharacterPortrait.gameObject.SetActive(portrait != null);
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

        #region 立绘显示

        /// <summary>
        /// 根据立绘动作JSON更新立绘显示
        /// </summary>
        private void ApplyCharacterDisplay(string characterActionsJson)
        {
            // 先隐藏所有立绘
            HideAllCharacters();

            // 没有立绘数据则直接返回
            if (string.IsNullOrEmpty(characterActionsJson))
            {
                return;
            }

            // 反序列化立绘动作列表
            RuntimeActionListWrapper wrapper = null;
            try
            {
                wrapper = JsonUtility.FromJson<RuntimeActionListWrapper>(characterActionsJson);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DialoguePanel] 立绘JSON解析失败: {e.Message}");
                return;
            }

            if (wrapper == null || wrapper.actions == null || wrapper.actions.Count == 0)
            {
                return;
            }

            // 遍历处理每个角色的立绘动作
            foreach (var charData in wrapper.actions)
            {
                if (charData == null) continue;

                int slotIndex = GetCharacterSlotIndex(charData.Position);
                if (slotIndex < 0) continue;
                if (slotIndex >= m_CharacterImages.Length || m_CharacterImages[slotIndex] == null) continue;

                switch (charData.ActionType)
                {
                    case CharacterActionType.Enter:
                    case CharacterActionType.ChangeSprite:
                        LoadCharacterSprite(charData, slotIndex);
                        break;

                    case CharacterActionType.Leave:
                        m_CharacterImages[slotIndex].gameObject.SetActive(false);
                        break;
                }
            }
        }

        /// <summary>
        /// 异步加载立绘Sprite并设置到对应位置
        /// </summary>
        private void LoadCharacterSprite(CharacterDisplayData charData, int slotIndex)
        {
            if (string.IsNullOrEmpty(charData.SpritePath))
            {
                Debug.LogWarning($"[DialoguePanel] 立绘SpritePath为空, 角色名: {charData.CharacterName}");
                return;
            }

            // 通过userData把slotIndex和偏移/缩放数据传给回调
            var callbackData = new CharacterLoadCallbackData
            {
                SlotIndex = slotIndex,
                OffsetX = charData.OffsetX,
                OffsetY = charData.OffsetY,
                Scale = charData.Scale
            };

            GameEntry.Resource.LoadAsset(
                charData.SpritePath,
                typeof(Sprite),
                new LoadAssetCallbacks(
                    OnLoadCharacterSpriteSuccess,
                    OnLoadCharacterSpriteFailure
                ),
                callbackData
            );
        }

        /// <summary>
        /// 立绘加载成功回调
        /// </summary>
        private void OnLoadCharacterSpriteSuccess(string assetName, object asset, float duration, object userData)
        {
            var callbackData = userData as CharacterLoadCallbackData;
            if (callbackData == null) return;

            int slotIndex = callbackData.SlotIndex;
            if (slotIndex < 0 || slotIndex >= m_CharacterImages.Length) return;
            if (m_CharacterImages[slotIndex] == null) return;

            Sprite sprite = asset as Sprite;
            if (sprite != null)
            {
                m_CharacterImages[slotIndex].sprite = sprite;
                m_CharacterImages[slotIndex].gameObject.SetActive(true);

                // 应用偏移和缩放
                ApplyCharacterTransform(slotIndex, callbackData.OffsetX, callbackData.OffsetY, callbackData.Scale);
            }
        }

        /// <summary>
        /// 立绘加载失败回调
        /// </summary>
        private void OnLoadCharacterSpriteFailure(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Debug.LogWarning($"[DialoguePanel] 立绘加载失败: {assetName}, 状态: {status}, 错误: {errorMessage}");
        }

        /// <summary>
        /// 应用立绘的偏移和缩放
        /// </summary>
        private void ApplyCharacterTransform(int index, float offsetX, float offsetY, float scale)
        {
            if (index < 0 || index >= m_CharacterRects.Length) return;
            if (m_CharacterRects[index] == null) return;

            var rect = m_CharacterRects[index];
            rect.anchoredPosition = m_OriginalPositions[index] + new Vector2(offsetX, offsetY);
            rect.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// 隐藏所有立绘
        /// </summary>
        private void HideAllCharacters()
        {
            for (int i = 0; i < m_CharacterImages.Length; i++)
            {
                if (m_CharacterImages[i] != null)
                {
                    m_CharacterImages[i].gameObject.SetActive(false);
                    m_CharacterImages[i].sprite = null;

                    // 重置偏移和缩放
                    if (m_CharacterRects[i] != null)
                    {
                        m_CharacterRects[i].anchoredPosition = m_OriginalPositions[i];
                        m_CharacterRects[i].localScale = Vector3.one;
                    }
                }
            }
        }

        /// <summary>
        /// 将 CharacterPosition 枚举映射为立绘 slot 数组索引
        /// </summary>
        private int GetCharacterSlotIndex(CharacterPosition pos)
        {
            switch (pos)
            {
                case CharacterPosition.Left:   return 0;
                case CharacterPosition.Center:  return 1;
                case CharacterPosition.Right:   return 2;
                default: return -1; // EX1~EX4 暂不支持
            }
        }

        /// <summary>
        /// 异步加载回调的传递数据
        /// </summary>
        private class CharacterLoadCallbackData
        {
            public int SlotIndex;
            public float OffsetX;
            public float OffsetY;
            public float Scale;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从流程层获取数据并显示当前对话
        /// </summary>
        private void ShowCurrentDialogue()
        {
            if (m_ProcedureGame == null)
            {
                Debug.Log("[DialoguePanel] m_ProcedureGame 为空");
                return;
            }

            DialogueDisplayData data = m_ProcedureGame.GetCurrentDialogue();

            if (data == null)
            {
                Debug.Log("[DialoguePanel] 获取对话数据失败");
                return;
            }

            Log.Info($"[DialoguePanel] 显示对话 - 说话人: {data.SpeakerName}, 文本: {data.DialogText}");

            // 应用立绘
            ApplyCharacterDisplay(data.CharacterActionsJson);

            // 调用已有的 SetDialogue 方法显示
            SetDialogue(data.SpeakerName, data.DialogText);
        }

        /// <summary>
        /// 点击继续，进入下一条对话
        /// </summary>
        public void OnContinueClick()
        {
            // 如果正在打字，先跳过打字效果
            if (m_IsTyping)
            {
                SkipTypewriter();
                return;
            }

            if (m_ProcedureGame == null) return;

            // 获取下一条对话
            DialogueDisplayData nextData = m_ProcedureGame.GoToNextDialogue();

            if (nextData == null)
            {
                // 剧情已结束，流程层会处理返回大地图
                Log.Info("[DialoguePanel] 剧情结束");
                return;
            }

            // 应用立绘
            ApplyCharacterDisplay(nextData.CharacterActionsJson);

            // 显示下一条对话
            SetDialogue(nextData.SpeakerName, nextData.DialogText);
        }

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
