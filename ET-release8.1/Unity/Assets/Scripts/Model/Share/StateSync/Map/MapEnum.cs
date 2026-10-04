namespace ET
{
    public enum MapType
    {
        SafeZone=0,//安全区，所有玩家都是一个阵营A
        Normal=1,//常规区域，玩家阵营A，怪物阵营B
        FreePK=2,//自由PK：玩家/怪物默认可互攻，队伍为盟友；宠物/召唤物跟主人（全地图类型均跟主人）
    }
}

