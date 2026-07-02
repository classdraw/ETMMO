using Unity.Mathematics;

namespace ET.Server
{
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

        }

        public static Unit CreateMonster(this MonsterMapComponent self,int id)
        {
            MonsterConfig monsterConfig = MonsterConfigCategory.Instance.Get(id);
            MonsterGroupConfig groupConfig = MonsterGroupConfigCategory.Instance.Get(id);
            float3 pos = new float3(groupConfig.Pos[0] / 1000f, groupConfig.Pos[1] / 1000f, groupConfig.Pos[2] / 1000f);
            pos += new float3(RandomGenerator.RandomNumber(-groupConfig.Range, groupConfig.Range)/1000f, 0f, RandomGenerator.RandomNumber(-groupConfig.Range, groupConfig.Range)/1000f);

            Unit unit = UnitFactory.CreateMonster(self.Scene(), monsterConfig.UnitConfigId, pos);
            unit.AddComponent<MonsterFlag,int,int>(id,monsterConfig.GroupId);
            return unit;
        }

    }
}

