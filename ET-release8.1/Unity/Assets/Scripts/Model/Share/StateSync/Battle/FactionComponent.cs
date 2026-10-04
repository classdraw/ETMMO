namespace ET
{
    /// <summary>
    /// 每个 Unit 必挂；阵营 Id 由 <see cref="CampHelper.ApplyMapFaction"/> 在创建/切图时写入，客户端由 UnitInfo.CampType 同步。
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class FactionComponent : Entity, IAwake, IAwake<int>
    {
        public int FactionId;
    }
}
