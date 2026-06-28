using System;

namespace ET.Server
{
    #region Bullet定时器
    [Invoke(TimerInvokeType.BulletTickTimer)]
    public class BulletTickTimerHandler: ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent self)
        {
            try
            {
                if (self == null || self.IsDisposed)
                {
                    return;
                }
                self.Tick();
            }
            catch (Exception e)
            {
                Log.Error($"Bullet BulletTickTimer error: {self.Id}\n{e}");
            }
        }
    }

    [Invoke(TimerInvokeType.BulletExpireTimer)]
    public class BulletExpireTimerHandler: ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent self)
        {
            try
            {
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                self.Expire();
            }
            catch (Exception e)
            {
                Log.Error($"Bullet BulletExpireTimer error: {self.Id}\n{e}");
            }
        }
    }

    #endregion
    
    [EntitySystemOf(typeof(BulletComponent))]
    [FriendOf(typeof(BulletComponent))]
    public static partial class BulletComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.BulletComponent self,int configId)
        {
            self.ConfigId = configId;
            self.OwnerId = 0;
            self.TickTimer = 0;
            self.AddComponent<ActionsTempComponent>();

        }
        [EntitySystem]
        private static void Destroy(this ET.Server.BulletComponent self)
        {
            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            timerComponent.Remove(ref self.TickTimer);
            timerComponent.Remove(ref self.ExpireTimer);
            self.TickTimer = 0;
            self.ExpireTimer = 0;
            
            self.PreDestroy();
            self.ConfigId = 0;
            self.OwnerId = 0;
        }

        public static Unit GetOwner(this BulletComponent self)
        {
            return self.Scene().GetComponent<UnitComponent>().Get(self.OwnerId);
        }

        public static void Start(this ET.Server.BulletComponent self)
        {
            Unit owner = self.GetOwner();
            if (owner==null||owner.IsDisposed||!owner.IsBattleUnit())
            {
                self.Dispose();
                return;
            }
            Log.Console($"Bullet: {self.ConfigId} Start");
            BulletConfig bulletConfig = self.Config;
            foreach (var actionsId in bulletConfig.AwakeActions)
            {
                self.CreateActions(actionsId, owner, owner, ActionsRunType.BulletAwake);
            }

            if (bulletConfig.Interval>0)
            {
                int interval = bulletConfig.Interval;
                if (interval<=100)
                {
                    interval = 100;//间隔时间最低100
                }
                
                self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(interval, (int)TimerInvokeType.BulletTickTimer, self);
            }

            self.RefreshExpireTimer();
        }

        public static void Expire(this BulletComponent self)
        {
            Unit bulletUnit = self.GetParent<Unit>();
            bulletUnit?.Dispose();
        }

        private static void RefreshExpireTimer(this BulletComponent self)
        {
            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            if (self.ExpireTimer != 0)
            {
                timerComponent.Remove(ref self.ExpireTimer);
            }

            int totalTime = self.Config.TotalTime;
            if (totalTime <= 0)
            {
                self.ExpireTimer = 0;
                return;
            }

            if (totalTime <= 100)
            {
                totalTime = 100;
            }

            long expireTime = TimeInfo.Instance.ServerFrameTime() + totalTime;
            self.ExpireTimer = timerComponent.NewOnceTimer(expireTime, (int)TimerInvokeType.BulletExpireTimer, self);
        }

        /// <summary>
        /// 准备销毁
        /// </summary>
        /// <param name="self"></param>
        private static void PreDestroy(this ET.Server.BulletComponent self)
        {
            Unit owner = self.GetOwner();
            if (owner==null||owner.IsDisposed||!owner.IsBattleUnit())
            {
                return;
            }
            Log.Console($"Bullet: {self.ConfigId} PreDestroy");
            BulletConfig bulletConfig = self.Config;
            if (bulletConfig.DestroyActions.Length==0)
            {
                return;
            }

            foreach (var actionsId in bulletConfig.DestroyActions)
            {
                self.CreateActions(actionsId, owner, owner, ActionsRunType.BulletDestroy);
            }
        }

        public static void Tick(this BulletComponent self)
        {
            Unit selfUnit = self.GetParent<Unit>();
            Unit owner = self.GetOwner();
            if (owner==null||owner.IsDisposed||!owner.IsBattleUnit())
            {
                self.Dispose();
                return;
            }
            
            Log.Console($"Bullet: {self.ConfigId} Tick");
            
            BulletConfig bulletConfig = self.Config;
            if (bulletConfig.TickActions.Length == 0 && bulletConfig.TickCastIds.Length == 0)
            {
                return;
            }

            using (ListComponent<Unit> list = ListComponent<Unit>.Create())
            {
                if (!self.TrySelectTickTargets(selfUnit, owner, bulletConfig, list))
                {
                    return;
                }

                if (list.Count == 0)
                {
                    return;
                }

                foreach (Unit target in list)
                {
                    foreach (int tickCastId in bulletConfig.TickCastIds)
                    {
                        int err = owner.CreateAndCast(tickCastId, target.Id, target.Position);
                        if (err != ErrorCode.ERR_Success)
                        {
                            Log.Warning($"Bullet TickCast failed: bullet={bulletConfig.Id} cast={tickCastId} target={target.Id} err={err}");
                        }
                    }

                    foreach (int actionsId in bulletConfig.TickActions)
                    {
                        self.CreateActions(actionsId, target, owner, ActionsRunType.BulletTick);
                    }
                }
            }
        }

        private static bool TrySelectTickTargets(this BulletComponent self, Unit bulletUnit, Unit owner, BulletConfig bulletConfig,
            ListComponent<Unit> list)
        {
            int[] shapeParam = bulletConfig.ShapeParam;
            if (shapeParam == null || shapeParam.Length < 4)
            {
                Log.Error($"BulletConfig {bulletConfig.Id} ShapeParam invalid");
                return false;
            }

            BulletShape bulletShape = (BulletShape)shapeParam[0];
            switch (bulletShape)
            {
                case BulletShape.Circle:
                {
                    ShapeSelectHelper.SelectCircle(owner, bulletUnit.Position, shapeParam[1], shapeParam[2],
                        (SelectCampType)shapeParam[3], owner.GetAoiUnits(), list);
                    return true;
                }
                default:
                    Log.Error($"BulletConfig {bulletConfig.Id} unsupported shape: {bulletShape}");
                    return false;
            }
        }

    }
}