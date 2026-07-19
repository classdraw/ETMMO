using System.Collections.Generic;

namespace ET.Server
{    
    [Actions(ActionsType.CastEmptyBullet)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(Buff))]
    public class Actions_CastEmptyBullet:IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Unit caster = actions.Caster;
            if (caster == null || caster.IsDisposed || !caster.IsBattleUnit())
            {
                return;
            }

            Unit owner = actions.Owner;
            if (owner == null || owner.IsDisposed)
            {
                return;
            }

            NoticeClientType noticeClientType;
            long castId = 0;

            if (actionsRunType == ActionsRunType.CastHit)
            {
                Cast cast = actions.CastSelf;
                if (cast == null)
                {
                    return;
                }

                // 多目标时只在第一个目标对应的 action 执行时发送一次
                if (cast.Targets.Count > 0)
                {
                    int targetIndex = cast.Targets.IndexOf(owner.Id);
                    if (targetIndex > 0)
                    {
                        return;
                    }
                }

                castId = cast.Id;
                noticeClientType = (NoticeClientType)cast.Config.NoticeClientType;
            }
            else if (actionsRunType == ActionsRunType.BuffTick)
            {
                Buff buff = actions.BuffSelf;
                if (buff == null)
                {
                    return;
                }

                noticeClientType = (NoticeClientType)buff.Config.NoticeClientType;
            }
            else
            {
                return;
            }

            M2C_CastEmptyBullet message = M2C_CastEmptyBullet.Create();
            message.CastId = castId;
            message.CasterId = caster.Id;
            message.ActionId = actions.ConfigId;
            message.TargetId = owner.Id;
            MapMessageHelper.SendClient(caster, message, noticeClientType);
        }
    }
}
