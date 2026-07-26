using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class MonsterConfigCategory : Singleton<MonsterConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, MonsterConfig> dict = new();
		
        public void Merge(object o)
        {
            MonsterConfigCategory s = o as MonsterConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public MonsterConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MonsterConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (MonsterConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, MonsterConfig> GetAll()
        {
            return this.dict;
        }

        public MonsterConfig GetOne()
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

	public partial class MonsterConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Unit配置id</summary>
		public int UnitConfigId { get; set; }
		/// <summary>组编号</summary>
		public int GroupId { get; set; }
		/// <summary>模型</summary>
		public string Model { get; set; }
		/// <summary>等级</summary>
		public int Level { get; set; }
		/// <summary>元素</summary>
		public int Element { get; set; }
		/// <summary>血量</summary>
		public int Hp { get; set; }
		/// <summary>物理攻击</summary>
		public int[] _Atk;
		
		[BsonIgnore]
		public int[] Atk
		{
			get
			{
				if(_Atk == null)
					_Atk = new int[] { };
				return _Atk;
			}
		}
		/// <summary>物理防御</summary>
		public int[] _Def;
		
		[BsonIgnore]
		public int[] Def
		{
			get
			{
				if(_Def == null)
					_Def = new int[] { };
				return _Def;
			}
		}
		/// <summary>最小魔法攻击</summary>
		public int[] _MAtk;
		
		[BsonIgnore]
		public int[] MAtk
		{
			get
			{
				if(_MAtk == null)
					_MAtk = new int[] { };
				return _MAtk;
			}
		}
		/// <summary>物理防御</summary>
		public int[] _MDef;
		
		[BsonIgnore]
		public int[] MDef
		{
			get
			{
				if(_MDef == null)
					_MDef = new int[] { };
				return _MDef;
			}
		}
		/// <summary>命中</summary>
		public int Hit { get; set; }
		/// <summary>95%miss</summary>
		public int Flee { get; set; }
		/// <summary>攻速</summary>
		public float AtkSpeed { get; set; }
		/// <summary>移速</summary>
		public float Speed { get; set; }
		/// <summary>攻击距离（普攻）</summary>
		public float AtkRange { get; set; }

	}
}
