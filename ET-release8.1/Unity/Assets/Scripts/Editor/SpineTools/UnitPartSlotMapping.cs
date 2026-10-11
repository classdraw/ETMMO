using System;

namespace ET
{
    public static class UnitPartSlotMapping
    {
        public sealed class Context
        {
            public int EyeBackIndex;
            public int EyeFrontIndex;
            public int HorseFootFrontTopIndex;
            public int HorseFootFrontBottomIndex;
            public int HorseFootBackTopIndex;
            public int HorseFootBackBottomIndex;
        }

        private static readonly UnitPartSlot[] EyeBackOrder =
        {
            UnitPartSlot.RightEyeBack, UnitPartSlot.LeftEyeBack,
        };

        private static readonly UnitPartSlot[] EyeFrontOrder =
        {
            UnitPartSlot.RightEyeFront, UnitPartSlot.LeftEyeFront,
        };

        private static readonly UnitPartSlot[] HorseFootFrontTopOrder =
        {
            UnitPartSlot.HorseFrontFootFrontTop, UnitPartSlot.HorseBackFootFrontTop,
        };

        private static readonly UnitPartSlot[] HorseFootFrontBottomOrder =
        {
            UnitPartSlot.HorseFrontFootFrontBottom, UnitPartSlot.HorseBackFootFrontBottom,
        };

        private static readonly UnitPartSlot[] HorseFootBackTopOrder =
        {
            UnitPartSlot.HorseFrontFootBackTop, UnitPartSlot.HorseBackFootBackTop,
        };

        private static readonly UnitPartSlot[] HorseFootBackBottomOrder =
        {
            UnitPartSlot.HorseFrontFootBackBottom, UnitPartSlot.HorseBackFootBackBottom,
        };

        public static bool TryResolve(
            bool isHorse,
            string partType,
            string weaponSubType,
            string subType,
            Context context,
            out UnitPartSlot slot,
            string weaponHandDir = null)
        {
            slot = default;
            if (string.IsNullOrEmpty(partType) || string.IsNullOrEmpty(subType))
            {
                return false;
            }

            context ??= new Context();

            if (isHorse)
            {
                return TryResolveHorse(subType, context, out slot);
            }

            switch (partType)
            {
                case "Eye":
                    if (string.Equals(subType, "Back", StringComparison.OrdinalIgnoreCase))
                    {
                        return TryTakeFromOrder(EyeBackOrder, ref context.EyeBackIndex, out slot);
                    }

                    if (string.Equals(subType, "Front", StringComparison.OrdinalIgnoreCase))
                    {
                        return TryTakeFromOrder(EyeFrontOrder, ref context.EyeFrontIndex, out slot);
                    }

                    return false;

                case "Body":
                    return TryMapBody(subType, out slot);

                case "Hair":
                    slot = UnitPartSlot.Hair;
                    return true;

                case "FaceHair":
                    slot = UnitPartSlot.FaceHair;
                    return true;

                case "Cloth":
                    return TryMapLeftRightBody(subType, UnitPartSlot.LeftCloth, UnitPartSlot.RightCloth, UnitPartSlot.BodyCloth,
                        out slot);

                case "Pant":
                    return TryMapLeftRight(subType, UnitPartSlot.LeftPant, UnitPartSlot.RightPant, out slot);

                case "Helmet":
                    if (string.Equals(subType, "Back", StringComparison.OrdinalIgnoreCase))
                    {
                        slot = UnitPartSlot.BackHelmet;
                        return true;
                    }

                    slot = UnitPartSlot.FrontHelmet;
                    return true;

                case "Armor":
                    return TryMapLeftRightBody(subType, UnitPartSlot.LeftArmor, UnitPartSlot.RightArmor, UnitPartSlot.BodyArmor, out slot);

                case "Back":
                    slot = UnitPartSlot.Back;
                    return true;

                case "Weapons":
                    return TryResolveWeapon(weaponSubType, weaponHandDir, out slot);

                default:
                    return false;
            }
        }

        private static bool TryResolveHorse(string subType, Context context, out UnitPartSlot slot)
        {
            slot = default;
            switch (subType)
            {
                case "BodyBack":
                    slot = UnitPartSlot.HorseBodyBack;
                    return true;
                case "BodyFront":
                    slot = UnitPartSlot.HorseBodyFront;
                    return true;
                case "Neck":
                    slot = UnitPartSlot.HorseNeck;
                    return true;
                case "Head":
                    slot = UnitPartSlot.HorseHead;
                    return true;
                case "Tail":
                    slot = UnitPartSlot.HorseTail;
                    return true;
                case "Acc":
                    slot = UnitPartSlot.HorseAcc;
                    return true;
                case "FootFrontTop":
                    return TryTakeFromOrder(HorseFootFrontTopOrder, ref context.HorseFootFrontTopIndex, out slot);
                case "FootFrontBottom":
                    return TryTakeFromOrder(HorseFootFrontBottomOrder, ref context.HorseFootFrontBottomIndex, out slot);
                case "FootBackTop":
                    return TryTakeFromOrder(HorseFootBackTopOrder, ref context.HorseFootBackTopIndex, out slot);
                case "FootBackBottom":
                    return TryTakeFromOrder(HorseFootBackBottomOrder, ref context.HorseFootBackBottomIndex, out slot);
                default:
                    return false;
            }
        }

        private static bool TryMapBody(string subType, out UnitPartSlot slot)
        {
            slot = default;
            switch (subType)
            {
                case "Head":
                    slot = UnitPartSlot.BodyHead;
                    return true;
                case "Body":
                    slot = UnitPartSlot.BodyBody;
                    return true;
                case "Arm_L":
                    slot = UnitPartSlot.LeftBodyArm;
                    return true;
                case "Arm_R":
                    slot = UnitPartSlot.RightBodyArm;
                    return true;
                case "Foot_L":
                    slot = UnitPartSlot.LeftBodyFoot;
                    return true;
                case "Foot_R":
                    slot = UnitPartSlot.RightBodyFoot;
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryMapLeftRight(string subType, UnitPartSlot left, UnitPartSlot right, out UnitPartSlot slot)
        {
            slot = default;
            if (string.Equals(subType, "Left", StringComparison.OrdinalIgnoreCase))
            {
                slot = left;
                return true;
            }

            if (string.Equals(subType, "Right", StringComparison.OrdinalIgnoreCase))
            {
                slot = right;
                return true;
            }

            return false;
        }

        private static bool TryMapLeftRightBody(string subType, UnitPartSlot left, UnitPartSlot right, UnitPartSlot body,
            out UnitPartSlot slot)
        {
            if (TryMapLeftRight(subType, left, right, out slot))
            {
                return true;
            }

            if (string.Equals(subType, "Body", StringComparison.OrdinalIgnoreCase))
            {
                slot = body;
                return true;
            }

            slot = default;
            return false;
        }

        public static bool TryResolveWeapon(string weaponSubType, string weaponHandDir, out UnitPartSlot slot)
        {
            slot = default;
            if (string.IsNullOrEmpty(weaponSubType))
            {
                return false;
            }

            string key = weaponSubType;
            if (string.Equals(key, "Hamme", StringComparison.OrdinalIgnoreCase))
            {
                key = "Hammer";
            }

            bool isLeft = string.Equals(weaponHandDir, "Left", StringComparison.OrdinalIgnoreCase);

            switch (key.ToLowerInvariant())
            {
                case "sword":
                    slot = isLeft ? UnitPartSlot.LeftWeaponSword : UnitPartSlot.RightWeaponSword;
                    return true;
                case "axe":
                    slot = isLeft ? UnitPartSlot.LeftWeaponAxe : UnitPartSlot.RightWeaponAxe;
                    return true;
                case "bow":
                    slot = isLeft ? UnitPartSlot.LeftWeaponBow : UnitPartSlot.RightWeaponBow;
                    return true;
                case "shield":
                    slot = isLeft ? UnitPartSlot.LeftWeaponShield : UnitPartSlot.RightWeaponShield;
                    return true;
                case "spear":
                    slot = isLeft ? UnitPartSlot.LeftWeaponSpear : UnitPartSlot.RightWeaponSpear;
                    return true;
                case "wand":
                    slot = isLeft ? UnitPartSlot.LeftWeaponWand : UnitPartSlot.RightWeaponWand;
                    return true;
                case "hammer":
                    slot = isLeft ? UnitPartSlot.LeftWeaponHammer : UnitPartSlot.RightWeaponHammer;
                    return true;
                case "dagger":
                    slot = isLeft ? UnitPartSlot.LeftWeaponDagger : UnitPartSlot.RightWeaponDagger;
                    return true;
                case "mace":
                    slot = isLeft ? UnitPartSlot.LeftWeaponMace : UnitPartSlot.RightWeaponMace;
                    return true;
                case "others":
                    slot = isLeft ? UnitPartSlot.LeftWeaponOthers : UnitPartSlot.RightWeaponOthers;
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryTakeFromOrder(UnitPartSlot[] order, ref int index, out UnitPartSlot slot)
        {
            slot = default;
            if (index >= order.Length)
            {
                return false;
            }

            slot = order[index];
            index++;
            return true;
        }
    }
}
