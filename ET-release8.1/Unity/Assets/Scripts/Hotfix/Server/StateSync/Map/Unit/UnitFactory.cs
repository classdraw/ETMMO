using System;
using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(BulletComponent))]
    public static partial class UnitFactory
    {
        public static UnitConfig GetUnitConfig(int configId)
        {
            return UnitConfigCategory.Instance.Get(configId);
        }

        public static Unit  Create(Scene scene, long id,int configId,string name, UnitType unitType)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            switch (unitType)
            {
                case UnitType.Player:
                {
                    UnitConfig unitConfig=UnitConfigCategory.Instance.Get(configId);

                    Unit unit = unitComponent.AddChildWithId<Unit, int,string>(id, configId,name);
                    
                    unit.AddComponent<MoveComponent>();
                    unit.Position = new float3(-8.7f, 0f, -15.5f);//最好给新手村第一个场景的坐标 或者新手安全区随机一个 可以写死
                    NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
                    
                    numericComponent.Set(NumericType.AOI, unitConfig.Aoi); // 视野6米
                    numericComponent.Set(NumericType.Level,1);//等级

                    int hp = NumericHelper.CalcHpResult(0, unitConfig.Vit, unitConfig.JobHp/1000f);
                    int sp = NumericHelper.CalcSpResult(0,unitConfig.Intell,unitConfig.JobSp/1000f);
                    numericComponent.Set(NumericType.SpeedBase,unitConfig.Speed/1000f); // 速度是3米每秒
                    
                    numericComponent.Set(NumericType.STRBase,unitConfig.Str);
                    numericComponent.Set(NumericType.AGIBase,unitConfig.Agi);
                    numericComponent.Set(NumericType.VITBase,unitConfig.Vit);
                    numericComponent.Set(NumericType.INTBase,unitConfig.Intell);
                    numericComponent.Set(NumericType.DEXBase,unitConfig.Dex);
                    numericComponent.Set(NumericType.LUKBase,unitConfig.Luk);
                    
                    numericComponent.Set(NumericType.HpBase, hp);
                    numericComponent.Set(NumericType.MaxHpBase, hp);
                    numericComponent.Set(NumericType.SpBase,sp);
                    //是否存活组件
                    unit.AddComponent<ReliveComponent>();
                    //背包组件
                    unit.AddComponent<KnapsackComponent>();
                    //施法组件
                    unit.AddComponent<CastComponent>();
                    unitComponent.Add(unit);
                    //装备组件后面加
                    // 加入aoi
                    var aoiEntity=unit.AddComponent<AOIEntity, int, float3>(unitConfig.Aoi, unit.Position);
                    return unit;
                }
                default:
                    throw new Exception($"not such unit type: {unitType}");
            }
        }
        /// <summary>
        /// 创建子弹
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="ownerId">拥有者</param>
        /// <param name="unitConfigId">一般写死9001 UnitConfig表</param>
        /// <param name="bulletConfigId">BulletConfig表</param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Unit CreateBullet(Scene scene, long ownerId, int unitConfigId, int bulletConfigId, float3 pos)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit owner = unitComponent.Get(ownerId);
            if (owner == null || owner.IsDisposed || !owner.IsBattleUnit())
            {
                Log.Error($"CreateBullet owner invalid: ownerId={ownerId}");
                return null;
            }

            UnitConfig unitConfig = UnitConfigCategory.Instance.Get(unitConfigId);
            Unit unit = unitComponent.AddChild<Unit, int, string>(unitConfigId,unitConfig.Name);
            unit.Position = pos;
            unit.OwnerId = ownerId;

            BulletComponent bulletComponent = unit.AddComponent<BulletComponent, int>(bulletConfigId);
            bulletComponent.OwnerId = ownerId;

            unitComponent.Add(unit);
            return unit;
        }


        public static Unit CreateMonster(Scene scene,int unitConfigId,float3 pos)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            UnitConfig unitConfig = UnitConfigCategory.Instance.Get(unitConfigId);
            Unit unit = unitComponent.AddChild<Unit, int, string>(unitConfigId,unitConfig.Name);
            unit.AddComponent<MoveComponent>();
            unit.Position = pos;
            
            
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            numericComponent.Set(NumericType.SpeedBase,unitConfig.Speed/1000f); // 速度是3米每秒
            numericComponent.Set(NumericType.AOI, unitConfig.Aoi); // 视野6米
            
            numericComponent.Set(NumericType.HpBase, 1000);
            numericComponent.Set(NumericType.MaxHpBase, 1000);
            
            unit.AddComponent<ReliveComponent>();
            unit.AddComponent<CastComponent>();
            unit.AddComponent<NumericNoticeComponent>();
            
            unitComponent.Add(unit);
            // 加入aoi
            var aoiEntity=unit.AddComponent<AOIEntity, int, float3>(unitConfig.Aoi, unit.Position);
            return unit;
        }
    }
}