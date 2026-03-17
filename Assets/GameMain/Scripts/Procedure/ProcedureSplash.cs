//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    /// <summary>
    /// 启动画面流程 - 显示游戏 Logo、加载提示等
    /// </summary>
    public class ProcedureSplash : ProcedureBase
    {
        #region 字段

        private float m_ElapsedTime = 0f;
        private float m_MinSplashTime = 0.5f; // 最少显示时间

        #endregion

        #region 生命周期

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedureSplash] Enter");

            m_ElapsedTime = 0f;

            // TODO: 打开启动画面 UI
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            m_ElapsedTime += elapseSeconds;

            if (m_ElapsedTime >= m_MinSplashTime)
            {
                ChangeState<ProcedureMainMenu>(procedureOwner);
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            Log.Info("[ProcedureSplash] Leave");
        }

        #endregion
    }
}