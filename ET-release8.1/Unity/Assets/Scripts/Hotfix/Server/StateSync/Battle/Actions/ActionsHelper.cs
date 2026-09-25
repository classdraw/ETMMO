using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(Buff))]
    [FriendOf(typeof(BulletComponent))]
    public static class ActionsHelper
    {
        public static IActions GetIActions(Scene scene, int actionsType)
        {
            ActionsDispatcherComponent actionsDispatcherComponent = scene.GetComponent<ActionsDispatcherComponent>();
            if (actionsDispatcherComponent == null)
            {
                return null;
            }

            return actionsDispatcherComponent.Get(actionsType);
        }

        public static Actions CreateActions(this ActionsTempComponent self, int configId)
        {
            return self.AddChild<Actions, int>(configId);
        }

        public static Actions CreateActions(this BulletComponent bulletComponent, int configId, Unit target, Unit caster,
            ActionsRunType actionsRunType, bool autoRun = true, bool autoDispose = true)
        {
            Actions actions = bulletComponent.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Caster = caster;
            actions.Owner = target;
            RunActions(bulletComponent.Root(), actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }

            return actions;
        }

        public static Actions CreateActions(this Cast cast, int configId, Unit owner, ActionsRunType actionsRunType, bool castSelfHit = false,
            bool autoRun = true, bool autoDispose = true)
        {
            Actions actions = cast.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Caster = cast.Caster;
            actions.Owner = owner;
            actions.CastSelfHit = castSelfHit;
            RunActions(cast.Root(), actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }

            return actions;
        }

        public static Actions CreateActions(this Buff buff, int configId, ActionsRunType actionsRunType, bool autoRun = true,
            bool autoDispose = true)
        {
            Actions actions = buff.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Owner = buff.Owner;
            if (buff.AddUnitId > 0)
            {
                actions.Caster = buff.Scene().GetComponent<UnitComponent>().Get(buff.AddUnitId);
            }
            
            RunActions(buff.Root(), actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }

            return actions;
        }

        public static void RunActions(Scene scene, Actions actions, ActionsRunType actionsRunType, bool autoRun = true, bool autoDispose = true)
        {
            if (!autoRun)
            {
                return;
            }

            if (autoDispose)
            {
                using (actions)
                {
                    RunActionsInner(scene, actions, actionsRunType);
                }
            }
            else
            {
                RunActionsInner(scene, actions, actionsRunType);
            }
        }

        public static void RunActionsInner(Scene scene, Actions actions, ActionsRunType actionsRunType)
        {
            IActions iActions = GetIActions(scene, actions.Config.Type);
            if (iActions == null)
            {
                Log.Error($"Actions not found: {actions.ConfigId}");
                return;
            }

            iActions.Run(actions, actionsRunType);
        }

        public static void ForEachActionTarget(this Actions actions, ActionsRunType actionsRunType, Action<Unit> handler, bool firstOnly = false)
        {
            using ListComponent<Unit> list = ListComponent<Unit>.Create();
            CollectActionTargets(actions, actionsRunType, list);
            foreach (Unit unit in list)
            {
                handler(unit);
                if (firstOnly)
                {
                    break;
                }
            }
        }

        public static void CollectActionTargets(this Actions actions, ActionsRunType actionsRunType, List<Unit> output)
        {
            output.Clear();
            UnitComponent unitComponent = actions.Scene()?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            switch (actionsRunType)
            {
                case ActionsRunType.CastHit:
                {
                    if (TryCollectHitFlyTargets(actions, actionsRunType, output))
                    {
                        break;
                    }

                    Cast cast = actions.CastSelf;
                    if (cast == null)
                    {
                        break;
                    }

                    if (actions.CastSelfHit)
                    {
                        TryAddBattleUnit(output, actions.Caster);
                    }
                    else
                    {
                        foreach (long targetId in cast.Targets)
                        {
                            TryAddBattleUnit(output, unitComponent.Get(targetId));
                        }
                    }

                    break;
                }
                case ActionsRunType.BuffAdd:
                case ActionsRunType.BuffRemove:
                case ActionsRunType.BuffTick:
                {
                    if (TryCollectHitFlyTargets(actions, actionsRunType, output))
                    {
                        break;
                    }

                    TryAddBattleUnit(output, actions.Owner);
                    break;
                }
                case ActionsRunType.BulletAwake:
                case ActionsRunType.BulletDestroy:
                case ActionsRunType.BulletTick:
                {
                    BulletComponent bulletComponent = actions.BulletSelf;
                    if (bulletComponent == null)
                    {
                        break;
                    }

                    foreach (long targetId in bulletComponent.Targets)
                    {
                        TryAddBattleUnit(output, unitComponent.Get(targetId));
                    }

                    break;
                }
            }
        }

        private static void TryAddBattleUnit(List<Unit> output, Unit unit)
        {
            if (unit == null || unit.IsDisposed || !unit.IsBattleUnit())
            {
                return;
            }

            output.Add(unit);
        }

        private static bool TryCollectHitFlyTargets(Actions actions, ActionsRunType actionsRunType, List<Unit> output)
        {
            if (actions.Config.Type != ActionsType.HitFlyTarget)
            {
                return false;
            }

            if (actionsRunType != ActionsRunType.CastHit && actionsRunType != ActionsRunType.BuffTick)
            {
                return false;
            }

            Unit center = actionsRunType == ActionsRunType.CastHit ? actions.Caster : actions.Owner;
            if (center == null || center.IsDisposed)
            {
                return true;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 1)
            {
                return true;
            }

            float range = config.ActionsParam[0] / 1000f;
            float rangeSq = range * range;
            CollectBeSeeUnitsInRange(center, rangeSq, output);
            return true;
        }

        private static void CollectBeSeeUnitsInRange(Unit center, float rangeSq, List<Unit> output)
        {
            Dictionary<long, EntityRef<AOIEntity>> beSeeUnits = center.GetBeSeeUnits();
            if (beSeeUnits == null || beSeeUnits.Count == 0)
            {
                return;
            }

            float3 casterPos = center.Position;
            foreach (AOIEntity aoiEntity in beSeeUnits.Values)
            {
                if (aoiEntity == null || aoiEntity.IsDisposed)
                {
                    continue;
                }

                Unit unit = aoiEntity.GetParent<Unit>();
                if (unit == null || unit.IsDisposed || unit.Id == center.Id)
                {
                    continue;
                }

                if (math.lengthsq(unit.Position - casterPos) > rangeSq)
                {
                    continue;
                }

                TryAddBattleUnit(output, unit);
            }
        }
    }
}
