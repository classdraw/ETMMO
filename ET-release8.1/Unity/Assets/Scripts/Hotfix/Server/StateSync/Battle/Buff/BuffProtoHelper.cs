namespace ET.Server
{
    [FriendOf(typeof(Buff))]
    public static class BuffProtoHelper
    {
        public static BuffProto Create(Buff buff)
        {
            BuffProto buffProto = BuffProto.Create(true);
            buffProto.Id = buff.Id;
            buffProto.ConfigId = buff.ConfigId;
            buffProto.ExpireTime = buff.ExpireTime;
            buffProto.CreateTime = buff.CreateTime;
            buffProto.ExtraData = CreateExtraDataBytes(buff);
            return buffProto;
        }

        private static byte[] CreateExtraDataBytes(Buff buff)
        {
            BuffExtraData extraData = new BuffExtraData
            {
                AddUnitId = buff.AddUnitId,
                AddSkillId = buff.AddSkillId,
                TickTime = buff.TickTime,
                TickBeginTime = buff.TickBeginTime,
                Layer = buff.Layer,
            };
            return MongoHelper.Serialize(extraData);
        }
    }
}
