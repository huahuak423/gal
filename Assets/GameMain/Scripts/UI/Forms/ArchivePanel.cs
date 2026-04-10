//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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

        [SerializeField] private Button m_ConfirmButton;
        [SerializeField] private Button m_CancelButton;

        [Header("确认面板")]
        [SerializeField] private Transform m_ConfirmPlate;

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
        private bool m_IsFromGameMenu = false;           // 是否从游戏菜单（MenuPanel）进入
        private ProcedureGame m_ProcedureGame = null;     // 游戏流程引用
        private string m_CurrentStoryToLoad = string.Empty; // 要加载的故事名称
        private int m_CurrentLoadSlot = -1;              // 当前加载的存档槽位

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
            m_SaveSlot1 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb1/ButtonNowSave");
            m_SaveSlot2 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb2/ButtonNowSave");
            m_SaveSlot3 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb3/ButtonNowSave");
            m_SaveSlot4 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb4/ButtonNowSave");
            m_SaveSlot5 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb5/ButtonNowSave");
            m_SaveSlot6 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb6/ButtonNowSave");
            m_SaveSlot7 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb7/ButtonNowSave");
            m_SaveSlot8 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb8/ButtonNowSave");
            m_SaveSlot9 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb9/ButtonNowSave");
            m_SaveSlot10 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb10/ButtonNowSave");
            m_SaveSlot11 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb11/ButtonNowSave");
            m_SaveSlot12 = this.GetComponentByPath<Button>("Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb12/ButtonNowSave");

            m_ConfirmButton = this.GetComponentByPath<Button>("Canvas/Background/ConfirmPlate/ButtonConfirm");
            m_CancelButton = this.GetComponentByPath<Button>("Canvas/Background/ConfirmPlate/ButtonCancel");
            m_ConfirmPlate = this.GetComponentByPath<Transform>("Canvas/Background/ConfirmPlate");

            // 绑定确认和取消按钮事件
            if (m_ConfirmButton != null)
            {
                m_ConfirmButton.onClick.AddListener(OnConfirmClick);
            }
            if (m_CancelButton != null)
            {
                m_CancelButton.onClick.AddListener(OnCancelClick);
            }
            
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

            // 检查存档按钮获取情况（仅在错误时输出）
            for (int i = 0; i < m_SaveSlots.Length; i++)
            {
                if (m_SaveSlots[i] == null)
                {
                    Debug.LogError($"[ArchivePanel] SaveSlot{i + 1} is null! Path: Canvas/Background/SavePlate/ScrollView/Viewport/Content/SavePrefeb{i + 1}");
                }
            }

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

            // Debug.Log($"[ArchivePanel] OnOpen called, userData: {userData}");

            // 隐藏确认面板
            HideConfirmPlate();

            // 检查是否从游戏菜单进入
            m_IsFromGameMenu = userData is ProcedureGame;
            m_ProcedureGame = userData as ProcedureGame;

            // 获取打开模式（保存模式/加载模式）
            if (userData is ArchiveMode mode)
            {
                m_Mode = mode;
                // Debug.Log($"[ArchivePanel] Mode set to: {mode}");
            }
            else if (userData is bool isNewGame)
            {
                // 兼容旧的 bool 参数（true=新游戏，false=继续游戏）
                m_Mode = isNewGame ? ArchiveMode.Save : ArchiveMode.Load;
                // Debug.Log($"[ArchivePanel] Mode set from bool: {m_Mode}");
            }
            else
            {
                m_Mode = ArchiveMode.Save; // 默认保存模式
                // Debug.Log("[ArchivePanel] Default mode set to: Save");
            }

            // 更新标题
            UpdateTitle();

            // 更新存档槽位显示
            UpdateSaveSlotDisplay();

            // Debug.Log($"[ArchivePanel] Opened successfully, Mode: {m_Mode}, FromGameMenu: {m_IsFromGameMenu}");
            // Debug.Log($"[ArchivePanel] SaveSystem available: {CustomEntry.SaveSystem != null}");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            HideConfirmPlate();
            m_SelectedSlotIndex = -1;
        }

        #endregion

        #region 按钮事件

        /// <summary>
        /// 存档槽位点击
        /// </summary>
        private void OnSaveSlotClick(int slotIndex)
        {
            Debug.Log($"[ArchivePanel] OnSaveSlotClick called, slotIndex: {slotIndex}");
            int slotId = slotIndex + 1; // 槽位ID从1开始
            // Debug.Log($"[ArchivePanel] Slot {slotId} clicked, Mode: {m_Mode}");
            m_SelectedSlotIndex = slotIndex;

            // 显示确认面板
            ShowConfirmPlate();
        }

        /// <summary>
        /// 确认按钮点击
        /// </summary>
        private void OnConfirmClick()
        {
            if (m_SelectedSlotIndex < 0)
            {
                Debug.LogWarning("[ArchivePanel] No slot selected");
                return;
            }

            int slotId = m_SelectedSlotIndex + 1;

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

            // 隐藏确认面板
            HideConfirmPlate();
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void OnCancelClick()
        {
            Debug.Log("[ArchivePanel] Cancel clicked");
            m_SelectedSlotIndex = -1;
            HideConfirmPlate();
        }

        /// <summary>
        /// 显示确认面板
        /// </summary>
        private void ShowConfirmPlate()
        {
            if (m_ConfirmPlate != null)
            {
                m_ConfirmPlate.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏确认面板
        /// </summary>
        private void HideConfirmPlate()
        {
            if (m_ConfirmPlate != null)
            {
                m_ConfirmPlate.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 存档槽位被选中（保存模式）
        /// </summary>
        private void OnSaveSlotSelected(int slotId)
        {
            // Debug.Log($"[ArchivePanel] Saving to slot {slotId}");

            // 显示保存提示
            ShowMessage("正在保存...");

            // 异步执行保存操作
            StartCoroutine(SaveWithRetry(slotId));
        }

        /// <summary>
        /// 带重试的保存操作
        /// </summary>
        private System.Collections.IEnumerator SaveWithRetry(int slotId, int maxRetries = 2)
        {
            bool success = false;
            int retryCount = 0;

            while (!success && retryCount < maxRetries)
            {
                retryCount++;
                success = CustomEntry.SaveSystem?.Save(slotId) ?? false;

                if (!success && retryCount < maxRetries)
                {
                    Debug.LogWarning($"[ArchivePanel] 保存失败，正在重试 ({retryCount}/{maxRetries})");
                    yield return new WaitForSeconds(0.5f);
                }
            }

            if (success)
            {
                // Debug.Log($"[ArchivePanel] 保存成功：槽位 {slotId}");
                ShowMessage("保存成功！");

                // 延迟后关闭存档界面，并返回大菜单
                DelayAndReturnToMainMenu();
            }
            else
            {
                Debug.LogError($"[ArchivePanel] 保存失败：槽位 {slotId}（重试{maxRetries}次后仍失败）");
                ShowMessage("保存失败，请稍后重试");
            }
        }

        /// <summary>
        /// 存档槽位被选中（加载模式）
        /// </summary>
        private void OnLoadSlotSelected(int slotId)
        {
            // Debug.Log($"[ArchivePanel] Loading from slot {slotId}");
            m_CurrentLoadSlot = slotId; // 保存当前加载的槽位

            // 显示加载提示
            ShowMessage("正在读取存档...");

            // 异步执行加载操作
            StartCoroutine(LoadWithRetry(slotId));
        }

        /// <summary>
        /// 带重试的加载操作
        /// </summary>
        private System.Collections.IEnumerator LoadWithRetry(int slotId, int maxRetries = 2)
        {
            bool success = false;
            int retryCount = 0;

            while (!success && retryCount < maxRetries)
            {
                retryCount++;
                success = CustomEntry.SaveSystem?.Load(slotId) ?? false;

                if (!success && retryCount < maxRetries)
                {
                    Debug.LogWarning($"[ArchivePanel] 加载失败，正在重试 ({retryCount}/{maxRetries})");
                    yield return new WaitForSeconds(0.5f);
                }
            }

            if (success)
            {
                // Debug.Log($"[ArchivePanel] 加载成功：槽位 {slotId}");
                ShowMessage("加载成功！");

                // 获取加载后的剧情状态
                var playerData = CustomEntry.PlayerData;
                string currentStory = playerData?.currentStoryGarphName;
                // Debug.Log($"[ArchivePanel] 加载后的 currentStory: '{currentStory}'");

                // 设置SaveLoadContext - 这是为了在流程切换时保持数据
                SaveLoadContext.SetSaveLoadContext(slotId, currentStory);

                // 等待一帧，确保Context被设置
                yield return null;

                // 使用SaveLoadManager处理流程切换（在关闭UI之前调用）
                var manager = SaveLoadManager.Instance;
                if (manager != null)
                {
                    manager.StartCoroutine(manager.LoadAndEnterGame(slotId));
                }
                else
                {
                    Debug.LogError("[ArchivePanel] SaveLoadManager instance is null!");
                }

                // 关闭存档界面
                CloseSelf();
            }
            else
            {
                // Debug.LogError($"[ArchivePanel] 加载失败：槽位 {slotId}（重试{maxRetries}次后仍失败）");
                ShowMessage("加载失败，请稍后重试");
            }
        }

        /// <summary>
        /// 显示提示消息
        /// </summary>
        private void ShowMessage(string message)
        {
            // 这里可以扩展实现更复杂的提示UI
            // 比如使用 UnityEngine.UI.Text 或其他UI组件显示消息
            // Debug.Log($"[ArchivePanel] {message}");
        }

        /// <summary>
        /// 返回按钮点击
        /// </summary>
        private void OnBackClick()
        {
            // 如果确认面板正在显示，先关闭它
            if (m_ConfirmPlate != null && m_ConfirmPlate.gameObject.activeSelf)
            {
                // Debug.Log("[ArchivePanel] Confirm plate is open, closing it first");
                HideConfirmPlate();
                m_SelectedSlotIndex = -1;
                return;
            }

            // Debug.Log("[ArchivePanel] Back clicked");
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

        /// <summary>
        /// 延迟后重新打开菜单（用于保存成功后）
        /// </summary>
        private void DelayAndOpenMenu()
        {
            // 检查游戏对象是否仍然 active
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[ArchivePanel] Game object is inactive, cannot open menu");
                return;
            }

            // 使用 Unity 的 Invoke 来延迟执行，避免协程问题
            Invoke(nameof(OpenMenuInternal), 1.5f);
        }

        private void OpenMenuInternal()
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[ArchivePanel] Game object is inactive when trying to open menu");
                return;
            }

            // 获取当前活动的 Procedure
            var currentProcedure = GameEntry.Procedure?.CurrentProcedure as ProcedureGame;
            if (currentProcedure != null)
            {
                currentProcedure.OpenMenu();
            }
            else
            {
                Debug.LogWarning("[ArchivePanel] CurrentProcedure is null, cannot open menu");
            }
        }

        /// <summary>
        /// 延迟后返回大菜单（用于"回到主菜单"流程）
        /// </summary>
        private void DelayAndReturnToMainMenu()
        {
            // 先关闭界面
            CloseSelf();

            // 检查游戏对象是否仍然 active
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[ArchivePanel] Game object is inactive, cannot return to main menu");
                return;
            }

            // 使用 Unity 的 Invoke 来延迟执行，避免协程问题
            Invoke(nameof(ReturnToMainMenuInternal), 0.5f);
        }

        private void ReturnToMainMenuInternal()
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[ArchivePanel] Game object is inactive when trying to return to main menu");
                return;
            }

            // 获取当前活动的 Procedure
            var currentProcedure = GameEntry.Procedure?.CurrentProcedure as ProcedureGame;
            if (currentProcedure != null)
            {
                // Debug.Log("[ArchivePanel] 返回大菜单");
                currentProcedure.ReturnToMainMenu();
            }
            else
            {
                Debug.LogWarning("[ArchivePanel] CurrentProcedure is null, cannot return to main menu");
            }
        }

        /// <summary>
        /// 延迟后加载游戏（用于加载成功后）
        /// </summary>
        private void LoadGameInternal()
        {
            // 这个方法已经不再使用，被 SaveLoadManager 替代
            Debug.LogWarning("[ArchivePanel] LoadGameInternal is deprecated, using SaveLoadManager instead");
        }

        /// <summary>
        /// 根据故事名称获取事件ID（临时实现，需要根据实际项目调整）
        /// </summary>
        private int GetEventIdFromStoryName(string storyName)
        {
            // 这里需要根据项目的实际逻辑来获取对应的事件ID
            // 临时实现：假设故事名称对应事件表中的某个事件ID
            // 实际项目中应该有一个映射关系或查询方法

            // 示例：查找EventRowData表中Title匹配storyName的记录
            try
            {
                if (GameEntry.DataTable.HasDataTable<EventRowData>())
                {
                    var eventTable = GameEntry.DataTable.GetDataTable<EventRowData>();
                    foreach (var eventData in eventTable)
                    {
                        if (eventData.Title == storyName || eventData.Id.ToString() == storyName)
                        {
                            return eventData.Id;
                        }
                    }
                }
                Debug.LogWarning($"[ArchivePanel] 未找到匹配的故事: {storyName}，默认返回1");
                return 1; // 默认返回第一个事件
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ArchivePanel] 根据故事名称查找事件ID失败: {e.Message}");
                return 1; // 出错时返回默认值
            }
        }

        #endregion
    }
}