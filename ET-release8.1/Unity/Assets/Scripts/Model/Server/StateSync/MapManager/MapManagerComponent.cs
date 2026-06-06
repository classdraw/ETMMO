using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class MapManagerComponent:Entity,IAwake,IDestroy
    {
        public long Timer;
        /// <summary>
        /// 地图Id对应分线列表
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, List<long>> mapCfgDict = new Dictionary<int, List<long>>();

        /// <summary>
        /// 角色ID对应所在地图ID
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<long, long> roleMapDict = new Dictionary<long, long>();
    }
}

