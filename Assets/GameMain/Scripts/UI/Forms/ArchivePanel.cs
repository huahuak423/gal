//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityGameFramework.Runtime;
using AVGGame;

namespace AVGGame
{
    /// <summary>
    /// 存档面板 - 用于选择存档槽位
    /// </summary>
    public class ArchivePanel : UIFormBase
    {
        #region 序列化字段 - 10个存档按钮

        [Header("存档按钮 - SavePlate 下的按钮")]
        [SerializeField] private Button m_SaveSlot1;
        [SerializeField] private Button m_SaveSlot2;
        [SerializeField] private Button m_SaveSlot3;
        [SerializeField] private Button m_SaveSlot4;
        [SerializeField] private Button m_SaveSlot5;
        [SerializeField] private Button m_SaveSlot6;
        [SerializeField] private Button m_SaveSlot7;
        [SerializeField] private Button m_SaveSlot8;
        [SerializeField] private Button m_SaveSlot9;
        [SerializeField] private Button m_SaveSlot10;

        #endregion

        #region 序列化字段 - 其他UI

        [Header("返回按钮")]
        [SerializeField] private Button m_ButtonBack;

        [Header("标题文本")]
        [SerializeField] private TextMeshProUGUI m_TitleText;

        #endregion

        #region 私有字段

        private bool m_IsNewGame = true;           // 是否是新游戏模式
        private int m_SelectedSlotIndex = -1;      // 当前选中的存档槽位

        private Button[] m_SaveSlots;              // 存档按钮数组缓存

        #endregion

        #region 属性

        public override int SortingOrder => 10;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 初始化存档按钮数组
            m_SaveSlots = new Button[]
            {
                m_SaveSlot1, m_SaveSlot2, m_SaveSlot3, m_SaveSlot4, m_SaveSlot5,
                m_SaveSlot6, m_SaveSlot7, m_SaveSlot8, m_SaveSlot9, m_SaveSlot10
            };

            // 绑定所有存档按钮的点击事件
            for (int i = 0; i < m_SaveSlots.Length; i++)
            {
                if (m_SaveSlots[i] != null)
                {
                    int slotIndex = i; // 闭包捕获
                    m_SaveSlots[i].onClick.AddListener(() => OnSaveSlotClick(slotIndex));
                }
            }

            // 绑定返回按钮事件
            if (m_ButtonBack != null)
            {
                m_ButtonBack.onClick.AddListener(OnBackClick);
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 获取打开模式（新游戏/继续游戏）
            if (userData is bool isNewGame)
            {
                m_IsNewGame = isNewGame;
            }
            else
            {
                m_IsNewGame = true; // 默认新游戏模式
            }

            // 更新标题
            UpdateTitle();

            // 更新存档槽位显示
            UpdateSaveSlotDisplay();

            Log.Info($"[ArchivePanel] Opened, IsNewGame: {m_IsNewGame}");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            m_SelectedSlotIndex = -1;
        }

        #endregion

        #region 按钮事件

        /// <summary>
        /// 存档槽位点击
        /// </summary>
        private void OnSaveSlotClick(int slotIndex)
        {
            Log.Info($"[ArchivePanel] Save slot {slotIndex} clicked");
            m_SelectedSlotIndex = slotIndex;

            // 跳转到对话界面
            OpenDialogueWithSlot(slotIndex);
        }

        /// <summary>
        /// 返回按钮点击
        /// </summary>
        private void OnBackClick()
        {
            Log.Info("[ArchivePanel] Back clicked");
            CloseSelf();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 更新标题文本
        /// </summary>
        private void UpdateTitle()
        {
            if (m_TitleText != null)
            {
                m_TitleText.text = m_IsNewGame ? "新游戏" : "继续游戏";
            }
        }

        /// <summary>
        /// 更新存档槽位显示
        /// </summary>
        private void UpdateSaveSlotDisplay()
        {
            for (int i = 0; i < m_SaveSlots.Length; i++)
            {
                if (m_SaveSlots[i] != null)
                {
                    // TODO: 根据存档数据更新按钮显示状态
                    // 这里可以设置按钮的interactable状态、文本内容等
                bool hasSave = HasSaveData(i);
                    // 暂时所有按钮都可点击
                    m_SaveSlots[i].interactable = true;
                }
            }
        }

        /// <summary>
        /// 检查是否有存档数据
        /// </summary>
        private bool HasSaveData(int slotIndex)
        {
            // TODO: 实际检查存档是否存在
            return false;
        }

        /// <summary>
        /// 打开对话界面
        /// </summary>
        private void OpenDialogueWithSlot(int slotIndex)
        {
            Log.Info($"[ArchivePanel] Opening dialogue with slot: {slotIndex}");

            // 创建存档数据对象
            var saveData = new SaveSlotData
            {
                SlotIndex = slotIndex,
                IsNewGame = m_IsNewGame
            };

            // 关闭当前面板
            CloseSelf();

            // 打开对话界面，传入存档数据
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Dialogue),
                UIGroupDefinition.Scene,
                Constant.AssetPriority.UIAsset,
                saveData
            );
        }

        #endregion

        #region 内部类

        /// <summary>
        /// 存档数据类
        /// </summary>
        public class SaveSlotData
        {
            public int SlotIndex;
            public bool IsNewGame;
        }
        #endregion
    }
}