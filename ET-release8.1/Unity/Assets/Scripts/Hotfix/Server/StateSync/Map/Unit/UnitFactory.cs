using System;
using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(BulletComponent))]
    [FriendOfAttribute(typeof(ET.Server.AOIEntity))]
    public static partial class UnitFactory
    {
        public static UnitConfig GetUnitConfig(int configId)
        {
            return UnitConfigCategory.Instance.Get(configId);
        }

        public static Unit Create(Scene scene, long id, int configId, string name, UnitType unitType)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            switch (unitType)
            {
                case UnitType.Player:
                case UnitType.Robot:
                {
                    return CreateHumanoidUnit(unitComponent, id, configId, name, unitType);
                }
                default:
                    throw new Exception($"not such unit type: {unitType}");
            }
        }

        private static Unit CreateHumanoidUnit(UnitComponent unitComponent, long id, int configId, string name, UnitType unitType)
        {
            UnitConfig unitConfig = UnitConfigCategory.Instance.Get(configId);

            Unit unit = unitComponent.AddChildWithId<Unit, int, string>(id, configId, name);
            if (unitType != UnitType.Robot)
            {
                unit.AddComponent<UnitDBSaveComponent>();
            }

            unit.AddComponent<MoveComponent>();
            unit.Position = new float3(-8.7f, 0f, -15.5f);
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            InitNumericFromConfigByPlayer(numericComponent, unitConfig, 1);

            unit.AddComponent<ReliveComponent>();
            unit.AddComponent<CastComponent>();
            unit.AddComponent<SkillStatusComponent>();
            unit.AddComponent<NumericNoticeComponent>();
            unit.AddComponent<BuffComponent>();
            unit.AddComponent<KnapsackComponent>();

            unitComponent.Add(unit);
            unit.AddComponent<AOIEntity, int, float3>(unitConfig.Aoi, unit.Position);

            return unit;
        }

        /// <summary>
        /// 压测机器人：与 <see cref="CreateMonster"/> 相同流程，固定 UnitConfig 9101（Type=Robot）。
        /// </summary>
        public static Unit CreatePressureRobot(Scene scene, float3 pos, string name = null)
        {
            int configId = RobotUnitHelper.RobotUnitConfigId;
            UnitConfig unitConfig = UnitConfigCategory.Instance.Get(configId);
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            name = string.IsNullOrEmpty(name) ? unitConfig.Name : name;

            Unit unit = unitComponent.AddChild<Unit, int, string>(configId, name);
            unit.TableConfigId = 0;
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(scene.Name);
            unit.Position = pos;

            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            InitNumericFromConfigByPlayer(numericComponent, unitConfig, 1);

            unit.AddComponent<ReliveComponent>();
            unit.AddComponent<CastComponent>();
            unit.AddComponent<SkillStatusComponent>();
            unit.AddComponent<NumericNoticeComponent>();
            unit.AddComponent<BuffComponent>();

            unitComponent.Add(unit);
            return unit;
        }

        /// <summary>地图刷怪/压测 Robot：须在 MapId 与阵营分配完成后再调用。</summary>
        public static void AddBattleUnitAoi(Unit unit, float3 pos)
        {
            unit.AddComponent<AOIEntity, int, float3>(unit.Config().Aoi, pos);
        }

        /// <summary>
        /// 创建子弹
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="ownerId">拥有者</param>
        /// <param name="bulletConfig">BulletConfig 表，Unit 取自 bulletConfig.UnitConfigId</param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Unit CreateBullet(Scene scene, long ownerId, BulletConfig bulletConfig, float3 pos,quaternion rotate)
        {
            if (bulletConfig == null)
            {
                Log.Error("CreateBullet bulletConfig is null");
                return null;
            }

            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit owner = unitComponent.Get(ownerId);
            if (owner == null || owner.IsDisposed || !owner.IsBattleUnit())
            {
                Log.Error($"CreateBullet owner invalid: ownerId={ownerId}");
                return null;
            }

            if (!UnitConfigCategory.Instance.Contain(bulletConfig.UnitConfigId))
            {
                Log.Error($"CreateBullet UnitConfig not found: bulletConfigId={bulletConfig.Id}, unitConfigId={bulletConfig.UnitConfigId}");
                return null;
            }

            UnitConfig unitConfig = UnitConfigCategory.Instance.Get(bulletConfig.UnitConfigId);
            Unit unit = unitComponent.AddChild<Unit, int, string>(bulletConfig.UnitConfigId, unitConfig.Name);
            unit.TableConfigId = bulletConfig.Id;
            unit.Position = pos;
            unit.Rotation = rotate;
            unit.OwnerId = ownerId;
            unit.MapId = owner.MapId;
            unit.TeamId = owner.TeamId;
            unit.AddComponent<CastComponent>();
            unit.AddComponent<MoveComponent>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            numericComponent.Set(NumericType.Speed,unitConfig.Speed);
            
            //子弹组件
            BulletComponent bulletComponent = unit.AddComponent<BulletComponent, int>(bulletConfig.Id);
            bulletComponent.OwnerId = ownerId;

            int aoiDistance = unitConfig.Aoi;
            if (aoiDistance <= 0)
            {
                AOIEntity ownerAoi = owner.GetComponent<AOIEntity>();
                if (ownerAoi != null && !ownerAoi.IsDisposed)
                {
                    aoiDistance = ownerAoi.ViewDistance;
                }
            }

            numericComponent.Set(NumericType.AOI, aoiDistance);
            CampHelper.CopyFactionFromUnit(unit, owner);
            unitComponent.Add(unit);
            unit.AddComponent<AOIEntity, int, float3>(aoiDistance, unit.Position);

            return unit;
        }


        public static Unit CreateMonster(Scene scene, MonsterConfig monsterConfig, float3 pos)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            UnitConfig unitConfig = UnitConfigCategory.Instance.Get(monsterConfig.UnitConfigId);
            Unit unit = unitComponent.AddChild<Unit, int, string>(monsterConfig.UnitConfigId, unitConfig.Name);
            unit.TableConfigId = monsterConfig.Id;
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(scene.Name);
            unit.Position = pos;


            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            InitNumericFromMonsterConfig(numericComponent, monsterConfig, unitConfig);

            unit.AddComponent<ReliveComponent>();
            unit.AddComponent<CastComponent>();
            unit.AddComponent<SkillStatusComponent>();
            unit.AddComponent<NumericNoticeComponent>();
            unit.AddComponent<BuffComponent>();

            if (monsterConfig.ModelType == (int)MonsterModelType.PartAssembly
                && !string.IsNullOrEmpty(monsterConfig.Model))
            {
                unit.BaseExternalDisplay = monsterConfig.Model;
            }

            unitComponent.Add(unit);
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
            numericComponent.Set(NumericType.Element, monsterConfig.Element);
            numericComponent.Set(NumericType.AOI, unitConfig.Aoi);
            numericComponent.Set(NumericType.Level, monsterConfig.Level);
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