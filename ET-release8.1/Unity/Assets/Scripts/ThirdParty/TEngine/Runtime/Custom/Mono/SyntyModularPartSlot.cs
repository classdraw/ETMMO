using UnityEngine;

namespace ET
{
    /// <summary>
    /// 与 Demo2 SyntyDynamicPartSlot 相同：实例化 RolePart，挂到槽位并重绑骨骼。
    /// </summary>
    public class SyntyModularPartSlot
    {
        readonly Transform slotParent;
        readonly Transform skeletonBoneRoot;

        GameObject currentInstance;

        public SyntyModularPartSlot(Transform slotParent, Transform skeletonBoneRoot)
        {
            this.slotParent = slotParent;
            this.skeletonBoneRoot = skeletonBoneRoot;
        }

        public GameObject CurrentInstance => currentInstance;

        public void Clear()
        {
            if (currentInstance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(currentInstance);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(currentInstance);
            }

            currentInstance = null;
        }

        public GameObject Equip(GameObject partPrefab)
        {
            Clear();
            if (partPrefab == null || slotParent == null || skeletonBoneRoot == null)
            {
                return null;
            }

            currentInstance = UnityEngine.Object.Instantiate(partPrefab, slotParent, false);
            currentInstance.name = partPrefab.name;
            currentInstance.transform.localPosition = Vector3.zero;
            currentInstance.transform.localRotation = Quaternion.identity;
            currentInstance.transform.localScale = Vector3.one;
            currentInstance.SetActive(true);
            SyntyHierarchyPaths.EnsureActiveHierarchy(slotParent);

            SkinnedMeshRenderer smr = currentInstance.GetComponent<SkinnedMeshRenderer>();
            SyntySkinnedPartMetadata metadata = currentInstance.GetComponent<SyntySkinnedPartMetadata>();
            if (smr != null)
            {
                smr.enabled = true;
                smr.updateWhenOffscreen = true;
            }

            if (smr != null && metadata != null)
            {
                SyntySkinnedBoneBinder.Bind(smr, skeletonBoneRoot, metadata);
            }

            return currentInstance;
        }
    }
}
