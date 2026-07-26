using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class BuffConfigCategory : Singleton<BuffConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, BuffConfig> dict = new();
		
        public void Merge(object o)
        {
            BuffConfigCategory s = o as BuffConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public BuffConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BuffConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BuffConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BuffConfig> GetAll()
        {
            return this.dict;
        }

        public BuffConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
    }

	public partial class BuffConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Buff类型</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>Buff覆盖方式</summary>
		public int ConverType { get; set; }
		/// <summary>Buff通知客户端方式</summary>
		public int NoticeClientType { get; set; }
		/// <summary>开始增加层数</summary>
		public int FirstAddLayer { get; set; }
		/// <summary>层数最大限制</summary>
		public int LayerLimit { get; set; }
		/// <summary>总时长(毫秒)</summary>
		public int TotalTime { get; set; }
		/// <summary>效果</summary>
		public int[] _AddAction;
		
		[BsonIgnore]
		public int[] AddAction
		{
			get
			{
				if(_AddAction == null)
					_AddAction = new int[] { };
				return _AddAction;
			}
		}
		/// <summary>Tick间隔时间(毫秒)</summary>
		public int TickTime { get; set; }
		/// <summary>效果</summary>
		public int[] _TickAction;
		
		[BsonIgnore]
		public int[] TickAction
		{
			get
			{
				if(_TickAction == null)
					_TickAction = new int[] { };
				return _TickAction;
			}
		}
		/// <summary>效果</summary>
		public int[] _RemoveAction;
		
		[BsonIgnore]
		public int[] RemoveAction
		{
			get
			{
				if(_RemoveAction == null)
					_RemoveAction = new int[] { };
				return _RemoveAction;
			}
		}
		/// <summary>Buff自身特效</summary>
		public int[] _OwnerEffect;
		
		[BsonIgnore]
		public int[] OwnerEffect
		{
			get
			{
				if(_OwnerEffect == null)
					_OwnerEffect = new int[] { };
				return _OwnerEffect;
			}
		}

	}
}
