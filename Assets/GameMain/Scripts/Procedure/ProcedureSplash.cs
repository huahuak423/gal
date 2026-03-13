//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameMain.Scripts.Base;

namespace AVGGame.Procedure
{
    /// <summary>
    /// 启动画面流程 - 显示游戏 Logo、加载提示等
    /// </summary>
    public class ProcedureSplash : ProcedureBase
    {
        #region 字段

        private float m_ElapsedTime = 0f;
        private float m_MinSplashTime = 2f; // 最少显示时间

        #endregion

        #region 生命周期

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("[ProcedureSplash] Enter splash procedure");

            m_ElapsedTime = 0f;

            // TODO: 打开启动画面 UI
            // GameEntry.UI.OpenUIForm(UIFormId.Splash);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            m_ElapsedTime += elapseSeconds;

            // 确保启动画面至少显示一段时间
            if (m_ElapsedTime >= m_MinSplashTime)
            {
                // TODO: 检查是否加载完成
                // 如果需要等待资源加载，可以在这里检查

                // 切换到主菜单流程
                ChangeState<ProcedureMainMenu>(procedureOwner);
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            // TODO: 关闭启动画面 UI
            // GameEntry.UI.CloseUIForm(UIFormId.Splash);

            Log.Info("[ProcedureSplash] Leave splash procedure");
        }

        #endregion
    }
}
