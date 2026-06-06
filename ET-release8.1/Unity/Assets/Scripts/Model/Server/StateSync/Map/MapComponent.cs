namespace ET.Server
{
    [ComponentOf(typeof (Scene))]
    public class MapComponent:Entity,IAwake,IDestroy
    {
        public int MapConfigId { get; set; }

        public CreateMapCtx Ctx => ctx;

        public CreateMapCtx ctx;
    }
}

