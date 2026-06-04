namespace ET.Server
{
    [EntitySystemOf(typeof(Cast))]
    [FriendOf(typeof(Cast))]
    public static partial class CastSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.Cast self,int configId)
        {
            self.ConfigId = configId;
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.Cast self)
        {
            self.ConfigId = 0;
            self.Caster = null;
            self.Targets.Clear();
            self.StartTime = 0;
        }

        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="cast"></param>
        /// <returns></returns>
        public static int Cast(this Cast cast)
        {
            int err = cast.CastCheck();//释放条件判断
            if (err!=ErrorCode.ERR_Success)
            {
                return err;
            }
            cast.SelectTargets();//选择目标
            err = cast.CastCheckBeforeBegin();
            if (err!=ErrorCode.ERR_Success)
            {
                return err;
            }
            
            cast.CastBeginAsync().Coroutine();//释放
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 释放技能前置判断
        /// </summary>
        /// <param name="cast"></param>
        /// <returns></returns>
        public static int CastCheck(this Cast cast)
        {
            return ErrorCode.ERR_Success;
        }
        /// <summary>
        /// 选择目标
        /// </summary>
        /// <param name="cast"></param>
        public static void SelectTargets(this Cast cast)
        {
            
        }
        /// <summary>
        /// 释放前检测
        /// </summary>
        /// <param name="cast"></param>
        /// <returns></returns>
        public static int CastCheckBeforeBegin(this Cast cast)
        {
            return ErrorCode.ERR_Success;
        }
        
        /// <summary>
        /// 技能释放
        /// </summary>
        /// <param name="cast"></param>
        public static async ETTask CastBeginAsync(this Cast cast)
        {
            await ETTask.CompletedTask;
        }
    }
}
