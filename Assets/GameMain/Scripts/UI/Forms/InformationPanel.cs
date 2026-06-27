using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameFramework.Resource;

namespace AVGGame
{
    /// <summary>
    /// 角色信息数据（用于信息面板切换显示）
    /// </summary>
    [System.Serializable]
    public class CharacterData
    {
        public int NpcId;
        public string Name;
        public string Age;
        public string Work;
        public string PortraitPath;
    }

    public class InformationPanel : UIFormBase
    {
        [Header("主要面板")]
        [SerializeField] private Transform m_InformationPlate;
        [SerializeField] private Transform m_PersonalPlate;
        [SerializeField] private Transform m_CharacterPlate;

        [Header("角色信息")]
        [SerializeField] private Text m_MineName;
        [SerializeField] private Text m_CharacterName;
        [SerializeField] private Image m_Character;
        [SerializeField] private Image m_MyPortrait;

        [Header("数值面板")]
        [SerializeField] private Text m_CharacterAge;
        [SerializeField] private Text m_CharacterWork;
        [SerializeField] private Text m_MineAge;
        [SerializeField] private Text m_MineWork;

        
        [Header("好感度与进度")]
        [SerializeField] private Slider m_LovePlate;
        [SerializeField] private Text m_LovePlateText;
        [SerializeField] private Scrollbar m_ScrollbarInspiration;

        [Header("按钮")]
        [SerializeField] private Button m_ButtonClose;
        [SerializeField] private Button m_ButtonEvent1;
        [SerializeField] private Button m_ButtonEvent2;
        [SerializeField] private Button m_ButtonEvent3;
        [SerializeField] private Button m_ButtonEvent4;
        [SerializeField] private Button m_ButtonEvent5;
        [SerializeField] private Button m_ButtonCharacter1;
        [SerializeField] private Button m_ButtonCharacter2;
        [SerializeField] private Button m_ButtonCharacter3;
        [SerializeField] private Button m_ButtonCharacter4;
        [SerializeField] private Button m_ButtonCharacter5;
        [SerializeField] private Button m_ButtonPersonal;
        [SerializeField] private Button m_ButtonCharacter;
        [SerializeField] private Button m_ButtonInventory;

        private ProcedureGame m_ProcedureGame;

        /// <summary>
        /// 静态标记：下次打开时直接显示角色(NPC)面板。
        /// 由 MenuPanel 等外部在 OpenUIForm 前设置，OnOpen 中读取后重置。
        /// </summary>
        public static bool OpenInCharacterTab = false;
        private Button[] m_EventButtons;
        private Button[] m_CharacterButtons;
        private int m_CurrentCharacterIndex = 0;

        // 角色数据（后续可改为从配置表读取）
        private CharacterData[] m_Characters = new CharacterData[]
        {
            new CharacterData { NpcId = 1, Name = "温叙",   Age = "", Work = "", PortraitPath = "Assets/GameMain/Art/New Characters/温叙/温叙常规.png" },
            new CharacterData { NpcId = 2, Name = "周杉",   Age = "", Work = "", PortraitPath = "Assets/GameMain/Art/New Characters/周杉/周杉.png" },
            new CharacterData { NpcId = 3, Name = "陈予荣", Age = "", Work = "", PortraitPath = "Assets/GameMain/Art/New Characters/陈予荣/陈予荣.png" },
        };

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
            m_MineName = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/PersonalPlate/NamePlate/CharacterName");

            // 数值面板
            m_CharacterAge = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/Plate/AgePlate/CharacterName");
            m_CharacterWork = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/Plate/WorkPlate/CharacterName");
            m_MineAge = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/PersonalPlate/NamePlate/AgePlate/Age");
            m_MineWork = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/PersonalPlate/NamePlate/WorkPlate/Work");

            // 进度
            m_LovePlate = this.GetComponentByPath<Slider>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/LovePlate/Slider");
            m_LovePlateText = this.GetComponentByPath<Text>("Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/LovePlate/TextConstLove");

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
            m_ButtonCharacter1 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/CharacterChangePlate/ButtonCharacter1");
            m_ButtonCharacter2 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/CharacterChangePlate/ButtonCharacter2");
            m_ButtonCharacter3 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/CharacterChangePlate/ButtonCharacter3");
            m_ButtonCharacter4 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/CharacterChangePlate/ButtonCharacter4");
            m_ButtonCharacter5 = this.GetComponentByPath<Button>("Canvas/Background/InformationPlate/CharacterPlate/CharacterChangePlate/ButtonCharacter5");

            m_EventButtons = new Button[] { m_ButtonEvent1, m_ButtonEvent2, m_ButtonEvent3, m_ButtonEvent4, m_ButtonEvent5 };

            // 角色切换按钮
            m_CharacterButtons = new Button[] { m_ButtonCharacter1, m_ButtonCharacter2, m_ButtonCharacter3, m_ButtonCharacter4, m_ButtonCharacter5 };
            for (int i = 0; i < m_CharacterButtons.Length; i++)
            {
                if (m_CharacterButtons[i] != null)
                {
                    int idx = i;
                    m_CharacterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(idx));
                    // 超出角色数据范围的按钮隐藏
                    m_CharacterButtons[i].gameObject.SetActive(i < m_Characters.Length);
                }
            }

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

            // ====== 图片文件挂载 ======
            // m_ButtonClose.image      — Canvas/Background/InformationPlate/ButtonClose
            // m_ButtonPersonal.image   — Canvas/Background/InformationPlate/ButtonPersonal
            // m_ButtonCharacter.image  — Canvas/Background/InformationPlate/ButtonCharacter
            // m_ButtonInventory.image  — Canvas/Background/InformationPlate/PersonalPlate/InventoryPlate/ButtonInventory
            // m_ButtonEvent1.image     — Canvas/Background/InformationPlate/CharacterPlate/InformationPlate/EventPanel/Viewport/Content/Event1
            // m_ButtonEvent2~5.image   — 同上，Event{N}

            // 加载女主立绘
            LoadMinePortrait();
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

            // 根据标记决定初始面板
            if (OpenInCharacterTab)
            {
                OpenInCharacterTab = false; // 读后重置
                ShowCharacterTab();
            }
            else
            {
                ShowPersonalTab();
            }
            RefreshMineInfo();
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
            RefreshCharacterInfo();
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

        private void OnCharacterButtonClicked(int index)
        {
            if (index < 0 || index >= m_Characters.Length) return;
            m_CurrentCharacterIndex = index;
            RefreshCharacterInfo();
            Log.Info($"[InformationPanel] 切换到角色: {m_Characters[index].Name}");
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

        #region 女主信息

        private const string c_MinePortraitPath = "Assets/GameMain/Art/New Characters/女主/主控常服微笑.png";

        /// <summary>
        /// 加载女主立绘
        /// </summary>
        private void LoadMinePortrait()
        {
            if (m_MyPortrait == null) return;

            GameEntry.Resource.LoadAsset(
                c_MinePortraitPath,
                typeof(Sprite),
                new LoadAssetCallbacks(
                    (assetName, asset, duration, userData) =>
                    {
                        if (m_MyPortrait != null)
                            m_MyPortrait.sprite = asset as Sprite;
                    },
                    (assetName, status, errorMessage, userData) =>
                        Log.Warning($"[InformationPanel] 女主立绘加载失败: {assetName}, {errorMessage}")
                )
            );
        }

        /// <summary>
        /// 刷新女主信息（名字取自玩家取名）
        /// </summary>
        private void RefreshMineInfo()
        {
            string playerName = CustomEntry.PlayerData?.PlayerName ?? "";
            if (m_MineName != null) m_MineName.text = playerName;
        }

        #endregion

        #region 数值刷新（与面板控制分离）

        private void RefreshStats()
        {
            if (m_ProcedureGame == null) return;

            PlayerStatsData stats = m_ProcedureGame.GetPlayerStats();
            if (stats == null) return;

            Log.Info($"[InformationPanel] RefreshStats - 行动点:{stats.ActionPoints}/{stats.MaxActionPoints} 周目:{stats.CurrentRound}");
        }

        /// <summary>
        /// 刷新当前选中角色的全部信息（图片、名字、年龄、职业、好感度、事件按钮）
        /// </summary>
        private void RefreshCharacterInfo()
        {
            if (m_CurrentCharacterIndex < 0 || m_CurrentCharacterIndex >= m_Characters.Length) return;

            var charData = m_Characters[m_CurrentCharacterIndex];

            // 文本
            if (m_CharacterName != null) m_CharacterName.text = charData.Name;
            if (m_CharacterAge != null) m_CharacterAge.text = charData.Age;
            if (m_CharacterWork != null) m_CharacterWork.text = charData.Work;

            // 角色立绘
            LoadCharacterPortrait(charData.PortraitPath);

            // 好感度
            if (CustomEntry.PlayerData != null)
            {
                int favorability = CustomEntry.PlayerData.GetFavorability(charData.NpcId);
                const int maxFavorability = 100;

                if (m_LovePlate != null)
                    m_LovePlate.value = (float)favorability / maxFavorability;
                if (m_LovePlateText != null)
                    m_LovePlateText.text = favorability.ToString();

                Log.Info($"[InformationPanel] 角色:{charData.Name} NPC:{charData.NpcId} 好感度:{favorability}");
            }

            // 事件按钮
            RefreshEventButtonsForNpc(charData.NpcId);
        }

        /// <summary>
        /// 异步加载角色立绘
        /// </summary>
        private void LoadCharacterPortrait(string portraitPath)
        {
            if (string.IsNullOrEmpty(portraitPath) || m_Character == null) return;

            GameEntry.Resource.LoadAsset(
                portraitPath,
                typeof(Sprite),
                new LoadAssetCallbacks(
                    (assetName, asset, duration, userData) =>
                    {
                        Sprite sprite = asset as Sprite;
                        if (sprite != null && m_Character != null)
                        {
                            m_Character.sprite = sprite;
                        }
                    },
                    (assetName, status, errorMessage, userData) =>
                        Log.Warning($"[InformationPanel] 角色立绘加载失败: {assetName}, {errorMessage}")
                )
            );
        }

        /// <summary>
        /// 根据NPC ID刷新事件回顾按钮
        /// </summary>
        private void RefreshEventButtonsForNpc(int npcId)
        {
            if (m_ProcedureGame == null) return;

            List<EventRowData> completedEvents = m_ProcedureGame.GetCompletedEventsByNpcId(npcId);

            for (int i = 0; i < m_EventButtons.Length; i++)
            {
                if (m_EventButtons[i] == null) continue;
                m_EventButtons[i].gameObject.SetActive(i < completedEvents.Count);
            }
        }

        #endregion
    }
}