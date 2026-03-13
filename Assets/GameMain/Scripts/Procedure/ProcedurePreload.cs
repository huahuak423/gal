//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace AVGGame.Procedure
{
    /// <summary>
    /// 预加载流程 - 游戏启动后的第一个流程
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedurePreload] Enter preload procedure");

            // TODO: 在这里进行资源预加载、数据表加载等

            // 加载完成后切换到主菜单流程
            // ChangeState<ProcedureMainMenu>(procedureOwner);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
                base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

                // TODO: 更新加载进度
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
                base.OnLeave(procedureOwner, isShutdown);
            Log.Info("[ProcedurePreload] Leave preload procedure");
        }
    }
}
