using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(MonsterFlag))]
    [FriendOf(typeof(MonsterFlag))]
    public static partial class MonsterFlagSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.MonsterFlag self,int monsterConfigId,int monsterGroupId)
        {
            self.ConfigId = monsterConfigId;
            self.GroupConfigId = monsterGroupId;
        }

        [EntitySystem]
        private static void Destroy(this ET.Server.MonsterFlag self)
        {
            self.Scene().GetComponent<MonsterMapComponent>().UnitCallDestroy(self.ConfigId,self.GroupConfigId);
            
            self.ConfigId = 0;
            self.GroupConfigId = 0;
        }
        

    }
}