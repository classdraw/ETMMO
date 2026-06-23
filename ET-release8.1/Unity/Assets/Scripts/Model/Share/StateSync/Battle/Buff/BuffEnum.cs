namespace ET
{
    /// <summary>
    /// buff覆盖叠加式
    /// </summary>
    public enum BuffCoverType:byte
    {
        AddTime=1,//增加时长：同ConfigId+同创建者叠加
        New=2,//新增独立
        Replace=3,//替换：同ConfigId+同创建者比较层数
        ResetTime=4,//重置持续时长：同ConfigId+同创建者
        SelfMutex=5,//自身互斥：同ConfigId+同创建者互斥
        Role=6,//角色互斥：同ConfigId+同创建者唯一，不同创建者可并存
        ClassifyMutex=7,//类型互斥：同Type已存在则拒绝添加
    }
    
    /// <summary>
    /// Buff类型码
    /// </summary>
    public enum BuffClassifyType:byte
    {
        /// <summary>
        /// 死亡删除
        /// </summary>
        Dead = 1,
        
        /// <summary>
        /// 使用技能后删除
        /// </summary>
        UseSkill = 2,
        
        /// <summary>
        /// 进入战斗时删除
        /// </summary>
        Fight = 4,
        
        /// <summary>
        /// 过图删除
        /// </summary>
        ChangeMap = 5,
        
        /// <summary>
        /// 复活后删除
        /// </summary>
        Relive = 7
    }
    /// <summary>
    /// Buff效果
    /// </summary>
    public enum BuffEffectType:byte
    {
        
    }
}

