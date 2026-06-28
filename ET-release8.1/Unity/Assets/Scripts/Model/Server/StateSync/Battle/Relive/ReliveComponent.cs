namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class ReliveComponent:Entity,IAwake,IDestroy
    {
        /// <summary>
        /// 是否存活
        /// </summary>
        public bool Alive = true;//默认存活
    }
}

