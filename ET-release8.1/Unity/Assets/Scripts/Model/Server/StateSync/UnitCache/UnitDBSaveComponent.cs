using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class UnitDBSaveComponent: Entity,IAwake,IDestroy
    {
        public long Timer;
        
        //一般发生改变需要保存的东西
        public HashSet<Type> EntityChangeTypeSet { get; } = new HashSet<Type>();
        //传送需要保存的东西
        public HashSet<Type> TransferChanges { get; } = new HashSet<Type>();

        public Dictionary<Type, byte[]> Bytes { get; } = new Dictionary<Type, byte[]>();

        //[BsonIgnore]
        //public HashSet<Type> ComponentTypes { get; } = new HashSet<Type>();
    }
}
