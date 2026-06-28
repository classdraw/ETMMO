namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class ReliveComponent:Entity,IAwake,IDestroy
    {
        public bool Alive = true;//默认存活
    }
}

