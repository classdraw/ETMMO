using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    public class MapUnit: Entity, IAwake<int>, IDestroy, ISerializeToWeb
    {
        /// <summary>
        /// 地图配置ID
        /// </summary>
        [BsonIgnore]
        public int MapConfigId => this.mapConfigId;
        [BsonIgnore]
        public MapConfig MapConfig => MapConfigCategory.Instance.Get(this.MapConfigId);
        
        public int mapConfigId;
        
        public int fiberId;
        public ActorId actorId;
        public string actorStr;
        public CreateMapCtx ctx;//创建地图上下文

        /// <summary>
        /// 当前地图人数
        /// </summary>
        public int count;//地图人数

        public long closeTime;//关闭时间
        public long validTime;//有效时间
    }
}

