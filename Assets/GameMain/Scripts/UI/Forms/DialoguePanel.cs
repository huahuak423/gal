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

        [Header("选项面板")]
        [SerializeField] private Transform m_SelectPanel;
        [SerializeField] private Transform m_ChoicesContainer;
        [SerializeField] private Button m_ChoiceButton1;
        [SerializeField] private Button m_ChoiceButton2;
        [SerializeField] private Button m_ChoiceButton3;
        [SerializeField] private Button m_ChoiceButton4;
        [SerializeField] private Button m_ChoiceButton5;
        [SerializeField] private Text m_ChoiceButton1Text;
        [SerializeField] private Text m_ChoiceButton2Text;
        [SerializeField] private Text m_ChoiceButton3Text;
        [SerializeField] private Text m_ChoiceButton4Text;
        [SerializeField] private Text m_ChoiceButton5Text;

        [Header("背景")]
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private Button m_BackgroundImageButton;

        [Header("菜单按钮")]
        [SerializeField] private Button m_ButtonMenu;
        [SerializeField] private Button m_ButtonSpeedUp;
        [SerializeField] private Button m_ButtonHistory;
        [SerializeField] private Button m_ButtonHide;
        [SerializeField] private Button m_ButtonInformation;
        [SerializeField] private Button m_ButtonSave;
        [SerializeField] private Button m_ButtonAuto;

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

        // 选项相关
        private List<Button> m_ChoiceButtons = new List<Button>();
        private GameObject m_ChoiceButtonPrefab;

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
            m_ButtonSpeedUp = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonSpeedUp");
            m_ButtonHistory = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonHistory");
            m_ButtonHide = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonHide");
            m_ButtonInformation = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonInformation");
            m_ButtonSave = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonSave");
            m_ButtonAuto= this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonAuto");
            m_ChoiceButton1 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button1");
            m_ChoiceButton2 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button2");
            m_ChoiceButton3 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button3");
            m_ChoiceButton4 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button4");
            m_ChoiceButton5 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button5");
            m_ChoiceButton1Text = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button1/TextOfSelection");
            m_ChoiceButton2Text = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button2/TextOfSelection");
            m_ChoiceButton3Text = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button3/TextOfSelection");
            m_ChoiceButton4Text = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button4/TextOfSelection");
            m_ChoiceButton5Text = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button5/TextOfSelection");

            // 挂载选项面板组件引用
            m_SelectPanel = this.GetComponentByPath<Transform>("Canvas/Background/SelectPanel");
            m_ChoicesContainer = this.GetComponentByPath<Transform>("Canvas/Background/SelectPanel/ChoicesContainer");

            Debug.Log($"[DialoguePanel] OnInit - SelectPanel: {m_SelectPanel != null}, ChoicesContainer: {m_ChoicesContainer != null}");
            if (m_SelectPanel != null)
            {
                Debug.Log($"[DialoguePanel] OnInit - SelectPanel path: {m_SelectPanel.name}, ActiveSelf: {m_SelectPanel.gameObject.activeSelf}");
            }

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

            // 绑定情报按钮事件
            if (m_ButtonInformation != null)
            {
                m_ButtonInformation.onClick.AddListener(OnButtonInformationClick);
            }

            // 绑定存档按钮事件
            if (m_ButtonSave != null)
            {
                m_ButtonSave.onClick.AddListener(OnSaveClick);
            }

            // 绑定背景按钮点击事件（用于继续对话）
            if (m_BackgroundImageButton != null)
            {
                m_BackgroundImageButton.onClick.AddListener(OnContinueClick);
            }

    
            // 初始化隐藏选项面板
            if (m_SelectPanel != null)
            {
                m_SelectPanel.gameObject.SetActive(false);
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
                m_ProcedureGame
            );
        }

        /// <summary>
        /// 打开情报界面
        /// </summary>
        private void OnButtonInformationClick()
        {
            Log.Info("[DialoguePanel] ButtonInformation clicked");
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Information),
                UIGroupDefinition.Popup,
                Constant.AssetPriority.UIAsset,
                m_ProcedureGame
            );
        }

        /// <summary>
        /// 打开存档界面（保存模式）
        /// </summary>
        private void OnSaveClick()
        {
            Log.Info("[DialoguePanel] Save clicked");
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Archive),
                UIGroupDefinition.Popup,
                Constant.AssetPriority.UIAsset,
                ArchivePanel.ArchiveMode.Save
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

            // 清理选项按钮事件（不销毁按钮，因为它们是插件的一部分）
            foreach (var button in m_ChoiceButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
            m_ChoiceButtons.Clear();

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

        #region 选项处理

        /// <summary>
        /// 解析选项JSON数据
        /// </summary>
        private List<RuntimeChoiceData> ParseChoicesJson(string choicesJson)
        {
            if (string.IsNullOrEmpty(choicesJson))
            {
                Debug.LogWarning("[DialoguePanel] 选项JSON为空");
                return null;
            }

            try
            {
                RuntimeChoiceListWrapper wrapper = JsonUtility.FromJson<RuntimeChoiceListWrapper>(choicesJson);
                if (wrapper != null && wrapper.Choices != null)
                {
                    Log.Info($"[DialoguePanel] 解析成功 {wrapper.Choices.Count} 个选项");
                    return wrapper.Choices;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialoguePanel] 选项JSON解析失败: {e.Message}");
                return null;
            }

            return null;
        }

        /// <summary>
        /// 选项选择回调
        /// </summary>
        private void OnChoiceMade(int nextDialogueId)
        {
            Log.Info($"[DialoguePanel] 选项已选择，跳转到对话ID: {nextDialogueId}");

            if (m_ProcedureGame == null)
            {
                Debug.LogError("[DialoguePanel] m_ProcedureGame 为空，无法处理选项选择");
                return;
            }

            // 设置下一对话ID
            m_ProcedureGame.SetNextDialogueId(nextDialogueId);

            // 延迟一小帧后显示下一句对话
            StartCoroutine(ShowNextDialogueAfterChoice());
        }

        /// <summary>
        /// 选择后延迟显示下一句对话
        /// </summary>
        private System.Collections.IEnumerator ShowNextDialogueAfterChoice()
        {
            yield return null; // 等待一帧，确保UI更新完成

            // 清空对话文本
            if (m_DialogueText != null)
            {
                m_DialogueText.text = "";
            }

            // 显示下一句对话
            ShowCurrentDialogue();
        }

        /// <summary>
        /// 显示选项面板
        /// </summary>
        private void ShowChoicesPanel(List<RuntimeChoiceData> choicesData)
        {
            Debug.Log($"[DialoguePanel] === ShowChoicesPanel 开始 ===");
            Debug.Log($"[DialoguePanel] m_SelectPanel: {m_SelectPanel != null}");

            if (m_SelectPanel == null)
            {
                Debug.LogError("[DialoguePanel] SelectPanel not found");
                return;
            }

            Log.Info($"[DialoguePanel] 显示选项面板，共 {choicesData.Count} 个选项");

            // 检查选项面板的初始状态
            Debug.Log($"[DialoguePanel] 选项面板初始状态 - ActiveSelf: {m_SelectPanel.gameObject.activeSelf}, ActiveInHierarchy: {m_SelectPanel.gameObject.activeInHierarchy}");

            // 清空旧的按钮引用和事件
            foreach (var button in m_ChoiceButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
            m_ChoiceButtons.Clear();

            // 初始化所有选项按钮为不可见
            if (m_ChoiceButton1 != null) m_ChoiceButton1.gameObject.SetActive(false);
            if (m_ChoiceButton2 != null) m_ChoiceButton2.gameObject.SetActive(false);
            if (m_ChoiceButton3 != null) m_ChoiceButton3.gameObject.SetActive(false);
            if (m_ChoiceButton4 != null) m_ChoiceButton4.gameObject.SetActive(false);
            if (m_ChoiceButton5 != null) m_ChoiceButton5.gameObject.SetActive(false);

            // 使用固定按钮显示选项
            Button[] allButtons = { m_ChoiceButton1, m_ChoiceButton2, m_ChoiceButton3, m_ChoiceButton4, m_ChoiceButton5 };
            Text[] allButtonTexts = { m_ChoiceButton1Text,m_ChoiceButton2Text,m_ChoiceButton3Text,m_ChoiceButton4Text,m_ChoiceButton5Text};

            Debug.Log($"[DialoguePanel] 按钮检查: Button1={allButtons[0]!=null}, Text1={allButtonTexts[0]!=null}");

            for (int i = 0; i < choicesData.Count && i < 5; i++)
            {
                var choiceData = choicesData[i];

                Debug.Log($"[DialoguePanel] 处理选项 {i}: {choiceData.ChoiceText}");

                // 检查选项条件是否满足
                if (!CanShowChoice(choiceData))
                {
                    Debug.Log($"[DialoguePanel] 隐藏选项 {i} due to unmet conditions");
                    continue;
                }

                // 使用固定按钮
                if (allButtons[i] != null && allButtonTexts[i] != null)
                {
                    // 设置选项文本
                    allButtonTexts[i].text = choiceData.ChoiceText;

                    // 清除旧的事件
                    allButtons[i].onClick.RemoveAllListeners();
                    // 添加新的事件
                    allButtons[i].onClick.AddListener(() => OnChoiceButtonClick(choiceData.NextId, choiceData.Rewards));

                    // 显示按钮
                    allButtons[i].gameObject.SetActive(true);
                    m_ChoiceButtons.Add(allButtons[i]);

                    Debug.Log($"[DialoguePanel] 已设置按钮 {i}: {choiceData.ChoiceText}, Active: {allButtons[i].gameObject.activeSelf}");
                }
                else
                {
                    Debug.LogError($"[DialoguePanel] 无法使用按钮 {i}: Button={allButtons[i]!=null}, Text={allButtonTexts[i]!=null}");
                }
            }

            // 显示选项面板
            m_SelectPanel.gameObject.SetActive(true);
            Debug.Log($"[DialoguePanel] 设置选项面板 active = true");
            Debug.Log($"[DialoguePanel] 选项面板当前状态 - ActiveSelf: {m_SelectPanel.gameObject.activeSelf}, ActiveInHierarchy: {m_SelectPanel.gameObject.activeInHierarchy}");

            // 检查 Canvas Group
            var canvasGroup = m_SelectPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                Debug.Log($"[DialoguePanel] 选项面板 CanvasGroup - Alpha: {canvasGroup.alpha}, Interactable: {canvasGroup.interactable}, BlocksRaycasts: {canvasGroup.blocksRaycasts}");
            }

            Log.Info("[DialoguePanel] 选项面板已显示");
        }

        /// <summary>
        /// 隐藏选项面板
        /// </summary>
        private void HideChoicesPanel()
        {
            if (m_SelectPanel != null)
            {
                m_SelectPanel.gameObject.SetActive(false);
            }

            // 清理按钮事件（不销毁按钮，因为它们是插件的一部分）
            foreach (var button in m_ChoiceButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
            m_ChoiceButtons.Clear();
        }

        /// <summary>
        /// 选项按钮点击
        /// </summary>
        private void OnChoiceButtonClick(int nextDialogueId, List<ChoiceReward> rewards)
        {
            Log.Info($"[DialoguePanel] 选项已选择，跳转到对话ID: {nextDialogueId}");

            if (m_ProcedureGame == null)
            {
                Debug.LogError("[DialoguePanel] m_ProcedureGame 为空，无法处理选项选择");
                return;
            }

            // 隐藏选项面板
            HideChoicesPanel();

            // 应用选项奖励（流程层处理）
            m_ProcedureGame.ApplyChoiceRewards(rewards);

            // 设置下一对话ID
            m_ProcedureGame.SetNextDialogueId(nextDialogueId);

            // 延迟显示下一句对话
            StartCoroutine(ShowNextDialogueAfterChoice());
        }

        /// <summary>
        /// 检查选项是否满足显示条件
        /// </summary>
        private bool CanShowChoice(RuntimeChoiceData choice)
        {
            if (choice.Conditions == null || choice.Conditions.Count == 0)
            {
                // 没有条件，默认显示
                return true;
            }

            var playerData = CustomEntry.PlayerData;
            if (playerData == null)
            {
                return false;
            }

            foreach (var condition in choice.Conditions)
            {
                bool conditionMet = false;

                switch (condition.Type)
                {
                    case ConditionType.PlayerAttribute:
                        int currentAttr = 0;
                        switch (condition.AttributeType)
                        {
                            case PlayerAttributeType.Charm:
                                currentAttr = playerData.Charm;
                                break;
                            case PlayerAttributeType.Inspiration:
                                currentAttr = playerData.Inspiration;
                                break;
                            case PlayerAttributeType.Sanity:
                                currentAttr = playerData.Sanity;
                                break;
                        }
                        conditionMet = condition switch
                        {
                            { Operator: ConditionOperator.GreaterThanOrEqual } => currentAttr >= condition.Value,
                            { Operator: ConditionOperator.LessThanOrEqual } => currentAttr <= condition.Value,
                            { Operator: ConditionOperator.Equal } => currentAttr == condition.Value,
                            _ => false
                        };
                        break;

                    case ConditionType.NpcFavorability:
                        if (int.TryParse(condition.NpcId, out int npcId))
                        {
                            int favorability = playerData.GetFavorability(npcId);
                            conditionMet = condition switch
                            {
                                { Operator: ConditionOperator.GreaterThanOrEqual } => favorability >= condition.Value,
                                { Operator: ConditionOperator.LessThanOrEqual } => favorability <= condition.Value,
                                { Operator: ConditionOperator.Equal } => favorability == condition.Value,
                                _ => false
                            };
                        }
                        else
                        {
                            Debug.LogWarning($"[DialoguePanel] 无效的NPC ID: {condition.NpcId}");
                        }
                        break;

                    case ConditionType.SpecialItem:
                        if (int.TryParse(condition.ItemId, out int itemId))
                        {
                            bool hasItem = playerData.HasItem(itemId);
                            conditionMet = condition.RequireItem ? hasItem : !hasItem;
                        }
                        else
                        {
                            Debug.LogWarning($"[DialoguePanel] 无效的物品 ID: {condition.ItemId}");
                            conditionMet = false;
                        }
                        break;
                }

                if (!conditionMet)
                {
                    // 只要有一个条件不满足，就不显示该选项
                    return false;
                }
            }

            // 所有条件都满足
            return true;
        }

        
        #endregion

        #region 立绘与背景显示

        /// <summary>
        /// 根据立绘动作JSON更新立绘显示
        /// </summary>
        private void ApplyCharacterDisplay(string characterActionsJson)
        {
            // 没有立绘数据则保持当前立绘不动
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

        /// <summary>
        /// 异步加载并设置背景图
        /// </summary>
        private void ApplyBackground(string backgroundPath)
        {
            if (string.IsNullOrEmpty(backgroundPath) || m_BackgroundImage == null)
            {
                return;
            }

            GameEntry.Resource.LoadAsset(
                backgroundPath,
                typeof(Sprite),
                new LoadAssetCallbacks(
                    OnLoadBackgroundSuccess,
                    OnLoadBackgroundFailure
                )
            );
        }

        private void OnLoadBackgroundSuccess(string assetName, object asset, float duration, object userData)
        {
            if (m_BackgroundImage == null) return;

            Sprite sprite = asset as Sprite;
            if (sprite != null)
            {
                m_BackgroundImage.sprite = sprite;
            }
        }

        private void OnLoadBackgroundFailure(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Debug.LogWarning($"[DialoguePanel] 背景图加载失败: {assetName}, 状态: {status}, 错误: {errorMessage}");
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

            Debug.Log("[DialoguePanel] === ShowCurrentDialogue 开始 ===");

            DialogueDisplayData data = m_ProcedureGame.GetCurrentDialogue();

            if (data == null)
            {
                Debug.Log("[DialoguePanel] 获取对话数据失败");
                return;
            }

            Debug.Log($"[DialoguePanel] 获取数据成功 - NodeType: {data.NodeType}, ChoicesJson: {data.ChoicesJson}");

            Log.Info($"[DialoguePanel] 显示对话 - 说话人: {data.SpeakerName}, 文本: {data.DialogText}, 类型: {data.NodeType}");

            // 应用立绘
            ApplyCharacterDisplay(data.CharacterActionsJson);

            // 应用背景图
            ApplyBackground(data.BackgroundPath);

            // 检查是否是选择节点
            if (data.NodeType == 1) // 选择节点
            {
                Log.Info("[DialoguePanel] 检测到选择节点，显示内部选项面板");
                Debug.Log($"[DialoguePanel] Options JSON: {data.ChoicesJson}");

                // 解析选项数据
                List<RuntimeChoiceData> choicesData = ParseChoicesJson(data.ChoicesJson);
                Debug.Log($"[DialoguePanel] ParseChoicesJson 返回: {(choicesData != null ? choicesData.Count.ToString() : "null")}");

                if (choicesData != null && choicesData.Count > 0)
                {
                    Log.Info($"[DialoguePanel] 解析成功 {choicesData.Count} 个选项");
                    foreach (var choice in choicesData)
                    {
                        Log.Info($"[DialoguePanel] 选项: {choice.ChoiceText}, NextId: {choice.NextId}");
                    }

                    // 显示选项面板
                    ShowChoicesPanel(choicesData);

                    // 隐藏对话文本
                    if (m_DialogueText != null)
                    {
                        m_DialogueText.text = "";
                    }

                    // 隐藏继续提示
                    if (m_ContinueIndicator != null)
                    {
                        m_ContinueIndicator.SetActive(false);
                    }

                    Debug.Log("[DialoguePanel] === 选项显示完成 ===");
                    return;
                }
                else
                {
                    Debug.LogError("[DialoguePanel] 选项数据解析失败或为空");
                    // 如果选项解析失败，按普通对话处理
                }
            }

            // 普通对话或非选择节点
            // 隐藏选项面板
            HideChoicesPanel();

            // 显示对话
            SetDialogue(data.SpeakerName, data.DialogText);

            // 显示继续提示
            if (m_ContinueIndicator != null)
            {
                m_ContinueIndicator.SetActive(true);
            }
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

            // 如果正在显示选项面板，不做任何处理（等待选项）
            if (m_SelectPanel != null && m_SelectPanel.gameObject.activeSelf)
            {
                Debug.Log("[DialoguePanel] 正在显示选项面板，等待选择");
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

            Debug.Log($"[DialoguePanel] OnContinueClick - 获取到下一条对话: NodeType={nextData.NodeType}");

            // 使用 ShowCurrentDialogue 来正确处理选项节点
            ShowCurrentDialogue();
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
