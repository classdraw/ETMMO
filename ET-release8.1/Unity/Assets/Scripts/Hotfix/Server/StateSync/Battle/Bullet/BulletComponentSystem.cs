using System;
using Unity.Mathematics;

namespace ET.Server
{
    #region Bullet定时器

    [Invoke(TimerInvokeType.BulletTickTimer)]
    public class BulletTickTimerHandler : ATimer<BulletComponent>
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

    [Invoke(TimerInvokeType.BulletTickTimer2)]
    public class BulletTickTimer2Handler : ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent self)
        {
            try
            {
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                self.Tick1();
            }
            catch (Exception e)
            {
                Log.Error($"Bullet BulletTickTimer2 error: {self.Id}\n{e}");
            }
        }
    }

    [Invoke(TimerInvokeType.BulletTickTimer3)]
    public class BulletTickTimer3Handler : ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent self)
        {
            try
            {
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                self.Tick2();
            }
            catch (Exception e)
            {
                Log.Error($"Bullet BulletTickTimer3 error: {self.Id}\n{e}");
            }
        }
    }

    [Invoke(TimerInvokeType.BulletExpireTimer)]
    public class BulletExpireTimerHandler : ATimer<BulletComponent>
    {
        protected override void Run(BulletComponent self)
        {
            try
            {
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                self.DisposeSelf();
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
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    public static partial class BulletComponentSystem
    {
        #region 生命周期

        [EntitySystem]
        private static void Awake(this BulletComponent self, int configId)
        {
            self.ConfigId = configId;
            self.OwnerId = 0;
            self.TickTimer = 0;
            self.TickTimer2 = 0;
            self.TickTimer3 = 0;
            self.ExpireTimer = 0;
            self.TickCount = 0;
            self.InputUnitId = 0;
            self.InputPos = default;
            self.Targets.Clear();
            self.AddComponent<ActionsTempComponent>();
        }

        [EntitySystem]
        private static void Destroy(this BulletComponent self)
        {
            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            timerComponent.Remove(ref self.TickTimer);
            timerComponent.Remove(ref self.TickTimer2);
            timerComponent.Remove(ref self.TickTimer3);
            timerComponent.Remove(ref self.ExpireTimer);

            self.TickTimer = 0;
            self.TickTimer2 = 0;
            self.TickTimer3 = 0;
            self.ExpireTimer = 0;

            self.ConfigId = 0;
            self.OwnerId = 0;
            self.TickCount = 0;
            self.InputUnitId = 0;
            self.InputPos = default;
            self.Targets.Clear();
        }

        public static void Start(this BulletComponent self)
        {
            Unit owner = self.GetOwner();
            if (!self.IsOwnerValid(owner))
            {
                self.DisposeSelf();
                return;
            }

            Log.Console($"Bullet: {self.ConfigId} Start");
            BulletConfig bulletConfig = self.Config;

            foreach (int actionsId in bulletConfig.AwakeActions)
            {
                self.CreateActions(actionsId, owner, owner, ActionsRunType.BulletAwake);
            }

            if (bulletConfig.Interval > 0)
            {
                int interval = bulletConfig.Interval;
                if (interval <= 100)
                {
                    interval = 100;
                }

                self.TickTimer = self.Root().GetComponent<TimerComponent>()
                    .NewRepeatedTimer(interval, TimerInvokeType.BulletTickTimer, self);
            }

            if (bulletConfig.Tick1.Length > 0)
            {
                self.TickTimer2 = self.Root().GetComponent<TimerComponent>()
                    .NewRepeatedTimer(100, TimerInvokeType.BulletTickTimer2, self);
            }

            if (bulletConfig.Tick2.Length > 0)
            {
                self.TickTimer3 = self.Root().GetComponent<TimerComponent>()
                    .NewRepeatedTimer(1000, TimerInvokeType.BulletTickTimer3, self);
            }

            self.RefreshExpireTimer();
        }

        public static void DisposeSelf(this BulletComponent self)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }

            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            timerComponent.Remove(ref self.TickTimer);
            timerComponent.Remove(ref self.TickTimer2);
            timerComponent.Remove(ref self.TickTimer3);
            timerComponent.Remove(ref self.ExpireTimer);

            Unit owner = self.GetOwner();
            if (owner == null || owner.IsDisposed)
            {
                self.DoDispose();
                return;
            }

            BulletConfig bulletConfig = self.Config;
            if (bulletConfig.DestroyActions.Length > 0)
            {
                foreach (int actionsId in bulletConfig.DestroyActions)
                {
                    self.CreateActions(actionsId, owner, owner, ActionsRunType.BulletDestroy);
                }
            }

            self.DoDispose();
        }

        /// <summary>
        /// 销毁子弹 Unit：通知客户端移除 → Dispose（AOI 与子组件 Destroy 随 Unit 销毁）。
        /// </summary>
        public static void DoDispose(this BulletComponent self)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }

            Unit bulletUnit = self.GetParent<Unit>();
            if (bulletUnit == null || bulletUnit.IsDisposed)
            {
                return;
            }            
            if (bulletUnit.GetComponent<MoveComponent>() != null)
            {
                bulletUnit.Stop(0);
            }
            
            MapMessageHelper.NoticeUnitRemoveBroadcast(bulletUnit);
            bulletUnit.Dispose();
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
            self.ExpireTimer = timerComponent.NewOnceTimer(expireTime, TimerInvokeType.BulletExpireTimer, self);
        }

        public static Unit GetOwner(this BulletComponent self)
        {
            return self.Scene().GetComponent<UnitComponent>().Get(self.OwnerId);
        }

        private static bool IsOwnerValid(this BulletComponent self, Unit owner)
        {
            return owner != null && !owner.IsDisposed && owner.IsBattleUnit();
        }

        #endregion

        #region 结算 Tick

        /// <summary>
        /// Interval 档结算；仅本方法使用 TickCount / TickLimit。
        /// TickLimit 到次数只停本档定时器，子弹仍按 TotalTime 销毁。
        /// </summary>
        public static void Tick(this BulletComponent self)
        {
            Unit bulletUnit = self.GetParent<Unit>();
            Unit owner = self.GetOwner();
            if (!self.IsOwnerValid(owner))
            {
                self.DisposeSelf();
                return;
            }

            BulletConfig bulletConfig = self.Config;
            if (bulletConfig.TickActions.Length == 0 && bulletConfig.TickCastIds.Length == 0)
            {
                return;
            }

            if (bulletConfig.TickLimit > 0 && self.TickCount >= bulletConfig.TickLimit)
            {
                return;
            }

            self.TrySelectTickTargets(bulletUnit, bulletConfig);
            self.TickCount++;

            if (bulletConfig.TickCastIds.Length > 0)
            {
                foreach (int tickCastId in bulletConfig.TickCastIds)
                {
                    int err = owner.CreateAndCast(tickCastId, 0, bulletUnit.Position, false, self.Targets);
                    if (err != ErrorCode.ERR_Success)
                    {
                        Log.Console($"子弹 {self.ConfigId} 释放Cast {tickCastId} 失败: {err}");
                    }
                }
            }

            if (bulletConfig.TickActions.Length > 0)
            {
                foreach (int actionsId in bulletConfig.TickActions)
                {
                    self.CreateActions(actionsId, owner, owner, ActionsRunType.BulletTick);
                }
            }

            if (bulletConfig.TickLimit > 0 && self.TickCount >= bulletConfig.TickLimit)
            {
                // 结算次数到了，提前结束
                self.DisposeSelf();
            }
        }

        public static void Tick1(this BulletComponent self)
        {
            if (!self.IsOwnerValid(self.GetOwner()))
            {
                self.DisposeSelf();
                return;
            }

            BulletConfig bulletConfig = self.Config;
            if (bulletConfig.Tick1.Length == 0)
            {
                return;
            }

            self.SelectTarget();
            Unit bulletUnit = self.GetParent<Unit>();
            foreach (int actionsId in bulletConfig.Tick1)
            {
                self.CreateActions(actionsId, bulletUnit, bulletUnit, ActionsRunType.BulletTick);
            }
        }

        public static void Tick2(this BulletComponent self)
        {
            if (!self.IsOwnerValid(self.GetOwner()))
            {
                self.DisposeSelf();
                return;
            }

            BulletConfig bulletConfig = self.Config;
            if (bulletConfig.Tick2.Length == 0)
            {
                return;
            }

            self.SelectTarget();
            Unit bulletUnit = self.GetParent<Unit>();
            foreach (int actionsId in bulletConfig.Tick2)
            {
                self.CreateActions(actionsId, bulletUnit, bulletUnit, ActionsRunType.BulletTick);
            }
        }

        #endregion

        #region 选目标

        private static void SelectTarget(this BulletComponent self)
        {
            Unit bulletUnit = self.GetParent<Unit>();
            self.TrySelectTickTargets(bulletUnit, self.Config);
        }

        /// <summary>
        /// 按 ShapeParam 选目标（逻辑同 CastSystem.SelectTargetsInner，以子弹 Unit 为主体）；每次清空并重建 Targets，排除 OwnerId。
        /// </summary>
        private static bool TrySelectTickTargets(this BulletComponent self, Unit bulletUnit, BulletConfig bulletConfig)
        {
            self.Targets.Clear();

            int[] shapeParam = bulletConfig.ShapeParam;
            if (shapeParam == null || shapeParam.Length == 0)
            {
                return false;
            }

            ShapeType shapeType = (ShapeType)shapeParam[0];
            if (shapeType == ShapeType.Single)
            {
                Log.Error($"BulletConfig {bulletConfig.Id} unsupported shape: {shapeType}");
                return false;
            }

            float3 pos = bulletUnit.Position;
            long ownerId = self.OwnerId;

            using (ListComponent<Unit> list = ListComponent<Unit>.Create())
            {
                switch (shapeType)
                {
                    case ShapeType.Circle:
                        {
                            if (shapeParam.Length < 4)
                            {
                                Log.Error($"BulletConfig {bulletConfig.Id} Circle ShapeParam invalid");
                                return false;
                            }

                            int needCount = self.GetSelectNeedCount(bulletConfig, shapeParam[2]);
                            ShapeSelectHelper.SelectCircle(bulletUnit, pos, shapeParam[1], needCount, (SelectCampType)shapeParam[3],
                                bulletUnit.GetAoiUnits(), list);
                            break;
                        }
                    case ShapeType.Rectangle:
                        {
                            if (shapeParam.Length < 6)
                            {
                                Log.Error($"BulletConfig {bulletConfig.Id} Rectangle ShapeParam invalid");
                                return false;
                            }

                            int needCount = self.GetSelectNeedCount(bulletConfig, shapeParam[4]);
                            ShapeSelectHelper.SelectRectangle(bulletUnit, pos, shapeParam[1], shapeParam[2], shapeParam[3], needCount,
                                (SelectCampType)shapeParam[5], bulletUnit.GetAoiUnits(), list);
                            break;
                        }
                    case ShapeType.Fan:
                        {
                            if (shapeParam.Length < 5)
                            {
                                Log.Error($"BulletConfig {bulletConfig.Id} Fan ShapeParam invalid");
                                return false;
                            }

                            int needCount = self.GetSelectNeedCount(bulletConfig, shapeParam[3]);
                            ShapeSelectHelper.SelectFan(bulletUnit, pos, shapeParam[1], shapeParam[2], needCount, (SelectCampType)shapeParam[4],
                                bulletUnit.GetAoiUnits(), list);
                            break;
                        }
                    default:
                        Log.Error($"BulletConfig {bulletConfig.Id} unsupported shape: {shapeType}");
                        return false;
                }

                foreach (Unit targetUnit in list)
                {
                    if (targetUnit.Id == ownerId)
                    {
                        continue;
                    }

                    if (bulletConfig.TargetNumber > 0 && self.Targets.Count >= bulletConfig.TargetNumber)
                    {
                        break;
                    }

                    self.Targets.Add(targetUnit.Id);
                }
            }

            return true;
        }

        /// <summary>
        /// TargetNumber：-1 不限制（仅 ShapeParam 人数）；>0 与 Shape 人数取更严；0 仅 ShapeParam。
        /// </summary>
        private static int GetSelectNeedCount(this BulletComponent self, BulletConfig bulletConfig, int shapeNeedCount)
        {
            if (bulletConfig.TargetNumber == -1)
            {
                return shapeNeedCount <= 0 ? int.MaxValue : shapeNeedCount;
            }

            if (bulletConfig.TargetNumber > 0)
            {
                if (shapeNeedCount <= 0)
                {
                    return bulletConfig.TargetNumber;
                }

                return Math.Min(shapeNeedCount, bulletConfig.TargetNumber);
            }

            return shapeNeedCount <= 0 ? int.MaxValue : shapeNeedCount;
        }

        #endregion
    }
}
