using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(MountComponent))]
    [FriendOf(typeof(MountComponent))]
    [FriendOf(typeof(GameObjectComponent))]
    [FriendOf(typeof(PoolComponent))]
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
            if (self.Items.Count <= 0)
            {
                return;
            }

            long now = TimeInfo.Instance.ClientFrameTime();
            for (int i = self.Items.Count - 1; i >= 0; i--)
            {
                MountItem item = self.Items[i];
                if (item.WaitDestroyTime > 0 && now >= item.WaitDestroyTime)
                {
                    self.RemoveEffectAt(i);
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

            GameObject effectGo = await unit.Scene().GetComponent<PoolComponent>().GetEffect(assetPath);
            if (effectGo == null)
            {
                Log.Error($"MountEffect failed, GetEffect returned null, assetPath={assetPath}");
                return null;
            }

            effectGo.transform.SetParent(boneGo.transform, false);
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
            self.Items.Add(new MountItem
            {
                Object = effectGo,
                PoolKey = assetPath,
                WaitDestroyTime = waitDestroyTime,
            });

            return effectGo;
        }

        private static void ClearAllEffects(this MountComponent self)
        {
            for (int i = self.Items.Count - 1; i >= 0; i--)
            {
                self.RemoveEffectAt(i);
            }
        }

        private static void RemoveEffectAt(this MountComponent self, int index)
        {
            if (index < 0 || index >= self.Items.Count)
            {
                return;
            }

            MountItem item = self.Items[index];
            self.Items.RemoveAt(index);

            if (item.Object == null)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            PoolComponent poolComponent = unit.Scene().GetComponent<PoolComponent>();
            if (poolComponent == null || string.IsNullOrEmpty(item.PoolKey))
            {
                UnityEngine.Object.Destroy(item.Object);
                return;
            }

            poolComponent.ReturnEffect(item.PoolKey, item.Object);
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
