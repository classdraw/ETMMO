using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ComponentOf]
    public class UnitDBSaveComponent: Entity,IAwake,IDestroy
    {
        public long Timer;

        public HashSet<Type> EntityChangeTypeSet = new HashSet<Type>();
        
        public HashSet<Type> TransferChanges { get; } = new HashSet<Type>();

        public Dictionary<Type, byte[]> Bytes { get; } = new Dictionary<Type, byte[]>();

        [BsonIgnore]
        public HashSet<Type> ComponentTypes { get; } = new HashSet<Type>();
    }
}
