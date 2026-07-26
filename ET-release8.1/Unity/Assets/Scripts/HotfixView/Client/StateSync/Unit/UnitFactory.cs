using Unity.Mathematics;
using UnityEngine;

namespace ET.Client
{
    public static partial class UnitFactory
    {
        public static float3 GetEffectSpawnPosition(Unit caster, EffectConfig effectConfig)
        {
            float3 spawnPosition = caster.Position;
            if (effectConfig.BindBone <= 0)
            {
                return spawnPosition;
            }

            GameObjectComponent gameObjectComponent = caster.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null || gameObjectComponent.GameObject == null)
            {
                return spawnPosition;
            }

            ReferenceCollector referenceCollector = gameObjectComponent.GameObject.GetComponent<ReferenceCollector>();
            if (referenceCollector == null)
            {
                return spawnPosition;
            }

            string boneKey = ((BindBoneType)effectConfig.BindBone).ToString();
            GameObject boneGo = referenceCollector.Get<GameObject>(boneKey);
            if (boneGo == null)
            {
                return spawnPosition;
            }

            Vector3 worldPosition = boneGo.transform.TransformPoint(ToUnityVector3(effectConfig.Offset));
            return worldPosition;
        }

        private static Vector3 ToUnityVector3(float[] values)
        {
            if (values == null || values.Length != 3)
            {
                return Vector3.zero;
            }

            return new Vector3(values[0], values[1], values[2]);
        }
    }
}
