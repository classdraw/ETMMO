namespace ET
{
    #region 技能
    //技能开始
    public struct CastStart
    {
        public long CastId;
        public long CasterId;
        public int CasterConfigId;
        
    }
    
    //技能命中
    public struct CastHit
    {
        public long CastId;
        public long CasterId;
        public long TargetId;
        public int HitIndex;
        public bool IsSelf;
    }
    
    //技能结束
    public struct CastFinish
    {
        public long CastId;
        public long CasterId;
    }
    //技能服务器判定释放失败
    public struct CastError
    {
        public long CasterId;
    }

    //技能打断
    public struct CastBreak
    {
        public long CastId;
        public long CasterId;
    }

    //战斗结果飘字
    public struct BattleResult
    {
        public long AttackerId;
        public long TargetId;
        public long Damage;
        public bool IsCrit;
    }

    //假子弹
    public struct CastEmptyBullet
    {
        public Unit BulletUnit;
        public Unit Caster;
        public Unit Target;
        public int EffectConfigId;
        public int FlyTimeMs;
    }


    public struct BuffAdd
    {
        public Unit Unit;
        public long BuffId;
        public int BuffConfigId;
    }

    public struct BuffRemove
    {
        public Unit Unit;
        public long BuffId;
    }

    public struct BuffTick
    {
        public Unit Unit;
        public long BuffId;
    }

    public struct BuffUpdate
    {
        public Unit Unit;
        public long BuffId;
        public int BuffConfigId;
    }
    #endregion
}

