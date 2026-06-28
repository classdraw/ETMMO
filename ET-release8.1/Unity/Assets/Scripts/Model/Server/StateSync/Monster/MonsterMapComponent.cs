namespace ET.Server
{
    [ChildOf(typeof(MonsterMapComponent))]
    public class MonsterCreateInfo:Entity,IAwake<int>,IDestroy
    {
        public int MonsterConfigId;
    }

    [ComponentOf(typeof(Scene))]
    public class MonsterMapComponent:Entity,IAwake,IDestroy
    {
    
    }
}

