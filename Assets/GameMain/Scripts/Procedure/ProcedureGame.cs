//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.Scripts.Base.GameEntry;

namespace AVGGame.Procedure
{
    /// <summary>
    /// 游戏主流程 - AVG 游戏的核心流程，处理对话、剧情等
    /// </summary>
    public class ProcedureGame : ProcedureBase
    {
        #region 字段

        private bool m_IsPaused = false;

        #endregion

        #region 生命周期

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedureGame] Enter game procedure");

            // TODO: 打开游戏主界面 UI
            // GameEntry.UI.OpenUIForm(UIFormId.GameMain);

            // TODO: 初始化游戏数据
            InitializeGame(procedureOwner);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_IsPaused)
            {
                return;
            }

            // TODO: 更新游戏逻辑
            // 例如：对话系统、剧情系统等
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            // TODO: 清理游戏数据
            CleanupGame();

            Log.Info("[ProcedureGame] Leave game procedure");
        }

        #endregion

        #region 游戏控制

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            m_IsPaused = true;
            Log.Info("[ProcedureGame] Game paused");
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            m_IsPaused = false;
            Log.Info("[ProcedureGame] Game resumed");
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu(IFsm<IProcedureManager> procedureOwner)
        {
            ChangeState<ProcedureMainMenu>(procedureOwner);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame(IFsm<IProcedureManager> procedureOwner)
        {
            // TODO: 加载场景
            // GameEntry.Scene.LoadScene(SceneId.GameScene);

            // TODO: 加载角色
            // GameEntry.Entity.ShowEntity<Entity>(EntityId.Player);

            // TODO: 开始对话
            // GameEntry.Dialog.StartDialog(DialogId.Introduction);
        }

        /// <summary>
        /// 清理游戏
        /// </summary>
        private void CleanupGame()
        {
            // TODO: 卸载场景
            // GameEntry.Scene.UnloadScene(SceneId.GameScene);

            // TODO: 隐藏所有实体
            // GameEntry.Entity.HideAllLoadedEntities();
        }

        #endregion
    }
}
