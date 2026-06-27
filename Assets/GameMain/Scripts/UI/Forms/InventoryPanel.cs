using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    public class InventoryPanel : UIFormBase
    {
        public override int SortingOrder => 250;

        [Header("主要面板")]
        [SerializeField] private Image m_Background;
        [SerializeField] private Transform m_InventoryPlate;
        [SerializeField] private Image m_InventoryPlatePhoto;

        [Header("承诺/进度")]
        [SerializeField] private Transform m_PromisePlate;
        [SerializeField] private Image m_PromisePlatePhoto;

        [SerializeField] private Text m_TextPromise;

        [Header("物品栏")]
        [SerializeField] private Button m_ButtonClose;
        [SerializeField] private Button m_ButtonExit;

        [Header("装饰图")]        
        [SerializeField] private Image m_Promise1;
        [SerializeField] private Image m_Promise2;
        [SerializeField] private Image m_Promise3;
        [SerializeField] private Image m_Promise4;
        [SerializeField] private Image m_Promise5;

        private ProcedureGame m_ProcedureGame;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 主要面板
            m_Background = this.GetComponentByPath<Image>("Canvas/Background");
            m_InventoryPlate = this.GetComponentByPath<Transform>("Canvas/Background/InventoryPlate");
            m_InventoryPlatePhoto = this.GetComponentByPath<Image>("Canvas/Background/InventoryPlate");

            // 承诺/进度
            m_PromisePlate = this.GetComponentByPath<Transform>("Canvas/Background/PromisePlate");
            m_PromisePlatePhoto= this.GetComponentByPath<Image>("Canvas/Background/PromisePlate");
            m_Promise1= this.GetComponentByPath<Image>("Canvas/Background/InventoryPlate/ImagePromise1");
            m_Promise2= this.GetComponentByPath<Image>("Canvas/Background/InventoryPlate/ImagePromise2");
            m_Promise3= this.GetComponentByPath<Image>("Canvas/Background/InventoryPlate/ImagePromise3");
            m_Promise4= this.GetComponentByPath<Image>("Canvas/Background/InventoryPlate/ImagePromise4");
            m_Promise5= this.GetComponentByPath<Image>("Canvas/Background/InventoryPlate/ImagePromise5");
            m_ButtonExit= this.GetComponentByPath<Button>("Canvas/Background/ButtonExit");
            m_TextPromise = this.GetComponentByPath<Text>("PromisePlate");
            m_ButtonClose = this.GetComponentByPath<Button>("Canvas/Background/PromisePlate/ButtonClose");


            if (m_ButtonClose != null)
            {
                m_ButtonClose.onClick.AddListener(OnButtonCloseClick);
            }

            // ====== 图片文件挂载 ======
            // m_ButtonClose.image — Canvas/Background/PromisePlate/ButtonClose
            // m_ButtonExit.image  — Canvas/Background/ButtonExit
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (userData is ProcedureGame procedureGame)
            {
                m_ProcedureGame = procedureGame;
            }

            RefreshInventory();
            Log.Info("[InventoryPanel] OnOpen");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            Log.Info("[InventoryPanel] OnClose");
        }

        private void OnButtonCloseClick()
        {
            CloseSelf();
        }

        private void RefreshInventory()
        {
            // TODO: 从游戏数据中读取并刷新物品栏
            Log.Info("[InventoryPanel] RefreshInventory");
        }
    }
}