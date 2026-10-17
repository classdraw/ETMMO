using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LoginRoleModelPreviewComponent))]
    [FriendOf(typeof(LoginRoleModelPreviewComponent))]
    public static partial class LoginRoleModelPreviewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LoginRoleModelPreviewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoginRoleModelPreviewComponent self)
        {
            foreach (KeyValuePair<string, GameObject> kv in self.PreviewInstances)
            {
                if (kv.Value != null)
                {
                    UnityEngine.Object.Destroy(kv.Value);
                }
            }

            self.PreviewInstances.Clear();
            self.LoadedPrefabPaths.Clear();
            self.LeftActiveKey = null;
            self.RightActiveKey = null;
        }

        public static async ETTask ApplySlotPreview(
            this LoginRoleModelPreviewComponent self,
            Scene root,
            LoginRolePreviewSlot slot,
            Transform anchor,
            int job,
            int gender)
        {
            if (self.IsDisposed || anchor == null)
            {
                return;
            }

            if (!RoleModelPrefabHelper.TryGetPrefabAssetPath(job, gender, out string assetPath))
            {
                Log.Warning($"LoginRoleModelPreview: invalid job/gender job={job} gender={gender}");
                return;
            }

            string instanceKey = BuildInstanceKey(slot, job, gender);
            string previousKey = slot == LoginRolePreviewSlot.Left ? self.LeftActiveKey : self.RightActiveKey;
            if (previousKey == instanceKey)
            {
                return;
            }

            if (!string.IsNullOrEmpty(previousKey) && self.PreviewInstances.TryGetValue(previousKey, out GameObject previousGo) && previousGo != null)
            {
                previousGo.SetActive(false);
            }

            if (!self.PreviewInstances.TryGetValue(instanceKey, out GameObject instance) || instance == null)
            {
                ResourcesLoaderComponent loader = root.GetComponent<ResourcesLoaderComponent>();
                if (loader == null)
                {
                    return;
                }

                GameObject prefab = await loader.LoadAssetAsync<GameObject>(assetPath);
                if (prefab == null)
                {
                    Log.Error($"LoginRoleModelPreview: prefab load failed path={assetPath}");
                    return;
                }

                self.LoadedPrefabPaths.Add(assetPath);
                ClearAnchorPlaceholders(anchor);
                instance = UnityEngine.Object.Instantiate(prefab, anchor);
                instance.transform.SetParent(anchor, false);
                instance.name = $"Preview_{instanceKey}";
                ResetLocalTransform(instance.transform);
                SetLayerRecursively(instance, LayerMask.NameToLayer(RoleModelPrefabHelper.UiObjectLayerName));
                self.PreviewInstances[instanceKey] = instance;
            }
            else if (instance.transform.parent != anchor)
            {
                instance.transform.SetParent(anchor, false);
                ResetLocalTransform(instance.transform);
            }

            instance.SetActive(true);

            if (slot == LoginRolePreviewSlot.Left)
            {
                self.LeftActiveKey = instanceKey;
            }
            else
            {
                self.RightActiveKey = instanceKey;
            }
        }

        private static string BuildInstanceKey(LoginRolePreviewSlot slot, int job, int gender)
        {
            return $"{(int)slot}_{job}_{gender}";
        }

        private static void ResetLocalTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private static void ClearAnchorPlaceholders(Transform anchor)
        {
            for (int i = anchor.childCount - 1; i >= 0; i--)
            {
                Transform child = anchor.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith("Preview_"))
                {
                    continue;
                }

                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null || layer < 0)
            {
                return;
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.layer = layer;
            }
        }
    }
}
