using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    public class MapPanel : UIFormBase
    {
        #region 字段 
        [Header("地图按钮")]
        [SerializeField] private Button m_ButtonPlace1;
        [SerializeField] private Button m_ButtonPlace2;
        [SerializeField] private Button m_ButtonPlace3;
        [SerializeField] private Button m_ButtonPlace4;
        [SerializeField] private Button m_ButtonPlace5;
        [SerializeField] private Button m_ButtonPlace6;
        [SerializeField] private Button m_ButtonPlace7;
        [SerializeField] private Button m_ButtonPlace8;
        
        #endregion
        
        #region 属性
        
        #endregion
        
        #region 生命周期
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            if (m_ButtonPlace1 != null)
            {
                m_ButtonPlace1.onClick.AddListener(OnButtonPlace1Click);
            }
        }
        #endregion
        
        #region 按钮事件
        private void OnButtonPlace1Click()
        {
            Log.Info("[Place1Click] clicked");
            CloseSelf();
            //选择地图1
        }
        #endregion
        
        #region 公共方法
        
        #endregion
    }
}