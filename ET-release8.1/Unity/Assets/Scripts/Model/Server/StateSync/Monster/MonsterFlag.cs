namespace ET.Server
{
    
    [ComponentOf(typeof(Unit))]
    public class MonsterFlag:Entity,IAwake<int,int>,IDestroy
    {
        public int ConfigId;
        public int GroupConfigId;//怪物群组id
    }
}

