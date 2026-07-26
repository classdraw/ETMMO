using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class BulletConfigCategory : Singleton<BulletConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, BulletConfig> dict = new();
		
        public void Merge(object o)
        {
            BulletConfigCategory s = o as BulletConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public BulletConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BulletConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BulletConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BulletConfig> GetAll()
        {
            return this.dict;
        }

        public BulletConfig GetOne()
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

	public partial class BulletConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>形状参数</summary>
		public int[] _ShapeParam;
		
		[BsonIgnore]
		public int[] ShapeParam
		{
			get
			{
				if(_ShapeParam == null)
					_ShapeParam = new int[] { };
				return _ShapeParam;
			}
		}
		/// <summary>持续时间(毫秒)</summary>
		public int TotalTime { get; set; }
		/// <summary>创建时触发</summary>
		public int[] _AwakeActions;
		
		[BsonIgnore]
		public int[] AwakeActions
		{
			get
			{
				if(_AwakeActions == null)
					_AwakeActions = new int[] { };
				return _AwakeActions;
			}
		}
		/// <summary>结算间隔(毫秒)</summary>
		public int Interval { get; set; }
		/// <summary>结算技能编号</summary>
		public int[] _TickCastIds;
		
		[BsonIgnore]
		public int[] TickCastIds
		{
			get
			{
				if(_TickCastIds == null)
					_TickCastIds = new int[] { };
				return _TickCastIds;
			}
		}
		/// <summary>结算行为</summary>
		public int[] _TickActions;
		
		[BsonIgnore]
		public int[] TickActions
		{
			get
			{
				if(_TickActions == null)
					_TickActions = new int[] { };
				return _TickActions;
			}
		}
		/// <summary>销毁前触发</summary>
		public int[] _DestroyActions;
		
		[BsonIgnore]
		public int[] DestroyActions
		{
			get
			{
				if(_DestroyActions == null)
					_DestroyActions = new int[] { };
				return _DestroyActions;
			}
		}
		/// <summary>模型</summary>
		public string Model { get; set; }

	}
}
