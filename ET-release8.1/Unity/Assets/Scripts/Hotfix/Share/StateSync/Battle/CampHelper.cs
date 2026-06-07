namespace ET
{
    /// <summary>
    /// 阵营/敌友判定。CampType 表示地图关系模式；自由PK下由 OwnerId + TeamId 决定动态盟友。
    /// </summary>
    public static class CampHelper
    {

        public static bool IsAlly(Unit a, Unit b)
        {
            if (a == null || b == null || a.IsDisposed || b.IsDisposed)
            {
                return false;
            }
            //是同一个实体
            if (a.Id == b.Id)
            {
                return true;
            }

            CampType mode = (CampType)a.CampType;
            switch (mode)
            {
                case CampType.CampA:
                case CampType.CampB:
                    return a.CampType == b.CampType;
                case CampType.CampPK:
                    return false;
            }

            return true;
        }

        public static bool IsHostile(Unit a, Unit b)
        {
            return !IsAlly(a, b);
        }

        /// <summary>
        /// 传送进图后按地图类型刷新 CampType / OwnerId；TeamId 沿用 Unit 自身数据。
        /// </summary>
        public static void ApplyMapTransferData(Unit unit, int mapConfigId)
        {
            MapConfig mapConfig = MapConfigCategory.Instance.Get(mapConfigId);
            switch ((MapType)mapConfig.Type)
            {
                case MapType.SafeZone:
                {
                    unit.CampType = (int)CampType.CampA; //安全区都是CampA
                    break;
                }
                case MapType.Normal:
                {
                    if (unit.IsPlayer())
                    {
                        unit.CampType = (int)CampType.CampA;
                    }else if (unit.IsMonster())
                    {
                        unit.CampType = (int)CampType.CampB;
                    }
                    break;
                }
                case MapType.FreePK:
                {
                    if (unit.IsPlayer())
                    {
                        unit.CampType = (int)CampType.CampPK;
                    }else if (unit.IsMonster())
                    {
                        unit.CampType = (int)CampType.CampPK;
                    }
                    break;
                }
            }
        }

    }
}
