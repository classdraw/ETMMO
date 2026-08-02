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
                    unit.AddComponent<UnitDBSaveComponent>();
                    
                    
                    unit.AddComponent<MoveComponent>();
                    unit.Position = new float3(-8.7f, 0f, -15.5f);//最好给新手村第一个场景的坐标 或者新手安全区随机一个 可以写死
                    NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
                    InitNumericFromConfigByPlayer(numericComponent, unitConfig,1);
                    
                    unit.AddComponent<ReliveComponent>();
                    unit.AddComponent<CastComponent>();
                    unit.AddComponent<NumericNoticeComponent>();
                    unit.AddComponent<BuffComponent>();
                    //背包组件
                    unit.AddComponent<KnapsackComponent>();

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


        public static Unit CreateMonster(Scene scene,MonsterConfig monsterConfig,float3 pos)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            UnitConfig unitConfig = UnitConfigCategory.Instance.Get(monsterConfig.UnitConfigId);
            Unit unit = unitComponent.AddChild<Unit, int, string>(monsterConfig.UnitConfigId,unitConfig.Name);
            unit.AddComponent<MoveComponent>();
            unit.Position = pos;
            
            
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            InitNumericFromMonsterConfig(numericComponent, monsterConfig, unitConfig);
            
            unit.AddComponent<ReliveComponent>();
            unit.AddComponent<CastComponent>();
            unit.AddComponent<NumericNoticeComponent>();
            unit.AddComponent<BuffComponent>();
            
            unitComponent.Add(unit);
            // 加入aoi
            var aoiEntity=unit.AddComponent<AOIEntity, int, float3>(unitConfig.Aoi, unit.Position);
            return unit;
        }

        private static void InitNumericFromConfigByPlayer(NumericComponent numericComponent, UnitConfig unitConfig, int level)
        {
            int str = unitConfig.Str;
            int agi = unitConfig.Agi;
            int vit = unitConfig.Vit;
            int intell = unitConfig.Intell;
            int dex = unitConfig.Dex;
            int luk = unitConfig.Luk;

            numericComponent.Set(NumericType.Element, (int)ElementType.Neutral);
            numericComponent.Set(NumericType.AOI, unitConfig.Aoi);
            numericComponent.Set(NumericType.Level, level);
            numericComponent.Set(NumericType.SpeedBase, unitConfig.Speed / 1000f);

            numericComponent.Set(NumericType.STRBase, str);
            numericComponent.Set(NumericType.AGIBase, agi);
            numericComponent.Set(NumericType.VITBase, vit);
            numericComponent.Set(NumericType.INTBase, intell);
            numericComponent.Set(NumericType.DEXBase, dex);
            numericComponent.Set(NumericType.LUKBase, luk);

            int hp = NumericHelper.CalcHpResult(level, vit, unitConfig.JobHp / 1000f);
            int sp = NumericHelper.CalcSpResult(level, intell, unitConfig.JobSp / 1000f);
            numericComponent.Set(NumericType.HpBase, hp);
            numericComponent.Set(NumericType.MaxHpBase, hp);
            numericComponent.Set(NumericType.SpBase, sp);

            bool isRanged = unitConfig.Range > 1;
            int atk = isRanged
                ? NumericHelper.CalcPlayerRangedAtk(dex, str, luk)
                : NumericHelper.CalcPlayerMeleeAtk(str, dex, luk);
            numericComponent.Set(NumericType.AtkBase, atk);
            numericComponent.Set(NumericType.AtkRandom, 0);

            numericComponent.Set(NumericType.DefBase, NumericHelper.CalcPlayerDef(vit));
            numericComponent.Set(NumericType.DefRandom, 0);

            int matkMin = NumericHelper.CalcPlayerMAtkMin(intell);
            int matkMax = NumericHelper.CalcPlayerMAtkMax(intell);
            numericComponent.Set(NumericType.MAtkBase, matkMin);
            numericComponent.Set(NumericType.MAtkRandom, matkMax - matkMin);

            numericComponent.Set(NumericType.MDefBase, NumericHelper.CalcPlayerMDef(intell));
            numericComponent.Set(NumericType.MDefRandom, 0);

            numericComponent.Set(NumericType.Hit, NumericHelper.CalcPlayerHit(level, dex));
            numericComponent.Set(NumericType.Flee, NumericHelper.CalcPlayerFlee(level, agi));
            numericComponent.Set(NumericType.AtkSpeed, NumericHelper.CalcPlayerAtkSpeed(agi, dex));
            numericComponent.Set(NumericType.AtkRange, unitConfig.Range);
        }

        private static void InitNumericFromMonsterConfig(NumericComponent numericComponent, MonsterConfig monsterConfig, UnitConfig unitConfig)
        {
            numericComponent.Set(NumericType.Element,monsterConfig.Element);
            numericComponent.Set(NumericType.AOI, unitConfig.Aoi);
            numericComponent.Set(NumericType.Level,monsterConfig.Level);
            numericComponent.Set(NumericType.SpeedBase, unitConfig.Speed / 1000f);

            numericComponent.Set(NumericType.STRBase, unitConfig.Str);
            numericComponent.Set(NumericType.AGIBase, unitConfig.Agi);
            numericComponent.Set(NumericType.VITBase, unitConfig.Vit);
            numericComponent.Set(NumericType.INTBase, unitConfig.Intell);
            numericComponent.Set(NumericType.DEXBase, unitConfig.Dex);
            numericComponent.Set(NumericType.LUKBase, unitConfig.Luk);

            numericComponent.Set(NumericType.HpBase, monsterConfig.Hp);
            numericComponent.Set(NumericType.MaxHpBase, monsterConfig.Hp);
            numericComponent.Set(NumericType.SpBase, 0);

            SetRangeNumeric(numericComponent, NumericType.AtkBase, NumericType.AtkRandom, monsterConfig.Atk);
            SetRangeNumeric(numericComponent, NumericType.DefBase, NumericType.DefRandom, monsterConfig.Def);
            SetRangeNumeric(numericComponent, NumericType.MAtkBase, NumericType.MAtkRandom, monsterConfig.MAtk);
            SetRangeNumeric(numericComponent, NumericType.MDefBase, NumericType.MDefRandom, monsterConfig.MDef);

            numericComponent.Set(NumericType.Hit, monsterConfig.Hit);
            numericComponent.Set(NumericType.Flee, monsterConfig.Flee);
            numericComponent.Set(NumericType.AtkSpeed, monsterConfig.AtkSpeed);
            numericComponent.Set(NumericType.AtkRange, monsterConfig.AtkRange);
        }

        private static void SetRangeNumeric(NumericComponent numericComponent, int baseType, int randomType, int[] values)
        {
            if (values == null || values.Length == 0)
            {
                return;
            }

            numericComponent.Set(baseType, values[0]);
            if (values.Length >= 2)
            {
                numericComponent.Set(randomType, values[1] - values[0]);
            }
        }
    }
}