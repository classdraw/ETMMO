namespace ET.Server
{
    [ComponentOf(typeof (Scene))]
    public class MapComponent:Entity,IAwake
    {
        public int MapConfigId { get; set; }

        public MapConfig CurrentMapConfig => this.currentMapConfig;
        public MapConfig currentMapConfig;
    }
}

