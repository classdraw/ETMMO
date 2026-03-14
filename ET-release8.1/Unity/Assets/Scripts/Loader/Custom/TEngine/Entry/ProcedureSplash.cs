using System.Collections;
using System.Collections.Generic;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureModule>;
using UnityEngine;
using TEngine;
using Cysharp.Threading.Tasks;

namespace ET
{
    /// <summary>
    /// 流程 => 闪屏。
    /// </summary>
    public class ProcedureSplash : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            //可以干一些框架初始化的事
            Log.Info("ProcedureSplash:EnterStartGame");
            //初始化资源包
            ChangeState<ProcedureStartGame>(procedureOwner);

        }

    }
}
