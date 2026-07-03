namespace ET.Server
{
    //刷怪器
    [ChildOf(typeof(MonsterMapComponent))]
    public class MonsterCreateInfo:Entity,IAwake<int>,IDestroy
    {
        public int MonsterConfigId;
    }

    [ComponentOf(typeof(Scene))]
    public class MonsterMapComponent:Entity,IAwake,IDestroy
    {
        public int MapConfigId;
    }
}

