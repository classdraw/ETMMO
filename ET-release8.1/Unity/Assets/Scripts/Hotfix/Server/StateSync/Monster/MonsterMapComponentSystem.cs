using System;
using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(MonsterCreateInfo))]
    [Invoke(TimerInvokeType.CreateMonsterTimer)]
    public class CreateMonsterTimerHandler: ATimer<MonsterCreateInfo>
    {
        protected override void Run(MonsterCreateInfo self)
        {
            try
            {
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                self.GetParent<MonsterMapComponent>().CreateMonster(self.MonsterConfigId);
                self.Dispose();
            }
            catch (Exception e)
            {
                Log.Error($"MonsterMapComponent CreateMonsterTimer error: {self.Id}\n{e}");
            }
        }
    }
    
    

    [Invoke(TimerInvokeType.DestroyMonsterTimer)]
    public class DestroyMonsterTimerHandler: ATimer<Unit>
    {
        protected override void Run(Unit self)
        {
            try
            {
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                self.Dispose();
            }
            catch (Exception e)
            {
                Log.Error($"Unit DestroyMonsterTimerHandler error: {self.Id}\n{e}");
            }
        }
    }
    
    [EntitySystemOf(typeof(MonsterMapComponent))]
    [FriendOf(typeof(MonsterMapComponent))]
    public static partial class MonsterMapComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.MonsterMapComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ET.Server.MonsterMapComponent self)
        {
            self.MapConfigId = 0;
        }

        public static void InitByMapConfig(this MonsterMapComponent self, int mapConfigId)
        {
            self.MapConfigId = mapConfigId;
            MapConfig mapConfig = MapConfigCategory.Instance.Get(mapConfigId);
            Log.Console($"[MonsterMap] 初始化刷怪器，地图={mapConfigId}，名称={mapConfig.Title}");

            int spawnCount = 0;
            foreach (MonsterConfig monsterConfig in MonsterConfigCategory.Instance.GetAll().Values)
            {
                if (!MonsterGroupConfigCategory.Instance.Contain(monsterConfig.GroupId))
                {
                    Log.Error($"MonsterConfig {monsterConfig.Id} GroupId {monsterConfig.GroupId} not found");
                    continue;
                }

                MonsterGroupConfig groupConfig = MonsterGroupConfigCategory.Instance.Get(monsterConfig.GroupId);
                if (groupConfig.mapId != mapConfigId)
                {
                    continue;
                }

                if (self.CreateMonster(monsterConfig.Id) != null)
                {
                    spawnCount++;
                }
            }

            Log.Console($"[MonsterMap] 地图={mapConfigId} 刷怪完成，数量={spawnCount}");
        }


        public static void UnitCallDestroy(this MonsterMapComponent self,int monsterConfigId,int groupId)
        {
            if (self == null || self.IsDisposed || self.IScene == null || self.IScene.Fiber == null)
            {
                return;
            }

            Scene root = self.Root();
            if (root == null || root.IsDisposed)
            {
                return;
            }

            TimerComponent timerComponent = root.GetComponent<TimerComponent>();
            if (timerComponent == null || timerComponent.IsDisposed)
            {
                return;
            }

            long now = TimeInfo.Instance.ServerFrameTime();
            timerComponent.NewOnceTimer(now+3000, (int)TimerInvokeType.CreateMonsterTimer, self.AddChild<MonsterCreateInfo,int>(monsterConfigId));
        }

        public static Unit CreateMonster(this MonsterMapComponent self,int monsterConfigId)
        {
            MonsterConfig monsterConfig = MonsterConfigCategory.Instance.Get(monsterConfigId);
            MonsterGroupConfig groupConfig = MonsterGroupConfigCategory.Instance.Get(monsterConfig.GroupId);
            if (groupConfig.Pos == null || groupConfig.Pos.Length < 3)
            {
                Log.Error($"MonsterGroupConfig {monsterConfig.GroupId} Pos invalid");
                return null;
            }

            float3 pos = new float3(groupConfig.Pos[0] / 1000f, groupConfig.Pos[1] / 1000f, groupConfig.Pos[2] / 1000f);
            pos += new float3(RandomGenerator.RandomNumber(-groupConfig.Range, groupConfig.Range)/1000f, 0f, RandomGenerator.RandomNumber(-groupConfig.Range, groupConfig.Range)/1000f);

            Unit unit = UnitFactory.CreateMonster(self.Scene(), monsterConfig.UnitConfigId, pos);
            unit.AddComponent<MonsterFlag,int,int>(monsterConfigId,monsterConfig.GroupId);
            return unit;
        }

    }
}

