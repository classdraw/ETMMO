using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(MountComponent))]
    [FriendOf(typeof(MountComponent))]
    [FriendOf(typeof(GameObjectComponent))]
    public static partial class MountComponentSystem
    {
        private const string EffectBundlePathPrefix = "Assets/Bundles/Effect/";

        [EntitySystem]
        private static void Destroy(this MountComponent self)
        {
            self.ClearAllEffects();
        }

        [EntitySystem]
        private static void Awake(this MountComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this MountComponent self)
        {
            if (self.Objects.Count <= 0)
            {
                return;
            }

            long now = TimeInfo.Instance.ClientFrameTime();
            for (int i = self.Objects.Count - 1; i >= 0; i--)
            {
                long destroyTime = self.WaitDestroyTimes[i];
                if (destroyTime > 0 && now >= destroyTime)
                {
                    GameObject effectGo = self.Objects[i];
                    //Log.Info($"MountEffect timed destroy, unitId={self.GetParent<Unit>().Id}, effectGo={effectGo?.name}, now={now}, destroyTime={destroyTime}, delta={now - destroyTime}ms");
                    self.RemoveEffectAt(i, "Update timed");
                }
            }
        }

        public static async ETTask<GameObject> MountEffect(this MountComponent self, int effectConfigId)
        {
            if (!EffectConfigCategory.Instance.Contain(effectConfigId))
            {
                Log.Error($"EffectConfig not found: {effectConfigId}");
                return null;
            }

            Unit unit = self.GetParent<Unit>();
            GameObjectComponent gameObjectComponent = unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null || gameObjectComponent.GameObject == null)
            {
                Log.Error($"MountEffect failed, GameObjectComponent is null, unitId={unit.Id}");
                return null;
            }

            EffectConfig effectConfig = EffectConfigCategory.Instance.Get(effectConfigId);
            GameObject unitGo = gameObjectComponent.GameObject;
            ReferenceCollector referenceCollector = unitGo.GetComponent<ReferenceCollector>();
            if (referenceCollector == null)
            {
                Log.Error($"MountEffect failed, ReferenceCollector is null, unitId={unit.Id}");
                return null;
            }

            string boneKey = ((BindBoneType)effectConfig.BindBone).ToString();
            GameObject boneGo = referenceCollector.Get<GameObject>(boneKey);
            if (boneGo == null)
            {
                Log.Error($"MountEffect failed, bone not found, unitId={unit.Id}, boneKey={boneKey}");
                return null;
            }

            if (string.IsNullOrEmpty(effectConfig.Model))
            {
                Log.Error($"MountEffect failed, effect model is empty, effectConfigId={effectConfigId}");
                return null;
            }

            string assetPath = $"{EffectBundlePathPrefix}{effectConfig.Model}.prefab";
            Scene scene = unit.Scene();
            ResourcesLoaderComponent resourcesLoader = scene.GetComponent<ResourcesLoaderComponent>();
            if (resourcesLoader == null)
            {
                Log.Error($"MountEffect failed, ResourcesLoaderComponent is null, unitId={unit.Id}");
                return null;
            }

            GameObject prefab = await resourcesLoader.LoadAssetAsync<GameObject>(assetPath);
            if (prefab == null)
            {
                Log.Error($"MountEffect failed, prefab not found: {assetPath}");
                return null;
            }

            GameObject effectGo = UnityEngine.Object.Instantiate(prefab, boneGo.transform, false);
            effectGo.name = $"{effectConfig.Model}_{effectConfigId}";
            effectGo.transform.localPosition = ToUnityVector3(effectConfig.Offset);
            effectGo.transform.localScale = ToUnityScaleVector3(effectConfig.Scale);

            ReferenceParticleCollector particleCollector = effectGo.GetComponent<ReferenceParticleCollector>();
            if (particleCollector != null)
            {
                particleCollector.PlayAll();
            }

            long now = TimeInfo.Instance.ClientFrameTime();
            long waitDestroyTime = effectConfig.DestroyTime <= 0 ? 0 : now + effectConfig.DestroyTime;
            self.Objects.Add(effectGo);
            self.WaitDestroyTimes.Add(waitDestroyTime);

            //Log.Info($"MountEffect created, unitId={unit.Id}, effectConfigId={effectConfigId}, effectGo={effectGo.name}, now={now}, destroyTime={waitDestroyTime}, configDestroyTime={effectConfig.DestroyTime}ms, count={self.Objects.Count}");

            return effectGo;
        }

        private static void ClearAllEffects(this MountComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            //Log.Info($"MountEffect clear all, unitId={unit?.Id}, count={self.Objects.Count}");
            for (int i = self.Objects.Count - 1; i >= 0; i--)
            {
                self.RemoveEffectAt(i, "ClearAll");
            }
        }

        private static void RemoveEffectAt(this MountComponent self, int index, string reason)
        {
            if (index < 0 || index >= self.Objects.Count)
            {
                return;
            }

            GameObject effectGo = self.Objects[index];
            long waitDestroyTime = index < self.WaitDestroyTimes.Count ? self.WaitDestroyTimes[index] : -1;
            //Log.Info($"MountEffect remove, unitId={self.GetParent<Unit>().Id}, reason={reason}, index={index}, effectGo={effectGo?.name}, waitDestroyTime={waitDestroyTime}, countBefore={self.Objects.Count}");

            if (effectGo != null)
            {
                UnityEngine.Object.Destroy(effectGo);
            }

            self.Objects.RemoveAt(index);
            if (index < self.WaitDestroyTimes.Count)
            {
                self.WaitDestroyTimes.RemoveAt(index);
            }
        }

        private static Vector3 ToUnityVector3(float[] values)
        {
            if (values == null || values.Length != 3)
            {
                return Vector3.zero;
            }

            return new Vector3(values[0], values[1], values[2]);
        }

        private static Vector3 ToUnityScaleVector3(float[] values)
        {
            if (values == null || values.Length != 3)
            {
                return Vector3.one;
            }
            
            return new Vector3(values[0], values[1], values[2]);
        }
    }
}
