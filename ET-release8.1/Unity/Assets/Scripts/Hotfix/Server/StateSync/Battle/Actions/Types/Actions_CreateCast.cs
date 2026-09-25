using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [Actions(ActionsType.CreateCast)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    public class Actions_CreateCast : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            RunAsync(actions, actionsRunType).Coroutine();
        }

        public async ETTask RunAsync(Actions actions, ActionsRunType actionsRunType)
        {
            Cast cast = actions.CastSelf;
            if (cast == null || cast.IsDisposed || actionsRunType != ActionsRunType.CastFinish)
            {
                return;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 1)
            {
                Log.Error($"Actions_CreateCast ActionsParam invalid: configId={config.Id}");
                return;
            }

            int castConfigId = config.ActionsParam[0];
            Unit unit = cast.Caster;
            if (unit == null || unit.IsDisposed || !unit.IsBattleUnit())
            {
                return;
            }

            long inputUnitId = cast.InputUnitId;
            float3 inputPos = cast.InputPos;
            List<long> targets = null;
            if (cast.Targets.Count > 0)
            {
                targets = new List<long>(cast.Targets);
            }

            await unit.Root().GetComponent<TimerComponent>().WaitFrameAsync();

            if (unit.IsDisposed)
            {
                return;
            }

            unit.CreateAndCast(castConfigId, inputUnitId, inputPos, false, targets);
        }
    }
}
