using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(SkillStatusComponent))]
    public static class CoolDownNoticeHelper
    {
        public static void SendCoolDownChange(Unit owner, int castConfigId, long coolDownEndTime, long coolDownStartTime)
        {
            if (owner == null || owner.IsDisposed)
            {
                return;
            }

            M2C_CoolDownChange message = M2C_CoolDownChange.Create();
            message.CastConfigIds.Add(castConfigId);
            message.CoolDownTimes.Add(coolDownEndTime);
            message.CoolDownStartTimes.Add(coolDownStartTime);
            MapMessageHelper.SendClient(owner, message, NoticeClientType.Self);
        }

        public static async ETTask SyncAllCoolDowns(Unit owner)
        {
            SkillStatusComponent skillStatusComponent = owner?.GetComponent<SkillStatusComponent>();
            if (owner == null || owner.IsDisposed || skillStatusComponent == null || skillStatusComponent.IsDisposed)
            {
                return;
            }

            long now = TimeInfo.Instance.ServerFrameTime();
            M2C_CoolDownChange message = M2C_CoolDownChange.Create();

            foreach (KeyValuePair<int, long> kv in skillStatusComponent.CoolDowns)
            {
                int castConfigId = kv.Key;
                long coolDownEndTime = kv.Value;
                if (now > coolDownEndTime)
                {
                    continue;
                }

                skillStatusComponent.CoolDownStartTimes.TryGetValue(castConfigId, out long coolDownStartTime);
                message.CastConfigIds.Add(castConfigId);
                message.CoolDownTimes.Add(coolDownEndTime);
                message.CoolDownStartTimes.Add(coolDownStartTime);
            }

            if (message.CastConfigIds.Count == 0)
            {
                message.Dispose();
                return;
            }

            await MapMessageHelper.SendToClient(owner, message);
        }
    }
}
