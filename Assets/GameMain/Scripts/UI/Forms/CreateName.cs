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
    /// 起名面板 - 玩家输入名称并确认
    /// </summary>
    public class CreateName : UIFormBase
    {
        #region 序列化字段

        [Header("按钮")]
        [SerializeField] private Button CorrectName;           // 确认名称（CreatePlate）
        [SerializeField] private Button CorrectNameTrue;        // 确认确认（CorrectPlate）
        [SerializeField] private Button CancelName;            // 取消（CorrectPlate）
        [SerializeField] private Button CorrectMessage;         // 确认消息（MessagePlate）

        [Header("文本")]
        [SerializeField] private Text PlayerName;              // 名称显示
        [SerializeField] private Text Message;                 // 消息提示
        [SerializeField] private Text NameCorrect;             // "名称已确认"（CreatePlate）
        [SerializeField] private Text MessageCorrect;           // "消息已确认"（MessagePlate）
        [SerializeField] private Text NameCorrectTrue;          // 确认按钮文本（CorrectPlate）
        [SerializeField] private Text NameCancelTrue;           // 取消按钮文本（CorrectPlate）
        [SerializeField] private Text Tips;                    // 提示文字（CorrectPlate）
        [SerializeField] private Text Warning;                 // 警告文字（CorrectPlate）

        [Header("输入框")]
        [SerializeField] private TMP_InputField m_InputField;  // 名称输入框

        [Header("面板")]
        [SerializeField] private Transform CreatePlate;        // 输入面板（初始显示）
        [SerializeField] private Transform CorrectPlate;       // 确认面板
        [SerializeField] private Transform MessagePlate;         // 消息面板

        #endregion

        #region 私有字段

        private const int MaxNameLength = 8;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Debug.Log("[CreateName] OnInit 开始");

            // 使用序列化字段（已赋值的组件）而非重新查找
            CorrectName = this.GetComponentByPath<Button>("Canvas/Background/CreatePlate/ButtonCorrectName");
            CorrectNameTrue = this.GetComponentByPath<Button>("Canvas/Background/CorrectPlate/ButtonCorrect");
            CancelName = this.GetComponentByPath<Button>("Canvas/Background/CorrectPlate/ButtonCancel");
            CorrectMessage = this.GetComponentByPath<Button>("Canvas/Background/MessagePlate/ButtonCorrect");
            // 通过 GetComponentByPath 获取其他组件

            m_InputField = this.GetComponentByPath<TMP_InputField>("Canvas/Background/CreatePlate/InputField");
            PlayerName = this.GetComponentByPath<Text>("Canvas/Background/CreatePlate/InputField/Text");
            Message = this.GetComponentByPath<Text>("Canvas/Background/MessagePlate/TextConstMessageTips");
            NameCorrect = this.GetComponentByPath<Text>("Canvas/Background/CreatePlate/ButtonCorrectName/TextCorrect");
            MessageCorrect = this.GetComponentByPath<Text>("Canvas/Background/MessagePlate/ButtonCorrect/TextCorrect");
            NameCorrectTrue = this.GetComponentByPath<Text>("Canvas/Background/CorrectPlate/ButtonCorrect/TextCorrect");
            NameCancelTrue = this.GetComponentByPath<Text>("Canvas/Background/CorrectPlate/ButtonCancel/TextCancel");
            Tips = this.GetComponentByPath<Text>("Canvas/Background/CorrectPlate/TextTips");
            Warning = this.GetComponentByPath<Text>("Canvas/Background/CorrectPlate/TextWarning");

            // 获取面板引用
            CreatePlate = this.GetComponentByPath<Transform>("Canvas/Background/CreatePlate");
            CorrectPlate = this.GetComponentByPath<Transform>("Canvas/Background/CorrectPlate");
            MessagePlate = this.GetComponentByPath<Transform>("Canvas/Background/MessagePlate");


            // 绑定 InputField 输入变化事件，同步到 PlayerName
            if (m_InputField != null)
            {
                m_InputField.onValueChanged.AddListener((value) =>
                {
                    if (PlayerName != null)
                    {
                        PlayerName.text = value;
                    }
                });
                Debug.Log("[CreateName] OnInit - m_InputField onValueChanged 绑定成功");
            }
            else
            {
                Debug.LogError("[CreateName] OnInit - m_InputField 为空！");
            }

            // 绑定按钮事件
            if (CorrectName != null)
            {
                CorrectName.onClick.AddListener(OnConfirmClick);
                Debug.Log("[CreateName] OnInit - CorrectName 事件绑定成功");
            }
            else
            {
                Debug.LogError("[CreateName] OnInit - CorrectName 为空，无法绑定事件！");
            }

            if (CorrectNameTrue != null)
            {
                CorrectNameTrue.onClick.AddListener(OnCorrectClick);
                Debug.Log("[CreateName] OnInit - CorrectNameTrue 事件绑定成功");
            }
            if (CancelName != null)
            {
                CancelName.onClick.AddListener(OnCancelClick);
                Debug.Log("[CreateName] OnInit - CancelName 事件绑定成功");
            }
            if (CorrectMessage != null)
            {
                CorrectMessage.onClick.AddListener(OnSureClick);
                Debug.Log("[CreateName] OnInit - CorrectMessage 事件绑定成功");
            }

            // ====== 图片文件挂载 ======
            // CorrectName.image      — Canvas/Background/CreatePlate/ButtonCorrectName
            // CorrectNameTrue.image  — Canvas/Background/CorrectPlate/ButtonCorrect
            // CancelName.image        — Canvas/Background/CorrectPlate/ButtonCancel
            // CorrectMessage.image    — Canvas/Background/MessagePlate/ButtonCorrect
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 初始状态：只显示 CreatePlate，隐藏其他两个面板
            if (CreatePlate != null)
            {
                CreatePlate.gameObject.SetActive(true);
            }
            if (CorrectPlate != null)
                CorrectPlate.gameObject.SetActive(false);

            if (MessagePlate != null)
                MessagePlate.gameObject.SetActive(false);

            // 重置输入
            if (Message != null)
                Message.gameObject.SetActive(false);

            if (PlayerName != null)
                PlayerName.text = "";

            if (m_InputField != null)
            {
                m_InputField.text = "";
                m_InputField.Select();
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            // 通过 SaveLoadContext 回调通知 ProcedureMainMenu 起名完成
            Debug.Log($"[CreateName] OnClose - 回调前，OnCreateNameComplete={(SaveLoadContext.OnCreateNameComplete != null ? "非空" : "空")}");
            SaveLoadContext.OnCreateNameComplete?.Invoke();
            SaveLoadContext.OnCreateNameComplete = null;
        }

        #endregion

        #region 面板控制

        private void HideCorrectPlate()
        {
            if (CorrectPlate != null)
                CorrectPlate.gameObject.SetActive(false);
        }

        private void HideMessagePlate()
        {
            if (MessagePlate != null)
                MessagePlate.gameObject.SetActive(false);
        }

        #endregion

        #region 按钮回调

        /// <summary>
        /// CorrectName - 名称输入面板的确认按钮
        /// 校验名称是否有效：非空、不超长
        /// </summary>
        private void OnConfirmClick()
        {
            // 直接从 PlayerName 获取文本（InputField 下的 Text 子物体）
            string name = PlayerName != null ? PlayerName.text : "";
            Debug.Log($"[CreateName] OnConfirmClick - 获取到的名称: '{name}'");

            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.Log("[CreateName] OnConfirmClick - 名称为空，显示警告");
                MessagePlate.gameObject.SetActive(true);
                if (Message != null)
                {
                    Message.gameObject.SetActive(true);
                    Message.text = "请输入名称";
                }
                return;
            }

            if (name.Length > MaxNameLength)
            {
                Debug.Log($"[CreateName] OnConfirmClick - 名称过长: {name.Length} > {MaxNameLength}");
                if (Message != null)
                {
                    MessagePlate.gameObject.SetActive(true);
                    Message.gameObject.SetActive(true);
                    Message.text = $"名称不能超过{MaxNameLength}个字";
                }
                return;
            }

            // 名称有效，显示到确认面板并弹出确认面板
            Debug.Log("[CreateName] OnConfirmClick - 名称有效，显示CorrectPlate");

            if (Message != null)
            {
                Message.gameObject.SetActive(true);
                Message.text = "欢迎您，希望您玩的开心";
            }

            if (CorrectPlate != null)
            {
                CorrectPlate.gameObject.SetActive(true);
                Debug.Log("[CreateName] OnConfirmClick - CorrectPlate 已显示");
            }
        }

        /// <summary>
        /// CorrectNameTrue - 确认面板的确认按钮
        /// 隐藏确认面板，弹出消息面板
        /// </summary>
        private void OnCorrectClick()
        {
            string name = PlayerName != null ? PlayerName.text : "";

            if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength)
            {
                Debug.Log("[CreateName] OnCorrectClick - 名称无效，隐藏CorrectPlate");
                HideCorrectPlate();
                return;
            }

            Debug.Log("[CreateName] OnCorrectClick - 显示MessagePlate");
            HideCorrectPlate();

            if (MessagePlate != null)
            {
                MessagePlate.gameObject.SetActive(true);
                Debug.Log("[CreateName] OnCorrectClick - MessagePlate 已显示");
            }
        }

        /// <summary>
        /// CancelName - 确认面板的取消按钮
        /// 隐藏确认面板，回到输入面板
        /// </summary>
        private void OnCancelClick()
        {
            Debug.Log("[CreateName] OnCancelClick 被调用");
            HideCorrectPlate();
        }

        /// <summary>
        /// CorrectMessage - 消息面板的确认按钮
        /// 保存名称并关闭面板
        /// </summary>
        private void OnSureClick()
        {
            string name = PlayerName != null ? PlayerName.text : "";

            if (!string.IsNullOrWhiteSpace(name) && name.Length <= MaxNameLength)
            {
                CustomEntry.PlayerData?.SetPlayerName(name);
                Debug.Log($"[CreateName] 名称已保存: {name}");            
                CloseSelf();
            }
            else
            {
                Debug.LogWarning($"[CreateName] OnSureClick - 名称无效: '{name}'");
                MessagePlate.gameObject.SetActive(false);
            }

            if (CorrectPlate != null)
                CorrectPlate.gameObject.SetActive(false);

        }

        #endregion
    }
}
