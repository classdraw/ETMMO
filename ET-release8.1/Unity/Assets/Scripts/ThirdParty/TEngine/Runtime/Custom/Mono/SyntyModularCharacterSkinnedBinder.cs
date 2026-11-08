using UnityEngine;

namespace ET
{
    /// <summary>
    /// Role 预制体进场景后，按 Metadata 将各部件 SkinnedMeshRenderer 重新绑定到骨架（与 Demo 换装逻辑一致）。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-200)]
    public class SyntyModularCharacterSkinnedBinder : MonoBehaviour
    {
        [Tooltip("留空则从 ReferenceCollector.Root 解析内层骨架 Root。")]
        [SerializeField]
        Transform skeletonBoneRoot;

        [SerializeField]
        bool rebindOnAwake = true;

        void Awake()
        {
            var animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }

            if (rebindOnAwake)
            {
                RebindAllParts();
            }
        }

        public void RebindAllParts()
        {
            Transform boneRoot = skeletonBoneRoot != null
                ? skeletonBoneRoot
                : SyntySkinnedBoneBinder.ResolveSkeletonBoneRoot(transform);
            if (boneRoot == null)
            {
                Debug.LogWarning($"[SyntySkinnedBinder] 未找到骨架：{name}", this);
                return;
            }

            skeletonBoneRoot = boneRoot;
            SyntySkinnedBoneBinder.RebindAllSkinnedParts(transform, boneRoot);
        }

#if UNITY_EDITOR
        public void SetSkeletonBoneRoot(Transform boneRoot)
        {
            skeletonBoneRoot = boneRoot;
        }
#endif
    }
}
