using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class SyntySkinnedBoneBinder
    {
        public static void RebindAllSkinnedParts(Transform roleRoot, Transform skeletonBoneRoot)
        {
            if (roleRoot == null || skeletonBoneRoot == null)
            {
                return;
            }

            foreach (SyntySkinnedPartMetadata metadata in roleRoot.GetComponentsInChildren<SyntySkinnedPartMetadata>(true))
            {
                SkinnedMeshRenderer smr = metadata.GetComponent<SkinnedMeshRenderer>();
                if (smr == null)
                {
                    continue;
                }

                if (metadata.transform.parent != null)
                {
                    SyntyHierarchyPaths.EnsureActiveHierarchy(metadata.transform);
                }

                smr.enabled = true;
                smr.updateWhenOffscreen = true;
                Bind(smr, skeletonBoneRoot, metadata);
            }
        }

        /// <summary>
        /// Demo2 用 ModularCharacter 根下路径 Root；Role 预制体为 ReferenceCollector.Root → 内层骨骼 Root。
        /// </summary>
        public static Transform ResolveSkeletonBoneRoot(Transform roleRoot)
        {
            if (roleRoot == null)
            {
                return null;
            }

            Transform demoStyle = SyntyHierarchyPaths.FindByPath(roleRoot, "Root");
            if (demoStyle != null)
            {
                Transform inner = FindDirectChild(demoStyle, "Root");
                if (inner != null)
                {
                    return inner;
                }

                if (FindDirectChild(demoStyle, "Modular_Characters") == null)
                {
                    return demoStyle;
                }
            }

            Transform anchor = null;
            ReferenceCollector collector = roleRoot.GetComponent<ReferenceCollector>();
            if (collector != null)
            {
                GameObject rootGo = collector.Get<GameObject>("Root");
                if (rootGo != null)
                {
                    anchor = rootGo.transform;
                }
            }

            if (anchor == null)
            {
                anchor = FindDirectChild(roleRoot, "Root");
            }

            if (anchor == null)
            {
                return roleRoot;
            }

            Transform innerSkeleton = FindDirectChild(anchor, "Root");
            if (innerSkeleton != null)
            {
                return innerSkeleton;
            }

            foreach (Transform t in anchor.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Hips")
                {
                    return t;
                }
            }

            return anchor;
        }

        public static void Bind(SkinnedMeshRenderer smr, Transform skeletonRoot, SyntySkinnedPartMetadata metadata)
        {
            if (smr == null || skeletonRoot == null || metadata == null || metadata.boneNames == null || metadata.boneNames.Length == 0)
            {
                Debug.LogWarning("[SyntySkinnedBoneBinder] 参数不完整，无法绑定骨骼。");
                return;
            }

            Dictionary<string, Transform> map = BuildBoneMap(skeletonRoot);
            var bones = new Transform[metadata.boneNames.Length];
            for (int i = 0; i < metadata.boneNames.Length; i++)
            {
                string boneName = metadata.boneNames[i];
                if (!map.TryGetValue(boneName, out bones[i]))
                {
                    Debug.LogWarning($"[SyntySkinnedBoneBinder] 骨架上缺少骨骼: {boneName}（部件 {smr.name}）");
                }
            }

            smr.bones = bones;

            if (!string.IsNullOrEmpty(metadata.rootBoneName) && map.TryGetValue(metadata.rootBoneName, out Transform root))
            {
                smr.rootBone = root;
            }

            int missing = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null)
                {
                    missing++;
                }
            }

            if (missing > 0)
            {
                Debug.LogWarning(
                    $"[SyntySkinnedBoneBinder] {smr.name} 有 {missing}/{bones.Length} 根骨骼未绑定，skeletonRoot={skeletonRoot.name}");
            }

            smr.ResetBounds();
        }

        public static void FillMetadataFromRenderer(SkinnedMeshRenderer smr, SyntySkinnedPartMetadata metadata)
        {
            if (smr == null || smr.bones == null || smr.bones.Length == 0 || metadata == null)
            {
                return;
            }

            metadata.boneNames = new string[smr.bones.Length];
            for (int i = 0; i < smr.bones.Length; i++)
            {
                metadata.boneNames[i] = smr.bones[i] != null ? smr.bones[i].name : string.Empty;
            }

            metadata.rootBoneName = smr.rootBone != null ? smr.rootBone.name : "Hips";
        }

        public static Dictionary<string, Transform> BuildBoneMap(Transform skeletonRoot)
        {
            var map = new Dictionary<string, Transform>();
            foreach (Transform t in skeletonRoot.GetComponentsInChildren<Transform>(true))
            {
                if (IsUnderModularCharacters(t))
                {
                    continue;
                }

                if (map.TryGetValue(t.name, out Transform existing))
                {
                    if (PreferBoneTransform(t, existing))
                    {
                        map[t.name] = t;
                    }

                    continue;
                }

                map.Add(t.name, t);
            }

            return map;
        }

        static bool IsUnderModularCharacters(Transform t)
        {
            for (Transform walk = t; walk != null; walk = walk.parent)
            {
                if (walk.name == "Modular_Characters")
                {
                    return true;
                }
            }

            return false;
        }

        static bool PreferBoneTransform(Transform candidate, Transform existing)
        {
            bool candidateMod = IsUnderModularCharacters(candidate);
            bool existingMod = IsUnderModularCharacters(existing);
            if (candidateMod != existingMod)
            {
                return !candidateMod;
            }

            return candidate.GetSiblingIndex() < existing.GetSiblingIndex();
        }

        static Transform FindDirectChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
