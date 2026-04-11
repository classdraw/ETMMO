using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 眼睛部位：库表/协议只存一份配置 Id（前眼 EyeFront、后眼 EyeBack），显示层需同时写到 Left/Right 绑点。
    /// </summary>
    public static class AvatarEyePairUtility
    {
        public static bool IsFrontEyePart(AvatarPartType t)
        {
            return t == AvatarPartType.Eye_Front_Left || t == AvatarPartType.Eye_Front_Right;
        }

        public static bool IsBackEyePart(AvatarPartType t)
        {
            return t == AvatarPartType.Eye_Back_Left || t == AvatarPartType.Eye_Back_Right;
        }

        public static bool IsEyePairPart(AvatarPartType t)
        {
            return IsFrontEyePart(t) || IsBackEyePart(t);
        }

        public static bool HasBindPointForCollector(ReferenceSpriteCollector collector, AvatarPartType configPartType)
        {
            if (collector == null)
            {
                return false;
            }

            if (IsFrontEyePart(configPartType))
            {
                return collector.Get(AvatarPartType.Eye_Front_Left.ToString()) != null
                    || collector.Get(AvatarPartType.Eye_Front_Right.ToString()) != null;
            }

            if (IsBackEyePart(configPartType))
            {
                return collector.Get(AvatarPartType.Eye_Back_Left.ToString()) != null
                    || collector.Get(AvatarPartType.Eye_Back_Right.ToString()) != null;
            }

            return collector.Get(configPartType.ToString()) != null;
        }

        public static bool HasBindPointForAvatarPart(Dictionary<AvatarPartType, SpriteRenderer> parts, AvatarPartType configPartType)
        {
            if (parts == null)
            {
                return false;
            }

            if (IsFrontEyePart(configPartType))
            {
                return SlotExists(parts, AvatarPartType.Eye_Front_Left) || SlotExists(parts, AvatarPartType.Eye_Front_Right);
            }

            if (IsBackEyePart(configPartType))
            {
                return SlotExists(parts, AvatarPartType.Eye_Back_Left) || SlotExists(parts, AvatarPartType.Eye_Back_Right);
            }

            return parts.TryGetValue(configPartType, out SpriteRenderer sr) && sr != null;
        }

        private static bool SlotExists(Dictionary<AvatarPartType, SpriteRenderer> parts, AvatarPartType partType)
        {
            return parts.TryGetValue(partType, out SpriteRenderer sr) && sr != null;
        }

        public static void ApplySpriteToEyePair(ReferenceSpriteCollector collector, AvatarPartType configPartType, Sprite sprite)
        {
            if (collector == null || sprite == null)
            {
                return;
            }

            if (IsFrontEyePart(configPartType))
            {
                SetCollectorSlot(collector, AvatarPartType.Eye_Front_Left, sprite);
                SetCollectorSlot(collector, AvatarPartType.Eye_Front_Right, sprite);
            }
            else if (IsBackEyePart(configPartType))
            {
                SetCollectorSlot(collector, AvatarPartType.Eye_Back_Left, sprite);
                SetCollectorSlot(collector, AvatarPartType.Eye_Back_Right, sprite);
            }
        }

        public static void ApplySpriteToEyePair(Dictionary<AvatarPartType, SpriteRenderer> parts, AvatarPartType configPartType, Sprite sprite)
        {
            if (parts == null || sprite == null)
            {
                return;
            }

            if (IsFrontEyePart(configPartType))
            {
                SetDictSlot(parts, AvatarPartType.Eye_Front_Left, sprite);
                SetDictSlot(parts, AvatarPartType.Eye_Front_Right, sprite);
            }
            else if (IsBackEyePart(configPartType))
            {
                SetDictSlot(parts, AvatarPartType.Eye_Back_Left, sprite);
                SetDictSlot(parts, AvatarPartType.Eye_Back_Right, sprite);
            }
        }

        private static void SetCollectorSlot(ReferenceSpriteCollector collector, AvatarPartType partType, Sprite sprite)
        {
            SpriteRenderer sr = collector.Get(partType.ToString());
            if (sr != null)
            {
                sr.sprite = sprite;
            }
        }

        private static void SetDictSlot(Dictionary<AvatarPartType, SpriteRenderer> parts, AvatarPartType partType, Sprite sprite)
        {
            if (parts.TryGetValue(partType, out SpriteRenderer sr) && sr != null)
            {
                sr.sprite = sprite;
            }
        }
    }
}
