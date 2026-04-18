using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    public class InformationPanel : UIFormBase
    {
        [Header("主要面板")]
        [SerializeField] private Transform m_InformationPlate;
        [SerializeField] private Transform m_PersonalPlate;
        [SerializeField] private Transform m_CharacterPlate;

        [Header("角色信息")]
        [SerializeField] private Text m_CharacterName;
        [SerializeField] private Image m_Character;
        [SerializeField] private Image m_MyPortrait;

        [Header("数值面板")]
        [SerializeField] private Text m_CharacterAge;
        [SerializeField] private Text m_CharacterWork;
        [SerializeField] private Text m_Inspiration;
        [SerializeField] private Text m_Reason;
        [SerializeField] private Text m_Charm;

        [Header("好感度与进度")]
        [SerializeField] private RectTransform m_LovePlate;
        [SerializeField] private Scrollbar m_ScrollbarInspiration;

        [Header("按钮")]
        [SerializeField] private Button m_ButtonClose;
        [SerializeField] private Button m_ButtonEvent1;
        [SerializeField] private Button m_ButtonEvent2;
        [SerializeField] private Button m_ButtonEvent3;
        [SerializeField] private Button m_ButtonEvent4;
        [SerializeField] private Button m_ButtonEvent5;
        [SerializeField] private Button m_ButtonPersonal;
        [SerializeField] private Button m_ButtonCharacter;
        [SerializeField] private Button m_ButtonInventory;

        private ProcedureGame m_ProcedureGame;
        private Button[] m_EventButtons;

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 面板路径 - InformationPlate 是根容器，PersonalPlate 和 CharacterPlate 是子面板
            m_InformationPlate = this.GetComponentByPath<Transform>("Canvas/Background/InformationPlate");
            m_PersonalPlate = this.GetComponentByPath<Transform>("Canvas/Background/InformationPlate/PersonalPlate");
            m_CharacterPlate = this.GetComponentByPath<Transform>("Canvas/Background/InformationPlate/CharacterPlate");

            // 角色信息
            m_CharacterName = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/NamePlate/CharacterName");
            m_Character = this.GetComponentByPath<Image>("Canvas/Background/InformationPlate/CharacterPlate/Character");
            m_MyPortrait = this.GetComponentByPath<Image>("Canvas/Background/InformationPlate/PersonalPlate/MyPortrait");

            // 数值面板
            m_CharacterAge = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/Plate/AgePlate/CharacterName");
            m_CharacterWork = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/Plate/WorkPlate/CharacterName");
            m_Inspiration = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/PersonalPlate/Inspiration/TextVarInspiration");
            m_Reason = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/PersonalPlate/Reason/TextVarReason");
            m_Charm = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/PersonalPlate/Charm/TextVarCharm");

            // 进度
            m_LovePlate = this.GetComponentByPath<RectTransform>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/LovePlate");

            // 按钮
            m_ButtonClose = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/ButtonClose");
            m_ButtonPersonal = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/ButtonPersonal");
            m_ButtonCharacter = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/ButtonCharacter");
            m_ButtonInventory = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/PersonalPlate/InventoryPlate/ButtonInventory");
            m_ButtonEvent1 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/EventPanel/Viewport/Content/Event1");
            m_ButtonEvent2 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/EventPanel/Viewport/Content/Event2");
            m_ButtonEvent3 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/EventPanel/Viewport/Content/Event3");
            m_ButtonEvent4 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/EventPanel/Viewport/Content/Event4");
            m_ButtonEvent5 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/EventPanel/Viewport/Content/Event5");

            m_EventButtons = new Button[] { m_ButtonEvent1, m_ButtonEvent2, m_ButtonEvent3, m_ButtonEvent4, m_ButtonEvent5 };

            // 绑定按钮事件
            if (m_ButtonClose != null)
                m_ButtonClose.onClick.AddListener(OnButtonCloseClick);

            if (m_ButtonPersonal != null)
                m_ButtonPersonal.onClick.AddListener(OnPersonalTabClick);

            if (m_ButtonCharacter != null)
                m_ButtonCharacter.onClick.AddListener(OnCharacterTabClick);

            if (m_ButtonInventory != null)
                m_ButtonInventory.onClick.AddListener(OnInventoryTabClick);

            // 事件按钮统一绑定
            for (int i = 0; i < m_EventButtons.Length; i++)
            {
                int index = i;
                if (m_EventButtons[index] != null)
                    m_EventButtons[index].onClick.AddListener(() => OnEventButtonClicked(index));
            }

            // 默认显示个人面板
            ShowPersonalTab();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 强制将自身 Canvas 置于最顶层
            Canvas selfCanvas = GetComponentInChildren<Canvas>();
            if (selfCanvas != null)
                selfCanvas.sortingOrder = 100;

            if (userData is ProcedureGame procedureGame)
            {
                m_ProcedureGame = procedureGame;
            }

            ShowPersonalTab();
            RefreshStats();
            Log.Info("[InformationPanel] OnOpen");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            Log.Info("[InformationPanel] OnClose");
        }

        #endregion

        #region 面板切换（仅控制显示隐藏）

        private void ShowPersonalTab()
        {
            if (m_PersonalPlate != null)
                m_PersonalPlate.gameObject.SetActive(true);
            if (m_CharacterPlate != null)
                m_CharacterPlate.gameObject.SetActive(false);
            Log.Info("[InformationPanel] ShowPersonalTab");
        }

        private void ShowCharacterTab()
        {
            if (m_PersonalPlate != null)
                m_PersonalPlate.gameObject.SetActive(false);
            if (m_CharacterPlate != null)
                m_CharacterPlate.gameObject.SetActive(true);
            RefreshEventButtons();
            Log.Info("[InformationPanel] ShowCharacterTab");
        }

        #endregion

        #region 按钮事件

        private void OnPersonalTabClick()
        {
            ShowPersonalTab();
        }

        private void OnCharacterTabClick()
        {
            ShowCharacterTab();
        }

        private void OnInventoryTabClick()
        {
            // 打开背包界面
            if (m_ProcedureGame != null)
            {
                m_ProcedureGame.OpenInventory();
            }
        }

        private void OnEventButtonClicked(int index)
        {
            Log.Info($"[InformationPanel] EventButton {index + 1} clicked");
            // TODO: 打开对应事件的回顾对话
        }

        private void OnButtonCloseClick()
        {
            CloseSelf();
        }

        #endregion

        #region 数值刷新（与面板控制分离）

        private void RefreshStats()
        {
            if (m_ProcedureGame == null) return;

            PlayerStatsData stats = m_ProcedureGame.GetPlayerStats();
            if (stats == null) return;

            if (m_Inspiration != null)
                m_Inspiration.text = stats.Inspiration.ToString();

            if (m_Charm != null)
                m_Charm.text = stats.Charm.ToString();

            if (m_Reason != null)
                m_Reason.text = stats.Sanity.ToString();

            Log.Info($"[InformationPanel] RefreshStats - 灵感:{stats.Inspiration} 魅力:{stats.Charm} 理智:{stats.Sanity} 行动点:{stats.ActionPoints}/{stats.MaxActionPoints} 周目:{stats.CurrentRound}");
        }

        private void RefreshEventButtons()
        {
            if (m_ProcedureGame == null) return;

            int npcId = m_ProcedureGame.GetCurrentNpcId();
            List<EventRowData> completedEvents = m_ProcedureGame.GetCompletedEventsByNpcId(npcId);

            for (int i = 0; i < m_EventButtons.Length; i++)
            {
                if (m_EventButtons[i] == null) continue;

                if (i < completedEvents.Count)
                {
                    m_EventButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    m_EventButtons[i].gameObject.SetActive(false);
                }
            }
        }

        #endregion
    }
}