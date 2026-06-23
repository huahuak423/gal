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
using GameFramework.Sound;

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
        [SerializeField] private Image m_TextImage;
        [SerializeField] private Button m_BackgroundImageButton;

        [Header("菜单按钮")]
        [SerializeField] private Button m_ButtonMenu;
        [SerializeField] private Button m_ButtonSpeedUp;
        [SerializeField] private Button m_ButtonHistory;
        [SerializeField] private Button m_ButtonHide;
        [SerializeField] private Button m_ButtonInformation;
        [SerializeField] private Button m_ButtonSave;
        [SerializeField] private Button m_ButtonAuto;
        [SerializeField] private Text m_ButtonMenuText;
        [SerializeField] private Text m_ButtonSpeedUpText;
        [SerializeField] private Text m_ButtonHistoryText;
        [SerializeField] private Text m_ButtonHideText;
        [SerializeField] private Text m_ButtonInformationText;
        [SerializeField] private Text m_ButtonSaveText;
        [SerializeField] private Text m_ButtonAutoText;

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

        // 音频相关
        private string m_CurrentBgmPath = null;

        // 自动播放 & 加速
        private bool m_IsAutoMode = false;
        private bool m_IsSpeedUpMode = false;  // 快进按钮开关
        private bool m_IsReplayMode = false;   // 当前是否为重玩（历史回顾/下一周目）
        private readonly int[] m_SpeedLevels = { 1, 2, 4, 8 };
        private int m_SpeedIndex = 0;           // 当前速度索引
        private float m_AutoAdvanceTimer = 0f;  // 自动播放计时器
        private bool m_AutoAdvanceWaiting = false; // 是否正在等待自动播放

        // 已读对话追踪（用于快进跳过）
        private HashSet<int> m_ReadDialogueIds = new HashSet<int>();
        private int m_CurrentDisplayingDialogueId = 0; // 当前正在显示的对话ID

        // 立绘相关
        private Image[] m_CharacterImages = new Image[3];
        private RectTransform[] m_CharacterRects = new RectTransform[3];
        private Vector2[] m_OriginalPositions = new Vector2[3];

        // 对话框区域引用（用于隐藏/显示）
        private Transform m_TextPlate;

        // 对话框背景 Sprite 缓存
        private Sprite m_DialogBoxWithSpeaker;
        private Sprite m_DialogBoxWithoutSpeaker;
        private const string c_DialogBoxWithSpeakerPath = "Assets/GameMain/Art/UI_Common/UI/对话框.png";
        private const string c_DialogBoxWithoutSpeakerPath = "Assets/GameMain/Art/UI_Common/UI/对话框（无名字）.png";

        // 选项相关
        private List<Button> m_ChoiceButtons = new List<Button>();
        private GameObject m_ChoiceButtonPrefab;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 挂载组件引用
            m_CharacterNameText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/CharacterName/TextConstCharacterName");
            m_DialogueText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/TextConstDialogue");
            m_TextPlate = this.GetComponentByPath<Transform>("Canvas/Background/TextPlate");
            m_BackgroundImage = this.GetComponentByPath<Image>("Canvas/Background");
            m_TextImage = this.GetComponentByPath<Image>("Canvas/Background/TextPlate/CharacterName");
            m_BackgroundImageButton = this.GetComponentByPath<Button>("Canvas/Background");
            m_ButtonAuto = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonAuto");
            m_ButtonSpeedUp = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonSpeedUp");
            m_ButtonHistory = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonHistory");
            m_ButtonHide = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonHide");
            m_ButtonInformation = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonInformation");
            m_ButtonSave = this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonSave");
            m_ButtonMenu= this.GetComponentByPath<Button>("Canvas/Background/TextPlate/DialoguePlate/ButtonMenu");
            m_ButtonAutoText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonAuto/TextForTest");
            m_ButtonSpeedUpText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonSpeedUp/TextForTest");
            m_ButtonHistoryText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonHistory/TextForTest");
            m_ButtonHideText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonHide/TextForTest");
            m_ButtonInformationText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonInformation/TextForTest");
            m_ButtonSaveText = this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/ButtonPlate/ButtonSave/TextForTest");
            m_ButtonMenuText= this.GetComponentByPath<Text>("Canvas/Background/TextPlate/DialoguePlate/ButtonMenu/TextForTest");
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

            // 挂载立绘组件引用 (0=女主主控立绘, 1=Left, 2=Right)
            m_CharacterImages[0] = this.GetComponentByPath<Image>("Canvas/Background/CharacterImage0");
            if (m_CharacterImages[0] != null)
            {
                m_CharacterRects[0] = m_CharacterImages[0].rectTransform;
                m_OriginalPositions[0] = m_CharacterRects[0].anchoredPosition;
                m_CharacterImages[0].gameObject.SetActive(false);
            }

            for (int i = 1; i < 3; i++)
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

            // 绑定加速按钮事件
            if (m_ButtonSpeedUp != null)
            {
                m_ButtonSpeedUp.onClick.AddListener(OnSpeedUpClick);
            }

            // 绑定自动播放按钮事件
            if (m_ButtonAuto != null)
            {
                m_ButtonAuto.onClick.AddListener(OnAutoClick);
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

            // 预加载对话框背景 Sprite
            Debug.Log("[DialoguePanel] 开始预加载对话框背景 Sprite");
            GameEntry.Resource.LoadAsset(
                c_DialogBoxWithSpeakerPath,
                typeof(Sprite),
                new LoadAssetCallbacks(
                    (assetName, asset, duration, userData) =>
                    {
                        m_DialogBoxWithSpeaker = asset as Sprite;
                        Debug.Log($"[DialoguePanel] 对话框(有名字)加载成功: {assetName}, sprite={m_DialogBoxWithSpeaker}");
                    },
                    (assetName, status, errorMessage, userData) => Debug.LogWarning($"[DialoguePanel] 对话框(有名字)加载失败: {assetName}, {errorMessage}")
                )
            );
            GameEntry.Resource.LoadAsset(
                c_DialogBoxWithoutSpeakerPath,
                typeof(Sprite),
                new LoadAssetCallbacks(
                    (assetName, asset, duration, userData) =>
                    {
                        m_DialogBoxWithoutSpeaker = asset as Sprite;
                        Debug.Log($"[DialoguePanel] 对话框(无名字)加载成功: {assetName}, sprite={m_DialogBoxWithoutSpeaker}");
                    },
                    (assetName, status, errorMessage, userData) => Debug.LogWarning($"[DialoguePanel] 对话框(无名字)加载失败: {assetName}, {errorMessage}")
                )
            );

            
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

        /// <summary>
        /// 快进按钮：切换快进模式（开启/关闭），同时速度在 1x 2x 4x 8x 之间循环
        /// 快进模式：已读文本直接跳过，未读文本按速度倍率加速
        /// </summary>
        private void OnSpeedUpClick()
        {
            // 切换快进模式
            m_IsSpeedUpMode = !m_IsSpeedUpMode;

            // 同时循环速度等级
            m_SpeedIndex = (m_SpeedIndex + 1) % m_SpeedLevels.Length;
            int speed = m_SpeedLevels[m_SpeedIndex];

            Debug.Log($"[DialoguePanel] 快进: {(m_IsSpeedUpMode ? "开启" : "关闭")}，速度: {speed}x");

            // 如果正在自动播放倒计时，重置计时器以应用新速度
            if (m_IsAutoMode && !m_IsTyping && m_AutoAdvanceWaiting)
            {
                m_AutoAdvanceTimer = 0f;
            }
        }

        /// <summary>
        /// 自动播放按钮：切换自动模式
        /// </summary>
        private void OnAutoClick()
        {
            m_IsAutoMode = !m_IsAutoMode;
            Debug.Log($"[DialoguePanel] 自动播放: {(m_IsAutoMode ? "开启" : "关闭")}, m_IsTyping={m_IsTyping}, m_IsSpeedUpMode={m_IsSpeedUpMode}, speed={m_SpeedLevels[m_SpeedIndex]}x");

            if (m_IsAutoMode && !m_IsTyping)
            {
                // 当前没有在打字，立即开始自动倒计时
                m_AutoAdvanceTimer = 0f;
                m_AutoAdvanceWaiting = true;
            }
            else if (!m_IsAutoMode)
            {
                // 关闭自动，取消等待中的倒计时
                m_AutoAdvanceWaiting = false;
                m_AutoAdvanceTimer = 0f;
            }
        }

        /// <summary>
        /// 每帧更新：处理自动播放计时器
        /// </summary>
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (!m_AutoAdvanceWaiting || !m_IsAutoMode) return;

            m_AutoAdvanceTimer += elapseSeconds;
            float delay = 1.0f / m_SpeedLevels[m_SpeedIndex];

            if (m_AutoAdvanceTimer >= delay)
            {
                Debug.Log($"[DialoguePanel] 自动播放计时器触发 timer={m_AutoAdvanceTimer:F2}s >= delay={delay:F2}s, speed={m_SpeedLevels[m_SpeedIndex]}x");
                m_AutoAdvanceWaiting = false;
                m_AutoAdvanceTimer = 0f;
                OnContinueClick();
            }
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

            // 读取是否为重玩事件，决定快进模式的初始行为
            // 重玩（历史回顾/下一周目）→ 快进=加速+自动跳过
            // 首次进入 → 快进=只加速文字
            if (m_ProcedureGame != null)
            {
                m_IsReplayMode = m_ProcedureGame.IsReplayEvent;
                Debug.Log($"[DialoguePanel] OnOpen - 重玩状态: {m_IsReplayMode}");
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

            // 重置自动播放计时器
            m_AutoAdvanceWaiting = false;
            m_AutoAdvanceTimer = 0f;

            m_IsAutoMode = false;
            m_IsSpeedUpMode = false;
            m_IsReplayMode = false;
            m_SpeedIndex = 0;
            m_ReadDialogueIds.Clear();

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

                // 标记为已读
                m_ReadDialogueIds.Add(m_CurrentDisplayingDialogueId);

                // 显示继续提示
                if (m_ContinueIndicator != null)
                {
                    m_ContinueIndicator.SetActive(true);
                }

                // 跳过后如果自动模式开启，开始自动倒计时
                if (m_IsAutoMode)
                {
                    m_AutoAdvanceTimer = 0f;
                    m_AutoAdvanceWaiting = true;
                }
            }
            else if (!m_IsTyping)
            {
                // 如果已经显示完毕，触发完成回调
                m_OnComplete?.Invoke();
            }
        }

        /// <summary>
        /// 快进模式下跳过当前已读对话，直接进入下一句
        /// </summary>
        private void SkipToNextDialogue()
        {
            if (m_ProcedureGame == null) return;

            // 先标记当前为已读（以防万一还没标记）
            m_ReadDialogueIds.Add(m_CurrentDisplayingDialogueId);

            DialogueDisplayData nextData = m_ProcedureGame.GoToNextDialogue();
            if (nextData == null)
            {
                Log.Info("[DialoguePanel] 剧情结束（快进）");
                return;
            }

            // 立即显示下一句（不走打字机）
            ShowCurrentDialogue();
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

        #region 音频播放

        /// <summary>
        /// 播放对话节点的音频（BGM/SE/Voice）
        /// </summary>
        private void PlayDialogueAudio(DialogueDisplayData data)
        {
            // BGM：路径非空且与当前不同时切换，留空则继续播放
            if (!string.IsNullOrEmpty(data.BgmPath) && data.BgmPath != m_CurrentBgmPath)
            {
                // BGM组只有1个Agent且Avoid Being Replaced=false，新播自动替换旧BGM
                m_CurrentBgmPath = data.BgmPath;
                var bgmParams = new PlaySoundParams
                {
                    Loop = true,
                    Priority = 0,
                    VolumeInSoundGroup = 1f,
                    FadeInSeconds = 0.5f,
                    Pitch = 1f,
                    PanStereo = 0f,
                    SpatialBlend = 0f,
                    MaxDistance = 100f,
                    DopplerLevel = 0f
                };
                GameEntry.Sound.PlaySound(data.BgmPath, "BGM", bgmParams);
                Debug.Log($"[DialoguePanel] BGM切换: {data.BgmPath}");
            }

            // SE：路径非空时播放一次
            if (!string.IsNullOrEmpty(data.SePath))
            {
                var seParams = new PlaySoundParams
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
                GameEntry.Sound.PlaySound(data.SePath, "Sound", seParams);
                Debug.Log($"[DialoguePanel] SE播放: {data.SePath}");
            }

            // Voice：路径非空时播放一次（同组Avoid Being Replaced会自动打断上一句）
            if (!string.IsNullOrEmpty(data.VoicePath))
            {
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
                GameEntry.Sound.PlaySound(data.VoicePath, "Voice", voiceParams);
                Debug.Log($"[DialoguePanel] Voice播放: {data.VoicePath}");
            }
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

            // 记录当前对话ID（用于已读追踪和快进跳过）
            m_CurrentDisplayingDialogueId = data.CurrentNodeId;

            Log.Info($"[DialoguePanel] 显示对话 - 说话人: {data.SpeakerName}, 文本: {data.DialogText}, 类型: {data.NodeType}");

            // 应用立绘
            ApplyCharacterDisplay(data.CharacterActionsJson);

            // 应用背景图
            ApplyBackground(data.BackgroundPath);

            // 播放音频（BGM持续播放，SE和Voice当句播放）
            PlayDialogueAudio(data);

            // 控制对话框区域显示/隐藏
            if (m_TextPlate != null)
            {
                m_TextPlate.gameObject.SetActive(!data.HideDialoguePanel);
            }

            // 根据有无说话人切换对话框背景
            if (m_TextImage != null)
            {
                bool hasSpeaker = !string.IsNullOrEmpty(data.SpeakerName);
                Sprite targetSprite = hasSpeaker ? m_DialogBoxWithSpeaker : m_DialogBoxWithoutSpeaker;

                if (targetSprite != null)
                {
                    m_TextImage.sprite = targetSprite;
                }
                else
                {
                    // 缓存未命中，按需异步加载
                    string loadPath = hasSpeaker ? c_DialogBoxWithSpeakerPath : c_DialogBoxWithoutSpeakerPath;
                    Debug.Log($"[DialoguePanel] 对话框Sprite未缓存，按需加载: {loadPath}");
                    GameEntry.Resource.LoadAsset(
                        loadPath,
                        typeof(Sprite),
                        new LoadAssetCallbacks(
                            (assetName, asset, duration, userData) =>
                            {
                                Sprite loaded = asset as Sprite;
                                if (hasSpeaker) m_DialogBoxWithSpeaker = loaded;
                                else m_DialogBoxWithoutSpeaker = loaded;
                                if (m_TextImage != null) m_TextImage.sprite = loaded;
                                Debug.Log($"[DialoguePanel] 对话框Sprite按需加载成功: {assetName}");
                            },
                            (assetName, status, errorMessage, userData) => Debug.LogWarning($"[DialoguePanel] 对话框Sprite加载失败: {assetName}, {errorMessage}")
                        )
                    );
                }
            }

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

                    // 选项出现时隐藏对话框区域
                    if (m_TextPlate != null)
                    {
                        m_TextPlate.gameObject.SetActive(false);
                    }

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
            // 用户手动操作，取消自动播放倒计时
            m_AutoAdvanceWaiting = false;
            m_AutoAdvanceTimer = 0f;

            // 快进按钮开启，且（已读 OR 重玩）→ 直接跳下一句
            if (m_IsSpeedUpMode && (m_ReadDialogueIds.Contains(m_CurrentDisplayingDialogueId) || m_IsReplayMode))
            {
                Debug.Log($"[DialoguePanel] 快进跳过已读对话: {m_CurrentDisplayingDialogueId}");
                SkipToNextDialogue();
                return;
            }

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
        /// 打字机协程（受速度倍率影响）
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

            int speed = m_SpeedLevels[m_SpeedIndex];
            bool isAlreadyRead = m_ReadDialogueIds.Contains(m_CurrentDisplayingDialogueId);

            // 快进按钮开启，且（已读 OR 重玩）→ 直接显示全文，不等待
            if (m_IsSpeedUpMode && (isAlreadyRead || m_IsReplayMode))
            {
                if (m_DialogueText != null)
                {
                    m_DialogueText.text = text;
                }
                yield return null; // 让 UI 先刷新一帧
            }
            else
            {
                // 8倍速直接显示全文
                if (speed >= 8)
                {
                    if (m_DialogueText != null)
                    {
                        m_DialogueText.text = text;
                    }
                }
                else
                {
                    float typeSpeed = 0.03f / speed; // 基础0.03s，速度越快间隔越短

                    for (int i = 0; i < text.Length; i++)
                    {
                        // 实时读取速度（中途变速立即生效）
                        speed = m_SpeedLevels[m_SpeedIndex];
                        if (speed >= 8)
                        {
                            if (m_DialogueText != null)
                                m_DialogueText.text = text;
                            break;
                        }

                        typeSpeed = 0.03f / speed;

                        if (m_DialogueText != null)
                        {
                            m_DialogueText.text += text[i];
                        }
                        yield return new WaitForSeconds(typeSpeed);
                    }
                }
            }

            m_IsTyping = false;

            // 标记为已读
            m_ReadDialogueIds.Add(m_CurrentDisplayingDialogueId);

            // 显示继续提示
            if (m_ContinueIndicator != null)
            {
                m_ContinueIndicator.SetActive(true);
            }

            m_TypeWriterCoroutine = null;

            // 自动播放：文字显示完后开始自动倒计时
            if (m_IsAutoMode)
            {
                m_AutoAdvanceTimer = 0f;
                m_AutoAdvanceWaiting = true;
                Debug.Log($"[DialoguePanel] TypewriterRoutine结束 → 自动计时器启动, delay={1.0f / m_SpeedLevels[m_SpeedIndex]:F2}s");
            }
        }

        #endregion
    }
}
