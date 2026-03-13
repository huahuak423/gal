//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using GameMain.Scripts.Base;
using GameEntry = GameMain.Scripts.Base.GameEntry;

namespace AVGGame.Procedure
{
    /// <summary>
    /// 主菜单流程 - 显示游戏主界面、开始游戏按钮等
    /// </summary>
    public class ProcedureMainMenu : ProcedureBase
    {
        #region 生命周期

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedureMainMenu] Enter main menu procedure");

            // TODO: 打开主菜单 UI
            // GameEntry.UI.OpenUIForm(UIFormId.MainMenu);

            // TODO: 播放背景音乐
            // GameEntry.Sound.PlayMusic(AudioId.BGM_MainMenu);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // TODO: 处理主菜单逻辑
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            // TODO: 关闭主菜单 UI
            // GameEntry.UI.CloseUIForm(UIFormId.MainMenu);

            Log.Info("[ProcedureMainMenu] Leave main menu procedure");
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            // TODO: 初始化新游戏数据
            // GameEntry.DataNode.SetData("NewGame", true);

            // 切换到游戏流程
            // ChangeState<ProcedureGame>(procedureOwner);
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void ContinueGame()
        {
            // TODO: 加载存档
            // GameEntry.Save.LoadLastSave();

            // 切换到游戏流程
            // ChangeState<ProcedureGame>(procedureOwner);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            GameEntry.ShutdownGame(ShutdownType.Quit);
        }

        #endregion
    }
}
