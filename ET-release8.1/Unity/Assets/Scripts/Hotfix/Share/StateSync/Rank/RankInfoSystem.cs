namespace ET
{
    [EntitySystemOf(typeof(RankInfo))]
    [FriendOfAttribute(typeof(ET.RankInfo))]
    public static partial class RankInfoSystem
    {
        public static void FromMessage(this RankInfo self, RankInfoProto rankInfoProto)
        {
            self.UnitId = rankInfoProto.UnitId;
            self.Name = rankInfoProto.Name;
            self.RankValue = rankInfoProto.RankValue;
        }

        public static RankInfoProto ToMessage(this RankInfo self)
        {
            RankInfoProto rankInfoProto = RankInfoProto.Create();
            rankInfoProto.Id = self.Id;
            rankInfoProto.UnitId = self.UnitId;
            rankInfoProto.Name = self.Name;
            rankInfoProto.RankValue = self.RankValue;
            return rankInfoProto;
        }
        [EntitySystem]
        private static void Awake(this ET.RankInfo self)
        {

        }
        [EntitySystem]
        private static void Destroy(this ET.RankInfo self)
        {
            self.UnitId = default;
            self.Name   = default;
            self.RankValue  = default;
        }
    }
}