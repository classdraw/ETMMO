namespace ET
{
    /// <summary>
    /// SPUM 换装槽位，与 PreviewMatchingElement / matchingTables 行序对应。
    /// 命名：UnitType Key + 结构名；Unit 为人形部位（如 LeftEyeBack），Horse 为马匹（如 HorseBodyBack）。
    /// </summary>
    public enum UnitPartSlot : byte
    {
        // UnitType: Unit（43 项，Root.prefab matchingTables）
        RightEyeBack = 0,
        RightEyeFront = 1,
        LeftEyeBack = 2,
        LeftEyeFront = 3,

        BodyHead = 4,
        BodyBody = 5,
        LeftBodyArm = 6,
        RightBodyArm = 7,
        LeftBodyFoot = 8,
        RightBodyFoot = 9,

        Hair = 10,
        FaceHair = 11,

        LeftCloth = 12,
        RightCloth = 13,
        BodyCloth = 14,

        RightPant = 15,
        LeftPant = 16,

        BackHelmet = 17,
        FrontHelmet = 18,

        BodyArmor = 19,
        LeftArmor = 20,
        RightArmor = 21,

        Back = 22,

        LeftWeaponSword = 23,
        RightWeaponSword = 24,
        LeftWeaponAxe = 25,
        RightWeaponAxe = 26,
        LeftWeaponBow = 27,
        RightWeaponBow = 28,
        LeftWeaponShield = 29,
        RightWeaponShield = 30,
        LeftWeaponSpear = 31,
        RightWeaponSpear = 32,
        LeftWeaponWand = 33,
        RightWeaponWand = 34,
        LeftWeaponHammer = 35,
        RightWeaponHammer = 36,
        LeftWeaponDagger = 37,
        RightWeaponDagger = 38,
        LeftWeaponMace = 39,
        RightWeaponMace = 40,
        LeftWeaponOthers = 41,
        RightWeaponOthers = 42,

        // UnitType: Horse, Index 11（14 项，LoadPrefabs_ChracterPivot HorseRoot matchingTables）
        HorseBodyBack = 43,
        HorseBodyFront = 44,
        HorseNeck = 45,
        HorseHead = 46,
        HorseFrontFootFrontTop = 47,
        HorseFrontFootFrontBottom = 48,
        HorseFrontFootBackTop = 49,
        HorseFrontFootBackBottom = 50,
        HorseBackFootFrontTop = 51,
        HorseBackFootFrontBottom = 52,
        HorseBackFootBackTop = 53,
        HorseBackFootBackBottom = 54,
        HorseTail = 55,
        HorseAcc = 56,

        Count = 57,
    }
}
