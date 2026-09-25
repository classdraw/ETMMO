namespace ET.Server
{
    [Actions(ActionsType.CastEmptyBullet)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(Buff))]
    public class Actions_CastEmptyBullet : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Unit caster = actions.Caster;
            if (caster == null || caster.IsDisposed || !caster.IsBattleUnit())
            {
                return;
            }

            NoticeClientType noticeClientType;
            long castId = 0;
            long targetId = 0;

            if (actionsRunType == ActionsRunType.CastHit)
            {
                Cast cast = actions.CastSelf;
                if (cast == null)
                {
                    return;
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

            bool hasTarget = false;
            actions.ForEachActionTarget(actionsRunType, target =>
            {
                hasTarget = true;
                targetId = target.Id;
            }, firstOnly: true);

            if (!hasTarget)
            {
                return;
            }

            SendCastEmptyBullet(caster, castId, actions.ConfigId, targetId, noticeClientType);
        }

        private static void SendCastEmptyBullet(Unit caster, long castId, int actionId, long targetId, NoticeClientType noticeClientType)
        {
            M2C_CastEmptyBullet message = M2C_CastEmptyBullet.Create();
            message.CastId = castId;
            message.CasterId = caster.Id;
            message.ActionId = actionId;
            message.TargetId = targetId;
            MapMessageHelper.SendClient(caster, message, noticeClientType);
        }
    }
}
