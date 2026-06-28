using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(ReliveComponent))]
    public static class ReliveHelper
    {
        /// <summary>
        /// 是否存活
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool IsAlive(this Unit unit)
        {
            return unit.GetComponent<ReliveComponent>()?.Alive??true;
        }

        public static int OnSiteRelive(this Unit unit)
        {
            if (unit.IsAlive())
            {
                return ErrorCode.ERR_Relive_Alive;
            }
            //复活
            unit.DoRelive(unit.Position,1.0f);//原地复活 后面再传送
            return ErrorCode.ERR_Success;
        }

        public static int PointRelive(this Unit unit,float pos)
        {
            if (unit.IsAlive())
            {
                return ErrorCode.ERR_Relive_Alive;
            }
            unit.DoRelive(pos,1.0f);
            return ErrorCode.ERR_Success;
        }

        public static void DoRelive(this Unit unit,float3 pos,float hpRate)
        {
            if (unit.IsAlive())
            {
                return;
            }

            unit.Position = pos;
            NumericComponent numericComponent=unit.GetComponent<NumericComponent>();
            if (numericComponent!=null)
            {
                var maxHp = numericComponent[NumericType.MaxHp];
                var needHp = maxHp * hpRate;
                numericComponent[NumericType.HpBase] = math.clamp((int)needHp, 1, maxHp);
            }
            ReliveComponent reliveComponent=unit.GetComponent<ReliveComponent>();
            if (reliveComponent!=null)
            {
                reliveComponent.Alive = true;
            }

            
        }
    }
}

