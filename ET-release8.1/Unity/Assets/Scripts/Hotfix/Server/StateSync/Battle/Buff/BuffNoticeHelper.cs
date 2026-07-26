namespace ET.Server
{
    [FriendOf(typeof(BuffComponent))]
    public static class BuffNoticeHelper
    {
        public static async ETTask SendBuffAddToViewer(Unit viewer, Unit owner, Buff buff)
        {
            if (viewer == null || viewer.IsDisposed || owner == null || owner.IsDisposed || buff == null || buff.IsDisposed)
            {
                return;
            }

            NoticeClientType noticeClientType = (NoticeClientType)buff.Config.NoticeClientType;
            if (!MapMessageHelper.ShouldNoticeToViewer(viewer, owner, noticeClientType))
            {
                return;
            }

            M2C_BuffAdd m2CBuffAdd = M2C_BuffAdd.Create();
            m2CBuffAdd.UnitId = owner.Id;
            m2CBuffAdd.BuffData = BuffProtoHelper.Create(buff);
            await MapMessageHelper.SendToClient(viewer, m2CBuffAdd);
        }

        public static void SendBuffAdd(Unit owner, Buff buff)
        {
            if (owner == null || owner.IsDisposed || buff == null || buff.IsDisposed)
            {
                return;
            }

            M2C_BuffAdd m2CBuffAdd = M2C_BuffAdd.Create();
            m2CBuffAdd.BuffData = BuffProtoHelper.Create(buff);
            m2CBuffAdd.UnitId = owner.Id;
            MapMessageHelper.SendClient(owner, m2CBuffAdd, (NoticeClientType)buff.Config.NoticeClientType);
        }

        public static void SendBuffRemove(Unit owner, Buff buff)
        {
            if (owner == null || owner.IsDisposed || buff == null || buff.IsDisposed)
            {
                return;
            }

            M2C_BuffRemove m2CBuffRemove = M2C_BuffRemove.Create();
            m2CBuffRemove.BuffId = buff.Id;
            m2CBuffRemove.UnitId = owner.Id;
            MapMessageHelper.SendClient(owner, m2CBuffRemove, (NoticeClientType)buff.Config.NoticeClientType);
        }

        public static void SendBuffUpdate(Unit owner, Buff buff)
        {
            if (owner == null || owner.IsDisposed || buff == null || buff.IsDisposed)
            {
                return;
            }

            M2C_BuffUpdate m2CBuffUpdate = M2C_BuffUpdate.Create();
            m2CBuffUpdate.UnitId = owner.Id;
            m2CBuffUpdate.BuffData = BuffProtoHelper.Create(buff);
            MapMessageHelper.SendClient(owner, m2CBuffUpdate, (NoticeClientType)buff.Config.NoticeClientType);
        }

        public static void SendBuffTick(Unit owner, Buff buff)
        {
            if (owner == null || owner.IsDisposed || buff == null || buff.IsDisposed)
            {
                return;
            }

            M2C_BuffTick m2CBuffTick = M2C_BuffTick.Create();
            m2CBuffTick.BuffId = buff.Id;
            m2CBuffTick.UnitId = owner.Id;
            MapMessageHelper.SendClient(owner, m2CBuffTick, (NoticeClientType)buff.Config.NoticeClientType);
        }
    }
}
