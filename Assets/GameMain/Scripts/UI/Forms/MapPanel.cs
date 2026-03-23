using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    public class MapPanel : UIFormBase
    {
        #region 字段 
        //游戏主流程引用
        private ProcedureGame m_ProcedureGame;
        
        [Header("地图按钮")]
        [SerializeField] private Button m_ButtonPlace1;
        [SerializeField] private Button m_ButtonPlace2;
        [SerializeField] private Button m_ButtonPlace3;
        [SerializeField] private Button m_ButtonPlace4;
        [SerializeField] private Button m_ButtonPlace5;
        [SerializeField] private Button m_ButtonPlace6;
        [SerializeField] private Button m_ButtonPlace7;
        [SerializeField] private Button m_ButtonPlace8;
        [SerializeField] private Button m_ButtonPlace9;
        
        [Header("--- 小地图 UI 引用 ---")]
        private List<GameObject> m_Buttonlist = new List<GameObject>();
        private Transform m_SelectPanel;
        private Button m_Button1;
        private Button m_Button2;
        private Button m_Button3;
        private Button m_Button4;
        private Button m_Button5;
        private Button m_ButtonExit;
        private Text m_TextOfSelection1;
        private Text m_TextOfSelection2;
        private Text m_TextOfSelection3;
        private Text m_TextOfSelection4;
        private Text m_TextOfSelection5;
        
        #endregion
        
        #region 属性
        
        #endregion
        
        #region 生命周期
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            m_ProcedureGame = (ProcedureGame)userData;
            
            //挂载组件引用
            m_ButtonPlace1 = this.GetComponentByPath<Button>("Canvas/Background/MapPlate/ButtonPlace1");
            m_ButtonPlace2 = this.GetComponentByPath<Button>("Canvas/Background/MapPlate/ButtonPlace2");
            m_ButtonPlace3 = this.GetComponentByPath<Button>("Canvas/Background/MapPlate/ButtonPlace3");
            m_ButtonPlace4 = this.GetComponentByPath<Button>("Canvas/Background/MapPlate/ButtonPlace4");
            m_ButtonPlace5 = this.GetComponentByPath<Button>("Canvas/Background/MapPlate/ButtonPlace5");
            
            //小地图
            
            m_SelectPanel = this.GetComponentByPath<Transform>("Canvas/Background/SelectPanel");
            m_Button1 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button1");
            m_Button2 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button2");
            m_Button3 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button3");
            m_Button4 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button4");
            m_Button5 = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/Background/Button5");
            m_ButtonExit = this.GetComponentByPath<Button>("Canvas/Background/SelectPanel/ButtonExit");
            m_TextOfSelection1 = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button1/TextOfSelection");
            m_TextOfSelection2 = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button2/TextOfSelection");
            m_TextOfSelection3 = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button3/TextOfSelection");
            m_TextOfSelection4 = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button4/TextOfSelection");
            m_TextOfSelection5 = this.GetComponentByPath<Text>("Canvas/Background/SelectPanel/Background/Button5/TextOfSelection");
            m_Buttonlist.Add(m_Button1.gameObject);
            m_Buttonlist.Add(m_Button2.gameObject);
            m_Buttonlist.Add(m_Button3.gameObject);
            m_Buttonlist.Add(m_Button4.gameObject);
            m_Buttonlist.Add(m_Button5.gameObject);
            
            if(m_SelectPanel != null)
                m_SelectPanel.gameObject.SetActive(false);
            
            if (m_ButtonExit != null)
            {
                m_ButtonExit.onClick.AddListener(OnSelectPanelExit);
            }
            
            if (m_ButtonPlace1 != null)
            {
                m_ButtonPlace1.onClick.AddListener(OnButtonPlace1Click);
            }
            
            if (m_ButtonPlace2 != null)
            {
                m_ButtonPlace2.onClick.AddListener(OnButtonPlace2Click);
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            // 注意：不要清空 m_Buttonlist，因为 OnInit 只调用一次
            // m_Buttonlist.Clear();  // ← 移除这行！

            // 隐藏选择面板
            if (m_SelectPanel != null)
            {
                m_SelectPanel.gameObject.SetActive(false);
            }

            base.OnClose(isShutdown, userData);
        }

        #endregion
        
        /// <summary>
        /// 选择地图1
        /// </summary>
        #region 按钮事件
        private void OnButtonPlace1Click()
        {
            Log.Info("[Place1Click] clicked");
            //获得该地图中已经解锁的事件列表
            List<EventRowData> eventList = m_ProcedureGame.GetVisibleEventsInMap(1);
            SelectStory(eventList);
        }
        
        /// <summary>
        /// 选择地图2
        /// </summary>
        private void OnButtonPlace2Click()
        {
            Log.Info("[Place1Click] clicked");
            //获得该地图中已经解锁的事件列表
            List<EventRowData> eventList = m_ProcedureGame.GetVisibleEventsInMap(2);
            SelectStory(eventList);
        }
        
        /// <summary>
        /// 打开并按条件显示可以进入的故事
        /// </summary>
        /// <param name="eventList"></param>
        private void SelectStory(List<EventRowData> eventList)
        {
            //打开选择面板
            m_SelectPanel.gameObject.SetActive(true);  
            //根据返回的事件数量来渲染按钮
            int dataCount = eventList != null ? eventList.Count : 0;

            for (int i = 0; i < 5; i++) //目前最多就五个按钮来渲染可选事件
            {
                GameObject nodeObj = m_Buttonlist[i];

                if (i < dataCount)
                {
                    // --- 有数据：显示节点并绑定数据 ---
                    nodeObj.SetActive(true);
                    EventRowData currentData = eventList[i];
                    
                    Text selectText = nodeObj.GetComponentInChildren<Text>();
                    Button clickBtn = nodeObj.GetComponent<Button>();

                    // 挂载名字
                    if (selectText != null)
                    {
                        selectText.text = currentData.Title;
                    }

                    // 绑定点击事件
                    if (clickBtn != null)
                    {
                        clickBtn.onClick.RemoveAllListeners();
                        int eventId = currentData.Id; // 必须存局部变量
                        clickBtn.onClick.AddListener(() => 
                        {
                            OnEventButtonClicked(eventId);
                        });
                        
                        //检测该事件是否达到进入要求，若没达到则让按钮处于暗色不可选
                        if (m_ProcedureGame.IsEventPlayable(currentData))
                        {
                            clickBtn.interactable = true;
                        }
                        else
                        {
                            clickBtn.interactable = false;
                        }
                        //TODO:表面未达成条件原因
                    }
                }
                else
                {
                    // 没数据：隐藏多余的节点
                    nodeObj.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 关闭选择面板
        /// </summary>
        private void OnSelectPanelExit()
        {
            m_SelectPanel.gameObject.SetActive(false);
        }
        
        private void OnEventButtonClicked(int eventId)
        {
            //关闭选择面板
            m_SelectPanel.gameObject.SetActive(false);
            //进入对话剧情
            m_ProcedureGame.LoadStory(eventId);
        }
        #endregion
        
        #region 公共方法
        
        #endregion
    }
}