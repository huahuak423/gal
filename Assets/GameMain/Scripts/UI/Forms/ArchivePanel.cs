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
        [SerializeField] private Button m_SaveSlot11;
        [SerializeField] private Button m_SaveSlot12;

        #endregion

        #region 序列化字段 - 其他UI

        [Header("返回按钮")]
        [SerializeField] private Button m_ButtonBack;

        [Header("标题文本")]
        [SerializeField] private TextMeshProUGUI m_TitleText;

        #endregion

        #region 私有字段

        private ArchiveMode m_Mode = ArchiveMode.Save;  // 当前模式
        private int m_SelectedSlotIndex = -1;            // 当前选中的存档槽位
        private Button[] m_SaveSlots;                     // 存档按钮数组缓存
        private Image[] m_SaveSlotImages;                // 存档按钮图片数组缓存

        #endregion

        #region 嵌套类型

        /// <summary>
        /// 存档面板模式
        /// </summary>
        public enum ArchiveMode
        {
            /// <summary>加载模式 - 选择存档进入游戏</summary>
            Load,
            /// <summary>保存模式 - 选择位置放置存档内容</summary>
            Save
        }

        #endregion

        #region 属性

        public override int SortingOrder => 10;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 挂载组件引用 - 存档按钮
            m_SaveSlot1 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot1");
            m_SaveSlot2 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot2");
            m_SaveSlot3 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot3");
            m_SaveSlot4 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot4");
            m_SaveSlot5 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot5");
            m_SaveSlot6 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot6");
            m_SaveSlot7 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot7");
            m_SaveSlot8 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot8");
            m_SaveSlot9 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot9");
            m_SaveSlot10 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot10");
            m_SaveSlot11 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot11");
            m_SaveSlot12 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/SaveSlot12");

            // 挂载组件引用 - 其他UI
            m_ButtonBack = this.GetComponentByPath<Button>("Canvas/Background/ButtonBack");
            m_TitleText = this.GetComponentByPath<TextMeshProUGUI>("Canvas/Background/TitleText");

            // 初始化存档按钮数组
            m_SaveSlots = new[]
            {
                m_SaveSlot1, m_SaveSlot2, m_SaveSlot3, m_SaveSlot4, m_SaveSlot5,
                m_SaveSlot6, m_SaveSlot7, m_SaveSlot8, m_SaveSlot9, m_SaveSlot10,
                m_SaveSlot11, m_SaveSlot12
            };

            // 初始化存档按钮图片数组
            m_SaveSlotImages = new Image[m_SaveSlots.Length];
            for (int i = 0; i < m_SaveSlots.Length; i++)
            {
                if (m_SaveSlots[i] != null)
                {
                    m_SaveSlotImages[i] = m_SaveSlots[i].GetComponent<Image>();
                }
            }

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

            Debug.Log($"[ArchivePanel] OnOpen called, userData: {userData}");

            // 获取打开模式（保存模式/加载模式）
            if (userData is ArchiveMode mode)
            {
                m_Mode = mode;
                Debug.Log($"[ArchivePanel] Mode set to: {mode}");
            }
            else if (userData is bool isNewGame)
            {
                // 兼容旧的 bool 参数（true=新游戏，false=继续游戏）
                m_Mode = isNewGame ? ArchiveMode.Save : ArchiveMode.Load;
                Debug.Log($"[ArchivePanel] Mode set from bool: {m_Mode}");
            }
            else
            {
                m_Mode = ArchiveMode.Save; // 默认保存模式
                Debug.Log("[ArchivePanel] Default mode set to: Save");
            }

            // 更新标题
            UpdateTitle();

            // 更新存档槽位显示
            UpdateSaveSlotDisplay();

            Debug.Log($"[ArchivePanel] Opened successfully, Mode: {m_Mode}");
            Debug.Log($"[ArchivePanel] SaveSystem available: {CustomEntry.SaveSystem != null}");
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
            int slotId = slotIndex + 1; // 槽位ID从1开始
            Log.Info($"[ArchivePanel] Slot {slotId} clicked, Mode: {m_Mode}");
            m_SelectedSlotIndex = slotIndex;

            if (m_Mode == ArchiveMode.Save)
            {
                // 保存模式：执行保存操作
                OnSaveSlotSelected(slotId);
            }
            else
            {
                // 加载模式：执行加载操作
                OnLoadSlotSelected(slotId);
            }
        }

        /// <summary>
        /// 存档槽位被选中（保存模式）
        /// </summary>
        private void OnSaveSlotSelected(int slotId)
        {
            Log.Info($"[ArchivePanel] Saving to slot {slotId}");

            // 执行保存操作
            bool success = CustomEntry.SaveSystem?.Save(slotId) ?? false;

            if (success)
            {
                Log.Info($"[ArchivePanel] 保存成功：槽位 {slotId}");
                // 更新存档显示
                UpdateSaveSlotDisplay();
            }
            else
            {
                Log.Error($"[ArchivePanel] 保存失败：槽位 {slotId}");
                // TODO: 显示保存失败提示
            }
        }

        /// <summary>
        /// 存档槽位被选中（加载模式）
        /// </summary>
        private void OnLoadSlotSelected(int slotId)
        {
            Log.Info($"[ArchivePanel] Loading from slot {slotId}");

            // 执行加载操作
            bool success = CustomEntry.SaveSystem?.Load(slotId) ?? false;

            if (success)
            {
                Log.Info($"[ArchivePanel] 加载成功：槽位 {slotId}");

                // 关闭存档界面
                CloseSelf();

                // TODO: 根据加载的游戏状态跳转到相应的界面
                // 暂时直接打开对话界面（用于新游戏流程）
                // 实际应该根据存档中的剧情状态跳转
            }
            else
            {
                Log.Error($"[ArchivePanel] 加载失败：槽位 {slotId}");
                // TODO: 显示加载失败提示
            }
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
                m_TitleText.text = m_Mode == ArchiveMode.Load ? "继续游戏" : "保存游戏";
            }
        }

        /// <summary>
        /// 更新存档槽位显示
        /// </summary>
        private void UpdateSaveSlotDisplay()
        {
            for (int i = 0; i < m_SaveSlots.Length; i++)
            {
                if (m_SaveSlots[i] == null || m_SaveSlotImages[i] == null)
                    continue;

                int slotId = i + 1; // 槽位ID从1开始
                bool hasSave = HasSaveData(slotId);

                // 根据模式设置按钮可点击状态
                if (m_Mode == ArchiveMode.Load)
                {
                    // 加载模式：只有有存档的按钮可点击
                    m_SaveSlots[i].interactable = hasSave;
                }
                else
                {
                    // 保存模式：所有按钮都可点击
                    m_SaveSlots[i].interactable = true;
                }

                // 设置按钮图片颜色（空存档为灰色）
                if (hasSave)
                {
                    // 有存档：正常显示
                    m_SaveSlotImages[i].color = Color.white;
                }
                else
                {
                    // 无存档：灰色显示
                    m_SaveSlotImages[i].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
        }

        /// <summary>
        /// 检查是否有存档数据
        /// </summary>
        private bool HasSaveData(int slotId)
        {
            return CustomEntry.SaveSystem?.HasSave(slotId) ?? false;
        }

        #endregion
    }
}