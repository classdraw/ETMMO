using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 根据 Prefab 层级路径推断 UnitPartSlot（Root / SPUM 人形骨架命名）。
    /// </summary>
    internal static class UnitSpineCtrlSlotResolver
    {
        private static readonly Regex LeadingIndexTokenRegex = new Regex(@"-?\d+_(.+)$", RegexOptions.Compiled);
        private static readonly Regex CleanNameRegex = new Regex(@"[^a-zA-Z가-힣\s]", RegexOptions.Compiled);

        public static bool TryResolveSlotPart(
            SpriteRenderer renderer,
            Transform collectRoot,
            UnitPartSlotMapping.Context context,
            out int slotPart)
        {
            slotPart = -1;
            if (renderer == null)
            {
                return false;
            }

            string pathRootToLeaf = BuildPathRootToLeaf(renderer.transform, collectRoot);
            string renderName = NormalizeName(renderer.name);

            if (TryResolveDirect(pathRootToLeaf, renderName, out UnitPartSlot directSlot))
            {
                slotPart = (int)directSlot;
                return true;
            }

            if (!TryParseFromLegacyRules(renderer, collectRoot, pathRootToLeaf, renderName, context, out UnitPartSlot slot))
            {
                return false;
            }

            slotPart = (int)slot;
            return true;
        }

        private static string BuildPathRootToLeaf(Transform target, Transform root)
        {
            var segments = new List<string>();
            for (Transform t = target; t != null; t = t.parent)
            {
                segments.Add(NormalizeName(t.name));
                if (root != null && t == root)
                {
                    break;
                }
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private static string NormalizeName(string name)
        {
            return string.IsNullOrEmpty(name) ? string.Empty : name.Trim();
        }

        /// <summary>
        /// Root.prefab 常见路径 → 固定槽位（不依赖遍历顺序）。
        /// </summary>
        private static bool TryResolveDirect(string path, string renderName, out UnitPartSlot slot)
        {
            slot = default;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string p = path.Replace('\\', '/');

            if (ContainsSegment(p, "P_REye"))
            {
                slot = IsBackLayer(renderName, p) ? UnitPartSlot.RightEyeBack : UnitPartSlot.RightEyeFront;
                return true;
            }

            if (ContainsSegment(p, "P_LEye"))
            {
                slot = IsBackLayer(renderName, p) ? UnitPartSlot.LeftEyeBack : UnitPartSlot.LeftEyeFront;
                return true;
            }

            if (ContainsSegment(p, "5_Head")
                || (string.Equals(renderName, "Head", StringComparison.OrdinalIgnoreCase) && ContainsSegment(p, "P_Head")))
            {
                slot = UnitPartSlot.BodyHead;
                return true;
            }

            if (ContainsSegment(p, "7_Hair") || ContainsSegment(p, "P_Hair"))
            {
                slot = UnitPartSlot.Hair;
                return true;
            }

            if (ContainsSegment(p, "6_FaceHair") || ContainsSegment(p, "P_Mustache"))
            {
                slot = UnitPartSlot.FaceHair;
                return true;
            }

            if (string.Equals(renderName, "Body", StringComparison.OrdinalIgnoreCase)
                && ContainsSegment(p, "P_Body"))
            {
                slot = UnitPartSlot.BodyBody;
                return true;
            }

            if (IsBodyArmLeft(p, renderName))
            {
                slot = UnitPartSlot.LeftBodyArm;
                return true;
            }

            if (IsBodyArmRight(p, renderName))
            {
                slot = UnitPartSlot.RightBodyArm;
                return true;
            }

            if (IsBodyFootLeft(p, renderName))
            {
                slot = UnitPartSlot.LeftBodyFoot;
                return true;
            }

            if (IsBodyFootRight(p, renderName))
            {
                slot = UnitPartSlot.RightBodyFoot;
                return true;
            }

            if (IsClothLeft(p, renderName))
            {
                slot = UnitPartSlot.LeftCloth;
                return true;
            }

            if (IsClothRight(p, renderName))
            {
                slot = UnitPartSlot.RightCloth;
                return true;
            }

            if (IsClothBody(p, renderName))
            {
                slot = UnitPartSlot.BodyCloth;
                return true;
            }

            if (IsPantLeft(p, renderName))
            {
                slot = UnitPartSlot.LeftPant;
                return true;
            }

            if (IsPantRight(p, renderName))
            {
                slot = UnitPartSlot.RightPant;
                return true;
            }

            if (IsFrontHelmet(p, renderName))
            {
                slot = UnitPartSlot.FrontHelmet;
                return true;
            }

            if (IsBackHelmet(p, renderName))
            {
                slot = UnitPartSlot.BackHelmet;
                return true;
            }

            if (IsBodyArmor(p, renderName))
            {
                slot = UnitPartSlot.BodyArmor;
                return true;
            }

            if (IsArmorLeft(p, renderName))
            {
                slot = UnitPartSlot.LeftArmor;
                return true;
            }

            if (IsArmorRight(p, renderName))
            {
                slot = UnitPartSlot.RightArmor;
                return true;
            }

            if (ContainsSegment(p, "P_Back") && string.Equals(renderName, "Back", StringComparison.OrdinalIgnoreCase))
            {
                slot = UnitPartSlot.Back;
                return true;
            }

            if (ContainsSegment(p, "L_Weapon"))
            {
                slot = UnitPartSlot.LeftWeaponOthers;
                return true;
            }

            if (ContainsSegment(p, "R_Weapon"))
            {
                slot = UnitPartSlot.RightWeaponOthers;
                return true;
            }

            if (ContainsSegment(p, "L_Shield"))
            {
                slot = UnitPartSlot.LeftWeaponShield;
                return true;
            }

            if (ContainsSegment(p, "R_Shield"))
            {
                slot = UnitPartSlot.RightWeaponShield;
                return true;
            }

            return false;
        }

        private static bool TryParseFromLegacyRules(
            SpriteRenderer renderer,
            Transform collectRoot,
            string pathRootToLeaf,
            string renderName,
            UnitPartSlotMapping.Context context,
            out UnitPartSlot slot)
        {
            slot = default;
            var ancestorNames = new List<string>();
            for (Transform t = renderer.transform; t != null; t = t.parent)
            {
                ancestorNames.Add(NormalizeName(t.name));
                if (collectRoot != null && t == collectRoot)
                {
                    break;
                }
            }

            bool isHorse = pathRootToLeaf.IndexOf("Horse", StringComparison.OrdinalIgnoreCase) >= 0
                           || pathRootToLeaf.IndexOf("1_Horse", StringComparison.OrdinalIgnoreCase) >= 0;

            string partType = null;
            string weaponSubType = null;
            string weaponHandDir = null;
            string subType = CleanStructureName(renderName);

            ApplyAncestorTokenRules(ancestorNames, ref partType, ref subType, ref weaponSubType, ref weaponHandDir);

            if (string.IsNullOrEmpty(partType))
            {
                string assetPath = UnitSpineCtrlCollectUtil.ResolveSpriteAssetPath(renderer);
                if (TryParseFromSpriteAssetPath(assetPath, out bool pathIsHorse, out string pathPartType, out string pathWeaponSubType))
                {
                    isHorse = pathIsHorse;
                    partType = pathPartType;
                    weaponSubType = pathWeaponSubType;
                }
            }

            if (string.IsNullOrEmpty(partType))
            {
                return false;
            }

            if (string.Equals(partType, "Helmet", StringComparison.OrdinalIgnoreCase)
                && string.Equals(subType, "Helmet", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrEmpty(weaponHandDir))
            {
                weaponHandDir = "Front";
            }

            return UnitPartSlotMapping.TryResolve(
                isHorse,
                partType,
                weaponSubType,
                subType,
                context,
                out slot,
                weaponHandDir);
        }

        private static void ApplyAncestorTokenRules(
            IReadOnlyList<string> ancestorNames,
            ref string partType,
            ref string subType,
            ref string weaponSubType,
            ref string weaponHandDir)
        {
            for (int i = 0; i < ancestorNames.Count; i++)
            {
                string name = ancestorNames[i];
                if (!TryExtractIndexedToken(name, out string token))
                {
                    continue;
                }

                switch (token)
                {
                    case "Head":
                        partType = "Body";
                        subType = "Head";
                        break;
                    case "Hair":
                        partType = "Hair";
                        subType = "Hair";
                        break;
                    case "FaceHair":
                        partType = "FaceHair";
                        subType = "FaceHair";
                        break;
                    case "Helmet1":
                    case "Helmet2":
                        partType = "Helmet";
                        subType = token == "Helmet2" ? "Back" : "Front";
                        break;
                    case "L_Arm":
                        partType = "Body";
                        subType = "Arm_L";
                        break;
                    case "R_Arm":
                        partType = "Body";
                        subType = "Arm_R";
                        break;
                    case "L_Foot":
                        partType = "Body";
                        subType = "Foot_L";
                        break;
                    case "R_Foot":
                        partType = "Body";
                        subType = "Foot_R";
                        break;
                    case "LCArm":
                        partType = "Cloth";
                        subType = "Left";
                        break;
                    case "RCArm":
                        partType = "Cloth";
                        subType = "Right";
                        break;
                    case "L_Cloth":
                        partType = "Cloth";
                        subType = "Left";
                        break;
                    case "R_Cloth":
                        partType = "Cloth";
                        subType = "Right";
                        break;
                    case "L_Shoulder":
                        partType = "Armor";
                        subType = "Left";
                        break;
                    case "R_Shoulder":
                        partType = "Armor";
                        subType = "Right";
                        break;
                }
            }

            if ((string.Equals(subType, "Back", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(subType, "Front", StringComparison.OrdinalIgnoreCase))
                && string.IsNullOrEmpty(partType)
                && !string.IsNullOrEmpty(FindAncestorContaining(ancestorNames, "Helmet")))
            {
                partType = "Helmet";
            }
            else if ((string.Equals(subType, "Back", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(subType, "Front", StringComparison.OrdinalIgnoreCase))
                     && string.IsNullOrEmpty(partType))
            {
                partType = "Eye";
            }

            if (string.Equals(subType, "Body", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(partType))
            {
                partType = "Body";
                subType = "Body";
            }
        }

        private static bool TryExtractIndexedToken(string nodeName, out string token)
        {
            token = null;
            if (string.IsNullOrEmpty(nodeName))
            {
                return false;
            }

            Match match = LeadingIndexTokenRegex.Match(nodeName.Trim());
            if (!match.Success)
            {
                return false;
            }

            token = match.Groups[1].Value.Trim();
            return !string.IsNullOrEmpty(token);
        }

        private static string FindAncestorContaining(IReadOnlyList<string> names, string keyword)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return names[i];
                }
            }

            return null;
        }

        private static bool ContainsSegment(string path, string segment)
        {
            return path.IndexOf(segment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsBackLayer(string renderName, string path)
        {
            if (string.Equals(renderName, "Back", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return ContainsSegment(path, "PivotBack");
        }

        private static bool IsBodyArmLeft(string path, string renderName)
        {
            return string.Equals(renderName, "ArmL", StringComparison.OrdinalIgnoreCase)
                   || ContainsSegment(path, "20_L_Arm")
                   || ContainsSegment(path, "P_LArm");
        }

        private static bool IsBodyArmRight(string path, string renderName)
        {
            return string.Equals(renderName, "ArmR", StringComparison.OrdinalIgnoreCase)
                   || ContainsSegment(path, "-20_R_Arm")
                   || ContainsSegment(path, "P_RArm");
        }

        private static bool IsBodyFootLeft(string path, string renderName)
        {
            return ContainsSegment(path, "P_LFoot") || ContainsSegment(path, "_3L_Foot");
        }

        private static bool IsBodyFootRight(string path, string renderName)
        {
            return ContainsSegment(path, "P_RFoot") || ContainsSegment(path, "_12R_Foot");
        }

        private static bool IsClothLeft(string path, string renderName)
        {
            return ContainsSegment(path, "P_LCloth")
                   || ContainsSegment(path, "_2L_Cloth")
                   || ContainsSegment(path, "21_LCArm")
                   || ContainsSegment(path, "P_LCArm");
        }

        private static bool IsClothRight(string path, string renderName)
        {
            return ContainsSegment(path, "P_RCloth")
                   || ContainsSegment(path, "_11R_Cloth")
                   || ContainsSegment(path, "-19_RCArm")
                   || ContainsSegment(path, "P_RCArm");
        }

        private static bool IsClothBody(string path, string renderName)
        {
            return string.Equals(renderName, "ClothBody", StringComparison.OrdinalIgnoreCase)
                   || ContainsSegment(path, "P_ClothBody");
        }

        private static bool IsPantLeft(string path, string renderName)
        {
            return ContainsSegment(path, "_2L_Cloth") == false && ContainsSegment(path, "L_Pant");
        }

        private static bool IsPantRight(string path, string renderName)
        {
            return ContainsSegment(path, "R_Pant");
        }

        private static bool IsFrontHelmet(string path, string renderName)
        {
            return ContainsSegment(path, "11_Helmet1")
                   || (ContainsSegment(path, "P_Helmet") && string.Equals(renderName, "Front", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsBackHelmet(string path, string renderName)
        {
            return ContainsSegment(path, "12_Helmet2")
                   || (ContainsSegment(path, "P_Helmet") && string.Equals(renderName, "Back", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsBodyArmor(string path, string renderName)
        {
            return string.Equals(renderName, "BodyArmor", StringComparison.OrdinalIgnoreCase)
                   || ContainsSegment(path, "P_ArmorBody");
        }

        private static bool IsArmorLeft(string path, string renderName)
        {
            return ContainsSegment(path, "25_L_Shoulder");
        }

        private static bool IsArmorRight(string path, string renderName)
        {
            return ContainsSegment(path, "-15_R_Shoulder") || ContainsSegment(path, "R_Shoulder");
        }

        private static bool TryParseFromSpriteAssetPath(
            string itemPath,
            out bool isHorse,
            out string partType,
            out string weaponSubType)
        {
            isHorse = false;
            partType = string.Empty;
            weaponSubType = string.Empty;

            if (string.IsNullOrEmpty(itemPath))
            {
                return false;
            }

            string normalized = itemPath.Replace('\\', '/');
            if (normalized.IndexOf("/1_Horse/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isHorse = true;
            }
            else if (normalized.IndexOf("/0_Unit/", StringComparison.OrdinalIgnoreCase) < 0
                     && normalized.IndexOf("Legacy/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            string[] parts = normalized.Split('/');
            int spriteIndex = Array.IndexOf(parts, "0_Sprite");
            if (spriteIndex < 0 || spriteIndex + 1 >= parts.Length)
            {
                return false;
            }

            partType = StripFolderPrefix(parts[spriteIndex + 1]);
            if (isHorse)
            {
                partType = "Body";
            }

            if (string.Equals(partType, "Weapons", StringComparison.OrdinalIgnoreCase) && spriteIndex + 2 < parts.Length)
            {
                weaponSubType = StripFolderPrefix(parts[spriteIndex + 2]);
            }

            return !string.IsNullOrEmpty(partType);
        }

        private static string StripFolderPrefix(string folderName)
        {
            if (TryExtractIndexedToken(folderName, out string token))
            {
                return token;
            }

            return folderName;
        }

        private static string CleanStructureName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return string.Empty;
            }

            return CleanNameRegex.Replace(objectName, string.Empty).Trim();
        }
    }
}
