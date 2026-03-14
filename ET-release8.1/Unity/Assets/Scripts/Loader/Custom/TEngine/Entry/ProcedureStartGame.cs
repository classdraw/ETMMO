using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ET
{
    public class ProcedureStartGame : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(IFsm<IProcedureModule> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            //正式进入游戏
            Log.Info("ProcedureStartGame:StartGame");
            //StartGame().Forget();
        }

        private async UniTaskVoid StartGame()
        {
            await UniTask.Yield();
        }
    }
}
